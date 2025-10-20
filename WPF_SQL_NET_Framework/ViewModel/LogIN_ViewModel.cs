using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using WPF_SQL_NET_Framework.Command;
using WPF_SQL_NET_Framework.Model;

namespace WPF_SQL_NET_Framework.ViewModel
{
    public partial class Main_ViewModel : NotifyProperty
    {
        // DB LOG IN : Function
        //private void Init_DB() // .config단으로 내려라...? 무슨 의미일까?
        //{
        //    Main_Model mainInfo = new Main_Model
        //    {
        //        Name = $"DB_Server_{((DB_Info.Count() <= 0) ? 1 : DB_Info.Count() + 1)}",
        //        IP = "192.168.0.244",
        //        PORT = 1433,
        //        DB_UserName = "sa",
        //        DB_Password = "Admin123",
        //        DBInfos = new ObservableCollection<DB_Data>
        //        {
        //            new DB_Data { DBName = "GS425_IRE_DB" },
        //            new DB_Data { DBName = "GS425_IEE_DB" },
        //            new DB_Data { DBName = "IEE_TERRAIN" }
        //        }
        //    };

        //    DB_Info.Add(mainInfo);
        //}



        // DB LOG IN: Control
        private async Task<bool> Check_DBConnect()
        {
            if (!Check_Data(Server_List))
            {
                return false;
            }

            var task = Server_List.Where(s => s.PowerShell_State == true).SelectMany(select =>
            select.DB_List.Select(async d =>
            {
                _logger.CreateLog(LogLevel.Information, $"[SQL Client] 통신 확인 시작 - DB Name: {d.DBName}");

                string conStr = _IDB.SQLConnStr(select.Server_IP, select.Server_PORT, d.DBName, select.Server_ID, select.Server_PW);

                bool connect = await _IDB.DBConnect(select.Server_IP, select.Server_PORT, select.Server_ID, select.Server_PW, conStr, d.DBSQL);

                d.DBBrush = connect ? Brushes.Green : Brushes.Orange;
                d.DB_StepState = connect ? DB_Info.DB_STEP_State.BASE : DB_Info.DB_STEP_State.WARNING;

                _logger.CreateLog(LogLevel.Information, $"[SQL Client] 통신 확인 종료 - DB Name: {d.DBName}, 결과: {(connect ? "양호" : "불량")}]");

                return new { Server_Info = select, DB_Info = d, isConnected = connect };
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

        private async Task<bool> Check_PoewerShell()
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
                    _logger.CreateLog(LogLevel.Information, $"[PowerShell] 통신 확인 시작 - Server IP: {s.Server_IP}, PORT: {s.Server_PORT}");
                    bool connect = await _IDB.DB_PowerShell(s.Server_IP, s.Server_PORT);
                    s.PowerShell_State = connect;
                    s.DB_List.ToList().ForEach(x => x.DBBrush = connect ? Brushes.Orange : Brushes.Black);
                    s.DB_List.ToList().ForEach(db => db.DB_StepState = connect ? DB_Info.DB_STEP_State.WARNING : DB_Info.DB_STEP_State.CRITICAL);
                    _logger.CreateLog(LogLevel.Information, $"[PowerShell] 통신 확인 종료 - Server IP: {s.Server_IP}, PORT: {s.Server_PORT}, 결과: {(connect ? "양호" : "불량")}]");

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
                MessageBox.Show($"[Check PowerShell] 실패: {ex.Message}");
                return false;
            }
        }

        private bool Check_Data(ObservableCollection<Server_Info> DBList)
        {
            try
            {
                foreach (var item in DBList)
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
