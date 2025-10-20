using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_SQL_NET_Framework.Command;

namespace WPF_SQL_NET_Framework.Info
{
    public class Logger_Data
    {
        public string LOG_DateTime { get; set; }
        public LogLevel LOG_Level {  get; set; }
        public string LOG_Msg { get; set; }
    }
}
