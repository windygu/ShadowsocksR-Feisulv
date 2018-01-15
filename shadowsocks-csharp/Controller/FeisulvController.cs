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

        List<FeisulvHost> feisulvHosts;
        List<FeisulvProduct> products;


        string serverStringContent;
        public Configuration _config
        {
            get
            {
                return shadowsocksController.GetCurrentConfiguration();
            }
        }
        public FeisulvController(ShadowsocksController shadowsocksController)
        {
            this.shadowsocksController = shadowsocksController;
            this.products = new List<FeisulvProduct>();


        }


        /// <summary>
        /// 向服务器发送http请求，获取服务器信息
        /// </summary>
        /// <returns></returns>
        public void GetServerListFormFeisulv()
        {
            string content = GetServerContentFromFeisulv();

            string[] contents = content.Split('\n');
            foreach (var rec in contents)
            {
                string[] recs = rec.Split('|');
                FeisulvHost temp = new FeisulvHost(recs[1], recs[2]);
                feisulvHosts.Add(temp);
            }
        }


        public string  GetServerContentFromFeisulv()
        {
            string content;

            try
            {
                 content = Utils.GetHttpContentFromUrl("http://www.feisulv.com/test.php");

            }
            catch (Exception )
            {
                
                throw;
            }
            content = content.Replace(" ", "");//去空格
            return content;

        }

        public List<Server> GetServerInstance(List<FeisulvProduct> products,List<FeisulvHost> hosts)
        {
            List<Server> servers = new List<Server>();
            foreach (FeisulvProduct product in products)
            {
                foreach (FeisulvHost host in hosts)
                {
                    Server server = product.CloneFromProduct();
                    server.server = host.Domin;
                //    server.remarks
                }
            }

            return servers;
        }

        /// <summary>
        /// 从飞速率服务器获取节点更新数据
        /// </summary>
        /// <returns></returns>
        public void GetNodeUpdate(Configuration _config)
        { 
            GetExistProduct();
            this.ClearFeisulvServers();

            List<Server> servers= GetServerInstance(this.products,this.feisulvHosts);
            _config.configs.AddRange(servers);
        }

        private void ClearFeisulvServers()
        {
          
        }

        public void FeisulvNodeUpdate()
        {

        }


        public List<FeisulvProduct> GetExistProduct()
        {
            products = new List<FeisulvProduct>();
            FeisulvProduct product;

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

                        product = new FeisulvProduct();
                        product.serverModel = server;
                        product.serverModel.server = "123456789";
                        products.Add(product);

                    }
                }
                else
                {
                    newservers.Add(server);//保留非飞速率的节点
                }
            }
            //_config.configs = newservers;//删除所有飞速率节点

            return products ;
        }

        /// <summary>
        /// 扫描新速率官网的二维码获取服务器信息
        /// </summary>
        /// <param name="ssURL">扫描的二维码信息</param>
        /// <param name="force_group"></param>
        /// <param name="toLast"></param>
        /// <returns></returns>
        //public bool GetFeisulvServerList(Server server)
        //{
        //    try
        //    {

        //        _config.configs.Add(server);
        //        string recString = GetServerListFormFeisulv();//通过http请求获取服务器信息
        //        List<Server> servers = GetServerFrom_feisulv(recString, server);//将服务器信息转化为Server实例
        //        foreach (Server tmp in servers)
        //        {
        //            if (!_config.ServerIsExist(tmp))
        //            {
        //                _config.configs.Add(tmp);
        //            }
        //        }
        //        shadowsocksController.SaveConfig(_config);
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        Logging.LogUsefulException(e);
        //        return false;
        //    }

        //}

        public void GetFeisulvProductsFromConfig()
        {
            List<int> ports = new List<int>();

            foreach (Server aserver in _config.configs)
            {
                if (aserver.group.StartsWith("飞速率"))
                {
                    if (!ports.Contains(aserver.server_port))
                    {
                        FeisulvProduct aproduct = new FeisulvProduct();
                        aproduct.Name = aserver.remarks;
                        aproduct.Port = aserver.server_port;
                        aproduct.serverModel = aserver.Clone();
                        this.products.Add(aproduct);



                    }
                }
            }
        }

       
    }

    class FeisulvProduct
    {
        public string Name;
        public int Port;
        public Server serverModel;
        public Server CloneFromProduct()
        {
            return serverModel.Clone();
        }

    }
    class FeisulvHost
    {
        public string name;
        public string Domin;
        public FeisulvHost(string Domin, string name)
        {
            this.name = name;
            this.Domin = Domin;
        }
    }
}
