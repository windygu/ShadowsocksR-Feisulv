using Shadowsocks.Model;
using Shadowsocks.Util;
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
        Configuration _config;
        List<FeisulvServer> feisulvServers;
        Server serverModel;

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
            this._config = shadowsocksController.GetCurrentConfiguration();

        }


        /// <summary>
        /// 向服务器发送http请求，获取服务器信息
        /// </summary>
        /// <returns></returns>
        public void GetServerListFormFeisulv()
        {
            string content = Utils.GetHttpContentFromUrl("http://www.feisulv.com/test.php");
            content = content.Replace(" ", "");//去空格
            string[] contents = content.Split('\n');
            foreach (var rec in contents)
            {
                string[] recs = rec.Split('|');
                FeisulvServer temp = new FeisulvServer(recs[1], recs[2]);
                feisulvServers.Add(temp);
            }
        }
        /// <summary>
        /// 从飞速率服务器获取节点更新数据
        /// </summary>
        /// <returns></returns>
        public void GetNodeUpdate(Configuration _config)
        {
            List<Server> models = GetExistPort();
            string ServerString = GetServerListFormFeisulv();//通过http请求获取服务器节点信息
            foreach (Server model in models)
            {
                List<Server> servers = GetServerFrom_feisulv(model);//将服务器信息转化为Server实例
                _config.configs.AddRange(servers);
            }
        }

        public void FeisulvNodeUpdate()
        {

        }


        public List<Server> GetExistPort()
        {
            List<Server> newservers = new List<Server>();
            List<int> ports = new List<int>();
            List<Server> servermodels = new List<Server>();
            foreach (Server server in _config.configs)
            {
                if (server.group.IndexOf("飞速率") >= 0)
                {
                    int port = server.server_port;
                    if (!ports.Contains(port))
                    {
                        ports.Add(port);
                        servermodels.Add(server);
                    }
                }
                else
                {
                    newservers.Add(server);//保留非飞速率的节点
                }
            }
            //_config.configs = newservers;//删除所有飞速率节点

            return servermodels;
        }

        /// <summary>
        /// 扫描新速率官网的二维码获取服务器信息
        /// </summary>
        /// <param name="ssURL">扫描的二维码信息</param>
        /// <param name="force_group"></param>
        /// <param name="toLast"></param>
        /// <returns></returns>
        public bool GetFeisulvServerList(Server server)
        {
            try
            {

                _config.configs.Add(server);
                string recString = GetServerListFormFeisulv();//通过http请求获取服务器信息
                List<Server> servers = GetServerFrom_feisulv(recString, server);//将服务器信息转化为Server实例
                foreach (Server tmp in servers)
                {
                    if (!_config.ServerIsExist(tmp))
                    {
                        _config.configs.Add(tmp);
                    }
                }
                shadowsocksController.SaveConfig(_config);
                return true;
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                return false;
            }

        }

        public Server GetFeisulvServerModelFromConfig()
        {
            foreach (Server aserver in _config.configs)
            {
                if (aserver.group.StartsWith("飞速率"))
                {
                    this.serverModel = aserver.Clone() ;
                    
                }
            }
        }
    }

        class Feisulv_Product
        {
            string productName;
            string
        }
        class FeisulvServer
        {
            string name;
            string Domin;
            public FeisulvServer(string Domin, string name)
            {
                this.name = name;
                this.Domin = Domin;

            }
        }
    }
