using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WPF_SQL_NET_Framework.Command;
using WPF_SQL_NET_Framework.SingleTon;
using WPF_SQL_NET_Framework.Info;
using WPF_SQL_NET_Framework.Model;
using WPF_SQL_NET_Framework.View;

namespace WPF_SQL_NET_Framework.ViewModel
{
    public class VM_DBLogin : NotifyProperty
    {
        #region [Instance List]
        private M_DBLogin _DBLogin = new M_DBLogin();
        public event Action<bool> _State_Auto;
        private csServerList _ServerList;

        #endregion [Instance List]

        #region [ObservalbeCollection List]
        public ObservableCollection<Server_Info> Server_List
        {
            get { return _ServerList.Server_Lists; }
        }

        #endregion [ObservalbeCollection List]

        #region [PropertyChagne List]
        private bool _PB_Loading = false;
        public bool PB_Loading
        {
            get { return _PB_Loading; }
            set { _PB_Loading = value; OnPropertyChanged(nameof(PB_Loading)); }
        }

        private bool _Btn_Enable = true;
        public bool Btn_Enable
        {
            get { return _Btn_Enable; }
            set { _Btn_Enable = value; OnPropertyChanged(nameof(Btn_Enable)); }
        }

        private string _Btn_Text = "연결";
        public string Btn_Text
        {
            get { return _Btn_Text; }
            set { _Btn_Text = value; OnPropertyChanged(nameof(Btn_Text)); }
        }

        private int _SelectTabIndex;
        public int SelectTabIndex
        {
            get { return _SelectTabIndex; }
            set { _SelectTabIndex = value; OnPropertyChanged(nameof(SelectTabIndex)); }
        }

        #endregion [PropertyChagne List]

        #region [ButtonCommand List]
        // btn command 등록
        public ButtonCommand AddTabInfo { get; }
        public ButtonCommand DelTabInfo { get; }
        public ButtonCommand DB_Connect { get; }

        // 동작 등록
        public VM_DBLogin()
        {
            AddTabInfo = new ButtonCommand(Act_AddTabInfo);
            DelTabInfo = new ButtonCommand(Act_DelTabInfo);
            DB_Connect = new ButtonCommand(async con => await Act_DB_Connect());

            _ServerList = csServerList.GetInstance();
        }

        // 동작 구현
        private void Act_AddTabInfo()
        {
            try
            {
                _DBLogin.Add_List();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Act_DelTabInfo()
        {
            try
            {
                _DBLogin.Del_List(SelectTabIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async Task Act_DB_Connect()
        {
            try
            {
                Btn_Text = "연결 중";
                Btn_Enable = false;
                PB_Loading = true;

                if (await _DBLogin.Check_PoewerShell())
                {
                    if (await _DBLogin.DBConnector())
                    {
                        // login 성공 시 login 화면 종료 -> 메인 화면 show 이거 로직 재구성 필요
                        foreach (Window win in Application.Current.Windows)
                        {
                            if (win is DB_LogIN)
                            {
                                MainWindow main = new MainWindow();
                                main.Show();
                                win.Close();

                                //State_Auto(true);
                                
                                break;
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("모든 DB 연결 불가");
                    }
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
            finally
            {
                Btn_Text = "연결";
                Btn_Enable = true;
                PB_Loading = false;
            }
        }

        // 자동시작은 일단 보류
        private void State_Auto(bool state)
        {
            _State_Auto.Invoke(state);
        }

        #endregion [ButtonCommand List]
    }
}
