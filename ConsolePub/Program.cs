using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DC;

namespace ConsolePub
{
    class Program
    {
        static void Main(string[] args)
        {
            Random r = new Random();
            int max = 0;
            Thread t1 = new Thread(() =>
            {
                DCClient client = DCClient.Instance("TESTPUB", "global");
                while (true)
                {
                    client.Pub("0x0309", make0EEE());
                    Console.WriteLine("--0x0309--" + DateTime.Now.Second + "." + DateTime.Now.Millisecond);
                    Thread.Sleep(10000);
                }
            }
            );
            t1.Start();

            //Thread t2 = new Thread(() =>
            //{
            //    DCClient client = DCClient.Instance("TESTPUB", "global");
            //    while (true)
            //    {
            //        List<string> list = new List<string>();
            //        Signal sig = new Signal { Freq = (UInt64)r.Next(), Bw = (UInt32)r.Next(), TimeMark = DateTime.Now, Halo = "最无需思考的方法就是", List = new List<string>(new string[] { "asdfte", "satewr", "阿布才" }) };
            //        client.Pub<Signal>(sig);
            //        Console.WriteLine("--Signal--" + DateTime.Now.Second + "." + DateTime.Now.Millisecond);
            //        Thread.Sleep(2000);
            //    }
            //}
            //);
            //t2.Start();

            //Thread t3 = new Thread(() =>
            //{
            //    DCClient client = DCClient.Instance("TESTPUB", "global");
            //    while (true)
            //    {
            //        List<string> list = new List<string>();
            //        Signal sig = new Signal { Freq = (UInt64)r.Next(), Bw = (UInt32)r.Next(), TimeMark = DateTime.Now, Halo = "最无需思考的方法就是", List = new List<string>(new string[] { "asdfte", "satewr", "阿布才" }) };
            //        client.Pub("testObj", sig);
            //        Console.WriteLine("--Signal--" + DateTime.Now.Second + "." + DateTime.Now.Millisecond);
            //        //Thread.Sleep(10);
            //    }
            //}
            //);
            //t3.Start();

            //Thread t2 = new Thread(() =>
            //{
            //    DCClient client = DCClient.Instance("TESTPUB", "global");
            //    while (true)
            //    {
            //        client.Pub("0x0304", make0EEE());
            //        Console.WriteLine("--0x0304--" + DateTime.Now.Second + "." + DateTime.Now.Millisecond);
            //        //Thread.Sleep(1);
            //    }
            //}
            //);
            //t2.Start();

            //Thread t3 = new Thread(() =>
            //{
            //    DCClient client = DCClient.Instance("Service1", "global");
            //    while (true)
            //    {
            //        client.Pub("20002", Encoding.Unicode.GetBytes("Status Service1, timestamp " + DateTime.Now.Second + "." + DateTime.Now.Millisecond));
            //        Thread.Sleep(r.Next(10, 1000));
            //    }
            //}
            //);
            //t3.Start();

            //Thread t4 = new Thread(() =>
            //{
            //    DCClient client = DCClient.Instance("Service2", "global");
            //    while (true)
            //    {
            //        client.Pub("20002", Encoding.Unicode.GetBytes("Status Service2, timestamp " + DateTime.Now.Second + "." + DateTime.Now.Millisecond));
            //        Thread.Sleep(r.Next(10, 1000));
            //    }
            //}
            //);
            //t4.Start();

            //Thread t5 = new Thread(() =>
            //{
            //    DCClient client = DCClient.Instance("Service3", "global");
            //    while (true)
            //    {
            //        client.Pub("20002", Encoding.Unicode.GetBytes("Status Service3, timestamp " + DateTime.Now.Second + "." + DateTime.Now.Millisecond));
            //        Thread.Sleep(r.Next(10, 1000));
            //    }
            //}
            //);
            //t5.Start();

            Console.ReadLine();
        }

        static byte[] freqPack = new byte[320];
        static byte[] makeFreqPack()
        {
            return freqPack;
        }

        static byte[] ePack = Encoding.Default.GetBytes("Hello 你好");
        static byte[] make0EEE()
        {
            return ePack;
        }
    }
}
