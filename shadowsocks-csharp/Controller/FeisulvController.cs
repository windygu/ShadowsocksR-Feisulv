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
    public class FeisulvController
    {
        ShadowsocksController shadowsocksController;

        List<FeisulvHost> feisulvHosts;
        List<FeisulvProduct> products;
        public class FeisulvNodeUpdateFinishEventArgs : EventArgs
        {
            public List<Server> RemovedServers;
            public List<Server> AddedServers;
            public string message;
        }
        public event EventHandler<FeisulvNodeUpdateFinishEventArgs> FeisulvNodeUpdateFinish;
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

        }


        /// <summary>
        /// 向服务器发送http请求，获取服务器信息
        /// </summary>
        /// <returns></returns>
        public void GetFeiSulvHosts()
        {
            feisulvHosts = new List<FeisulvHost>();
            string content = GetServerContentFromFeisulv();

            string[] contents = content.Split('\n');
            foreach (var rec in contents)
            {
                string[] recs = rec.Split('|');
                FeisulvHost temp = new FeisulvHost(recs[1].Replace("\n","").Replace("\r",""), recs[2].Replace("\n", "").Replace("\r", ""));
                feisulvHosts.Add(temp);
            }
        }


        public string GetServerContentFromFeisulv()
        {
            string content;

            try
            {
                content = Utils.GetHttpContentFromUrl("http://www.feisulv.com/test.php");

            }
            catch (Exception)
            {

                throw;
            }
            content = content.Replace(" ", "");//去空格
            return content;

        }

        public List<Server> GetServerInstance(List<FeisulvProduct> products, List<FeisulvHost> hosts)
        {
            List<Server> servers = new List<Server>();
            foreach (FeisulvProduct product in products)
            {
                foreach (FeisulvHost host in hosts)
                {
                    Server server = product.CloneFromProduct();
                    server.server = host.Domin;
                    server.remarks = host.name;
                    server.group = "飞速率-" + product.Port;
                    servers.Add(server);

                }
            }

            return servers;
        }
        public List<FeisulvProduct> DereplicationProducts()
        {
            List<FeisulvProduct> productsnew = new List<FeisulvProduct>();
            foreach (var item in this.products)
            {
                bool replication = false;
                foreach (var newProductItem in productsnew)
                {
                    if (newProductItem.Port == item.Port)
                    {
                        replication = true;
                        break;
                    }
                }
                if (!replication)
                {
                    productsnew.Add(item);
                }
            }
            return productsnew;
        }
        /// <summary>
        /// 从飞速率服务器获取节点更新数据
        /// </summary>
        /// <returns></returns>
        public void FeisulvNodeUpdate()
        {
            products = GetExistProductsFromConfig();
            products = DereplicationProducts();
            GetFeiSulvHosts();
            //  this.ClearFeisulvServers();
            //      ClearFeisulvServers();

            List<Server> feisulvNewServers = GetServerInstance(this.products, this.feisulvHosts);
            List<Server> feisulvOldServers = GetFeisulvOldServers();
            List<Server> RemovedServers = new List<Server>();
            List<Server> AddServers = new List<Server>();
            foreach (Server item in feisulvOldServers)
            {
                if (!feisulvNewServers.Contains(item))
                {
                    RemovedServers.Add(item);
                    _config.servers.Remove(item);

                }
            }

            foreach (Server item in feisulvNewServers)
            {
                if (!feisulvOldServers.Contains(item))
                {
                    AddServers.Add(item);
                    _config.servers.Add(item);

                }
            }
            FeisulvNodeUpdateFinishEventArgs args = new FeisulvNodeUpdateFinishEventArgs();

            args.RemovedServers = RemovedServers;
            args.AddedServers = AddServers;


            //_config.servers.AddRange(   ???  );
            Controllers.shadowsocksController.SaveServersConfig(_config);
            FeisulvNodeUpdateFinish?.Invoke(this, args);
        }

        private List<Server> GetFeisulvOldServers()
        {
            List<Server> feisulvServers = new List<Server>();
            foreach (Server server in _config.servers)
            {
                if (server.group.IndexOf("飞速率") >= 0)
                {
                    feisulvServers.Add(server);
                }
            }
            return feisulvServers;
        }

        //private void ClearFeisulvServers()
        //{
        //    Configuration configcopy = new Configuration();
        //    configcopy.CopyFrom(_config);
        //    Predicate<Server> finder = (Server s) =>
        //    {
        //        if (s.group.IndexOf("飞速率") >= 0)
        //        {
        //            return true;
        //        }
        //        return false;
        //    };

        //    configcopy.servers.RemoveAll(finder);
        //}


        public List<FeisulvProduct> GetExistProductsFromConfig()
        {
            if (this.products == null)
            {
                products = new List<FeisulvProduct>();
            }
            FeisulvProduct product;
            List<int> ports = new List<int>();
            foreach (Server server in _config.servers)
            {
                if (server.group.IndexOf("飞速率") >= 0)
                {
                    int port = server.server_port;

                    if (!ports.Contains(port))
                    {
                        ports.Add(port);
                        product = new FeisulvProduct();
                        product.Port = port;
                        product.serverModel = server.Clone();
                        product.serverModel.server = "88.88.88.88";
                        products.Add(product);
                    }
                }
            }
            return products;
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


        internal bool AddProductFromServer(Server server)
        {
            //清除重复
            FeisulvProduct product = new FeisulvProduct();
            product.Name = "";
            product.Port = server.server_port;
            product.serverModel = server;
            product.serverModel.server = "1234354";
            return AddFeisulvProduct(product);
        }

        private bool AddFeisulvProduct(FeisulvProduct product)
        {
            products = GetExistProductsFromConfig();
            foreach (FeisulvProduct item in products)
            {
                if (product.Port == item.Port && item.serverModel.isMatchServer(product.serverModel))//product.serverModel.password==item.serverModel.password)
                {
                    return false;
                }
            }
            this.products.Add(product);
            return true;
        }
    }

    public class FeisulvProduct
    {
        public string Name;
        public int Port;
        public Server serverModel;
        public Server CloneFromProduct()
        {
            return serverModel.Clone();
        }

    }
    public class FeisulvHost
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
