using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataImportLib;

namespace DataImportRunner
{
    class Program
    {
        static void Main(string[] args)
        {

            DataImportExecutor di = new DataImportExecutor(System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);

            di.Run();
        }
    }
}
