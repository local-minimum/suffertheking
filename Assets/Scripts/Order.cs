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
        public OrderType orderType = OrderType.None;

        public enum ExcecutionSteps { Ordered, Executing, Executed};

        ExcecutionSteps exectued = ExcecutionSteps.Ordered;

        public ExcecutionSteps ExectuionStep
        {
            get
            {
                return exectued;
            }
        }

        public IEnumerator<Coroutine> execute()
        {
            if (exectued != ExcecutionSteps.Ordered)
            {
                Debug.LogWarning("Attempting to execute order twice");
                yield return null;
            }

            exectued = ExcecutionSteps.Executing;
            yield return StartCoroutine(_execute());
        }

        protected void completeExecution()
        {
            exectued = ExcecutionSteps.Executed;
        }

        abstract protected IEnumerator<WaitForSeconds> _execute();

        public void undo()
        {
            _undo(exectued != ExcecutionSteps.Ordered);

            if (OnUndoOrder != null)
                OnUndoOrder(cost);
        }

        abstract protected void _undo(bool executed);

        abstract protected int cost
        {
            get;
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
            Debug.Log(string.Format("Made tax change {0} to {1} resulting in {2} tax",
                taxChange, region.name, region.demographics.taxation));
            taxOrders.Remove(this.region);
            completeExecution();
            yield return new WaitForSeconds(1f);
        }

        override protected void _undo(bool executed)
        {
            if (executed)
                region.demographics.taxation -= taxChange;
            taxOrders.Remove(region);
        }

        public static void Create(Tile region, int taxChange)
        {
            var order = OrderLog.Create<TaxOrder>();
            order.region = region;
            order.taxChange = taxChange;
            Game.ActionPoints.ConsumePoints(order.cost);
            taxOrders.Add(region, order);    
        }

        public static void Tax(Tile region, int taxChange)
        {
            if (!HasTaxChangeOrder(region))
                TaxOrder.Create(region, taxChange);
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

        void Awake()
        {
            orderType = OrderType.Taxation;
        }
    }
}