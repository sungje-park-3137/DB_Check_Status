using Microsoft.Extensions.Logging;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using WPF_SQL_NET_Framework.Command;
using WPF_SQL_NET_Framework.Interface;
using WPF_SQL_NET_Framework.Model;

namespace WPF_SQL_NET_Framework.ViewModel
{
    public partial class Main_ViewModel : NotifyProperty
    {
        private ObservableCollection<LogEvent> _logevents = new ObservableCollection<LogEvent>();
        public ObservableCollection<LogEvent> Logevent
        {
            get { return _logevents; }
            set { _logevents = value; OnPropertyChanged(nameof(Logevent)); }
        }

        private bool _alllog = true;
        public bool ALL_Log
        {
            get { return _alllog; }
            set { _alllog = value; OnPropertyChanged(nameof(ALL_Log)); Log_Option(); }
        }

        private bool _inflog = false;
        public bool INF_Log
        {
            get { return _inflog; }
            set { _inflog = value; OnPropertyChanged(nameof(INF_Log)); Log_Option(); }
        }

        private bool _warlog = false;
        public bool WAR_Log
        {
            get { return _warlog; }
            set { _warlog = value; OnPropertyChanged(nameof(WAR_Log)); Log_Option(); }
        }

        private bool _errlog = false;
        public bool ERR_Log
        {
            get { return _errlog; }
            set { _errlog = value; OnPropertyChanged(nameof(ERR_Log)); Log_Option(); }
        }

        private bool _ftllog = false;
        public bool FTL_Log
        {
            get { return _ftllog; }
            set { _ftllog = value; OnPropertyChanged(nameof(FTL_Log)); Log_Option(); }
        }

        private string _Search_Txt = string.Empty;
        public string Search_Txt
        {
            get { return _Search_Txt; }
            set { _Search_Txt = value; OnPropertyChanged(nameof(Search_Txt)); }
        }


        private void Log_Option()
        {
            // 개념-filter 동작: 람다로 조건문에 부합하면 true를 반환 -> true로 반환 된 데이터만 show
            // 지금 구조 문제: filter 조건 먼저 확인 -> 내용 수행
            // -> 변수 하나 참조 값으로 빼서 해당 값에 true를 누적한다
            // -> 가 아니라 반환 값이 true 가 핵심임 어짜피 event 들어오면 다음 if 문 돌아가고 누적 됨
            var viewdatagrid = CollectionViewSource.GetDefaultView(Logevent);

            viewdatagrid.Filter = temp =>
            {
                if (ALL_Log)
                {
                    return true;
                }
                if (INF_Log && (temp as LogEvent).Level == LogEventLevel.Information)
                {
                    return true;
                }
                if (WAR_Log && (temp as LogEvent).Level == LogEventLevel.Warning)
                {
                    return true;
                }
                if (ERR_Log && (temp as LogEvent).Level == LogEventLevel.Error)
                {
                    return true;
                }
                if (FTL_Log && (temp as LogEvent).Level == LogEventLevel.Fatal)
                {
                    return true;
                }


                return false;
            };

            viewdatagrid.Refresh();
        }

        private void Log_Option(string searchItem)
        {
            var viewdatagrid = CollectionViewSource.GetDefaultView(Logevent);

            viewdatagrid.Filter = temp =>
            {
                bool checkbox = false;
                bool search = false;

                if (ALL_Log 
                || (INF_Log && (temp as LogEvent).Level == LogEventLevel.Information) || (WAR_Log && (temp as LogEvent).Level == LogEventLevel.Warning)
                || (ERR_Log && (temp as LogEvent).Level == LogEventLevel.Error) || (FTL_Log && (temp as LogEvent).Level == LogEventLevel.Fatal))
                {
                    checkbox = true;
                }

                if ((temp as LogEvent).MessageTemplate.ToString().Contains(searchItem))
                {
                    search = true;
                }


                return checkbox && search;
            };

            viewdatagrid.Refresh();
        }



        // DB Query: Control
        private DispatcherTimer _Timer;
        private void Check_Ping()
        {
            if (Check_List() == false)
            {
                return;
            }

            _Timer = new DispatcherTimer();
            _Timer.Interval = TimeSpan.FromSeconds(3);
            _Timer.Tick += TimerONAsync;
            _Timer.Start();
        }

        private async void TimerONAsync(object sender, EventArgs e)
        {
            switch (STEP_State)
            {
                case DB_STEP_State.BASE:

                    for (int stepBase = 0; stepBase < 3; stepBase++)
                    {
                        // 조건 문을 추가 한 이유
                        /*
                         * 어떤 리스트만 뽑아와야하냐? 부정응답이 온 리스트만 간추려야한다.
                         * 뭐가문제냐? 최초에 실행 시에는 경고,오류 리스트를 다담고 한번 검사 한뒤 추려야함
                         * 구현 해야하는 요소는 ? 검사를 한뒤 추린다
                         * -> 검사를 했다 라는 걸 알아야함 
                         */
                        if (stepBase < 1)
                        {
                            var allResult = Server_List.Where(server => server.PowerShell_State == true)
                                .SelectMany(serverList => serverList.DB_List)
                                    .Where(step => step.DB_StepState == DB_Info.DB_STEP_State.BASE || step.DB_StepState == DB_Info.DB_STEP_State.WARNING)
                                        .Select(dbInfo =>
                                        {
                                            _logger.CreateLog(LogLevel.Information, $"[BASE] DB Name: {dbInfo.DBName} - PING 통신 시작");
                                            bool checkBase = _IDB.STEP_Base(dbInfo);
                                            _logger.CreateLog(LogLevel.Information, $"[BASE] DB Name: {dbInfo.DBName} - PING 통신 결과: {(checkBase ? "양호" : "불량")}");

                                            return new { isQuery = checkBase };
                                        }).ToList(); // tolist 안해두면 ui 업데이트 반영이 안됨 ui 결과가 밀려버림

                            if (allResult.All(x => x.isQuery == true))
                            {
                                STEP_State = DB_STEP_State.BASE;
                                break;
                            }
                        }
                        else
                        {
                            var allResult = Server_List.Where(server => server.PowerShell_State == true)
                                .SelectMany(serverList => serverList.DB_List)
                                    .Where(step => step.DB_StepState == DB_Info.DB_STEP_State.WARNING)
                                        .Select(dbInfo =>
                                        {
                                            _logger.CreateLog(LogLevel.Information, $"[BASE] DB Name: {dbInfo.DBName} - PING 통신 시작");
                                            bool checkBase = _IDB.STEP_Base(dbInfo);
                                            _logger.CreateLog(LogLevel.Information, $"[BASE] DB Name: {dbInfo.DBName} - PING 통신 결과: {(checkBase ? "양호" : "불량")}");
                                            
                                            return new { isQuery = checkBase };
                                        }).ToList();

                            if (allResult.All(x => x.isQuery == false))
                            {
                                if (stepBase == 2)
                                {
                                    STEP_State = DB_STEP_State.WARNING;
                                }
                            }
                        }
                    }

                    break;

                case DB_STEP_State.WARNING:
                    for (int stepWarning = 0; stepWarning < 10; stepWarning++)
                    {
                        // 조건 문을 추가 한 이유
                        /*
                         * 어떤 리스트만 뽑아와야하냐? 부정응답이 온 리스트만 간추려야한다.
                         * 뭐가문제냐? 최초에 실행 시에는 경고,오류 리스트를 다담고 한번 검사 한뒤 추려야함
                         * 구현 해야하는 요소는 ? 검사를 한뒤 추린다
                         * -> 검사를 했다 라는 걸 알아야함 
                         */
                        if (stepWarning < 1)
                        {
                            var allResult = Server_List.Where(server => server.PowerShell_State == true)
                                .SelectMany(serverList => serverList.DB_List)
                                    .Where(step => step.DB_StepState == DB_Info.DB_STEP_State.ERROR || step.DB_StepState == DB_Info.DB_STEP_State.WARNING)
                                        .Select(dbInfo =>
                                        {
                                            _logger.CreateLog(LogLevel.Warning, $"[WARNING] DB Name: {dbInfo.DBName} - SQL Client Connection 확인 시작");
                                            bool checkWarning = _IDB.STEP_Warning(dbInfo);
                                            _logger.CreateLog(LogLevel.Warning, $"[WARNING] DB Name: {dbInfo.DBName} - SQL Client Connection 확인 결과: {(checkWarning ? "양호" : "불량")}");

                                            return new { isQuery = checkWarning };
                                        }).ToList();

                            if (allResult.All(x => x.isQuery == true))
                            {
                                STEP_State = DB_STEP_State.BASE;
                                break;
                            }
                        }
                        else
                        {
                            var allResult = Server_List.Where(server => server.PowerShell_State == true)
                                .SelectMany(serverList => serverList.DB_List)
                                    .Where(step => step.DB_StepState == DB_Info.DB_STEP_State.WARNING)
                                        .Select(dbInfo =>
                                        {
                                            _logger.CreateLog(LogLevel.Warning, $"[WARNING] DB Name: {dbInfo.DBName} - SQL Client Connection 확인 시작");
                                            bool checkWarning = _IDB.STEP_Warning(dbInfo);
                                            _logger.CreateLog(LogLevel.Warning, $"[WARNING] DB Name: {dbInfo.DBName} - SQL Client Connection 확인 결과: {(checkWarning ? "양호" : "불량")}");

                                            return new { isQuery = checkWarning };
                                        }).ToList();

                            if (allResult.All(x => x.isQuery == false))
                            {
                                if (stepWarning == 9)
                                {
                                    STEP_State = DB_STEP_State.ERROR;
                                }
                            }
                        }
                    }

                    break;

                case DB_STEP_State.ERROR:

                    for (int stepError = 0; stepError < 10; stepError++)
                    {
                        if (stepError < 1)
                        {
                            var findList = Server_List.Where(server => server.PowerShell_State == true)
                                .Where(db => db.DB_List
                                    .Any(x => x.DB_StepState == DB_Info.DB_STEP_State.ERROR || x.DB_StepState == DB_Info.DB_STEP_State.WARNING)).ToList();

                            var allResult = findList
                                .Select(async serverList =>
                                {
                                    _logger.CreateLog(LogLevel.Error, $"[ERROR] Server [IP: {serverList.Server_IP}, PORT: {serverList.Server_PORT}] - Network cmd 통신 시작");
                                    bool checkError = await _IDB.STEP_Error(serverList);
                                    _logger.CreateLog(LogLevel.Error, $"[ERROR] Server [IP: {serverList.Server_IP}, PORT: {serverList.Server_PORT}] - Network cmd 통신 결과: {(checkError ? "양호" : "불량")}");

                                    return new { isQuery = checkError };
                                }).ToList();

                            var taskError = await Task.WhenAll(allResult);

                            if (taskError.All(x => x.isQuery == true))
                            {
                                STEP_State = DB_STEP_State.BASE;
                                break;
                            }
                        }
                        else
                        {
                            var findList = Server_List.Where(server => server.PowerShell_State == true)
                                .Where(db => db.DB_List
                                    .Any(x => x.DB_StepState == DB_Info.DB_STEP_State.ERROR)).ToList();

                            var allResult = findList
                                .Select(async serverList =>
                                {
                                    _logger.CreateLog(LogLevel.Error, $"[ERROR] Server [IP: {serverList.Server_IP}, PORT: {serverList.Server_PORT}] - Network cmd 통신 시작");
                                    bool checkError = await _IDB.STEP_Error(serverList);
                                    _logger.CreateLog(LogLevel.Error, $"[ERROR] Server [IP: {serverList.Server_IP}, PORT: {serverList.Server_PORT}] - Network cmd 통신 결과: {(checkError ? "양호" : "불량")}");

                                    return new { isQuery = checkError };
                                }).ToList();

                            var taskError = await Task.WhenAll(allResult);

                            if (taskError.All(x => x.isQuery == false))
                            {
                                if (stepError == 9)
                                {
                                    STEP_State = DB_STEP_State.CRITICAL;
                                }
                            }
                        }
                    }

                    break;

                case DB_STEP_State.CRITICAL:
                    for (int stepCritical = 0; stepCritical < 10; stepCritical++)
                    {
                        if (stepCritical < 1)
                        {
                            var findList = Server_List.Where(db => db.DB_List
                                    .Any(x => x.DB_StepState == DB_Info.DB_STEP_State.ERROR || x.DB_StepState == DB_Info.DB_STEP_State.CRITICAL)).ToList();

                            var allResult = findList
                                .Select(async serverList =>
                                {
                                    _logger.CreateLog(LogLevel.Critical, $"[CRITICAL] Server [IP: {serverList.Server_IP}, PORT: {serverList.Server_PORT}] - Power Shell 통신 시작");
                                    bool checkCritical = await _IDB.STEP_Critical(serverList);
                                    _logger.CreateLog(LogLevel.Critical, $"[CRITICAL] Server [IP: {serverList.Server_IP}, PORT: {serverList.Server_PORT}] - Power Shell 통신 결과 : {(checkCritical ? "양호" : "불량")}");

                                    return new { isQuery = checkCritical };
                                }).ToList();

                            var taskCritical = await Task.WhenAll(allResult);

                            if (taskCritical.All(x => x.isQuery == true))
                            {
                                STEP_State = DB_STEP_State.BASE;
                                break;
                            }
                        }
                        else
                        {
                            var findList = Server_List.Where(db => db.DB_List
                                    .Any(x => x.DB_StepState == DB_Info.DB_STEP_State.CRITICAL)).ToList();

                            var allResult = findList
                                .Select(async serverList =>
                                {
                                    _logger.CreateLog(LogLevel.Critical, $"[CRITICAL] Server [IP: {serverList.Server_IP}, PORT: {serverList.Server_PORT}] - Power Shell 통신 시작");
                                    bool checkCritical = await _IDB.STEP_Critical(serverList);
                                    _logger.CreateLog(LogLevel.Critical, $"[CRITICAL] Server [IP: {serverList.Server_IP}, PORT: {serverList.Server_PORT}] - Power Shell 통신 결과 : {(checkCritical ? "양호" : "불량")}");

                                    return new { isQuery = checkCritical };
                                }).ToList();

                            var taskCritical = await Task.WhenAll(allResult);

                            if (taskCritical.All(x => x.isQuery == false))
                            {
                                if (stepCritical == 9)
                                {
                                    _logger.CreateLog(LogLevel.Critical, $"모든 DB 이상 상태 확인 완료, Critical 상태가 지속 확인되어 Auto Connection 확인을 중지합니다.");
                                    AutoCheck = false;
                                    STEP_State = DB_STEP_State.BASE;
                                }
                            }
                        } 
                    }

                    break;

                default:
                    AutoCheck = false;
                    STEP_State = DB_STEP_State.BASE;

                    break;
            }
        }

        private void TimerOFF()
        {
            _logger.CreateLog(LogLevel.Information, "Auto Check Timer STOP");
            AutoCheck = false;
            _Timer.Stop();
            STEP_State = DB_STEP_State.BASE;
        }

        

        private DB_STEP_State STEP_State = DB_STEP_State.BASE;

        public enum DB_STEP_State
        {
            BASE,
            WARNING,
            ERROR,
            CRITICAL
        }

        private void Check_Query_DBInfo()
        {
            for (int i = 0; i < 2; i++)
            {
                string tempQuery = string.Empty;

                if (i == 0)
                {
                    //
                    tempQuery = @"SELECT SERVERPROPERTY('ServerName') AS ServerName, sqlserver_start_time AS Uptime FROM sys.dm_os_sys_info;";

                    // foreach 중 break; 사용할땐 -> for,foreach 사용
                    foreach (var server in Server_List)
                    {
                        server.DB_Server_RcvTable.Clear();

                        foreach (var db in server.DB_List)
                        {
                            if (_IDB.SendQuery_DataAdapter(tempQuery, db.DBSQL, out DataTable rcvdatatable))
                            {
                                server.DB_Server_RcvTable = rcvdatatable;
                                break;
                            }
                            else
                            {

                            }
                        }
                    }
                }
                else
                {
                    tempQuery = @"SELECT DB_NAME(database_id) AS DatabaseName," +
                        "SUM(num_of_reads) AS Reads,SUM(num_of_writes) AS Writes," +
                        "SUM(num_of_bytes_read)/1024/1024 AS MB_Read, " +
                        "SUM(num_of_bytes_written)/1024/1024 AS MB_Written FROM sys.dm_io_virtual_file_stats(NULL, NULL) " +
                        "GROUP BY database_id " +
                        "ORDER BY (SUM(num_of_reads) + SUM(num_of_writes)) DESC;";

                    // foreach 중 break; 사용할땐 -> for,foreach 사용
                    foreach (var server in Server_List)
                    {
                        server.DB_RcvTable.Clear();

                        foreach (var db in server.DB_List)
                        {
                            if (_IDB.SendQuery_DataAdapter(tempQuery, db.DBSQL, out DataTable rcvdatatable))
                            {
                                server.DB_RcvTable = rcvdatatable;
                                break;
                            }
                            else
                            {

                            }
                        }
                    }
                }

               // LINQ
                //var temp = Server_List.SelectMany(x => x.DB_List.Select(y =>
                //{
                //    bool receive = _IDB.SendQuery_DataAdapter(tempQuery, y.DBSQL, out DataTable rcvdatatable);

                //    x.DB_Server_RcvTable = receive ? rcvdatatable : null;
                //    y.DBBrush = receive ? Brushes.Green : Brushes.Orange;

                //    return new { Server_Info = x, DB_Info = y, receive };
                //})).ToList();

                


                // waitall: 동기
                // whenall: 비동기
            }
        }

        // Common: Check
        private bool Check_List()
        {
            try
            {
                if (Server_List.Count() <= 0)
                {
                    throw new Exception("SQL Server List의 정보가 존재하지 않습니다.");
                }

                List<DB_Info> dbList = Server_List.SelectMany(x => x.DB_List).ToList();

                if (dbList.Count <= 0)
                {
                    throw new Exception("DB 정보의 데이터가 존재하지 않습니다.");
                }
                if (dbList.All(x => x.DBSQL.State != ConnectionState.Open) || dbList.All(y => y.DBSQL == null))
                {
                    throw new Exception("DB와 연결된 데이터가 존재하지 않습니다. Query를 전송할 DB와 연결해주세요.");
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"[Data 내용 확인] 실패: {ex.Message}");
                return false;
            }
        }




        // STEP 별 로직 관련 함수
        // Interface vs Partial
        // 일단 둘다 구현
        // -> (구현 완)parital : 유지보수가 어렵다
        // -> (구현 중)Interface : 유지보수가 쉽다, 기능들만 모아서 관리 가능 -> 더 좋은거같음

        // partial 구현
        /*/// <summary>
        /// [시작, Base] 응답이 왔는가?
        /// </summary>
        /// <param name="dbinfo"></param>
        /// <returns>
        /// true: 왔다(DB_StepState = Base), false: 안왔다(DB_StepState = Warning)
        /// </returns>
        private bool STEP_Base(DB_Info dbinfo)
        {
            try
            {
                if (dbinfo.DB_StepState == DB_Info.DB_STEP_State.ERROR || dbinfo.DB_StepState == DB_Info.DB_STEP_State.CRITICAL)
                {
                    return false;
                }

                AppendLog($"[BASE] DB Name: {dbinfo.DBName} - PING 통신 시작");

                dbinfo.DBBrush = Brushes.White;

                bool receive = _IDB.SendQuery_DataReader("SELECT 1", dbinfo.DBSQL, out List<string> data);
                string rcvdata = string.Join(",", data);

                AppendLog($"[BASE] Query: SELECT 1, RCV: {rcvdata}");

                if (receive)
                {
                    if (rcvdata.Equals("1"))
                    {
                        dbinfo.DBBrush = Brushes.Green;
                        dbinfo.DB_StepState = DB_Info.DB_STEP_State.BASE;
                        AppendLog($"[BASE] DB Name: {dbinfo.DBName} - PING 통신 결과: 양호");
                        //AppendLog($"[BASE] DB Name: {dbinfo.DBName} - PING 통신 종료");
                        return true;
                    }
                    else
                    {
                        dbinfo.DBBrush = Brushes.Orange;
                        dbinfo.DB_StepState = DB_Info.DB_STEP_State.WARNING;
                        AppendLog($"[BASE] DB Name: {dbinfo.DBName} - PING 통신 결과: 불량");
                        //AppendLog($"[BASE] DB Name: {dbinfo.DBName} - PING 통신 종료");
                        return false;
                    }
                }
                else
                {
                    dbinfo.DBBrush = Brushes.Orange;
                    dbinfo.DB_StepState = DB_Info.DB_STEP_State.WARNING;
                    AppendLog($"[BASE] DB Name: {dbinfo.DBName} - PING 통신 결과: 불량");
                    //AppendLog($"[BASE] DB Name: {dbinfo.DBName} - PING 통신 종료");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// [경고, Warning] 응답이 왔는가?
        /// </summary>
        /// <param name="dbinfo"></param>
        /// <returns>
        /// true: 왔다(DB_StepState = Base), false: 안왔다(DB_StepState = Warning)
        /// </returns>
        private bool STEP_Warning(DB_Info dbinfo)
        {
            try
            {
                if (dbinfo.DB_StepState == DB_Info.DB_STEP_State.CRITICAL)
                {
                    return false;
                }

                AppendLog($"[WARNING] DB Name: {dbinfo.DBName} - SQL Client Connection 확인 시작");

                dbinfo.DBBrush = Brushes.White;

                bool sqlconnection = dbinfo.DBSQL.State == ConnectionState.Open ? true : false;

                if (sqlconnection)
                {
                    dbinfo.DBBrush = Brushes.Green;
                    dbinfo.DB_StepState = DB_Info.DB_STEP_State.BASE;
                    AppendLog($"[WARNING] DB Name: {dbinfo.DBName} - SQL Client Connection 결과 : 양호");
                    //AppendLog($"[WARNING] DB Name: {dbinfo.DBName} - SQL Client Connection 확인 종료");

                    return true;
                }
                else
                {
                    dbinfo.DBBrush = Brushes.Orange;
                    dbinfo.DB_StepState = DB_Info.DB_STEP_State.WARNING;
                    AppendLog($"[WARNING] DB Name: {dbinfo.DBName} - SQL Client Connection 결과 : 불량");
                    //AppendLog($"[WARNING] DB Name: {dbinfo.DBName} - SQL Client Connection 확인 종료");

                    return false;
                }
                    
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// [에러, Error] 응답이 왔는가?
        /// </summary>
        /// <param name="serverinfo"></param>
        /// <returns>
        /// true: 왔다(DB_StepState = Warning), false: 안왔다(DB_StepState = Error)
        /// </returns>
        private async Task<bool> STEP_Error(Server_Info serverinfo)
        {
            try
            {
                // 추가: 양호한 상태인 DB는 색상처리 제외 시키기 위해
                var findList = serverinfo.DB_List.Where(s => s.DB_StepState == DB_Info.DB_STEP_State.WARNING || s.DB_StepState == DB_Info.DB_STEP_State.ERROR).ToList();

                AppendLog($"[ERROR] Server [IP: {serverinfo.Server_IP}, PORT: {serverinfo.Server_PORT}] - Network cmd 통신 시작");

                findList.ForEach(db =>
                {
                    db.DBBrush = Brushes.White;
                });

                string rcvdata = await _IDB.DB_ProcessCMD($"SQLCMD -S {serverinfo.Server_IP},{serverinfo.Server_PORT} -U {serverinfo.Server_ID} -P {serverinfo.Server_PW} -Q \"SELECT 1\"");
                AppendLog($"[ERROR] CMD Query: SQLCMD -S {serverinfo.Server_IP},{serverinfo.Server_PORT} -U {serverinfo.Server_ID} -P {serverinfo.Server_PW} -Q \"SELECT 1\", RCV: {rcvdata}");

                if (rcvdata.Equals("1"))
                {
                    findList.ForEach(db =>
                    {
                        db.DBBrush = Brushes.Orange;
                        db.DB_StepState = DB_Info.DB_STEP_State.WARNING;
                    });

                    AppendLog($"[ERROR] Server [IP: {serverinfo.Server_IP}, PORT: {serverinfo.Server_PORT}] - Network cmd 통신 결과 : 양호");
                    return true;
                }
                else
                {
                    findList.ToList().ForEach(db =>
                    {
                        db.DBBrush = Brushes.Red;
                        db.DB_StepState = DB_Info.DB_STEP_State.ERROR;
                    });

                    AppendLog($"[ERROR] Server [IP: {serverinfo.Server_IP}, PORT: {serverinfo.Server_PORT}] - Network cmd 통신 결과 : 불량");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// [Critical] 응답이 왔는가?
        /// </summary>
        /// <param name="serverinfo"></param>
        /// <returns>
        /// true: 왔다(DB_StepState = Error), false: 안왔다(DB_StepState = Critical)
        /// </returns>
        private async Task<bool> STEP_Critical(Server_Info serverinfo)
        {
            try
            {
                // 추가: 양호한 상태인 DB는 색상처리 제외 시키기 위해
                var findList = serverinfo.DB_List.Where(s => s.DB_StepState == DB_Info.DB_STEP_State.CRITICAL || s.DB_StepState == DB_Info.DB_STEP_State.ERROR).ToList();

                AppendLog($"[CRITICAL] Server [IP: {serverinfo.Server_IP}, PORT: {serverinfo.Server_PORT}] - Power Shell 통신 시작");

                findList.ForEach(db =>
                {
                    db.DBBrush = Brushes.White;
                });

                bool psresult = await _IDB.DB_PowerShell(serverinfo.Server_IP, serverinfo.Server_PORT);

                if (psresult)
                {
                    findList.ForEach(db =>
                    {
                        db.DBBrush = Brushes.Red;
                        db.DB_StepState = DB_Info.DB_STEP_State.ERROR;
                    });

                    AppendLog($"[CRITICAL] Server [IP: {serverinfo.Server_IP}, PORT: {serverinfo.Server_PORT}] - Power Shell 통신 결과 : 양호");
                    return true;
                }
                else
                {
                    findList.ForEach(db =>
                    {
                        db.DBBrush = Brushes.Black;
                        db.DB_StepState = DB_Info.DB_STEP_State.CRITICAL;
                    });

                    AppendLog($"[CRITICAL] Server [IP: {serverinfo.Server_IP}, PORT: {serverinfo.Server_PORT}] - Power Shell 통신 결과 : 불량");
                    return false;
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }*/
    }
}
