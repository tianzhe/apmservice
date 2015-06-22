using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using apmservice.Events;
using System.Threading;

namespace apmservice
{
    public class Proxy : MarshalByRefObject
    {
        private Assembly assembly = null;
        public void LoadAssemlyFromFile(string path)
        {
            assembly = Assembly.LoadFile(path);
        }

        public bool Invoke(object sender, ModelGenerateEvent.ModelGenerateEventArgs args)
        {
            if (assembly == null)
            {
                return false;
            }
            var types = assembly.GetTypes();
            var tp = types
                .Where(t => t.Name.Equals(args.ClassName, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();
            if (tp == null)
            {
                return false;
            }
            MethodInfo method = tp.GetMethod(args.MethodName);
            if (method == null)
            {
                return false;
            }
            Object obj = Activator.CreateInstance(tp);
            method.Invoke(obj, args.InvokeArgs);

            return true;
        }
    }
}
