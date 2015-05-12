using System;
using System.Collections.Generic;
using System.Data;
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

        public static bool RegularCheck()
        {
            ////////////////////////////////////////// Regular Health Check ///////////////////////////////////////////////

            // We want to do a simple health check.
            //  In some environments the load balancer will call it as often as every five seconds

            // 1. Database

            var chkMsg = CheckDatabases();

            if (chkMsg != String.Empty)
            {
                Log.FatalFormat("Regular Database check failed: {0}", chkMsg);
                return false;
            }
            return true;
        }

        public static HealthCheckSection CheckDatabasesEx()
        {
            var chkMsg = CheckDatabases();

            var entry1 = new HealthCheckEntry
            {
                ItemName = "Database connectivity",
                Result = chkMsg != "OK",
                ResultDescription = chkMsg,
                OddRow = false
            };

            var section = new HealthCheckSection
            {
                Title = "Database",
                Entries = new List<HealthCheckEntry>()
            };

            section.Entries.Add(entry1);

            if (!entry1.Result)
            {
                // Can't get to db to make more checks
                return section;
            }

            var entry2 = CheckDbStoredProc("DB1", "GetSubjectBedTime", "@uid", true);
            var entry3 = CheckDbStoredProc("DB2", "uspWorkoutCategory", "@workoutCategoryId", false);
            var entry4 = CheckDbSrchInstalled("DB2", true);
            var entry5 = CheckDbDateTime("DB1", false);
            var entry6 = new HealthCheckEntry()
            {
                ItemName = "Windows Server DateTime.Now",
                Result = true,
                ResultDescription = DateTime.Now.ToString(),
                OddRow = true
            };

            section.Entries.Add(entry2);
            section.Entries.Add(entry3);
            section.Entries.Add(entry4);
            section.Entries.Add(entry5);
            section.Entries.Add(entry6);

            return section;
        }

        protected static string CheckDatabases()
        {
            // Just do a quick check suitable for a frequent load balancer call

            var db1Status = CheckDbConnection("DB1");
            var db2Status = CheckDbConnection("DB2");

            if (db1Status.Item2 || db2Status.Item2)
            {
                return String.Format("{0}{1}", db1Status.Item1, db2Status.Item1);
            }
            return "OK";
        }

        protected static Tuple<string, bool> CheckDbConnection(string connStrKey)
        {
            var connString = Helper.GetConnString(connStrKey);

            if (connString == string.Empty)
            {
                return new Tuple<string, bool>(string.Format("Connection String {0} does not exist", connStrKey), false);
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
                    return new Tuple<string, bool>(String.Format("Connection Key {0}, Sql Connection Error: {1} ", connStrKey,
                        sqlE.Message), false);
                }
            }
            return new Tuple<string, bool>(String.Empty, true);
        }

        protected static HealthCheckEntry CheckDbStoredProc(string connStringKey, string spName, string paramName, bool oddRow)
        {
            var spResult = DbHelper.SqlStoredProcedureAccessCheck(Helper.GetConnString(connStringKey), spName, paramName, 1);

            return new HealthCheckEntry()
            {
                ItemName = String.Format("Execute {0} {1}", connStringKey, spName),
                Result = spResult.Item2,
                ResultDescription = spResult.Item1,
                OddRow = oddRow
            };
        }

        protected static HealthCheckEntry CheckDbSrchInstalled(string connStringKey, bool oddRow)
        {
            var textSrchInstalled = DbHelper.GetServerProperty(Helper.GetConnString(connStringKey),
                "IsFullTextInstalled");

            return new HealthCheckEntry()
            {
                ItemName = String.Format("{0} Full Text Search", connStringKey),
                Result = textSrchInstalled == 1,
                ResultDescription = textSrchInstalled == 1 ? "Installed" : "Not Installed",
                OddRow = oddRow
            };
        }

        protected static HealthCheckEntry CheckDbDateTime(string connStringKey, bool oddRow)
        {
            var dataTable = DbHelper.GetDataTableConStrKey(connStringKey,
                "SELECT GETDATE() fn_GetDate, SYSDATETIME() fn_SysDateTime");
            DataRow dataRow = null;

            if (dataTable.TableName.Equals("MyData"))
            {
                dataRow = dataTable.Rows[0];
            }

            return new HealthCheckEntry()
            {
                ItemName = String.Format("{0} SQL Server GetDate()", connStringKey),
                Result = dataRow != null,
                ResultDescription = dataRow != null ? dataRow[0].ToString() : "Unable to get SQL time",
                OddRow = oddRow
            };
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