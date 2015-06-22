using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Security.AccessControl;

namespace apmservice
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            RegistryKey key = null;
            try
            {

                key = Registry.LocalMachine.OpenSubKey(
                    "SYSTEM\\CurrentControlSet\\Services\\Active Portfolio Management", true);
                if (key == null)
                {
                    key = Registry.LocalMachine.CreateSubKey(
                        "SYSTEM\\CurrentControlSet\\Services\\Active Portfolio Management");
                }

                key.SetValue("DataYesToken",
                    "5de3d02508fb9258fe5192f4a905e139110a1214723fa81dc2a3fcd225c8d0e4", RegistryValueKind.String);

                key.SetValue("DataYesBaseUrl", "https://api.wmcloud.com:443/data", RegistryValueKind.String);

                key.SetValue("SyncMarketIndexDataInterval", 15000, RegistryValueKind.QWord);
                key.SetValue("SyncTradeDataInterval", 15000, RegistryValueKind.QWord);
                key.SetValue("DefaultQueryStartDate", "2012-01-01", RegistryValueKind.String);
                key.SetValue("ProviderNames", new string[]{}, RegistryValueKind.MultiString);
            }
            catch(Exception ex)
            {
                Console.WriteLine(
                    string.Format("Unexpected error occurred when attempting to install apmservice.\nMessage = {0}\nStack Trace = {1}",
                    ex.Message, ex.StackTrace));
            }
            finally
            {
                if(key != null)
                {
                    key.Close();
                }
            }
        }
    }
}
