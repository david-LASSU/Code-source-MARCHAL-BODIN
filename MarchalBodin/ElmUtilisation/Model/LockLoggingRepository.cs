using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using MBCore.Model;

namespace ElmUtilisation.Model
{
    class LockLoggingRepository : BaseCialAbstract
    {
        internal Collection<LockLog> GetAll()
        {
            Collection<LockLog> list = new Collection<LockLog>();

            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();

                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = @"SELECT *,
                                        CASE
                                            WHEN CbFile = 'F_ARTICLE' THEN
                                                (SELECT AR_Ref FROM F_ARTICLE A WHERE A.cbMarq = L.CbMarq)
                                            WHEN CbFile = 'F_COMPTET' THEN
                                                (SELECT CT_Num FROM F_COMPTET CT WHERE CT.cbMarq = L.CbMarq)
                                            WHEN CbFile = 'F_DOCENTETE' THEN
                                                (SELECT DO_Piece FROM F_DOCENTETE DO WHERE DO.cbMarq = L.CbMarq)
                                            WHEN CbFile = 'F_CAISSE' THEN
                                                (SELECT CA_Intitule FROM F_CAISSE CA WHERE CA.CbMarq = L.CbMarq)
                                            WHEN CbFile = 'F_FAMILLE' THEN
                                                (SELECT FA_CodeFamille FROM F_FAMILLE FA WHERE FA.CbMarq = L.CbMarq)
                                            WHEN CbFile = 'F_COMPTEG' THEN
                                                (SELECT CG_Num FROM F_COMPTEG CG WHERE CG.CbMarq = L.CbMarq)
                                            WHEN CbFile = 'F_JMOUV' THEN
                                                (SELECT JO_Num FROM F_JMOUV JO WHERE JO.CbMarq = L.CbMarq)
                                            WHEN CbFile = 'F_CONTACTT' THEN
                                                (SELECT CONCAT(CT_Prenom, ' ', CT_Nom) FROM F_CONTACTT CTT WHERE CTT.CbMarq = L.CbMarq)
                                            ELSE
                                                '???'
                                        END AS REF
                                        FROM LockLogging L
                                        ORDER BY CbFile;";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            LockLog log = new LockLog()
                            {
                                Callid = (int) reader["CallId"],
                                CallTime = (DateTime) reader["CallTime"],
                                CallingUser = ((string) reader["CallingUser"]).Split('\\')[1],
                                CallingMachine = (string) reader["CallingMachine"],
                                CbFile = (string) reader["CbFile"],
                                CbMarq = (int) reader["CbMarq"],
                                Ref = reader["REF"] == DBNull.Value ? "" : (string)reader["REF"]
                            };

                            list.Add(log);
                        }
                    }
                }
            }

            return list;
        }
    }
}
