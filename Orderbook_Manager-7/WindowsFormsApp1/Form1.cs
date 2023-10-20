using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using tt_net_sdk;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static tt.messaging.ttus.Common.Types;
using System.Collections;
using OrderBook_Management;
using System.Threading;
using tt.messaging.ttus;
using System.Reflection;

namespace OrderBook_Management
{
    public partial class Form1 : Form
    {
        tt_net_sdk.ServiceEnvironment environment;

        public Form1(tt_net_sdk.ServiceEnvironment environment)
        {
            InitializeComponent();
            this.environment = environment;
        }
        private TradeSubscription m_instrumentTradeSubscription = null;
        private IReadOnlyCollection<tt_net_sdk.Account> m_accounts = null;
        private ConcurrentDictionary<String, Thread> threads = new ConcurrentDictionary<String,Thread>();
        private bool m_isOrderBookDownloaded = false;
        private bool m_isOrdersSynced = false;
        private object m_Lock = new object();
        private Order_Action_Functions Order_Action_Functions = null;
        public string selected_ase="";
        public int desired_number =2;

        private object m_Hold = new object();
        private object m_dis = new object();
        private object m_Resume = new object();
        private object m_delete = new object();
        private object m_OrderAdded = new object();
        private object m_OrderUpdated = new object();
        private object m_OrderDeleted = new object();
        private object m_OrderFilled = new object();
        private object m_Orders_Storing = new object();
        private object m_Orders_Routing = new object();
        private object m_Spike_Action = new object();
       /* BindingList<MyCustomType> bindingList = new BindingList<MyCustomType>();*/
       public Fill_Subscription m_Fs = null;
        private TTAPI m_api = null;
        private InstrumentLookup m_instrLookupRequest = null;
        private PriceSubscription m_priceSubsciption = null;
        private tt_net_sdk.WorkerDispatcher m_disp = null;
        private tt_net_sdk.Dispatcher m_disp_1 = null;
        private readonly string account_name = "JRathore-SIM"; // Enter your Account In
        private readonly int account_idx = 0;
        TradeSubscriptionTTAccountFilter tsiAF;

        public ConcurrentDictionary<string, Order> or_orders = new ConcurrentDictionary<string, Order>();
       public ConcurrentDictionary<string, Order> qs_orders = new ConcurrentDictionary<string, Order>();
       public ConcurrentDictionary<string, Order> ase_or_orders = new ConcurrentDictionary<string, Order>();
       public ConcurrentDictionary<string, Order> ase_qs_orders = new ConcurrentDictionary<string, Order>();

        public ConcurrentDictionary<string, Order> or_orders_copy = null; // newConcurrentDictionary<string, Order>();
        public ConcurrentDictionary<string, Order> qs_orders_copy = null; // newConcurrentDictionary<string, Order>();
        public ConcurrentDictionary<string, Order> ase_or_orders_copy = null; // newConcurrentDictionary<string, Order>();
        public ConcurrentDictionary<string, Order> ase_qs_orders_copy = null; // newConcurrentDictionary<string, Order>();        

        public ConcurrentDictionary<string, Order> sr1_zq_spread = new ConcurrentDictionary<string, Order>(); // new Dictionary<string, Order>();        
        public ConcurrentDictionary<string, Order> sr3_zq_spread = new ConcurrentDictionary<string, Order>(); // new Dictionary<string, Order>();        
        public ConcurrentDictionary<string, Order> sr3_zq_naked = new ConcurrentDictionary<string, Order>();  // new Dictionary<string, Order>();        
        public ConcurrentDictionary<string, Order> sr3_sr1_naked = new ConcurrentDictionary<string, Order>(); // new Dictionary<string, Order>();        
        public ConcurrentDictionary<string, Order> sr1_zq_naked = new ConcurrentDictionary<string, Order>(); // new Dictionary<string, Order>();        
        public ConcurrentDictionary<string, Order> spread_ase = new ConcurrentDictionary<string, Order>(); // new Dictionary<string, Order>();        
        public ConcurrentDictionary<string, Order> meeting_ase = new ConcurrentDictionary<string, Order>(); // new Dictionary<string, Order>();        
        public ConcurrentDictionary<string, Order> fly_ase = new ConcurrentDictionary<string, Order>(); // new Dictionary<string, Order>();


        public ConcurrentDictionary<string, string> ase_fill = new ConcurrentDictionary<string, string>(); // new Dictionary<string, string>();        
        public ConcurrentDictionary<string, DateTime> ase_fill_executed = new ConcurrentDictionary<string, DateTime>(); // new Dictionary<string, string>();        
       
        // Instrument Information 
        private string m_market = "";
        private string m_product = "";
        private string m_prodType = "Synthetic";
        private string m_alias = "";
        private string pause_time = null;
        private string delete_time = null;
        private string delete_time_1 = null;
        private string resume_time = null;
        private string order_add_time = null;
        private DateTime pause_date = new DateTime();
        private DateTime delete_date = new DateTime();
        private DateTime delete_date_1 = new DateTime();
        private DateTime resume_date = new DateTime();
        private DateTime order_add_date = new DateTime();
        private DateTime order_added_from_file_time = new DateTime();
      
        private InstrumentLookup m_instrLookupRequest_1 = null;
        public Instrument inst = null;

        private bool pause_play = false;
        private bool delete_add= false;
        private bool delete_only= false;
        private bool file_add= false;

        private bool or_pause = false;
        private bool qs_pause = false;
        private bool ase_or_pause = false;
        private bool ase_qs_pause = false;

        private bool or_delete = false;
        private bool qs_delete = false;
        private bool ase_or_delete = false; 
        private bool ase_qs_delete = false;

        private bool or_delete_1 = false;
        private bool qs_delete_1 = false;
        private bool ase_or_delete_1 = false;
        private bool ase_qs_delete_1 = false;
        private bool or_add = false;
        private bool qs_add = false;
        private bool ase_or_add = false;
        private bool ase_qs_add = false;

        private bool or_orders_on_hold = false;
        private bool qs_orders_on_hold = false;
        private bool ase_or_orders_on_hold = false;
        private bool ase_qs_orders_on_hold = false;

        private bool or_orders_stored = false;
        private bool qs_orders_stored = false;
        private bool ase_or_orders_stored = false;
        private bool ase_qs_orders_stored = false;

        private string OR_Orders_File_Path = @"C:\tt\order_details\or_orders.csv";
        private string QS_Orders_File_Path = @"C:\tt\order_details\qs_orders.csv";
        private string ASE_OR_Orders_File_Path = @"C:\tt\order_details\ase_or_orders.csv";
        private string ASE_QS_Orders_File_Path = @"C:\tt\order_details\ase_qs_orders.csv";
        private string OR_Orders_File_Path_i = @"C:\tt\order_details\or_orders_copy.csv";
        private string QS_Orders_File_Path_i = @"C:\tt\order_details\qs_orders_copy.csv";
        private string ASE_OR_Orders_File_Path_i = @"C:\tt\order_details\ase_or_orders_copy.csv";
        private string ASE_QS_Orders_File_Path_i = @"C:\tt\order_details\ase_qs_orders_copy.csv";
        private ConcurrentDictionary<String, CancellationTokenSource> cts = new ConcurrentDictionary<String, CancellationTokenSource>();

        System.Timers.Timer m_timer = null;
        System.Timers.Timer m_timer_2 = null;
        System.Timers.Timer m_timer_3 = null;
        System.Timers.Timer m_timer_4 = null;
        int initializa_time_min = 5;
        public void Start(tt_net_sdk.TTAPIOptions apiConfig)
        {
            m_disp = tt_net_sdk.Dispatcher.AttachWorkerDispatcher();
            m_disp.DispatchAction(() =>
            {
                Init(apiConfig);
            });

            m_disp.Run();
        }
        

        public void Init(tt_net_sdk.TTAPIOptions apiConfig)
        {
            ApiInitializeHandler apiInitializeHandler = new ApiInitializeHandler(ttNetApiInitHandler);
            TTAPI.ShutdownCompleted += TTAPI_ShutdownCompleted;
            TTAPI.CreateTTAPI(tt_net_sdk.Dispatcher.Current, apiConfig, apiInitializeHandler);
        }

        public void ttNetApiInitHandler(TTAPI api, ApiCreationException ex)
        {
            if (ex == null)
            {
                Console.WriteLine("TT.NET SDK Initialization Complete");
                if (environment.ToString().Contains("Sim"))
                {
                    button20.Text = "SIM";
                    button20.BackColor = Color.Green;
                    button21.Text = account_name;
                    button21.BackColor = Color.Green;
                }
                else if (environment.ToString().Contains("Live"))
                {
                    button20.Text = "LIVE";
                    button20.BackColor = Color.Red;
                    button21.Text = account_name;
                    button20.BackColor = Color.Red;
                }
                else
                {
                    button20.Text = "UNKNOWN";
                    button20.BackColor = Color.DarkGray;
                    button21.Text = account_name;
                    button21.BackColor = Color.Green;
                }
                m_api = api;
                m_api.TTAPIStatusUpdate += new EventHandler<TTAPIStatusUpdateEventArgs>(m_api_TTAPIStatusUpdate);
                m_api.Start();
            }
            else if (ex.IsRecoverable)
            {
                // this is in informational update from the SDK
                Console.WriteLine("TT.NET SDK Initialization Message: {0}", ex.Message);
                if (ex.Code == ApiCreationException.ApiCreationError.NewAPIVersionAvailable)
                {
                    // a newer version of the SDK is available - notify someone to upgrade
                }
            }
            else
            {
                Console.WriteLine("TT.NET SDK Initialization Failed: {0}", ex.Message);
                if (ex.Code == ApiCreationException.ApiCreationError.NewAPIVersionRequired)
                {
                    // do something to upgrade the SDK package since it will not start until it is upgraded 
                    // to the minimum version noted in the exception message
                }
                Dispose();
            }
        }

        public void m_api_TTAPIStatusUpdate(object sender, TTAPIStatusUpdateEventArgs e)
        {
            Console.WriteLine("TTAPIStatusUpdate: {0}", e);
            if (e.IsReady == false)
            {
                // TODO: Do any connection lost processing here
                return;
            }
          
            //
            if (object.ReferenceEquals(m_instrLookupRequest, null) == false)
                return;

            // Status is up and we have not started a subscription yet

            // connection to TT is established
            Console.WriteLine("TT.NET SDK Authenticated");
            /*
            MarketId marketKey = Market.GetMarketIdFromName(m_market);
            
            */
            Order_Action_Functions = new Order_Action_Functions();
            Order_Action_Functions.dispatcher = tt_net_sdk.Dispatcher.Current;
            m_instrumentTradeSubscription = new TradeSubscription(tt_net_sdk.Dispatcher.Current);
            tsiAF = new TradeSubscriptionTTAccountFilter(account_name, false, "Acct Filter");
            m_instrumentTradeSubscription.SetFilter(tsiAF);

            m_instrumentTradeSubscription.OrderUpdated += new EventHandler<OrderUpdatedEventArgs>(m_instrumentTradeSubscription_OrderUpdated);
            m_instrumentTradeSubscription.OrderAdded += new EventHandler<OrderAddedEventArgs>(m_instrumentTradeSubscription_OrderAdded);
            m_instrumentTradeSubscription.OrderDeleted += new EventHandler<OrderDeletedEventArgs>(m_instrumentTradeSubscription_OrderDeleted);
            m_instrumentTradeSubscription.OrderFilled += new EventHandler<OrderFilledEventArgs>(m_instrumentTradeSubscription_OrderFilled);
            m_instrumentTradeSubscription.OrderRejected += new EventHandler<OrderRejectedEventArgs>(m_instrumentTradeSubscription_OrderRejected);
            m_instrumentTradeSubscription.OrderBookDownload += new EventHandler<OrderBookDownloadEventArgs>(m_instrumentTradeSubscription_OrderBookDownload);
            m_instrumentTradeSubscription.Start();
            m_timer = new System.Timers.Timer()
            {
                Interval = 1000,
                Enabled = true,
                AutoReset = true
            };
            m_timer.Elapsed += new ElapsedEventHandler(m_pause_play_UpdateHandler);
            m_timer.Start();
            m_accounts = m_api.Accounts;
            comboBox1.Items.AddRange(new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12"," " });
            comboBox2.Items.AddRange(new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
                                                    "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40",
                                                    "41", "42", "43", "44", "45", "46", "47", "48", "49", "50","51", "52", "53", "54", "55", "56", "57", "58", "59", "60"," "});
            comboBox3.Items.AddRange(new string[] { "AM", "PM" });

            comboBox4.Items.AddRange(new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", " " });
            comboBox5.Items.AddRange(new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
                                                    "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40",
                                                    "41", "42", "43", "44", "45", "46", "47", "48", "49", "50","51", "52", "53", "54", "55", "56", "57", "58", "59", "60"," "});
            comboBox6.Items.AddRange(new string[] { "AM", "PM" });

            comboBox7.Items.AddRange(new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" });
            comboBox8.Items.AddRange(new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
                                                    "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40",
                                                    "41", "42", "43", "44", "45", "46", "47", "48", "49", "50","51", "52", "53", "54", "55", "56", "57", "58", "59", "60"});
            comboBox9.Items.AddRange(new string[] { "AM", "PM" });
            comboBox10.Items.AddRange(new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" });
            comboBox11.Items.AddRange(new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
                                                    "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40",
                                                    "41", "42", "43", "44", "45", "46", "47", "48", "49", "50","51", "52", "53", "54", "55", "56", "57", "58", "59", "60"});
            comboBox12.Items.AddRange(new string[] { "AM", "PM" });
            comboBox13.Items.AddRange(new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" });
            comboBox14.Items.AddRange(new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
                                                    "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40",
                                                    "41", "42", "43", "44", "45", "46", "47", "48", "49", "50","51", "52", "53", "54", "55", "56", "57", "58", "59", "60"});
            comboBox15.Items.AddRange(new string[] { "AM", "PM" });
            comboBox16.Items.AddRange(new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" });
            comboBox17.Items.AddRange(new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
                                                    "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40",
                                                    "41", "42", "43", "44", "45", "46", "47", "48", "49", "50","51", "52", "53", "54", "55", "56", "57", "58", "59", "60"});
            comboBox18.Items.AddRange(new string[] { "AM", "PM" });
            comboBox19.Items.AddRange(new string[] { "sr1_zq_spread", "sr3_zq_spread", "sr3_zq_naked","sr1_zq_naked", "spread_ase" , "meeting_ase","fly_ase","sr3_sr1_naked" });
            m_timer_2 = new System.Timers.Timer()
            {
                Interval = 1000,
                Enabled = true,
                AutoReset = true
            };
            m_timer_2.Elapsed += new ElapsedEventHandler(m_delete_order_handler);
            m_timer_2.Start();
            m_timer_3 = new System.Timers.Timer()
            {
                Interval = 500,
                Enabled = true,
                AutoReset = true
            };
            m_timer_3.Elapsed += new ElapsedEventHandler(m_ase_updater);
            m_timer_3.Start();
            m_timer_4 = new System.Timers.Timer()
            {
                Interval = 2000,
                Enabled = true,
                AutoReset = true
            };
            m_timer_4.Elapsed += new ElapsedEventHandler(m_ase_iniaializer);
            m_timer_4.Start();
        }
        private void m_pause_play_UpdateHandler(object sender, EventArgs e)
        {
            if (m_isOrderBookDownloaded)
            {
                if (pause_play)
                {
                    lock (m_Hold)
                    {
                        if (DateTime.Today.Date == pause_date & DateTime.Now.ToString("h:mm tt") == pause_time & or_orders != null & !or_orders_on_hold & or_pause)
                        {
                            or_orders_on_hold = true;
                            Console.WriteLine("In O/R Hold");
                            Order_Action_Functions.Hold_Orders(or_orders, "O/R", m_instrumentTradeSubscription);
                            pause_play = false;
                        }
                        Console.WriteLine(DateTime.Today.Date == pause_date);
                        Console.WriteLine(DateTime.Now.ToString("h:mm tt") == pause_time);
                        Console.WriteLine(qs_orders != null);
                        Console.WriteLine(!qs_orders_on_hold);
                        Console.WriteLine(qs_pause);
                        // For Holding QS Orders:
                        if (DateTime.Today.Date == pause_date & DateTime.Now.ToString("h:mm tt") == pause_time & qs_orders != null & !qs_orders_on_hold & qs_pause)
                        {
                            qs_orders_on_hold = true;
                            Console.WriteLine("In QS Hold");
                            Order_Action_Functions.Hold_Orders(qs_orders, "QS", m_instrumentTradeSubscription);
                            pause_play = false;
                        }
                        if (DateTime.Today.Date == pause_date & DateTime.Now.ToString("h:mm tt") == pause_time & ase_or_orders != null & !ase_or_orders_on_hold & ase_or_pause)
                        {
                            ase_or_orders_on_hold = true;
                            Console.WriteLine("In ASE O/R Hold");
                            Order_Action_Functions.Hold_Orders(ase_or_orders, "ASE O/R", m_instrumentTradeSubscription);
                            pause_play = false;
                        }
                        if (DateTime.Today.Date == pause_date & DateTime.Now.ToString("h:mm tt") == pause_time & ase_qs_orders != null & !ase_qs_orders_on_hold & ase_qs_pause)
                        {
                            ase_qs_orders_on_hold = true;
                            Console.WriteLine("In ASE QS Hold");
                            Order_Action_Functions.Hold_Orders(ase_qs_orders, "ASE QS", m_instrumentTradeSubscription);
                            pause_play = false;
                        }
                    }
                }
                lock (m_Resume)
                {
                    if (DateTime.Today.Date == resume_date & DateTime.Now.ToString("h:mm tt") == resume_time & or_orders != null & or_pause)
                    {
                        Console.WriteLine("In O/R Resume");                       
                        Order_Action_Functions.Resume_Orders(or_orders, "O/R", m_instrumentTradeSubscription);
                        or_pause= false;
                    }

                    if (DateTime.Today.Date == resume_date & DateTime.Now.ToString("h:mm tt") == resume_time & qs_orders != null  & qs_pause)
                    {
                        Console.WriteLine("In QS Resume");
                        Order_Action_Functions.Resume_Orders(qs_orders, "QS", m_instrumentTradeSubscription);
                        qs_pause= false;
                    }
                    if (DateTime.Today.Date == resume_date & DateTime.Now.ToString("h:mm tt") == resume_time & ase_or_orders != null  & ase_or_pause)
                    {
                        Console.WriteLine("In ASE O/R Resume");
                        Order_Action_Functions.Resume_Orders(ase_or_orders, "ASE O/R", m_instrumentTradeSubscription);
                        ase_or_pause= false;
                    }

                    if (DateTime.Today.Date == resume_date & DateTime.Now.ToString("h:mm tt") == resume_time & ase_qs_orders != null  & ase_qs_pause)
                    {
                        Console.WriteLine("In ASE QS Resume");
                        Order_Action_Functions.Resume_Orders(ase_qs_orders, "ASE QS", m_instrumentTradeSubscription);
                        ase_qs_pause= false;
                    }
                }
            }
        }
        private void m_delete_order_handler(object sender, EventArgs e)
        {
            if (m_isOrderBookDownloaded & (delete_add | file_add))
            {
                lock (m_Orders_Storing)
                {
                    if (DateTime.Today.Date == delete_date & DateTime.Now.ToString("h:mm tt") == delete_time & or_orders != null & !or_orders_stored & or_delete)
                    {
                        or_orders_stored = true;
                        or_orders_copy = new ConcurrentDictionary<string, Order>(or_orders);
                        Console.WriteLine("In O/R Storing");
                        Order_Action_Functions.Write_in_Txt_File(or_orders_copy, OR_Orders_File_Path);
                        Order_Action_Functions.Delete_Orders_without_storing(or_orders, "O/R", m_instrumentTradeSubscription);
                    }
                    if (DateTime.Today.Date == delete_date & DateTime.Now.ToString("h:mm tt") == delete_time & qs_orders != null & !qs_orders_stored & qs_delete)
                    {
                        qs_orders_stored = true;
                        qs_orders_copy = new ConcurrentDictionary<string, Order>(qs_orders);
                        Console.WriteLine("In QS Storing");
                        Console.WriteLine("QS Copy Count: " + qs_orders_copy.Count());
                        if (File.Exists(QS_Orders_File_Path))
                        {
                            File.Delete(QS_Orders_File_Path);
                        }
                        Order_Action_Functions.Write_in_Txt_File(qs_orders_copy, QS_Orders_File_Path);
                        Order_Action_Functions.Delete_Orders_without_storing(qs_orders, "QS", m_instrumentTradeSubscription);
                    }
                    if (DateTime.Today.Date == delete_date & DateTime.Now.ToString("h:mm tt") == delete_time & ase_or_orders != null & !ase_or_orders_stored & ase_or_delete)
                    {
                        ase_or_orders_stored = true;
                    
                        ase_or_orders_copy = new ConcurrentDictionary<string, Order>(ase_or_orders);
                        Console.WriteLine("In ASE O/R Storing");
                        if (File.Exists(ASE_OR_Orders_File_Path))
                        {
                            File.Delete(ASE_OR_Orders_File_Path);
                        }
                        Order_Action_Functions.Write_in_Txt_File(ase_or_orders_copy, ASE_OR_Orders_File_Path);
                        Order_Action_Functions.Delete_Orders_without_storing(ase_or_orders, "ASE O/R", m_instrumentTradeSubscription);
                    }
                    if (DateTime.Today.Date == delete_date & DateTime.Now.ToString("h:mm tt") == delete_time & ase_qs_orders != null & !ase_qs_orders_stored & ase_qs_delete)
                    {
                        ase_qs_orders_stored = true;
                        ase_qs_orders_copy = new ConcurrentDictionary<string, Order>(ase_qs_orders);
                        Console.WriteLine("In ASE QS Storing");
                        if (File.Exists(ASE_QS_Orders_File_Path))
                        {
                            File.Delete(ASE_QS_Orders_File_Path);
                        }
                        Order_Action_Functions.Write_in_Txt_File(ase_qs_orders_copy, ASE_QS_Orders_File_Path);
                        Order_Action_Functions.Delete_Orders_without_storing(ase_qs_orders, "ASE QS", m_instrumentTradeSubscription);
                    }
                }
            
                lock (m_Orders_Routing)
                {
                  
                    if (DateTime.Today.Date == order_add_date & DateTime.Now.ToString("h:mm tt") == order_add_time & ((or_orders_stored & or_orders_copy != null) | file_add) & or_add)
                    {
                        or_orders_stored = false;
                        Console.WriteLine("In O/R New Order");

                        if (!file_add)
                        {
                            if (Order_Action_Functions.or_orders_copy.Count != 0)
                            {
                                Order_Action_Functions.Send_Order_Normal(or_orders_copy, "O/R", m_instrumentTradeSubscription, m_accounts, account_idx);
                            }
                            else
                            {
                                Order_Action_Functions.Read_Txt_File(Order_Action_Functions.or_orders_copy, OR_Orders_File_Path, "OR", m_instrumentTradeSubscription, m_accounts, account_idx, false);
                                order_added_from_file_time = DateTime.Now;
                            }
                        }
                        else
                        {
                            if (Order_Action_Functions.or_orders_copy.Count != 0)
                            {
                                Console.WriteLine("Dict Contains Data: Count: " + Order_Action_Functions.or_orders_copy.Count());
                                Order_Action_Functions.or_orders_copy.Clear();
                            }
                            Order_Action_Functions.Read_Txt_File(Order_Action_Functions.or_orders_copy, OR_Orders_File_Path, "OR", m_instrumentTradeSubscription, m_accounts, account_idx, false);
                            order_added_from_file_time = DateTime.Now;
                            file_add = false;
                        }
                      
                        if (File.Exists(OR_Orders_File_Path))
                        {
                            if (File.Exists(OR_Orders_File_Path_i))
                            {

                                File.Delete(OR_Orders_File_Path_i);
                            }
                            System.IO.FileInfo file = new System.IO.FileInfo(OR_Orders_File_Path_i);
                            file.Directory.Create();
                            File.Copy(OR_Orders_File_Path, OR_Orders_File_Path_i, true);
                            File.Delete(OR_Orders_File_Path);
                        }
                        or_add = false;
                    }
                    if (DateTime.Today.Date == order_add_date & DateTime.Now.ToString("h:mm tt") == order_add_time & ((qs_orders_stored & qs_orders_copy != null) | file_add) & qs_add)
                    {
                        qs_orders_stored = false;
                        Console.WriteLine("In QS New Order");
                        if (!file_add)
                        {
                            if (Order_Action_Functions.qs_orders_copy.Count != 0)
                            {
                                Order_Action_Functions.Send_Order_Normal(qs_orders_copy, "QS", m_instrumentTradeSubscription, m_accounts, account_idx);
                            }
                            else
                            {
                                Order_Action_Functions.Read_Txt_File(Order_Action_Functions.qs_orders_copy, QS_Orders_File_Path, "QS", m_instrumentTradeSubscription, m_accounts, account_idx, false);
                                order_added_from_file_time = DateTime.Now;
                            }
                        }
                        else
                        {
                            if (Order_Action_Functions.qs_orders_copy.Count != 0)
                            {
                                Console.WriteLine("Dict Contains Data: Count: " + Order_Action_Functions.qs_orders_copy.Count());
                                Order_Action_Functions.qs_orders_copy.Clear();
                            }
                            Order_Action_Functions.Read_Txt_File(Order_Action_Functions.qs_orders_copy, QS_Orders_File_Path, "QS", m_instrumentTradeSubscription, m_accounts, account_idx, false);
                            order_added_from_file_time = DateTime.Now;
                            file_add = false;
                        }
                        if (File.Exists(QS_Orders_File_Path))
                        {
                            if (File.Exists(QS_Orders_File_Path_i))
                            {

                                File.Delete(QS_Orders_File_Path_i);
                            }
                            System.IO.FileInfo file = new System.IO.FileInfo(QS_Orders_File_Path_i);
                            file.Directory.Create();
                            File.Copy(QS_Orders_File_Path, QS_Orders_File_Path_i, true);
                            File.Delete(QS_Orders_File_Path);
                        }
                        qs_add = false;
                    }
                    if (DateTime.Today.Date == order_add_date & DateTime.Now.ToString("h:mm tt") == order_add_time & ((ase_or_orders_stored & ase_or_orders_copy != null) | file_add) & ase_or_add)
                    {
                        ase_or_orders_stored = false;
                        Console.WriteLine("In ASE O/R New Order");
                        if (!file_add)
                        {
                            if (Order_Action_Functions.ase_or_orders_copy.Count != 0)
                            {
                                Order_Action_Functions.Send_Orders_Task_Creator(ase_or_orders_copy, "ASE O/R", m_instrumentTradeSubscription, m_accounts, account_idx);
                            }
                            else
                            {
                                Order_Action_Functions.Read_Txt_File(Order_Action_Functions.ase_or_orders_copy, ASE_OR_Orders_File_Path, "ASE OR", m_instrumentTradeSubscription, m_accounts, account_idx, true);
                                order_added_from_file_time = DateTime.Now;
                            }
                        }
                        else
                        {
                            if (Order_Action_Functions.ase_or_orders_copy.Count != 0)
                            {
                                Console.WriteLine("Dict Contains Data: Count: " + Order_Action_Functions.ase_or_orders_copy.Count());
                                Order_Action_Functions.ase_or_orders_copy.Clear();
                            }
                            Order_Action_Functions.Read_Txt_File(Order_Action_Functions.ase_or_orders_copy, ASE_OR_Orders_File_Path, "ASE OR", m_instrumentTradeSubscription, m_accounts, account_idx, true);
                            order_added_from_file_time = DateTime.Now;
                        }
                        if (File.Exists(ASE_OR_Orders_File_Path))
                        {
                            if (File.Exists(ASE_OR_Orders_File_Path_i))
                            {

                                File.Delete(ASE_OR_Orders_File_Path_i);
                            }
                            System.IO.FileInfo file = new System.IO.FileInfo(ASE_OR_Orders_File_Path_i);
                            file.Directory.Create();
                            File.Copy(ASE_OR_Orders_File_Path, ASE_OR_Orders_File_Path_i, true);
                            File.Delete(ASE_OR_Orders_File_Path);
                        }
                        ase_or_add = false;
                    }
                    if (DateTime.Today.Date == order_add_date & DateTime.Now.ToString("h:mm tt") == order_add_time & ((ase_qs_orders_stored & ase_qs_orders_copy != null) | file_add) & ase_qs_add)
                    {
                        ase_qs_orders_stored = false;
                        Console.WriteLine("In ASE QS New Order");
                        if (!file_add)
                        {
                            if (Order_Action_Functions.ase_qs_orders_copy.Count != 0)
                            {
                                Order_Action_Functions.Send_Orders_Task_Creator(ase_qs_orders_copy, "ASE QS", m_instrumentTradeSubscription, m_accounts, account_idx);
                            }
                            else
                            {
                                Order_Action_Functions.Read_Txt_File(Order_Action_Functions.ase_qs_orders_copy, ASE_QS_Orders_File_Path, "ASE QS", m_instrumentTradeSubscription, m_accounts, account_idx, true);
                                order_added_from_file_time = DateTime.Now;
                            }
                        }
                        else
                        {
                            if (Order_Action_Functions.ase_qs_orders_copy.Count != 0)
                            {
                                Console.WriteLine("Dict Contains Data: Count: " + Order_Action_Functions.ase_qs_orders_copy.Count());
                                Order_Action_Functions.ase_qs_orders_copy.Clear();
                            }
                            Order_Action_Functions.Read_Txt_File(Order_Action_Functions.ase_qs_orders_copy, ASE_QS_Orders_File_Path, "ASE QS", m_instrumentTradeSubscription, m_accounts, account_idx, true);
                            order_added_from_file_time = DateTime.Now;
                        }
                        if (File.Exists(ASE_QS_Orders_File_Path))
                        {
                            if (File.Exists(ASE_QS_Orders_File_Path_i))
                            {

                                File.Delete(ASE_QS_Orders_File_Path_i);
                            }
                            System.IO.FileInfo file = new System.IO.FileInfo(ASE_QS_Orders_File_Path_i);
                            file.Directory.Create();
                            File.Copy(ASE_QS_Orders_File_Path, ASE_QS_Orders_File_Path_i, true);
                            File.Delete(ASE_QS_Orders_File_Path);
                        }
                        ase_qs_add = false;
                    }
                }
            }
             if (m_isOrderBookDownloaded & delete_only)
            {
                lock (m_delete)
                {
                    
                    if (DateTime.Today.Date == delete_date_1 & DateTime.Now.ToString("h:mm tt") == delete_time_1 & or_orders != null & or_delete_1)
                    {
                        Console.WriteLine("In O/R delete");               
                        Order_Action_Functions.Delete_Orders_without_storing(or_orders, "O/R", m_instrumentTradeSubscription);
                        delete_only = false;
                    }

                    if (DateTime.Today.Date == delete_date_1 & DateTime.Now.ToString("h:mm tt") == delete_time_1 & qs_orders != null & qs_delete_1)
                    {
                        Console.WriteLine("In QS Delete");
                        Order_Action_Functions.Delete_Orders_without_storing(qs_orders, "QS", m_instrumentTradeSubscription);
                        delete_only = false;
                    }

                    if (DateTime.Today.Date == delete_date_1 & DateTime.Now.ToString("h:mm tt") == delete_time_1 & ase_or_orders != null &  ase_or_delete_1)
                    {
                        Console.WriteLine("In ASE O/R Delete");
                        Order_Action_Functions.Delete_Orders_without_storing(ase_or_orders, "ASE O/R", m_instrumentTradeSubscription);
                        delete_only = false;
                    }

                    if (DateTime.Today.Date == delete_date_1 & DateTime.Now.ToString("h:mm tt") == delete_time_1 & ase_qs_orders != null & ase_qs_delete_1)
                    {
                        Console.WriteLine("In ASE QS delete");
                        Order_Action_Functions.Delete_Orders_without_storing(ase_qs_orders, "ASE QS", m_instrumentTradeSubscription);
                        delete_only = false;
                    }
                }
            }
        }
        private void m_ase_updater(object sender, EventArgs e)
        {
            if (m_isOrderBookDownloaded)
            {
                foreach (var kvp in ase_or_orders)
                {
                    
                    string lowercaseValue = kvp.Value.Instrument.ToString().ToLower(); // Convert to lowercase for case-insensitive comparison
                    if ((lowercaseValue.Contains("sr1") && lowercaseValue.Contains("sr3")) || (lowercaseValue.Contains(".net") && lowercaseValue.Contains("sr1")))
                    {
                        if (!sr3_sr1_naked.ContainsKey(kvp.Key))
                        {
                            sr3_sr1_naked.TryAdd(kvp.Key, kvp.Value);
                            if (selected_ase == "sr3_sr1_naked")
                            {
                                dataGridView1.Invoke(new Action(() => 
                                dataGridView1.Rows.Add(kvp.Key, kvp.Value.Instrument, kvp.Value.BuySell, kvp.Value.LimitPrice, kvp.Value.OrderQuantity, kvp.Value.OrderQuantity - kvp.Value.FillQuantity, kvp.Value.DisclosedQuantity,false,0,"","Inactive",kvp.Value.ExchTransactionTime)
                                
                                ));
                                dgstatus();
                            }
                        }
                    }
                    if (((lowercaseValue.Contains("sr3") && lowercaseValue.Contains("zq")) || (lowercaseValue.Contains(".net") && lowercaseValue.Contains("zq"))) && !lowercaseValue.Contains(" 3") && !lowercaseValue.Contains("fl") && !lowercaseValue.Contains("meeting") && !lowercaseValue.Contains("()"))
                    {
                        if (!sr3_zq_naked.ContainsKey(kvp.Key))
                        {
                            sr3_zq_naked.TryAdd(kvp.Key, kvp.Value);
                            if (selected_ase == "sr3_zq_naked")
                            {
                                dataGridView1.Invoke(new Action(() =>
                                dataGridView1.Rows.Add(kvp.Key, kvp.Value.Instrument, kvp.Value.BuySell, kvp.Value.LimitPrice, kvp.Value.OrderQuantity, kvp.Value.OrderQuantity - kvp.Value.FillQuantity, kvp.Value.DisclosedQuantity, false,0, "", "Inactive", kvp.Value.ExchTransactionTime)
                                ));
                                dgstatus();

                            }
                        }
                    }
                    if (lowercaseValue.Contains("leg") || lowercaseValue.Contains(" a "))
                    {
                        if (!spread_ase.ContainsKey(kvp.Key))
                        {
                            spread_ase.TryAdd(kvp.Key, kvp.Value);
                            if (selected_ase == "spread_ase")
                            {
                                dataGridView1.Invoke(new Action(() =>
                                dataGridView1.Rows.Add(kvp.Key, kvp.Value.Instrument, kvp.Value.BuySell, kvp.Value.LimitPrice, kvp.Value.OrderQuantity, kvp.Value.OrderQuantity - kvp.Value.FillQuantity, kvp.Value.DisclosedQuantity, false,0, "", "Inactive", kvp.Value.ExchTransactionTime)
                                ));
                                dgstatus();

                            }

                        }
                    }
                   
                    if (lowercaseValue.Contains("sr1") && lowercaseValue.Contains("vs "))
                    {
                        if (!sr1_zq_naked.ContainsKey(kvp.Key))
                        {
                            sr1_zq_naked.TryAdd(kvp.Key, kvp.Value);
                            if (selected_ase == "sr1_zq_naked")
                            {
                                dataGridView1.Invoke(new Action(() =>
                                dataGridView1.Rows.Add(kvp.Key, kvp.Value.Instrument, kvp.Value.BuySell, kvp.Value.LimitPrice, kvp.Value.OrderQuantity, kvp.Value.OrderQuantity - kvp.Value.FillQuantity, kvp.Value.DisclosedQuantity, false,0, "", "Inactive", kvp.Value.ExchTransactionTime)
                                ));
                                dgstatus();

                            }
                        }
                    }
                }
                foreach (var kvp in ase_qs_orders)
                {
                  
                    string lowercaseValue = kvp.Value.Instrument.ToString().ToLower(); // Convert to lowercase for case-insensitive comparison

                    if ((lowercaseValue.Contains(" 3") || lowercaseValue.Contains("( )") || lowercaseValue.Contains("st1..") || lowercaseValue.Contains("fl")) && !lowercaseValue.Contains("sr1"))
                    {
                        if (!sr3_zq_spread.ContainsKey(kvp.Key))
                        {
                            sr3_zq_spread.TryAdd(kvp.Key, kvp.Value);
                            if (selected_ase == "sr3_zq_spread")
                            {
                                dataGridView1.Invoke(new Action(() =>
                                dataGridView1.Rows.Add(kvp.Key, kvp.Value.Instrument, kvp.Value.BuySell, kvp.Value.LimitPrice, kvp.Value.OrderQuantity, kvp.Value.OrderQuantity - kvp.Value.FillQuantity, kvp.Value.DisclosedQuantity, false,0, "", "Inactive", kvp.Value.ExchTransactionTime)
                                ));
                                dgstatus();

                            }
                        }
                    }
                    if (lowercaseValue.Contains("meeting"))
                    {
                        if (!meeting_ase.ContainsKey(kvp.Key))
                        {
                            meeting_ase.TryAdd(kvp.Key, kvp.Value);
                            if (selected_ase == "meeting_ase")
                            {
                                dataGridView1.Invoke(new Action(() =>
                                dataGridView1.Rows.Add(kvp.Key, kvp.Value.Instrument, kvp.Value.BuySell, kvp.Value.LimitPrice, kvp.Value.OrderQuantity, kvp.Value.OrderQuantity - kvp.Value.FillQuantity, kvp.Value.DisclosedQuantity, false,0, "", "Inactive", kvp.Value.ExchTransactionTime)
                                ));
                                dgstatus();

                            }
                        }
                    }
                    if (lowercaseValue.Contains("fly")&&!lowercaseValue.Contains("leg1"))
                    {
                        if (!fly_ase.ContainsKey(kvp.Key))
                        {
                            fly_ase.TryAdd(kvp.Key, kvp.Value);
                            if (selected_ase == "fly_ase")
                            {
                                dataGridView1.Invoke(new Action(() =>
                                dataGridView1.Rows.Add(kvp.Key, kvp.Value.Instrument, kvp.Value.BuySell, kvp.Value.LimitPrice, kvp.Value.OrderQuantity, kvp.Value.OrderQuantity - kvp.Value.FillQuantity, kvp.Value.DisclosedQuantity, false,0, "", "Inactive", kvp.Value.ExchTransactionTime)
                                ));
                                dgstatus();

                            }
                        }
                    }
                    if (lowercaseValue.Contains("spread") || lowercaseValue.Contains("inter") || lowercaseValue.Contains(" 1.") || lowercaseValue.Contains(" 2."))
                    {
                        if (!sr1_zq_spread.ContainsKey(kvp.Key))
                        {
                            sr1_zq_spread.TryAdd(kvp.Key, kvp.Value);
                            if (selected_ase == "sr1_zq_spread")
                            {
                                dataGridView1.Invoke(new Action(() =>
                                dataGridView1.Rows.Add(kvp.Key, kvp.Value.Instrument, kvp.Value.BuySell, kvp.Value.LimitPrice, kvp.Value.OrderQuantity, kvp.Value.OrderQuantity - kvp.Value.FillQuantity, kvp.Value.DisclosedQuantity, false,0, "", "Inactive", kvp.Value.ExchTransactionTime)
                                ));
                            
                                dgstatus();
                            }
                        }
                    }
                }
              
            
           
            }
        }
        private void m_ase_iniaializer(object sender, EventArgs e)
        {
            foreach (var kvp in ase_fill_executed)
            {
                
                if ((DateTime.Now - kvp.Value).TotalMinutes>=initializa_time_min)
                {
                    Order_Action_Functions.initialize_order(kvp.Key, m_instrumentTradeSubscription, m_accounts, account_idx);
                    ase_fill_executed.TryRemove(kvp.Key, out DateTime dt);
                    if (ase_fill.TryGetValue(Convert.ToString(kvp.Key), out string st))
                    {
                        ase_fill[kvp.Key] = "residue";
                        ase_fill.TryUpdate(kvp.Key, "residue", st);
                    }
                    dgstatus();
                }
                
            }
        }
        public void Set_Default_Values_Ord_Management()
        {
            DateTime market_open_time = DateTime.Parse("3:30:00 AM");

            if (DateTime.Today.DayOfWeek == DayOfWeek.Monday & DateTime.Now.TimeOfDay > market_open_time.TimeOfDay)
            {
                checkBox15.Checked = true;
                checkBox13.Checked = true;

                dateTimePicker3.Value = DateTime.Today;
                comboBox16.Text = "3";
                comboBox17.Text = "35";
                comboBox18.Text = "AM";

                checkBox14.Checked = true;
                checkBox16.Checked = true;

                button4.PerformClick();
            }
        }
        void datagrid_updation(DataGridView dg, string x, Order or)
        {
            for (int i = 0; i < dg.RowCount; i++)
            {
                if (Convert.ToString(dg.Rows[i].Cells[0].Value).Contains(x))
                {
                    Console.WriteLine(or.OrderQuantity);
                    dg.Rows[i].Cells[0].Value = or.SiteOrderKey;
                    dg.Rows[i].Cells[1].Value = or.Instrument;
                    dg.Rows[i].Cells[2].Value = or.BuySell;
                    dg.Rows[i].Cells[3].Value = or.LimitPrice;
                    dg.Rows[i].Cells[4].Value = or.OrderQuantity;
                    dg.Rows[i].Cells[5].Value = or.OrderQuantity-or.FillQuantity;
                    dg.Rows[i].Cells[6].Value = or.DisclosedQuantity;
                    dg.Rows[i].Cells[11].Value = or.ExchTransactionTime;
                  
                }
            }
        }
        void datagrid_delete(DataGridView dg, string x)
        {
            for (int i = 0; i < dg.RowCount; i++)
            {
                if (Convert.ToString(dg.Rows[i].Cells[0].Value).Contains(x))
                {
                    dg.Rows.RemoveAt(i);


                }
            }
        }
        void dgstatus()
        {
            Thread.Sleep(1000);
            
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {

                if (ase_fill.TryGetValue(Convert.ToString(dataGridView1.Rows[i].Cells[0].Value),out string st))
                {
                    Console.WriteLine("st");
                    if(st== "failed")
                    {
                        dataGridView1.Rows[i].Cells[7].Value = false;
                        dataGridView1.Rows[i].Cells[10].Value = "failed";
                        dataGridView1.Rows[i].Cells[10].Style.BackColor = Color.Red;
                        dataGridView1.Rows[i].Cells[10].Style.ForeColor = Color.White;
                    }

                    else if (st == "Active")
                    {
                        dataGridView1.Rows[i].Cells[7].Value = true;
                        dataGridView1.Rows[i].Cells[10].Value = "Active";
                        dataGridView1.Rows[i].Cells[10].Style.BackColor = Color.Green;
                        dataGridView1.Rows[i].Cells[10].Style.ForeColor = Color.White;
                    }
                    else
                    {
                        dataGridView1.Rows[i].Cells[7].Value = false;
                        dataGridView1.Rows[i].Cells[10].Value = st;
                        dataGridView1.Rows[i].Cells[10].Style.BackColor = Color.Beige;
                        dataGridView1.Rows[i].Cells[10].Style.ForeColor = Color.Black;
                    }


                }
               

            }
        }
        void UpdateValueIfPresent(ConcurrentDictionary<string, Order> dictionary, string key, Order newValue)
        {
           
            if (dictionary.TryGetValue(key, out Order existingValue))
            {
                // The key was present, update the value
                dictionary.TryUpdate(key, newValue, existingValue);
            }
            
        }
        public ConcurrentDictionary<string, Order> Dictionary_finder(String selected_ase)
        {
            if (selected_ase == "sr1_zq_spread")
            {
                return sr1_zq_spread;
            }
            else if (selected_ase == "sr1_zq_naked")
            {
                return sr1_zq_naked;
            }
            else if (selected_ase == "sr3_zq_spread")
            {
                return sr3_zq_spread;
            }
            else if (selected_ase == "sr3_zq_naked")
            {
                return sr3_zq_naked;
            }
            else if (selected_ase == "sr3_sr1_naked")
            {
                return sr3_sr1_naked;
            }
            else if (selected_ase == "meeting_ase")
            {
                return meeting_ase;
            }
            else if (selected_ase == "fly_ase")
            {
                return fly_ase;
            }
            else if (selected_ase == "spread_ase")
            {
                return spread_ase;

            }
            return new ConcurrentDictionary<string, Order>();
        }

        private void fill_on_data(bool condition, String Order_key, int er)
        {
            if (condition == true)

            {
                lock (m_Lock)
                {
                    string new_key = Order_Action_Functions.Change_Order_Reload(Order_key, er, m_instrumentTradeSubscription, m_accounts, account_idx);
                    if (new_key == "failed")
                    {
                        if (ase_fill.TryGetValue(Order_key, out String existingValue))
                        {
                            ase_fill.TryUpdate(Order_key, "failed", existingValue);
                        }

                    }
                    else
                    {
                        ase_fill[Order_key] = "Residue";
                        if (ase_fill.TryGetValue(new_key, out String existingValue))
                        {
                            // The key was present, update the value
                            ase_fill.TryUpdate(new_key, "Executed+" + er, existingValue);
                        }
                        else
                        {
                            ase_fill.TryAdd(new_key, "Executed+" + er);

                        }
                        
                            ase_fill_executed.TryAdd(new_key, DateTime.Now);

                    }
                    if (cts.TryGetValue(Order_key, out var cts_1))
                    {

                        cts_1.Cancel();
                    }
                    cts.TryRemove(Order_key, out var ctss_1);
                    dgstatus();
                }
            }
        }
        void m_instrumentTradeSubscription_OrderBookDownload(object sender, OrderBookDownloadEventArgs e)
        {
            Console.WriteLine("Orderbook downloaded...");
           
            List<Order> all_orders_lst = e.Orders.ToList();
            foreach (Order ord in all_orders_lst)
            {
                if (ord.Instrument.Product.Type == ProductType.Future & !or_orders.ContainsKey(ord.SiteOrderKey) & ((ord.OrderSource != OrderSource.Ase) & (ord.OrderSource != OrderSource.PrimeAse)))
                {
                    or_orders.TryAdd(ord.SiteOrderKey, ord);
                }
                if (ord.Instrument.Product.Type == ProductType.MultilegInstrument & !qs_orders.ContainsKey(ord.SiteOrderKey) & ((ord.OrderSource != OrderSource.Ase) & (ord.OrderSource != OrderSource.PrimeAse)))
                {
                    qs_orders.TryAdd(ord.SiteOrderKey, ord);
                }
                if (ord.Instrument.Product.Type == ProductType.Synthetic & ord.Algo == null)
                {
                    SpreadDetails sp_detail = ord.Instrument.GetSpreadDetails();
                    SpreadLegDetails sp_leg = sp_detail.GetLeg(0);
                    Console.WriteLine(ord.SiteOrderKey);
                    Console.WriteLine(ord.SyntheticType);
                    if (sp_leg.Instrument.Product.Type == ProductType.Future & !ase_or_orders.ContainsKey(ord.SiteOrderKey))
                    {
                        ase_or_orders.TryAdd(ord.SiteOrderKey, ord);
                    }
                    if (sp_leg.Instrument.Product.Type == ProductType.MultilegInstrument & !ase_qs_orders.ContainsKey(ord.SiteOrderKey))
                    {
                        ase_qs_orders.TryAdd(ord.SiteOrderKey, ord);
                    }
                }
            }

            m_isOrderBookDownloaded = true;
            button7.Text = "Running";
            button7.BackColor = Color.Green;
            button7.ForeColor = Color.White;
          
        }
        void m_instrumentTradeSubscription_OrderRejected(object sender, OrderRejectedEventArgs e)
        {
            Console.WriteLine("\nOrderRejected [{0}]", e.Order.SiteOrderKey);
            
        }
        void m_instrumentTradeSubscription_OrderFilled(object sender, OrderFilledEventArgs e)
        {
           
                if (e.FillType == tt_net_sdk.FillType.Full)
                {
                datagrid_delete(dataGridView1, e.Fill.SiteOrderKey);
                sr1_zq_spread.TryRemove(e.OldOrder.SiteOrderKey, out Order orders_1);
                sr1_zq_naked.TryRemove(e.OldOrder.SiteOrderKey, out Order orders_2);
                sr3_zq_spread.TryRemove(e.OldOrder.SiteOrderKey, out Order orders_3);
                sr3_zq_naked.TryRemove(e.OldOrder.SiteOrderKey, out Order orders_4);
                sr3_sr1_naked.TryRemove(e.OldOrder.SiteOrderKey, out Order orders_5);
                spread_ase.TryRemove(e.OldOrder.SiteOrderKey, out Order orders_6);
                fly_ase.TryRemove(e.OldOrder.SiteOrderKey, out Order orders_7);
                meeting_ase.TryRemove(e.OldOrder.SiteOrderKey, out Order orders_8);
                if(ase_fill.TryGetValue(e.OldOrder.SiteOrderKey, out String orders_9))
                {
                    if (orders_9 == "Active")
                    {
                        if (cts.TryGetValue(e.OldOrder.SiteOrderKey, out var cts_1))
                        {
                            cts_1.Cancel();
                        }
                        cts.TryRemove(e.OldOrder.SiteOrderKey, out var ctss_1);
                    }
                    ase_fill.TryRemove(e.OldOrder.SiteOrderKey, out String order_9);

                }

                ase_fill_executed.TryRemove(e.OldOrder.SiteOrderKey, out DateTime orders_10);
                Console.WriteLine("\nOrderFullyFilled [{0}]: {1}@{2}", e.Fill.SiteOrderKey, e.Fill.Quantity, e.Fill.MatchPrice);

                    if (e.OldOrder.Instrument.Product.Type == ProductType.Future & or_orders.ContainsKey(e.OldOrder.SiteOrderKey)
                        & ((e.OldOrder.OrderSource != OrderSource.Ase) & (e.OldOrder.OrderSource != OrderSource.PrimeAse)))
                    {
                        Console.WriteLine("Filled: O/R Contains Key: " + or_orders.ContainsKey(e.OldOrder.SiteOrderKey));
                       
                        or_orders.TryRemove(e.OldOrder.SiteOrderKey,out Order orders);
                        Console.WriteLine("Filled: O/R Count: " + or_orders.Count());
                        
                    }
                    if (e.OldOrder.Instrument.Product.Type == ProductType.MultilegInstrument & qs_orders.ContainsKey(e.OldOrder.SiteOrderKey)
                        & ((e.OldOrder.OrderSource != OrderSource.Ase) & (e.OldOrder.OrderSource != OrderSource.PrimeAse)))
                    {
                        Console.WriteLine("Filled: QS Contains Key: " + qs_orders.ContainsKey(e.OldOrder.SiteOrderKey));
                       
                        qs_orders.TryRemove(e.OldOrder.SiteOrderKey, out Order order);
                        Console.WriteLine("Filled: QS Count: " + qs_orders.Count());
                       
                    }
                }
                else
                {
                UpdateValueIfPresent(sr1_zq_spread, e.OldOrder.SiteOrderKey, e.NewOrder);
                UpdateValueIfPresent(sr3_zq_spread, e.OldOrder.SiteOrderKey, e.NewOrder);
                UpdateValueIfPresent(sr1_zq_naked, e.OldOrder.SiteOrderKey, e.NewOrder);
                UpdateValueIfPresent(sr3_zq_naked, e.OldOrder.SiteOrderKey, e.NewOrder);
                UpdateValueIfPresent(sr3_sr1_naked, e.OldOrder.SiteOrderKey, e.NewOrder);
                UpdateValueIfPresent(spread_ase, e.OldOrder.SiteOrderKey, e.NewOrder);
                UpdateValueIfPresent(fly_ase, e.OldOrder.SiteOrderKey, e.NewOrder);
                UpdateValueIfPresent(meeting_ase, e.OldOrder.SiteOrderKey, e.NewOrder);
                

                datagrid_updation(dataGridView1, e.OldOrder.SiteOrderKey, e.NewOrder);
                Console.WriteLine("\nOrderPartiallyFilled [{0}]: {1}@{2}", e.Fill.SiteOrderKey, e.Fill.Quantity, e.Fill.MatchPrice);
                    if (e.Fill.Instrument.Product.Type == ProductType.Future & ((e.OldOrder.OrderSource != OrderSource.Ase) & (e.OldOrder.OrderSource != OrderSource.PrimeAse)))
                    {
                        Console.WriteLine("Partial Filled: O/R Contains Key: " + or_orders.ContainsKey(e.Fill.SiteOrderKey));
                        
                        or_orders[e.Fill.SiteOrderKey] = e.NewOrder;
                        Console.WriteLine("Partial Filled: O/R Count: " + or_orders.Count());
                      
                    }
                    if (e.Fill.Instrument.Product.Type == ProductType.MultilegInstrument & ((e.OldOrder.OrderSource != OrderSource.Ase) & (e.OldOrder.OrderSource != OrderSource.PrimeAse)))
                    {
                        Console.WriteLine("Partial Filled: QS Contains Key: " + qs_orders.ContainsKey(e.Fill.SiteOrderKey));
                        
                        qs_orders[e.Fill.SiteOrderKey] = e.NewOrder;
                        Console.WriteLine("Partial Filled: QS Count: " + qs_orders.Count());
                       
                    }
                    if (e.Fill.Instrument.Product.Type == ProductType.Synthetic & e.NewOrder.Algo == null)
                    {
                        Console.WriteLine("In Synthetic Partial Fills");
                        SpreadDetails sp_detail = e.Fill.Instrument.GetSpreadDetails();
                        SpreadLegDetails sp_leg = sp_detail.GetLeg(0);

                        if (sp_leg.Instrument.Product.Type == ProductType.Future & ase_or_orders.ContainsKey(e.Fill.SiteOrderKey))
                        {
                            Console.WriteLine("Partial Filled: ASE O/R Contains Key: " + ase_or_orders.ContainsKey(e.Fill.SiteOrderKey));
                           
                            ase_or_orders[e.Fill.SiteOrderKey] = e.NewOrder;
                            Console.WriteLine("Partial Filled: ASE O/R Count: " + ase_or_orders.Count());
                            
                        }
                        if (sp_leg.Instrument.Product.Type == ProductType.MultilegInstrument & ase_qs_orders.ContainsKey(e.Fill.SiteOrderKey))
                        {
                            Console.WriteLine("Partial Filled: ASE QS Contains Key: " + ase_qs_orders.ContainsKey(e.Fill.SiteOrderKey));
                           
                            ase_qs_orders[e.Fill.SiteOrderKey] = e.NewOrder;
                            Console.WriteLine("Partial Filled: ASE QS Count: " + ase_qs_orders.Count());
                          
                        }
                    }
                
            }
        }
        void m_instrumentTradeSubscription_OrderDeleted(object sender, OrderDeletedEventArgs e)
        {
            /* lock (m_OrderDeleted)
             {*/
            datagrid_delete(dataGridView1, e.OldOrder.SiteOrderKey);
            sr1_zq_spread.TryRemove(e.OldOrder.SiteOrderKey, out Order orders_1);
            sr1_zq_naked.TryRemove(e.OldOrder.SiteOrderKey, out Order orders_2);
            sr3_zq_spread.TryRemove(e.OldOrder.SiteOrderKey, out Order orders_3);
            sr3_zq_naked.TryRemove(e.OldOrder.SiteOrderKey, out Order orders_4);
            sr3_sr1_naked.TryRemove(e.OldOrder.SiteOrderKey, out Order orders_5);
            spread_ase.TryRemove(e.OldOrder.SiteOrderKey, out Order orders_6);
            fly_ase.TryRemove(e.OldOrder.SiteOrderKey, out Order orders_7);
            meeting_ase.TryRemove(e.OldOrder.SiteOrderKey, out Order orders_8);
            if (ase_fill.TryGetValue(e.OldOrder.SiteOrderKey, out String orders_9))
            {
                if (orders_9 == "Active")
                {
                    if (cts.TryGetValue(e.OldOrder.SiteOrderKey, out var cts_1))
                    {
                        cts_1.Cancel();
                    }
                    cts.TryRemove(e.OldOrder.SiteOrderKey, out var ctss_1);
                }
                ase_fill.TryRemove(e.OldOrder.SiteOrderKey, out String order_9);

            }
            ase_fill_executed.TryRemove(e.OldOrder.SiteOrderKey, out DateTime x_1);
            Console.WriteLine("\nOrderDeleted [{0}]", e.OldOrder.SiteOrderKey);

                if (e.OldOrder.Instrument.Product.Type == ProductType.Future & ((e.OldOrder.OrderSource != OrderSource.Ase) & (e.OldOrder.OrderSource != OrderSource.PrimeAse)))
                {
                    Console.WriteLine("Deleted: O/R Contains Key: " + or_orders.ContainsKey(e.OldOrder.SiteOrderKey));
                    
                    or_orders.TryRemove(e.OldOrder.SiteOrderKey, out Order orders);
                    Console.WriteLine("Deleted: O/R Count: " + or_orders.Count());
                    foreach (string key in or_orders.Keys)
                    {
                        Console.WriteLine(or_orders[key].Instrument + " " + or_orders[key].LimitPrice.ToString() + " " + or_orders[key].WorkingQuantity.ToString() + " " + key);
                        Console.WriteLine(or_orders[key].BuySell.ToString() + " " + or_orders[key].Instrument.Product.Type.ToString());
                    }
                   
                }
                if (e.OldOrder.Instrument.Product.Type == ProductType.MultilegInstrument & ((e.OldOrder.OrderSource != OrderSource.Ase) & (e.OldOrder.OrderSource != OrderSource.PrimeAse)))
                {
                    Console.WriteLine("Deleted: QS Contains Key: " + qs_orders.ContainsKey(e.OldOrder.SiteOrderKey));
                    
                    qs_orders.TryRemove(e.OldOrder.SiteOrderKey,out Order orders);
                    Console.WriteLine("Deleted: QS Count: " + qs_orders.Count());
                
                }
                if (e.OldOrder.Instrument.Product.Type == ProductType.Synthetic & e.OldOrder.Algo == null)
                {
                    SpreadDetails sp_detail = e.OldOrder.Instrument.GetSpreadDetails();
                    SpreadLegDetails sp_leg = sp_detail.GetLeg(0);

                    if (sp_leg.Instrument.Product.Type == ProductType.Future)
                    {
                        Console.WriteLine("Deleted: ASE O/R Contains Key: " + ase_or_orders.ContainsKey(e.OldOrder.SiteOrderKey));
                        //form2.richTextBox1.AppendText("\nDeleted: ASE O/R Contains Key: " + ase_or_orders.ContainsKey(e.OldOrder.SiteOrderKey));
                        ase_or_orders.TryRemove(e.OldOrder.SiteOrderKey, out Order orders);
                        Console.WriteLine("Deleted: ASE O/R Count: " + ase_or_orders.Count());
                        //form2.richTextBox1.AppendText("\nDeleted: ASE O/R Count: " + ase_or_orders.Count());
                    }
                    if (sp_leg.Instrument.Product.Type == ProductType.MultilegInstrument)
                    {
                        Console.WriteLine("Deleted: ASE QS Contains Key: " + ase_qs_orders.ContainsKey(e.OldOrder.SiteOrderKey));
                        ase_qs_orders.TryRemove(e.OldOrder.SiteOrderKey, out Order orders);
                        Console.WriteLine("Deleted: ASE QS Count: " + ase_qs_orders.Count());
                    }
            }
        }
        void m_instrumentTradeSubscription_OrderAdded(object sender, OrderAddedEventArgs e)
        {
           
           
            dataGridView1.Refresh();
            Console.WriteLine("\nOrderAdded [{0}] {1}: {2}", e.Order.SiteOrderKey, e.Order.BuySell, e.Order.ToString());
                if (e.Order.Instrument.Product.Type == ProductType.Future & !or_orders.ContainsKey(e.Order.SiteOrderKey) & ((e.Order.OrderSource != OrderSource.Ase) & (e.Order.OrderSource != OrderSource.PrimeAse)))
                {
                    or_orders.TryAdd(e.Order.SiteOrderKey, e.Order);
                    Console.WriteLine("Order Added: O/R Count: " + or_orders.Count());
                   
                }
                if (e.Order.Instrument.Product.Type == ProductType.MultilegInstrument & !qs_orders.ContainsKey(e.Order.SiteOrderKey) & ((e.Order.OrderSource != OrderSource.Ase) & (e.Order.OrderSource != OrderSource.PrimeAse)))
                {
                    qs_orders.TryAdd(e.Order.SiteOrderKey, e.Order);
                    Console.WriteLine("Order Added: QS Count: " + qs_orders.Count());
                    
                }
                if (e.Order.Instrument.Product.Type == ProductType.Synthetic & e.Order.Algo == null)
                {
                    SpreadDetails sp_detail = e.Order.Instrument.GetSpreadDetails();
                    SpreadLegDetails sp_leg = sp_detail.GetLeg(0);
                
                if (sp_leg.Instrument.Product.Type == ProductType.Future & !ase_or_orders.ContainsKey(e.Order.SiteOrderKey))
                    {
                        ase_or_orders.TryAdd(e.Order.SiteOrderKey, e.Order);
                        Console.WriteLine("Order Added: ASE O/R Count: " + ase_or_orders.Count());
                       
                    }
                    if (sp_leg.Instrument.Product.Type == ProductType.MultilegInstrument & !ase_qs_orders.ContainsKey(e.Order.SiteOrderKey))
                    {
                        ase_qs_orders.TryAdd(e.Order.SiteOrderKey, e.Order);
                        Console.WriteLine("Order Added: ASE QS Count: " + ase_qs_orders.Count());
                       
                    }
                }
            
        }
        void m_instrumentTradeSubscription_OrderUpdated(object sender, OrderUpdatedEventArgs e)
        {
            /*lock (m_OrderUpdated)
            {*/
            UpdateValueIfPresent(sr1_zq_spread, e.OldOrder.SiteOrderKey, e.NewOrder);
            UpdateValueIfPresent(sr3_zq_spread, e.OldOrder.SiteOrderKey, e.NewOrder);
            UpdateValueIfPresent(sr1_zq_naked, e.OldOrder.SiteOrderKey, e.NewOrder);
            UpdateValueIfPresent(sr3_zq_naked, e.OldOrder.SiteOrderKey, e.NewOrder);
            UpdateValueIfPresent(sr3_sr1_naked, e.OldOrder.SiteOrderKey, e.NewOrder);
            UpdateValueIfPresent(spread_ase, e.OldOrder.SiteOrderKey, e.NewOrder);
            UpdateValueIfPresent(fly_ase, e.OldOrder.SiteOrderKey, e.NewOrder);
            UpdateValueIfPresent(meeting_ase, e.OldOrder.SiteOrderKey, e.NewOrder);
            datagrid_updation(dataGridView1, e.OldOrder.SiteOrderKey, e.NewOrder);
                Console.WriteLine("\nOrderUpdated [{0}] with price={1}", e.OldOrder.SiteOrderKey, e.OldOrder.LimitPrice);

                if (e.OldOrder.Instrument.Product.Type == ProductType.Future & or_orders.ContainsKey(e.OldOrder.SiteOrderKey)
                    & ((e.OldOrder.OrderSource != OrderSource.Ase) & (e.OldOrder.OrderSource != OrderSource.PrimeAse)))
                {
                    Console.WriteLine("Updated: O/R Contains Key: " + or_orders.ContainsKey(e.OldOrder.SiteOrderKey));
                   
                    or_orders[e.OldOrder.SiteOrderKey] = e.NewOrder;
                    Console.WriteLine("Updated: O/R Count: " + or_orders.Count() + " For: " + e.OldOrder.SiteOrderKey);
                    
                }
                if (e.OldOrder.Instrument.Product.Type == ProductType.MultilegInstrument & qs_orders.ContainsKey(e.OldOrder.SiteOrderKey)
                    & ((e.OldOrder.OrderSource != OrderSource.Ase) & (e.OldOrder.OrderSource != OrderSource.PrimeAse)))
                {
                    Console.WriteLine("Updated: QS Contains Key: " + qs_orders.ContainsKey(e.OldOrder.SiteOrderKey));
                  
                    qs_orders[e.OldOrder.SiteOrderKey] = e.NewOrder;
                    Console.WriteLine("Updated: QS Count: " + qs_orders.Count() + " For: " + e.OldOrder.SiteOrderKey);
                    
                }
                if (e.OldOrder.Instrument.Product.Type == ProductType.Synthetic & e.OldOrder.Algo == null)
                {
                    SpreadDetails sp_detail = e.OldOrder.Instrument.GetSpreadDetails();
                    SpreadLegDetails sp_leg = sp_detail.GetLeg(0);

                    if (sp_leg.Instrument.Product.Type == ProductType.Future & ase_or_orders.ContainsKey(e.OldOrder.SiteOrderKey))
                    {
                        Console.WriteLine("Updated: ASE O/R Contains Key: " + ase_or_orders.ContainsKey(e.OldOrder.SiteOrderKey));
                       
                        ase_or_orders[e.OldOrder.SiteOrderKey] = e.NewOrder;
                        Console.WriteLine("Updated: ASE O/R Count: " + ase_or_orders.Count() + " For: " + e.OldOrder.SiteOrderKey);
                       
                    }
                    if (sp_leg.Instrument.Product.Type == ProductType.MultilegInstrument & ase_qs_orders.ContainsKey(e.OldOrder.SiteOrderKey))
                    {
                        Console.WriteLine("Updated: ASE QS Contains Key: " + ase_qs_orders.ContainsKey(e.OldOrder.SiteOrderKey));
                        
                        ase_qs_orders[e.OldOrder.SiteOrderKey] = e.NewOrder;
                        Console.WriteLine("Updated: ASE QS Count: " + ase_qs_orders.Count() + " For: " + e.OldOrder.SiteOrderKey);
                    }
                }
        }

        public void Dispose()
        {
            if (object.ReferenceEquals(m_instrLookupRequest, null) == false)
                m_instrLookupRequest.Dispose();

            if (object.ReferenceEquals(m_priceSubsciption, null) == false)
                m_priceSubsciption.Dispose();

            TTAPI.Shutdown();
        }

        public void TTAPI_ShutdownCompleted(object sender, EventArgs e)
        {
            Console.WriteLine("TTAPI Shutdown completed");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (m_isOrderBookDownloaded)
            {
                pause_time = comboBox1.Text + ":" + comboBox2.Text + " " + comboBox3.Text;
                resume_time = comboBox4.Text + ":" + comboBox5.Text + " " + comboBox6.Text;
                pause_date = dateTimePicker1.Value.Date;
                resume_date = dateTimePicker2.Value.Date;
                pause_play = true;
                or_pause=checkBox1.Checked;
                qs_pause = checkBox2.Checked;
                ase_or_pause=checkBox3.Checked;
                ase_qs_pause=checkBox4.Checked;
                Console.WriteLine("\n PAUSE PLAY button clicked");
                or_orders_on_hold = false;
                qs_orders_on_hold = false;
                ase_or_orders_on_hold = false;
                ase_qs_orders_on_hold = false;

            }
        }
    
        private void button3_Click(object sender, EventArgs e)
        {
            if(m_isOrderBookDownloaded)
            {
                delete_time_1 = comboBox13.Text + ":" + comboBox14.Text + " " + comboBox15.Text;
                
                delete_date_1 = dateTimePicker5.Value.Date;
               
                delete_only = true;
                or_delete_1 = checkBox9.Checked;
                qs_delete_1 = checkBox10.Checked;
                ase_or_delete_1 = checkBox11.Checked;
                ase_qs_delete_1 = checkBox12.Checked;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (m_isOrderBookDownloaded)
            {
                order_add_time = comboBox16.Text + ":" + comboBox17.Text + " " + comboBox18.Text;

                order_add_date = dateTimePicker6.Value.Date;

                file_add = true;
                or_add = checkBox13.Checked;
                qs_add = checkBox14.Checked;
                ase_or_add = checkBox15.Checked;
                ase_qs_add = checkBox16.Checked;
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (m_isOrderBookDownloaded)
            {
                delete_time = comboBox7.Text + ":" + comboBox8.Text + " " + comboBox9.Text;
                order_add_time = comboBox10.Text + ":" + comboBox11.Text + " " + comboBox12.Text;
                delete_date = dateTimePicker3.Value.Date;
                order_add_date = dateTimePicker4.Value.Date;
                delete_add = true;
                or_delete = checkBox5.Checked;
                qs_delete = checkBox6.Checked;
                ase_or_delete = checkBox7.Checked;
                ase_qs_delete = checkBox8.Checked;
                or_add = checkBox5.Checked;
                qs_add = checkBox6.Checked;
                ase_or_add = checkBox7.Checked;
                ase_qs_add = checkBox8.Checked;
                Console.WriteLine("\n dlete add button clicked");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
          selected_ase = comboBox19.Text;
            ConcurrentDictionary<string, Order> dict = Dictionary_finder(selected_ase);
                dataGridView1.Rows.Clear();
                dataGridView1.DataSource = null;
                foreach (var kvp in dict)
                {
                    dataGridView1.Rows.Add(kvp.Key,kvp.Value.Instrument,kvp.Value.BuySell,kvp.Value.LimitPrice,kvp.Value.OrderQuantity,kvp.Value.OrderQuantity-kvp.Value.FillQuantity,kvp.Value.DisclosedQuantity, false,0, "", "Inactive", kvp.Value.ExchTransactionTime);
                    
                }
             
            dgstatus();
        }
       
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
           
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Update" & dataGridView1.Rows[e.RowIndex].Cells[6].Value!=null)
            {
                Console.WriteLine(Convert.ToBoolean(dataGridView1.Rows[e.RowIndex].Cells[7].Value));
                ConcurrentDictionary<string, Order> dict = Dictionary_finder(selected_ase);
                
                    if (   dataGridView1.Rows[e.RowIndex].Cells[6].Value.ToString() != dict[dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString()].DisclosedQuantity.ToString() &  
                         Convert.ToDecimal( dataGridView1.Rows[e.RowIndex].Cells[6].Value.ToString())  <= Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells[5].Value.ToString()))
                    
                    {
                    if (Convert.ToString(dataGridView1.Rows[e.RowIndex].Cells[10].Value) == "Inactive")
                    {
                        if (MessageBox.Show("Are you sure want to upadte the Dislclosed Quantity ?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            Console.WriteLine("yess");
                            String new_key = Order_Action_Functions.Change_Order_Reload(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString(), dataGridView1.Rows[e.RowIndex].Cells[6].Value.ToString(), m_instrumentTradeSubscription, m_accounts, account_idx);

                        }
                    }
                    else
                    {
                        MessageBox.Show("You cant change dislosed Qunatity for this row!!!!!", "Already Active");
                    }
                    }
                    else if (  Convert.ToBoolean(dataGridView1.Rows[e.RowIndex].Cells[7].Value) & 
                         Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells[8].Value) != 0 &
                         Convert.ToString(dataGridView1.Rows[e.RowIndex].Cells[10].Value) == "Inactive"& 
                         Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells[8].Value.ToString()) + dict[dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString()].DisclosedQuantity.ToDecimal() <= Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells[5].Value.ToString())- desired_number)
                    {
                        Console.WriteLine(Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells[8].Value));
                        if (MessageBox.Show("Are you sure want to upadte the Dislclosed Quantity on basis of fill?", "Fill Updater", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            Console.WriteLine("yess tewo");
                            var cancellation_token = new CancellationTokenSource();
                        m_disp_1 = Dispatcher.Current;
                        Task.Run(() =>
                        {
                            m_Fs = new Fill_Subscription(m_disp_1, m_api, dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString(), m_instrumentTradeSubscription, cancellation_token.Token, Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells[8].Value));
                            m_Fs.FillSubWorkingEventHandler_1 += new FillSubWorkingEventHandler(fill_on_data);
                            m_Fs.Start();

                        });
                        cts[dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString()] = cancellation_token;
                            dataGridView1.Rows[e.RowIndex].Cells[10].Style.BackColor = Color.Green;
                            dataGridView1.Rows[e.RowIndex].Cells[10].Value = "Active";
                            dataGridView1.Rows[e.RowIndex].Cells[10].Style.ForeColor = Color.White;
                            ase_fill.TryAdd(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString(), "Active");
                        }
                    }
                    else if (Convert.ToBoolean(dataGridView1.Rows[e.RowIndex].Cells[7].Value) & Convert.ToString(dataGridView1.Rows[e.RowIndex].Cells[10].Value) == "Active")
                    {
                        MessageBox.Show("Fill Subscription Checker for this row is already active !!", "Already Active");
                    }
                    else if (!Convert.ToBoolean(dataGridView1.Rows[e.RowIndex].Cells[7].Value) & Convert.ToString(dataGridView1.Rows[e.RowIndex].Cells[10].Value) == "Active")
                    {
                        if (MessageBox.Show("Are you sure want to stop the fill checker?", "Fill Updater Stop?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            Console.WriteLine("stopping");
                            dataGridView1.Rows[e.RowIndex].Cells[10].Style.BackColor = Color.White;
                            dataGridView1.Rows[e.RowIndex].Cells[10].Value = "Inactive";
                            dataGridView1.Rows[e.RowIndex].Cells[10].Style.ForeColor = Color.Black;
                        if (cts.TryGetValue(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString(), out var cts_1))
                        {
                            cts_1.Cancel();
                        }
                        
                        cts.TryRemove(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString(), out var ctss_1);
                            ase_fill.TryRemove(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString(), out string x);
                        }
                    }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure want to upadte the Dislclosed Quantity of all ases ?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Console.WriteLine("yess");
                lock (m_dis)
                {
                    ConcurrentDictionary<string, Order> dict = Dictionary_finder(selected_ase); // new Dictionary<string, Order>();        
                    int counter=0;
                    for (int i = 0; i < dataGridView1.RowCount - 1; i++)
                    {
                        if (numericUpDown1.Value != dict[dataGridView1.Rows[i].Cells[0].Value.ToString()].DisclosedQuantity.ToDecimal() &
                             numericUpDown1.Value+ dict[dataGridView1.Rows[i].Cells[0].Value.ToString()].DisclosedQuantity.ToDecimal()<= Convert.ToDecimal(dataGridView1.Rows[i].Cells[5].Value.ToString()))
                        {
                            if (dataGridView1.Rows[i].Cells[10].Value.ToString() == "Inactive")
                            {
                                Order_Action_Functions.Change_Order_Reload(dataGridView1.Rows[i].Cells[0].Value.ToString(), numericUpDown1.Value.ToString(), m_instrumentTradeSubscription, m_accounts, account_idx);
                                counter++;
                            }
                        }

                    }
                    if (counter == 0)
                    {
                        MessageBox.Show("Changing Disclosed Quantity failed for all rows", "Failed");
                    }
                }
            }
        }
        private void button8_Click(object sender, EventArgs e)
        {
            if (!checkBox17.Checked)
            {

                if (MessageBox.Show("Are you sure want to stop the fill checker for all the ases ?", "Fill Updater Stop?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    for (int i = 0; i < dataGridView1.RowCount-1; i++)
                    {
                        if (Convert.ToBoolean(dataGridView1.Rows[i].Cells[7].Value) & Convert.ToString(dataGridView1.Rows[i].Cells[10].Value) == "Active")
                        {
                            dataGridView1.Rows[i].Cells[8].Value = 0;
                            dataGridView1.Rows[i].Cells[7].Value = false;
                            dataGridView1.Rows[i].Cells[10].Style.BackColor = Color.White;
                            dataGridView1.Rows[i].Cells[10].Value = "Inactive";
                            dataGridView1.Rows[i].Cells[10].Style.ForeColor = Color.Black;
                            if (cts.TryGetValue(dataGridView1.Rows[i].Cells[0].Value.ToString(), out var cts_1))
                            {
                                cts_1.Cancel();
                            }
                            cts.TryRemove(dataGridView1.Rows[i].Cells[0].Value.ToString(), out var ctss_1);
                            ase_fill.TryRemove(dataGridView1.Rows[i].Cells[0].Value.ToString(), out string x);
                            dgstatus();
                        }
                       
                    }
                }
            }
            else
            {
                if (MessageBox.Show("Are you sure want to upadte the Dislclosed Quantity on basis of fill for all ases?", "Fill Updater", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    int counter = 0;
                    for (int i = 0; i < dataGridView1.RowCount - 1; i++)
                    {
                        
                        if (!Convert.ToBoolean(dataGridView1.Rows[i].Cells[7].Value) &
                             numericUpDown2.Value != 0 &
                             Convert.ToString(dataGridView1.Rows[i].Cells[10].Value) != "Active" &
                               numericUpDown2.Value + Convert.ToDecimal(dataGridView1.Rows[i].Cells[6].Value.ToString()) <= Convert.ToDecimal(dataGridView1.Rows[i].Cells[5].Value.ToString()) - desired_number
                               )

                        {
                            dataGridView1.Rows[i].Cells[8].Value = numericUpDown2.Value;
                            String key = dataGridView1.Rows[i].Cells[0].Value.ToString();
                            Console.WriteLine(i);
                            Console.WriteLine(dataGridView1.RowCount);
                            var cancellation_token = new CancellationTokenSource();
                            m_disp_1 = Dispatcher.Current;
                            Task.Run(() =>
                            {
                                m_Fs = new Fill_Subscription(m_disp_1, m_api, key, m_instrumentTradeSubscription, cancellation_token.Token, Convert.ToInt32(numericUpDown2.Value));
                                m_Fs.FillSubWorkingEventHandler_1 += new FillSubWorkingEventHandler(fill_on_data);
                                m_Fs.Start();
                            });

                            cts[dataGridView1.Rows[i].Cells[0].Value.ToString()] = cancellation_token;


                            dataGridView1.Rows[i].Cells[10].Style.BackColor = Color.Green;
                            dataGridView1.Rows[i].Cells[10].Value = "Active";
                            dataGridView1.Rows[i].Cells[10].Style.ForeColor = Color.White;
                            ase_fill.TryAdd(dataGridView1.Rows[i].Cells[0].Value.ToString(), "Active");
                            dgstatus();
                            counter++;
                        }

                        

                    }
                    if (counter == 0)
                    {
                        MessageBox.Show("Changing Disclosed Quantity on basis of fill failed for all rows", "Failed");
                    }
                }
            }

        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Console.WriteLine("In form closing");

            Dispose();

        }
    }
}
