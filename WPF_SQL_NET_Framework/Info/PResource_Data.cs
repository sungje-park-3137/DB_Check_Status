using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_SQL_NET_Framework.Command;

namespace WPF_SQL_NET_Framework.Info
{
    public class PResource_Data : NotifyProperty
    {
        private double _PCpuTime = 0;
        public double PCpuTime
        {
            get { return _PCpuTime; }
            set { _PCpuTime = value; OnPropertyChanged(nameof(PCpuTime)); }
        }

        private long _PMemory = 0;
        public long PMemory
        {
            get { return _PMemory; }
            set { _PMemory = value; OnPropertyChanged(nameof(PMemory)); }
        }

        private DateTime _PDateTime = new DateTime();
        public DateTime PDateTime
        {
            get { return _PDateTime; }
            set { _PDateTime = value; OnPropertyChanged(nameof(PDateTime)); }
        }
    }
}
