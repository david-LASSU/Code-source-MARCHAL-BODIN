using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MBCore.Model
{
    public class FournisseurRepository : BaseCialAbstract
    {
        private static readonly Regex refFournRegEx = new Regex("[^ABCDEFGHIJKLMNOPQRSTUVWXYZ$_/+-.0123456789]");

        public IEnumerable<Fournisseur> GetAutoCompleteList()
        {
            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();
                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = "SELECT CT_Num, CT_Intitule FROM F_COMPTET WHERE CT_Type = 1 AND CT_Sommeil = 0";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return new Fournisseur { CtNum = (string)reader[0], Intitule = (string)reader[1] };
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Nettoie la chaine entrée par l'utilisateur
        /// afin de la rendre "Sage compliant"
        /// </summary>
        /// <param name="refFourn"></param>
        /// <returns></returns>
        public static string CleanRefFourn(string refFourn)
        {
            if (refFourn == null)
            {
                return null;
            }

            // todo Erreur de cohérence dans LiaisonDocVente si ref > 18 caractères!
            // int maxLength = 19;
            int maxLength = 18;
            
            if (refFourn.Length > maxLength)
            {
                refFourn = refFourn.Substring(0, maxLength);
            }
            return refFournRegEx.Replace(refFourn.ToUpper(), "-");
        }
    }
}
