using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBCore.Model
{
    public class UniteRepository : BaseCialAbstract
    {
        public IEnumerable<Unite> GetAll()
        {
            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();
                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = "SELECT U_Intitule, cbMarq FROM P_UNITE";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return new Unite() {
                                Intitule = (string)reader["U_Intitule"],
                                CbMarq = (int)reader["cbMarq"]
                            };
                        }
                    }
                }
            }
        }
    }
}
