using Microsoft.Extensions.Logging;
using Serilog.Events;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;
using WPF_SQL_NET_Framework.Interface;
using WPF_SQL_NET_Framework.SingleTon;
using WPF_SQL_NET_Framework.Info;

namespace WPF_SQL_NET_Framework.Model
{
    public class M_DBControl
    {
        #region [Instance]
        private csServerList _serverList;
        private csLogger _Logger;
        private readonly ISTEPSequence _IStep;
        private readonly IDBConnect _IDBConnect;
        public event Action<bool> State_AutoCheck;

        #endregion

        #region [Constructor]
        public M_DBControl()
        {
            _IStep = new STEPFunc();
            _IDBConnect = new DBFunc();
            _serverList = csServerList.GetInstance();
            _Logger = csLogger.GetInstance();
        }

        #endregion

        #region [Variable]
        private DB_STEP_State STEP_State = DB_STEP_State.BASE;
        private ObservableCollection<Server_Info> Server_List
        {
            get { return _serverList.Server_Lists; }
        }

        private enum DB_STEP_State
        {
            BASE,
            WARNING,
            ERROR,
            CRITICAL
        }

        #endregion

        #region [Method]
        /// <summary>
        /// DataGrid Filter 기능 사용 - Option: CheckBox
        /// </summary>
        /// <param name="logs"></param>
        /// <param name="alllog"></param>
        /// <param name="inflog"></param>
        /// <param name="warlog"></param>
        /// <param name="errlog"></param>
        /// <param name="ftllog"></param>
        public void Log_Option(ObservableCollection<Logger_Data> logs, bool alllog, bool inflog, bool warlog, bool errlog, bool crilog)
        {
            // 개념-filter 동작: 람다로 조건문에 부합하면 true를 반환 -> true로 반환 된 데이터만 show
            // 지금 구조 문제: filter 조건 먼저 확인 -> 내용 수행
            // -> 변수 하나 참조 값으로 빼서 해당 값에 true를 누적한다
            // -> 가 아니라 반환 값이 true 가 핵심임 어짜피 event 들어오면 다음 if 문 돌아가고 누적 됨
            var viewdatagrid = CollectionViewSource.GetDefaultView(logs);

            viewdatagrid.Filter = temp =>
            {
                bool checkbox = false;

                if (alllog
                || (inflog && (temp as Logger_Data).LOG_Level == LogLevel.Information)
                || (warlog && (temp as Logger_Data).LOG_Level == LogLevel.Warning)
                || (errlog && (temp as Logger_Data).LOG_Level == LogLevel.Error)
                || (crilog && (temp as Logger_Data).LOG_Level == LogLevel.Critical))
                {
                    checkbox = true;
                }

                return checkbox;
            };

            viewdatagrid.Refresh();
        }

        /// <summary>
        /// DataGrid Filter 기능 사용 - Option: CheckBox + SearchText
        /// </summary>
        /// <param name="logs"></param>
        /// <param name="alllog"></param>
        /// <param name="inflog"></param>
        /// <param name="warlog"></param>
        /// <param name="errlog"></param>
        /// <param name="ftllog"></param>
        /// <param name="searchItem"></param>
        public void Log_Option(ObservableCollection<Logger_Data> logs, bool alllog, bool inflog, bool warlog, bool errlog, bool crilog, string searchItem)
        {
            var viewdatagrid = CollectionViewSource.GetDefaultView(logs);

            viewdatagrid.Filter = temp =>
            {
                bool checkbox = false;
                bool search = false;

                if (alllog
                || (inflog && (temp as Logger_Data).LOG_Level == LogLevel.Information)
                || (warlog && (temp as Logger_Data).LOG_Level == LogLevel.Warning)
                || (errlog && (temp as Logger_Data).LOG_Level == LogLevel.Error)
                || (crilog && (temp as Logger_Data).LOG_Level == LogLevel.Critical))
                {
                    checkbox = true;
                }

                if ((temp as Logger_Data).LOG_Msg.ToUpper().Contains(searchItem.ToUpper())
                || (temp as Logger_Data).LOG_Level.ToString().ToUpper().Contains(searchItem.ToUpper())
                || (temp as Logger_Data).LOG_DateTime.Contains(searchItem))
                {
                    search = true;
                }

                return checkbox && search;
            };

            viewdatagrid.Refresh();
        }
    
        public void DBDisconnector()
        {
            if (Server_List.Count > 0)
            {
                Server_List.ToList().ForEach(list => list.DB_List.ToList().ForEach(dblist =>
                {
                    _IDBConnect.DBDisconnect(dblist.DBSQL);
                }));
            }
        }

        private DispatcherTimer _Timer;
        public void Check_Ping()
        {
            _Timer = new DispatcherTimer();
            _Timer.Interval = TimeSpan.FromSeconds(3);
            _Timer.Tick += Check_STEP;

            _Timer.Start();
        }

        // LOG 이름에 [log level] 불필요
        private async void Check_STEP(object sender, EventArgs e)
        {
            switch (STEP_State)
            {
                case DB_STEP_State.BASE:
                    for (int stepBase = 0; stepBase < 3; stepBase++)
                    {
                        var allResult = Server_List.Where(server => server.PowerShell_State == true)
                                .SelectMany(serverList => serverList.DB_List)
                                .Where(step => step.DB_StepState == DB_Info.DB_STEP_State.BASE || step.DB_StepState == DB_Info.DB_STEP_State.WARNING)
                                .Select(dbInfo =>
                                {
                                    _Logger.CreateLog(LogLevel.Information, $"DB Name: {dbInfo.DBName} - PING 통신 시작");
                                    bool checkBase = _IStep.STEP_Base(dbInfo);
                                    _Logger.CreateLog(LogLevel.Information, $"DB Name: {dbInfo.DBName} - PING 통신 결과: {(checkBase ? "양호" : "불량")}");

                                    return new { isQuery = checkBase };
                                }).ToList(); // tolist 안해두면 ui 업데이트 반영이 안됨 ui 결과가 밀려버림

                        if (allResult.All(x => x.isQuery == true))
                        {
                            STEP_State = DB_STEP_State.BASE;
                            break;
                        }
                        else if (stepBase == 2)
                        {
                            STEP_State = DB_STEP_State.WARNING;
                        }
                    }
                    
                    break;

                case DB_STEP_State.WARNING:
                    for (int stepWarning = 0; stepWarning < 10; stepWarning++)
                    {
                        var allResult = Server_List.Where(server => server.PowerShell_State == true)
                                .SelectMany(serverList => serverList.DB_List)
                                    .Where(step => step.DB_StepState == DB_Info.DB_STEP_State.ERROR || step.DB_StepState == DB_Info.DB_STEP_State.WARNING)
                                        .Select(dbInfo =>
                                        {
                                            _Logger.CreateLog(LogLevel.Warning, $"DB Name: {dbInfo.DBName} - SQL Client Connection 확인 시작");
                                            bool checkWarning = _IStep.STEP_Warning(dbInfo);
                                            _Logger.CreateLog(LogLevel.Warning, $"DB Name: {dbInfo.DBName} - SQL Client Connection 확인 결과: {(checkWarning ? "양호" : "불량")}");

                                            return new { isQuery = checkWarning };
                                        }).ToList();

                        if (allResult.All(x => x.isQuery == true))
                        {
                            STEP_State = DB_STEP_State.BASE;
                            break;
                        }
                        else if (stepWarning == 9)
                        {
                            STEP_State = DB_STEP_State.ERROR;
                        }
                    }
                    
                    break;

                case DB_STEP_State.ERROR:
                    for (int stepError = 0; stepError < 10; stepError++)
                    {
                        var findList = Server_List.Where(server => server.PowerShell_State == true)
                                .Where(db => db.DB_List
                                    .Any(x => x.DB_StepState == DB_Info.DB_STEP_State.ERROR || x.DB_StepState == DB_Info.DB_STEP_State.WARNING)).ToList();

                        var allResult = findList
                            .Select(async serverList =>
                            {
                                _Logger.CreateLog(LogLevel.Error, $"Server [IP: {serverList.Server_IP}, PORT: {serverList.Server_PORT}] - Network cmd 통신 시작");
                                bool checkError = await _IStep.STEP_Error(serverList);
                                _Logger.CreateLog(LogLevel.Error, $"Server [IP: {serverList.Server_IP}, PORT: {serverList.Server_PORT}] - Network cmd 통신 결과: {(checkError ? "양호" : "불량")}");

                                return new { isQuery = checkError };
                            }).ToList();

                        var taskError = await Task.WhenAll(allResult);

                        if (taskError.All(x => x.isQuery == true))
                        {
                            STEP_State = DB_STEP_State.BASE;
                            break;
                        }
                        else if (stepError == 9)
                        {
                            STEP_State = DB_STEP_State.CRITICAL;
                        }
                    }
                    
                    break;

                case DB_STEP_State.CRITICAL:
                    for (int stepCritical = 0; stepCritical < 10; stepCritical++)
                    {
                        var findList = Server_List.Where(db => db.DB_List
                                    .Any(x => x.DB_StepState == DB_Info.DB_STEP_State.CRITICAL)).ToList();

                        var allResult = findList
                            .Select(async serverList =>
                            {
                                _Logger.CreateLog(LogLevel.Critical, $"Server [IP: {serverList.Server_IP}, PORT: {serverList.Server_PORT}] - Power Shell 통신 시작");
                                bool checkCritical = await _IStep.STEP_Critical(serverList);
                                _Logger.CreateLog(LogLevel.Critical, $"Server [IP: {serverList.Server_IP}, PORT: {serverList.Server_PORT}] - Power Shell 통신 결과 : {(checkCritical ? "양호" : "불량")}");

                                return new { isQuery = checkCritical };
                            }).ToList();

                        var taskCritical = await Task.WhenAll(allResult);

                        if (taskCritical.All(x => x.isQuery == true))
                        {
                            STEP_State = DB_STEP_State.BASE;
                            break;
                        }
                        else if (stepCritical == 9)
                        {
                            _Logger.CreateLog(LogLevel.Critical, $"모든 DB 이상 상태 확인 완료, Critical 상태가 지속 확인되어 Auto Connection 확인을 중지합니다.");
                            State_Auto(false);
                            STEP_State = DB_STEP_State.BASE;
                        }
                    }

                    break;
            }
        }
        
        public void Stop_STEP()
        {
            _Logger.CreateLog(LogLevel.Information, "Auto Check Timer STOP");
            State_Auto(false); // model에서 직접 viewmodel로 속성값을 바꿔서는 안됨
            _Timer.Stop();
            STEP_State = DB_STEP_State.BASE;
        }

        // model -> viewmodel로 속성 값 변경 이벤트 전달
        public void State_Auto(bool state)
        {
            State_AutoCheck.Invoke(state);
        }

        public async void DB_Retry(int serverIndex, DB_Info dbList)
        {
            Server_Info findServer = Server_List[serverIndex];
            DB_Info tempDB_List = dbList;

            switch (tempDB_List.DB_StepState)
            {
                case DB_Info.DB_STEP_State.BASE:

                    _Logger.CreateLog(LogLevel.Information, $"DB Name: {tempDB_List.DBName} - PING 통신 시작");
                    bool checkBase = _IStep.STEP_Base(tempDB_List);
                    _Logger.CreateLog(LogLevel.Information, $"DB Name: {tempDB_List.DBName} - PING 통신 결과: {(checkBase ? "양호" : "불량")}");

                    break;

                case DB_Info.DB_STEP_State.WARNING:

                    _Logger.CreateLog(LogLevel.Warning, $"DB Name: {tempDB_List.DBName} - SQL Client Connection 확인 시작");
                    bool checkWarning = _IStep.STEP_Warning(tempDB_List);
                    _Logger.CreateLog(LogLevel.Warning, $"DB Name: {tempDB_List.DBName} - SQL Client Connection 확인 결과: {(checkWarning ? "양호" : "불량")}");

                    break;

                case DB_Info.DB_STEP_State.ERROR:

                    _Logger.CreateLog(LogLevel.Error, $"Server [IP: {findServer.Server_IP}, PORT: {findServer.Server_PORT}] - Network cmd 통신 시작");
                    bool checkError = await _IStep.STEP_Error(findServer);
                    _Logger.CreateLog(LogLevel.Error, $"Server [IP: {findServer.Server_IP}, PORT: {findServer.Server_PORT}] - Network cmd 통신 결과: {(checkError ? "양호" : "불량")}");

                    break;

                case DB_Info.DB_STEP_State.CRITICAL:

                    _Logger.CreateLog(LogLevel.Critical, $"Server [IP: {findServer.Server_IP}, PORT: {findServer.Server_PORT}] - Power Shell 통신 시작");
                    bool checkCritical = await _IStep.STEP_Critical(findServer);
                    _Logger.CreateLog(LogLevel.Critical, $"Server [IP: {findServer.Server_IP}, PORT: {findServer.Server_PORT}] - Power Shell 통신 결과 : {(checkCritical ? "양호" : "불량")}");

                    break;
            }
        }

        #endregion
    }
}
