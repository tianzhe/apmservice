using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace apmservice.Contracts
{
    [DataContract]
    class StockDailyReturnRequest
    {
        [DataMember(Name = "listStatusCD")]
        public string StockListStatus { get; set; }

        [DataMember(Name = "secID")]
        public string SecureId { get; set; }

        [DataMember(Name = "ticker")]
        public string StockId { get; set; }

        [DataMember(Name = "beginDate")]
        public string QueryStartDate { get; set; }

        [DataMember(Name = "endDate")]
        public string QueryEndDate { get; set; }

        /// <summary>
        /// Select ReturnRateNonReinvest >= dailyReturnNoReinvLower or <= dailyReturnNoReinvLower
        /// </summary>
        [DataMember(Name = "dailyReturnNoReinvLower")]
        public double QueryReturnRateNonReinvestLower { get; set; }

        [DataMember(Name = "dailyReturnNoReinvUpper")]
        public double QueryReturnRateNonReinvestUpper { get; set; }

        [DataMember(Name = "dailyReturnReinvLower")]
        public double QueryReturnRateReinvestLower { get; set; }

        [DataMember(Name = "dailyReturnReinvUpper")]
        public double QueryReturnRateReinvestUpper { get; set; }

        /// <summary>
        /// 0 - Set limit (default value)
        /// 1 - IPO, without limit
        /// 2 - The 1st day after stock structure is changed, no limit
        /// 3 - Regulated by stock exchange, no limit
        /// </summary>
        [DataMember(Name = "isChgPctl")]
        public Int16 IsFluctuationLimitControlled { get; set; }

        [DataMember(Name = "field")]
        public string Field { get; set; }

    }
}
