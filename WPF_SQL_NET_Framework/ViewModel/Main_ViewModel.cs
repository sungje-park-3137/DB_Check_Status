using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using WPF_SQL_NET_Framework.Command;
using WPF_SQL_NET_Framework.Interface;
using WPF_SQL_NET_Framework.Logger;
using WPF_SQL_NET_Framework.Model;
using WPF_SQL_NET_Framework.View;
using WPF_SQL_NET_Framework.View.EF;

namespace WPF_SQL_NET_Framework.ViewModel
{
    public partial class Main_ViewModel : NotifyProperty
    {
        private readonly IDB_Function _IDB; // Interface (DB Conn + STEP Func)
        private csLogerTest _logger = new csLogerTest(); // MS Logging + serilog
        // *** 현재 프로젝트의 모든 btn command는 main_viewmodel에서 실행 ***
        // -> 구조 변경 해야할듯
        // 1. partail 쓰지말고
        // 2. 기능은 다 interface
        // 3. 각 view마다 viewmodel 개별 관리
        // 4. 공통사항 app으로 내릴것

        // Property List: DB SQL Client
        private ObservableCollection<Server_Info> _serverinfo = new ObservableCollection<Server_Info>();
        public ObservableCollection<Server_Info> Server_List
        {
            get { return _serverinfo; }
            set { _serverinfo = value; OnPropertyChanged(nameof(Server_List)); }
        }

        // Property List: DB Entity Framwork6
        // ef info list -> FeedBack: config 공유 -> EF 사용 X
        /*private ObservableCollection<EF_Data> _efinfo = new ObservableCollection<EF_Data>();
        public ObservableCollection<EF_Data> DB_EF_Info
        {
            get { return _efinfo; }
            set { _efinfo = value; OnPropertyChanged(nameof(DB_EF_Info)); }
        }
        private DB_TEST_Address _EF_SelectTable = new DB_TEST_Address();
        public DB_TEST_Address EF_SelectTable
        {
            get { return _EF_SelectTable; }
            set { _EF_SelectTable = value; OnPropertyChanged(nameof(EF_SelectTable)); Show_SelectItem(); }
        }*/


        // LOG View로 전달할: Log List
        private ObservableCollection<string> _loglist = new ObservableCollection<string>();
        public ObservableCollection<string> Log_List
        {
            get { return _loglist; }
            set { _loglist = value; OnPropertyChanged(nameof(Log_List)); }
        }

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
                        Check_Ping();
                    }
                    else
                    {
                        TimerOFF();
                    }
                }
            }
        }

        private bool _Btn_Enable = true;
        public bool Btn_Enable
        {
            get { return _Btn_Enable; }
            set { _Btn_Enable = value; OnPropertyChanged(nameof(Btn_Enable)); }
        }

        private int _selectIndex;
        public int SelectTabIndex
        {
            get { return _selectIndex; }
            set { _selectIndex = value; OnPropertyChanged(nameof(SelectTabIndex)); }
        }

        private string _Btn_Text = "연결";
        public string Btn_Text
        {
            get { return _Btn_Text; }
            set { _Btn_Text = value; OnPropertyChanged(nameof(Btn_Text)); }
        }

        private bool _PB_Loading = false;
        public bool PB_Loading
        {
            get { return _PB_Loading; }
            set { _PB_Loading = value; OnPropertyChanged(nameof(PB_Loading)); }
        }

        private int _serverlistIndex;
        public int Index_ServerList
        {
            get { return _serverlistIndex; }
            set { _serverlistIndex = value; OnPropertyChanged(nameof(Index_ServerList)); }
        }

        private bool _Show_AllLogs = false;
        public bool Show_AllLogs
        {
            get { return _Show_AllLogs; }
            set
            {
                if (_Show_AllLogs != value)
                {
                    _Show_AllLogs = value;
                    OnPropertyChanged(nameof(Show_AllLogs));

                    if (Show_AllLogs == true)
                    {
                        Check_Ping();
                    }
                    else
                    {
                        TimerOFF();
                    }
                }
            }
        }

        private bool _Show_WARLogs = false;
        public bool Show_WARLogs
        {
            get { return _Show_WARLogs; }
            set
            {
                if (_Show_WARLogs != value)
                {
                    _Show_WARLogs = value;
                    OnPropertyChanged(nameof(Show_WARLogs));

                    if (Show_WARLogs == true)
                    {
                        Check_Ping();
                    }
                    else
                    {
                        TimerOFF();
                    }
                }
            }
        }

        private bool _Show_INFLogs = false;
        public bool Show_INFLogs
        {
            get { return _Show_INFLogs; }
            set
            {
                if (_Show_INFLogs != value)
                {
                    _Show_INFLogs = value;
                    OnPropertyChanged(nameof(Show_INFLogs));

                    if (Show_INFLogs == true)
                    {
                        Check_Ping();
                    }
                    else
                    {
                        TimerOFF();
                    }
                }
            }
        }

        private bool _Show_ERRLogs = false;
        public bool Show_ERRLogs
        {
            get { return _Show_ERRLogs; }
            set
            {
                if (_Show_ERRLogs != value)
                {
                    _Show_ERRLogs = value;
                    OnPropertyChanged(nameof(Show_ERRLogs));

                    if (Show_ERRLogs == true)
                    {
                        Check_Ping();
                    }
                    else
                    {
                        TimerOFF();
                    }
                }
            }
        }





        // Command
        public ButtonCommand AddTabInfo { get; }
        public ButtonCommand DB_Connect { get; }
        public ButtonCommand DB_Reconnect { get; }
        public ButtonCommand DelTabInfo { get; }
        public ButtonCommand Server_Query {  get; }
        public ButtonCommand DB_EF { get; }
        public ButtonCommand EF_Read { get; }
        public ButtonCommand EF_Create { get; }
        public ButtonCommand EF_Update { get; }
        public ButtonCommand EF_Delete { get; }
        public ButtonCommand DB_PowerShell { get; }
        public ButtonCommand DB_SelectRetry {  get; }
        public ButtonCommand Search_Btn { get; }



        public Main_ViewModel()
        {
            AddTabInfo = new ButtonCommand(Act_AddTab);
            DB_Connect = new ButtonCommand(async x => await Act_DBConnect());
            DB_Reconnect = new ButtonCommand(Act_DB_Reconnect);
            DelTabInfo = new ButtonCommand(Act_DelTab);
            Server_Query = new ButtonCommand(Act_Server_Query);
            DB_EF = new ButtonCommand(Act_DB_EF);
            DB_PowerShell = new ButtonCommand(async y => await Act_DB_PowerShell());
            DB_SelectRetry = new ButtonCommand(Act_DB_SelectRetry);
            Search_Btn = new ButtonCommand(Act_SearchTxt);


            /*
            EF_Read = new ButtonCommand(Act_EF_Read);
            EF_Create = new ButtonCommand(Act_DB_EF_Create);
            EF_Update = new ButtonCommand(Act_EF_Update);
            EF_Delete = new ButtonCommand(Act_EF_Delete);*/

            //Init_DB();
            //Init_EF();

            // Init -> fd: config로 내려라... (?)
            // DB List Init
            _IDB = new DBFunc();
            Server_List.Add(_IDB.Init_DBInfo(Server_List.Count()));
            Logevent = _logger.logEvent;
        }

        public bool CanCheck()
        {
            return true;
        }

        // fd: confing로 내려라(?)가 무슨 의미인지 모르겠다 아직...
        // re: interface에 일단 집어넣자, 작업 interface
        private void Act_AddTab()
        {
            Server_List.Add(_IDB.Init_DBInfo(Server_List.Count()));
        }

        private void Act_DelTab()
        {
            if (SelectTabIndex < 0)
            {
                MessageBox.Show("삭제할 탭을 선택해주세요");
            }
            else
            {
                Server_List.RemoveAt(SelectTabIndex);
            }
        }

        // feed back: (nuger) logger 작업 처리 필요
        // 1. 로그 파일화
        // 2. viewer 옵션 처리 화
        //public void AppendLog(LogLevel level, string msg)
        //{
        //    try
        //    {
        //        // con queue 사용할것
        //        //string appendlog = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {msg}";
        //        //Log_List.Add(appendlog);



        //        Logger_Data tempLD = new Logger_Data
        //        {
        //            loglevel = level,
        //            logmsg = msg
        //        };

        //        ConcurrentQueue<Logger_Data> conqueue = new ConcurrentQueue<Logger_Data>();
        //        conqueue.Enqueue(tempLD);

        //        if (conqueue.Count > 0)
        //        {
        //            if (conqueue.TryDequeue(out Logger_Data result))
        //            {
        //                _logger.CreateLog(result.loglevel, result.logmsg);
        //            }
        //            else
        //            {
        //                throw new Exception("concurrentQueue의 데이터 가져오기 실패!");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}

        // re: 완료, 작업: lnterface, linq
        public void Act_DB_Reconnect()
        {
            try
            {
                // void 형식은 그냥 foreach. for문 사용(?)
                foreach (var server in Server_List)
                {
                    foreach (var db in server.DB_List)
                    {
                        _IDB.DBDisconnect(db.DBSQL);
                    }
                }

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

        // re: 완료, 작업: lnterface, linq
        public async Task Act_DBConnect()
        {
            try
            {
                Btn_Text = "연결 중";
                Btn_Enable = false;
                PB_Loading = true;

                if (await Check_PoewerShell())
                {
                    if (await Check_DBConnect())
                    {
                        foreach (Window win in Application.Current.Windows)
                        {
                            if (win is DB_LogIN)
                            {
                                MainWindow main = new MainWindow();
                                //main.DataContext = this; //-> data context 할당 방법이 locator에 존재하는 거 같은데 지금은 우선 data resource 활용(추후 ㄱㄱ)
                                AutoCheck = true;
                                main.Show();
                                win.Close();
                                break;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("모든 DB 연결 불가");
                    }
                }
                else
                {
                    MessageBox.Show("모든 Server 연결 불가");
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

        // re: 완료, 작업: lnterface, linq
        private void Act_Server_Query()
        {
            try
            {
                Check_Query_DBInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // new: 추가, powershell 동작 확인 기능 별도 생성
        private async Task Act_DB_PowerShell()
        {
            if (await Check_PoewerShell())
            {
                //
            }
            else
            {
                MessageBox.Show("모든 Server 연결 불가");
            }
        }

        // usercontrol -> itemsource에 할당되어 있는 data와 command 가지고 오는 법 -> commandparameter 사용.
        public async void Act_DB_SelectRetry(object _btn_DBInfo)
        {
            // 총 2가지의 정보가 필요함
            // 전체 서버 ip, port를 관리하는 server_info
            // -> tab index: 해결 완료
            // 서버 내부의 클릭한 db sql
            // -> command, commandparameter 로 해결
            Server_Info findServer = Server_List[Index_ServerList];
            DB_Info tempDB_List = (DB_Info)_btn_DBInfo;

            switch (tempDB_List.DB_StepState)
            {
                case DB_Info.DB_STEP_State.BASE:

                    _logger.CreateLog(LogLevel.Information, $"[BASE] DB Name: {tempDB_List.DBName} - PING 통신 시작");
                    bool checkBase = _IDB.STEP_Base(tempDB_List);
                    _logger.CreateLog(LogLevel.Information, $"[BASE] DB Name: {tempDB_List.DBName} - PING 통신 결과: {(checkBase ? "양호" : "불량")}");

                    break;

                case DB_Info.DB_STEP_State.WARNING:

                    _logger.CreateLog(LogLevel.Warning, $"[WARNING] DB Name: {tempDB_List.DBName} - SQL Client Connection 확인 시작");
                    bool checkWarning = _IDB.STEP_Warning(tempDB_List);
                    _logger.CreateLog(LogLevel.Warning, $"[WARNING] DB Name: {tempDB_List.DBName} - SQL Client Connection 확인 결과: {(checkWarning ? "양호" : "불량")}");

                    break;

                case DB_Info.DB_STEP_State.ERROR:

                    _logger.CreateLog(LogLevel.Error, $"[ERROR] Server [IP: {findServer.Server_IP}, PORT: {findServer.Server_PORT}] - Network cmd 통신 시작");
                    bool checkError = await _IDB.STEP_Error(findServer);
                    _logger.CreateLog(LogLevel.Error, $"[ERROR] Server [IP: {findServer.Server_IP}, PORT: {findServer.Server_PORT}] - Network cmd 통신 결과: {(checkError ? "양호" : "불량")}");

                    break;

                case DB_Info.DB_STEP_State.CRITICAL:

                    _logger.CreateLog(LogLevel.Critical, $"[CRITICAL] Server [IP: {findServer.Server_IP}, PORT: {findServer.Server_PORT}] - Power Shell 통신 시작");
                    bool checkCritical = await _IDB.STEP_Critical(findServer);
                    _logger.CreateLog(LogLevel.Critical, $"[CRITICAL] Server [IP: {findServer.Server_IP}, PORT: {findServer.Server_PORT}] - Power Shell 통신 결과 : {(checkCritical ? "양호" : "불량")}");

                    break;
            }
        }

        private void Act_SearchTxt()
        {
            try
            {
                Log_Option(Search_Txt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        // EF 관련: 우선 SQL Client로 작업 진행 지시
        private void Act_DB_EF()
        {
            try
            {
                //bool result = SendQuery_DataReader("SELECT name, state, state_desc FROM sys.databases WHERE name = 'GS425_IEE_DB';", DB_Info, out List<string> rcvdata);
            }
            catch (Exception ex)
            {
                //AppendLog(ex.Message);
            }
        }

        private void Act_EF_Read()
        {
            try
            {
                DB_EF_Read();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Act_DB_EF_Create()
        {
            try
            {
                DB_EF_Create();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Act_EF_Delete()
        {
            try
            {
                DB_EF_Del();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Act_EF_Update()
        {
            try
            {
                DB_EF_Update();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
