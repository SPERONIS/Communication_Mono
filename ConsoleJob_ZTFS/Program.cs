using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Topshelf;

namespace ConsoleJob_ZTFS
{
    class Program
    {
        static void Main(string[] args)
        {
            Host host = HostFactory.New(x =>
            {
                x.Service<JobService>(s =>
                {

                    s.ConstructUsing(name => new JobService());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.StartAutomatically();
                x.SetDescription("计划任务调度服务_子途文件服务");
                x.SetDisplayName("ConsoleJob_ZTFS");
                x.SetServiceName("ConsoleJob_ZTFS");
                x.RunAsNetworkService();
            });

            host.Run();
        }
    }
}
