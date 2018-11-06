using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Topshelf;

namespace BizFileStore
{
    class Program
    {
        static void Main(string[] args)
        {
            string name = string.Format("BizFileStore - 文件存储访问服务 - {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Host host = HostFactory.New(x =>
            {
                x.Service<FileStoreSrv>(s =>
                {

                    s.ConstructUsing(sc => new FileStoreSrv());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.SetServiceName("BizFileStore");
                x.SetDisplayName(name);
                x.SetDescription(name);
                
                x.StartAutomatically();
                x.RunAsLocalService();
            });

            host.Run();

        }
    }
}
