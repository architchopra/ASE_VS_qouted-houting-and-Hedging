using System;
using System.Collections.Generic;
using tt_net_sdk;
/*using Serilog;*/
using System.Threading;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Linq;
using System.Windows.Forms;

namespace OrderBook_Management
{
    //public delegate void ReloadFillEventHandler(string status);
    public delegate void FillSubWorkingEventHandler(bool status,string key,int er);
    public class Fill_Subscription
    {
        private Dispatcher m_disp_FS;
        private TTAPI m_apiInstance = null;
        private bool m_FS_isDisposed = false;

        private FillsSubscription m_fs = null;
        public event FillSubWorkingEventHandler FillSubWorkingEventHandler_1;
        private TradeSubscription ts;
        private string key;
        private Queue<DateTime> Orders_filled = new Queue<DateTime>();
        private readonly CancellationToken cancellationToken;
        int time_ms = 200000;
        public int desired_number =2;
        int er;

        public Fill_Subscription() { }
        public Fill_Subscription(Dispatcher disp, TTAPI m_api,string key,TradeSubscription ts, CancellationToken cancellationToken,int er)
        {
            this.m_disp_FS = disp;
            this.m_apiInstance = m_api;
            this.key = key;
            this.ts = ts;
            this.cancellationToken = cancellationToken;
            this.er = er;
        }
        public void Start()
        {
            Console.WriteLine("Starting Fill Subscription");
            m_apiInstance.StartFillFeed();
            //NEWTHREAD
            m_fs = new FillsSubscription(m_disp_FS);
            m_fs.FillAdded += m_fs_FillAdded;
            m_fs.FillListEnd += m_fs_FillListEnd;
            m_fs.FillListStart += m_fs_FillListStart;
            m_fs.Start();
            Console.WriteLine("Fill Subscription Started");
        }
        void m_fs_FillListStart(object sender, FillListEventArgs e)
        {
            Console.WriteLine("Begin adding fills from {0}", e.FeedConnectionKey.ToString());
        }
        void m_fs_FillListEnd(object sender, FillListEventArgs e)
        {
            Console.WriteLine("Finished adding fills from {0}", e.FeedConnectionKey.ToString());
            
        }
        void m_fs_FillAdded(object sender, FillAddedEventArgs e)
        {
            //this.AseFillReceivedEvent(e.Fill);
            try
            {
                Console.WriteLine("in fill_sub");
                cancellationToken.ThrowIfCancellationRequested();
                if (e.Fill.Instrument.Product.Type == ProductType.Synthetic & e.Fill.InstrumentKey.MarketId == MarketId.ASE)
                {
                    Console.WriteLine("ASE Fill: {0}, {1}, {2}, {3}", e.Fill.InstrumentKey, e.Fill.BuySell, e.Fill.Quantity, e.Fill.MatchPrice);
                    if (ts.Orders.ContainsKey(key))
                    {
                        if (key == e.Fill.SiteOrderKey & e.Fill.Quantity == ts.Orders[key].DisclosedQuantity /*& e.Fill.MatchPrice== ts.Orders[key].LimitPrice*/)
                        {

                            Orders_filled.Enqueue(DateTime.Now);
                            Console.WriteLine("Contains Key,{0},{1}", Orders_filled.Count > 0, (DateTime.Now - Orders_filled.Peek()).TotalMilliseconds);
                            while (Orders_filled.Count > 0 & (DateTime.Now - Orders_filled.Peek()).TotalMilliseconds > time_ms)
                            {
                                Orders_filled.Dequeue();
                            }
                            if (Orders_filled.Count >= desired_number)
                            {

                                this.FillSubWorkingEventHandler_1(true, key, er);
                            }
                            Console.WriteLine(Orders_filled.Count);

                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Task was canceled.");
                Dispose_FS();
                
                    // Unattached callbacks and dispose of all subscriptions
                    
                }
        }

        public void Dispose_FS()
        {

            lock (m_disp_FS)
            {
                if (m_fs != null)
                {
                    m_fs.FillAdded -= m_fs_FillAdded;

                    m_fs.FillListEnd -= m_fs_FillListEnd;
                    m_fs.FillListStart -= m_fs_FillListStart;

                    m_fs.Dispose();
                    m_fs = null;
                }
                Console.WriteLine("hi,{0}",Thread.CurrentThread.ManagedThreadId);
               /* try
                {
                    Thread.CurrentThread.Abort();
                }
                catch (ThreadAbortException ex)
                {
                    Console.WriteLine($"ThreadAbortException caught: {ex.Message}");
                    // Perform any necessary cleanup here
                }*/
            }
               
            
        }

        // Just For Checking Dictionary Values: ----- Remove Later
       
    }

   
}
