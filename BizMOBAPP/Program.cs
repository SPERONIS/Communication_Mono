using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Topshelf;

namespace BizMOBAPP
{
    class Program
    {
        static void Main(string[] args)
        {
            //MobAppSrv s = new MobAppSrv();
            //s.Start();
            string name = string.Format("BizMOBAPP - 移动用户软件云端业务服务 - {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Host host = HostFactory.New(x =>
            {
                x.Service<MobAppSrv>(s =>
                {

                    s.ConstructUsing(sc => new MobAppSrv());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.SetServiceName("BizMOBAPP");
                x.SetDisplayName(name);
                x.SetDescription(name);

                x.StartAutomatically();
                x.RunAsLocalService();
            });

            host.Run();

        }
    }
}
