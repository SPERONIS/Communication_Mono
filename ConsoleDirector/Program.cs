using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Topshelf;

namespace ConsoleDirector
{
    class Program
    {
        static void Main(string[] args)
        {
            Host host = HostFactory.New(x =>
            {
                x.Service<DirectorService>(s =>
                {

                    s.ConstructUsing(name => new DirectorService());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.StartAutomatically();
                x.SetDescription("数据总线调谐服务-t");
                x.SetDisplayName("ConsoleDirector-t");
                x.SetServiceName("ConsoleDirector-t");
                x.RunAsNetworkService();
            });

            host.Run();
        }
    }
}
