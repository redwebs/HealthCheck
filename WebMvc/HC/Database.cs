using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using log4net;
using WebMvc.CsLib;
using WebMvc.Models;

namespace WebMvc.HC
{
    public class Database
    {
        protected static readonly ILog Log = LogManager.GetLogger("Healthchk");

        private static bool ItemFailed;

        public static bool RegularCheck()
        {
            ////////////////////////////////////////// Regular Health Check ///////////////////////////////////////////////

            // We want to do a simple health check.
            //  In some environments the load balancer will call it as often as every five seconds

            // 1. Database

            var chkMsg = CheckDatabases();

            // System Version

            if (ItemFailed)
            {
                Log.FatalFormat("Regular Database check failed: {0}", chkMsg);
            }
            return !ItemFailed;
        }

        protected static string CheckDatabases()
        {
            // Just do a quick check suitable for a frequent load balancer call

            var db1Status = CheckDbConnection("DB1");
            var db2Status = CheckDbConnection("DB2");

            if (ItemFailed)
            {
                return String.Format("{0}{1}", db1Status, db2Status);
            }
            return "OK";
        }

        protected static string CheckDbConnection(string connStrKey)
        {
            var connString = Helper.GetConnString(connStrKey);

            if (connString == string.Empty)
            {
                ItemFailed = true;
                return string.Format("Connection String {0} does not exist", connStrKey);
            }
            // If it's going to time out, we don't want to wait the standard 30 seconds.
            connString += "; Connection Timeout=5";

            using (var conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                }
                catch (SqlException sqlE)
                {
                    ItemFailed = true;
                    return String.Format("Connection Key {0}, Sql Connection Error: {1} ", connStrKey,
                        sqlE.Message);
                }
            }
            return string.Empty;
        }

        public static string GetVersionInfoWebApp()
        {
            const string missingMsg = "Not Found";
            var sb = new StringBuilder();
            sb.Append("Application Version Info");

            var version = Assembly.GetExecutingAssembly().GetName().Version;

            if (version == null)
            {
                sb.Append(missingMsg);
            }
            else
            {
                sb.AppendFormat("Version {0}", version);
                sb.AppendFormat("Major {0}", version.Major);
                sb.AppendFormat("Minor {0}", version.Minor);
                sb.AppendFormat("Build {0}", version.Build);
                sb.AppendFormat("Revison {0}", version.Revision);
            }
            return sb.ToString();
        }
    }
}