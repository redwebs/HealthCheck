using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace WebMvc.CsLib
{
    public static class DbHelper
    {
//    private static readonly ILog Log = LogManager.GetLogger("Healthchk");

        public static int DatabaseTimeoutSeconds = 60;

        public static Tuple<string, bool> SqlStoredProcedureAccessCheck(string connString, string storedProcName,
            string paramName, int paramVal)
        {
            var sqlCommand = new SqlCommand();
            var dataTable = new DataTable(storedProcName);
            sqlCommand.Parameters.Add(new SqlParameter(paramName, paramVal));

            using (var sqlConn = new SqlConnection(connString))
            {
                try
                {
                    sqlConn.Open();
                    sqlCommand.CommandTimeout = DatabaseTimeoutSeconds;
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = storedProcName;
                    sqlCommand.Connection = sqlConn;

                    var dataAdapter = new SqlDataAdapter(sqlCommand);
                    dataAdapter.Fill(dataTable);
                    return new Tuple<string, bool>(storedProcName + " was executed successfully.", true);
                }
                catch (Exception ex)
                {
                    string msg = String.Format("Error executing Stored Proc: {0}, Message: {1}", storedProcName,
                        ex.Message);
 //                   Log.Error(msg);
                    return new Tuple<string, bool>(msg, false);
                }
            }
        }

        public static int GetServerProperty(string connString, string propertyName)
        {
            var sqlCommand = new SqlCommand();
            SqlConnection sqlConn = null;

            try
            {
                sqlConn = new SqlConnection(connString);
                sqlConn.Open();

                sqlCommand.CommandTimeout = DatabaseTimeoutSeconds;
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = string.Format("select SERVERPROPERTY('{0}')", propertyName);
                sqlCommand.Connection = sqlConn;

                using (var reader = sqlCommand.ExecuteReader())
                {
                    reader.Read();
                    var val = reader.GetInt32(0);

                    return val;
                }
            }
            catch (Exception ex)
            {
                string msg = "Error in Database.GetServerProperty\r\n\tProperty: " +
                             propertyName + "\r\n\tMessage:" + ex.Message;
                //Log.Error(msg);
            }
            finally
            {
                if (sqlConn != null)
                {
                    sqlConn.Close();
                }
            }
            return -100;
        }

        public static bool SqlStoredProcExists(string database, string procedureName)
        {
            var query = String.Format("select * from sysobjects where type='P' and name='{0}'", procedureName);

            using (
                var conn =
                    new SqlConnection(GetConnStrForDb(ConfigurationManager.ConnectionStrings[database].ConnectionString))
                )
            {
                conn.Open();
                using (var command = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static string GetConnStrForDb(string database)
        {
            return ConfigurationManager.ConnectionStrings[database].ConnectionString;
        }

        public static DataTable GetDataTableConStrKey(string connStrKey, string sqlQuery)
        {
            return GetDataTable(ConfigurationManager.ConnectionStrings[connStrKey].ConnectionString, sqlQuery);
        }

        public static DataTable GetDataTable(string connString, string sqlQuery)
        {
            try
            {
                using (var sqlConn = new SqlConnection(connString))
                {
                    sqlConn.Open();

                    var sqlCommand = sqlConn.CreateCommand();
                    sqlCommand.CommandType = CommandType.Text;
                    sqlCommand.CommandText = sqlQuery;
                    sqlCommand.Connection = sqlConn;
                    var dataAdapter = new SqlDataAdapter(sqlCommand);
                    var dataTable = new DataTable("MyData");
                    dataAdapter.Fill(dataTable);
                    return dataTable;
                }
            }
            catch (Exception ex)
            {
//                Log.ErrorFormat("Error in Database.SqlQuery\r\n\tConn String: {0}\r\n\t Query: {1}\r\n\tMessage: {2}",
  //                  connString, sqlQuery, ex.Message);
            }
            return new DataTable("Empty");
        }

        public static string DataTableToCsvString(DataTable dt)
        {
            var sbReport = new StringBuilder();
            var nRowCntr = 0;

            for (var nFldH = 0; nFldH < dt.Columns.Count - 1; nFldH++)
            {
                sbReport.Append("\"");
                sbReport.Append(dt.Columns[nFldH].ColumnName);
                sbReport.Append("\",");
            }
            sbReport.Append("\"");
            sbReport.Append(dt.Columns[dt.Columns.Count - 1].ColumnName);
            sbReport.Append("\"\n");

            for (nRowCntr = 0; nRowCntr < dt.Rows.Count; nRowCntr++)
            {
                for (var nFld = 0; nFld < dt.Columns.Count - 1; nFld++)
                {
                    sbReport.Append("\"");
                    sbReport.Append(dt.Rows[nRowCntr][nFld]);
                    sbReport.Append("\",");
                }
                sbReport.Append("\"");
                sbReport.Append(dt.Rows[nRowCntr][dt.Columns.Count - 1]);
                sbReport.Append("\"\n");
            }
            return sbReport.ToString();
        }


    }
}