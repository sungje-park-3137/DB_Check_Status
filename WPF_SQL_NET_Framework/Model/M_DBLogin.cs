using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WPF_SQL_NET_Framework.SingleTon;
using WPF_SQL_NET_Framework.Interface;
using WPF_SQL_NET_Framework.Info;

namespace WPF_SQL_NET_Framework.Model
{
    public class M_DBLogin
    {
        private readonly csServerList _ServerList;
        private readonly csLogger _Logger;
        private readonly IDBConnect _IDBConnect;
        private readonly IServerList _IServerList;

        private ObservableCollection<Server_Info> Server_List
        {
            get { return _ServerList.Server_Lists; }
        }

        public M_DBLogin()
        {
            _IDBConnect = new DBFunc();
            _IServerList = new LISTFunc();

            _ServerList = csServerList.GetInstance();
            _Logger = csLogger.GetInstance();
        }

        public void Add_List()
        {
            Server_List.Add(_IServerList.Init_DBInfo(Server_List.Count()));
        }

        public void Del_List(int selectIndex)
        {
            if (selectIndex >= 0)
            {
                Server_List.RemoveAt(selectIndex);
                for (int i = 0; i < Server_List.Count; i++)
                {
                    Server_List[i].TabName = $"DB_Server_{i + 1}";
                }
            }
        }

        public async Task<bool> DBConnector()
        {
            try
            {
                if (!Check_Data(Server_List))
                {
                    return false;
                }

                var task = Server_List.Where(s => s.PowerShell_State == true).SelectMany(select =>
                select.DB_List.Select(async d =>
                {
                    _Logger.CreateLog(LogLevel.Information, $"[SQL Client] 통신 확인 시작 - DB Name: {d.DBName}");

                    string conStr = _IDBConnect.SQLConnStr(select.Server_IP, select.Server_PORT, d.DBName, select.Server_ID, select.Server_PW);

                    bool connect = await _IDBConnect.DBConnect(select.Server_IP, select.Server_PORT, select.Server_ID, select.Server_PW, conStr, d.DBSQL);

                    d.DBBrush = connect ? Brushes.Green : Brushes.Orange;
                    d.DB_StepState = connect ? DB_Info.DB_STEP_State.BASE : DB_Info.DB_STEP_State.WARNING;

                    _Logger.CreateLog(LogLevel.Information, $"[SQL Client] 통신 확인 종료 - DB Name: {d.DBName}, 결과: {(connect ? "양호" : "불량")}]");

                    return new { isConnected = connect };
                }));

                var result = await Task.WhenAll(task);

                if (result.All(x => x.isConnected == false))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("DBConnector 함수 오류 발생: " + ex.Message);
                return false;
            }
            
        }

        public async Task<bool> Check_PoewerShell()
        {
            try
            {
                if (!Check_Data(Server_List))
                {
                    return false;
                }

                // select: 상위 컬렉션 task list 
                // select many: 상위 -> 하위 컬렉션 task까지 모두 담을 수 있음
                // 따라서 지금은 server의 ip, port의 해당하는 상위 컬렉션만 사용하므로 -> select
                var task = Server_List.Select(async s =>
                {
                    _Logger.CreateLog(LogLevel.Information, $"[PowerShell] 통신 확인 시작 - Server IP: {s.Server_IP}, PORT: {s.Server_PORT}");
                    bool connect = await _IDBConnect.DB_PowerShell(s.Server_IP, s.Server_PORT);
                    s.PowerShell_State = connect;

                    if (connect)
                    {
                        s.DB_List.ToList().ForEach(db =>
                        {
                            db.DBBrush = Brushes.Orange;
                            db.DB_StepState = DB_Info.DB_STEP_State.WARNING;
                        });
                        
                        _Logger.CreateLog(LogLevel.Information, $"[PowerShell] 통신 확인 종료 - Server IP: {s.Server_IP}, PORT: {s.Server_PORT}, 결과: 양호");
                    }
                    else
                    {
                        s.DB_List.ToList().ForEach(db =>
                        {
                            db.DBBrush = Brushes.Black;
                            db.DB_StepState = DB_Info.DB_STEP_State.CRITICAL;
                        });
                                                
                        _Logger.CreateLog(LogLevel.Critical, $"[PowerShell] 통신 확인 종료 - Server IP: {s.Server_IP}, PORT: {s.Server_PORT}, 결과: 불량");
                    }

                    return new { Server_Info = s, psConnected = connect };
                });

                var result = await Task.WhenAll(task);

                if (result.All(x => x.psConnected == false))
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Check_PowerShell 함수 오류 발생: {ex.Message}");
                return false;
            }
        }

        private bool Check_Data(ObservableCollection<Server_Info> checklist)
        {
            try
            {
                foreach (var item in checklist)
                {
                    if (string.IsNullOrEmpty(item.Server_IP))
                    {
                        throw new Exception("IP 값을 입력하세요");
                    }
                    if (item.Server_PORT <= 0)
                    {
                        throw new Exception("PORT 값을 입력하세요");
                    }
                    if (string.IsNullOrEmpty(item.Server_ID))
                    {
                        throw new Exception("ID를 입력하세요");
                    }
                    if (string.IsNullOrEmpty(item.Server_PW))
                    {
                        throw new Exception("PW를 입력하세요");
                    }
                    if (item.DB_List.Count <= 0)
                    {
                        throw new Exception("DB 정보를 입력하세요");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
    }
}
