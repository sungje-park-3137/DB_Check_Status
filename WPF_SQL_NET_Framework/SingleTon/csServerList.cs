using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_SQL_NET_Framework.Info;

namespace WPF_SQL_NET_Framework.SingleTon
{
    public class csServerList
    {
        private static csServerList instance;
        public static csServerList GetInstance()
        {
            if (instance == null)
            {
                instance = new csServerList();
            }

            return instance;
        }

        public ObservableCollection<Server_Info> Server_Lists { get; } = new ObservableCollection<Server_Info>();
    }
}
