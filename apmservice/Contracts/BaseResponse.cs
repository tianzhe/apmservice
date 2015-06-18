using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace apmservice.Contracts
{
    [DataContract]
    class BaseResponse
    {
        [DataMember(Name="retCode")]
        public Int32 ReturnCode{get;set;}

        [DataMember(Name="retMsg")]
        public string ReturnMessage{get;set;}
    }
}
