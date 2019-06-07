using System;
using MBCore.Model;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace InterMagService.Model
{
    /// <summary>
    /// </summary>
    public class Repository : BaseCialAbstract
    {

        private string _machineName = Environment.MachineName.ToUpper();

        public Repository()
        {
#if DEBUG
            _machineName = "SRVSQL04";
#endif
        }

        /// <summary>
        /// Met à jour des champs de document en fonction de leur évolution
        /// @todo Utiliser les transactions
        /// </summary>
        public void UpdateDocuments()
        {
            try
            {
                foreach (Database db in dbList)
                {
#if DEBUG
                    if (db.name != "SMODEV")
                    {
                        continue;
                    }
#endif
                    if (string.Equals(_machineName, db.server.Split('.').First()))
                    {
                        using (var cnx = new SqlConnection(db.cnxString))
                        {
                            cnx.Open();
                            // Vide les Numpiece qui n'ont plus lieu d'être depuis le 26-12-16
                            using (var cmd = cnx.CreateCommand())
                            {
                                cmd.CommandText = @"SELECT cbMarq FROM F_DOCLIGNE DL
                                                    WHERE ISNULL(NumPiece, '') <> '' 
                                                    AND DO_Date > convert(varchar, '26-12-2016 00:00:00', 120)
                                                    AND (SELECT COUNT(*) FROM F_CMLIEN WHERE DL_NoIn = DL.DL_No OR DL_NoOut = DL.DL_No) = 0";
                                using (var reader = cmd.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        var dt = new DataTable();
                                        dt.Load(reader);
                                        foreach (DataRow row in dt.Rows)
                                        {
                                            try
                                            {
                                                using (var cmd2 = cnx.CreateCommand())
                                                {
                                                    cmd2.CommandText = "UPDATE F_DOCLIGNE SET NumPiece = '' WHERE cbMarq = @cbMarq";
                                                    cmd2.Parameters.AddWithValue("@cbMarq", row["cbMarq"]);
                                                    cmd2.ExecuteNonQuery();
                                                }
                                            }
                                            catch (Exception)
                                            {
                                                // Elément en cours d'utilisation
                                                //EventLog.WriteEntry("Application", "ERR1: " + e.ToString(), EventLogEntryType.Warning);
                                            }
                                        }
                                    }
                                }
                            }

                            // Met à jour les NumPiece 
                            using (var cmd = cnx.CreateCommand())
                            {
                                cmd.CommandText = @"SELECT DLA.DL_No AS DLA_NO, DLV.DL_No AS DLV_NO
                                                    FROM F_CMLIEN CM
                                                    JOIN F_DOCLIGNE DLV ON DLV.DL_No = CM.DL_NoOut
                                                    JOIN F_DOCLIGNE DLA ON DLA.DL_No = CM.DL_NoIn
                                                    WHERE (DLA.DO_Piece <> ISNULL(DLV.NumPiece, '')) OR (DLV.DO_Piece <> ISNULL(DLA.NumPiece, ''))";

                                using (var reader = cmd.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        // Buffer result
                                        var dt = new DataTable();
                                        dt.Load(reader);
                                        foreach (DataRow row in dt.Rows)
                                        {
                                            try
                                            {
                                                using (var cmd2 = cnx.CreateCommand())
                                                {
                                                    cmd2.CommandText = "UPDATE F_DOCLIGNE SET NumPiece = dbo.GET_CM_PIECE(DL_No) WHERE DL_No IN(@dlaNo, @dlvNo)";
                                                    cmd2.Parameters.AddWithValue("@dlaNo", row["DLA_NO"]);
                                                    cmd2.Parameters.AddWithValue("@dlvNo", row["DLV_NO"]);
                                                    cmd2.ExecuteNonQuery();
                                                }
                                            }
                                            catch (Exception)
                                            {
                                                // Elément en cours d'utilisation
                                                //EventLog.WriteEntry("Application", "ERR2: "+e.ToString(), EventLogEntryType.Warning);
                                            }
                                        }
                                    }
                                }
                            }

                            // Met à jour les APC Reference
                            using (var cmd = cnx.CreateCommand())
                            {
                                cmd.CommandText = @"SELECT DISTINCT DE.DO_Piece, DE.DO_Ref, DE.DO_Type, DE.DO_Statut, DO_Date, DV.DO_Piece AS DVPiece, DV.DO_Statut AS DVStatut, DV.DO_Type AS DVType
                                                    FROM F_DOCENTETE DE
                                                    LEFT JOIN (SELECT DISTINCT DLA.DO_Piece AS APC, DOV.DO_Piece, DOV.DO_Statut, DOV.DO_Type 
                                                        FROM F_DOCLIGNE DLA 
                                                        JOIN F_CMLIEN CM ON CM.DL_NoIn = DLA.DL_No
                                                        JOIN F_DOCLIGNE DLV ON DLV.DL_No = CM.DL_NoOut
                                                        JOIN F_DOCENTETE DOV ON DOV.DO_Piece = DLV.DO_Piece) AS DV ON DV.APC = DE.DO_Piece
                                                    WHERE DE.DO_Type = 11 AND DO_Date > convert(varchar, '26-12-2016 00:00:00', 120)
                                                    ORDER BY DE.DO_Piece DESC";
                                using (var reader = cmd.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        var dt = new DataTable();
                                        dt.Load(reader);
                                        foreach (DataRow row in dt.Rows)
                                        {
                                            try
                                            {
                                                string docAPiece = row["DO_Piece"].ToString();
                                                string docARef = row["DO_Ref"].ToString();
                                                short docAType = (short)row["DO_Type"];
                                                short docAStatut = (short)row["DO_Statut"];

                                                string docVPiece = row["DVPiece"] == DBNull.Value ? null : row["DVPiece"].ToString();
                                                short? docVStatut = row["DVStatut"] == DBNull.Value ? null : (short?)row["DVStatut"];
                                                short? docVType = row["DVType"] == DBNull.Value ? null : (short?)row["DVType"];

                                                Debug.Print($"{docAPiece} - {docARef} - {docAType} - {docAStatut} - {docVPiece} - {docVStatut} - {docVType}");
                                                // APC sans CM statut < Accepté = DMD PRIX STOCK
                                                if (docVType == null && docAStatut < 2 && docARef != "DMD PRIX STOCK")
                                                {
                                                    using (var cmd2 = cnx.CreateCommand())
                                                    {
                                                        cmd2.CommandText = "UPDATE F_DOCENTETE SET DO_Ref = 'DMD PRIX STOCK' WHERE DO_Piece = @doPiece";
                                                        cmd2.Parameters.AddWithValue("@doPiece", docAPiece);
                                                        cmd2.ExecuteNonQuery();
                                                    }
                                                }
                                                // APC sans CM statut Accepté = CDE STOCK
                                                else if (docVType == null && docAStatut == 2 && docARef != "STOCK")
                                                {
                                                    using (var cmd2 = cnx.CreateCommand())
                                                    {
                                                        cmd2.CommandText = "UPDATE F_DOCENTETE SET DO_Ref = 'STOCK' WHERE DO_Piece = @doPiece";
                                                        cmd2.Parameters.AddWithValue("@doPiece", docAPiece);
                                                        cmd2.ExecuteNonQuery();
                                                    }
                                                }
                                                // APC avec CM sur devis statut < Accepté
                                                else if (docVType == 0 && docAStatut < 2 && docARef != "DMD PRIX CLIENT")
                                                {
                                                    using (var cmd2 = cnx.CreateCommand())
                                                    {
                                                        cmd2.CommandText = "UPDATE F_DOCENTETE SET DO_Ref = 'DMD PRIX CLIENT' WHERE DO_Piece = @doPiece";
                                                        cmd2.Parameters.AddWithValue("@doPiece", docAPiece);
                                                        cmd2.ExecuteNonQuery();
                                                    }
                                                }
                                                // APC avec CM sur type > devis ou APC avec CM sur devis et statut Accepté
                                                else if ((docVType > 0 || (docVType == 0 && docAStatut == 2)) && docARef != "RESERVATION")
                                                {
                                                    using (var cmd2 = cnx.CreateCommand())
                                                    {
                                                        cmd2.CommandText = "UPDATE F_DOCENTETE SET DO_Statut = 2, DO_Ref = 'RESERVATION' WHERE DO_Piece = @doPiece";
                                                        cmd2.Parameters.AddWithValue("@doPiece", docAPiece);
                                                        cmd2.ExecuteNonQuery();
                                                    }
                                                }

                                                // Si VDE non accepté mais APC accepté on update le VDE
                                                if (docVType == 0 && docVStatut < 2 && docAStatut == 2)
                                                {
                                                    using (var cmd2 = cnx.CreateCommand())
                                                    {
                                                        cmd2.CommandText = "UPDATE F_DOCENTETE SET DO_Statut = 2 WHERE DO_Piece = @doPiece";
                                                        cmd2.Parameters.AddWithValue("@doPiece", docVPiece);
                                                        cmd2.ExecuteNonQuery();
                                                    }
                                                }
                                            }
                                            catch (Exception)
                                            {
                                                // Elément en cours d'utilisation
                                                //EventLog.WriteEntry("Application", "ERR1: " + e.ToString(), EventLogEntryType.Warning);
                                            }
                                        }
                                    }
                                }
                            }

                            // Vide les APC Reference
                            using (var cmd = cnx.CreateCommand())
                            {
                                cmd.CommandText = @"SELECT DO_Piece
                                                    FROM F_DOCENTETE
                                                    WHERE DO_Type > 11 AND DO_Type < 17 AND DO_Ref IN ('DMD PRIX STOCK', 'DMD PRIX CLIENT', 'STOCK', 'RESERVATION')";
                                using (var reader = cmd.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        var dt = new DataTable();
                                        dt.Load(reader);
                                        foreach (DataRow row in dt.Rows)
                                        {
                                            try
                                            {
                                                using (var cmd2 = cnx.CreateCommand())
                                                {
                                                    cmd2.CommandText = "UPDATE F_DOCENTETE SET DO_Ref = '' WHERE DO_Piece = @doPiece";
                                                    cmd2.Parameters.AddWithValue("@doPiece", row["DO_Piece"].ToString());
                                                    cmd2.ExecuteNonQuery();
                                                }
                                            }
                                            catch (Exception)
                                            {
                                                // Elément en cours d'utilisation
                                                //EventLog.WriteEntry("Application", "ERR1: " + e.ToString(), EventLogEntryType.Warning);
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("Application", e.ToString(), EventLogEntryType.Error);
            }
        }
    }
}
