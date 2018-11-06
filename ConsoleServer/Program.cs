using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Net.NetworkInformation;
using Topshelf;
using System.Net;
using DC;
using System.Management;
using System.IO;
using System.Security.Cryptography;

namespace ConsoleServer
{
    class Program
    {
        static string licFile = AppDomain.CurrentDomain.BaseDirectory + "License.txt";
        static string infoFile = AppDomain.CurrentDomain.BaseDirectory + "ComputerId.txt";

        static string getCpuInfo() //读取CPU信息
        {
            ManagementClass mobj = new ManagementClass("Win32_Processor");
            ManagementObjectCollection moc = mobj.GetInstances();
            List<string> cpuIds = new List<string>();
            foreach (ManagementObject mo in moc)
            {
                cpuIds.Add(mo.Properties["ProcessorId"].Value.ToString()); ;
            }
            return String.Join("|", cpuIds.ToArray());
        }

        static string encrypt(string info)
        {
            HashAlgorithm alg = (HashAlgorithm)SHA256.Create();

            byte[] t = alg.ComputeHash(Encoding.Unicode.GetBytes(info + "8186__"));
            StringBuilder sb = new StringBuilder(32);
            for (int i = 0; i < t.Length; i++)
            {
                sb.Append(t[i].ToString("X").PadLeft(2, '0'));
            }
            return sb.ToString();
        }

        static void Main (string[] args)
		{
            //DCServer server = DCServer.Instance ("global");
            //server.StartWork ();

            //PerformanceCounter pc_cpu = new PerformanceCounter ();
            //pc_cpu.CategoryName = "Process";
            //pc_cpu.CounterName = "% Processor Time";
            //pc_cpu.InstanceName = GetInstanceName ();
            //PerformanceCounter pc_ram = new PerformanceCounter ();
            //pc_ram.CategoryName = "Process";
            //pc_ram.CounterName = "Private Bytes";
            //pc_ram.InstanceName = GetInstanceName ();
            //PerformanceCounter pc_vram = new PerformanceCounter ();
            //pc_vram.CategoryName = "Process";
            //pc_vram.CounterName = "Virtual Bytes";
            //pc_vram.InstanceName = GetInstanceName ();

            //Thread pcThread = new Thread(new ThreadStart(delegate()
            //{
            //    while (true)
            //    {
            //        Thread.Sleep(5000);
            //        try
            //        {
            //            Console.WriteLine(string.Format("CPU:{0:F2}%;内存:{1:F2}/{2:F2}"
            //                , pc_cpu.NextValue()
            //                , pc_ram.NextValue() / (1024 * 1024)
            //                , pc_vram.NextValue() / (1024 * 1024)
            //               ));
            //        }
            //        catch (Exception e)
            //        {
            //            Console.WriteLine(e.Message);
            //        }
            //    }
            //}
            //));
            //pcThread.IsBackground = true;
            //pcThread.Start();

            Host host = HostFactory.New(x =>
            {
                x.Service<DCServer>(s =>
                {

                    s.ConstructUsing(name => DCServer.Instance("global"));
                    s.WhenStarted(tc => tc.StartWork());
                    s.WhenStopped(tc => tc.StopWork());
                });

                x.StartAutomatically();
                x.SetDescription("数据总线服务-t");
                x.SetDisplayName("ConsoleServer-t");
                x.SetServiceName("ConsoleServer-t");
                x.RunAsNetworkService();
            });

            host.Run();
        }

        private static string GetInstanceName()
        {
            // Used Reflector to find the correct formatting:
//            string assemblyName = GetAssemblyName();
//            if ((assemblyName == null) || (assemblyName.Length == 0))
//            {
//                assemblyName = AppDomain.CurrentDomain.FriendlyName;
//            }
//            StringBuilder builder = new StringBuilder(assemblyName);
//            for (int i = 0; i < builder.Length; i++)
//            {
//                switch (builder[i])
//                {
//                    case '/':
//                    case '\\':
//                    case '#':
//                        builder[i] = '_';
//                        break;
//                    case '(':
//                        builder[i] = '[';
//                        break;
//
//                    case ')':
//                        builder[i] = ']';
//                        break;
//                }
//            }
//            string instanceName = builder.ToString();
//            return instanceName;
			var process = Process.GetCurrentProcess();
			var isUnix = Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;
			var instanceName = isUnix ? string.Format("{0}/{1}", process.Id,process.ProcessName) : process.ProcessName;
			return instanceName;
        }

        private static string GetAssemblyName()
        {
            string str = null;
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                AssemblyName name = entryAssembly.GetName();
                if (name != null)
                {
                    str = name.Name;
                }
            }
            return str;
        }

    }
}
