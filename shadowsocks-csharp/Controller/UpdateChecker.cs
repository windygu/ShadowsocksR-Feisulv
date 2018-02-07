using Shadowsocks.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Windows.Forms;

namespace Shadowsocks.Controller
{
    public class UpdateChecker
    {
        //  private const string UpdateURL = "https://raw.githubusercontent.com/breakwa11/breakwa11.github.io/master/update/ssr-win-4.0.xml";
        private const string UpdateURL = "http://update.qwssr.com/ssr/versionList.xml";
        public Version LatestVersion;
        public event EventHandler<NewVersionFoundEventArgs> VersionGetHandler;
        public class NewVersionFoundEventArgs : EventArgs
        {
            public OnNewVersionFondAction action=OnNewVersionFondAction.nothing;
        }
        private Version currentVersion;

        public Version CurrentVersion
        {
            get
            {
                if (currentVersion==null)
                {
                    currentVersion= new Version(currentVersionNum);
                }
                return currentVersion;
            }

            set
            {
                currentVersion = value;
            }
        }
        public const string Name = "ShadowsocksR";
        public const string Copyright = "Copyright © BreakWa11 2017. Fork from Shadowsocks by clowwindy";
        public const string currentVersionNum = "4.7.4";
        public bool forceUpdate = false;


        public enum OnNewVersionFondAction
        {
            shutdown = -1,
            nothing = 0,
            alert = 1,
            slient = 2
        }


#if !_DOTNET_4_0
        public const string NetVer = "2.0";
#elif !_CONSOLE
        public const string NetVer = "4.0";
#else
        public const string NetVer = "";
#endif
        public const string FullVersion = currentVersionNum +
#if DEBUG
        " Debug";
#else
/*
        " Alpha";
/*/
        "";
//*/
#endif

        private static bool UseProxy = true;
        private XmlDocument doc;
        private Version.State preferVersion;


        public void CheckUpdate(Configuration config, bool forceUpdate = false)
        {
            this.forceUpdate = forceUpdate;
            try
            {
                WebClient http = new WebClient();
                http.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.3319.102 Safari/537.36");
                if (UseProxy)
                {
                    WebProxy proxy = new WebProxy(IPAddress.Loopback.ToString(), config.localPort);
                    if (!string.IsNullOrEmpty(config.authPass))
                    {
                        proxy.Credentials = new NetworkCredential(config.authUser, config.authPass);
                    }
                    http.Proxy = proxy;
                }
                else
                {
                    http.Proxy = null;
                }
                //UseProxy = !UseProxy;


                http.DownloadStringCompleted += http_DownloadStringCompleted;
                http.DownloadStringAsync(new Uri(UpdateURL + "?rnd=" + Util.Utils.RandUInt32().ToString()));
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
            }
        }

        public static int CompareVersion(string l, string r)
        {
            var ls = l.Split('.');
            var rs = r.Split('.');
            for (int i = 0; i < Math.Max(ls.Length, rs.Length); i++)
            {
                int lp = (i < ls.Length) ? int.Parse(ls[i]) : 0;
                int rp = (i < rs.Length) ? int.Parse(rs[i]) : 0;
                if (lp != rp)
                {
                    return lp - rp;
                }
            }
            return 0;
        }

        //public class VersionComparer : IComparer<string>
        //{
        //    // Calls CaseInsensitiveComparer.Compare with the parameters reversed. 
        //    public int Compare(string x, string y)
        //    {
        //        return CompareVersion(ParseVersionFromURL(x), ParseVersionFromURL(y));
        //    }

        //}

        //private static string ParseVersionFromURL(string url)
        //{
        //    Match match = Regex.Match(url, @".*" + Name + @"-win.*?-([\d\.]+)\.\w+", RegexOptions.IgnoreCase);
        //    if (match.Success)
        //    {
        //        if (match.Groups.Count == 2)
        //        {
        //            return match.Groups[1].Value;
        //        }
        //    }
        //    return null;
        //}

        //private void SortVersions(List<string> versions)
        //{
        //    versions.Sort(new VersionComparer());
        //}

        //private bool IsNewVersion(string url)
        //{
        //    if (url.IndexOf("prerelease") >= 0)
        //    {
        //        return false;
        //    }
        //    // check dotnet 4.0
        //    AssemblyName[] references = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
        //    System.Version dotNetVersion = Environment.Version;

        //    foreach (AssemblyName reference in references)
        //    {
        //        if (reference.Name == "mscorlib")
        //        {
        //            dotNetVersion = reference.Version;
        //        }
        //    }
        //    if (dotNetVersion.Major >= 4)
        //    {
        //        if (url.IndexOf("dotnet4.0") < 0)
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        if (url.IndexOf("dotnet4.0") >= 0)
        //        {
        //            return false;
        //        }
        //    }
        //    string version = ParseVersionFromURL(url);
        //    if (version == null)
        //    {
        //        return false;
        //    }
        //    string currentVersion = CurrentVersion;

        //    if (url.IndexOf("banned") > 0 && CompareVersion(version, currentVersion) == 0
        //        || url.IndexOf("deprecated") > 0 && CompareVersion(version, currentVersion) > 0)
        //    {
        //        Application.Exit();
        //        return false;
        //    }
        //    return CompareVersion(version, currentVersion) > 0;
        //}
        Dictionary<string, Version> versionDict;


        private void http_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                doc = new XmlDocument();
                doc.LoadXml(e.Result.Replace("\n", ""));
                XmlNodeList elelist = doc.GetElementsByTagName("version");
                versionDict = new Dictionary<string, Version>();
                foreach (System.Xml.XmlNode item in elelist)
                {
                    Version versionItem = new Version();
                    versionItem.versionNum = item["versionNum"].InnerText;
                    versionItem.url = item["url"].InnerText;
                    versionItem.state = (Version.State)Enum.Parse(typeof(Version.State), item["state"].InnerText);
                    versionDict.Add(versionItem.versionNum, versionItem);
                }
                foreach (var version in versionDict)
                {
                    if (version.Value.state == preferVersion)
                    {
                        this.LatestVersion = version.Value;
                        break;
                    }
                }
                NewVersionFoundEventArgs args = new NewVersionFoundEventArgs();

                if (versionDict.ContainsKey(currentVersionNum))
                {
                    switch (versionDict[currentVersionNum].state)
                    {
                        case Version.State.banned:
                            args.action = OnNewVersionFondAction.shutdown;
                            break;
                        case Version.State.deprecated:
                            args.action = OnNewVersionFondAction.slient;
                            break;
                        case Version.State.outdated:
                            args.action = OnNewVersionFondAction.alert;
                            break;
                        case Version.State.stable:
                            break;
                        case Version.State.beta:
                            break;
                        default:
                            break;
                    }
                }
                VersionGetHandler(this, args);
            }
            catch (Exception ex)
            {
                if (e.Error != null)
                {
                    Logging.Debug(e.Error.ToString());
                }
                Logging.Debug(ex.ToString());
                return;
            }
        }

        public class Version
        {
            public Version()
            {

            }
            public Version(string versionNum)
            {
                this.versionNum = versionNum;
            }
            public enum State
            {
                banned = -3,
                outdated = -1,
                deprecated = -2,
                stable = 0,
                beta = 1
            }
            public State state;
            public string versionNum;
            public string url;
            public override string ToString()
            {
                string str = "version:" + versionNum;
                str += "\r\n" + "url:" + url;
                str += "\r\n" + "state:" + state.ToString();
                return str;

            }
        }

    }

}
