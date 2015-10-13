using UnityEngine;
using System.Collections.Generic;
using System;

namespace Boardgame.Data
{
    public delegate void UndoOrder(int cost);

    public abstract class Order : MonoBehaviour
    {

        public static event UndoOrder OnUndoOrder;

        public enum OrderType { None, Taxation };

        protected OrderType orderType = OrderType.None;

        static List<Order> orders = new List<Order>();

        public enum ExcecutionSteps { Ordered, Executing, Executed};

        ExcecutionSteps exectued = ExcecutionSteps.Ordered;

        virtual public void execute()
        {
            if (exectued != ExcecutionSteps.Ordered)
            {
                Debug.LogWarning("Attempting to execute order twice");
                return;
            }

            exectued = ExcecutionSteps.Executing;
            StartCoroutine(_execute());
        }

        protected void completeExecution()
        {
            exectued = ExcecutionSteps.Executed;
            orders.Remove(this);
        }

        abstract protected IEnumerator<WaitForSeconds> _execute();

        public void undo()
        {
            _undo();
            if (OnUndoOrder != null)
                OnUndoOrder(cost);
            orders.Remove(this);
        }

        abstract protected void _undo();

        abstract protected int cost
        {
            get;
        }

        protected Order()
        {
            orders.Add(this);
            Debug.Log(this);
        }

        public static void ClearAllOrders()
        {
            while (orders.Count > 0)
                orders[0].undo();
        }

        public static Order NextOrder
        {
            get
            {
                if (orders.Count > 0)
                    return orders[0];
                return null;
            }
        }

        public static IEnumerator<WaitForEndOfFrame> ExecuteOrders()
        {
            var order = NextOrder;
            while (order != null)
            {
                if (order.exectued == ExcecutionSteps.Executed)
                {
                    order = NextOrder;
                } else if (order.exectued == ExcecutionSteps.Ordered)
                    order.execute();

                yield return new WaitForEndOfFrame();
            }
            ClearAllOrders();
            Game.Step();
        }

    }


    public class TaxOrder : Order
    {
        int taxChange = 0;
        Tile region;

        static Dictionary<Tile, TaxOrder> taxOrders = new Dictionary<Tile, TaxOrder>();
        static int _cost = 1;

        override protected int cost
        {
            get
            {
                return _cost;
            }
        }

        public static int Cost(Tile region)
        {
            if (HasTaxChangeOrder(region))
                return 0;
            return _cost;
        }

        protected override IEnumerator<WaitForSeconds> _execute()
        {
            region.demographics.taxation += taxChange;
            taxOrders.Remove(this.region);
            completeExecution();
            yield return new WaitForSeconds(1f);
        }

        override protected void _undo()
        {
            region.demographics.taxation -= taxChange;
            taxOrders.Remove(region);
        }

        public TaxOrder(Tile region, int taxChange)
        {
            orderType = OrderType.Taxation;
            this.region = region;
            this.taxChange = taxChange;
            Game.ActionPoints.ConsumePoints(cost);
            taxOrders.Add(region, this);
        }

        public static void Tax(Tile region, int taxChange)
        {
            if (!HasTaxChangeOrder(region))
                new TaxOrder(region, taxChange);
            else
            {

                taxOrders[region].taxChange += taxChange;
                if (taxOrders[region].taxChange == 0)
                    taxOrders[region].undo();
            }

        }

        public static int TaxChangeOrdered(Tile region)
        {
            if (HasTaxChangeOrder(region))
                return taxOrders[region].taxChange;
            return 0;
        }

        public static void RemoveTaxOrder(Tile region)
        {
            if (HasTaxChangeOrder(region))
                taxOrders[region].undo();
        }

        public static bool HasTaxChangeOrder(Tile region)
        {
            return taxOrders.ContainsKey(region);
        }
    }
}