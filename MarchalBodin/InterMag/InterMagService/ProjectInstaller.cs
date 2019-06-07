using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;

namespace InterMagService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        private ServiceProcessInstaller spi;
        private ServiceInstaller si;

        public ProjectInstaller()
        {
            InitializeComponent();

            spi = new ServiceProcessInstaller();
            //spi.Account = ServiceAccount.LocalSystem;
            spi.Account = ServiceAccount.User;
            spi.Username = null;
            spi.Password = null;

            si = new ServiceInstaller();
            si.ServiceName = "InterMagService";
            si.StartType = ServiceStartMode.Automatic;
            
            Installers.Add(spi);
            Installers.Add(si);

            this.AfterInstall += ProjectInstaller_AfterInstall;
        }

        private void ProjectInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            new ServiceController(si.ServiceName).Start();
        }
    }
}
