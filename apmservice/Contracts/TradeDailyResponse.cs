using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace apmservice.Contracts
{
    [DataContract]
    class TradeDailyResponse : BaseResponse
    {
        [DataMember(Name = "data")]
        public List<TradeDailyResponseAttribute> Payload { get; set; }
    }

    [DataContract]
    class TradeDailyResponseAttribute
    {
        [DataMember(Name = "secID")]
        public string SecureId { get; set; }

        [DataMember(Name = "tradeDate")]
        public string TradeDate { get; set; }

        [DataMember(Name = "ticker")]
        public string StockId { get; set; }

        [DataMember(Name = "secShortName")]
        public string StockShortName { get; set; }

        [DataMember(Name = "exchangeCD")]
        public string ExchangeCode { get; set; }

        [DataMember(Name = "preClosePrice")]
        public double PreviousClosePrice { get; set; }

        [DataMember(Name = "actPreClosePrice")]
        public double ActualPreviousClosePrice { get; set; }

        [DataMember(Name = "openPrice")]
        public double OpenPrice { get; set; }

        [DataMember(Name = "highestPrice")]
        public double HighPrice { get; set; }

        [DataMember(Name = "lowestPrice")]
        public double LowPrice { get; set; }

        [DataMember(Name = "closePrice")]
        public double ClosePrice { get; set; }

        [DataMember(Name = "turnoverVol")]
        public double TradeVolume { get; set; }

        [DataMember(Name = "turnoverValue")]
        public double TradeValue { get; set; }

        [DataMember(Name = "dealAmount")]
        public Int32 TradeAmount { get; set; }

        [DataMember(Name = "turnoverRate")]
        public double Turnover { get; set; }

        [DataMember(Name = "accumAdjFactor")]
        public double AccumulatedAdjacentFactor { get; set; }

        [DataMember(Name = "negMarketValue")]
        public double CirculatedMarketValue { get; set; }

        [DataMember(Name = "marketValue")]
        public double TotalMarketValue { get; set; }

        [DataMember(Name = "PE")]
        public double PETTM { get; set; }

        [DataMember(Name = "PE1")]
        public double PE { get; set; }

        [DataMember(Name = "PB")]
        public double PB { get; set; }
    }
}
