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
                _syncmktIdxDataInterval = TimeSpan.Parse(key.GetValue("SyncMarketIndexDataInterval").ToString()).TotalMilliseconds;
                _syncTradeDataInterval = TimeSpan.Parse(key.GetValue("SyncTradeDataInterval").ToString()).TotalMilliseconds;
                _syncSectorConstituentStocksInterval = TimeSpan.Parse(key.GetValue("SyncSectorConstituentStocksInterval").ToString()).TotalMilliseconds;
                _genPortfolioInterval = TimeSpan.Parse(key.GetValue("GeneratePortfolioInterval").ToString()).TotalMilliseconds;
                _defaultStartDate = DateTime.Parse(key.GetValue("DefaultQueryStartDate").ToString());
                _providers = (string[])key.GetValue("ProviderNames");
                imagePath = key.GetValue("ImagePath").ToString().TrimEnd('\"').TrimStart('\"');
                _isCreateSyncDailyTradeDataJob = Int32.Parse(key.GetValue("IsCreateSyncMarketIndexDataJob").ToString()) == 1;
                _isCreateSyncMarketIndexDataJob = Int32.Parse(key.GetValue("IsCreateSyncDailyTradeDateJob").ToString()) == 1;
                _isCreateSyncSectorConstituentStocksJob = Int32.Parse(key.GetValue("IsCreateSyncSectorConstituentStocksJob").ToString()) == 1;
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
                                if(assembly == null)
                                {
                                    throw new ApplicationException(
                                        string.Format("Unable to load the assembly from the path {0}", modulePath));
                                }
                                var types = assembly.GetTypes().ToList();
                                if(types.Count == 0)
                                {
                                    throw new ApplicationException(
                                        string.Format("Unable to get the types from the assembly {0}", assembly.FullName));
                                }
                                var type = types
                                    .Where(t => t.Name.Equals("Model", StringComparison.InvariantCultureIgnoreCase))
                                    .FirstOrDefault();
                                if(type == null)
                                {
                                    throw new ApplicationException(
                                        string.Format("Unable to get the type {0} from the assembly {1}", 
                                        "Model", assembly.FullName));
                                }
                                var method = type.GetMethod("Initialize");
                                if(method == null)
                                {
                                    throw new ApplicationException(
                                        string.Format("Unable to get the method {0} from the assembly {1}", 
                                        "Initialize", assembly.FullName));
                                }
                                Object obj = Activator.CreateInstance(type);
                                method.Invoke(obj, new object[]{subKey.Name});

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

                    // Set timer for market index data synchronization
                    if(_isCreateSyncMarketIndexDataJob)
                    {
                        Thread syncMktIdxDataThread = new Thread(new ThreadStart(KickOffSyncMktIdxData));
                        syncMktIdxDataThread.Start();
                    }

                    // Set timer for trade data synchronization
                    if(_isCreateSyncDailyTradeDataJob)
                    {
                        Thread syncTradeDataThread = new Thread(new ThreadStart(KickOffSyncTradeData));
                        syncTradeDataThread.Start();
                    }

                    // Set timer for sector constituent stocks synchronization
                    if(_isCreateSyncSectorConstituentStocksJob)
                    {
                        Thread syncSectorConstituentStocksThread = new Thread(new ThreadStart(KickOffSyncSectorConstituentStocks));
                        syncSectorConstituentStocksThread.Start();
                    }

                    // Set timer for generate portfolio
                    Thread genPortfolioThread = new Thread(new ThreadStart(KickOffGenPortfolio));
                    genPortfolioThread.Start();
                }
            }

            eventLog1.WriteEntry("Active Portfolio Management Service Started!", EventLogEntryType.Information);
        }

        private void KickOffGenPortfolio()
        {
            _genPortfolioTimer = new System.Timers.Timer(_genPortfolioInterval);
            _genPortfolioTimer.Elapsed += _genPortfolioTimer_Elapsed;
            _genPortfolioTimer.Start();
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

        private void KickOffSyncSectorConstituentStocks()
        {
            _syncSectorConstituentStocksTimer = new System.Timers.Timer(_syncSectorConstituentStocksInterval);
            _syncSectorConstituentStocksTimer.Elapsed += _syncSectorConstituentStocksTimer_Elapsed;
            _syncSectorConstituentStocksTimer.Start();
        }

        private void _genPortfolioTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_genPortfolioLock)
            {
                if(
                    (_isCreateSyncMarketIndexDataJob && !_isExistingMarketIndexDataSyncJob) ||
                    (_isCreateSyncDailyTradeDataJob && !_isExistingDailyTradeDataSyncJob) ||
                    (_isCreateSyncSectorConstituentStocksJob && !_isExistingSectorConstituentStocksSyncJob) ||
                    _isExistingGenPortfolioJob)
                {
                    return;
                }

                _isExistingGenPortfolioJob = true;

                var continuation = Task.WhenAll(tasks);
                continuation.Wait();

                if(continuation.Status == TaskStatus.RanToCompletion)
                {
                    ModelGenerateEvent.ModelGenerateEventArgs args =
                        new ModelGenerateEvent.ModelGenerateEventArgs
                        {
                            MethodName = "Generate",
                            ClassName = "Model",
                            InvokeArgs = null
                        };

                    try
                    {
                        GeneratePortfolio.Invoke(this, args);
                    }
                    catch(Exception ex)
                    {
                        eventLog1.WriteEntry(
                            string.Format(
                            "Unexpected error encountered when attempting to call method '{0}' in class '{1}' in assembly '{2}'.\nDetails = {3}",
                            args.MethodName, args.ClassName, GeneratePortfolio.Target.ToString(), ex.Message),
                            EventLogEntryType.Error);
                    }
                    finally
                    {
                        tasks.RemoveAll(a => a.Status == TaskStatus.RanToCompletion);

                        _isExistingGenPortfolioJob = false;
                        _isExistingMarketIndexDataSyncJob = false;
                        _isExistingDailyTradeDataSyncJob = false;
                        _isExistingSectorConstituentStocksSyncJob = false;
                    }
                }
            }
        }

        private void _syncMktIdxDataTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_mktIdxDatalock)
            {
                if (!_isExistingMarketIndexDataSyncJob)
                {
                    tasks.Add(Task.Factory.StartNew(CreateMktIndexDataSyncTask));
                    _isExistingMarketIndexDataSyncJob = true;
                }
            }
        }

        private void _syncTradeDataTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_tradeDataLock)
            {
                if (_isExistingDailyTradeDataSyncJob)
                {
                    tasks.Add(Task.Factory.StartNew(CreateDailyTradeDataSyncTask));
                    _isExistingDailyTradeDataSyncJob = true;
                }
            }
        }

        private void _syncSectorConstituentStocksTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_sectorConstituentStocksLock)
            {
                if (!_isExistingSectorConstituentStocksSyncJob)
                {
                    tasks.Add(Task.Factory.StartNew(CreateSectorConstituentStocksSyncTask));
                    _isExistingSectorConstituentStocksSyncJob = true;
                }
            }
        }

        private void CreateSectorConstituentStocksSyncTask()
        {
            var _astock = new astockEntities();

            var sectors = _astock.TRD_Sector.Select(s => s.SectorTypeId).ToList();

            foreach(var sector in sectors)
            {
                var existingData =
                    _astock.STK_MKT_SectorDaily
                    .Where(a => a.SectorTypeId.Equals(sector, StringComparison.InvariantCultureIgnoreCase))
                    .Select(a => a.TradeDate).OrderByDescending(a => a).ToList();

                if (existingData.Count != 0 && ((DateTime)existingData.ElementAt(0)).Date >= DateTime.Now.Date)
                {
                    continue;
                }

                DateTime nextDay =
                    existingData.Count == 0 ?
                        _defaultStartDate :
                        ((DateTime)existingData.ElementAt(0)).AddDays(1);

                if (!Util.SyncSectorDailyData(sector, _astock, nextDay, null))
                {
                    continue;
                }
            }

            var firms = _astock.TRD_Co.Select(a => new { a.StockId, a.SectorTypeId }).ToList();

            foreach(var firm in firms)
            {
                var existingData =
                    _astock.STK_MKT_RiskFactorDaily
                    .Where(a => a.Symbol.Equals(firm.StockId, StringComparison.InvariantCultureIgnoreCase) && a.SectorBeta1 != null && a.SectorBeta2 != null)
                    .Select(a => a.TradingDate2).OrderByDescending(a => a).ToList();

                if (existingData.Count != 0 && ((DateTime)existingData.ElementAt(0)).Date >= DateTime.Now.Date)
                {
                    continue;
                }

                DateTime nextDay =
                    existingData.Count == 0 ?
                        _defaultStartDate :
                        ((DateTime)existingData.ElementAt(0)).AddDays(1);

                if (!Util.CalculateSectorBetaPerStock(firm.StockId, firm.SectorTypeId, _astock, nextDay, null))
                {
                    continue;
                }
            }
        }

        private void CreateMktIndexDataSyncTask()
        {
            var _astock = new astockEntities();

            foreach (var idx in MarketIndexType)
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

                if (!Util.SyncMarketIndexDailyData(idx, _astock, nextDay, null))
                {
                    continue;
                }
            }
        }

        private void CreateDailyTradeDataSyncTask()
        {
            var _astock = new astockEntities();

            var firms = _astock.TRD_Co.Select(a => a.StockId).OrderBy(a => a).ToList();
            foreach (var firm in firms)
            {
                // Get Secure ID
                string secureId = Util.GetSecureId(firm);
                if (string.IsNullOrEmpty(secureId))
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

                if (!Util.SyncStockTradeDailyData(secureId, firm, _astock, nextDay, null))
                {
                    continue;
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
        private static System.Timers.Timer _syncSectorConstituentStocksTimer;
        private static System.Timers.Timer _genPortfolioTimer;

        private string _token;
        private string _baseUrl;
        private double _syncmktIdxDataInterval;
        private double _syncTradeDataInterval;
        private double _genPortfolioInterval;
        private double _syncSectorConstituentStocksInterval;
        private DateTime _defaultStartDate;
        private string[] _providers;
        private event ModelGenerateEvent.ModelGenerateEventHandler GeneratePortfolio;
        private static List<AppDomain> domains = new List<AppDomain>();
        private static List<Task> tasks = new List<Task>();
        private static bool _isExistingMarketIndexDataSyncJob = false;
        private static bool _isExistingDailyTradeDataSyncJob = false;
        private static bool _isExistingSectorConstituentStocksSyncJob = false;
        private static bool _isExistingGenPortfolioJob = false;
        private static bool _isCreateSyncMarketIndexDataJob = false;
        private static bool _isCreateSyncDailyTradeDataJob = false;
        private static bool _isCreateSyncSectorConstituentStocksJob = true;
        private static object _mktIdxDatalock = new object();
        private static object _tradeDataLock = new object();
        private static object _sectorConstituentStocksLock = new object();
        private static object _genPortfolioLock = new object();

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
