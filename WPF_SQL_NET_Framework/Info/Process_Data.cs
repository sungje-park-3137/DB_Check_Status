using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_SQL_NET_Framework.Command;

namespace WPF_SQL_NET_Framework.Info
{
    public class Process_Data : NotifyProperty
    {
        private bool _IsCheck = false;
        public bool IsCheck
        {
            get { return _IsCheck; }
            set { _IsCheck = value; OnPropertyChanged(nameof(IsCheck)); }
        }

        private int _PID = 0;
        public int PID
        {
            get { return _PID; }
            set { _PID = value; OnPropertyChanged(nameof(PID)); }
        }

        private string _PName = string.Empty;
        public string PName
        {
            get { return _PName; }
            set { _PName = value; OnPropertyChanged(nameof(PName)); }
        }

        private string _PPath = string.Empty;
        public string PPath
        {
            get { return _PPath; }
            set { _PPath = value; OnPropertyChanged(nameof(PPath)); }
        }

        private ObservableCollection<PResource_Data> _PResource = new ObservableCollection<PResource_Data>();
        public ObservableCollection<PResource_Data> PResource
        {
            get { return _PResource; }
            set { _PResource = value; OnPropertyChanged(nameof(PResource)); }
        }

        

        private double _LastPCpuTime = 0;
        public double LastPCpuTime
        {
            get { return _LastPCpuTime; }
            set { _LastPCpuTime = value; OnPropertyChanged(nameof(LastPCpuTime)); }
        }

        private long _LastPMemory = 0;
        public long LastPMemory
        {
            get { return _LastPMemory; }
            set { _LastPMemory = value; OnPropertyChanged(nameof(LastPMemory)); }
        }
    }
}
