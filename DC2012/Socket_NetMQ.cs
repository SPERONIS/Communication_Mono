using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetMQ;

namespace DC
{
    public class Socket_NetMQ : Socket
    {
        //static NetMQContext ctx;
        //public static NetMQContext Ctx
        //{
        //    get
        //    {
        //        if (ctx == null)
        //        {
        //            ctx = NetMQContext.Create();
        //            ctx.ThreadPoolSize = 8;
        //        }

        //        return ctx;
        //    }
        //}

        //private NetMQSocket innerSocket;

        //private Socket_NetMQ(NetMQSocket realSocket)
        //{
        //    this.innerSocket = realSocket;
        //}

        //static void SetSocketOpt(NetMQSocket socket)
        //{
        //    socket.Options.TcpKeepalive = true;
        //    socket.Options.ReconnectIntervalMax = TimeSpan.FromMilliseconds(30000);
        //    socket.Options.Linger = TimeSpan.FromMilliseconds(0);
        //}

        //static public Socket_NetMQ Create(SocketType socketType)
        //{
        //    NetMQSocket socket;
        //    switch (socketType)
        //    {
        //        case SocketType.Pub:
        //            //socket = Socket_NetMQ.Ctx.CreateSocket(NetMQ.ZmqSocketType.Pub);
        //            socket = Socket_NetMQ.Ctx.CreateSocket(NetMQ.zmq.ZmqSocketType.Pub);
        //            SetSocketOpt(socket);
        //            return new Socket_NetMQ(socket);
        //        case SocketType.Pull:
        //            //socket = Socket_NetMQ.Ctx.CreateSocket(NetMQ.ZmqSocketType.Pull);
        //            socket = Socket_NetMQ.Ctx.CreateSocket(NetMQ.zmq.ZmqSocketType.Pull);
        //            return new Socket_NetMQ(socket);
        //        case SocketType.Push:
        //            //socket = Socket_NetMQ.Ctx.CreateSocket(NetMQ.ZmqSocketType.Push);
        //            socket = Socket_NetMQ.Ctx.CreateSocket(NetMQ.zmq.ZmqSocketType.Push);
        //            return new Socket_NetMQ(socket);
        //        case SocketType.Sub:
        //            //socket = Socket_NetMQ.Ctx.CreateSocket(NetMQ.ZmqSocketType.Sub);
        //            socket = Socket_NetMQ.Ctx.CreateSocket(NetMQ.zmq.ZmqSocketType.Sub);
        //            return new Socket_NetMQ(socket);
        //        default:
        //            throw new Exception("Non support SocketType.");
        //    }
        //}

        private static NetMQContext innerContext;
        private NetMQSocket innerSocket;

        static Socket_NetMQ()
        {
            innerContext = NetMQContext.Create();
            //innerContext.ThreadPoolSize = 4;    //打开此设置，ConsoleServer重启会导致各Client报异常，不知何故
        }

        public Socket_NetMQ(SocketType socketType)
        {
            switch (socketType)
            {
                case SocketType.Pub:
                    innerSocket = innerContext.CreateSocket(NetMQ.ZmqSocketType.Pub);
                    break;
                case SocketType.Pull:
                    innerSocket = innerContext.CreateSocket(NetMQ.ZmqSocketType.Pull);
                    break;
                case SocketType.Push:
                    innerSocket = innerContext.CreateSocket(NetMQ.ZmqSocketType.Push);
                    break;
                case SocketType.Sub:
                    innerSocket = innerContext.CreateSocket(NetMQ.ZmqSocketType.Sub);
                    break;
                default:
                    throw new Exception("Non support SocketType.");
            }
            innerSocket.Options.TcpKeepalive = true;
            innerSocket.Options.ReconnectInterval = TimeSpan.FromMilliseconds(100);
            innerSocket.Options.ReconnectIntervalMax = TimeSpan.FromMilliseconds(8000);
            innerSocket.Options.Linger = TimeSpan.FromMilliseconds(0);

        }

        public override void Connect(string address)
        {
            this.innerSocket.Connect(address);
        }

        public override void Bind(string address)
        {
            this.innerSocket.Bind(address);
        }

        public override void Subscribe(byte[] topic)
        {
            this.innerSocket.Subscribe(topic);
        }

        public override void Unsubscribe(byte[] topic)
        {
            this.innerSocket.Unsubscribe(topic);
        }

        public override void Send(byte[] envelope, byte[] data)
        {
            //this.innerSocket.SendMore(envelope);
            //this.innerSocket.Send(data);
            this.innerSocket.SendMoreFrame(envelope).SendFrame(data);
        }

        public override void Receive(out byte[] envelope, out byte[] data)
        {
            //envelope = this.innerSocket.Receive();
            //data = this.innerSocket.Receive();
            envelope = this.innerSocket.ReceiveFrameBytes();
            data = this.innerSocket.ReceiveFrameBytes();
        }

        public override void TrySend(int timeOutMilliseconds, byte[] envelope, byte[] data)
        {
            this.innerSocket.TrySendMultipartBytes(TimeSpan.FromMilliseconds(timeOutMilliseconds), envelope, data);
        }

        public override void TryReceive(int timeOutMilliseconds, out byte[] envelope, out byte[] data)
        {
            List<byte[]> lst = new List<byte[]>();
            this.innerSocket.TryReceiveMultipartBytes(TimeSpan.FromMilliseconds(timeOutMilliseconds), ref lst, 2);
            if (lst.Count == 2)
            {
                envelope = lst[0];
                data = lst[1];
            }
            else
            {
                envelope = null;
                data = null;
            }
        }

        public override void Dispose()
        {
            //this.innerSocket.IgnoreErrors = true;
            this.innerSocket.Dispose();
            //this.innerContext.Dispose();
        }
    }
}
