﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apmservice.Contracts;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;

namespace apmservice
{
    static class Util
    {
        private static string _token = string.Empty;
        private static string _baseUrl = string.Empty;
        private static EventLog _eventLog = null;
        private static astockEntities _astock = new astockEntities();

        public enum ReturnRateType
        {
            REINVEST = 0,
            NON_REINVEST
        }

        public static void Initialize(string token, string baseUrl, EventLog eventlog)
        {
            _token = token;
            _baseUrl = baseUrl;
            _eventLog = eventlog;
        }

        private static double GetMarketType(string stockId)
        {
            if (stockId.StartsWith("6"))
            {
                return 1;
            }
            else if (stockId.StartsWith("900"))
            {
                return 2;
            }
            else if (stockId.StartsWith("002") || stockId.StartsWith("000"))
            {
                return 4;
            }
            else if (stockId.StartsWith("200"))
            {
                return 8;
            }
            else
            {
                return 16;
            }
        }

        public static Dictionary<DateTime, double> GetReturnRate(string secureId, string stockId, DateTime startDate, DateTime? endDate, ReturnRateType type)
        {
            if (endDate != null && startDate >= endDate)
            {
                return null;
            }

            var payload = 
                endDate == null ?
                    new StockDailyReturnRequest
                    {
                        SecureId = secureId,
                        QueryStartDate = startDate.ToString("yyyyMMdd")
                    } :
                    new StockDailyReturnRequest
                    {
                        SecureId = secureId,
                        QueryStartDate = startDate.ToString("yyyyMMdd"),
                        QueryEndDate = ((DateTime)endDate).ToString("yyyyMMdd")
                    };

            try 
            {
                var request = endDate == null ?
                    (HttpWebRequest)WebRequest.Create(
                        string.Format(
                            "{0}/ap/api/equity/getEquRetud.json?field=&ticker=000001&secID=&beginDate=&endDate=&dailyReturnNoReinvLower=&dailyReturnNoReinvUpper=&dailyReturnReinvLower=&dailyReturnReinvUpper=&isChgPctl=&listStatusCD=",
                            _baseUrl, payload.SecureId, payload.QueryStartDate)) :
                    (HttpWebRequest)WebRequest.Create(
                        string.Format(
                            "{0}/api/market/getMktEqud.json?secID={1}&beginDate={2}&endDate={3}",
                            _baseUrl, payload.SecureId, payload.QueryStartDate, payload.QueryEndDate));

                request.Method = "GET";
                request.Headers.Add("Authorization: Bearer " + _token);
                var response = (HttpWebResponse)request.GetResponse();
                StockDailyReturnResponse jsonResponse;

                using (Stream stream = response.GetResponseStream())
                {
                    if (stream == null)
                    {
                        return null;
                    }

                    Encoding encoding = string.IsNullOrEmpty(response.CharacterSet) ?
                        Encoding.Default : Encoding.GetEncoding(response.CharacterSet);

                    using (var reader = new StreamReader(stream, encoding))
                    {
                        var result = reader.ReadToEnd();
                        jsonResponse = JsonConvert.DeserializeObject<StockDailyReturnResponse>(result);
                        if (jsonResponse.ReturnCode != 1)
                        {
                            var error = JsonConvert.DeserializeObject<ErrorResponse>(result);
                            _eventLog.WriteEntry(
                                string.Format(
                                    "Unexpected error encountered when attempting to get return rate for {0}.\nError = {1}",
                                    stockId, error.ReturnMessage), EventLogEntryType.Error);

                            return null;
                        }
                    }
                }

                Dictionary<DateTime, double> ret = new Dictionary<DateTime, double>();
                switch(type)
                {
                    case ReturnRateType.NON_REINVEST:
                        foreach(var rate in jsonResponse.Payload)
                        {
                            ret.Add();
                        }
                        return ret;

                    case ReturnRateType.REINVEST:
                        foreach(var rate in jsonResponse.Payload)
                        {
                            ret.Add();
                        }
                        return ret;

                    default:
                        _eventLog.WriteEntry(
                            string.Format(
                                "Unrecognized return rate type {0}", type), 
                            EventLogEntryType.Error);

                        return null;
                }
            }
            catch(Exception ex)
            {
                _eventLog.WriteEntry(
                    string.Format(
                        "Unexpected error occurred.\nDetails = {0}\nStack Trace = {1}",
                        ex.Message, ex.StackTrace), EventLogEntryType.Error);

                if (ex.InnerException != null)
                {
                    _eventLog.WriteEntry(
                        string.Format(
                            "Inner Exception Message = {0}\nStack Trace = {1}",
                            ex.InnerException.Message, ex.InnerException.StackTrace),
                            EventLogEntryType.Error);
                }

                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="secureId"></param>
        /// <param name="stockId"></param>
        /// <param name="startDate">The start date, included</param>
        /// <param name="endDate">The end date, included, if it's null, then it means the latest</param>
        /// <returns></returns>
        public static bool SyncStockTradeDailyData(string secureId, string stockId, DateTime startDate, DateTime? endDate)
        {
            if(endDate != null && startDate >= endDate)
            {
                return false;
            }

            try
            {
                DateTime previousDay = startDate.AddDays(-1);
                TradeDailyResponse jsonResponse = null;

                var payload = endDate == null ?
                        new TradeDailyRequest
                        {
                            SecureId = secureId,
                            Field = string.Empty,
                            QueryStartDate = startDate.ToString("yyyyMMdd")
                        } :
                        new TradeDailyRequest
                        {
                            SecureId = secureId,
                            Field = string.Empty,
                            QueryStartDate = startDate.ToString("yyyyMMdd"),
                            QueryEndDate = ((DateTime)endDate).ToString("yyyyMMdd")
                        };

                var request = endDate == null ?
                    (HttpWebRequest)WebRequest.Create(
                        string.Format(
                            "{0}/api/market/getMktEqud.json?secID={1}&beginDate={2}&field={3}",
                            _baseUrl, payload.SecureId, payload.QueryStartDate, payload.Field)) :
                    (HttpWebRequest)WebRequest.Create(
                        string.Format(
                            "{0}/api/market/getMktEqud.json?secID={1}&beginDate={2}&endDate={3}&field={4}",
                            _baseUrl, payload.SecureId, payload.QueryStartDate, payload.QueryEndDate, payload.Field));

                request.Method = "GET";
                request.Headers.Add("Authorization: Bearer " + _token);
                var response = (HttpWebResponse)request.GetResponse();

                using (Stream stream = response.GetResponseStream())
                {
                    if(stream == null)
                    {
                        return false;
                    }

                    Encoding encoding = string.IsNullOrEmpty(response.CharacterSet) ?
                        Encoding.Default : Encoding.GetEncoding(response.CharacterSet);

                    using (var reader = new StreamReader(stream, encoding))
                    {
                        var result = reader.ReadToEnd();
                        jsonResponse = JsonConvert.DeserializeObject<TradeDailyResponse>(result);
                        if (jsonResponse.ReturnCode != 1)
                        {
                            var error = JsonConvert.DeserializeObject<ErrorResponse>(result);
                            _eventLog.WriteEntry(
                                string.Format(
                                    "Unexpected error encountered when attempting to get the stock trade data for {0}.\nError = {1}",
                                    stockId, error.ReturnMessage), EventLogEntryType.Error);

                            return false;
                        }
                    }
                }

                if(jsonResponse == null)
                {
                    return false;
                }

                var toBeInserted = jsonResponse.Payload.Select(a =>
                    new
                    {
                        a.StockId,
                        a.TradeDate,
                        a.OpenPrice,
                        a.HighPrice,
                        a.LowPrice,
                        a.ClosePrice,
                        a.TradeValue,
                        a.TradeVolume,
                        a.CirculatedMarketValue,
                        a.TotalMarketValue,
                        a.PreviousClosePrice,
                        a.ExchangeCode,
                        TradeDateConverted = DateTime.Parse(a.TradeDate)
                    })
                    .OrderBy(b => b.TradeDateConverted).ToList();

                double? prevClosePriceComparableReinvest = null;
                double? prevClosePriceComparableNonReinvest = null;
                var previousDayRecord = _astock.STK_MKT_TradeDaily
                    .Where(a => a.StockID.Equals(stockId, StringComparison.InvariantCultureIgnoreCase)
                            && a.TradeDate2 == (DateTime)previousDay)
                    .FirstOrDefault();
                            
                if(previousDayRecord != null)
                {
                    prevClosePriceComparableNonReinvest = previousDayRecord.ClosePriceComparableNonReinvest;
                    prevClosePriceComparableReinvest = previousDayRecord.ClosePriceComparableReinvest;
                }

                foreach (var trade in toBeInserted)
                {
                    //var returnRateNonReinvest = trade.ClosePrice / trade.PreviousClosePrice - 1;
                    //var returnRateReinvest = returnRateNonReinvest;

                    var returnRateNonReinvest = 
                        GetReturnRate(
                            secureId, 
                            trade.StockId, 
                            DateTime.Parse(trade.TradeDate), 
                            null, 
                            ReturnRateType.NON_REINVEST)[DateTime.Parse(trade.TradeDate)];

                    var returnRateReinvest = 
                        GetReturnRate(
                            secureId, 
                            trade.StockId, 
                            DateTime.Parse(trade.TradeDate), 
                            null, 
                            ReturnRateType.REINVEST)[DateTime.Parse(trade.TradeDate)];

                    var curClosePriceComparableNonReinvest =
                        prevClosePriceComparableNonReinvest == null ?
                        null : prevClosePriceComparableNonReinvest * (1 + returnRateNonReinvest);
                    var curClosePriceComparableReinvest =
                        prevClosePriceComparableReinvest == null ?
                        null : prevClosePriceComparableReinvest * (1 + returnRateReinvest);

                    STK_MKT_TradeDaily newRecord = new STK_MKT_TradeDaily
                    {
                        StockID = trade.StockId,
                        TradeDate = trade.TradeDate,
                        TradeDate2 = DateTime.Parse(trade.TradeDate),
                        OpenPrice = trade.OpenPrice,
                        HighPrice = trade.HighPrice,
                        LowPrice = trade.LowPrice,
                        ClosePrice = trade.ClosePrice,
                        SumStockTrade = trade.TradeVolume,
                        SumVolumeTrade = trade.TradeValue,
                        SumCirculatedMarketValue = trade.CirculatedMarketValue,
                        SumTotalMarketValue = trade.TotalMarketValue,
                        ReturnRateReinvest = returnRateReinvest,
                        ReturnRateNonReinvest = returnRateNonReinvest,
                        ClosePriceComparableReinvest = curClosePriceComparableReinvest,
                        ClosePriceComparableNonReinvest = curClosePriceComparableNonReinvest,
                        MarketType = GetMarketType(trade.StockId),
                        TradeStatus = 1,
                        LatestCapitalChangeDate = null,
                        UniqueID = Guid.NewGuid(),
                        TRD_Co = _astock.TRD_Co
                            .Where(a => a.StockId.Equals(stockId, StringComparison.InvariantCultureIgnoreCase))
                            .FirstOrDefault()
                    };
#if(RELEASE)
                    _astock.STK_MKT_TradeDaily.Add(newRecord);
#endif

                    prevClosePriceComparableNonReinvest = curClosePriceComparableNonReinvest;
                    prevClosePriceComparableReinvest = curClosePriceComparableReinvest;
                }
#if(RELEASE)
                _astock.SaveChanges();
#endif
                _eventLog.WriteEntry(
                    string.Format("Successfully inserted {0} trade data records to database for stock ID {1}",
                    toBeInserted.Count, stockId), EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                _eventLog.WriteEntry(
                    string.Format(
                        "Unexpected error occurred.\nDetails = {0}\nStack Trace = {1}",
                        ex.Message, ex.StackTrace), EventLogEntryType.Error);

                if (ex.InnerException != null)
                {
                    _eventLog.WriteEntry(
                        string.Format(
                            "Inner Exception Message = {0}\nStack Trace = {1}",
                            ex.InnerException.Message, ex.InnerException.StackTrace),
                            EventLogEntryType.Error);
                }

                return false;
            }

            return true;
        }

        public static string GetSecureId(string stockId)
        {
            string secureId = string.Empty;
            
            try
            {
                var payload = new SecureIdRequest
                {
                    AssetClass = "E",
                    StockId = stockId,
                    Field = string.Empty
                };
                var request = (HttpWebRequest)WebRequest.Create(
                    string.Format(
                        "{0}/api/master/getSecID.json?assetClass={1}&ticker={2}&field={3}",
                        _baseUrl, payload.AssetClass, payload.StockId, payload.Field));
                request.Method = "GET";
                request.Headers.Add("Authorization: Bearer " + _token);
                var response = (HttpWebResponse)request.GetResponse();

                using (Stream stream = response.GetResponseStream())
                {
                    if (stream != null)
                    {
                        Encoding encoding = string.IsNullOrEmpty(response.CharacterSet) ?
                            Encoding.Default : Encoding.GetEncoding(response.CharacterSet);

                        using (var reader = new StreamReader(stream, encoding))
                        {
                            var result = reader.ReadToEnd();
                            var jsonResponse = JsonConvert.DeserializeObject<SecureIdResponse>(result);
                            if (jsonResponse.ReturnCode != 1)
                            {
                                var error = JsonConvert.DeserializeObject<ErrorResponse>(result);
                                _eventLog.WriteEntry(
                                    string.Format(
                                        "Unexpected error encountered when attempting to get the SecureID for {0}.\nError = {1}",
                                        stockId, error.ReturnMessage), EventLogEntryType.Error);
                            }
                            secureId = jsonResponse.Payload.FirstOrDefault().SecureId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _eventLog.WriteEntry(
                    string.Format(
                        "Unexpected error occurred.\nDetails = {0}\nStack Trace = {1}",
                        ex.Message, ex.StackTrace),
                        EventLogEntryType.Error);

                if (ex.InnerException != null)
                {
                    _eventLog.WriteEntry(
                        string.Format(
                            "Inner Exception Message = {0}\nStack Trace = {1}",
                            ex.InnerException.Message, ex.InnerException.StackTrace),
                            EventLogEntryType.Error);
                }
            }
         
            return secureId;
        }
    }
}
