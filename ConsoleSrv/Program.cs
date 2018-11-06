using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DC;
using System.Threading;

namespace ConsoleSrv
{
    class Program
    {
        static Random r = new Random();
        static void Main(string[] args)
        {
            DCClient client = DCClient.Instance("TESTSRV", "global");
            client.Srv("Add", Program.add);
        }

        //static byte[] retBytes = new byte[10000000];
        private static byte[] add(string extInfo, byte[] data)
        {
            string recv = Encoding.Unicode.GetString(data);
            Console.WriteLine("Received from {0}: {1}", extInfo, recv);
            //Thread.Sleep(r.Next(1000,3000));
            string[] para = recv.Split('+');
            if (int.Parse(para[0]) > 500)
                throw new Exception("Number Large Than 500.");
            return Encoding.Unicode.GetBytes(string.Format("{0} + {1} = {2}\t{3}", para[0], para[1], int.Parse(para[0]) + int.Parse(para[1]), DateTime.Now));
        }
    }
}
