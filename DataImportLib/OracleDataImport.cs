using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using CSU.PA.TJS.Toolkit;
using System.Data.SqlClient;
using DataImportLib.Models;
using NLog;

namespace DataImportLib
{
    class OracleDataImport
    {
        private static Logger logger = LogManager.GetLogger("OracleDataImport");

        public string SourceTable { get; set; }
        public string DestinationTable { get; set; }

        private string sourceConnectionString;
        private string destinationConnectionString;
        private string metaDataConnectionString;

        private DataTable srcSchemaTable;
        private DataTable dstSchemaTable;

        private Landing_Table Table;
        private Landing_Table_Log TableLog;
        private di_operation operation;
        private LandingRepository repo;
        private int ExecutionId;
        

        private string tableName;
        public string TableName
        {
            get { return tableName; }
        }

        public OracleDataImport(Landing_Table Table, int ExecutionId, LandingRepository Repository)
        {
            String password = RijndaelSimple.Decrypt(Table.Source_Pass, "passPhrase", "saltValue", "SHA1", 1, "initVector", 128);
            repo = Repository;

            this.Table = Table;
            sourceConnectionString = "Provider=OraOLEDB.Oracle; Data Source=" + Table.Source_DB + ".world; User Id=" + Table.Source_User + "; Password=" + password + ";";

            tableName = TableName;
            this.ExecutionId = ExecutionId;
        }

        public void ImportTable()
        {
            SourceTable = tableName;
            DestinationTable = tableName;

            string[] ignoreRowCountTables = { "SWVTRAN" };

            logger.Info("Beginning landing for table {0}", Table.Table_Name);
            
            operation = repo.NewOperation(ExecutionId, string.Format("Data import for {0}.{1}", Table.Dest_Schema, Table.Table_Name));
            repo.AddMessage(operation.operation_id, operation.operation_desc + "\\ImportTable", "OnStart", string.Format("Import {0}.{1} beginning", Table.Dest_Schema, Table.Table_Name), "Information");


            TableLog = repo.New_Log_Record(Table.id);

            try
            {
                SourceTableCount();
                repo.AddMessage(operation.operation_id, operation.operation_desc + "\\ImportTable", "Information", string.Format("Source row count for {0}.{1} is {2}", Table.Dest_Schema, Table.Table_Name,TableLog.Source_Rows), "Information");

                repo.AddMessage(operation.operation_id, operation.operation_desc + "\\ImportTable", "Information", string.Format("Starting to land table {0}.{1}", Table.Dest_Schema, Table.Table_Name), "Information");
                LandTable();
                LandingTableCount();
                repo.AddMessage(operation.operation_id, operation.operation_desc + "\\ImportTable", "Information", string.Format("Finished landing {0}.{1} row count is {2}", Table.Dest_Schema, Table.Table_Name,TableLog.Landing_Rows), "Information");

                repo.AddMessage(operation.operation_id, operation.operation_desc + "\\ImportTable", "Information", string.Format("Starting add hashes for table {0}.{1}", Table.Dest_Schema, Table.Table_Name), "Information");
                HashTable();
                repo.AddMessage(operation.operation_id, operation.operation_desc + "\\ImportTable", "Information", string.Format("Finished adding hashes for table {0}.{1}", Table.Dest_Schema, Table.Table_Name), "Information");


                logger.Info("Rows in {0} source: {1}, landing: {2}", Table.Table_Name,TableLog.Source_Rows,TableLog.Landing_Rows);


                if (TableLog.Source_Rows != TableLog.Landing_Rows)
                {
                    if (!ignoreRowCountTables.Contains(Table.Table_Name))
                    {
                        throw new Exception("Source and landing table rows don't match, not continuing");
                    }
                    else
                    {
                        logger.Info("Exception Ignored for excluded table {0}.  Rowcount exception. Landing rows: {1} Stage rows: {2}", Table.Table_Name, TableLog.Landing_Rows, TableLog.Stage_Rows);
                    }
                }


                repo.AddMessage(operation.operation_id, operation.operation_desc + "\\ImportTable", "Information", string.Format("Starting stage for {0}.{1}", Table.Dest_Schema, Table.Table_Name), "Information");
                StageTable();

                StageTableCount();
                repo.AddMessage(operation.operation_id, operation.operation_desc + "\\ImportTable", "Information", string.Format("Finished stage for {0}.{1} row count is {2}", Table.Dest_Schema, Table.Table_Name,TableLog.Stage_Rows), "Information");

                logger.Info("Rows in {0} stage: {1}", Table.Table_Name, TableLog.Stage_Rows);

                if (TableLog.Stage_Rows == TableLog.Source_Rows)
                {
                    TableLog.Success = 1;
                    repo.AddMessage(operation.operation_id, operation.operation_desc + "\\ImportTable", "Information", "Import finished and source/stage row counts match: "+TableLog.Source_Rows+":"+TableLog.Stage_Rows+" rows", "Information");
                    operation.operation_status = "Success";
                } else
                {
                    TableLog.Success = 0;
                    repo.AddMessage(operation.operation_id, operation.operation_desc + "\\ImportTable", "OnError", "Row Mismatch between source and staging table", "Error");
                    operation.operation_status = "Finished with row count error";
                }
                 
            }

            catch (Exception ex)
            {
                logger.Error(ex, "Exception while landing {0}: {1}", Table.Table_Name,ex.Message);
                operation.operation_status = "Failed";

                repo.AddMessage(operation.operation_id, operation.operation_desc + "\\ImportTable", "OnError", "Exception while trying to import table: " + ex.Message + " " + ex.Source + " " + ex.StackTrace, "Error");

                operation.operation_status = "Failed";
            }

            finally
            {
                operation.end_time = DateTime.Now;

                repo.Update_Log(TableLog);
                repo.Update(operation);
                logger.Info("Finished landing for table {0}", Table.Table_Name);
                repo.AddMessage(operation.operation_id, operation.operation_desc + "\\ImportTable", "OnComplete", "Import finished and source/stage row counts match", "Information");
            }

            
            /*
             * hashing this out for now,   moving to simplistic method, revisit later
            //try
            //{
                if (GetSourceSchema())
                {
                    if (GetDestSchema())
                    {
                        if (!SchemasMatch())
                        {
                            WriteToLog("info", "Schema's don't match, drop and recreating");
                            DropDestTable();
                            CreateDestTable();
                        }
                    }
                    else
                    {
                        WriteToLog("info", "Destination Table doesn't exist, creating");
                        CreateDestTable();
                    }

                    // At this point the destination table should be good to go
                    GetDestSchema(); // refresh the schema

                    DeleteAndImport();
                }
                else
                {
                    WriteToLog("Error", "Source table does not exist");
                }
            //}
            //catch (Exception ex)
            //{
            //    WriteToLog("Error", "Unspecified error during import: "+ex.Message);

            //}
            */
        }

        private bool LandTable()
        {
            logger.Info("Landing table {0}", Table.Table_Name);
            TableLog.Begin_Land = DateTime.Now;
            if (!GetSourceSchema())
            {
                throw new Exception("Could not find source table");
            }


            if (repo.TableExists(Table.Dest_Schema, Table.Table_Name))
            {
                repo.AddMessage(operation.operation_id, operation.operation_desc + "\\ImportTable\\LandTable", "Information", string.Format("Truncating landing table {0}.{1}", Table.Dest_Schema, Table.Table_Name), "Information");
                logger.Info("Truncating table {0}", Table.Table_Name);
                repo.TruncateTable(Table.Dest_Schema, Table.Table_Name);
            }
            else
            {
                repo.AddMessage(operation.operation_id, operation.operation_desc + "\\ImportTable\\LandTable", "Information", string.Format("Creating landing table {0}.{1}", Table.Dest_Schema, Table.Table_Name), "Information");
                logger.Info("table {0} does not exist, creating it", Table.Table_Name);
                CreateDestTable();
            }

            TableLog.Begin_Land = DateTime.Now;
            repo.Update_Log(TableLog);
            using (OleDbConnection conn = new OleDbConnection(sourceConnectionString))
            {
                string sql = string.Format("select * from {0}", Table.Table_Name);
                OleDbCommand cmd = new OleDbCommand(sql, conn);
                cmd.CommandTimeout = 0;
                conn.Open();
                OleDbDataReader dr = cmd.ExecuteReader();

                SqlConnection dstConn = (SqlConnection)repo.GetConnection();
                dstConn.Open();
                SqlBulkCopy bcp = new SqlBulkCopy(dstConn, SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.UseInternalTransaction, null);
                bcp.BulkCopyTimeout = 0;
                bcp.BatchSize = 100000;
                bcp.SqlRowsCopied += new SqlRowsCopiedEventHandler(OnSqlRowsCopied);
                bcp.NotifyAfter = 250000;
                bcp.DestinationTableName = "["+Table.Dest_Schema + "].[" + Table.Table_Name+"]";
                bcp.WriteToServer(dr);

                dstConn.Close();
                conn.Close();
            }
            TableLog.End_Land = DateTime.Now;
            repo.Update_Log(TableLog);

            logger.Info("Finished landing table {0}", Table.Table_Name);

            return true;
        }

        private bool HashTable()
        {
            logger.Info("Adding Hashes for table {0}", Table.Table_Name);
            TableLog.Begin_Hash = DateTime.Now;
            using (SqlConnection conn = (SqlConnection)repo.GetConnection())
            {
                SqlCommand cmd = new SqlCommand(GetHashQuery(), conn);
                cmd.CommandTimeout = 0;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            TableLog.End_Hash = DateTime.Now;
            repo.Update_Log(TableLog);

            logger.Info("Finished adding Hashes for table {0}", Table.Table_Name);

            return true;
        }

        private bool StageTable()
        {
            logger.Info("Staging table {0}", Table.Table_Name);
            TableLog.Begin_Stage = DateTime.Now;

            if (!repo.TableExists(Table.Dest_Schema, Table.Table_Name+"_History"))
            {
                repo.AddMessage(operation.operation_id, operation.operation_desc + "\\ImportTable\\StageTable", "Information", string.Format("Creating stage table {0}.{1}_History", Table.Dest_Schema, Table.Table_Name), "Information");
                CreateStageTable();
            }

            if (!repo.TableExists(Table.Dest_Schema, Table.Table_Name + "_Archive"))
            {
                repo.AddMessage(operation.operation_id, operation.operation_desc + "\\ImportTable\\StageTable", "Information", string.Format("Creating archive table {0}.{1}_Archive", Table.Dest_Schema, Table.Table_Name), "Information");
                CreateArchiveTable();
            }

            StringBuilder qryInsert = new StringBuilder();
            StringBuilder qryDelete = new StringBuilder();
            

            qryInsert.Append(string.Format("insert into [{0}].[{1}_History] ({2}, DW_Insert_Date, DW_Row_Hash) \r\n",Table.Dest_Schema,Table.Table_Name,GetFieldList()));
            qryInsert.Append(string.Format("select {0}, DW_Load_Date,DW_Row_Hash \r\n",GetFieldList()));
            qryInsert.Append(string.Format("from [{0}].[{1}] \r\n",Table.Dest_Schema,Table.Table_Name));
            qryInsert.Append(string.Format("where DW_Row_Hash not in (select DW_Row_Hash from [{0}].[{1}_History]);", Table.Dest_Schema, Table.Table_Name));

            
            qryDelete.Append("declare @deletes TABlE ( hash binary(32) );\r\n");
            qryDelete.Append(string.Format("insert into @deletes select DW_Row_Hash from [{0}].[{1}_History] where DW_Row_Hash not in (select DW_Row_Hash from [{0}].[{1}]);\r\n",Table.Dest_Schema,Table.Table_Name));
            qryDelete.Append("BEGIN TRAN\r\n");
            qryDelete.Append(string.Format("insert into [{0}].[{1}_Archive] ({2},DW_Insert_Date,DW_Delete_Date,DW_Row_Hash) select {2},DW_Insert_Date,CURRENT_TIMESTAMP,DW_Row_Hash from [{0}].[{1}_History] where DW_Row_Hash in (select hash from @deletes);\r\n",Table.Dest_Schema,Table.Table_Name,GetFieldList()));
            qryDelete.Append(string.Format("delete from [{0}].[{1}_History] where DW_Row_Hash in (select hash from @deletes);\r\n",Table.Dest_Schema,Table.Table_Name));
            qryDelete.Append("COMMIT\r\n");
            qryDelete.Append("select count(*) from @deletes;");

            using (SqlConnection conn = (SqlConnection)repo.GetConnection())
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(qryInsert.ToString(), conn);
                cmd.CommandTimeout = 0;
                TableLog.New_Stage_Rows = cmd.ExecuteNonQuery();
                repo.AddMessage(operation.operation_id, operation.operation_desc + "\\ImportTable\\StageTable", "Information", string.Format("Added {2} rows to  {0}.{1}_History", Table.Dest_Schema, Table.Table_Name,TableLog.New_Stage_Rows), "Information");

                cmd.CommandText = qryDelete.ToString();
                TableLog.Deleted_Stage_Rows = (int)cmd.ExecuteScalar();

                logger.Info("Staging {0}, {1} rows inserted, {2} rows deleted",Table.Table_Name,TableLog.New_Stage_Rows,TableLog.Deleted_Stage_Rows);
                repo.AddMessage(operation.operation_id, operation.operation_desc + "\\ImportTable\\StageTable", "Information", string.Format("Removed {2} rows from {0}.{1}_History", Table.Dest_Schema, Table.Table_Name, TableLog.New_Stage_Rows), "Information");

                conn.Close();
            }

            TableLog.End_Stage = DateTime.Now;
            repo.Update_Log(TableLog);

            logger.Info("Finished staging table {0}", Table.Table_Name);

            return true;
        }

        private void SourceTableCount()
        {
            using (OleDbConnection conn = new OleDbConnection(sourceConnectionString))
            {
                string sql = string.Format("select count(*) from {0}", Table.Table_Name);
                OleDbCommand cmd = new OleDbCommand(sql, conn);
                cmd.CommandTimeout = 0;
                conn.Open();
                TableLog.Source_Rows = System.Convert.ToInt32(cmd.ExecuteScalar());
                conn.Close();
            }
        }

        private void LandingTableCount()
        {
            using (SqlConnection conn = (SqlConnection)repo.GetConnection())
            {
                string sql = string.Format("select count(*) from [{0}].[{1}];", Table.Dest_Schema, Table.Table_Name);
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandTimeout = 0;
                conn.Open();
                TableLog.Landing_Rows = (int)cmd.ExecuteScalar();

                conn.Close();
            }
        }

        private bool StageTableCount()
        {
            using (SqlConnection conn = (SqlConnection)repo.GetConnection())
            {
                string sql = string.Format("select count(*) from [{0}].[{1}_History];", Table.Dest_Schema, Table.Table_Name);
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandTimeout = 0;
                conn.Open();
                TableLog.Stage_Rows = (int)cmd.ExecuteScalar();

                conn.Close();
            }
            repo.Update_Log(TableLog);

            return true;
        }


        /// <summary>
        /// Will compare the counts for SCN's in the source and destination tables and drop any with SCN's that don't match
        /// </summary>
        private void DeleteAndImport()
        {
            List<long> toDelete = new List<long>();
            List<long> toImport = new List<long>();

            Dictionary<long, int> dstScns = new Dictionary<long, int>();
            using (SqlConnection conn = new SqlConnection(destinationConnectionString))
            {
                SqlCommand dstCmd = new SqlCommand("select ora_rowscn,count(*) count from " + SourceTable + " GROUP BY ora_rowscn", conn);
                dstCmd.CommandTimeout = 240;
                conn.Open();
                SqlDataReader dstReader = dstCmd.ExecuteReader();
                while (dstReader.Read()) dstScns.Add(System.Convert.ToInt64(dstReader[0]), System.Convert.ToInt32(dstReader[1]));
                dstReader.Close();
                conn.Close();
            }

            using (OleDbConnection conn = new OleDbConnection(sourceConnectionString))
            {
                OleDbCommand srcCmd = new OleDbCommand("select ora_rowscn,count(*) count from " + DestinationTable + " GROUP BY ora_rowscn", conn);
                srcCmd.CommandTimeout = 240;
                conn.Open();
                OleDbDataReader srcReader = srcCmd.ExecuteReader();
                long scn;
                int count;
                while (srcReader.Read())
                {
                    scn = System.Convert.ToInt64(srcReader[0]);
                    count = System.Convert.ToInt32(srcReader[1]);

                    if (dstScns.ContainsKey(scn))
                    {
                        // Destination table contains this scn, check to see if the counts match
                        if (!dstScns[scn].Equals(count))
                        {
                            // number of elements in this scn don't match, drop and reload this scn 'page'
                            toDelete.Add(scn);
                            toImport.Add(scn);
                        }
                        // otherwise it's okay
                    }
                    else
                    {
                        // Destination table doesn't contain this scn, import
                        toImport.Add(scn);
                    }
                }
                srcReader.Close();
            }

            /*if (DeleteSCNList(toDelete))
            {
                if (toImport.Count > 0)
                {
                    ImportSCNList(toImport);
                }
                else
                {
                    WriteToLog("info", "Nothing to import");
                }
            }*/
        }

        /*private bool DeleteSCNList(List<long> toDelete)
        {
            int scns = toDelete.Count;
            int rows = 0;
            
            Stack<long> deletionList = new Stack<long>(toDelete);
           while(deletionList.Count > 0)
           {
               Stack<long> deletionBatch = new Stack<long>();

               while (deletionBatch.Count < 100 && deletionList.Count > 0)
               {
                   deletionBatch.Push(deletionList.Pop());
               }

                StringBuilder deleteList = new StringBuilder();
                string comma = "";
                foreach (long i in deletionBatch)
                {
                    deleteList.Append(comma);
                    deleteList.Append(i.ToString());
                    if (comma == "") comma = ",";
                }

                using (SqlConnection conn = new SqlConnection(destinationConnectionString))
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand("select count(*) from " + DestinationTable + " where ora_rowscn in (" + deleteList.ToString() + ")", conn);
                        conn.Open();
                        rows += System.Convert.ToInt32(cmd.ExecuteScalar());
                        cmd.CommandText = "delete from " + DestinationTable + " where ora_rowscn in (" + deleteList.ToString() + ")";
                        cmd.CommandTimeout = 240;
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        WriteToLog("Error","Error deleting values from dest table, message: "+ex.Message);
                        return false;
                    }
                }
            }
            if(toDelete.Count > 0) WriteToLog("info", "Deleted " + toDelete.Count + " scn's with " + rows + " rows");
            return true;
        }*/

        private string GetFieldList()
        {
            StringBuilder fieldList = new StringBuilder();
            string comma = "";

            // List of fields in table
            foreach (DataRow dr in srcSchemaTable.Rows)
            {
                fieldList.Append(comma);
                fieldList.Append(dr[0].ToString());
                if (comma == "") comma = ",";
            }
            
            return fieldList.ToString();
        }

        private string GetHashQuery()
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("update ["+Table.Dest_Schema+"].["+Table.Table_Name+"] set DW_Load_Date = CURRENT_TIMESTAMP,DW_Row_Hash = convert(binary(64), HASHBYTES('SHA2_256',");

            string comma = "";

            foreach(DataRow dr in srcSchemaTable.Rows)
            {
                sql.Append(comma);
                sql.Append(string.Format("COALESCE(CONVERT(VARBINARY(8000),[{0}]), 0x00) + 0x2c", dr[0].ToString()));
                if (comma == "") comma = " +\r\n";
            }

            sql.Append("));");

            return sql.ToString();
        }

        
        private bool GetSourceSchema()
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(sourceConnectionString))
                {
                    conn.Open();
                    OleDbCommand cmd = new OleDbCommand("select * from " + Table.Table_Name + " where rownum < 0", conn);
                    OleDbDataReader reader = cmd.ExecuteReader();
                    srcSchemaTable = reader.GetSchemaTable();
                    reader.Close();
                    conn.Close();
                }
            }
#if !DEBUG
            catch (Exception ex)
            {
                srcSchemaTable = null;
                return false;
            }
#endif
            finally { }

            return true;
        }

        private bool GetDestSchema()
        {
            dstSchemaTable = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(destinationConnectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select top 0 * from [" + DestinationTable + "]", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    dstSchemaTable = reader.GetSchemaTable();
                    reader.Close();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                dstSchemaTable = null;
                return false;
            }

            return true;
        }

        private void DropDestTable()
        {
            using (SqlConnection conn = new SqlConnection(destinationConnectionString))
            {
                SqlCommand cmd = new SqlCommand("DROP TABLE [" + DestinationTable + "]", conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        private void CreateDestTable()
        {
            dstSchemaTable = srcSchemaTable.Copy();

            DataRow dr = dstSchemaTable.NewRow();

            dr = dstSchemaTable.NewRow();
            dr["ColumnName"] = "DW_Load_Date";
            dr["ColumnSize"] = 16;
            dr["DataType"] = System.Type.GetType("System.DateTime");
            dr["NumericPrecision"] = 27;
            dr["NumericScale"] = 7;
            dr["AllowDBNull"] = true;
            dr["IsKey"] = false;
            dstSchemaTable.Rows.Add(dr);

            dr = dstSchemaTable.NewRow();
            dr["ColumnName"] = "DW_Row_Hash";
            dr["ColumnSize"] = 16;
            dr["DataType"] = System.Type.GetType("System.Byte[]");
            dr["ColumnSize"] = 32;
            dr["AllowDBNull"] = true;
            dr["IsKey"] = false;
            dstSchemaTable.Rows.Add(dr);


            using (SqlConnection conn = (SqlConnection)repo.GetConnection())
            {
                string sql = SqlTableCreator.GetCreateSQL(Table.Dest_Schema, Table.Table_Name, dstSchemaTable);
                logger.Info("Creating table {0} with script {1}", Table.Table_Name, sql);
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                cmd.ExecuteNonQuery();


                cmd.CommandText = "ALTER TABLE ["+Table.Dest_Schema+"].[" + Table.Table_Name + "] ADD DEFAULT (CURRENT_TIMESTAMP) FOR [DW_Load_Date]";
                cmd.ExecuteNonQuery();

                conn.Close();
            }
        }

        private void CreateStageTable()
        {
            dstSchemaTable = srcSchemaTable.Copy();

            DataRow dr = dstSchemaTable.NewRow();

            dr = dstSchemaTable.NewRow();
            dr["ColumnName"] = "DW_Insert_Date";
            dr["ColumnSize"] = 16;
            dr["DataType"] = System.Type.GetType("System.DateTime");
            dr["NumericPrecision"] = 27;
            dr["NumericScale"] = 7;
            dr["AllowDBNull"] = true;
            dr["IsKey"] = false;
            dstSchemaTable.Rows.Add(dr);

            dr = dstSchemaTable.NewRow();
            dr["ColumnName"] = "DW_Row_Hash";
            dr["ColumnSize"] = 16;
            dr["DataType"] = System.Type.GetType("System.Byte[]");
            dr["ColumnSize"] = 32;
            dr["AllowDBNull"] = true;
            dr["IsKey"] = false;
            dstSchemaTable.Rows.Add(dr);

            using (SqlConnection conn = (SqlConnection)repo.GetConnection())
            {
                string sql = SqlTableCreator.GetCreateSQL(Table.Dest_Schema, Table.Table_Name+"_History", dstSchemaTable);
                logger.Info("Creating table {0}_History with script {1}", Table.Table_Name, sql);
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                cmd.ExecuteNonQuery();

                conn.Close();
            }
        }

        private void CreateArchiveTable()
        {
            dstSchemaTable = srcSchemaTable.Copy();

            DataRow dr = dstSchemaTable.NewRow();

            dr = dstSchemaTable.NewRow();
            dr["ColumnName"] = "DW_Insert_Date";
            dr["ColumnSize"] = 16;
            dr["DataType"] = System.Type.GetType("System.DateTime");
            dr["NumericPrecision"] = 27;
            dr["NumericScale"] = 7;
            dr["AllowDBNull"] = true;
            dr["IsKey"] = false;
            dstSchemaTable.Rows.Add(dr);

            dr = dstSchemaTable.NewRow();
            dr["ColumnName"] = "DW_Delete_Date";
            dr["ColumnSize"] = 16;
            dr["DataType"] = System.Type.GetType("System.DateTime");
            dr["NumericPrecision"] = 27;
            dr["NumericScale"] = 7;
            dr["AllowDBNull"] = true;
            dr["IsKey"] = false;
            dstSchemaTable.Rows.Add(dr);

            dr = dstSchemaTable.NewRow();
            dr["ColumnName"] = "DW_Row_Hash";
            dr["ColumnSize"] = 16;
            dr["DataType"] = System.Type.GetType("System.Byte[]");
            dr["ColumnSize"] = 32;
            dr["AllowDBNull"] = true;
            dr["IsKey"] = false;
            dstSchemaTable.Rows.Add(dr);

            using (SqlConnection conn = (SqlConnection)repo.GetConnection())
            {
                string sql = SqlTableCreator.GetCreateSQL(Table.Dest_Schema, Table.Table_Name + "_Archive", dstSchemaTable);
                logger.Info("Creating table {0}_Archive with script {1}", Table.Table_Name, sql);
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                cmd.ExecuteNonQuery();

                conn.Close();
            }
        }


        private bool SchemasMatch()
        {
            // Compare the schematables of the source and destinate table

            if (dstSchemaTable.Rows.Count != srcSchemaTable.Rows.Count + 3) 
                return false;

            for (int i = 0; i < srcSchemaTable.Rows.Count; i++)
            {
                string srcType = SqlTableCreator.SQLGetType(srcSchemaTable.Rows[i]);
                string dstType = SqlTableCreator.SQLGetType(dstSchemaTable.Rows[i]);

                if(!srcType.Equals(dstType))
                    return false;
            }

            // The following three rows should be the metadata
            if(!dstSchemaTable.Rows[srcSchemaTable.Rows.Count]["ColumnName"].Equals("DW_Load_Date")) 
                return false;
            if(!dstSchemaTable.Rows[srcSchemaTable.Rows.Count+1]["ColumnName"].Equals("DW_Row_Hash")) 
                return false;

            return true;
        }

        private static void OnSqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        {
            Logger logger = LogManager.GetLogger("OracleDataImport");
            
            logger.Info("Copied {0} rows of {1} so far...", e.RowsCopied,((SqlBulkCopy)sender).DestinationTableName);
        }

    }



}
