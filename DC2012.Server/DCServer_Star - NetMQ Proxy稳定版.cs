using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetMQ;

namespace DC
{
    public class DCServer_Star : DCServer
    {
        private string configName;
        private bool isStart = false;

        private ServerCfg cfg;

        internal override ServerCfg Cfg
        {
            get { return cfg; }
        }


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

        public override void StartWork()
        {
            if (!isStart)
            {
                Task.Factory.StartNew(initServerThr);
                //initServerThr();
                isStart = true;
                DCLogger.LogInfo("ConsoleServer started..");
            }
            else
            {
                DCLogger.LogInfo("ConsoleServer already started..");
            }
        }

        Poller poller;
        Proxy proxy;
        private void initServerThr()
        {
            using (NetMQContext context = NetMQContext.Create())
            {
                using (var receiver = context.CreateXSubscriberSocket())
                using (var publisher = context.CreateXPublisherSocket())
                {
                    try
                    {
                        receiver.Bind(cfg.ServerR);
                        publisher.Bind(cfg.ServerS);
                    }
                    catch (Exception ex)
                    {
                        DCLogger.LogError("Exception in ServerThr's bind work:{0}", ex.Message);
                    }

                    // send heartbeat every two seconds
                    poller = new Poller();
                    NetMQTimer heartbeatTimer = new NetMQTimer(10000);
                    poller.AddTimer(heartbeatTimer);
                    heartbeatTimer.Elapsed += (sender, eventArgs) =>
                    {
                        try
                        {
                            publisher.SendMultipartBytes(Global.DC_CTRL_HB, BitConverter.GetBytes(DateTime.Now.Ticks));
                            DCLogger.LogInfo("Server's HeartBeat sended:{0}", DateTime.Now);
                        }
                        catch (Exception ex)
                        {
                            DCLogger.LogError("Exception in ServerThr's heartbeat work:{0}", ex.Message);
                        }
                    };

                    poller.PollTillCancelledNonBlocking();

                    // proxy messages between frontend / backend
                    proxy = new Proxy(receiver, publisher);

                    // blocks indefinitely
                    proxy.Start();

                    ////r & s                    
                    //receiver.ReceiveReady += (sender, eventArgs) =>
                    //    {
                    //        try
                    //        {

                    //            List<byte[]> dataLst = new List<byte[]>();
                    //            receiver.ReceiveMultipartBytes(ref dataLst, 2);
                    //            DCLogger.LogInfo("Received:{0}", Global.Encoding.GetString(dataLst[0]));

                    //            if (dataLst.Count != 2)
                    //                return;

                    //            publisher.SendMultipartBytes(dataLst[0], dataLst[1]);
                    //            DCLogger.LogInfo("Sended:{0}", Global.Encoding.GetString(dataLst[0]));
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            DCLogger.LogError("Exception in ServerThr's exchange work:{0}", ex.Message);
                    //        }
                    //    };


                    //poller.AddSocket(receiver);

                }
            }

        }

        public override void StopWork()
        {
            if (isStart)
            {
                killServerThr();

                DCLogger.LogInfo("ConsoleServer stopped..");
            }
            else
            {
                DCLogger.LogInfo("ConsoleServer already stopped..");
            }
        }

        private void killServerThr()
        {
            try
            {
                poller.Cancel();
                proxy.Stop();
            }
            catch { }
        }

    }
}
