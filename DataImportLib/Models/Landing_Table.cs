using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImportLib.Models
{
    [Table("Control.Landing_Table")]
    public class Landing_Table
    {
        [Key]
        public int id { get; set; }
        public string Table_Name { get; set; }
        public string Source_DB { get; set; }
        public string Source_User { get; set; }
        public string Source_Pass { get; set; }
        public string Dest_Schema { get; set; }
        public int? Active { get; set; }
        public string Notes { get; set; }
    }
}
