using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using WcfServiceLibrary;

namespace WcfServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO netsh http add urlacl url=http://+:8001/InterMagService user=DOMAIN\user
            Uri baseAddress = new Uri("http://" + Environment.MachineName + ":8001/InterMagService");
            using (ServiceHost host = new ServiceHost(typeof(IntermagService), baseAddress))
            {
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

                Console.WriteLine("The service is ready at {0}", baseAddress);
                Console.WriteLine("Press <Enter> to stop the service.");
                Console.ReadLine();

                // Close the ServiceHost.
                host.Close();
            }
        }
    }
}
