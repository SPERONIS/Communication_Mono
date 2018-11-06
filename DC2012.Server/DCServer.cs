using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Nini.Config;

namespace DC
{
    public class DCServer
    {
        private static IConfigSource cs;

        private static Dictionary<string, DCServer> servers = new Dictionary<string, DCServer>();

        static DCServer()
        {
            if (cs == null)
            {
                cs = new IniConfigSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Server.ini"));

                //FileMonitor fm = new FileMonitor(AppDomain.CurrentDomain.BaseDirectory, "Server.ini");
                //fm.Changed += new FileChanged(file_Changed);
                //fm.Start();
                //WatcherTimer watcherTimer = new WatcherTimer(watcher_Changed, 1000);
                //FileSystemWatcher watcher = new FileSystemWatcher(AppDomain.CurrentDomain.BaseDirectory, "Server.ini");
                //watcher.NotifyFilter = NotifyFilters.LastWrite;
                //watcher.Changed += new FileSystemEventHandler(watcherTimer.OnFileChanged);
                //watcher.EnableRaisingEvents = true;
            }
        }

        static void file_Changed(string path)
        {
            DCLogger.LogInfo(string.Format("Config file changed..[{0}]", path));
            cs.Reload();
            foreach (IConfig cfg in cs.Configs) //(string key in servers.Keys)
            {
                string configName = cfg.Name;
                if (servers.ContainsKey(configName))
                {
                    ServerCfg sc = getServerCfg(configName);
                    if (!sc.Equals(servers[configName].Cfg))
                    {
                        DCServer oldSrv = servers[configName];

                        DCServer server = new DCServer_Star(configName);
                        servers[configName] = server;

                        oldSrv.StopWork();
                        oldSrv = null;

                        servers[configName].StartWork();
                    }
                }
            }
        }

        public static DCServer Instance(string configName)
        {
            if (!servers.ContainsKey(configName))
            {
                lock (servers)
                {
                    if (!servers.ContainsKey(configName))
                    {
                        ServerCfg cc = getServerCfg(configName);
                        servers.Add(configName, new DCServer_Star(configName));
                    }
                }
            }
            return servers[configName];
        }

        public static DCServer Instance(ServerCfg cfg)
        {
            string configName = cfg.ToString();
            if (!servers.ContainsKey(configName))
            {
                lock (servers)
                {
                    if (!servers.ContainsKey(configName))
                    {
                        servers.Add(configName, new DCServer_Star(cfg, configName));
                    }
                }
            }
            return servers[configName];
        }

        internal static ServerCfg getServerCfg(string configName)
        {
            ServerCfg cc = new ServerCfg();
            IConfig cfg = cs.Configs[configName];
            cc.ServerR = cfg.Get("ServerR");
            cc.ServerS = cfg.Get("ServerS");
            cc.BindingType = cfg.Get("BindingType");
            //cc.MngAddr = cfg.Get("MngAddr");
            //cc.PubAddr = cfg.Get("PubAddr");
            //cc.SubAddr = cfg.Get("SubAddr");
            //cc.ReqAddr = cfg.Get("ReqAddr");
            //cc.ActAddr = cfg.Get("ActAddr");
            //cc.ActRply = cfg.Get("ActRply");
            //cc.SrvAddr = cfg.Get("SrvAddr");
            //cc.SrvRply = cfg.Get("SrvRply");
            //cc.SndAddr = cfg.Get("SndAddr");
            //cc.RcvAddr = cfg.Get("RcvAddr");
            return cc;
        }

        internal virtual ServerCfg Cfg
        {
            get { return null; }
        }

        public virtual void StartWork()
        {
        }

        public virtual void StopWork()
        {
        }
    }
}
