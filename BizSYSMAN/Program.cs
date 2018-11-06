using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Topshelf;

namespace BizSYSMAN
{
    class Program
    {
        static void Main(string[] args)
        {
            //MobAppSrv s = new MobAppSrv();
            //s.Start();
            string name = string.Format("BizSYSMAN - 系统管理维护云端业务服务 - {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Host host = HostFactory.New(x =>
            {
                x.Service<SysManSrv>(s =>
                {

                    s.ConstructUsing(sc => new SysManSrv());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.SetServiceName("BizSYSMAN");
                x.SetDisplayName(name);
                x.SetDescription(name);

                x.StartAutomatically();
                x.RunAsLocalService();
            });

            host.Run();

        }
    }
}
