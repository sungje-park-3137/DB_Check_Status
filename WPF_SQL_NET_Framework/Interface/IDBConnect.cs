using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WPF_SQL_NET_Framework.Model;
using WPF_SQL_NET_Framework.ViewModel;

namespace WPF_SQL_NET_Framework.Interface
{
    // interface: 규칙을 정의
    // 내부 class: interface를 상속받은 규칙은 모두 반드시 수행
    // -> 단, 규칙의 내부 내용은 각 class 별 다를 수 있음

    // IDB_Function 에서 구현해야할 규칙
    // 1. DB 연결관련
    public interface IDBConnect
    {
        string SQLConnStr(string ip, int port, string dbname, string id, string password);
        Task<bool> DBConnect(string ip, int port, string id, string password, string SQLConnectionString, SqlConnection dbconn);
        bool SendQuery_DataReader(string query, SqlConnection dbconn, out List<string> rcvdata);
        bool SendQuery_DataAdapter(string query, SqlConnection dbconn, out DataTable rcvdatatable);
        void DBDisconnect(SqlConnection dbconn);
        Task<bool> DB_PowerShell(string ip, int port);
        Task<string> DB_ProcessCMD(string query);
        SQL_DB_State ConvertStrToSQLState(string stringstate);
    }

    public class DBFunc : IDBConnect
    {
        //DB: Connection
        public async Task<bool> DBConnect(string ip, int port, string id, string password, string SQLConnectionString, SqlConnection dbconn)
        {
            try
            {
                //dbconn = new SqlConnection(SQLConnectionString);
                dbconn.ConnectionString = SQLConnectionString;
                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(3));

                await dbconn.OpenAsync(cts.Token);

                if (dbconn.State == System.Data.ConnectionState.Open)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("[DBConnect 오류] : " + ex.Message);
                Trace.Write(ex.Message);
                return false;
            }
        }

        public string SQLConnStr(string ip, int port, string dbname, string id, string password)
        {
            return new SqlConnectionStringBuilder()
            {
                DataSource = FormattableString.Invariant($"{ip}, {port}"),
                InitialCatalog = dbname,
                UserID = id,
                Password = password,
                PersistSecurityInfo = true,
                ConnectTimeout = 2,
                TrustServerCertificate = true,
            }.ConnectionString;
        }

        public bool SendQuery_DataReader(string query, SqlConnection dbconn, out List<string> rcvdata)
        {
            rcvdata = new List<string>();

            try
            {
                if (dbconn.State != ConnectionState.Open || dbconn == null)
                {
                    return false;
                }

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = dbconn;
                    cmd.CommandText = query;
                    cmd.CommandTimeout = 3;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader != null)
                        {
                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    rcvdata.Add(reader[i].ToString());
                                }
                            }

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public bool SendQuery_DataAdapter(string query, SqlConnection dbconn, out DataTable rcvdatatable)
        {
            rcvdatatable = new DataTable();

            try
            {
                if (dbconn.State != ConnectionState.Open || dbconn == null)
                {
                    return false;
                }

                using (SqlDataAdapter adapter = new SqlDataAdapter(query, dbconn))
                {
                    adapter.Fill(rcvdatatable);

                    if (rcvdatatable == null)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public void DBDisconnect(SqlConnection dbconn)
        {
            try
            {
                dbconn.Close();
                dbconn.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public async Task<bool> DB_PowerShell(string ip, int port)
        {
            try
            {
                string query = $"Test-NetConnection -ComputerName {ip} -Port {port}";

                using (PowerShell ps = PowerShell.Create())
                {
                    ps.AddScript(query);

                    var invokeresult = ps.BeginInvoke();
                    var result = await Task.Factory.FromAsync(invokeresult, ps.EndInvoke);

                    var temp = result.FirstOrDefault().Members;
                    if (temp == null)
                    {
                        return false;
                    }
                    else
                    {
                        return (bool)temp.Where(x => x.Name == "TcpTestSucceeded").FirstOrDefault().Value;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public async Task<string> DB_ProcessCMD(string query)
        {
            try
            {
                ProcessStartInfo cmd = new ProcessStartInfo();
                cmd.FileName = "cmd.exe";
                cmd.Arguments = "/c" + query;
                cmd.WindowStyle = ProcessWindowStyle.Hidden;
                cmd.CreateNoWindow = true;
                cmd.UseShellExecute = false;
                cmd.RedirectStandardOutput = true;
                cmd.RedirectStandardInput = true;
                cmd.RedirectStandardError = true;

                using (Process process = new Process())
                {
                    process.StartInfo = cmd;
                    process.Start();
                    // "           \r\n
                    // -----------\r\n
                    // 1\r\n
                    // \r\n
                    // (1 rows affected)\r\n"
                    // ㅁㅊ;
                    // ""
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();

                    process.WaitForExit();

                    if (string.IsNullOrEmpty(error))
                    {
                        // select 1에 최적화 되어 있음 -> 따로 해줘야할듯
                        string[] tempSplit = output.Split('\n');
                        string splitRcv = tempSplit[2].Trim();
                        return splitRcv;
                    }
                    else
                    {
                        return error;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return ex.Message;
            }
        }

        public SQL_DB_State ConvertStrToSQLState(string stringstate)
        {
            try
            {
                if (string.IsNullOrEmpty(stringstate))
                {
                    return SQL_DB_State.EMERGENCY;
                }

                switch (stringstate)
                {
                    case "ONLINE":
                        return SQL_DB_State.ONLINE;

                    case "OFFLINE":
                        return SQL_DB_State.OFFLINE;

                    case "RESTORING":
                        return SQL_DB_State.RESTORING;

                    case "RECOVERING":
                        return SQL_DB_State.RECOVERING;

                    case "RECOBERY_PENDING":
                        return SQL_DB_State.RECOBERY_PENDING;

                    case "SUSPECT":
                        return SQL_DB_State.SUSPECT;

                    case "EMERGENTCY":
                        return SQL_DB_State.EMERGENCY;

                    default:

                        return SQL_DB_State.EMERGENCY;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return SQL_DB_State.EMERGENCY;
            }
        }
    }



    // MS 공식 문서: 데이터베이스 상태 정의
    public enum SQL_DB_State
    {
        ONLINE,
        OFFLINE,
        RESTORING,
        RECOVERING,
        RECOBERY_PENDING,
        SUSPECT,
        EMERGENCY
    }
}
