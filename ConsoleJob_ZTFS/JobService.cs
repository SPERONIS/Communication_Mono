using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using Quartz;
using Quartz.Impl;
using Nini.Config;
using System.Data;
using RestSharp;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ConsoleJob_ZTFS
{
    public class JobService
    {
        IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();

        public void Start()
        {
            try
            {
                scheduler.Start();
                DC.DCLogger.LogInfo("Job Scheduler started...");

                IJobDetail ServerStatusReport = JobBuilder.Create<ServerStatusReport>()
                    .WithIdentity("ServerStatusReport", "group1")
                    .Build();


                ITrigger trigger1 = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow()
                    .WithCronSchedule("0 0/15 * * * ?")
                    .Build();


                scheduler.ScheduleJob(ServerStatusReport, trigger1);

            }
            catch (SchedulerException se)
            {
                DC.DCLogger.LogError("Job Scheduler started failed.{0}", se);
            }
        }

        public void Stop()
        {
            try
            {
                scheduler.Shutdown();
                DC.DCLogger.LogInfo("Job Scheduler stopped...");
            }
            catch (SchedulerException se)
            {
                DC.DCLogger.LogError("Job Scheduler stopped failed.{0}", se);
            }
        }
    }

    public class TestJob1 : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("TestJob1:{0}", DateTime.Now);
        }
    }

    public class ServerStatusReport : IJob
    {
        public class RequestData
        {
            public string ip { get; set; }
            public int port { get; set; }
        }

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                var client = new RestClient("http://121.42.56.32:8080");
                var request = new RestRequest("/filestor/serverstatus/report/v1", Method.POST);
                request.AddJsonBody(new RequestData { ip = GetLocalIP(), port = 8000 });

                // easy async support
                client.ExecuteAsync(request, response =>
                {
                    Console.WriteLine(response.Content);
                });

                DC.DCLogger.LogTrace("Job successed [ServerStatusReport]");
            }
            catch (Exception ex)
            {
                DC.DCLogger.LogError("Job failed [ServerStatusReport]:{0}", ex);
            }
        }

        /// <summary>
        /// 获取当前使用的IP
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIP()
        {
            string result = RunApp("route", "print", true);
            Match m = Regex.Match(result, @"0.0.0.0\s+0.0.0.0\s+(\d+.\d+.\d+.\d+)\s+(\d+.\d+.\d+.\d+)");
            if (m.Success)
            {
                return m.Groups[2].Value;
            }
            else
            {
                try
                {
                    System.Net.Sockets.TcpClient c = new System.Net.Sockets.TcpClient();
                    c.Connect("www.baidu.com", 80);
                    string ip = ((System.Net.IPEndPoint)c.Client.LocalEndPoint).Address.ToString();
                    c.Close();
                    return ip;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 获取本机主DNS
        /// </summary>
        /// <returns></returns>
        public static string GetPrimaryDNS()
        {
            string result = RunApp("nslookup", "", true);
            Match m = Regex.Match(result, @"\d+\.\d+\.\d+\.\d+");
            if (m.Success)
            {
                return m.Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 运行一个控制台程序并返回其输出参数。
        /// </summary>
        /// <param name="filename">程序名</param>
        /// <param name="arguments">输入参数</param>
        /// <returns></returns>
        public static string RunApp(string filename, string arguments, bool recordLog)
        {
            try
            {
                if (recordLog)
                {
                    Trace.WriteLine(filename + " " + arguments);
                }
                Process proc = new Process();
                proc.StartInfo.FileName = filename;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.Arguments = arguments;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.Start();

                using (System.IO.StreamReader sr = new System.IO.StreamReader(proc.StandardOutput.BaseStream, Encoding.Default))
                {
                    string txt = sr.ReadToEnd();
                    sr.Close();
                    if (recordLog)
                    {
                        Trace.WriteLine(txt);
                    }
                    if (!proc.HasExited)
                    {
                        proc.Kill();
                    }
                    return txt;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return ex.Message;
            }
        }
    }
}