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
    public class Server_Info : NotifyProperty
    {
        // 탭 이름
        private string _TabName = string.Empty;
        public string TabName
        {
            get { return _TabName; }
            set { _TabName = value; OnPropertyChanged(nameof(TabName)); }
        }

        // SERVER: IP
        private string _ip = string.Empty;
        public string Server_IP
        {
            get { return _ip; }
            set { _ip = value; OnPropertyChanged(nameof(Server_IP)); }
        }

        // SERVER: PORT
        private int _port = 0;
        public int Server_PORT
        {
            get { return _port; }
            set { _port = value; OnPropertyChanged(nameof(Server_PORT)); }
        }

        // SERVER: ID
        private string _username = string.Empty;
        public string Server_ID
        {
            get { return _username; }
            set { _username = value; OnPropertyChanged(nameof(Server_ID)); }
        }

        // SERVER: PW
        private string _password = string.Empty;
        public string Server_PW
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged(nameof(Server_PW)); }
        }

        // SERVER: DB
        private ObservableCollection<DB_Info> _dbname = new ObservableCollection<DB_Info>();
        public ObservableCollection<DB_Info> DB_List
        {
            get { return _dbname; }
            set { _dbname = value; OnPropertyChanged(nameof(DB_List)); }
        }

        // ?? 필요한가? 불필요해보임
        // -> 다중 서버 연결 확인할 경우 탭 별로 생성 되는데 탭(list) 각각 데이터를 가지고 있어야함
        //DB: Server DataTable Query
        private DataTable _DB_server_RcvTable = new DataTable();
        public DataTable DB_Server_RcvTable
        {
            get { return _DB_server_RcvTable; }
            set { _DB_server_RcvTable = value; OnPropertyChanged(nameof(DB_Server_RcvTable)); }
        }

        //DB: Database DataTable Query
        private DataTable _DB_RcvTable = new DataTable();
        public DataTable DB_RcvTable
        {
            get { return _DB_RcvTable; }
            set { _DB_RcvTable = value; OnPropertyChanged(nameof(DB_RcvTable)); }
        }


        // 신규 추가
        private bool _PowerShell_State = false;
        public bool PowerShell_State
        {
            get => _PowerShell_State;
            set { _PowerShell_State = value; OnPropertyChanged(nameof(PowerShell_State)); }
        }
    }
}
