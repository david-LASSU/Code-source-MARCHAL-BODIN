using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MBCore.Model;
using Objets100cLib;
using System.Data.SqlClient;
using System.ServiceModel;
using VerrouillageDoc.IntermagService;

namespace VerrouillageDoc
{
    public class LockRepository : BaseCialAbstract
    {
        public bool isVerrou(string doPiece)
        {
            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();

                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = "SELECT VERROU FROM F_DOCENTETE WHERE DO_PIECE = @doPiece";
                    cmd.Parameters.AddWithValue("@doPiece", doPiece);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            throw new Exception("Document non trouvé");
                        }
                        reader.Read();
                        return reader["VERROU"].ToString() == "Oui";
                    }
                }
            }
        }

        internal string Toggle(string doPiece)
        {
            return createClient(dbName).ToggleVerrou(dbName, doPiece);
        }

        /// <summary>
        /// Crée un client InterMagService
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        private IntermagServiceClient createClient(string dbName)
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

                IntermagServiceClient client = new IntermagServiceClient(binding, new EndpointAddress(new Uri(server)));

                return client;
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        public bool DocumentIsClosed(string doPiece)
        {
            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();
                using (SqlCommand cmd =cnx.CreateCommand())
                {
                    cmd.CommandText = @"SELECT * FROM LockLogging WHERE CbFile = 'F_DOCENTETE'
                                      AND CbMarq = (SELECT cbMarq FROM F_DOCENTETE WHERE DO_Piece = @doPiece)";
                    cmd.Parameters.AddWithValue("@doPiece", doPiece);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            throw new Exception($"Veuillez fermer le document {doPiece}");
                        }
                    }
                }
            }

            return true;
        }
    }
}
