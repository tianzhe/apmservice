using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace apmservice.Contracts
{
    [DataContract]
    class MarketIndexResponse : BaseResponse
    {
        [DataMember(Name = "data")]
        public List<MarketIndexResponseAttribute> Payload { get; set; }
    }

    [DataContract]
    class MarketIndexResponseAttribute
    {
        [DataMember(Name = "indexID")]
        public string IndexId { get; set; }

        [DataMember(Name = "tradeDate")]
        public string TradeDate { get; set; }

        [DataMember(Name = "ticker")]
        public string IndexTradeId { get; set; }

        [DataMember(Name = "secShortName")]
        public string IndexShortName { get; set; }

        [DataMember(Name = "exchangeCD")]
        public string ExchangeCode { get; set; }

        [DataMember(Name = "preCloseIndex")]
        public double PreviousCloseIndex { get; set; }

        [DataMember(Name = "openIndex")]
        public double OpenIndex { get; set; }

        [DataMember(Name = "lowestIndex")]
        public double LowIndex { get; set; }

        [DataMember(Name = "highestIndex")]
        public double HighIndex { get; set; }

        [DataMember(Name = "closeIndex")]
        public double CloseIndex { get; set; }

        [DataMember(Name = "turnoverVol")]
        public double TurnoverVolume { get; set; }

        [DataMember(Name = "turnoverValue")]
        public double TurnoverValue { get; set; }

        [DataMember(Name = "CHG")]
        public double IndexChangePoints { get; set; }

        [DataMember(Name = "CHGPct")]
        public double IndexChangePercentage { get; set; }
    }
}
