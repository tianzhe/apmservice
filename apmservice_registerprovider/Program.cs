using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace apmservice_registerprovider
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Count() != 2)
            {
                Console.WriteLine(
                    string.Format("Error - The provider register does not accept the number of arguments supplied - {0}", 
                    args.Count()));
                return;
            }

            RegistryKey parent = null;
            RegistryKey current = null;
            try
            {
                var keylocation = "SYSTEM\\CurrentControlSet\\Services\\Active Portfolio Management";
                parent = Registry.LocalMachine.OpenSubKey(keylocation, true);
                var names = ((string[])parent.GetValue("ProviderNames")).ToList();
                names.Add(args[0]);
                parent.SetValue("ProviderNames", names.ToArray(), RegistryValueKind.MultiString);
               
                current = parent.CreateSubKey(args[0]);
                current.SetValue("IsCritical", 1, RegistryValueKind.DWord);
                current.SetValue("ModulePath", args[1], RegistryValueKind.ExpandString);
            }
            catch(Exception ex)
            {
                Console.WriteLine(
                    string.Format("Unexpected error occurred while attempting to register the provider '{0}'.\nDetails = {1}",
                    args[0], ex.Message));
                return;
            }
            finally
            {
                if(parent != null)
                {
                    parent.Close();
                }
                if(current != null)
                {
                    current.Close();
                }
            }
        }
    }
}
