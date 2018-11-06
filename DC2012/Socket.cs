using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DC
{
    public enum SocketType
    {
        Push,
        Pull,
        Pub,
        Sub
    }

    public class Socket : IDisposable
    {
        static public Socket Create(SocketType socketType, string classType)
        {
            Socket socket = Activator.CreateInstance(Type.GetType(classType), socketType) as Socket;
            return socket;
        }

        public virtual void Connect(string address)
        {
            throw new NotSupportedException();
        }

        public virtual void Bind(string address)
        {
            throw new NotSupportedException();
        }

        public virtual void Subscribe(byte[] topic)
        {
            throw new NotSupportedException();
        }

        public virtual void Unsubscribe(byte[] topic)
        {
            throw new NotSupportedException();
        }

        //public virtual void SendMore(byte[] envelope)
        //{
        //    throw new NotSupportedException();
        //}

        //public virtual void Send(byte[] data)
        //{
        //    throw new NotSupportedException();
        //}

        //public virtual byte[] Receive()
        //{
        //    throw new NotSupportedException();
        //}

        public virtual void Send(byte[] envelope, byte[] data)
        {
            throw new NotSupportedException();
        }

        public virtual void TrySend(int timeOutMilliseconds, byte[] envelope, byte[] data)
        {
            throw new NotSupportedException();
        }

        public virtual void Receive(out byte[] envelope, out byte[] data)
        {
            throw new NotSupportedException();
        }

        public virtual void TryReceive(int timeOutMilliseconds, out byte[] envelope, out byte[] data)
        {
            throw new NotSupportedException();
        }

        public virtual void Dispose()
        {
            throw new NotSupportedException();
        }
    }
}
