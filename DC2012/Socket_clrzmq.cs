using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroMQ;

namespace DC
{
    public class Socket_clrzmq : Socket
    {
        static ZContext ctx;
        public static ZContext Ctx
        {
            get
            {
                if (ctx == null)
                {
                    ctx = ZContext.Create();
                    //ctx.ThreadPoolSize = 8;
                }

                return ctx;
            }
        }

        private ZSocket innerSocket;

        private Socket_clrzmq(ZSocket realSocket)
        {
            this.innerSocket = realSocket;
        }

        static void SetSocketOpt(ZSocket socket)
        {
            socket.TcpKeepAlive = TcpKeepaliveBehaviour.Enable;
            socket.ReconnectIntervalMax = TimeSpan.FromMilliseconds(30000);
            socket.Linger = TimeSpan.FromMilliseconds(0);
        }

        static public Socket_clrzmq Create(SocketType socketType)
        {
            ZSocket socket;
            switch (socketType)
            {
                case SocketType.Pub:
                    socket = new ZSocket(Socket_clrzmq.Ctx, ZSocketType.PUB);
                    SetSocketOpt(socket);
                    return new Socket_clrzmq(socket);
                case SocketType.Pull:
                    socket = new ZSocket(Socket_clrzmq.Ctx, ZSocketType.PULL);
                    return new Socket_clrzmq(socket);
                case SocketType.Push:
                    socket = new ZSocket(Socket_clrzmq.Ctx, ZSocketType.PUSH);
                    return new Socket_clrzmq(socket);
                case SocketType.Sub:
                    socket = new ZSocket(Socket_clrzmq.Ctx, ZSocketType.SUB);
                    return new Socket_clrzmq(socket);
                default:
                    throw new Exception("Non support SocketType.");
            }
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

        //public override void SendMore(byte[] envelope)
        //{
        //    this.innerSocket.SendMore(envelope);
        //}

        //public override void Send(byte[] data)
        //{
        //    this.innerSocket.Send(data);
        //}

        //public override byte[] Receive()
        //{
        //    return this.innerSocket.Receive();
        //}

        public override void Send(byte[] envelope, byte[] data)
        {
            using (var message = new ZMessage())
            {
                message.Add(new ZFrame(envelope));
                message.Add(new ZFrame(data));
                this.innerSocket.Send(message);
            }
        }

        public override void Receive(out byte[] envelope, out byte[] data)
        {
            using (ZMessage message = this.innerSocket.ReceiveMessage())
            {
                // Read envelope with address
                envelope = message[0].Read();

                // Read message contents
                data = message[1].Read();
            }
        }

        public override void Dispose()
        {
            //this.innerSocket.IgnoreErrors = true;
            this.innerSocket.Dispose();
        }
    }
}
