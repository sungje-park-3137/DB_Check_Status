using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WPF_SQL_NET_Framework.SingleTon;
using WPF_SQL_NET_Framework.Interface;
using WPF_SQL_NET_Framework.Info;

namespace WPF_SQL_NET_Framework.Model
{
    public class M_DBQuery
    {
        private ObservableCollection<Server_Info> Server_List => csServerList.GetInstance().Server_Lists;
        private readonly IDBConnect _IDBConnect;

        public M_DBQuery()
        {
            _IDBConnect = new DBFunc();
        }

        public void Check_Query_DBInfo()
        {
            try
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
                                if (_IDBConnect.SendQuery_DataAdapter(tempQuery, db.DBSQL, out DataTable rcvdatatable))
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
                                if (_IDBConnect.SendQuery_DataAdapter(tempQuery, db.DBSQL, out DataTable rcvdatatable))
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
                }

                    // 알고리즘 다시 생각
                    /*// 묻고
                    // 성공, 실패
                    // 담고, null

                    for (int i = 0; i < 2; i++)
                    {
                        string tempMsg = string.Empty;
                        switch (i)
                        {
                            case 0:
                                tempMsg = @"SELECT SERVERPROPERTY('ServerName') AS ServerName, sqlserver_start_time AS Uptime FROM sys.dm_os_sys_info;";
                                var taskItem = Server_List.SelectMany(server => server.DB_List.Select(db =>
                                {
                                    bool queryResrult = _IDBConnect.SendQuery_DataAdapter(tempMsg, db.DBSQL, out DataTable rcvData); // 비동기

                                    return new { isReults = queryResrult, Server_Info = server, DataTable = rcvData };
                                })).ToList();

                                taskItem.ForEach(list =>
                                {
                                    list.Server_Info.DB_Server_RcvTable = list.isReults ? list.DataTable : new DataTable(); // 데이터가 있다면 하나 뿌리고 빠짐
                                    return;
                                });


                                break;

                            case 1:
                                tempMsg = @"SELECT DB_NAME(database_id) AS DatabaseName," +
                                    "SUM(num_of_reads) AS Reads,SUM(num_of_writes) AS Writes," +
                                    "SUM(num_of_bytes_read)/1024/1024 AS MB_Read, " +
                                    "SUM(num_of_bytes_written)/1024/1024 AS MB_Written FROM sys.dm_io_virtual_file_stats(NULL, NULL) " +
                                    "GROUP BY database_id " +
                                    "ORDER BY (SUM(num_of_reads) + SUM(num_of_writes)) DESC;";

                                break;
                        }
                    }*/
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
