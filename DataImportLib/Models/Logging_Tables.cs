using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DataImportLib.Models
{
    [Table("Control.Landing_Table_Log")]
    public class Landing_Table_Log
    {
        [Key]
        public int id { get; set; }
        public int? Landing_Table_Id { get; set; }
        public int? Success { get; set; }
        public DateTime? Start_Time { get; set; }
        public DateTime? End_Time { get; set; }
        public DateTime? Begin_Land { get; set; }
        public DateTime? End_Land { get; set; }
        public DateTime? Begin_Hash { get; set; }
        public DateTime? End_Hash { get; set; }
        public DateTime? Begin_Stage { get; set; }
        public DateTime? End_Stage { get; set; }
        public int? Source_Rows { get; set; }
        public int? Landing_Rows { get; set; }
        public int? Stage_Rows { get; set; }
        public int? Deleted_Stage_Rows { get; set; }
        public int? New_Stage_Rows { get; set; }
        public string Notes { get; set; }
    }

    [Table("Control.di_event_messages")]
    public class di_event_message
    {
        [Key]
        public int event_id { get; set; }
        public int? operation_id { get; set; }
        public string execution_path { get; set; }
        public DateTime? message_time { get; set; }
        public string event_name { get; set; }
        public string event_message { get; set; }
        public string event_status { get; set; }
    }

    [Table("Control.di_execution")]
    public class di_execution
    {
        [Key]
        public int execution_id { get; set; }
        public string execution_desc { get; set; }
        public DateTime? start_time { get; set; }
        public string executed_as { get; set; }
        public string status { get; set; }
    }

    [Table("Control.di_operation")]
    public class di_operation
    {
        [Key]
        public int operation_id { get; set; }
        public int? execution_id { get; set; }
        public string operation_desc { get; set; }
        public DateTime? start_time { get; set; }
        public DateTime? end_time { get; set; }
        public string operation_status { get; set; }
    }



}
