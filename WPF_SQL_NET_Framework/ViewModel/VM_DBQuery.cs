using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WPF_SQL_NET_Framework.Command;
using WPF_SQL_NET_Framework.SingleTon;
using WPF_SQL_NET_Framework.Model;
using WPF_SQL_NET_Framework.Info;

namespace WPF_SQL_NET_Framework.ViewModel
{
    public class VM_DBQuery : NotifyProperty
    {
        #region [Instance List]
        private readonly M_DBQuery _DBQuery = new M_DBQuery();
        private csServerList _Server_List;

        #endregion [Instance List]

        #region [ObservalbeCollection List]
        public ObservableCollection<Server_Info> Server_List
        {
            get { return _Server_List.Server_Lists; }
        }

        #endregion [ObservalbeCollection List]

        #region [PropertyChagne List]


        #endregion [PropertyChagne List]

        #region [ButtonCommand List]
        // btn command 등록
        public ButtonCommand Server_Query { get; }

        // 동작 등록
        public VM_DBQuery()
        {
            Server_Query = new ButtonCommand(Act_Server_Query);
            _Server_List = csServerList.GetInstance();
        }

        // 동작 구현
        private void Act_Server_Query()
        {
            try
            {
                _DBQuery.Check_Query_DBInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion [ButtonCommand List]
    }
}
