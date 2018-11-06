using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZMQ;
using System.Threading;
using Topshelf.Configuration.Dsl;
using Topshelf.Shelving;

namespace ConsoleServer
{
    public class RRServer_Shelved : Bootstrapper<RRServer>
    {
        public void InitializeHostedService(IServiceConfigurator<RRServer> cfg)
        {
            cfg.HowToBuildService(s => new RRServer());
            cfg.WhenStarted(s => s.Start());
            cfg.WhenStopped(s => s.Stop());
        }
    }
}
