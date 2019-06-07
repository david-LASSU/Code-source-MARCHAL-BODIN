using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MBCore.Model;
using System.Data.SqlClient;
using System.Data;

namespace SyncDb.Model
{
    public class CatalogueRepository : AbstractRepository
    {
        private List<string> vCols = new List<string>() { "CL_No", "csCatalogue", "cbModification", "CL_Niveau" };
        public CatalogueRepository(Database TargetDb, Database SourceDb = null) : base(TargetDb, SourceDb)
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
                    cmd.CommandText = $@"SELECT L.PkValue, CL.CL_No
                                        FROM {sourceDb.dboChain}.[MB_SYNCSTATE] L
                                        LEFT JOIN {targetDb.dboChain}.[MB_SYNCSTATE] R ON R.ObjectName = L.ObjectName AND R.PkValue = L.PkValue
                                        LEFT JOIN {targetDb.dboChain}.[F_CATALOGUE] CL ON CL.CL_No = L.PkValue
                                        WHERE L.ObjectName = 'CATALOGUE' AND (R.LastUpdate < L.LastUpdate OR R.LastUpdate IS NULL)
                                        %PKFILTER%
                                        ORDER BY L.PkValue";
                    cmd.CommandTimeout = CmdTimeOut;
                    FilterGetLignes(cmd, PkFilter);

                    using (var reader = cmd.ExecuteReader())
                    {
                        var dt = new DataTable();
                        dt.Load(reader);

                        var lignes = new List<Ligne>();
                        lignes = (from DataRow row in dt.Rows
                                  select new Ligne()
                                  {
                                      ObjectName = "CATALOGUE",
                                      PKValue = (string)row["PkValue"],
                                      IsInsert = row["CL_No"] == DBNull.Value
                                  }).ToList();

                        return lignes;
                    }
                }
            }
        }

        public override bool MajLigne(Ligne ligne)
        {
            RaiseLogEvent($"[{ligne.PKValue}] " + (ligne.IsInsert ? "INSERT" : "UPDATE"));
            using (var cnxTarget = new SqlConnection(targetDb.cnxString))
            {
                cnxTarget.Open();
                var transaction = cnxTarget.BeginTransaction();
                try
                {
                    using (var cnxSource = new SqlConnection(sourceDb.cnxString))
                    {
                        cnxSource.Open();
                        using (var cmdSource = GetSelectCmdFromCnx(
                            cnxSource,
                            "F_CATALOGUE",
                            new List<string>() {"CL_No", "CL_Intitule", "CL_Code", "CL_Stock", "CL_NoParent", "CL_Niveau" },
                            ligne.PKValue
                        ))
                        {
                            using (var reader = cmdSource.ExecuteReader(CommandBehavior.SingleRow))
                            {
                                reader.Read();

                                // S'assure qu'il n'y aura pas de doublons de parent CL_No+CL_Intitule
                                using (var cmdTarget = cnxTarget.CreateCommand())
                                {
                                    cmdTarget.Transaction = transaction;
                                    cmdTarget.CommandText = $"UPDATE {cnxTarget.Database}.[dbo].[F_CATALOGUE] SET CL_Intitule = 'TEMPSYNC' + @clNo WHERE CL_NoParent = @clNoParent AND CL_Intitule = @clIntitule";
                                    cmdTarget.Parameters.AddWithValue("@clNo", ligne.PKValue);
                                    cmdTarget.Parameters.AddWithValue("@clNoParent", reader["CL_NoParent"]);
                                    cmdTarget.Parameters.AddWithValue("@clIntitule", reader["CL_Intitule"]);
                                    cmdTarget.CommandTimeout = CmdTimeOut;
                                    cmdTarget.ExecuteNonQuery();
                                }

                                if (ligne.IsInsert)
                                {
                                    InsertRowFromReader("F_CATALOGUE", reader, transaction);
                                }
                                else
                                {
                                    UpdateRowFromReader("F_CATALOGUE", reader, transaction);
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
                return false;
            }
        }

        public void DeleteCatalogue()
        {
            // Sécurité: ne pas appliquer à une base TARIF
            if (targetDb.name.Contains("TARIF"))
            {
                RaiseLogEvent("Pas de delete famille sur une base tarif");
                return;
            }
            // TODO : transactions ne fonctionnent pas
            using (var cnx = new SqlConnection(targetDb.cnxString))
            {
                try
                {
                    cnx.Open();
                    for (int i = 4; i > 0; i--)
                    {
                        using (var cmd = cnx.CreateCommand())
                        {
                            cmd.CommandText = $@"DELETE L FROM {targetDb.dboChain}.[F_CATALOGUE] L WHERE L.CL_No NOT IN(SELECT R.CL_No FROM {sourceDb.dboChain}.[F_CATALOGUE] R) AND CL_Niveau = @clNiv";
                            cmd.Parameters.AddWithValue("@clNiv", i);
                            RaiseLogEvent(cmd.CommandText.Replace("@clNiv", i.ToString()));
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    RaiseLogEvent(ex.ToString());
                }
            }
        }
    }
}
