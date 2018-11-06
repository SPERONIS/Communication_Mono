using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
//using CrossroadsIO;
//using NetMQ;

namespace DC
{
    public static class DCUtil
    {
        //public static byte[] Recv(NetMQSocket receiver, byte[] buffer, out int size)
        //{
        //    byte[] buf = receiver.Receive();
        //    size = buf.Length;
        //    return buf;
        //}

        //public static byte[] Recv(NetMQSocket receiver, byte[] buffer, out int size, int timeOut)
        //{
        //    receiver.Options.ReceiveTimeout = TimeSpan.FromSeconds(timeOut);
        //    byte[] buf = receiver.Receive();
        //    size = buf.Length;
        //    return buf; 
        //}

        /// <summary>
        /// 参数字符串转为字典结构
        /// </summary>
        /// <param name="paraStr">例如：username=bird&dob=2003-01-01&sex=male</param>
        /// <returns>例如：
        ///     dic[username]   bird
        ///     dic[dob]        2003-01-01
        ///     dic[sex]        male
        /// </returns>
        public static IDictionary<string, string> ParaStrToDic(string paraStr)
        {
            IDictionary<string, string> dic = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(paraStr))
                return dic;

            string[] paraSlice = paraStr.Split('&');
            foreach (string slice in paraSlice)
            {
                int idx = slice.IndexOf('=');
                dic.Add(slice.Substring(0, idx), slice.Substring(idx + 1));
            }

            return dic;
        }

        public static bool BytesEqual(byte[] src, byte[] dst)
        {
            if (src == null || dst == null)
                return false;
            if (src.Length != dst.Length)
                return false;
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] != dst[i])
                    return false;
            }
            return true;
        }

        public static bool BytesStart(byte[] src, byte[] dst)
        {
            if (src == null || dst == null)
                return false;
            if (src.Length > dst.Length)
                return false;
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] != dst[i])
                    return false;
            }
            return true;
        }

        public static byte[] BytesConcat(byte[] b1, byte[] b2)
        {
            byte[] result = b1.Concat(b2).ToArray();
            return result;
        }

        //public static IList<string> GetLocalIPAddr()
        //{
        //    // First get the host name of local machine.
        //    String strHostName = "";
        //    strHostName = Dns.GetHostName();

        //    // Then using host name, get the IP address list..
        //    IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
        //    IPAddress[] addr = ipEntry.AddressList;

        //    DCLogger.LogInfo("Local host name: " + strHostName);

        //    // address must be IpV4 (for our appl.) and not the loopback address
        //    //for (int i = 0; i < addr.Length; i++)
        //    //{
        //    //    DCLogger.LogInfo(" IP Address[" + i + "]: " + addr[i].ToString());
        //    //    if (addr[i].AddressFamily == System.Net.NetMQSockets.AddressFamily.InterNetworkV6) { DCLogger.LogInfo("-----!! IpV6 address"); }
        //    //    if (IPAddress.IsLoopback(addr[i])) { DCLogger.LogInfo("-----!! loopback address"); }
        //    //}

        //    List<string> selectedAddress = new List<string>();
        //    for (int i = 0; i < addr.Length; i++)
        //    {
        //        if (addr[i].AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
        //        {
        //            if (!IPAddress.IsLoopback(addr[i]))
        //            {
        //                selectedAddress.Add(addr[i].ToString());
        //                DCLogger.LogInfo(" using: " + addr[i].ToString() + " (index: " + i + ")");
        //                break;
        //            }
        //        }
        //    }

        //    if (selectedAddress.Count == 0) { DCLogger.LogInfo("!!! No useable network addres found !!!"); }

        //    return (selectedAddress);
        //}
    }
}
