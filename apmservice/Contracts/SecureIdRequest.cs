using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace apmservice.Contracts
{
    [DataContract]
    public class SecureIdRequest
    {
        [DataMember(Name="assetClass")]
        public string AssetClass { get; set; }

        [DataMember(Name="cnSpell")]
        public string ChineseNameSpell { get; set; }

        [DataMember(Name="partyID")]
        public Int64 PartyId { get; set; }

        [DataMember(Name="ticker")]
        public string StockId { get; set; }

        [DataMember(Name="field")]
        public string Field { get; set; }
    }
}
