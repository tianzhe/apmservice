using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace apmservice.Contracts
{
    [DataContract]
    class StockDailyReturnResponse : BaseResponse
    {
        [DataMember(Name = "data")]
        public List<StockDailyReturnResponseAttribute> Payload { get; set; }
    }

    [DataContract]
    class StockDailyReturnResponseAttribute
    {
        [DataMember(Name = "secID")]
        public string SecureId { get; set; }

        [DataMember(Name = "tradeDate")]
        public string TradeDate { get; set; }

        [DataMember(Name = "ticker")]
        public string StockId { get; set; }

        [DataMember(Name = "exchangeCD")]
        public string ExchangeCode { get; set; }

        [DataMember(Name = "secShortName")]
        public string StockShortName { get; set; }

        /// <summary>
        /// L - Listed
        /// S - Paused
        /// DE - Terminated
        /// UN - Not Listed
        /// </summary>
        [DataMember(Name = "listStatusCD")]
        public string ListStatusCode { get; set; }

        [DataMember(Name = "currencyCD")]
        public string TradeCurrency { get; set; }

        [DataMember(Name = "firstDayClosePrice")]
        public double ClosePriceListedFirstDay { get; set; }

        [DataMember(Name = "actPrevClosePrice")]
        public double ActualPreviousClosePrice { get; set; }

        [DataMember(Name = "closePrice")]
        public double ClosePrice { get; set; }

        [DataMember(Name = "perShareDivRatio")]
        public double PerShareDividendRatio { get; set; }

    }
}
