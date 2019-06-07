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
    class GlossaireRepository : AbstractRepository
    {
        public GlossaireRepository(Database TargetDb, Database SourceDb = null) : base(TargetDb, SourceDb)
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
                    cmd.CommandText = $@"SELECT L.PkValue, GL.GL_No
                                        FROM {sourceDb.dboChain}.[MB_SYNCSTATE] L
                                        LEFT JOIN {targetDb.dboChain}.[MB_SYNCSTATE] R ON R.ObjectName = L.ObjectName AND R.PkValue = L.PkValue
                                        LEFT JOIN {targetDb.dboChain}.[F_GLOSSAIRE] GL ON GL.GL_No = L.PkValue
                                        WHERE L.ObjectName = 'GLOSSAIRE' AND (R.LastUpdate < L.LastUpdate OR R.LastUpdate IS NULL)
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
                                      ObjectName = "GLOSSAIRE",
                                      PKValue = (string)row["PkValue"],
                                      IsInsert = row["GL_No"] == DBNull.Value
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

                        //
                        // F_GLOSSAIRE
                        //
                        using (var cmd = GetSelectCmdFromCnx(
                            cnxSource,
                            "F_GLOSSAIRE",
                            new List<string>() { "GL_No", "GL_Domaine", "GL_Intitule", "GL_Raccourci", "GL_PeriodeDeb", "GL_PeriodeFin", "GL_Text" },
                            ligne.PKValue))
                        {
                            using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                            {
                                reader.Read();
                                if (ligne.IsInsert)
                                {
                                    InsertRowFromReader("F_GLOSSAIRE", reader, transaction);
                                }
                                else
                                {
                                    UpdateRowFromReader("F_GLOSSAIRE", reader, transaction);
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
    }
}
