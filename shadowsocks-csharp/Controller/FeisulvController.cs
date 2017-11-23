using Shadowsocks.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Shadowsocks.Controller
{
    class FeisulvController
    {
        ShadowsocksController shadowsocksController;

        string serverStringContent;
        public Configuration Config
        {
            get
            {
                return shadowsocksController._config;
            }
        }
        public FeisulvController(ShadowsocksController shadowsocksController)
        {
            this.shadowsocksController = shadowsocksController;

        }

        string GetRawServerString()
        {

            return "";

        }
        /// <summary>
        /// 向服务器发送http请求，获取服务器信息
        /// </summary>
        /// <returns></returns>
        public string GetServerInfo()
        {
            try
            {
                WinINet.SetIEProxy(false, false, "", "");
                string url = @"http://www.feisulv.com/test.php";
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "GET";
                request.Timeout = 6000;
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/44.0.2403.89 Safari/537.36";
                request.Accept = "text/plain, */*; q=0.01";
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
                string recString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                WinINet.SetIEProxy(true, true, "127.0.0.1:1080", "");
                return recString;
            }
            catch (Exception ex)
            {
                string errormsg = ex.Message;
                throw;
            }
        }

        /// <summary>
        /// 扫描新速率官网的二维码获取服务器信息
        /// </summary>
        /// <param name="ssURL">扫描的二维码信息</param>
        /// <param name="force_group"></param>
        /// <param name="toLast"></param>
        /// <returns></returns>
        public bool GetFeisulvServerList(Server server, out List<Server> servers)
        {
            try
            {

                _config.configs.Add(server);
                string recString = GetServerInfo();//通过http请求获取服务器信息
                servers = GetServerFrom_feisulv(recString, server);//将服务器信息转化为Server实例
                foreach (Server tmp in servers)
                {
                    if (!_config.ServerIsExist(tmp))
                    {
                        _config.configs.Add(tmp);
                    }
                }
                SaveConfig(_config);
                return true;
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                return false;
            }

        }
        /// <summary>
        /// 获取服务器ip
        /// </summary>
        /// <param name="recString">通过http请求的服务器信息</param>
        /// <returns></returns>
        public List<Server> GetServerFrom_feisulv(string recString, Server servermodel)
        {
            List<Server> servers = new List<Server>();
            recString = recString.Replace(" ", "");//去空格
            string[] recStrings = recString.Split('\n');

            foreach (var rec in recStrings)
            {
                string[] recs = rec.Split('|');
                Server temp = servermodel.Clone();
                temp.server = recs[1];
                temp.remarks = recs[2];
                temp.group = "飞速率";
                servers.Add(temp);
            }
            return servers;
        }
    }

    class Feisulv_Product
    {
        string productName;

    }

}
