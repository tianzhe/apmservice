using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace apmservice.Contracts
{
    [DataContract]
    class SecureIdResponse : BaseResponse
    {
        [DataMember(Name = "data")]
        public List<SecureIdResponseAttribute> Payload { get; set; }
    }
    [DataContract]
    class SecureIdResponseAttribute
    {
        [DataMember(Name = "secID")]
        public string SecureId { get; set; }

        [DataMember(Name = "ticker")]
        public string StockId { get; set; }

        [DataMember(Name = "secShortName")]
        public string StockShortName { get; set; }

        [DataMember(Name = "cnSpell")]
        public string ChineseNameSpell { get; set; }

        [DataMember(Name = "exchangeCD")]
        public string ExchangeCode { get; set; }

        [DataMember(Name = "assetClass")]
        public string AssetClass { get; set; }

        [DataMember(Name = "listStatusCD")]
        public string ListStatusCode { get; set; }

        [DataMember(Name = "listDate")]
        public string ListDate { get; set; }

        [DataMember(Name = "transCurrCD")]
        public string TradeCurrency { get; set; }

        [DataMember(Name = "ISIN")]
        public string ISINCode { get; set; }

        [DataMember(Name = "partyID")]
        public Int64 PartyId { get; set; }
    }
}
