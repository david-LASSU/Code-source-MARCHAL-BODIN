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
    class ModRegRepository : AbstractRepository
    {
        public ModRegRepository(Database TargetDb, Database SourceDb = null) : base(TargetDb, SourceDb)
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
                    cmd.CommandText = $@"SELECT L.PkValue, MR.MR_No
                                        FROM {sourceDb.dboChain}.[MB_SYNCSTATE] L
                                        LEFT JOIN {targetDb.dboChain}.[MB_SYNCSTATE] R ON R.ObjectName = L.ObjectName AND R.PkValue = L.PkValue
                                        LEFT JOIN {targetDb.dboChain}.[F_MODELER] MR ON MR.MR_No = L.PkValue
                                        WHERE L.ObjectName = 'MODELER' AND (R.LastUpdate < L.LastUpdate OR R.LastUpdate IS NULL)
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
                                      ObjectName = "MODELER",
                                      PKValue = (string)row["PkValue"],
                                      IsInsert = row["MR_No"] == DBNull.Value
                                  }).ToList();

                        return lignes;
                    }
                }
            }
        }

        public override bool MajLigne(Ligne ligne)
        {
            RaiseLogEvent($"[{ligne.PKValue}] " + (ligne.IsInsert ? "INSERT": "UPDATE"));
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
                        // F_MODELER
                        //
                        // Champs à renseigner obligatoirement lors de l’ajout: MR_No, MR_Intitule
                        // Champs non modifiables: MR_No
                        using (var cmdSource = GetSelectCmdFromCnx(cnxSource, "F_MODELER", new List<string>() { "MR_Intitule" }, ligne.PKValue))
                        {
                            using (var reader = cmdSource.ExecuteReader(CommandBehavior.SingleRow))
                            {
                                reader.Read();

                                if (ligne.IsInsert)
                                {
                                    InsertRowFromReader("F_MODELER", reader, transaction);
                                }
                                else
                                {
                                    UpdateRowFromReader("F_MODELER", reader, transaction);
                                }
                            }
                        }

                        //
                        // F_EMODELER
                        //
                        Delete("F_EMODELER", transaction, "MR_No", ligne.PKValue);
                        CopySourceToTarget(
                            cnxSource,
                            "F_EMODELER",
                            new List<string>() {"MR_No", "N_Reglement", "ER_Condition", "ER_NbJour", "ER_JourTb01", "ER_JourTb02",
                                "ER_JourTb03", "ER_JourTb04", "ER_JourTb05", "ER_JourTb06", "ER_TRepart", "ER_VRepart"},
                            ligne.PKValue,
                            transaction);
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
