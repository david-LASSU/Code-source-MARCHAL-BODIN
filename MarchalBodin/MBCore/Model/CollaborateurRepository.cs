using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace MBCore.Model
{
    public class CollaborateurRepository : BaseCialAbstract
    {
        public Collection<Collaborateur> getAll()
        {
            Collection<Collaborateur> list = new Collection<Collaborateur>();

            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();

                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = "SELECT CO_No, CO_Nom, CO_Prenom FROM F_Collaborateur";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Collaborateur()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Co_No")),
                                Nom = reader["CO_Nom"].ToString(),
                                Prenom = reader["CO_Prenom"].ToString(),
                            });
                        }
                    }
                }
            }

            return list;
        }
    }
}
