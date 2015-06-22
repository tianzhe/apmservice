using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using apmservice.Contracts;
using System.Timers;
using Microsoft.Win32;
using System.Reflection;
using apmservice.Events;

namespace apmservice
{
    public partial class apmservice : ServiceBase
    {
        public apmservice()
        {
            InitializeComponent();
        }

        /*
         * Load all model dlls
         * Sync the latest stock index data
         * Sync the latest individual stock trade data
         * Fire the event, and callback to pre-registered model calculation function
         */
        protected override void OnStart(string[] args)
        {
//#if(DEBUG)
//            Debugger.Launch();
//#endif
            // Read registry key and values
            RegistryKey key = null;
            string imagePath = string.Empty;
            try
            {
                key = Registry.LocalMachine.OpenSubKey(
                    "SYSTEM\\CurrentControlSet\\Services\\Active Portfolio Management");
                
                _token = key.GetValue("DataYesToken").ToString();
                _baseUrl = key.GetValue("DataYesBaseUrl").ToString();
                _syncmktIdxDataInterval = UInt64.Parse(key.GetValue("SyncMarketIndexDataInterval").ToString());
                _syncTradeDataInterval = UInt64.Parse(key.GetValue("SyncTradeDataInterval").ToString());
                _defaultStartDate = DateTime.Parse(key.GetValue("DefaultQueryStartDate").ToString());
                _providers = (string[])key.GetValue("ProviderNames");
                imagePath = key.GetValue("ImagePath").ToString().TrimEnd('"').TrimStart('"');
            }
            catch(Exception ex)
            {
                eventLog1.WriteEntry(
                    string.Format(
                        "Unexpected error encountered when attempting to read the registry @ {0}.\nDetails = {1}", 
                        "HKLM\\SYSTEM\\CurrentControlSet\\Services\\Active Portfolio Management", ex.Message), 
                        EventLogEntryType.Error);
                if(key != null)
                {
                    key.Close();
                }
                throw;
            }
            
            // Initialize
            Util.Initialize(_token, _baseUrl, eventLog1);

            // Set timer for market index data synchronization
            Thread syncMktIdxDataThread = new Thread(new ThreadStart(KickOffSyncMktIdxData));
            syncMktIdxDataThread.Start();
            
            // Set timer for trade data synchronization
            Thread syncTradeDataThread = new Thread(new ThreadStart(KickOffSyncTradeData));
            syncTradeDataThread.Start();

            // Load module dlls
            if(_providers.Count() > 0)
            {
                foreach(var provider in _providers.Where(a => !string.IsNullOrEmpty(a)))
                {
                    RegistryKey subKey = null;
                    try
                    {
                        subKey = key.OpenSubKey(provider, true);
                        if (subKey != null)
                        {
                            var IsCritical = UInt32.Parse(subKey.GetValue("IsCritical").ToString());
                            if (IsCritical > 0)
                            {
                                var modulePath = subKey.GetValue("ModulePath").ToString();
                                modulePath = modulePath.TrimStart('\"').TrimEnd('\"');

                                // Initialize
                                Assembly assembly = Assembly.LoadFile(modulePath);
                                var nameSpace = provider;
                                var className = "Model";
                                var fqdn = nameSpace + '.' + className;
                                var types = assembly.GetTypes().ToList();
                                var type = types.Where(t => t.Name.Equals("Model", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                                var method = type.GetMethod("Initialize");
                                Object obj = Activator.CreateInstance(type);
                                method.Invoke(obj, new object[]{subKey, eventLog1});

                                // Register the callback
                                var domain = AppDomain.CreateDomain(provider);
                                var instance = (Proxy)domain.CreateInstanceFromAndUnwrap(imagePath, "apmservice.Proxy");
                                instance.LoadAssemlyFromFile(modulePath);
                                GeneratePortfolio += instance.Invoke;
                                domains.Add(domain);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        eventLog1.WriteEntry(
                            string.Format(
                                "Unexpected error encountered when attempting to load the module dlls {0}.\nDetails = {1}",
                                provider, ex.Message),
                                EventLogEntryType.Error);
                        throw;
                    }
                    finally
                    {
                        if(subKey != null)
                        {
                            subKey.Close();
                        }

                        if(key != null)
                        {
                            key.Close();
                        }
                    }
                }
            }

            eventLog1.WriteEntry("Active Portfolio Management Service Started!", EventLogEntryType.Information);
        }

        private void KickOffSyncMktIdxData()
        {
            _syncMktIdxDataTimer = new System.Timers.Timer(_syncmktIdxDataInterval);
            _syncMktIdxDataTimer.Elapsed += _syncMktIdxDataTimer_Elapsed;
            _syncMktIdxDataTimer.Start();
        }

        private void KickOffSyncTradeData()
        {
            _syncTradeDataTimer = new System.Timers.Timer(_syncTradeDataInterval);
            _syncTradeDataTimer.Elapsed += _syncTradeDataTimer_Elapsed;
            _syncTradeDataTimer.Start();
        }

        private void _syncMktIdxDataTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_mktIdxDatalock)
            {
                var _astock = new astockEntities();
                foreach(var idx in MarketIndexType)
                {
                    var existingData =
                        _astock.STK_MKT_IndexDaily
                        .Where(a => a.IndexID.Equals(idx, StringComparison.InvariantCultureIgnoreCase))
                        .Select(a => a.TradingDate2).OrderByDescending(a => a).ToList();

                    if (existingData.Count != 0 && ((DateTime)existingData.ElementAt(0)).Date >= DateTime.Now.Date)
                    {
                        continue;
                    }

                    DateTime nextDay = 
                        existingData.Count == 0 ?
                            _defaultStartDate :
                            ((DateTime)existingData.ElementAt(0)).AddDays(1);

                    if(!Util.SyncMarketIndexDailyData(idx, nextDay, null))
                    {
                        continue;
                    }
                }

                ModelGenerateEvent.ModelGenerateEventArgs args =
                    new ModelGenerateEvent.ModelGenerateEventArgs
                    {
                        MethodName = "Generate",
                        ClassName = "Model",
                        InvokeArgs = null
                    };

                GeneratePortfolio.BeginInvoke(this, args, null, null);
            }
        }

        private void _syncTradeDataTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock(_tradeDataLock)
            {
                var _astock = new astockEntities();
                var firms = _astock.TRD_Co.Select(a => a.StockId).OrderBy(a => a).ToList();
                foreach (var firm in firms)
                {
                    // Get Secure ID
                    string secureId = Util.GetSecureId(firm);
                    if(string.IsNullOrEmpty(secureId))
                    {
                        continue;
                    }
                    
                    // Get trade data
                    var existingData = _astock.STK_MKT_TradeDaily
                        .Where(a => a.StockID.Equals(firm, StringComparison.InvariantCultureIgnoreCase))
                        .Select(a => a.TradeDate2).OrderByDescending(a => a).ToList();

                    if (existingData.Count != 0 && ((DateTime)existingData.ElementAt(0)).Date >= DateTime.Now.Date)
                    {
                        continue;
                    }

                    DateTime nextDay = existingData.Count == 0 ? 
                                            _defaultStartDate : 
                                            ((DateTime)existingData.ElementAt(0)).AddDays(1);

                    if(!Util.SyncStockTradeDailyData(secureId, firm, nextDay, null))
                    {
                        continue;
                    }
                }
            }
        }

        protected override void OnStop()
        {
            foreach(var domain in domains)
            {
                AppDomain.Unload(domain);
            }
            eventLog1.WriteEntry("Active Portfolio Management Service Stopped!", EventLogEntryType.Information);
        }

        private static System.Timers.Timer _syncMktIdxDataTimer;
        private static System.Timers.Timer _syncTradeDataTimer;

        private string _token;
        private string _baseUrl;
        private UInt64 _syncmktIdxDataInterval;
        private UInt64 _syncTradeDataInterval;
        private DateTime _defaultStartDate;
        private string[] _providers;
        private event ModelGenerateEvent.ModelGenerateEventHandler GeneratePortfolio;
        private static List<AppDomain> domains = new List<AppDomain>();
        private static object _mktIdxDatalock = new object();
        private static object _tradeDataLock = new object();

        private static string[] MarketIndexType = 
            new string[] 
            { 
                "000002",   // Shanghai A Stock Index
                "000003",   // Shanghai B Stock Index
                "399005",   // Consolidated MSB Stock Index
                "399006",   // Consolidated GEMB Stock Index
                "399107",   // Consolidated Shenzhen A Stock Index
                "399108"    // Consolidated Shenzhen B Stock Index
            };

        //private static Dictionary<string, double> StockId2MarketTypeMap = new Dictionary<string, double>()
        //{
        //    {"000", 4},
        //    {"200", 8},
        //    {"6", 1},
        //    {"900", 2},
        //    {"002", 4},
        //    {"300", 16}
        //};
    }
}
