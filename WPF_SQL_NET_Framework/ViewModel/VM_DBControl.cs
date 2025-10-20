using Microsoft.Extensions.Logging;
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
using WPF_SQL_NET_Framework.Interface;
using WPF_SQL_NET_Framework.Info;
using WPF_SQL_NET_Framework.Model;
using WPF_SQL_NET_Framework.View;

namespace WPF_SQL_NET_Framework.ViewModel
{
    public class VM_DBControl : NotifyProperty
    {
        #region [Instance]
        private readonly Model.M_DBControl _DBControl = new Model.M_DBControl();
        private readonly M_DBLogin _DBLogin = new M_DBLogin();
        private readonly VM_DBLogin _VMDBLogin = new VM_DBLogin();
        private M_ProcessControl _MPControl = new M_ProcessControl();
        private csLogger _Logger;
        private csServerList _ServerList;
        private readonly VM_ProcessControl _PControl;

        #endregion

        #region [Variable]
        public ObservableCollection<Logger_Data> Logevent
        {
            get { return _Logger.logEvent; }
        }
        public ObservableCollection<Server_Info> Server_List
        {
            get { return _ServerList.Server_Lists; }
        }

        #endregion

        #region [Property]
        private bool _AutoCheck = false;
        public bool AutoCheck
        {
            get { return _AutoCheck; }
            set
            {
                if (_AutoCheck != value)
                {
                    _AutoCheck = value;
                    OnPropertyChanged(nameof(AutoCheck));

                    if (AutoCheck == true)
                    {
                        _DBControl.Check_Ping();
                    }
                    else
                    {
                        _DBControl.Stop_STEP();
                    }
                }
            }
        }

        private bool _alllog = true;
        public bool ALL_Log
        {
            get { return _alllog; }
            set { _alllog = value; OnPropertyChanged(nameof(ALL_Log)); _DBControl.Log_Option(Logevent, ALL_Log, INF_Log, WAR_Log, ERR_Log, FTL_Log); }
        }

        private bool _inflog = false;
        public bool INF_Log
        {
            get { return _inflog; }
            set { _inflog = value; OnPropertyChanged(nameof(INF_Log)); _DBControl.Log_Option(Logevent, ALL_Log, INF_Log, WAR_Log, ERR_Log, FTL_Log); }
        }

        private bool _warlog = false;
        public bool WAR_Log
        {
            get { return _warlog; }
            set { _warlog = value; OnPropertyChanged(nameof(WAR_Log)); _DBControl.Log_Option(Logevent, ALL_Log, INF_Log, WAR_Log, ERR_Log, FTL_Log); }
        }

        private bool _errlog = false;
        public bool ERR_Log
        {
            get { return _errlog; }
            set { _errlog = value; OnPropertyChanged(nameof(ERR_Log)); _DBControl.Log_Option(Logevent, ALL_Log, INF_Log, WAR_Log, ERR_Log, FTL_Log); }
        }

        private bool _ftllog = false;
        public bool FTL_Log
        {
            get { return _ftllog; }
            set { _ftllog = value; OnPropertyChanged(nameof(FTL_Log)); _DBControl.Log_Option(Logevent, ALL_Log, INF_Log, WAR_Log, ERR_Log, FTL_Log); }
        }

        private string _Search_Txt = string.Empty;
        public string Search_Txt
        {
            get { return _Search_Txt; }
            set { _Search_Txt = value; OnPropertyChanged(nameof(Search_Txt)); }
        }

        private int _Index_ServerList = 0;
        public int Index_ServerList
        {
            get { return _Index_ServerList; }
            set { _Index_ServerList = value; OnPropertyChanged(nameof(Index_ServerList)); }
        }

        #endregion [Property]

        #region [ButtonCommand]
        // btn command 등록
        public ButtonCommand Search_Btn { get; }
        public ButtonCommand DB_Test { get; }
        public ButtonCommand DB_PowerShell { get; }
        public ButtonCommand DB_Reconnect { get; }
        public ButtonCommand DB_SelectRetry { get; }

        // 동작 등록
        public VM_DBControl()
        {
            Search_Btn = new ButtonCommand(Act_SearchTxt);
            DB_PowerShell = new ButtonCommand(async y => await Act_DB_PowerShell());
            DB_Reconnect = new ButtonCommand(Act_DB_Reconnect);
            DB_SelectRetry = new ButtonCommand(Act_DB_Retry);

            // other (viewmodel || model) -> get: property event
            _DBControl.State_AutoCheck += Change_State;
            _VMDBLogin._State_Auto += Change_State;

            _Logger = csLogger.GetInstance();
            _ServerList = csServerList.GetInstance();

            DB_Test = new ButtonCommand(Act_DB_Test);
        }

        // 동작 구현
        private void Act_SearchTxt()
        {
            try
            {
                _DBControl.Log_Option(Logevent, ALL_Log, INF_Log, WAR_Log, ERR_Log, FTL_Log, Search_Txt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async Task Act_DB_PowerShell()
        {
            try
            {
                if (await _DBLogin.Check_PoewerShell())
                {
                    //
                }
                else
                {
                    throw new Exception("모든 Server 연결 불가");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Act_DB_Reconnect()
        {
            try
            {
                _DBControl.DBDisconnector();

                DB_LogIN _login = new DB_LogIN();
                _login.Show();
                foreach (Window win in Application.Current.Windows)
                {
                    if (win is MainWindow)
                    {
                        win.Close();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Act_DB_Retry(object _btn_DBInfo)
        {
            try
            {
                _DBControl.DB_Retry(Index_ServerList, (DB_Info)_btn_DBInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Change_State(bool state)
        {
            AutoCheck = state;
        }

        private void Act_DB_Test()
        {
            //_PControl.Process_List.Clear();
            //_MPControl.GetProcessList().ForEach(plist => _PControl.Process_List.Add(plist));
        }

        #endregion [ButtonCommand List]
    }
}
