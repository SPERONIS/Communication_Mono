﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using System.Threading.Tasks;

namespace DC
{
    public class DCClient_Star : DCClient
    {
        #region internal

        private ClientCfg cfg;

        internal override ClientCfg Cfg
        {
            get { return cfg; }
        }

        private bool disposed = false;

        private ConcurrentDictionary<string, ConcurrentDictionary<Action<byte[]>, object>> subHands = new ConcurrentDictionary<string, ConcurrentDictionary<Action<byte[]>, object>>();

        private ConcurrentDictionary<string, Func<string, byte[], byte[]>> srvTypes = new ConcurrentDictionary<string, Func<string, byte[], byte[]>>();

        private ConcurrentDictionary<string, ConcurrentDictionary<Action<byte[]>, object>> rcvHands = new ConcurrentDictionary<string, ConcurrentDictionary<Action<byte[]>, object>>();

        private ConcurrentDictionary<int, Action<ActResult>> actResultHands = new ConcurrentDictionary<int, Action<ActResult>>();

        internal DCClient_Star(ClientCfg clientCfg, string id)
        {
            this.id = id;
            this.guidStr = Guid.NewGuid().ToString();//Global.Encoding.GetString(guid);
            this.guid = Global.Encoding.GetBytes(guidStr);//Guid.NewGuid().ToByteArray();
            cfg = clientCfg;
            init();

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!disposed)
                throw e.ExceptionObject as Exception;
        }

        ~DCClient_Star()
        {
            this.Dispose();
        }

        protected internal void init()
        {
            Task.Factory.StartNew(initClientR);
            while (clientR == null || clientS == null)
            {
                Thread.Sleep(100);
            }
            //initClientS();

            this.Srv(Global.DC_CTRL_PING, (ping, pong) =>
            {
                return Global.DC_CTRL_ACK;
            });
        }

        NetMQContext context = NetMQContext.Create();
        private object clientSSync = new object();
        private PushSocket clientS;

        private object clientRSync = new object();
        private SubscriberSocket clientR;
        Poller poller;
        long lastTicksOfHb;
        private void initClientR()
        {
            poller = new Poller();
            {
                //clientR = context.CreateSubscriberSocket();
                //clientS = context.CreatePushSocket();
                {
                    try
                    {
                        createRSocket();

                        createSSocket();
                    }
                    catch (Exception ex)
                    {
                        DCLogger.LogError("Exception in connect work:{0}", ex.Message);
                    }

                    //r & s                    
                    clientR.ReceiveReady += (sender, eventArgs) =>
                    {
                        try
                        {

                            List<byte[]> dataLst = new List<byte[]>();
                            clientR.ReceiveMultipartBytes(ref dataLst, 2);

                            //if (dataLst.Count != 2)
                            //    return;

                            byte[] envelope = dataLst[0];
                            byte[] data = dataLst[1];

                            //if (envelope == null)
                            //    return;

                            if (DCUtil.BytesStart(Global.DC_CTRL_HB, envelope))
                            {
                                lastTicksOfHb = BitConverter.ToInt64(data, 0);
                                DCLogger.LogInfo("Server's HeartBeat received: {0}", new DateTime(lastTicksOfHb));
                            }

                            #region pubDataR 处理Sub接口订阅的数据
                            else if (DCUtil.BytesStart(Global.DC_HEAD_PS_PUB_BYTES, envelope))
                            {
                                string envelopeStr = Global.Encoding.GetString(envelope);
                                string envelopeStr0 = string.Concat(Global.DC_HEAD_PS_PUB, Global.ENVELOPE_SPLITTER, envelopeStr.Split(new string[] { Global.ENVELOPE_SPLITTER }, StringSplitOptions.None)[1]);
                                //Console.WriteLine("keys:" + string.Concat(clientRHands.Keys) + "_key:" + envelopeStr);

                                ConcurrentDictionary<Action<byte[]>, object> actions;
                                bool envelopeStrBingo = false;
                                if (subHands.TryGetValue(envelopeStr, out actions))
                                {
                                    envelopeStrBingo = true;

                                    foreach (Action<byte[]> handler in actions.Keys)
                                    {
                                        if (handler != null)
                                        {
                                            handler(data);
                                        }
                                    }
                                }
                                if (!envelopeStrBingo)
                                {
                                    if (subHands.TryGetValue(envelopeStr0, out actions))
                                    {
                                        foreach (Action<byte[]> handler in actions.Keys)
                                        {
                                            if (handler != null)
                                            {
                                                try
                                                {
                                                    handler(data);
                                                }
                                                catch (Exception ex)
                                                {
                                                    DCLogger.LogError("Exception occured when handling pub's data:{0}", ex.Message);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region actDataR 处理Srv接口接收的Act请求
                            else if (DCUtil.BytesStart(Global.DC_HEAD_AS_ACT_BYTES, envelope))
                            {
                                string envelopeStr = Global.Encoding.GetString(envelope);

                                string[] actDetail = envelopeStr.Split(new string[] { Global.ENVELOPE_SPLITTER }, StringSplitOptions.None);
                                if (srvTypes.ContainsKey(actDetail[1 + 1]))
                                {
                                    Thread inlineThread = new Thread(new ThreadStart(delegate()
                                    {
                                        byte[] actResultData = null;
                                        string replySn = "0";
                                        string extInfo = string.Empty;
                                        string lastSlice = actDetail[actDetail.Length - 1];
                                        try
                                        {
                                            replySn = actDetail[1 + 3];
                                            int idx = lastSlice.IndexOf("?");
                                            if (idx > 0)
                                            {
                                                extInfo = lastSlice.Substring(idx + 1);
                                            }
                                            actResultData = srvTypes[actDetail[1 + 1]](extInfo, data);
                                        }
                                        catch (Exception ex)
                                        {
                                            string srvSideException = string.Format("Srv side exception [{0}][{1}]: ", id, actDetail[1 + 1]) + ex.Message;
                                            actResultData = Global.Encoding.GetBytes(Global.Encoding.GetString(Global.DC_RPLY_EXCEPTION) + srvSideException);
                                            DCLogger.LogError(srvSideException);
                                        }
                                        if (actResultData == null)
                                        {
                                            actResultData = Global.DC_NULL_RPLY;
                                        }
                                        lock (clientSSync)
                                        {
                                            try
                                            {
                                                clientS.SendMultipartBytes(Global.Encoding.GetBytes(string.Concat(Global.DC_HEAD_AS_RPLY, Global.ENVELOPE_SPLITTER, actDetail[1 + 2], Global.ENVELOPE_SPLITTER, actDetail[1 + 1], Global.ENVELOPE_SPLITTER, id, Global.ENVELOPE_SPLITTER, replySn)), actResultData);
                                            }
                                            catch (Exception ex)
                                            {
                                                DCLogger.LogError("Exception occured when srv sending reply data back:{0}", ex.Message);
                                            }
                                        }
                                    }
                                    ));
                                    inlineThread.IsBackground = true;
                                    inlineThread.Start();
                                }
                            }
                            #endregion

                            #region srvRplyDataR 处理Act请求的Srv Reply
                            else if (DCUtil.BytesStart(Global.DC_HEAD_AS_RPLY_BYTES, envelope))
                            {
                                string envelopeStr = Global.Encoding.GetString(envelope);
                                if (DCUtil.BytesEqual(Global.DC_NULL_RPLY, data))
                                {
                                    data = null;
                                }

                                string[] replyDetail = envelopeStr.Split(new string[] { Global.ENVELOPE_SPLITTER }, StringSplitOptions.None);
                                int snKey = int.Parse(replyDetail[1 + 3]);
                                System.Diagnostics.Debug.WriteLine("ActResult received [{0}]", snKey);

                                Action<ActResult> handler;
                                if (actResultHands.TryGetValue(snKey, out handler))
                                {
                                    ActResult ar = new ActResult { ResultData = data, Type = replyDetail[1 + 1], Src = replyDetail[1 + 2], ActSn = int.Parse(replyDetail[1 + 3]) };
                                    actResultHands.TryRemove(snKey, out handler);
                                    System.Diagnostics.Debug.WriteLine("Act Key removed [{0}]", snKey);
                                    if (DCUtil.BytesStart(Global.DC_RPLY_EXCEPTION, ar.ResultData))
                                    {
                                        ar.Exception = new Exception(Global.Encoding.GetString(ar.ResultData, Global.DC_RPLY_EXCEPTION.Length, ar.ResultData.Length - Global.DC_RPLY_EXCEPTION.Length));
                                    }
                                    try
                                    {
                                        //Task.Factory.StartNew(() =>handler(ar));
                                        ThreadPool.QueueUserWorkItem(o =>
                                        {
                                            ActResult ar_real = ar as ActResult;
                                            handler(ar_real);
                                        }, ar);
                                    }
                                    catch (Exception ex)
                                    {
                                        DCLogger.LogError("Exception occured when act handling srv's reply data:{0}", ex.Message);
                                    }

                                    StringAndTime tnt = new StringAndTime { UniqueString = ar.Src, LastRefresh = DateTime.Now.Ticks };
                                    type2Srv.AddOrUpdate(
                                        ar.Type,
                                        new List<StringAndTime>(),
                                        (key, oldValue) =>
                                        {
                                            if (oldValue.Contains(tnt))
                                            {
                                                oldValue[oldValue.IndexOf(tnt)].LastRefresh = DateTime.Now.Ticks;
                                            }
                                            else
                                            {
                                                oldValue.Add(tnt);
                                                //Console.WriteLine("List<TypeAndTime>.Count: {0}", oldValue.Count);
                                            }
                                            //DCLogger.LogTrace("Refresh valid datetime of srv [{0}] type [{1}]", ar.Src, ar.Type);
                                            return oldValue;
                                        }
                                        );
                                }
                            }
                            #endregion

                            #region sndDataR 处理Rcv接口接受的数据
                            else if (DCUtil.BytesStart(Global.DC_HEAD_SR_SND_BYTES, envelope))
                            {

                                string envelopeStr = Global.Encoding.GetString(envelope);

                                ConcurrentDictionary<Action<byte[]>, object> actions;
                                if (rcvHands.TryGetValue(envelopeStr, out actions))
                                {
                                    foreach (Action<byte[]> handler in actions.Keys)
                                    {
                                        if (handler != null)
                                        {
                                            try
                                            {
                                                handler(data);
                                            }
                                            catch (Exception ex)
                                            {
                                                DCLogger.LogError("Exception occured when rcv handling snd's data:{0}", ex.Message);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            DCLogger.LogError("Exception in receive data proc work:{0}", ex.Message);
                        }
                    };


                    poller.AddSocket(clientR);

                    // send report every 8 seconds
                    NetMQTimer heartbeatTimer = new NetMQTimer(5000);
                    poller.AddTimer(heartbeatTimer);
                    heartbeatTimer.Elapsed += (sender, eventArgs) =>
                    {
                        try
                        {
                            if (lastTicksOfHb > 0 && (DateTime.Now.Ticks - lastTicksOfHb) > (38 * 10000000))
                            {
                                DCLogger.LogError("Server's connection losed");
                                reconnect();
                                //new System.Threading.Tasks.Task(() => reconnect()).Start();
                            }


                            IEnumerator<string> itor = this.srvTypes.Keys.GetEnumerator();
                            List<CltSrvInfo> srvInfos = new List<CltSrvInfo>();
                            while (itor.MoveNext())
                            {
                                if (!itor.Current.Equals(Global.DC_CTRL_PING))
                                    srvInfos.Add(new CltSrvInfo { ClientId = this.id, ClientSrvType = itor.Current });
                            }
                            if (srvInfos.Count > 0)
                            {
                                byte[] data = DCSerializer.ObjToBytes(srvInfos);
                                Task.Factory.StartNew(() =>
                                    this.Act(Global.DC_DIR_CLTSRVINFO_RPT, Global.DC_DIR_SRVID, data, 5)
                                    );
                                DCLogger.LogInfo("Report client status ok.");
                            }
                        }
                        catch (TimeoutException)
                        {
                            DCLogger.LogError("Report client status failed.");
                        }
                        catch (Exception ex)
                        {
                            DCLogger.LogError("Report detect a unhandled error:{0}", ex.Message);
                        }
                    };

                    poller.PollTillCancelled();
                }
            }
        }

        private void createSSocket()
        {
            clientS = context.CreatePushSocket();
            string[] srAddrs = cfg.ServerR.Split(',');
            foreach (string addr in srAddrs)
            {
                clientS.Connect(addr);
            }
        }

        private void createRSocket()
        {
            clientR = context.CreateSubscriberSocket();
            string[] addrs = cfg.ServerS.Split(',');
            foreach (string addr in addrs)
            {
                clientR.Connect(addr);
            }
            clientR.Subscribe(Global.DC_CTRL_HB);    //接收心跳数据包
            clientR.Subscribe(Global.Encoding.GetBytes(string.Concat(Global.DC_HEAD_AS_RPLY, Global.ENVELOPE_SPLITTER, this.guidStr)));    //接收Act操作的回复数据包
        }

        private void reconnect()
        {
            DCLogger.LogError("Connection broked.");
            try
            {
                //if (clientR != null)
                //    clientR.Dispose();
                //createRSocket();
                string[] addrs = cfg.ServerS.Split(',');
                foreach (string addr in addrs)
                {
                    clientR.Disconnect(addr);
                    clientR.Connect(addr);
                }
                DCLogger.LogInfo("Connection reonlined.");
            }
            catch (Exception ex)
            {
                DCLogger.LogError("Exception when establish reconnect:{0}", ex.Message);
            }
        }

        protected override void Dispose()
        {
            if (!disposed)
            {
                poller.Cancel();
                clientR.Dispose();
                clientS.Dispose();
                context.Dispose();

                disposed = true;
            }
        }
        #endregion

        #region interface

        public override bool Disposed
        {
            get { return disposed; }
        }


        #region P/S interface

        public override void Pub(string type, byte[] data)
        {
            byte[] envelope = Global.Encoding.GetBytes(string.Concat(Global.DC_HEAD_PS_PUB, Global.ENVELOPE_SPLITTER, type, Global.ENVELOPE_SPLITTER, id));
            lock (clientSSync)
            {
                clientS.SendMultipartBytes(envelope, data);
            }
        }

        public override void Sub(string type, Action<byte[]> handler)
        {
            this.Sub(type, string.Empty, handler);
        }

        public override void UnSub(string type, Action<byte[]> handler)
        {
            this.UnSub(type, string.Empty, handler);
        }

        private object obj = new object();
        public override void Sub(string type, string publisher, Action<byte[]> handler)
        {
            if (string.IsNullOrEmpty(type))
                throw new ArgumentNullException("type");
            if (handler == null)
                throw new ArgumentNullException("handler");

            string envelope = null;
            if (string.IsNullOrEmpty(publisher))
            {
                envelope = string.Concat(Global.DC_HEAD_PS_PUB, Global.ENVELOPE_SPLITTER, type);
            }
            else
            {
                envelope = string.Concat(Global.DC_HEAD_PS_PUB, Global.ENVELOPE_SPLITTER, type, Global.ENVELOPE_SPLITTER, publisher);
            }

            Thread inlineThread = new Thread(new ThreadStart(delegate()
            {
                ConcurrentDictionary<Action<byte[]>, object> actions = subHands.GetOrAdd(envelope, new ConcurrentDictionary<Action<byte[]>, object>());
                actions.GetOrAdd(handler, obj);
                System.Diagnostics.Debug.WriteLine("Sub Thr[" + Thread.CurrentThread.Name + "] Exited.");
            }
            ));
            inlineThread.Start();

            clientR.Subscribe(Global.Encoding.GetBytes(envelope));
        }

        public override void UnSub(string type, string publisher, Action<byte[]> handler)
        {
            if (string.IsNullOrEmpty(type))
                throw new ArgumentNullException("type");
            if (handler == null)
                throw new ArgumentNullException("handler");

            string envelope = null;
            if (string.IsNullOrEmpty(publisher))
            {
                envelope = string.Concat(Global.DC_HEAD_PS_PUB, Global.ENVELOPE_SPLITTER, type);
            }
            else
            {
                envelope = string.Concat(Global.DC_HEAD_PS_PUB, Global.ENVELOPE_SPLITTER, type, Global.ENVELOPE_SPLITTER, publisher);
            }

            ConcurrentDictionary<Action<byte[]>, object> actions;
            if (subHands.TryGetValue(envelope, out actions))
            {
                //((ICollection<KeyValuePair<Action<byte[]>, object>>)actions).Remove(new KeyValuePair<Action<byte[]>, object>(handler, actions[handler]));
                //if (actions.Count == 0)
                //{
                //    ((ICollection<KeyValuePair<string, ConcurrentDictionary<Action<byte[]>, object>>>)subHands).Remove(string, new KeyValuePair<string, ConcurrentDictionary<Action<byte[]>, object>>(envelope, subHands[envelope]));
                //}

                Thread inlineThread = new Thread(new ThreadStart(delegate()
                {
                    object tempo;
                    bool removed1 = false;
                    while (!removed1)
                    {
                        removed1 = !actions.ContainsKey(handler) || actions.TryRemove(handler, out tempo);
                        Thread.Sleep(5);
                    }

                    if (actions.Count == 0)
                    {
                        ConcurrentDictionary<Action<byte[]>, object> lst;
                        bool removed2 = false;
                        while (!removed2)
                        {
                            removed2 = !subHands.ContainsKey(envelope) || subHands.TryRemove(envelope, out lst);
                            Thread.Sleep(5);
                        }
                    }

                    System.Diagnostics.Debug.WriteLine("UnSub Thr[" + Thread.CurrentThread.Name + "] Exited.");
                }
                ));
                inlineThread.Start();
            }
            clientR.Unsubscribe(Global.Encoding.GetBytes(envelope));
        }

        #endregion

        #region A/S interface

        private int actSn = 0;

        private int ActSn
        {
            get
            {
                actSn++;
                if (actSn > 99999)
                {
                    actSn = 1;
                }
                return actSn;
            }
        }

        public override int Act(string type, string dest, byte[] data, Action<ActResult> handler)
        {
            int actSn = ActSn;
            actResultHands.AddOrUpdate(actSn, handler, (key, oldValue) => handler);
            int idx = type.IndexOf("?");
            string extInfo = string.Empty;
            if (idx > 0)
            {
                extInfo = type.Substring(idx);
                type = type.Substring(0, idx);
            }
            string head = string.Concat(Global.DC_HEAD_AS_ACT, Global.ENVELOPE_SPLITTER, dest, Global.ENVELOPE_SPLITTER, type, Global.ENVELOPE_SPLITTER, guidStr, Global.ENVELOPE_SPLITTER, actSn.ToString("D5"), Global.ENVELOPE_SPLITTER, id, extInfo);
            byte[] envelope = Global.Encoding.GetBytes(head);
            lock (clientSSync)
            {
                clientS.SendMultipartBytes(envelope, data);
            }

            return actSn;
        }

        public override ActResult Act(string type, string dest, byte[] data)
        {
            ActResult ret = null;
            this.Act(type, dest, data, (a) => { ret = a; });

            int waitTimes = 0;
            while (ret == null && waitTimes++ < 50)
            {
                Thread.Sleep(100);
            }

            if (ret == null)
            {
                throw new Exception(string.Format("Act {0} at {1} time out.", type, dest));
            }

            if (ret.Exception != null)
            {
                throw ret.Exception;
            }

            return ret;
        }

        public override ActResult Act(string type, string dest, byte[] data, int timeoutSeconds)
        {
            if (timeoutSeconds < 1)
            {
                throw new ArgumentException("Illegal timeoutSeconds.", "timeoutSeconds");
            }

            ActResult ret = null;
            this.Act(type, dest, data, (a) => { ret = a; });

            int waitTimes = 0;
            while (ret == null && waitTimes++ < (timeoutSeconds * 10))
            {
                Thread.Sleep(100);
            }

            if (ret == null)
            {
                throw new TimeoutException(string.Format("Act {0} at {1} time out.", type, dest));
            }

            if (ret.Exception != null)
            {
                throw ret.Exception;
            }

            return ret;
        }

        private ConcurrentDictionary<string, List<StringAndTime>> type2Srv = new ConcurrentDictionary<string, List<StringAndTime>>();
        public override int Act(string type, byte[] data, Action<ActResult> handler)
        {
            string baseType = type;
            int idx = baseType.IndexOf('?');
            if (idx > 0)
            {
                baseType = baseType.Substring(0, idx);
            }
            List<StringAndTime> srvId = null;
            if (type2Srv.TryGetValue(baseType, out srvId))
            {
                //DCLogger.LogTrace("srvId.Count = {0}", srvId.Count);
                srvId.RemoveAll((item) =>
                {
                    //DCLogger.LogTrace("DateTime.Now.Ticks - item.LastRefresh = {0}", DateTime.Now.Ticks - item.LastRefresh);
                    return (DateTime.Now.Ticks - item.LastRefresh) > (60 * 10000000);
                });
            }
            //if (type2Srv.ContainsKey(baseType))
            //{
            //    srvId = type2Srv[baseType];
            //    srvId.RemoveAll((item) => { return (DateTime.Now.Ticks - item.LastRefresh) > (30 * 1000); });
            //}

            if (srvId == null || srvId.Count == 0)
            {
                ActResult ar = this.Act(Global.DC_DIR_CLTSRVINFO_QRY, Global.DC_DIR_SRVID, Global.Encoding.GetBytes(baseType));
                DCLogger.LogTrace("Query available srv of type [{0}]", baseType);
                if (ar.ResultData.Length > 0)
                {
                    srvId = DCSerializer.BytesToObj<List<StringAndTime>>(ar.ResultData);
                    type2Srv.AddOrUpdate(baseType, srvId, (key, oldValue) => srvId);
                    //if (type2Srv.ContainsKey(baseType))
                    //{
                    //    type2Srv[baseType] = srvId;
                    //}
                    //else
                    //{
                    //    type2Srv.Add(baseType, srvId);
                    //}
                }
                else
                {
                    throw new MissingMethodException(string.Format("No one announce to service {0}", baseType));
                }
            }

            return this.Act(type, srvId[new Random().Next(0, srvId.Count)].UniqueString, data, handler);
        }

        public override ActResult Act(string type, byte[] data)
        {
            ActResult ret = null;
            this.Act(type, data, (a) => { ret = a; });

            int waitTimes = 0;
            while (ret == null && waitTimes++ < 50)
            {
                Thread.Sleep(100);
            }

            if (ret == null)
            {
                throw new TimeoutException(string.Format("Act {0} time out.", type));
            }

            if (ret.Exception != null)
            {
                throw ret.Exception;
            }

            return ret;
        }

        public override ActResult Act(string type, byte[] data, int timeoutSeconds)
        {
            if (timeoutSeconds < 1)
            {
                throw new ArgumentException("Illegal timeoutSeconds.", "timeoutSeconds");
            }

            ActResult ret = null;
            this.Act(type, data, (a) => { ret = a; });

            int waitTimes = 0;
            while (ret == null && waitTimes++ < (timeoutSeconds * 10))
            {
                Thread.Sleep(100);
            }

            if (ret == null)
            {
                throw new TimeoutException(string.Format("Act {0} time out.", type));
            }

            if (ret.Exception != null)
            {
                throw ret.Exception;
            }

            return ret;
        }

        public override void Srv(string type, Func<string, byte[], byte[]> handler)
        {
            if (string.IsNullOrEmpty(type))
                throw new ArgumentNullException("type");
            if (handler == null)
                throw new ArgumentNullException("handler");

            srvTypes.AddOrUpdate(type, handler, (key, oldValue) => handler);
            string subStr = string.Concat(Global.DC_HEAD_AS_ACT, Global.ENVELOPE_SPLITTER, id, Global.ENVELOPE_SPLITTER, type);
            clientR.Subscribe(Global.Encoding.GetBytes(subStr));
        }

        public override void UnSrv(string type, Func<string, byte[], byte[]> handler)
        {
            if (string.IsNullOrEmpty(type))
                throw new ArgumentNullException("type");
            if (handler == null)
                throw new ArgumentNullException("handler");


            Func<string, byte[], byte[]> func;
            srvTypes.TryRemove(key, out func);
            clientR.Unsubscribe(Global.Encoding.GetBytes(string.Concat(Global.DC_HEAD_AS_ACT, Global.ENVELOPE_SPLITTER, id, Global.ENVELOPE_SPLITTER, type)));
        }
        #endregion

        #region S/R interface

        public override void Snd(string type, string dest, byte[] data)
        {
            byte[] envelope = Global.Encoding.GetBytes(string.Concat(Global.DC_HEAD_SR_SND, Global.ENVELOPE_SPLITTER, dest, Global.ENVELOPE_SPLITTER, type));
            lock (clientSSync)
            {
                clientS.SendMultipartBytes(envelope, data);
            }
        }

        public override void Rcv(string type, Action<byte[]> handler)
        {
            if (string.IsNullOrEmpty(type))
                throw new ArgumentNullException("type");
            if (handler == null)
                throw new ArgumentNullException("handler");

            string envelope = string.Concat(Global.DC_HEAD_SR_SND, Global.ENVELOPE_SPLITTER, id, Global.ENVELOPE_SPLITTER, type);

            ConcurrentDictionary<Action<byte[]>, object> actions = rcvHands.GetOrAdd(envelope, new ConcurrentDictionary<Action<byte[]>, object>());
            actions.GetOrAdd(handler, new object());

            clientR.Subscribe(Global.Encoding.GetBytes(envelope));
        }

        public override void UnRcv(string type, Action<byte[]> handler)
        {
            if (string.IsNullOrEmpty(type))
                throw new ArgumentNullException("type");
            if (handler == null)
                throw new ArgumentNullException("handler");

            string envelope = string.Concat(Global.DC_HEAD_SR_SND, Global.ENVELOPE_SPLITTER, id, Global.ENVELOPE_SPLITTER, type);

            ConcurrentDictionary<Action<byte[]>, object> actions;
            if (rcvHands.TryGetValue(envelope, out actions))
            {
                object tempo;
                actions.TryRemove(handler, out tempo);
                if (actions.Count == 0)
                {
                    ConcurrentDictionary<Action<byte[]>, object> lst;
                    rcvHands.TryRemove(envelope, out lst);
                }
            }
            clientR.Unsubscribe(Global.Encoding.GetBytes(envelope));
        }

        #endregion

        #endregion

    }
}
