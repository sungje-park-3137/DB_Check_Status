using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_SQL_NET_Framework.Command;

namespace WPF_SQL_NET_Framework.Info
{
    public class EF_Data : NotifyProperty
    {
        private string _ef_name = string.Empty;
        public string EF_Name
        {
            get { return _ef_name; }
            set { _ef_name = value; OnPropertyChanged(nameof(EF_Name)); }
        }

        private string _ef_id = string.Empty;
        public string EF_ID
        {
            get { return _ef_id; }
            set { _ef_id = value; OnPropertyChanged(nameof(EF_ID)); }
        }

        private string _ef_pw = string.Empty;
        public string EF_PW
        {
            get { return _ef_pw; }
            set { _ef_pw = value; OnPropertyChanged(nameof(EF_PW)); }
        }

        private bool _ef_admin = false;
        public bool EF_Admin
        {
            get { return _ef_admin; }
            set { _ef_admin = value; OnPropertyChanged(nameof(EF_Admin)); }
        }
    }
}
