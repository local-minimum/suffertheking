using UnityEngine;
using System.Collections.Generic;
using Boardgame.Data;

namespace Boardgame
{
    public class OrderLog : MonoBehaviour
    {

        static OrderLog _instance;

        static Queue<Order> orders = new Queue<Order>();

        static OrderLog instance
        {
            get
            {
                if (_instance == null)
                {
                    var GO = new GameObject();
                    GO.AddComponent<OrderLog>();
                    GO.name = "OrderLog";
                }
                return _instance;
            }
        }

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        public static T Create<T>() where T: Order
        {
            var order = instance.gameObject.AddComponent<T>();
            orders.Enqueue(order);
            return order;
        }

        public static void ClearAllOrders()
        {
            while (orders.Count > 0)
            {
                var order = orders.Dequeue();
                order.undo();
                Destroy(order);
            }
        }


        static Order NextOrder
        {
            get
            {
                if (orders.Count > 0)
                    return orders.Dequeue();
                return null;
            }
        }

        public static IEnumerator<WaitForEndOfFrame> ExecuteOrders()
        {
            var order = NextOrder;
            IEnumerator<Coroutine> iter = null;
            while (order != null)
            {

                if (order.ExectuionStep == Order.ExcecutionSteps.Executed) // || safety < 0)
                {
                    if (order != null)
                        Destroy(order);
                    order = NextOrder;
                }
                else if (order.ExectuionStep == Order.ExcecutionSteps.Ordered)
                    iter = order.execute();

                if (iter != null)
                    iter.MoveNext();
                yield return new WaitForEndOfFrame();

            }
            ClearAllOrders();
            Game.Step();
        }
    }

}