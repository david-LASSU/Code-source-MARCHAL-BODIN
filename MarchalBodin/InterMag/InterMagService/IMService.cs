using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceProcess;
using InterMagService.Model;
using WcfServiceLibrary;
using System.Timers;

namespace InterMagService
{
    partial class IMService : ServiceBase
    {
        private Timer _timer;

        private Repository _contremarqueRepository = new Repository();

        public ServiceHost host = null;

        public IMService()
        {
            ServiceName = "InterMag";
        }

        protected override void OnStart(string[] args)
        {
            if (host != null)
            {
                host.Close();
            }

            // TODO netsh http add urlacl url=http://+:8001/InterMagService user=DOMAIN\user
            Uri baseAddress = new Uri("http://" + Environment.MachineName + ":8001/InterMagService");

            host = new ServiceHost(typeof(IntermagService), baseAddress);
            try
            {
                // WcfService
                BasicHttpBinding binding = new BasicHttpBinding();
                binding.Name = "binding1";
                binding.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
                binding.Security.Mode = BasicHttpSecurityMode.None;
                binding.OpenTimeout = new TimeSpan(0, 5, 0);
                binding.CloseTimeout = new TimeSpan(0, 5, 0);
                binding.SendTimeout = new TimeSpan(0, 5, 0);
                binding.ReceiveTimeout = new TimeSpan(0, 5, 0);
                binding.MaxReceivedMessageSize = 10485760;

                host.AddServiceEndpoint(typeof(IIntermagService), binding, baseAddress);

                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;

                host.Description.Behaviors.Add(smb);
                host.Open();

                // Crontab 60 secondes pour éviter les colisions lors des connections à Sage
                _timer = new Timer(60000);
                _timer.Elapsed += UpdCmLienOnTick;
                _timer.Enabled = true;

                EventLog.WriteEntry("InterMag service started", EventLogEntryType.Information);
            }
            catch (Exception e)
            {
                EventLog.WriteEntry($"InterMag service start error {e.Message}", EventLogEntryType.Error);
                host.Abort();
                Stop();
            }
        }

        private void UpdCmLienOnTick(object sender, EventArgs eventArgs)
        {
            //EventLog.WriteEntry("CM Tick Started", EventLogEntryType.Information);
            _contremarqueRepository.UpdateDocuments();
            //_contremarqueRepository.UpdateApcReference();
            //EventLog.WriteEntry("CM Tick finished", EventLogEntryType.Information);
        }

        protected override void OnStop()
        {
            if (host != null)
            {
                host.Close();
                host = null;
            }

            EventLog.WriteEntry("InterMag service stopped", EventLogEntryType.Information);
        }
    }
}
