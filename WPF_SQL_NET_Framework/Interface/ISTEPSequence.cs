using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WPF_SQL_NET_Framework.SingleTon;
using WPF_SQL_NET_Framework.Model;
using Microsoft.Extensions.Logging;
using WPF_SQL_NET_Framework.Info;

namespace WPF_SQL_NET_Framework.Interface
{
    public interface ISTEPSequence
    {
        /// <summary>
        /// [시작, Base] 응답이 왔는가?
        /// </summary>
        /// <param name="dbinfo"></param>
        /// <returns>
        /// true: 왔다(DB_StepState = Base), false: 안왔다(DB_StepState = Warning)
        /// </returns>
        bool STEP_Base(DB_Info dbinfo);

        /// <summary>
        /// [경고, Warning] 응답이 왔는가?
        /// </summary>
        /// <param name="dbinfo"></param>
        /// <returns>
        /// true: 왔다(DB_StepState = Base), false: 안왔다(DB_StepState = Warning)
        /// </returns>
        bool STEP_Warning(DB_Info dbinfo);

        /// <summary>
        /// [에러, Error] 응답이 왔는가?
        /// </summary>
        /// <param name="serverinfo"></param>
        /// <returns>
        /// true: 왔다(DB_StepState = Warning), false: 안왔다(DB_StepState = Error)
        /// </returns>
        Task<bool> STEP_Error(Server_Info serverinfo);

        /// <summary>
        /// [Critical] 응답이 왔는가?
        /// </summary>
        /// <param name="serverinfo"></param>
        /// <returns>
        /// true: 왔다(DB_StepState = Error), false: 안왔다(DB_StepState = Critical)
        /// </returns>
        Task<bool> STEP_Critical(Server_Info serverinfo);
    }

    public class STEPFunc : ISTEPSequence
    {
        private readonly IDBConnect _IDBConnect; // IDBConnect를 사용함 -> 

        public STEPFunc()
        {
            _IDBConnect = new DBFunc();
        }

        /// <summary>
        /// [시작, Base] 응답이 왔는가?
        /// </summary>
        /// <param name="dbinfo"></param>
        /// <returns>
        /// true: 왔다(DB_StepState = Base), false: 안왔다(DB_StepState = Warning)
        /// </returns>
        public bool STEP_Base(DB_Info dbinfo)
        {
            try
            {
                if (dbinfo.DB_StepState == DB_Info.DB_STEP_State.ERROR || dbinfo.DB_StepState == DB_Info.DB_STEP_State.CRITICAL)
                {
                    return false;
                }
                //AppendLog($"[BASE] DB Name: {dbinfo.DBName} - PING 통신 시작");

                dbinfo.DBBrush = Brushes.White;

                bool receive = _IDBConnect.SendQuery_DataReader("SELECT 1", dbinfo.DBSQL, out List<string> data);
                string rcvdata = string.Join(",", data);

                //AppendLog($"[BASE] Query: SELECT 1, RCV: {rcvdata}");

                if (receive)
                {
                    if (rcvdata.Equals("1"))
                    {
                        dbinfo.DBBrush = Brushes.Green;
                        dbinfo.DB_StepState = DB_Info.DB_STEP_State.BASE;
                        //AppendLog($"[BASE] DB Name: {dbinfo.DBName} - PING 통신 결과: 양호");
                        //AppendLog($"[BASE] DB Name: {dbinfo.DBName} - PING 통신 종료");
                        return true;
                    }
                    else
                    {
                        dbinfo.DBBrush = Brushes.Orange;
                        dbinfo.DB_StepState = DB_Info.DB_STEP_State.WARNING;
                        //AppendLog($"[BASE] DB Name: {dbinfo.DBName} - PING 통신 결과: 불량");
                        //AppendLog($"[BASE] DB Name: {dbinfo.DBName} - PING 통신 종료");
                        return false;
                    }
                }
                else
                {
                    dbinfo.DBBrush = Brushes.Orange;
                    dbinfo.DB_StepState = DB_Info.DB_STEP_State.WARNING;
                    //AppendLog($"[BASE] DB Name: {dbinfo.DBName} - PING 통신 결과: 불량");
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
        public bool STEP_Warning(DB_Info dbinfo)
        {
            try
            {
                if (dbinfo.DB_StepState == DB_Info.DB_STEP_State.CRITICAL)
                {
                    return false;
                }

                //AppendLog($"[WARNING] DB Name: {dbinfo.DBName} - SQL Client Connection 확인 시작");

                dbinfo.DBBrush = Brushes.White;

                bool sqlconnection = dbinfo.DBSQL.State == ConnectionState.Open ? true : false;

                if (sqlconnection)
                {
                    dbinfo.DBBrush = Brushes.Green;
                    dbinfo.DB_StepState = DB_Info.DB_STEP_State.BASE;
                    //AppendLog($"[WARNING] DB Name: {dbinfo.DBName} - SQL Client Connection 결과 : 양호");
                    //AppendLog($"[WARNING] DB Name: {dbinfo.DBName} - SQL Client Connection 확인 종료");

                    return true;
                }
                else
                {
                    dbinfo.DBBrush = Brushes.Orange;
                    dbinfo.DB_StepState = DB_Info.DB_STEP_State.WARNING;
                    //AppendLog($"[WARNING] DB Name: {dbinfo.DBName} - SQL Client Connection 결과 : 불량");
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
        public async Task<bool> STEP_Error(Server_Info serverinfo)
        {
            try
            {
                // 추가: 양호한 상태인 DB는 색상처리 제외 시키기 위해
                var findList = serverinfo.DB_List.Where(s => s.DB_StepState == DB_Info.DB_STEP_State.WARNING || s.DB_StepState == DB_Info.DB_STEP_State.ERROR).ToList();

                //AppendLog($"[ERROR] Server [IP: {serverinfo.Server_IP}, PORT: {serverinfo.Server_PORT}] - Network cmd 통신 시작");

                findList.ForEach(db =>
                {
                    db.DBBrush = Brushes.White;
                });

                string rcvdata = await _IDBConnect.DB_ProcessCMD($"SQLCMD -S {serverinfo.Server_IP},{serverinfo.Server_PORT} -U {serverinfo.Server_ID} -P {serverinfo.Server_PW} -Q \"SELECT 1\"");
                //AppendLog($"[ERROR] CMD Query: SQLCMD -S {serverinfo.Server_IP},{serverinfo.Server_PORT} -U {serverinfo.Server_ID} -P {serverinfo.Server_PW} -Q \"SELECT 1\", RCV: {rcvdata}");

                if (rcvdata.Equals("1"))
                {
                    findList.ForEach(db =>
                    {
                        db.DBBrush = Brushes.Orange;
                        db.DB_StepState = DB_Info.DB_STEP_State.WARNING;
                    });

                    //AppendLog($"[ERROR] Server [IP: {serverinfo.Server_IP}, PORT: {serverinfo.Server_PORT}] - Network cmd 통신 결과 : 양호");
                    return true;
                }
                else
                {
                    findList.ToList().ForEach(db =>
                    {
                        db.DBBrush = Brushes.Red;
                        db.DB_StepState = DB_Info.DB_STEP_State.ERROR;
                    });

                    //AppendLog($"[ERROR] Server [IP: {serverinfo.Server_IP}, PORT: {serverinfo.Server_PORT}] - Network cmd 통신 결과 : 불량");
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
        public async Task<bool> STEP_Critical(Server_Info serverinfo)
        {
            try
            {
                // 추가: 양호한 상태인 DB는 색상처리 제외 시키기 위해
                var findList = serverinfo.DB_List.Where(s => s.DB_StepState == DB_Info.DB_STEP_State.CRITICAL || s.DB_StepState == DB_Info.DB_STEP_State.ERROR).ToList();

                //AppendLog($"[CRITICAL] Server [IP: {serverinfo.Server_IP}, PORT: {serverinfo.Server_PORT}] - Power Shell 통신 시작");

                findList.ForEach(db =>
                {
                    db.DBBrush = Brushes.White;
                });

                bool psresult = await _IDBConnect.DB_PowerShell(serverinfo.Server_IP, serverinfo.Server_PORT);

                if (psresult)
                {
                    findList.ForEach(db =>
                    {
                        db.DBBrush = Brushes.Red;
                        db.DB_StepState = DB_Info.DB_STEP_State.ERROR;
                    });

                    //AppendLog($"[CRITICAL] Server [IP: {serverinfo.Server_IP}, PORT: {serverinfo.Server_PORT}] - Power Shell 통신 결과 : 양호");
                    return true;
                }
                else
                {
                    findList.ForEach(db =>
                    {
                        db.DBBrush = Brushes.Black;
                        db.DB_StepState = DB_Info.DB_STEP_State.CRITICAL;
                    });

                    //AppendLog($"[CRITICAL] Server [IP: {serverinfo.Server_IP}, PORT: {serverinfo.Server_PORT}] - Power Shell 통신 결과 : 불량");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
    }
}
