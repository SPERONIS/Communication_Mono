using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using MsgPack.Serialization;
using MsgPack;

namespace DC
{
    public static class DCSerializer
    {
        static ConcurrentDictionary<Type, IMessagePackSerializer> serDic = new ConcurrentDictionary<Type, IMessagePackSerializer>();
        public static T BytesToObj<T>(byte[] data)
        {
            if (data == null || data.Length == 0)
                return default(T);

            return (T)BytesToObj(typeof(T), data);
        }

        public static object BytesToObj(Type type, byte[] data)
        {
            if (data == null || data.Length == 0)
                return null;

            IMessagePackSerializer ser = serDic.GetOrAdd(type, MessagePackSerializer.Create(type));
            MemoryStream stream = new MemoryStream(data);
            Unpacker up = Unpacker.Create(stream);
            up.Read();
            return ser.UnpackFrom(up);
        }

        public static byte[] ObjToBytes(object obj)
        {
            if (obj == null)
                return null;

            Type type = obj.GetType();
            IMessagePackSerializer ser = serDic.GetOrAdd(type, MessagePackSerializer.Create(type));
            MemoryStream stream = new MemoryStream();
            ser.PackTo(Packer.Create(stream), obj);
            byte[] data = stream.ToArray();
            return data;
        }
    }
}
