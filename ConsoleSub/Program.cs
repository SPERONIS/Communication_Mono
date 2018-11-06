using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DC;
using System.Diagnostics;
using System.Threading;

namespace ConsoleSub
{
    class Program
    {
        static void Main(string[] args)
        {
            DCClient client = DCClient.Instance("TESTSUB", "global");
            //client.Sub("Status", "Service2", Program.display);
            client.Sub("0x0309", Program.display0309);
            //client.Sub("0x0304", Program.display0304);
            //client.Sub("0x0407", "001001RTMS00000001", Program.display0407);
            //client.Sub("20001", "001001DDCS00000001", Program.display);
            //client.Sub("20001", "001001WBFS00000001", Program.display);
            //client.Sub("20002", Program.display);
            //client.Sub<Signal>(displaySignal);
            //client.Sub("testObj", typeof(Signal2), displaySignal);
        }


        private static void display0309(byte[] data)
        {
            Console.WriteLine(Encoding.Default.GetString(data));
        }

        private static void display0304(byte[] data)
        {
            Console.WriteLine(data.Length + "--0x0304--" + DateTime.Now.Second + "." + DateTime.Now.Millisecond);
        }

        private static void display0407(byte[] data)
        {
            Console.WriteLine(data.Length + "--0x0407--" + DateTime.Now.Second + "." + DateTime.Now.Millisecond);
        }

        private static void display(byte[] data)
        {
            Console.WriteLine(Encoding.Default.GetString(data));
        }
    }
}
