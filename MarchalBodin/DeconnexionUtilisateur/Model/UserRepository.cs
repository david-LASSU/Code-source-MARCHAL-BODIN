using DeconnexionUtilisateur.IntermagService;
using MBCore.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Collections;

namespace DeconnexionUtilisateur.Model
{
    class UserRepository : BaseCialAbstract
    {
        public Collection<User> GetUsers()
        {
            IntermagServiceClient client = createClient();
            return new Collection<User>(client.GetUsers(dbName));
        }

        public void Disconnect(IList users)
        {
            IntermagServiceClient client = createClient();

            foreach (User user in users)
            {
                if (user.hostProcessId != "")
                {
                    // Vrai connexion Sage
                    client.killUser(dbName, user.hostname, user.ntUsername, user.hostProcessId, user.cbSession);
                }
                else
                {
                    // Connexion fantôme
                    client.deleteUserSession(dbName, user.cbSession);
                }
            }

            client.Close();
        }

        public void Ghost()
        {
            IntermagServiceClient client = createClient();
            client.deleteUserSession(dbName, null);
        }

        private IntermagServiceClient createClient()
        {
            string server;
#if DEBUG
            server = Environment.MachineName;
            //server = getDb(dbName).server;
#else
            server = getDb(dbName).server;
#endif
            server = $"http://{server}:8001/InterMagService";

            try
            {
                BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
                binding.Name = "BasicHttpBinding_" + Environment.MachineName;
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
                binding.OpenTimeout = new TimeSpan(0, 2, 0);
                binding.CloseTimeout = new TimeSpan(0, 2, 0);
                binding.SendTimeout = new TimeSpan(0, 2, 0);
                binding.ReceiveTimeout = new TimeSpan(0, 2, 0);

                IntermagServiceClient client = new IntermagServiceClient(binding, new EndpointAddress(new Uri(server)));

                return client;
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        public Collection<User> GetUsersSample() => new Collection<User>()
            {
                new User()
                {
                    cbSession = "51",
                    sessionId = "51",
                    hostProcessId = "2616",
                    programName = "Sage 100 Gestion Commerciale",
                    CbType = "CIAL",
                    cbUserName = "COMPTOIR2",
                    hostname = "UC-PC104",
                    ntUsername = "comptoir2"
                }
            };
    }
}
