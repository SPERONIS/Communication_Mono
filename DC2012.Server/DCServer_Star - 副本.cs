using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace DC
{
    public class DCServer_Star : DCServer
    {
        private string configName;
        private bool isStart = false;
        private bool isWork = true;

        private ServerCfg cfg;

        internal override ServerCfg Cfg
        {
            get { return cfg; }
        }

        Thread mngThread = null;

        bool mngThrExited = true;
        bool serverThrExited = true;

        internal DCServer_Star(string configName)
        {
            this.configName = configName;
            cfg = DCServer.getServerCfg(configName);
        }

        internal DCServer_Star(ServerCfg serverCfg, string configName)
        {
            this.configName = configName;
            cfg = serverCfg;
        }

        ~DCServer_Star()
        {
            if (isStart)
            {
                StopWork();
            }
        }

        public override void StartWork()
        {
            if (!isStart)
            {
                isWork = true;
                initServerThr();
                while (serverThrExited)
                {
                    Thread.Sleep(100);
                }

                initInnerGuardThr();

                isStart = true;

                DCLogger.LogInfo("ConsoleServer started..");
            }
            else
            {
                DCLogger.LogInfo("ConsoleServer already started..");
            }
        }

        Thread serverThread;
        Socket receiver;
        private void initServerThr()
        {
            serverThread = new Thread(new ThreadStart(delegate()
            {
                using (receiver = Socket.Create(SocketType.Pull, cfg.BindingType))
                using (Socket publisher = Socket.Create(SocketType.Pub, cfg.BindingType))
                {
                    try
                    {
                        receiver.Bind(cfg.ServerR);
                        //receiver.Bind(Global.INNER_CTRL_SERVERTHR + configName);

                        publisher.Bind(cfg.ServerS);

                        long length = 0;
                        Stopwatch watch = new Stopwatch();
                        watch.Start();

                        byte[] buffer = new byte[8388608];
                        while (isWork)
                        {
                            try
                            {
                                byte[] envelope;// = receiver.Receive();
                                byte[] data;// = receiver.Receive();
                                receiver.Receive(out envelope, out data);

                                if (envelope == null || envelope.Length == 0)
                                {
                                    continue;
                                }
                                if (data == null)
                                {
                                    continue;
                                }

                                //refreshSnap(envelope, data);

                                publisher.Send(envelope, data);
                                length += 1;

                                if (watch.ElapsedMilliseconds > 10000)
                                {
                                    DCLogger.LogInfo(string.Format("Exchange Speed:{0}pkgs/s\t{1}", length * 1000 / (watch.ElapsedMilliseconds), DateTime.Now));
                                    length = 0;
                                    watch.Reset();
                                    watch.Start();
                                }
                            }
                            catch (Exception e)
                            {
                                DCLogger.LogError("{0} get an error.[{1}]", "ServerThr work", e.Message);
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        DCLogger.LogFatal("{0} get a fatal error.[{1}]", "ServerThr init", e.Message);
                    }
                }

                DCLogger.LogInfo("ServerThr exited.");
                serverThrExited = true;
            }
            ));
            serverThread.IsBackground = true;
            serverThread.Start();
            serverThrExited = false;
        }

        private void initInnerGuardThr()
        {
            Thread innerGuardThread = new Thread(new ThreadStart(delegate()
            {
                while (isWork)
                {
                    if (serverThread == null || !serverThread.IsAlive)
                    {
                        initServerThr();
                    }
                    Thread.Sleep(10000);
                }
                DCLogger.LogInfo("InnerGuardThr exited.");
            }
            ));
            innerGuardThread.IsBackground = true;
            innerGuardThread.Start();
        }

        //private ConcurrentDictionary<string, string> cltInfo = new ConcurrentDictionary<string, string>();
        //private void initMngThr()
        //{
        //    mngThread = new Thread(new ThreadStart(delegate()
        //    {
        //        using (Socket receiver = Socket.Create(SocketType.Pull))
        //        {
        //            try
        //            {
        //                receiver.Bind(cfg.MngAddr); receiver.Bind(Global.INNER_CTRL_MNGTHR + configName);

        //                byte[] buffer = new byte[8388608]; //int size = 0;
        //                while (isWork)
        //                {
        //                    try
        //                    {
        //                        byte[] envelope;// = receiver.Receive();
        //                        byte[] data;// = receiver.Receive();
        //                        receiver.Receive(out envelope, out data);
        //                        if (envelope == null || envelope.Length == 0)
        //                        {
        //                            //Global.Log("Null envelope.");
        //                            continue;
        //                        }
        //                        if (data == null)
        //                        {
        //                            //Global.Log("Null data.");
        //                            continue;
        //                        }

        //                        string id = Global.Encoding.GetString(envelope);
        //                        string info = Global.Encoding.GetString(data);

        //                        cltInfo.AddOrUpdate(id, info, (key, oldVal) => info);
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        DCLogger.LogError("{0} get an error.[{1}]", "MngThr work", e.Message);
        //                    }
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                DCLogger.LogFatal("{0} get a fatal error.[{1}]", "MngThr init", e.Message);
        //            }
        //        }

        //        //DCLogger.LogInfo("MngThr exited.");
        //        mngThrExited = true;
        //    }
        //    ));
        //    mngThread.IsBackground = true;
        //    mngThread.Start();
        //    mngThrExited = false;
        //}

        public override void StopWork()
        {
            if (isStart)
            {
                isWork = false;
                killServerThr();
                while (!serverThrExited)
                {
                    Thread.Sleep(100);
                }

                isStart = false;

                DCLogger.LogInfo("ConsoleServer stopped..");
            }
            else
            {
                DCLogger.LogInfo("ConsoleServer already stopped..");
            }
        }

        private void killServerThr()
        {
            if (receiver != null)
                receiver.Dispose();
            //using (Socket killer = Socket.Create(SocketType.Push, cfg.BindingType))
            //{
            //    killer.Connect(Global.INNER_CTRL_SERVERTHR + configName);
            //    killer.Send(new byte[0], new byte[0]);
            //}
        }

    }
}
