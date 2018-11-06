using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Topshelf;
using Nini.Config;

namespace ContainerBiz
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfigSource cs = new IniConfigSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ContainerBiz.ini"));
            IConfig c = cs.Configs["config"];
            
            string description = string.Format("ContainerBiz - {0} - {1}", 
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(), 
                c.Get("ServicName"));

            string serviceName = c.Get("ServiceId");
            
            Host host = HostFactory.New(x =>
            {
                x.Service<ContainerBizSrv>(s =>
                {

                    s.ConstructUsing(sc => new ContainerBizSrv());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.SetServiceName(serviceName);
                x.SetDisplayName(description);
                x.SetDescription(description);

                x.StartAutomatically();
                x.RunAsNetworkService();
            });

            host.Run();

        }
    }
}
