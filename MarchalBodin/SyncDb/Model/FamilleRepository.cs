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
    public class FamilleRepository : AbstractRepository
    {
        private List<string> vCols = new List<string>() {"FA_CodeFamille", "csFamille", "csFamComptaAgg", "csFamFournissAgg", "csFamTarifAgg", "csFamTarifQteAgg", "CS"};
        public FamilleRepository(Database TargetDb, Database SourceDb = null) : base(TargetDb, SourceDb)
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
                    cmd.CommandText = $@"SELECT L.PkValue, FA.FA_CodeFamille
                                        FROM {sourceDb.dboChain}.[MB_SYNCSTATE] L
                                        LEFT JOIN {targetDb.dboChain}.[MB_SYNCSTATE] R ON R.ObjectName = L.ObjectName AND R.PkValue = L.PkValue
                                        LEFT JOIN {targetDb.dboChain}.[F_FAMILLE] FA ON FA.FA_CodeFamille = L.PkValue
                                        WHERE L.ObjectName = 'FAMILLE' AND (R.LastUpdate < L.LastUpdate OR R.LastUpdate IS NULL)
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
                                      ObjectName = "FAMILLE",
                                      PKValue = (string)row["PkValue"],
                                      IsInsert = row["FA_CodeFamille"] == DBNull.Value
                                  }).ToList();

                        return lignes;
                    }
                }
            }
        }

        /// <summary>
        /// Champs à renseigner obligatoirement lors de l’ajout
        /// FA_CodeFamille
        /// FA_Intitule
        /// FA_UniteVen
        ///
        /// Champs non modifiables en modification d’enregistrement
        /// Pour tous les types :
        /// FA_CodeFamille
        /// FA_Type
        ///
        /// Si FA_Type = 1 ou 2 (Centralisateur ou Total) alors tous les champs sont non modifiables, sauf l'intitule
        /// </summary>
        /// <param name="ligne"></param>
        /// <returns></returns>
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
                        // F_FAMILLE
                        //
                        using (var cmdSource = GetSelectCmdFromCnx(
                            cnxSource,
                            "F_FAMILLE",
                            new List<string>() {"FA_CodeFamille", "FA_Type", "FA_Intitule", "FA_UniteVen", "FA_Coef", "FA_SuiviStock", "FA_Garantie", "FA_Central", "FA_CodeFiscal", "FA_Pays",
                                "FA_UnitePoids", "FA_Escompte", "FA_Delai", "FA_HorsStat", "FA_VteDebit", "FA_NotImp", "FA_Frais01FR_Denomination", "FA_Frais01FR_Rem01REM_Valeur",
                                "FA_Frais01FR_Rem01REM_Type", "FA_Frais01FR_Rem02REM_Valeur", "FA_Frais01FR_Rem02REM_Type", "FA_Frais01FR_Rem03REM_Valeur", "FA_Frais01FR_Rem03REM_Type",
                                "FA_Frais02FR_Denomination", "FA_Frais02FR_Rem01REM_Valeur", "FA_Frais02FR_Rem01REM_Type", "FA_Frais02FR_Rem02REM_Valeur", "FA_Frais02FR_Rem02REM_Type",
                                "FA_Frais02FR_Rem03REM_Valeur", "FA_Frais02FR_Rem03REM_Type", "FA_Frais03FR_Denomination", "FA_Frais03FR_Rem01REM_Valeur", "FA_Frais03FR_Rem01REM_Type",
                                "FA_Frais03FR_Rem02REM_Valeur", "FA_Frais03FR_Rem02REM_Type", "FA_Frais03FR_Rem03REM_Valeur", "FA_Frais03FR_Rem03REM_Type", "FA_Contremarque", "FA_FactPoids",
                                "FA_FactForfait", "FA_Publie", "FA_RacineRef", "FA_RacineCB", "CL_No1", "CL_No2", "CL_No3", "CL_No4", "FA_Nature", "FA_NbColis", "FA_SousTraitance", "FA_Fictif"},
                            ligne.PKValue))
                        {
                            using (var reader = cmdSource.ExecuteReader(CommandBehavior.SingleRow))
                            {
                                reader.Read();
                                if (ligne.IsInsert)
                                {
                                    InsertRowFromReader("F_FAMILLE", reader, transaction);
                                }
                                else
                                {
                                    switch ((short)reader["FA_Type"])
                                    {
                                        case 0:
                                            UpdateRowFromReader("F_FAMILLE", reader, transaction, new List<string>() { "FA_Type" });
                                            break;
                                        case 1:
                                        case 2:
                                            using (var cmdTarget = cnxTarget.CreateCommand())
                                            {
                                                cmdTarget.Transaction = transaction;
                                                cmdTarget.CommandText = $@"UPDATE [{cnxTarget.Database}].[dbo].[F_FAMILLE] 
                                                                           SET [FA_Intitule] = @faIntitule
                                                                           WHERE [FA_CodeFamille] = @codeFam";
                                                cmdTarget.Parameters.AddWithValue("@faIntitule", reader["FA_Intitule"]);
                                                cmdTarget.Parameters.AddWithValue("@codeFam", ligne.PKValue);
                                                cmdTarget.CommandTimeout = CmdTimeOut;
                                                cmdTarget.ExecuteNonQuery();
                                            }
                                            break;
                                        default:
                                            throw new Exception($"FA_Type {reader["FA_Type"]} non pris en charge");
                                    }
                                }
                            }
                        }

                        //
                        // F_FAMCOMPTA
                        //
                        Delete("F_FAMCOMPTA", transaction, "FA_CodeFamille", ligne.PKValue);
                        CopySourceToTarget(
                            cnxSource,
                            "F_FAMCOMPTA",
                            new List<string>(){"FA_CodeFamille", "FCP_Type", "FCP_Champ", "FCP_ComptaCPT_CompteG", "FCP_ComptaCPT_CompteA",
                                "FCP_ComptaCPT_Taxe1", "FCP_ComptaCPT_Taxe2", "FCP_ComptaCPT_Taxe3", "FCP_TypeFacture", "FCP_ComptaCPT_Date1",
                                "FCP_ComptaCPT_Date2", "FCP_ComptaCPT_Date3", "FCP_ComptaCPT_TaxeAnc1", "FCP_ComptaCPT_TaxeAnc2", "FCP_ComptaCPT_TaxeAnc3"},
                            ligne.PKValue,
                            transaction);
                        //
                        // F_FAMFOURNISS
                        //
                        Delete("F_FAMFOURNISS", transaction, "FA_CodeFamille", ligne.PKValue);
                        CopySourceToTarget(
                            cnxSource,
                            "F_FAMFOURNISS",
                            new List<string>(){"FA_CodeFamille", "CT_Num", "FF_Unite", "FF_Conversion", "FF_DelaiAppro", "FF_Garantie", "FF_Colisage",
                                "FF_QteMini", "FF_QteMont", "EG_Champ", "FF_Principal", "FF_Devise", "FF_Remise", "FF_ConvDiv", "FF_TypeRem"},
                            ligne.PKValue,
                            transaction);
                        //
                        // F_FAMTARIF
                        //
                        // Champs à renseigner obligatoirement lors de l’ajout
                        //            FA_CodeFamille
                        //            FT_Categorie
                        //            EG_Champ
                        // Champs non modifiables en modification d’enregistrement
                        //            EG_Champ
                        //            FA_CodeFamille
                        //            FT_Categorie
                        Delete("F_FAMTARIF", transaction, "FA_CodeFamille", ligne.PKValue);
                        CopySourceToTarget(
                            cnxSource,
                            "F_FAMTARIF",
                            new List<string>() { "FA_CodeFamille", "FT_Categorie", "FT_Coef", "FT_PrixTTC", "FT_Arrondi", "FT_QteMont", "EG_Champ", "FT_Devise", "FT_Remise", "FT_Calcul", "FT_TypeRem" },
                            ligne.PKValue,
                            transaction);

                        //
                        // F_FAMTARIFQTE
                        //
                        // Champs à renseigner obligatoirement lors de l’ajout
                        //            FA_CodeFamille
                        //            FQ_RefCF
                        //            FQ_BorneSup
                        // Champs non modifiables en modification d’enregistrement
                        //            FA_CodeFamille
                        //            FQ_RefCF
                        //            FQ_BorneSup
                        Delete("F_FAMTARIFQTE", transaction, "FA_CodeFamille", ligne.PKValue);
                        CopySourceToTarget(
                            cnxSource,
                            "F_FAMTARIFQTE",
                            new List<string>() {"FA_CodeFamille", "FQ_RefCF", "FQ_BorneSup", "FQ_Remise01REM_Valeur", "FQ_Remise01REM_Type",
                                "FQ_Remise02REM_Valeur", "FQ_Remise02REM_Type", "FQ_Remise03REM_Valeur", "FQ_Remise03REM_Type"},
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

        public void DeleteFamilles()
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
                    var tables = new List<string>() { "F_FAMCLIENT", "F_FAMCOMPTA", "F_FAMFOURNISS", "F_FAMMODELE", "F_FAMTARIF", "F_FAMTARIFQTE", "F_FAMILLE" };

                    foreach (var table in tables)
                    {
                        using (var cmd = cnx.CreateCommand())
                        {
                            cmd.CommandText = $"DELETE L FROM {targetDb.dboChain}.[{table}] L WHERE L.FA_CodeFamille NOT IN(SELECT R.FA_CodeFamille FROM {sourceDb.dboChain}.[F_FAMILLE] R)";
                            RaiseLogEvent(cmd.CommandText);
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
