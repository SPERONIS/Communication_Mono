using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Topshelf;

namespace BizLKDESK
{
    class Program
    {
        static void Main(string[] args)
        {
            //LkDeskSrv s = new LkDeskSrv();
            //s.Start();
            string name = string.Format("BizLKDESK - 冷库综合管理软件云端业务服务 - {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Host host = HostFactory.New(x =>
            {
                x.Service<LkDeskSrv>(s =>
                {

                    s.ConstructUsing(sc => new LkDeskSrv());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.SetServiceName("BizLKDESK");
                x.SetDisplayName(name);
                x.SetDescription(name);

                x.StartAutomatically();
                x.RunAsLocalService();
            });

            host.Run();

        }
    }
}
