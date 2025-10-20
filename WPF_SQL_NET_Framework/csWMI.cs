using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WPF_SQL_NET_Framework.Info;
using WPF_SQL_NET_Framework.SingleTon;

namespace WPF_SQL_NET_Framework
{
    public class csWMI
    {
        public void GetProcessInfo()
        {
            string wmiQuery = "Select * From Win32_Process";
            ManagementObjectSearcher search = new ManagementObjectSearcher(wmiQuery);
            ManagementObjectCollection processList = search.Get();

            var type = processList.GetType();
        }
    }
}
