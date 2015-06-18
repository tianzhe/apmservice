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
            // Read registry key and values
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(
                    "SYSTEM\\CurrentControlSet\\Services\\Active Portfolio Management");
                
                _token = key.GetValue("DataYesToken").ToString();
                _baseUrl = key.GetValue("DataYesBaseUrl").ToString();
                _syncmktIdxDataInterval = UInt64.Parse(key.GetValue("SyncMarketIndexDataInterval").ToString());
                _syncTradeDataInterval = UInt64.Parse(key.GetValue("SyncTradeDataInterval").ToString());
            }
            catch(Exception ex)
            {
                eventLog1.WriteEntry(
                    string.Format(
                        "Unexpected error encountered when attempting to read the registry @ {0}.\nDetails = {1}", 
                        "HKLM\\SYSTEM\\CurrentControlSet\\Services\\Active Portfolio Management", ex.Message), 
                        EventLogEntryType.Error);
                throw;
            }
            _astock = new astockEntities();

            Thread syncMktIdxDataThread = new Thread(new ThreadStart(KickOffSyncMktIdxData));
            syncMktIdxDataThread.Start();
            //_syncMktIdxDataTimer = new System.Timers.Timer(_syncmktIdxDataInterval);
            //_syncMktIdxDataTimer.Elapsed += _syncMktIdxDataTimer_Elapsed;
            //_syncMktIdxDataTimer.Start();

            Thread syncTradeDataThread = new Thread(new ThreadStart(KickOffSyncTradeData));
            syncTradeDataThread.Start();

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
            }
        }

        private void _syncTradeDataTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock(_tradeDataLock)
            {
                Util.Initialize(_token, _baseUrl, eventLog1);
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
            eventLog1.WriteEntry("Active Portfolio Management Service Stopped!", EventLogEntryType.Information);
        }

        private static astockEntities _astock;
        private static System.Timers.Timer _syncMktIdxDataTimer;
        private static System.Timers.Timer _syncTradeDataTimer;

        private string _token;
        private string _baseUrl;
        private UInt64 _syncmktIdxDataInterval;
        private UInt64 _syncTradeDataInterval;
        private static object _mktIdxDatalock = new object();
        private static object _tradeDataLock = new object();
        private DateTime _defaultStartDate = DateTime.Parse("2012-01-01");

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
