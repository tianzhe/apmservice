using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apmservice.Events
{
    public class ModelGenerateEvent
    {
        [Serializable]
        public class ModelGenerateEventArgs : EventArgs
        {
            public string ClassName { get; set; }
            public string MethodName { get; set; }
            public Object[] InvokeArgs { get; set; }
        }

        public delegate bool ModelGenerateEventHandler(object sender, ModelGenerateEventArgs e);
    }
}
