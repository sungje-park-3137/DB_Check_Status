using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WPF_SQL_NET_Framework.Command;

namespace WPF_SQL_NET_Framework.Info
{
    public class DB_Info : NotifyProperty
    {
        // DB: Data Base 이름
        private string _dbname = string.Empty;
        public string DBName
        {
            get { return _dbname; }
            set { _dbname = value; OnPropertyChanged(nameof(DBName)); }
        }

        // DB: 연결
        private SqlConnection _dbsql = new SqlConnection();
        public SqlConnection DBSQL
        {
            get { return _dbsql; }
            set { _dbsql = value; OnPropertyChanged(nameof(DBSQL)); }
        }

        // DB: 상태 정보
        private Brush _dbbrush = Brushes.White;
        public Brush DBBrush
        {
            get { return _dbbrush; }
            set { _dbbrush = value; OnPropertyChanged(nameof(DBBrush)); }
        }

        // DB: (Query문에 대한) 응답 상태 정보
        private SQL_DB_State _DB_State = new SQL_DB_State();
        public SQL_DB_State DB_State
        {
            get { return _DB_State; }
            set { _DB_State = value; OnPropertyChanged(nameof(DB_State)); }
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

        private DB_STEP_State _STEP_State = new DB_STEP_State();
        public DB_STEP_State DB_StepState
        {
            get { return _STEP_State; }
            set { _STEP_State = value; OnPropertyChanged(nameof(DB_StepState)); }
        }
        
        public enum DB_STEP_State
        {
            BASE,
            WARNING,
            ERROR,
            CRITICAL
        }
    }
}
