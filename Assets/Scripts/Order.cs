using UnityEngine;
using System.Collections.Generic;

public delegate void UndoOrder(int cost);

public abstract class Order
{

    public static event UndoOrder OnUndoOrder;

    public enum OrderType {None, Taxation};

    protected OrderType orderType = OrderType.None;

    static List<Order> orders = new List<Order>();

    protected bool exectued = false;

    virtual public void execute()
    {
        exectued = true;
        orders.Remove(this);
    }

    virtual public void undo()
    {
        if (OnUndoOrder != null)
            OnUndoOrder(cost);
        orders.Remove(this);
    }
    abstract protected int cost
    {
        get;
    }

    protected Order()
    {
        orders.Add(this);
        Debug.Log(this);
    }

    public static void Clear()
    {
        while (orders.Count > 0)
            orders[0].undo();
    }

    public static List<Order> GetOrdersByType(OrderType type)
    {
        var ordersByType = new List<Order>();

        for (int i=0, l=orders.Count; i< l;i++)
        {
            if (orders[i].orderType == type)
                ordersByType.Add(orders[i]);
        }
        return ordersByType;
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

    override public void execute()
    {
        if (!exectued)
            region.population.taxation += taxChange;
        taxOrders.Remove(this.region);
        base.execute();
    }

    override public void undo()
    {
        if (exectued)
            region.population.taxation -= taxChange;
        taxOrders.Remove(region);
        base.undo();
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