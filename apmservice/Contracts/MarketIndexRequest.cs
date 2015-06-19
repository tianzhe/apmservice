using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace apmservice.Contracts
{
    [DataContract]
    class MarketIndexRequest
    {
        [DataMember(Name = "indexID")]
        public string IndexId { get; set; }

        [DataMember(Name = "ticker")]
        public string IndexTradeId { get; set; }

        [DataMember(Name = "tradeDate")]
        public string TradeDate { get; set; }

        [DataMember(Name = "beginDate")]
        public string QueryStartDate { get; set; }

        [DataMember(Name = "endDate")]
        public string QueryEndDate { get; set; }

        [DataMember(Name = "field")]
        public string Field { get; set; }
    }
}
