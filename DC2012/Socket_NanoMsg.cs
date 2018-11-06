using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NNanomsg;
using NNanomsg.Protocols;

namespace DC
{
    public class Socket_NanoMsg : Socket
    {
        private NanomsgSocketBase innerSocket;

        private Socket_NanoMsg(NanomsgSocketBase realSocket)
        {
            this.innerSocket = realSocket;
        }

        //static void SetSocketOpt(NetMQSocket socket)
        //{
        //    socket.Options.TcpKeepalive = true;
        //    socket.Options.ReconnectIntervalMax = TimeSpan.FromMilliseconds(30000);
        //    socket.Options.Linger = TimeSpan.FromMilliseconds(0);
        //    //socket.KeepAlive = true;
        //    //socket.ReconnectIntervalMax = TimeSpan.FromMilliseconds(30000);
        //    //socket.Linger = TimeSpan.FromMilliseconds(0);
        //}

        static public Socket_NanoMsg Create(SocketType socketType)
        {
            NanomsgSocketBase socket;
            switch (socketType)
            {
                case SocketType.Pub:
                    socket = new PublishSocket();
                    return new Socket_NanoMsg(socket);
                case SocketType.Pull:
                    socket = new PullSocket();
                    return new Socket_NanoMsg(socket);
                case SocketType.Push:
                    socket = new PushSocket();
                    return new Socket_NanoMsg(socket);
                case SocketType.Sub:
                    socket = new SubscribeSocket();
                    return new Socket_NanoMsg(socket);
                default:
                    throw new Exception("Non support SocketType.");
            }
        }

        public override void Connect(string address)
        {
            NN.Connect(this.innerSocket.SocketID, address);
        }

        public override void Bind(string address)
        {
            NN.Bind(this.innerSocket.SocketID, address);
        }

        public override void Subscribe(byte[] topic)
        {
            (this.innerSocket as SubscribeSocket).Subscribe(topic);
        }

        public override void Unsubscribe(byte[] topic)
        {
            (this.innerSocket as SubscribeSocket).Unsubscribe(topic);
        }

        //public override void SendMore(byte[] envelope)
        //{
        //    NN.Send(this.innerSocket.SocketID, envelope, SendRecvFlags.DONTWAIT);
        //    //this.innerSocket.SendMore(envelope);
        //}

        //public override void Send(byte[] data)
        //{
        //    NN.Send(this.innerSocket.SocketID, data, SendRecvFlags.DONTWAIT);
        //    //this.innerSocket.Send(data);
        //}

        //public override byte[] Receive()
        //{
        //    byte[] data;
        //    NN.Recv(this.innerSocket.SocketID, out data, SendRecvFlags.NONE);
        //    return data;
        //    //return this.innerSocket.Receive();
        //}

        public override void Send(byte[] envelope, byte[] data)
        {
            byte[] tnSnd = envelope.Concat(Global.NANOMSG_SPLITTER).Concat(data).ToArray();
            NN.Send(this.innerSocket.SocketID, tnSnd, SendRecvFlags.NONE);
            //this.innerSocket.SendMore(envelope);
            //this.innerSocket.Send(data);
        }

        public override void Receive(out byte[] envelope, out byte[] data)
        {
            byte[] toRcv;
            NN.Recv(this.innerSocket.SocketID, out toRcv, SendRecvFlags.NONE);

            int envEnd = this.IndexOf(toRcv, Global.NANOMSG_SPLITTER);
            int dataStart = envEnd + Global.NANOMSG_SPLITTER.Length;

            envelope = new byte[envEnd];
            data = new byte[toRcv.Length - dataStart];
            Array.Copy(toRcv, 0, envelope, 0, envEnd);
            Array.Copy(toRcv, dataStart, data, 0, toRcv.Length - dataStart);
            //var contains = !Global.NANOMSG_SPLITTER.Except(toRcv).Any();

            //envelope = this.innerSocket.Receive();
            //data = this.innerSocket.Receive();
        }

        public override void Dispose()
        {
            this.innerSocket.Dispose();
        }

        /// <summary>
        /// 报告指定的 System.Byte[] 在此实例中的第一个匹配项的索引。
        /// </summary>
        /// <param name="srcBytes">被执行查找的 System.Byte[]。</param>
        /// <param name="searchBytes">要查找的 System.Byte[]。</param>
        /// <returns>如果找到该字节数组，则为 searchBytes 的索引位置；如果未找到该字节数组，则为 -1。如果 searchBytes 为 null 或者长度为0，则返回值为 -1。</returns>
        internal int IndexOf(byte[] srcBytes, byte[] searchBytes)
        {
            if (srcBytes == null) { return -1; }
            if (searchBytes == null) { return -1; }
            if (srcBytes.Length == 0) { return -1; }
            if (searchBytes.Length == 0) { return -1; }
            if (srcBytes.Length < searchBytes.Length) { return -1; }
            for (int i = 0; i < srcBytes.Length - searchBytes.Length; i++)
            {
                if (srcBytes[i] == searchBytes[0])
                {
                    if (searchBytes.Length == 1) { return i; }
                    bool flag = true;
                    for (int j = 1; j < searchBytes.Length; j++)
                    {
                        if (srcBytes[i + j] != searchBytes[j])
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag) { return i; }
                }
            }
            return -1;
        }
    }
}
