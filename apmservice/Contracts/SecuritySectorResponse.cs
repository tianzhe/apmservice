using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace apmservice.Contracts
{
    [DataContract]
    class SecuritySectorResponse : BaseResponse
    {
        [DataMember(Name = "data")]
        public List<SecuritySectorResponseAttribute> Payload { get; set; }
    }

    [DataContract]
    class SecuritySectorResponseAttribute
    {
        [DataMember(Name = "typeID")]
        public string TypeId { get; set; }

        [DataMember(Name = "typeName")]
        public string TypeName { get; set; }

        [DataMember(Name = "parentID")]
        public string ParentId { get; set; }

        [DataMember(Name = "typeLevel")]
        public Int16 TypeLevel { get; set; }
    }
}
