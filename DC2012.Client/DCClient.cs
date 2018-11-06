using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Nini.Config;

namespace DC
{
    public class DCClient
    {
        #region instance

        private static IConfigSource cs;

        private static Dictionary<string, DCClient> clients = new Dictionary<string, DCClient>();
        private static object clientsLocker = new object();
        //protected static Socket innerCtrl;

        static DCClient()
        {
            //if (innerCtrl == null)
            //{
            //    innerCtrl = Socket.Create(SocketType.Pub);//Global.Ctx.CreateSocket(ZmqSocketType.Pub);
            //    innerCtrl.Bind(Global.INNER_CTRL_ADDR);
            //}
            if (cs == null)
            {
                cs = new IniConfigSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Client.ini"));

                FileMonitor fm = new FileMonitor(AppDomain.CurrentDomain.BaseDirectory, "Client.ini");
                fm.Changed += new FileChanged(file_Changed);
                fm.Start();
            }
        }

        static void file_Changed(string path)
        {
            DCLogger.LogInfo(string.Format("Config file changed..[{0}]", path));
            cs.Reload();
            foreach (string key in clients.Keys)
            {
                string clientId = key.Split(':')[0];
                string configName = key.Split(':')[1];
                ClientCfg cc = getClientCfg(configName);
                if (!cc.Equals(clients[key].Cfg))
                {
                    DCClient oldClt = clients[key];

                    DCClient client = new DCClient_Star(cc, clientId);
                    clients[key] = client;

                    //innerCtrl.Send(oldClt.guid, Global.DC_CTRL_EXIT);
                    //System.Threading.Thread.Sleep(100);
                    oldClt.Dispose();
                    oldClt = null;
                }
            }
        }

        public static DCClient Instance()
        {
            return Instance("default");
        }

        public static DCClient Instance(string configName)
        {
            string key = configName;
            if (!clients.ContainsKey(key))
            {
                lock (clientsLocker)
                {
                    if (!clients.ContainsKey(key))
                    {
                        ClientCfg cc = getClientCfg(configName);
                        DCClient client = new DCClient_Star(cc, cc.ClientID);
                        client.key = key;
                        clients.Add(key, client);
                    }
                }
            }
            return clients[key];  
        }

        public static DCClient Instance(string clientId, string configName)
        {
            string key = string.Concat(clientId, ":", configName);
            if (!clients.ContainsKey(key))
            {
                lock (clientsLocker)
                {
                    if (!clients.ContainsKey(key))
                    {
                        ClientCfg cc = getClientCfg(configName);
                        DCClient client = new DCClient_Star(cc, clientId);
                        client.key = key;
                        clients.Add(key, client);
                    }
                }
            }
            return clients[key];
        }

        public static DCClient Instance(string clientId, ClientCfg clientCfg)
        {
            string key = string.Concat("[", clientId, "][", clientCfg.ToString(), "]");
            if (!clients.ContainsKey(key))
            {
                lock (clientsLocker)
                {
                    if (!clients.ContainsKey(key))
                    {
                        DCClient client = new DCClient_Star(clientCfg, clientId);
                        client.key = key;
                        clients.Add(key, client);
                    }
                }
            }
            return clients[key];
        }

        private static ClientCfg getClientCfg(string configName)
        {
            ClientCfg cc = new ClientCfg();
            IConfig cfg = cs.Configs[configName];
            cc.ServerR = cfg.Get("ServerR");
            cc.ServerS = cfg.Get("ServerS");
            cc.ClientID = cfg.Get("ClientID");
            cc.BindingType = cfg.Get("BindingType");
            return cc;
        }

        public static void Dispose(DCClient client)
        {
            if (client != null)
            {
                //innerCtrl.Send(client.guid, Global.DC_CTRL_EXIT);
                //System.Threading.Thread.Sleep(100);
                clients.Remove(client.key);
                client.Dispose();
                client = null;
            }
        }

        protected internal string id;

        protected internal string key;

        protected internal byte[] guid;

        protected internal string guidStr;

        #endregion

        #region interface

        public enum HandlerThreadMode
        {
            SingleThread,//单线程，本实例的所有接收数据包被排队处理
            OneThreadPerType,//本实例的每个type的接收数据包被排队处理
            OneThreadPerData,//本实例对每个接收数据包都在新线程处理
        }

        public virtual bool Disposed
        {
            get
            { return false; }
        }

        /// <summary>
        /// 将数据块以指定类型发出。
        /// </summary>
        /// <param name="type">指定的数据类型</param>
        /// <param name="data">待发送的数据块</param>
        public virtual void Pub(string type, byte[] data)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 订阅指定类型数据，以指定的委托处理到达的数据块。可指定多个处理委托。
        /// </summary>
        /// <param name="type">指定订阅的数据类型</param>
        /// <param name="handler">指定数据块处理委托</param>
        public virtual void Sub(string type, Action<byte[]> handler)
        {
            throw new NotSupportedException();
        }
        public virtual void Sub(string type, Action<byte[]> handler, HandlerThreadMode htm)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 订阅指定发送方发出的指定类型数据，以指定的委托处理到达的数据块。可指定多个处理委托。
        /// </summary>
        /// <param name="type">指定订阅的数据类型</param>
        /// <param name="publisher">指定数据发送方</param>
        /// <param name="handler">指定数据块处理委托</param>
        public virtual void Sub(string type, string publisher, Action<byte[]> handler)
        {
            throw new NotSupportedException();
        }
        public virtual void Sub(string type, string publisher, Action<byte[]> handler, HandlerThreadMode htm)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 取消数据订阅。
        /// </summary>
        /// <param name="type">取消订阅的数据类型</param>
        /// <param name="handler">取消的数据块处理委托</param>
        public virtual void UnSub(string type, Action<byte[]> handler)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 取消数据订阅。
        /// </summary>
        /// <param name="type">取消订阅的数据类型</param>
        /// <param name="publisher">取消订阅的数据发送方</param>
        /// <param name="handler">取消的数据块处理委托</param>
        public virtual void UnSub(string type, string publisher, Action<byte[]> handler)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 请求指定数据类型的最新快照。
        /// </summary>
        /// <param name="type">指定请求的数据类型</param>
        /// <returns>数据最新快照，无数据时返回"DC_NULL_SNAP"字符串的Unicode编码</returns>
        public virtual byte[] Req(string type)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 请求指定数据类型的最新快照。
        /// </summary>
        /// <param name="type">指定请求的数据类型</param>
        /// <param name="publisher">指定请求的数据发送方</param>
        /// <returns>数据最新快照，无数据时返回"DC_NULL_SNAP"字符串的Unicode编码</returns>
        public virtual byte[] Req(string type, string publisher)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 向指定目的方发出指定类型的数据块处理请求。默认超时时长。
        /// </summary>
        /// <param name="type">指定数据块处理请求类型</param>
        /// <param name="dest">指定目的方</param>
        /// <param name="data">待处理的数据块</param>
        /// <returns>数据块处理结果</returns>
        /// <exception cref="System.Exception">1-执行超时无结果返回，可能是网络不通或无注册提供该服务的远程实例；2-远程实例执行请求发生的异常</exception>
        public virtual ActResult Act(string type, string dest, byte[] data)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 向指定目的方发出指定类型的数据块处理请求。
        /// </summary>
        /// <param name="type">指定数据块处理请求类型</param>
        /// <param name="dest">指定目的方</param>
        /// <param name="data">待处理的数据块</param>
        /// <param name="timeoutSeconds">超时时间(秒)</param>
        /// <returns>数据块处理结果</returns>
        /// <exception cref="System.Exception">1-执行超时无结果返回，可能是网络不通或无注册提供该服务的远程实例；2-远程实例执行请求发生的异常</exception>
        public virtual ActResult Act(string type, string dest, byte[] data, int timeoutSeconds)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 向指定目的方发出指定类型的数据块处理请求。
        /// </summary>
        /// <param name="type">指定数据块处理请求类型</param>
        /// <param name="dest">指定目的方</param>
        /// <param name="data">待处理的数据块</param>
        /// <param name="handler">接收数据块处理结果的委托，若处理超时或未连接到目的方，则委托可能永远不会被调用</param>
        /// <returns>操作流水号，DCClient实例级别共用。每次Act调用自增1，该值会在handler委托的ActResult实体中带回</returns>
        /// <exception cref="System.Exception">远程实例执行请求发生的异常</exception>
        public virtual int Act(string type, string dest, byte[] data, Action<ActResult> handler)
        {
            throw new NotSupportedException();
        }
        public virtual int Act(string type, string dest, byte[] data, Action<ActResult> handler, HandlerThreadMode htm)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 向不确定目的方发出指定类型的数据块处理请求。由系统自动负载均衡。
        /// </summary>
        /// <param name="type">指定数据块处理请求类型</param>
        /// <param name="data">待处理的数据块</param>
        /// <param name="handler">接收数据块处理结果的委托，若处理超时或未连接到目的方，则委托可能永远不会被调用</param>
        /// <returns>操作流水号，DCClient实例级别共用。每次Act调用自增1，该值会在handler委托的ActResult实体中带回</returns>
        /// <exception cref="System.Exception">远程实例执行请求发生的异常</exception>
        public virtual int Act(string type, byte[] data, Action<ActResult> handler)
        {
            throw new NotSupportedException();
        }
        public virtual int Act(string type, byte[] data, Action<ActResult> handler, HandlerThreadMode htm)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 向不确定目的方发出指定类型的数据块处理请求。
        /// </summary>
        /// <param name="type">指定数据块处理请求类型</param>
        /// <param name="data">待处理的数据块</param>
        /// <param name="timeoutSeconds">超时时间(秒)</param>
        /// <returns>数据块处理结果</returns>
        /// <exception cref="System.Exception">1-执行超时无结果返回，可能是网络不通或无注册提供该服务的远程实例；2-远程实例执行请求发生的异常</exception>
        public virtual ActResult Act(string type, byte[] data, int timeoutSeconds)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 向不确定目的方发出指定类型的数据块处理请求。默认超时时长。
        /// </summary>
        /// <param name="type">指定数据块处理请求类型</param>
        /// <param name="dest">指定目的方</param>
        /// <param name="data">待处理的数据块</param>
        /// <returns>数据块处理结果</returns>
        /// <exception cref="System.Exception">1-执行超时无结果返回，可能是网络不通或无注册提供该服务的远程实例；2-远程实例执行请求发生的异常</exception>
        public virtual ActResult Act(string type, byte[] data)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 注册处理指定类型的数据块。每个DCClient实例每个类型只能注册一个处理委托。
        /// </summary>
        /// <param name="type">注册处理的数据块类型</param>
        /// <param name="handler">指定到达的数据块的处理委托</param>
        public virtual void Srv(string type, Func<string, byte[], byte[]> handler)
        {
            throw new NotSupportedException();
        }
        public virtual void Srv(string type, Func<string, byte[], byte[]> handler, HandlerThreadMode htm)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 取消注册处理指定类型的数据块。每个DCClient实例每个类型只能注册一个处理委托。
        /// </summary>
        /// <param name="type">取消注册处理的数据块类型</param>
        /// <param name="handler">数据块的处理委托</param>
        public virtual void UnSrv(string type, Func<string, byte[], byte[]> handler)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 向指定目的方发出指定类型的数据块。
        /// </summary>
        /// <param name="type">指定数据块处理请求类型</param>
        /// <param name="dest">指定目的方</param>
        /// <param name="data">待发送的数据块</param>
        public virtual void Snd(string type, string dest, byte[] data)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 向不确定目的方发出指定类型的数据块。
        /// </summary>
        /// <param name="type">指定数据块处理请求类型</param>
        /// <param name="data">待发送的数据块</param>
        public virtual void Snd(string type, byte[] data)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 注册接收指定类型的数据块。
        /// </summary>
        /// <param name="type">注册处理的数据块类型</param>
        /// <param name="handler">指定到达的数据块的处理委托</param>
        public virtual void Rcv(string type, Action<byte[]> handler)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 取消注册接收指定类型的数据块。
        /// </summary>
        /// <param name="type">取消注册处理的数据块类型</param>
        /// <param name="handler">数据块的处理委托</param>
        public virtual void UnRcv(string type, Action<byte[]> handler)
        {
            throw new NotSupportedException();
        }

        #endregion

        //#region interface couple

        //public virtual DCCouple RegisterCouple(string dest)
        //{
        //    throw new NotSupportedException();
        //}

        //#endregion

        internal virtual ClientCfg Cfg
        {
            get { return null; }
        }

        protected virtual void Dispose()
        {

        }
    }
}
