using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ListeCommandes
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //@TODO Ne fonctionne pas sur les serveurs car plusieurs users connectés
            //Process proc = Process.GetCurrentProcess();
            //int count = Process.GetProcesses().Count(p => p.ProcessName == proc.ProcessName);
            //if (count <= 1) return;

            //MessageBox.Show("Le programme est déjà en cours d'exécution...");
            //App.Current.Shutdown();
        }
    }
}
