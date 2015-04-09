
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using log4net;
using Newtonsoft.Json;

public partial class Healthcheck : System.Web.UI.Page
{
    #region variables

    protected StringBuilder Sb = new StringBuilder();
    protected bool ItemFailed = false;
    protected bool OddRow = true;
    protected HealthCheckQueryString ReqQueryStr;
    protected static readonly ILog Log = LogManager.GetLogger("Healthchk");

    #endregion

    #region constants

    protected const string OddRowClass = "class='odd'";
    protected const string FailRowClass = "class='fail'";

    protected const string HtmlTableBottom = "</table>";
    protected const string HtmlBottom = "</div></body></html>";

    // Name of the web site
    protected const string WebSiteName = "Mighty Site";

    // The key names of the connection strings
    protected const string Db1ConStrKey = "DB1";
    protected const string Db2ConStrKey = "DB2";

    #endregion

    #region Page Events

    protected void Page_Load(object sender, EventArgs e)
    {
        ////////////////////////////////////////// Regular Health Check ///////////////////////////////////////////////

        // We want to check for the simple health check first which will have no query string
        //  In our environment the load balancer will call it as often as every five seconds

        if (Request.QueryString.Count.Equals(0))
        {
            var result = RegularCheck();

            if (ItemFailed)
            {
                Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                Response.ContentType = "text/plain";
            }
            Log.FatalFormat("Regular Database check failed: {0}", result);
            Response.Write(result);
            Response.End();
        }

        // Get Query String into an object

        ReqQueryStr =
            Helper.QueryStringObject<HealthCheckQueryString>(HttpUtility.UrlDecode(Request.QueryString.ToString()));

        ///////////////////////////////////////////// Get Version /////////////////////////////////////////////////

        if (!String.IsNullOrEmpty(ReqQueryStr.GetVersion))
        {
            // get a simple one line edition of the version
            GetVersionInfo();
            Response.Write(Sb.ToString());
            return;
        }

        ////////////////////////////////////////// Detailed Health Check ///////////////////////////////////////////////

        if (!String.IsNullOrEmpty(ReqQueryStr.Details))
        {
            var result = DetailCheck();
            Response.Write(result);
            Response.End();
        }

        var msg = String.Format("Healthcheck failed to determine the appropriate action to take for Query String: {0}",
            HttpUtility.UrlDecode(Request.QueryString.ToString()));
        Log.Error(msg);
        Response.StatusCode = (int) HttpStatusCode.InternalServerError;
        Response.ContentType = "text/plain";
        Response.Write(msg);
        Response.End();
    }

    #endregion

    #region Main HC Functions

    protected string RegularCheck()
    {
        // 1. Database

        Sb.Append(CheckDatabases());

        // System Version

        if (ItemFailed)
        {
            GetVersionInfo();
        }

        return Sb.ToString();
    }

    protected string DetailCheck()
    {
        Sb.Append(HtmlBody(Helper.GetAppSetting("ProgramName")));

        // ----------------------- Databases --------------------------
        
        Sb.Append(HtmlTableTop("Databases"));

        var connStatus = CheckDatabases();

        if (connStatus.Equals("OK"))
        {
            Sb.Append(HtmlTableRow(true, "Database Connections", "All Databases Connected"));
            CheckDbStoredProc();
        }
        else
        {
            Sb.Append(HtmlTableRow(false, "Database Connections", connStatus));
            ItemFailed = true;
        }
        Sb.Append(HtmlTableBottom);

        // ----------------------- SSO Service --------------------------

        Sb.Append(HtmlTableTop("Single Sign On Service"));

        string chkSso;

        var endPoint = SiteSettings.MightySiteSSOurl + "/healthcheck?details=true";

        if (!String.IsNullOrEmpty(SiteSettings.SSOClient))
        {
            endPoint += "&login_id=" + SiteSettings.SSOClient;
        }

        var ssoStatus = ServiceHealthCheckEx(endPoint, out chkSso);

        if (ssoStatus.Contains("Somefile.css"))
        {
            // MightySite SSO requires a login to run healthcheck, a css file link in the HTML returned clues us in
            //  that we have been sent to a login page
            Sb.Append(HtmlTableRow(false, "SSO Health Check", "Redirected to login"));
            ItemFailed = true;
            Log.Debug("Healthcheck SSO was redirected to login.");
        }
        else
        {
            if (chkSso == "OK")
            {
                Sb.Append(HtmlTableRow(true, "SSO Health Check HTTP Return Code", chkSso));
                var theList = Helper.GetHtmlListItems(ssoStatus, '-');

                foreach (var tuple in theList)
                {
                    Sb.Append(HtmlTableRow(true, tuple.Item1, tuple.Item2));
                }
            }
            else
            {
                if (chkSso == "Exception")
                {
                    Sb.Append(HtmlTableRow(false, "SSO Health Check Exception", ssoStatus));

                }
                else
                {
                    Sb.Append(HtmlTableRow(false, "SSO Health Check HTTP Return Code", chkSso));
                }
                var uri = new Uri(SiteSettings.MightySiteSSOurl);
                var dnsAddr = Helper.GetHostDnsAddress(uri.Host);
                Sb.Append(HtmlTableRow(false, "SSO DNS lookup", dnsAddr.Item2));

            }
        }

        Sb.Append(HtmlTableBottom);

        // ----------------------- Data Service --------------------------

        Sb.Append(HtmlTableTop("Data Service"));

        string chkData;
       
        endPoint = SiteSettings.MightySiteDataDomain + "healthcheck";
        endPoint += "?details=true";
        var dataStatus = ServiceHealthCheckEx(endPoint, out chkData);

        if (chkData == "OK")
        {
            Sb.Append(HtmlTableRow(true, "Data Health Check HTTP Return Code", chkData));
            var theList = Helper.GetHtmlListItems(dataStatus, '-');

            foreach (var tuple in theList)
            {
                Sb.Append(HtmlTableRow(true, tuple.Item1, tuple.Item2));
            }
        }
        else
        {
            if (chkData == "Exception")
            {
                Sb.Append(HtmlTableRow(false, "Data Health Check Exception", dataStatus));
            }
            else
            {
                Sb.Append(HtmlTableRow(false, "Data Health Check HTTP Return Code", chkData));
            }
            var uri = new Uri(SiteSettings.MightySiteDataDomain);
            var dnsAddr = Helper.GetHostDnsAddress(uri.Host);
            Sb.Append(HtmlTableRow(false, "Data DNS lookup", dnsAddr.Item2));
        }

        Sb.Append(HtmlTableBottom);

        // ----------------------- App Settings, Version, Time zones --------------------------

        ListAppSettings();
        GetVersionInfoTable();
        TimeZoneInformation();

        Sb.Append(HtmlBottom);
        return Sb.ToString();
    }

    protected void ListAppSettings()
    {
        Sb.Append(HtmlTableTop("App Settings - URLs"));

        Sb.Append(HtmlTableRow(true, "MightySiteDataDomain", SiteSettings.MightySiteDataDomain));
        Sb.Append(HtmlTableRow(true, "MightySiteModulesDomain", SiteSettings.MightySiteModulesDomain));
        Sb.Append(HtmlTableRow(true, "MightySiteDomain", SiteSettings.MightySiteDomain));
        Sb.Append(HtmlTableRow(true, "MightySiteSSOurl", SiteSettings.MightySiteSSOurl));
        Sb.Append(HtmlTableRow(true, "RedirectIfUnauthorized", SiteSettings.RedirectIfUnauthorized));
        Sb.Append(HtmlTableRow(true, "ZeeHost", SiteSettings.ZeeHost));
        Sb.Append(HtmlTableRow(true, "StaticContentPrefix", SiteSettings.StaticContentPrefix));
        Sb.Append(HtmlTableRow(true, "StaticContentPrefixAdmin", SiteSettings.StaticContentPrefixAdmin));
        Sb.Append(HtmlTableRow(true, "ThxWebPath", SiteSettings.ThxWebPath));

        Sb.Append(HtmlTableBottom);
        Sb.Append(HtmlTableTop("App Settings - GameTime"));

        Sb.Append(HtmlTableRow(true, "GameTimeAPI_URL", SiteSettings.GameTimeAPI_URL));
        Sb.Append(HtmlTableRow(true, "GameTimeScriptURL", SiteSettings.GameTimeScriptURL));
        Sb.Append(HtmlTableRow(true, "GameTimeServerPrefix", SiteSettings.GameTimeServerPrefix));
        Sb.Append(HtmlTableRow(true, "GameTimeWidgetKey", SiteSettings.GameTimeWidgetKey));
        Sb.Append(HtmlTableRow(true, "API_SITE", SiteSettings.API_SITE));

        Sb.Append(HtmlTableBottom);

        Sb.Append(HtmlTableTop("App Settings - DevCon"));
        Sb.Append(HtmlTableRow(true, "DevConSync", SiteSettings.DevConSync.ToString()));
        Sb.Append(HtmlTableRow(true, "DevConApiPrefix", SiteSettings.DevConApiPrefix));
        Sb.Append(HtmlTableRow(true, "DevConScfxUrl", SiteSettings.DevConXrfxUrl));
        Sb.Append(HtmlTableRow(true, "DevConUpdateMinutes", SiteSettings.DevConUpdateMinutes.ToString()));

        Sb.Append(HtmlTableBottom);
    }

    #endregion

    #region Remote Service

    protected bool ServiceHealthCheck(string url, out string status)
    {
        try
        {
            var httpClient = new HttpClient();

            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, certificate, chain, errors) => { return true; };

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            var response = httpClient.SendAsync(requestMessage).Result;

            status = response.StatusCode.ToString();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            ItemFailed = true;
            return false;
        }
        catch (Exception ex)
        {
            status = String.Format("GetAsync Exception: {0}", ex.Message);
            ItemFailed = true;

            if (ex.InnerException != null)
            {
                status += String.Format("GetAsync Inner Exception: {0}", ex.InnerException.Message);
            }
            Log.DebugFormat("Healthcheck.ServiceHealthCheck status: {0}, URL: {1}", status, url);
        }
        return false;
    }

    protected string ServiceHealthCheckEx(string url, out string status)
    {
        try
        {
            var httpClient = new HttpClient();

            // Handshake the HTTPS
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, certificate, chain, errors) => { return true; };

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            // Send the request to the server, SendAsync but ().Results makes it Sync
            var response = httpClient.SendAsync(requestMessage).Result;

            status = response.StatusCode.ToString(); // in detail chk for NotFound

            if (status == "OK")
            {
                return response.Content.ReadAsStringAsync().Result;
            }
            ItemFailed = true;
            return string.Format("HttpClient returned: {0}", status);
        }
        catch (Exception ex)
        {
            status = "Exception";
            var returnStr = String.Format("GetAsync Exception at URL: {0} Message: {1}", url, ex.Message);
            ItemFailed = true;

            if (ex.InnerException != null)
            {
                if (ex.InnerException.InnerException != null)
                {
                    // This is where "Unable to connect to the remote server" shows up
                    returnStr += " Exception: " + ex.InnerException.InnerException.Message;
                }
                else
                {
                    returnStr += " Inner Exception: " + ex.InnerException.Message;
                }
            }
            Log.DebugFormat("Healthcheck.ServiceHealthCheckEx status: {0}, URL: {1}", returnStr, url);
            return returnStr;
       }
    }

    #endregion
    
    #region Database

    protected string CheckDatabases()
    {
        // Just do a quick check suitable for a frequent load balancer call

        var db1Status = CheckDbConnection("DB1");
        var db2Status = CheckDbConnection("DB2");

        if (db1Status != string.Empty || db2Status != string.Empty)
        {
            return String.Format("{0}{1}", db1Status, db2Status);
        }
        return "OK";
    }

    protected string CheckDbConnection(string connStrKey)
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

    protected void CheckDbStoredProc()
    {
        var spResult = DbHelper.SqlStoredProcedureAccessCheck(Helper.GetConnString(Db1ConStrKey), "GetSubjectBedTime", "@uid", 1);

        if (spResult.Item2)
        {
            Sb.Append(HtmlTableRow(true, "Execute DB1.dbo.GetSubjectBedTime", "OK"));
        }
        else
        {
            Sb.Append(HtmlTableRow(false, "Execute DB1.dbo.GetSubjectBedTime", spResult.Item1));
            ItemFailed = true;
        }

        spResult = DbHelper.SqlStoredProcedureAccessCheck(Helper.GetConnString(Db2ConStrKey), "uspWorkoutCategory",
                "@workoutCategoryId", 2);

        if (spResult.Item2)
        {
            Sb.Append(HtmlTableRow(true, "Execute DB2.dbo.uspWorkoutCategory", "OK"));
        }
        else
        {
            Sb.Append(HtmlTableRow(false, "Execute DB2.dbo.uspWorkoutCategory", spResult.Item1));
            ItemFailed = true;
        }

        var textSrchInstalled = DbHelper.GetServerProperty(Helper.GetConnString(Db2ConStrKey), "IsFullTextInstalled");

        if (textSrchInstalled == 1)
        {
            Sb.Append(HtmlTableRow(true, "DB2 Full text search", "Installed"));
        }
        else
        {
            Sb.Append(HtmlTableRow(false, "DB2 Full text search", "Not Installed"));
            ItemFailed = true;
        }

        var dataTable = DbHelper.GetDataTableConStrKey(Db1ConStrKey, "SELECT GETDATE() fn_GetDate, SYSDATETIME() fn_SysDateTime");
        DataRow dataRow = null;

        if (dataTable.TableName.Equals("MyData"))
        {
            dataRow = dataTable.Rows[0];
        }
        if (dataRow != null)
        {
            Sb.Append(HtmlTableRow(true, "SQL Server GetDate()", dataRow[0].ToString()));
            Sb.Append(HtmlTableRow(true, "Windows Server DateTime.Now", DateTime.Now.ToString()));
        }
        else
        {
            Sb.Append(HtmlTableRow(false, "GetDate()", "Unable to get SQL time"));
        }
    }

    #endregion

    #region Version

    public void GetVersionInfo()
    {
        Sb.Append("Version Info: ");
        Sb.Append(GetGlobalResourceObject("About", "Major"));
        Sb.Append(".");
        Sb.Append(GetGlobalResourceObject("About", "Minor"));
        Sb.Append(".");
        Sb.Append(GetGlobalResourceObject("About", "Build"));
        Sb.Append(".");
        Sb.Append(GetGlobalResourceObject("About", "Increment"));
        Sb.Append(DateTime.Now);
    }

    public void GetVersionInfoTable()
    {
        // This function is for reading an about.resx file included in the App_GlobalResources site folder

        const string missingMsg = "Not Found";
        Sb.Append(HtmlTableTop("Version Info"));

        var globalResourceObject = GetGlobalResourceObject("About", "Major");
        Sb.Append(HtmlTableRow(true, "Major ", (globalResourceObject == null) ? missingMsg : globalResourceObject.ToString()));

        globalResourceObject = GetGlobalResourceObject("About", "Minor");
        Sb.Append(HtmlTableRow(true, "Minor ", (globalResourceObject == null) ? missingMsg : globalResourceObject.ToString()));

        globalResourceObject = GetGlobalResourceObject("About", "Patch");
        Sb.Append(HtmlTableRow(true, "Patch ", (globalResourceObject == null) ? missingMsg : globalResourceObject.ToString()));
        
        globalResourceObject = GetGlobalResourceObject("About", "Build");
        Sb.Append(HtmlTableRow(true, "Build ", (globalResourceObject == null) ? missingMsg : globalResourceObject.ToString()));

        globalResourceObject = GetGlobalResourceObject("About", "Increment");
        Sb.Append(HtmlTableRow(true, "Increment ", (globalResourceObject == null) ? missingMsg : globalResourceObject.ToString()));
        Sb.Append(HtmlTableBottom);
    }

    #endregion

    #region HTML Tables

    protected string HtmlBody(string serverName)
    {
        return
            String.Format(
                @"<!DOCTYPE html><html><head><title>HC:{0}</title><link rel='stylesheet' href='Content/HealthCheck.css' type='text/css'></head><body><div><table width=90% cellspacing=2 cellpadding=4 cols=2><thead><tr class='odd'><th colspan='2' scope='col' abbr='Home'>{0} Healthcheck: {1}</th></tr></thead></table>",
                WebSiteName, Request.Url.GetComponents(UriComponents.Host, UriFormat.SafeUnescaped));
    }

    protected string HtmlTableTop(string sectionName)
    {
        OddRow = false;
        return
            String.Format(
                @"<table width=90% cellspacing=2 cellpadding=4 cols=2><thead><tr class='odd'><th colspan='2' scope='col' abbr='Home'>{0}</th></tr></thead>",
                sectionName);
    }

    protected string HtmlTableRow(bool passed, string item, string result)
    {
        var classDeclaration = string.Empty;

        if (passed)
        {
            if (OddRow)
            {
                classDeclaration = OddRowClass;
            }
        }
        else
        {
            classDeclaration = FailRowClass;
        }
        OddRow = !OddRow;

        return String.Format(@"<tr {0}><td>{1}</td><td>{2}</td>", classDeclaration, item, result);
    }

    #endregion

    #region Timezone
    
    public void TimeZoneInformation()
    {
        const string timeFmt = "yyyy-MM-dd HH:mm";

        // Get the local time zone and the current local time and year.
        var localZone = TimeZone.CurrentTimeZone;
        var currentDate = DateTime.Now;
        var currentYear = currentDate.Year;

        Sb.Append(HtmlTableTop("Server Time Zone Information"));
        // Display the names for standard time and daylight saving time for the local time zone.
        Sb.Append(HtmlTableRow(true, "Standard time name", localZone.StandardName));
        Sb.Append(HtmlTableRow(true, "Daylight saving time name", localZone.DaylightName));

        // Display the current date and time and show if they occur in daylight saving time.
        Sb.Append(HtmlTableRow(true, "Current date and time:", currentDate.ToString(timeFmt)));
        Sb.Append(HtmlTableRow(true, "Daylight saving time?", localZone.IsDaylightSavingTime(currentDate).ToString()));

        // Get the current Coordinated Universal Time (UTC) and UTC offset.
        var currentUtc = localZone.ToUniversalTime(currentDate);
        var currentOffset = localZone.GetUtcOffset(currentDate);

        Sb.Append(HtmlTableRow(true, "Coordinated Universal Time", currentUtc.ToString(timeFmt)));
        Sb.Append(HtmlTableRow(true, "UTC offset", currentOffset.ToString()));

        // Get the DaylightTime object for the current year.
        DaylightTime daylight = localZone.GetDaylightChanges(currentYear);

        // Display the daylight saving time range for the current year.
        Sb.Append(HtmlTableRow(true, String.Format("Daylight saving time {0} Start", currentYear), daylight.Start.ToString(timeFmt)));
        Sb.Append(HtmlTableRow(true, String.Format("Daylight saving time {0} End", currentYear), daylight.End.ToString(timeFmt)));
        Sb.Append(HtmlTableRow(true, String.Format("Daylight saving time {0} Delta", currentYear), daylight.Delta.ToString()));

        Sb.Append(HtmlTableBottom);
    }

    #endregion

    #region Email

    // These are not called or displayed above, but are a template for displaying specific data from a database

    public int GetEmailQueue()
    {
        var dt = DbHelper.GetDataTableConStrKey(Db1ConStrKey,"Select count(*) 'Queued' from EmailQueue where nextProcessingTime < getdate() ");
 
        if (dt.Rows.Count != 0)
        {
            return (int)dt.Rows[0]["queued"];
        }
        return 0;
    }

    public int GetTotalInEmailQueue() 
    {
        var dt = DbHelper.GetDataTableConStrKey(Db1ConStrKey, "select count(*) 'Stored' from EmailQueue;");

        if (dt.Rows.Count != 0)
        {
            return (int) dt.Rows[0]["Stored"];
        } 
        return 0;
    }
    #endregion
}

// This is a class used by Helper.QueryStringObject to convert the query string into a class
//  This comes in handy when you have a lot of query string variables

public class HealthCheckQueryString
{
    [JsonProperty("getversion")]
    public string GetVersion { get; set; } // Just return the system version

    [JsonProperty("details")]
    public string Details { get; set; } // If present do a detailed HC

    public HealthCheckQueryString()
    {
        // Set any defaults here
        Details = "true";
    }
}

