using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_SQL_NET_Framework.Info;

namespace WPF_SQL_NET_Framework.Interface
{
    public interface IServerList
    {
        Server_Info Init_DBInfo(int cnttab);
    }

    public class LISTFunc : IServerList
    {
        public Server_Info Init_DBInfo(int cntTab)
        {
            Server_Info server = new Server_Info
            {
                TabName = $"DB_Server_{cntTab + 1}",
                Server_IP = "192.168.0.244",
                Server_PORT = 1433,
                Server_ID = "sa",
                Server_PW = "Admin123",
                DB_List = new System.Collections.ObjectModel.ObservableCollection<DB_Info>
                {
                    new DB_Info {DBName = "GS425_IRE_DB"},
                    new DB_Info {DBName = "GS425_IEE_DB"},
                    new DB_Info {DBName = "IEE_TERRAIN"}
                }
            };

            return server;
        }
    }
}