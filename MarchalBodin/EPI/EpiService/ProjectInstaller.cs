using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;

namespace EpiService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        private void EpiService_AfterInstall(object sender, InstallEventArgs e)
        {
            ServiceController sc = new ServiceController(EpiService.ServiceName);
            if (sc.Status != ServiceControllerStatus.Stopped)
            {
                sc.Stop();
            }

            sc.WaitForStatus(ServiceControllerStatus.Stopped);
            sc.Start();
        }

        private void EpiService_BeforeUninstall(object sender, InstallEventArgs e)
        {
            ServiceController sc = new ServiceController(EpiService.ServiceName);
            if (sc.Status != ServiceControllerStatus.Stopped)
            {
                sc.Stop();
            }
            
            sc.WaitForStatus(ServiceControllerStatus.Stopped);
        }
    }
}
