using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Configuration;
using Dapper;
using Dapper.Contrib;
using Dapper.Contrib.Extensions;

namespace DataImportLib.Models
{
    class LandingRepository
    {
        private string ConnectionString;

        public LandingRepository(string ConnectionString)
        {
            this.ConnectionString = ConnectionString;
        }

        public IDbConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public List<Landing_Table> GetLandingTables()
        {
            string sql = "select * from Control.Landing_Table where Active = 1;";

            List<Landing_Table> list = (List<Landing_Table>)GetConnection().Query<Landing_Table>(sql);
            return list;
        }

        public Landing_Table_Log New_Log_Record(int LandingTableId)
        {
            var o = new Landing_Table_Log();
            o.Landing_Table_Id = LandingTableId;
            GetConnection().Insert(o);

            return o;
        }

        public void Update_Log(Landing_Table_Log TableLog)
        {
            GetConnection().Update(TableLog);
        }

        public List<Landing_Table_Log> GetLogEntries(DateTime SinceWhen)
        {
            return (List<Landing_Table_Log>)GetConnection().Query<Landing_Table_Log>("select * from Control.Landing_Table_Log where Start_Time > @start order by Start_Time", new { start = SinceWhen });
        }

        public bool TableExists(string Schema, string TableName)
        {
            int count = GetConnection().QuerySingle<int>("select count(*) from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA = @schema and TABLE_NAME = @table;", new { schema = Schema, table = TableName });

            if (count == 1) return true;

            return false;
        }

        public bool TruncateTable(string Schema, string TableName)
        {
            string sql = String.Format("TRUNCATE TABLE [{0}].[{1}];", Schema, TableName);

            GetConnection().Execute(sql);

            return false;
        }

        public DateTime GetCurrentServerTime()
        {
            return GetConnection().ExecuteScalar<DateTime>("select CURRENT_TIMESTAMP;");
        }

        public di_execution NewExecution(string Description)
        {
            var o = new di_execution();
            o.execution_desc = Description;
            o.start_time = DateTime.Now;
            o.executed_as = Environment.UserName;
            o.status = "Running";
            GetConnection().Insert(o);

            return o;
        }

        public bool Update(di_execution exec)
        {
            return GetConnection().Update(exec);
        }

        public bool Update(di_operation op)
        {
            return GetConnection().Update(op);
        }

        public di_operation NewOperation(int ExecutionId, string Description)
        {
            var o = new di_operation();
            o.operation_desc = Description;
            o.start_time = DateTime.Now;
            o.operation_status = "Running";
            o.execution_id = ExecutionId;
            GetConnection().Insert(o);

            return o;
        }

        public di_event_message AddMessage(int OperationId,string ExecutionPath,string EventName,string EventMessage,string EventStatus)
        {
            var message = new di_event_message();
            message.operation_id = OperationId;
            message.execution_path = ExecutionPath;
            message.message_time = DateTime.Now;
            message.event_name = EventName;
            message.event_status = EventStatus;
            message.event_message = EventMessage;

            GetConnection().Insert(message);

            return message;
        }

    }
}
