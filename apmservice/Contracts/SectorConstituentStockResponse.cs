using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace apmservice.Contracts
{
    [DataContract]
    class SectorConstituentStockResponse : BaseResponse
    {
        [DataMember(Name = "data")]
        public List<SectorConstituentStockResponseAttribute> Payload { get; set; }
    }

    [DataContract]
    class SectorConstituentStockResponseAttribute
    {
        [DataMember(Name = "ticker")]
        public string StockId { get; set; }

        [DataMember(Name = "typeID")]
        public string TypeId { get; set; }

        [DataMember(Name = "typeName")]
        public string SectorName { get; set; }

        [DataMember(Name = "secID")]
        public string SecureId { get; set; }

        [DataMember(Name = "exchangeCD")]
        public string ExchangeCode { get; set; }

        [DataMember(Name = "secShortName")]
        public string StockShortName { get; set; }
    }
}
