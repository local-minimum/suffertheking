﻿using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Boardgame.Data
{
    public delegate void UndoOrder(int cost);

    public abstract class Order : MonoBehaviour
    {

        public static event UndoOrder OnUndoOrder;

        public enum OrderType { None, Taxation, Deployment };
        public OrderType orderType = OrderType.None;

        public enum ExcecutionSteps {Created, Ordered, Executing, Executed};

        ExcecutionSteps exectued = ExcecutionSteps.Created;

        public ExcecutionSteps ExectuionStep
        {
            get
            {
                return exectued;
            }
        }

        protected void SetOrdered()
        {
            exectued = ExcecutionSteps.Ordered;

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

        public void completeExecution()
        {
            exectued = ExcecutionSteps.Executed;
        }

        abstract protected IEnumerator<WaitForSeconds> _execute();

        public bool undo()
        {
            var allowed = _undo(exectued);
            if (allowed && OnUndoOrder != null)
                OnUndoOrder(cost);
            return allowed;
        }

        abstract protected bool _undo(ExcecutionSteps executed);

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
            yield return new WaitForSeconds(.5f);
        }

        override protected bool _undo(ExcecutionSteps executed)
        {
            if (executed == ExcecutionSteps.Executed)
                region.demographics.taxation -= taxChange;
            taxOrders.Remove(region);
            return true;
        }

        public static void Create(Tile region, int taxChange)
        {
            var order = OrderLog.Create<TaxOrder>();
            order.region = region;
            order.taxChange = taxChange;
            order.SetOrdered();
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

    [Serializable]
    public class DeploymentOrderUnitDetails
    {

        public MilitaryUnitType type;
        public int count;
        public string location;
    }

    [Serializable]
    public class DeploymentOrderDetails
    {

        public int orderNumber;
        public string[] pathByRegionNames;
        public DeploymentOrderUnitDetails[] units = new DeploymentOrderUnitDetails[0];
        public bool moveSynchronized;
        public bool signed;
    }

    public class DeploymentOrder : Order
    {
        DeploymentOrderDetails data = new DeploymentOrderDetails();

        static List<DeploymentOrder> deploymentOrders = new List<DeploymentOrder>();

        int previousCost = -1;

        public bool synchronizedMovement
        {
            get
            {
                return data.moveSynchronized;
            }

            set
            {
                data.moveSynchronized = value;
                CheckActionPointsConsequences();
            }
        }

        public bool ChangeTroopAllocation(MilitaryUnitType type, int countChange)
        {
            return false;
        }

        public int GetTroopAllocation(MilitaryUnitType type)
        {
            return 0;
        }

        protected override int cost
        {
            get
            {
                return data.units.Length > 1 ? 2 : 1;
            }
        }

        void CheckActionPointsConsequences()
        {
            var currentCost = cost;
            if (previousCost >= 0 && previousCost != currentCost)
                Game.ActionPoints.ConsumePoints(currentCost - previousCost);
            previousCost = currentCost;
        }

        void Awake ()
        {
            orderType = OrderType.Deployment;
        }

        new public void SetOrdered()
        {
            base.SetOrdered();
        }

        protected override IEnumerator<WaitForSeconds> _execute()
        {
            throw new NotImplementedException();
        }

        protected override bool _undo(ExcecutionSteps executed)
        {
            var allowed = executed == ExcecutionSteps.Created || executed == ExcecutionSteps.Executed || Game.ActionPoints.ConsumePoints(1);
            
            if (allowed)
                deploymentOrders.Remove(this);
            return allowed;
        }

        static public DeploymentOrder Create(Tile[] path, bool moveSynchronized)
        {
            var pathByRegionNames = GetPathAsRegionNames(path);
            var order = OrderLog.Create<DeploymentOrder>();
            order.data.moveSynchronized = moveSynchronized;
            order.data.pathByRegionNames = pathByRegionNames;
            Game.ActionPoints.ConsumePoints(order.cost);
            deploymentOrders.Add(order);
            return order;
        }

        static string[] GetPathAsRegionNames(Tile[] path)
        {
            var pathByRegionNames = new string[path.Length];
            for (int i = 0; i < path.Length; i++)
                pathByRegionNames[i] = path[i].name;
            return pathByRegionNames;
        }

    }
}