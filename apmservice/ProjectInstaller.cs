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

            RegistryKey key = Registry.LocalMachine.OpenSubKey(
                "SYSTEM\\CurrentControlSet\\Services\\Active Portfolio Management", true);
            if(key != null)
            {
                Registry.LocalMachine.DeleteSubKey("SYSTEM\\CurrentControlSet\\Services\\Active Portfolio Management");
            }
            key = Registry.LocalMachine.CreateSubKey(
                "SYSTEM\\CurrentControlSet\\Services\\Active Portfolio Management");

            key.SetValue("DataYesToken", 
                "5de3d02508fb9258fe5192f4a905e139110a1214723fa81dc2a3fcd225c8d0e4", RegistryValueKind.ExpandString);

            key.SetValue("DataYesBaseUrl", "https://api.wmcloud.com:443/data", RegistryValueKind.ExpandString);

            key.SetValue("SyncMarketIndexDataInterval", 15000, RegistryValueKind.QWord);
            key.SetValue("SyncTradeDataInterval", 15000, RegistryValueKind.QWord);
        }
    }
}
