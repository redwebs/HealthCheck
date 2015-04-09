using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;

namespace WebForm.CsLib
{
    public static class Helper
    {
//    private static readonly ILog Log = LogManager.GetLogger("Healthchk");

        public static T QueryStringObject<T>(string qString)
        {
            // Convert a query string into a class of type T

            if (String.IsNullOrEmpty(qString))
            {
                return JsonConvert.DeserializeObject<T>("{}");
                ;
            }
            // These next two lines convert a query string into a JSON string
            var jsonString = qString.Replace("&", "\",").Replace("=", ":\"");
            jsonString = "{" + jsonString + "\"}";
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public static string GetConnString(string key)
        {
            if (ConfigurationManager.ConnectionStrings[key] == null)
            {
                //           Log.DebugFormat("Attempt to read non-existent Connection String with key = {0}", key);
                return string.Empty;
            }
            return ConfigurationManager.ConnectionStrings[key].ToString();
        }


        public static string GetAppSetting(string key)
        {
            if (ConfigurationManager.AppSettings[key] == null)
            {
                //Log.ErrorFormat("Attempt to read non-existent AppSetting String with key = {0}", key);
                return string.Empty;
            }
            return ConfigurationManager.AppSettings[key];
        }

        public static string GetAppSetting(string key, string defString)
        {
            if (ConfigurationManager.AppSettings[key] == null)
            {
                //Log.DebugFormat("Attempt to read non-existent AppSetting String with key = {0}, default sent", key);
                return defString;
            }
            return ConfigurationManager.AppSettings[key];
        }

        public static bool GetAppSettingBool(string key, bool defBool)
        {
            if (ConfigurationManager.AppSettings[key] != null)
            {
                bool convBool = false;
                var valString = ConfigurationManager.AppSettings[key];
                if (bool.TryParse(valString, out convBool))
                {
                    return convBool;
                }
            }
            //Log.DebugFormat("Attempt to read non-existent AppSetting Bool with key = {0}, default sent", key);
            return defBool;
        }

        public static int GetAppSettingInt(string key, int defNumber)
        {
            if (ConfigurationManager.AppSettings[key] != null)
            {
                int convNumber = 0;
                var valString = ConfigurationManager.AppSettings[key];
                if (int.TryParse(valString, out convNumber))
                {
                    return convNumber;
                }
            }
            //Log.DebugFormat("Attempt to read non-existent AppSetting Number with key = {0}, default sent", key);
            return defNumber;
        }

        public static string GetSessionString(string key, string defString)
        {
            if (HttpContext.Current.Session[key] == null)
            {
                //Log.ErrorFormat("Attempt to read non-existent Session String with key = {0}, default sent", key);
                return defString;
            }
            return HttpContext.Current.Session[key].ToString();
        }

        public static bool GetSessionBool(string key, bool defVal)
        {
            if (HttpContext.Current.Session[key] == null)
            {
                //Log.ErrorFormat("Attempt to read non-existent Session Bool with key = {0}, default sent", key);
                return defVal;
            }
            return (bool) HttpContext.Current.Session[key];
        }

        public static string GetCurrentSessionId()
        {
            if (HttpContext.Current == null)
            {
                return "*none*";
            }
            var ss = HttpContext.Current.Session;

            if (ss == null)
            {
                return "*none*";
            }
            return ss.SessionID;
        }

        public static string DateTimeStrSortable(string prefix)
        {
            const string format = "yyyy-MM-dd_HHmm";
            return (prefix + "_" + DateTime.Now.ToString(format));
        }

        public static string DateTimeStrSortable(string prefix, string extension)
        {
            const string format = "yyyy-MM-dd_HHmm";
            return (prefix + "_" + DateTime.Now.ToString(format) + "." + extension);
        }

        /// <summary>
        /// Read a Text file and return it's content in a string
        /// </summary>
        /// <param name="filePath">Full path to the file</param>
        /// <returns>UTF8 encoded contents of the file</returns>
        /// 
        public static string TextFileToString(string filePath)
        {
            var bytes = TextFileToByteArray(filePath);

            if (bytes == null)
            {
                return string.Empty;
            }
            var enc = new UTF8Encoding();
            return enc.GetString(bytes);
        }

        /// <summary>
        /// Read a Text file and return it's content in a byte array
        /// </summary>
        /// <param name="filePath">Full path to the file</param>
        /// <returns>Byte[] contents of the file</returns>
        /// 
        public static byte[] TextFileToByteArray(string filePath)
        {
            byte[] buff = null;

            if (File.Exists(filePath))
            {
                var fs = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite);
                var br = new BinaryReader(fs);
                long numBytes = new FileInfo(filePath).Length;
                buff = br.ReadBytes((int) numBytes);
                fs.Close();
            }
            else
            {
                //Log.ErrorFormat("Helper.TextFileToByteArray could not find file: {0}", filePath);
            }
            return buff;
        }

        /// <summary>
        /// Encrypt a string using Triple DES encryption and additional SHA1 hash. 
        /// </summary>
        /// <param name="clearText">UTF8 Clear text to be encrypted</param>
        /// <param name="key">Encryption key used by both methods</param>
        /// <returns>Encrypted base 64 string</returns>
        /// 
        public static string EncryptTextDual(string clearText, string key)
        {
            byte[] key24Len = new byte[24];

            var toEncryptArray = Encoding.UTF8.GetBytes(clearText);

            var sha1Provider = new SHA512CryptoServiceProvider();
            var keyArray = sha1Provider.ComputeHash(Encoding.UTF8.GetBytes(key));
            sha1Provider.Clear();

            for (int idx = 0; idx < 24; idx++)
            {
                key24Len[idx] = keyArray[idx];
            }

            var tripleDesProvider = new TripleDESCryptoServiceProvider {};
            tripleDesProvider.Key = key24Len;
            tripleDesProvider.Mode = CipherMode.ECB;
            tripleDesProvider.Padding = PaddingMode.PKCS7;

            var iCryptoTransform = tripleDesProvider.CreateEncryptor();
            var resultArray = iCryptoTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tripleDesProvider.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        /// Decrypt a Base 64 string using Triple DES encryption method and SHA1 hash. 
        /// </summary>
        /// <param name="cipherText">Encrypted text</param>
        /// <param name="key">Encryption key used by both methods</param>
        /// <returns>Clear text UTF8 string</returns>
        /// 
        public static Tuple<string, bool> DecryptTextDual(string cipherText, string key)
        {
            try
            {
                var key24Len = new byte[24];
                var toDecryptArray = Convert.FromBase64String(cipherText);

                var sha1Provider = new SHA512CryptoServiceProvider();
                var keyArray = sha1Provider.ComputeHash(Encoding.UTF8.GetBytes(key));
                sha1Provider.Clear();

                for (var idx = 0; idx < 24; idx++)
                {
                    key24Len[idx] = keyArray[idx];
                }

                var tripleDesProvider = new TripleDESCryptoServiceProvider {};
                tripleDesProvider.Key = key24Len;
                tripleDesProvider.Mode = CipherMode.ECB;
                tripleDesProvider.Padding = PaddingMode.PKCS7;

                var iCryptoTransform = tripleDesProvider.CreateDecryptor();
                var resultArray = iCryptoTransform.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);

                tripleDesProvider.Clear();
                return new Tuple<string, bool>(Encoding.UTF8.GetString(resultArray), true);
            }
            catch (Exception ex)
            {
                return new Tuple<string, bool>(ex.Message, false);
            }
        }

        public static string ListDirectoryFiles(string dirPath, string fileExtension)
        {
            var sb = new StringBuilder();

            try
            {
                var files =
                    from file in Directory.EnumerateFiles(dirPath, "*." + fileExtension, SearchOption.TopDirectoryOnly)
                    select new
                    {
                        File = file,
                    };

                //    dirPath += "\\";

                sb.AppendLine(files.Count().ToString());
                foreach (var f in files)
                {
                    sb.AppendLine(f.File.Replace(dirPath, string.Empty));
                }
            }
            catch (UnauthorizedAccessException uAEx)
            {
                sb.Append(uAEx.Message);
            }
            catch (PathTooLongException pathEx)
            {
                sb.Append(pathEx.Message);
            }
            catch (Exception ex)
            {
                sb.Append(ex.Message);
            }
            return sb.ToString();
        }

        public static List<string> ListDirectoryFiles(string dirPath, string fileExtension, out bool success)
        {
            string errMsg;
            success = false;

            try
            {
                var files =
                    from file in Directory.EnumerateFiles(dirPath, "*." + fileExtension, SearchOption.TopDirectoryOnly)
                    select file;

                success = true;
                return files.ToList();
            }
            catch (UnauthorizedAccessException uAEx)
            {
                errMsg = uAEx.Message;
            }
            catch (PathTooLongException pathEx)
            {
                errMsg = pathEx.Message;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }
            return new List<string> {errMsg};
        }

        public static string StripHtml(string source)
        {
            try
            {
                // Remove HTML Development formatting
                // Replace line breaks with space
                // because browsers inserts space
                var result = source.Replace("\r", " ");
                // Replace line breaks with space
                // because browsers inserts space
                result = result.Replace("\n", " ");
                // Remove step-formatting
                result = result.Replace("\t", string.Empty);
                // Remove repeating spaces because browsers ignore them
                result = Regex.Replace(result,
                    @"( )+", " ");

                // Remove the header (prepare first by clearing attributes)
                result = Regex.Replace(result,
                    @"<( )*head([^>])*>", "<head>",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"(<( )*(/)( )*head( )*>)", "</head>",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    "(<head>).*(</head>)", string.Empty,
                    RegexOptions.IgnoreCase);

                // remove all scripts (prepare first by clearing attributes)
                result = Regex.Replace(result,
                    @"<( )*script([^>])*>", "<script>",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"(<( )*(/)( )*script( )*>)", "</script>",
                    RegexOptions.IgnoreCase);
                //result = Regex.Replace(result,
                //         @"(<script>)([^(<script>\.</script>)])*(</script>)",
                //         string.Empty,
                //         RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"(<script>).*(</script>)", string.Empty,
                    RegexOptions.IgnoreCase);

                // remove all styles (prepare first by clearing attributes)
                result = Regex.Replace(result,
                    @"<( )*style([^>])*>", "<style>",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"(<( )*(/)( )*style( )*>)", "</style>",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    "(<style>).*(</style>)", string.Empty,
                    RegexOptions.IgnoreCase);

                // insert tabs in spaces of <td> tags
                result = Regex.Replace(result,
                    @"<( )*td([^>])*>", "\t",
                    RegexOptions.IgnoreCase);

                // insert line breaks in places of <BR> and <LI> tags
                result = Regex.Replace(result,
                    @"<( )*br( )*>", "\r",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"<( )*li( )*>", "\r",
                    RegexOptions.IgnoreCase);

                // insert line paragraphs (double line breaks) in place
                // if <P>, <DIV> and <TR> tags
                result = Regex.Replace(result,
                    @"<( )*div([^>])*>", "\r\r",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"<( )*tr([^>])*>", "\r\r",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"<( )*p([^>])*>", "\r\r",
                    RegexOptions.IgnoreCase);

                // Remove remaining tags like <a>, links, images,
                // comments etc - anything that's enclosed inside < >
                result = Regex.Replace(result,
                    @"<[^>]*>", string.Empty,
                    RegexOptions.IgnoreCase);

                // replace special characters:
                result = Regex.Replace(result,
                    @" ", " ",
                    RegexOptions.IgnoreCase);

                result = Regex.Replace(result,
                    @"&bull;", " * ",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"&lsaquo;", "<",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"&rsaquo;", ">",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"&trade;", "(tm)",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"&frasl;", "/",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"&lt;", "<",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"&gt;", ">",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"&copy;", "(c)",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    @"&reg;", "(r)",
                    RegexOptions.IgnoreCase);
                // Remove all others. More can be added, see
                // http://hotwired.lycos.com/webmonkey/reference/special_characters/
                result = Regex.Replace(result,
                    @"&(.{2,6});", string.Empty,
                    RegexOptions.IgnoreCase);

                // for testing
                //Regex.Replace(result,
                //       this.txtRegex.Text,string.Empty,
                //       RegexOptions.IgnoreCase);

                // make line breaking consistent
                result = result.Replace("\n", "\r");

                // Remove extra line breaks and tabs:
                // replace over 2 breaks with 2 and over 4 tabs with 4.
                // Prepare first to remove any whitespaces in between
                // the escaped characters and remove redundant tabs in between line breaks
                result = Regex.Replace(result,
                    "(\r)( )+(\r)", "\r\r",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    "(\t)( )+(\t)", "\t\t",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    "(\t)( )+(\r)", "\t\r",
                    RegexOptions.IgnoreCase);
                result = Regex.Replace(result,
                    "(\r)( )+(\t)", "\r\t",
                    RegexOptions.IgnoreCase);
                // Remove redundant tabs
                result = Regex.Replace(result,
                    "(\r)(\t)+(\r)", "\r\r",
                    RegexOptions.IgnoreCase);
                // Remove multiple tabs following a line break with just one tab
                result = Regex.Replace(result,
                    "(\r)(\t)+", "\r\t",
                    RegexOptions.IgnoreCase);
                // Initial replacement target string for line breaks
                var breaks = "\r\r\r";
                // Initial replacement target string for tabs
                var tabs = "\t\t\t\t\t";
                for (var index = 0; index < result.Length; index++)
                {
                    result = result.Replace(breaks, "\r\r");
                    result = result.Replace(tabs, "\t\t\t\t");
                    breaks = breaks + "\r";
                    tabs = tabs + "\t";
                }

                // That's it.
                return result;
            }
            catch
            {
                return source;
            }
        }

        public static bool IsMobileBrowser(HttpRequest request)
        {
            var u = request.ServerVariables["HTTP_USER_AGENT"];
            var b =
                new Regex(
                    @"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino",
                    RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var v =
                new Regex(
                    @"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-",
                    RegexOptions.IgnoreCase | RegexOptions.Multiline);

            return b.IsMatch(u) || v.IsMatch(u.Substring(0, 4));
        }

        public static bool IsMobileOrTabletBrowser(HttpRequest request)
        {
            var u = request.ServerVariables["HTTP_USER_AGENT"];
            var b =
                new Regex(
                    @"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino|android|ipad|playbook|silk",
                    RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var v =
                new Regex(
                    @"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-",
                    RegexOptions.IgnoreCase | RegexOptions.Multiline);

            return b.IsMatch(u) || v.IsMatch(u.Substring(0, 4));
        }

        public static List<Tuple<string, string>> GetHtmlListItems(string html, char splitter)
        {
            var tupleList = new List<Tuple<string, string>> {};
            var indexOfStart = 0;

            var indexOfLi = html.IndexOf("<li>", indexOfStart, StringComparison.Ordinal);

            while (indexOfLi > 0)
            {
                var indexOfLiEnd = html.IndexOf("</li>", indexOfLi + 4, StringComparison.Ordinal);

                var listItem = html.Substring(indexOfLi + 4, indexOfLiEnd - indexOfLi - 4);

                if (listItem.Length > 0)
                {
                    var itemArray = listItem.Split(splitter);

                    if (itemArray.Length > 1)
                    {
                        var atuple = new Tuple<string, string>(itemArray[0].Trim(), itemArray[1].Trim());
                        tupleList.Add(atuple);
                    }
                    else
                    {
                        var atuple = new Tuple<string, string>(itemArray[0].Trim(), string.Empty);
                        tupleList.Add(atuple);
                    }
                }
                indexOfStart = indexOfLiEnd + 1;
                indexOfLi = html.IndexOf("<li>", indexOfStart, StringComparison.Ordinal);
            }
            return tupleList;
        }

        public static Tuple<bool, string> GetHostDnsAddress(string url)
        {
            try
            {
                var addr = Dns.GetHostAddresses(url);

                if (addr.Length > 0)
                {
                    var firstOrDefault = addr.FirstOrDefault();
                    if (firstOrDefault != null)
                    {
                        return new Tuple<bool, string>(true, firstOrDefault.ToString());
                    }
                }
                return new Tuple<bool, string>(false, "No Address Found");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("no data of the requested type was found"))
                {
                    return new Tuple<bool, string>(false, "No Address Found");
                }
                return new Tuple<bool, string>(false, ex.Message);
            }
        }
    }
}