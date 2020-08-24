using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using DataImportLib;

namespace OracleDataImport
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string [] args = Environment.GetCommandLineArgs();
            

                Application.Run(new Form1());

        }


    }
}
