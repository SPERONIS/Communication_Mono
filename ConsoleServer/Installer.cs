using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Configuration.Install;
using System.Diagnostics;
using System.ComponentModel;
using System.ServiceProcess;

namespace ConsoleServer
{
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        private ServiceInstaller _serviceInstaller;

        private ServiceProcessInstaller _serviceProcessInstaller;

        public ProjectInstaller()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this._serviceInstaller = new ServiceInstaller();
            _serviceProcessInstaller = new ServiceProcessInstaller();

            this._serviceProcessInstaller.Account = ServiceAccount.LocalService;

            this._serviceInstaller.DisplayName = "ConsoleServer";
            this._serviceInstaller.ServiceName = "ConsoleServer";

            this.Installers.AddRange(new Installer[] { this._serviceProcessInstaller, this._serviceInstaller });
        }
    }
}
