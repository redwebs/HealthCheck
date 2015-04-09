
/// <summary>
/// Class to read/return SiteSettings
/// </summary>
/// 
public static class SiteSettings
{
    static SiteSettings()
    {
        API_SITE = Helper.GetAppSetting("API_SITE");
        AuthSource = Helper.GetAppSetting("AuthSource");
        AvatarStaticContentPrefix = Helper.GetAppSetting("AvatarStaticContentPrefix", "/");
        GameTimeAPI_URL = Helper.GetAppSetting("GameTimeAPI_URL");
        GameTimeScriptURL = Helper.GetAppSetting("GameTimeScriptURL");
        GameTimeServerPrefix = Helper.GetAppSetting("GameTimeServerPrefix");
        GameTimeWidgetKey = Helper.GetAppSetting("GameTimeWidgetKey");
        GameTimeTimeOut = Helper.GetAppSettingInt("GameTimeTimeOut", 10000);
        GameTimeSuppressLog = Helper.GetAppSettingBool("GameTimeSuppressLog", true);
        CryptKey = Helper.GetAppSetting("CryptKey");
        ErrorReportsIdentString = Helper.GetAppSetting("ErrorReportsIdentString");
        FacebookAppId = Helper.GetAppSetting("FacebookAppId");
        FacebookChannelUrlSuffix = Helper.GetAppSetting("FacebookChannelUrlSuffix");
        IfNoUserIDRedirectTo = Helper.GetAppSetting("IfNoUserIDRedirectTo");
        IsTesting = Helper.GetAppSetting("IsTesting");
        ThxLocationPath = Helper.GetAppSetting("ThxLocationPath");
        ThxWebPath = Helper.GetAppSetting("ThxWebPath");
        OnlyExternalUsers = Helper.GetAppSetting("OnlyExternalUsers");
        ThxDirectory = Helper.GetAppSetting("ThxDirectory");
        ThxId = Helper.GetAppSettingInt("ThxId", 7);
        ThxName = Helper.GetAppSetting("ThxName");
        ThxDurationInWeeks = Helper.GetAppSettingInt("ThxDurationInWeeks", 4);
        ThxNbrPhases = Helper.GetAppSettingInt("ThxNbrPhases", 3);
        RedirectIfUnauthorized = Helper.GetAppSetting("RedirectIfUnauthorized");
        ZeeHost = Helper.GetAppSetting("ZeeHost");
        MightySiteDataDomain = Helper.GetAppSetting("MightySiteDataDomain");
        MightySiteModulesDomain = Helper.GetAppSetting("MightySiteModulesDomain");
        SSOClient = Helper.GetAppSetting("SSOClient");
        MightySiteSSOUser = Helper.GetAppSetting("MightySiteSSOUser");
        MightySiteSSOPassword = Helper.GetAppSetting("MightySiteSSOPassword");
        MightySiteSSOurlAdditional = Helper.GetAppSetting("MightySiteSSOurlAdditional");
        MightySiteDomain = Helper.GetAppSetting("MightySiteDomain");
        MightySiteLoginChallengePassword = Helper.GetAppSetting("MightySiteLoginChallengePassword");
        MightySiteLoginChallengeUsername = Helper.GetAppSetting("MightySiteLoginChallengeUsername");
        MightySiteSSOurl = Helper.GetAppSetting("MightySiteSSOurl");
        StaticContentPrefix = Helper.GetAppSetting("StaticContentPrefix", "/");
        StaticContentPrefixAdmin = Helper.GetAppSetting("StaticContentPrefixAdmin");
        ThemeType = Helper.GetAppSetting("ThemeType");
        TrainerRoleId = Helper.GetAppSetting("TrainerRoleId", "4");
        Debug = Helper.GetAppSetting("Debug");
        wsauthkey = Helper.GetAppSetting("wsauthkey");
        wsauthkeyname = Helper.GetAppSetting("wsauthkeyname", "Authorization-Key");
        MightySiteWallPassword = Helper.GetAppSetting("MightySiteWallPassword");
        MightySiteWallUser = Helper.GetAppSetting("MightySiteWallUser");
        MightySitePresent = Helper.GetAppSettingBool("MightySitePresent", true);
        GameTimePresent = Helper.GetAppSettingBool("GameTimePresent", true);
        XrServiceAttemptsNo = Helper.GetAppSettingInt("XrServiceAttemptsNo", 5);
        StaticSource = Helper.GetAppSetting("StaticSource");
        StaticCloudFrontURL = Helper.GetAppSetting("StaticCloudFrontURL");
        StaticAmazonZeeURL = Helper.GetAppSetting("StaticAmazonZeeURL");
        RefreshTokenInterval = Helper.GetAppSettingInt("RefreshTokenInterval", -1);
        EnableHealthTrackers = Helper.GetAppSettingBool("EnableHealthTrackers", false);
        DevConSync = Helper.GetAppSettingBool("DevConSync", false);
        DevConXrfxUrl = Helper.GetAppSetting("DevConXrfxUrl", string.Empty);
        DevConApiPrefix = Helper.GetAppSetting("DevConApiPrefix", string.Empty);
        DevConUpdateMinutes = Helper.GetAppSettingInt("DevConUpdateMinutes", 2);
    }

    public static string API_SITE { get; set; }
    public static string AuthSource { get; set; }
    public static string AvatarStaticContentPrefix { get; set; }
    public static string GameTimeAPI_URL { get; set; }
    public static string GameTimeScriptURL { get; set; }
    public static string GameTimeServerPrefix { get; set; }
    public static string GameTimeWidgetKey { get; set; }
    public static int GameTimeTimeOut { get; set; }
    public static bool GameTimeSuppressLog { get; set; }
    public static string CryptKey { get; set; }
    public static string EnableTimeTravel { get; set; }
    public static string ErrorReportsIdentString { get; set; }
    public static string ExerciseStaticContentPrefix { get; set; }
    public static string FacebookAppId { get; set; }
    public static string FacebookChannelUrlSuffix { get; set; }
    public static string IfNoUserIDRedirectTo { get; set; }
    public static string IsTesting { get; set; }
    public static string IsSFT { get; set; }
    public static string LogoutIfNoSCtoken { get; set; }
    public static string MemberRoleId { get; set; }
    public static string ThxLocationPath { get; set; }
    public static string ThxWebPath { get; set; }
    public static string OnlyExternalUsers { get; set; }
    public static string PGPFilePath { get; set; }
    public static string PGPenabled { get; set; }
    public static string PGPhomedirectory { get; set; }
    public static string PGPoriginator { get; set; }
    public static string PGPpassphrase { get; set; }
    public static string PGPrecipient { get; set; }
    public static string ThxDirectory { get; set; }
    public static int ThxId { get; set; }
    public static string ThxName { get; set; }
    public static int ThxDurationInWeeks { get; set; }
    public static int ThxNbrPhases { get; set; }
    public static string RedirectIfUnauthorized { get; set; }
    public static string ZeeHost { get; set; }
    public static string MightySiteDataDomain { get; set; }
    public static string MightySiteModulesDomain { get; set; }
    public static string SSOClient { get; set; }
    public static string MightySiteSSOUser { get; set; }
    public static string MightySiteSSOPassword { get; set; }
    public static string MightySiteSSOurlAdditional { get; set; }
    public static string MightySiteDomain { get; set; }
    public static string MightySiteLoginChallengePassword { get; set; }
    public static string MightySiteLoginChallengeUsername { get; set; }
    public static string MightySiteSSOurl { get; set; }
    public static string StaticContentPrefix { get; set; }
    public static string StaticContentPrefixAdmin { get; set; }
    public static string ThemeType { get; set; }
    public static string TrainerRoleId { get; set; }
    public static string Debug { get; set; }
    public static string wsauthkey { get; set; }
    public static string wsauthkeyname { get; set; }
    public static string MightySiteWallPassword { get; set; }
    public static string MightySiteWallUser { get; set; }
    public static bool MightySitePresent { get; set; }
    public static bool GameTimePresent { get; set; }
    public static string IsLocalIDE { get; set; }
    public static int XrServiceAttemptsNo { get; set; }
    public static string YM_UserName { get; set; }
    public static string YM_Password { get; set; }
    public static string StaticSource { get; set; }
    public static string StaticCloudFrontURL { get; set; }
    public static string StaticAmazonZeeURL { get; set; }
    public static int RefreshTokenInterval { get; set; }
    public static bool EnableHealthTrackers { get; set; }
    public static bool DevConSync { get; set; }
    public static string DevConXrfxUrl { get; set; }
    public static string DevConApiPrefix { get; set; }
    public static int DevConUpdateMinutes { get; set; }
}