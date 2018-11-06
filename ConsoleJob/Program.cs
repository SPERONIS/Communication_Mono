using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Topshelf;

namespace ConsoleJob
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
                x.SetDescription("计划任务调度服务");
                x.SetDisplayName("ConsoleJob");
                x.SetServiceName("ConsoleJob");
                x.RunAsNetworkService();
            });

            host.Run();
        }
    }
}
