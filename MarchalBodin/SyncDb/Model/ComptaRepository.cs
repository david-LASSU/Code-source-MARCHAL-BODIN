using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MBCore.Model;
using System.Data;
using System.Data.SqlClient;

namespace SyncDb.Model
{
    class ComptaRepository : AbstractRepository
    {
        public ComptaRepository(Database TargetDb, Database SourceDb = null) : base(TargetDb, SourceDb)
        {
            targetDb = TargetDb;
            sourceDb = SourceDb;
        }

        public override List<Ligne> GetLignes(string PkFilter = null)
        {
            using (var cnx = new SqlConnection(sourceDb.cnxString))
            {
                cnx.Open();
                using (var cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = $@"SELECT L.PkValue, CG.CG_Num
                                        FROM {sourceDb.dboChain}.[MB_SYNCSTATE] L
                                        LEFT JOIN {targetDb.dboChain}.[MB_SYNCSTATE] R ON R.ObjectName = L.ObjectName AND R.PkValue = L.PkValue
                                        LEFT JOIN {targetDb.dboChain}.[F_COMPTEG] CG ON CG.CG_Num = L.PkValue
                                        WHERE L.ObjectName = 'COMPTA' AND (R.LastUpdate < L.LastUpdate OR R.LastUpdate IS NULL)
                                        %PKFILTER%
                                        ORDER BY L.PkValue";
                    cmd.CommandTimeout = CmdTimeOut;
                    FilterGetLignes(cmd, PkFilter);

                    using (var reader = cmd.ExecuteReader())
                    {
                        // Buffer le résultat
                        var dt = new DataTable();
                        dt.Load(reader);

                        var lignes = new List<Ligne>();

                        lignes = (from DataRow row in dt.Rows
                                  select new Ligne()
                                  {
                                      ObjectName = "COMPTA",
                                      PKValue = (string)row["PkValue"],
                                      IsInsert = row["CG_Num"] == DBNull.Value
                                  }).ToList();

                        return lignes;
                    }
                }
            }
        } 

        public override bool MajLigne(Ligne ligne)
        {
            // Ne doit fonctionner que pour l'insert
            if (!ligne.IsInsert)
            {
                RaiseLogEvent($"{ligne.PKValue} n'est pas nouveau.");
                return false;
            }

            RaiseLogEvent($"[{ligne.PKValue}] INSERT");
            using (var cnxTarget = new SqlConnection(targetDb.cnxString))
            {
                cnxTarget.Open();
                SqlTransaction transaction = cnxTarget.BeginTransaction();

                try
                {
                    using (var cnxSource = new SqlConnection(sourceDb.cnxString))
                    {
                        cnxSource.Open();
                        using (var cmdSource = GetSelectCmdFromCnx(
                            cnxSource, 
                            "F_COMPTEG",
                            new List<string>() {"CG_Num", "CG_Type", "CG_Intitule", "CG_Classement", "N_Nature", "CG_Report", "CR_Num", "CG_Raccourci", "CG_Saut", "CG_Regroup", "CG_Analytique",
                            "CG_Echeance", "CG_Quantite", "CG_Lettrage", "CG_Tiers", "CG_DateCreate", "CG_Devise", "N_Devise", "TA_Code", "CG_Sommeil", "CG_ReportAnal"},
                            ligne.PKValue
                            ))
                        {
                            using (var reader = cmdSource.ExecuteReader(CommandBehavior.SingleRow))
                            {
                                while (reader.Read())
                                {
                                    InsertRowFromReader("F_COMPTEG", reader, transaction);
                                }
                            }
                        }
                    }

                    transaction.Commit();

                    UpdateSyncState(ligne);
                    return true;
                }
                catch (Exception ex)
                {
                    RaiseLogEvent($"Commit Exception: {ex}");
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        RaiseLogEvent($"Rollback Exception Type: {ex2}");
                    }
                }
            }

            return false;
        }
    }
}
