using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Topshelf;

namespace ConnectorRest
{
    class Program
    {
        static void Main(string[] args)
        {
            Host host = HostFactory.New(x =>
            {
                x.Service<ConnRestService>(s =>
                {

                    s.ConstructUsing(name => new ConnRestService());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.StartAutomatically();
                x.SetDescription("Rest访问接入器");
                x.SetDisplayName("ConnectorRest");
                x.SetServiceName("ConnectorRest");
                x.RunAsLocalSystem();   //必须使用本地系统帐号安装运行，否则无法成功Start Listner。
            });

            host.Run();
        }
    }
}
