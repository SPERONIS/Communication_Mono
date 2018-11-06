using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Topshelf;

namespace BizSampleSrv
{
    class Program
    {
        static void Main(string[] args)
        {
            Host host = HostFactory.New(x =>
            {
                x.Service<SampleSrv>(s =>
                {

                    s.ConstructUsing(name => new SampleSrv());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.StartAutomatically();
                x.SetDescription("业务服务例子");
                x.SetDisplayName("BizSampleSrv");
                x.SetServiceName("BizSampleSrv");
                x.RunAsLocalService();
            });

            host.Run();
        }
    }
}
