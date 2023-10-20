using OrderBook_Management;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tt_net_sdk;

namespace OrderBook_Management
{
    
    public class Reload_Mangement
    {
        private tt_net_sdk.Instrument instr = null;
        private object m_Hold = new object();
        private Order_Action_Functions Order_Action_Functions = null;
        public OrderProfile Send_Order(Instrument instrument, BuySell buysell, Price price, Quantity quantity, OrderType orderType, tt_net_sdk.TimeInForce TIF,
            IReadOnlyCollection<Account> m_account, int acc_idx, string text)
        {
            OrderProfile op = new OrderProfile(instrument)
            {
                BuySell = buysell,
                OrderType = orderType,
                TimeInForce = TIF,
                Account = m_account.ElementAt(acc_idx),
                LimitPrice = price,
                OrderQuantity = quantity,
                TextA = text
            };


            return op;
        }
        public String Change_Order_Reload(string order_key, string disclosed, TradeSubscription trade_subscription, IReadOnlyCollection<Account> m_account, int acc_idx)
        {
            DateTime dt = DateTime.Now;
            if (trade_subscription.Orders.ContainsKey(order_key))
            {
                OrderProfile op = trade_subscription.Orders[order_key].GetOrderProfile();
                Console.WriteLine("Hurray {0}", op.DisclosedQuantity);
                if ((op.DisclosedQuantity != Quantity.Empty) || op.DisclosedQuantity != Quantity.Empty)
                {
                    Console.WriteLine(Convert.ToInt32(disclosed));
                    int r = Convert.ToInt32(disclosed) - Convert.ToInt32(op.DisclosedQuantity);
                    Console.WriteLine(r);
                    Quantity tq = trade_subscription.Orders[order_key].OrderQuantity - trade_subscription.Orders[order_key].FillQuantity;
                    instr = op.Instrument;
                    OrderProfile op1 = Send_Order(instr, op.BuySell, op.LimitPrice, tq - op.DisclosedQuantity, op.OrderType, op.TimeInForce, m_account, acc_idx, op.TextA);
                    op1.DisclosedQuantity = r + op.DisclosedQuantity;
                    Console.WriteLine("Hurray");
                    if (!trade_subscription.SendOrder(op1))
                    {
                        Console.WriteLine("Send new  order failed.");
                        return "failed";
                    }
                    else
                    {
                        Console.WriteLine("Send new  order succeeded.");
                        op.OrderQuantity = op.DisclosedQuantity + trade_subscription.Orders[order_key].FillQuantity;
                        op.Action = OrderAction.Change;
                        if (!trade_subscription.SendOrder(op))
                        {
                            Console.WriteLine("Previous order deletion failed.");
                        }
                        else
                        {
                            Console.WriteLine("Previous order deleted,{0}", DateTime.Now - dt);
                        }
                        return op1.SiteOrderKey;
                    }
                }
                else
                {
                    Console.WriteLine("Nan case");
                    return "failed";
                }
            }
            return "failed";
        }

        public String Change_Order_Reload(string order_key, int er, TradeSubscription trade_subscription, IReadOnlyCollection<Account> m_account, int acc_idx)
        {
            string new_key = "failed"; ;
            DateTime dt = DateTime.Now;
            if (trade_subscription.Orders.ContainsKey(order_key))
            {
                OrderProfile op = trade_subscription.Orders[order_key].GetOrderProfile();
                Console.WriteLine("Hurray {0}", op.DisclosedQuantity);
                if ((op.DisclosedQuantity != Quantity.Empty) || op.DisclosedQuantity != Quantity.Empty)
                {
                    Quantity tq = trade_subscription.Orders[order_key].OrderQuantity - trade_subscription.Orders[order_key].FillQuantity;

                    instr = op.Instrument;
                    OrderProfile op1 = Send_Order(instr, op.BuySell, op.LimitPrice, tq - op.DisclosedQuantity, op.OrderType, op.TimeInForce, m_account, acc_idx, op.TextA);
                    op1.DisclosedQuantity = er + op.DisclosedQuantity;
                    op1.TextA = Convert.ToString(op.DisclosedQuantity);
                    Console.WriteLine("Hurray");
                    if (!trade_subscription.SendOrder(op1))
                    {
                        Console.WriteLine("Send new  order failed.");
                        new_key = "failed";
                    }
                    else
                    {
                        DateTime dt_2 = DateTime.Now;

                        Console.WriteLine("Send new  order succeeded.");

                        op.OrderQuantity = op.DisclosedQuantity + trade_subscription.Orders[order_key].FillQuantity;
                        op.Action = OrderAction.Change;

                        if (!trade_subscription.SendOrder(op))
                        {
                            Console.WriteLine("Previous order deletion failed.");
                        }
                        else
                        {
                            Console.WriteLine("Previous order deleted,{0},{1}", DateTime.Now - dt, DateTime.Now - dt_2);
                        }
                        new_key = op1.SiteOrderKey;
                    }
                }
                else
                {
                    Console.WriteLine("Nan case");
                    new_key = "failed";
                }
            }
            return new_key;
        }
        public void initialize_order(string order_key, TradeSubscription trade_subscription, IReadOnlyCollection<Account> m_account, int acc_idx)
        {
            DateTime dt = DateTime.Now;

            if (trade_subscription.Orders.ContainsKey(order_key))
            {
                OrderProfile op = trade_subscription.Orders[order_key].GetOrderProfile();

                if ((op.DisclosedQuantity != Quantity.Empty) || op.DisclosedQuantity != Quantity.Empty)
                {
                    Quantity tq = trade_subscription.Orders[order_key].OrderQuantity - trade_subscription.Orders[order_key].FillQuantity - op.DisclosedQuantity;

                    instr = op.Instrument;
                    OrderProfile op1 = Send_Order(instr, op.BuySell, op.LimitPrice, tq, op.OrderType, op.TimeInForce, m_account, acc_idx, op.TextA);
                    op1.DisclosedQuantity = Quantity.FromString(op.Instrument, op.TextA);

                    op.OrderQuantity = trade_subscription.Orders[order_key].FillQuantity + op.DisclosedQuantity;
                    op.Action = OrderAction.Change;
                    Console.WriteLine("Hurray");
                    if (!trade_subscription.SendOrder(op))
                    {
                        Console.WriteLine("Send old order update failed");
                    }
                    else
                    {
                        DateTime dt_2 = DateTime.Now;

                        Console.WriteLine("Send old order update succeeded .");
                        if (!trade_subscription.SendOrder(op1))
                        {
                            Console.WriteLine("send new order failed.");
                        }
                        else
                        {
                            Console.WriteLine("send new order succeeded,{0},{1}", DateTime.Now - dt, DateTime.Now - dt_2);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Nan case");
                }
            }
        }
    }
}
