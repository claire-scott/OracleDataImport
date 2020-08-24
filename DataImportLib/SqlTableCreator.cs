using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

/*
 * http://darrylagostinelli.com/2011/06/27/create-a-sql-table-from-a-datatable-in-c-net/
 *
 * -- Daryyl Agostinelli
 * I found some code about this topic — the code listing was a tiny bit incomplete, so I’ve corrected it and listed it here for all to see and use.
 *
 * Original Posting on MSDN - http://social.msdn.microsoft.com/Forums/en/adodotnetdataproviders/thread/4929a0a8-0137-45f6-86e8-d11e220048c3
 *
 * To download the code, hover over the code and look for a javascript popup to appear at the top right of the source code window.
 * 
*/


namespace CSU.PA.TJS.Toolkit
{
    public class SqlTableCreator
    {
        #region Instance Variables
        private SqlConnection _connection;
        public SqlConnection Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        private SqlTransaction _transaction;
        public SqlTransaction Transaction
        {
            get { return _transaction; }
            set { _transaction = value; }
        }

        private string _tableName;
        public string DestinationTableName
        {
            get { return _tableName; }
            set { _tableName = value; }
        }
        #endregion

        #region Constructor
        public SqlTableCreator() { }
        public SqlTableCreator(SqlConnection connection) : this(connection, null) { }
        public SqlTableCreator(SqlConnection connection, SqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }
        #endregion

        #region Instance Methods
        /*public object Create(DataTable schema)
        {
            return Create(schema, null);
        }
        public object Create(DataTable schema, int numKeys)
        {
            int[] primaryKeys = new int[numKeys];
            for (int i = 0; i < numKeys; i++)
            {
                primaryKeys[i] = i;
            }
            return Create(schema, primaryKeys);
        }
        public object Create(DataTable schema, int[] primaryKeys)
        {
            string sql = GetCreateSQL(null,_tableName, schema, primaryKeys);

            SqlCommand cmd;
            if (_transaction != null && _transaction.Connection != null)
                cmd = new SqlCommand(sql, _connection, _transaction);
            else
                cmd = new SqlCommand(sql, _connection);

            return cmd.ExecuteNonQuery();
        }*/

        public object CreateFromDataTable(DataTable table)
        {
            string sql = GetCreateFromDataTableSQL(_tableName, table);

            SqlCommand cmd;
            if (_transaction != null && _transaction.Connection != null)
                cmd = new SqlCommand(sql, _connection, _transaction);
            else
                cmd = new SqlCommand(sql, _connection);

            return cmd.ExecuteNonQuery();
        }
        #endregion

        #region Static Methods

        public static string GetCreateSQL(string SchemaName, string TableName, DataTable SchemaTable)
        {
            string sql;

            if(SchemaName == null)
            {
                sql = string.Format("CREATE TABLE [{1}] (\n", SchemaName, TableName);
            } else
            {
                sql = string.Format("CREATE TABLE [{0}].[{1}] (\n", SchemaName, TableName);
            }

            // columns
            foreach (DataRow column in SchemaTable.Rows)
            {
                if (!(SchemaTable.Columns.Contains("IsHidden") && (bool)column["IsHidden"]))
                {
                    sql += "\t[" + column["ColumnName"].ToString() + "] " + SQLGetType(column);

                    if (SchemaTable.Columns.Contains("AllowDBNull") && (bool)column["AllowDBNull"] == false)
                        sql += " NOT NULL";

                    sql += ",\n";
                }
            }
            sql = sql.TrimEnd(new char[] { ',', '\n' }) + "\n";

            // primary keys
            /*
            string pk = ", CONSTRAINT PK_" + TableName + " PRIMARY KEY CLUSTERED (";
            bool hasKeys = (primaryKeys != null && primaryKeys.Length > 0);
            if (hasKeys)
            {
                // user defined keys
                foreach (int key in primaryKeys)
                {
                    pk += schema.Rows[key]["ColumnName"].ToString() + ", ";
                }
            }
            else
            {
                // check schema for keys
                string keys = string.Join(", ", GetPrimaryKeys(schema));
                pk += keys;
                hasKeys = keys.Length > 0;
            }
            pk = pk.TrimEnd(new char[] { ',', ' ', '\n' }) + ")\n";
            if (hasKeys) sql += pk;
            */

            sql += ")";

            return sql;
        }

        public static string GetCreateFromDataTableSQL(string tableName, DataTable table)
        {
            string sql = "CREATE TABLE [" + tableName + "] (\n";
            // columns
            foreach (DataColumn column in table.Columns)
            {
                sql += "[" + column.ColumnName + "] " + SQLGetType(column) + ",\n";
            }
            sql = sql.TrimEnd(new char[] { ',', '\n' }) + "\n";
            // primary keys
            if (table.PrimaryKey.Length > 0)
            {
                sql += "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED (";
                foreach (DataColumn column in table.PrimaryKey)
                {
                    sql += "[" + column.ColumnName + "],";
                }
                sql = sql.TrimEnd(new char[] { ',' }) + "))\n";
            }

            //if not ends with ")"
            if ((table.PrimaryKey.Length == 0) && (!sql.EndsWith(")")))
            {
                sql += ")";
            }

            return sql;
        }

        public static string[] GetPrimaryKeys(DataTable schema)
        {
            List<string> keys = new List<string>();

            foreach (DataRow column in schema.Rows)
            {
                if (schema.Columns.Contains("IsKey") && (bool)column["IsKey"])
                    keys.Add(column["ColumnName"].ToString());
            }

            return keys.ToArray();
        }

        // Return T-SQL data type definition, based on schema definition for a column
        public static string SQLGetType(object type, int? columnSize, int? numericPrecision, int? numericScale)
        {
            switch (type.ToString())
            {
                case "System.String":
                    return "NVARCHAR(" + ((columnSize == -1) ? "255" : (columnSize > 8000) ? "MAX" : columnSize.ToString()) + ")";

                case "System.Byte[]":
                    if (columnSize == 32 || columnSize == 64)
                    {
                        // If it's 32 or 64 bytes long, it's probably a hash,
                        return "BINARY(" + columnSize.ToString() + ")";
                    }
                    else
                    {
                        return "VARBINARY(" + ((columnSize == -1) ? "255" : (columnSize > 8000) ? "MAX" : columnSize.ToString()) + ")";
                    }
                
                case "System.Decimal":
                    if (numericScale > 0 && numericScale != 255)
                        return "NUMERIC(" + numericPrecision.ToString() + "," + numericScale.ToString() + ")";
                    else if (numericPrecision > 10)
                        return "BIGINT";
                    else
                        return "INT";

                case "System.Double":
                case "System.Single":
                    return "NUMERIC(" + numericPrecision.ToString() + ","+numericScale.ToString()+")";

                case "System.Int64":
                    return "BIGINT";

                case "System.Int16":
                case "System.Int32":
                    return "INT";

                case "System.DateTime":
                    return "DATETIME2";

                case "System.Boolean":
                    return "BIT";

                case "System.Byte":
                    return "TINYINT";

                case "System.Guid":
                    return "UNIQUEIDENTIFIER";

                default:
                    throw new Exception(type.ToString() + " not implemented.");
            }
        }



        // Overload based on row from schema table
        public static string SQLGetType(DataRow schemaRow)
        {
            string type;

            type = SQLGetType(schemaRow["DataType"],
                                Convert.IsDBNull(schemaRow["ColumnSize"]) ? 0 : System.Convert.ToInt32(schemaRow["ColumnSize"].ToString()),
                                Convert.IsDBNull(schemaRow["NumericPrecision"]) ? 0 : System.Convert.ToInt32(schemaRow["NumericPrecision"].ToString()),
                                Convert.IsDBNull(schemaRow["NumericScale"]) ? 0 : System.Convert.ToInt32(schemaRow["NumericScale"].ToString()));

            return type;
        }
        // Overload based on DataColumn from DataTable type
        public static string SQLGetType(DataColumn column)
        {
            return SQLGetType(column.DataType, column.MaxLength, 10, 2);
        }
        #endregion
    }
}