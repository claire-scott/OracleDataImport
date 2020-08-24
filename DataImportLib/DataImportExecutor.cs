using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataImportLib.Models;
using System.Threading.Tasks;
using System.Threading;

namespace DataImportLib
{
    public class DataImportExecutor
    {
        private string ConnectionString;
        private LandingRepository repo;
        private static Semaphore ImportLock;

        List<Task> tasks;

        public DataImportExecutor(string ConnectionString)
        {
            ImportLock = new Semaphore(5, 5);
            tasks = new List<Task>();
            this.ConnectionString = ConnectionString;
            repo = new LandingRepository(ConnectionString);
        }

        public void Run()
        {

            List<Landing_Table> import_tables = repo.GetLandingTables();

            tasks.Clear();

            di_execution execution_log = repo.NewExecution("Oracle data import");


            foreach (Landing_Table table in import_tables)
            {

                OracleDataImport odi = new OracleDataImport(table,execution_log.execution_id,repo);

                tasks.Add(Task.Factory.StartNew(() => DoWork(odi)));

            }

            Task.WaitAll(tasks.ToArray());

            execution_log.status = "Finished";
            repo.Update(execution_log);

        }

        public static void DoWork(object data)
        {
            OracleDataImport odi = (OracleDataImport)data;

            ImportLock.WaitOne();

            odi.ImportTable();

            ImportLock.Release();
        }

    }
}
