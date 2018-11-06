using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Topshelf;

namespace BizXSDESK
{
    class Program
    {
        static void Main(string[] args)
        {
            //XsDeskSrv s = new XsDeskSrv();
            //s.Start();
            string name = string.Format("BizXSDESK - 销售综合管理软件云端业务服务 - {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Host host = HostFactory.New(x =>
            {
                x.Service<XsDeskSrv>(s =>
                {

                    s.ConstructUsing(sc => new XsDeskSrv());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.SetServiceName("BizXSDESK");
                x.SetDisplayName(name);
                x.SetDescription(name);

                x.StartAutomatically();
                x.RunAsLocalService();
            });

            host.Run();

        }
    }
}
