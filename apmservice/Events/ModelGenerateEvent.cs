using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Messaging;

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

        public static void ModelGenerateTaskFinished(IAsyncResult result)
        {
            lock(_endInvokeLock)
            {
                AsyncResult asyncResult = (AsyncResult)result;

                ModelGenerateEventHandler handler = (ModelGenerateEventHandler)asyncResult.AsyncDelegate;
                bool b = handler.EndInvoke(result);
            }
        }

        private static object _endInvokeLock = new object();
    }
}
