using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DC;
using System.Threading;
using System.IO;

namespace ConsoleAct
{
    class Program
    {
        static void Main(string[] args)
        {
            DCClient client = DCClient.Instance("TESTACT", "global");
            Random r = new Random();
            while (true)
            {
                //string read = Console.ReadLine();
                Thread.Sleep(r.Next(1000));
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        string requet = string.Format("{0}+{1}", r.Next(10, 1000), r.Next(100, 1000));
                        ActResult ar = client.Act("Add?TESTACT", "TESTSRV", Encoding.Unicode.GetBytes(requet), 2);
                        Console.WriteLine("{0}\t{1}", Encoding.Unicode.GetString(ar.ResultData), DateTime.Now);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }

        static void Hnd(ActResult ar)
        {
            if (ar.Exception != null)
            {
                Console.WriteLine(ar.Exception);
            }
            else
            {
                Console.WriteLine("{0}\t{1}", Encoding.Unicode.GetString(ar.ResultData), DateTime.Now);
                //Console.WriteLine("{0}\t{1}", BitConverter.ToInt32(ar.ResultData, 0), DateTime.Now);
            }
        }
    }
}
