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
    public class ArticleRepository : AbstractRepository
    {
        public ArticleRepository(Database TargetDb, Database SourceDb = null) : base(TargetDb, SourceDb)
        {
            targetDb = TargetDb;
            sourceDb = SourceDb;
        }

        public void MajConditionnements()
        {
            using (var cnxTarget = new SqlConnection(targetDb.cnxString))
            {
                cnxTarget.Open();

                var transaction = cnxTarget.BeginTransaction();

                try
                {
                    using (var cmd = cnxTarget.CreateCommand())
                    {
                        cmd.Transaction = transaction;
                        cmd.CommandTimeout = CmdTimeOut;

                        //
                        // P_CONDITIONNEMENT
                        //
                        cmd.CommandText = $@"UPDATE L SET L.P_Conditionnement = R.P_Conditionnement 
                                            FROM {targetDb.dboChain}.[P_CONDITIONNEMENT] L 
                                            JOIN {sourceDb.dboChain}.[P_CONDITIONNEMENT] R ON R.cbMarq = L.cbMarq
                                            WHERE L.P_Conditionnement <> R.P_Conditionnement";
                        cmd.ExecuteNonQuery();

                        //
                        // F_ENUMCOND
                        //
                        // Delete invalids conds
                        cmd.CommandText = $@"DELETE R FROM {targetDb.dboChain}.[F_ENUMCOND] R 
                                             WHERE R.EC_Champ IN(SELECT cbMarq FROM {targetDb.dboChain}.[P_CONDITIONNEMENT] WHERE P_Conditionnement = '')";
                        cmd.ExecuteNonQuery();
                    }
                    //
                    // Crée nouveaux conds
                    //
                    using (var cnxSource = new SqlConnection(sourceDb.cnxString))
                    {
                        cnxSource.Open();
                        using (var cmd = cnxSource.CreateCommand())
                        {
                            cmd.CommandText = $@"SELECT L.EC_Champ, L.EC_Enumere, L.EC_Quantite 
                                                 FROM {sourceDb.dboChain}.[F_ENUMCOND] L 
                                                 LEFT JOIN {targetDb.dboChain}.[F_ENUMCOND] R ON R.EC_Champ = L.EC_Champ AND R.EC_Enumere = L.EC_Enumere 
                                                 WHERE R.EC_Champ IS NULL";
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    InsertRowFromReader("F_ENUMCOND", reader, transaction);
                                }
                            }
                        }
                    }
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
        }

        public void MajGammes()
        {
            using (var cnxTarget = new SqlConnection(targetDb.cnxString))
            {
                cnxTarget.Open();
                var transaction = cnxTarget.BeginTransaction();

                try
                {
                    using (var cmd = cnxTarget.CreateCommand())
                    {
                        cmd.Transaction = transaction;
                        cmd.CommandTimeout = CmdTimeOut;

                        //
                        // P_GAMME
                        //
                        cmd.CommandText = $@"UPDATE L SET L.G_Intitule = R.G_Intitule, L.G_Type = R.G_Type
                                            FROM {targetDb.dboChain}.[P_GAMME] L 
                                            JOIN {sourceDb.dboChain}.[P_GAMME] R ON R.cbMarq = L.cbMarq
                                            WHERE L.G_Intitule <> R.G_Intitule";
                        cmd.ExecuteNonQuery();

                        //
                        // F_ENUMGAMME
                        //
                        // Delete invalids gammes
                        cmd.CommandText = $@"DELETE R FROM {targetDb.dboChain}.[F_ENUMGAMME] R 
                                            WHERE EG_Champ IN(SELECT cbMarq FROM {targetDb.dboChain}.[P_GAMME] WHERE G_Intitule = '')";
                        cmd.ExecuteNonQuery();
                    }

                    //
                    // Insert new 
                    //
                    using (var cnxSource = new SqlConnection(sourceDb.cnxString))
                    {
                        cnxSource.Open();
                        using (var cmd = cnxSource.CreateCommand())
                        {
                            cmd.CommandText = $@"SELECT L.EG_Champ, L.EG_Ligne, L.EG_Enumere, L.EG_BorneSup 
                                                 FROM {sourceDb.dboChain}.[F_ENUMGAMME] L 
                                                 LEFT JOIN {targetDb.dboChain}.[F_ENUMGAMME] R ON R.EG_Champ = L.EG_Champ AND R.EG_Enumere = L.EG_Enumere 
                                                 WHERE R.EG_Champ IS NULL";
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    InsertRowFromReader("F_ENUMGAMME", reader, transaction);
                                }
                            }
                        }
                    }
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
        }

        public override List<Ligne> GetLignes(string PkFilter = null)
        {
            using (var cnx = new SqlConnection(sourceDb.cnxString))
            {
                cnx.Open();
                using (var cmd = cnx.CreateCommand())
                {

                    cmd.CommandText = $@"SELECT L.PkValue, A.AR_Ref
                                        FROM {sourceDb.dboChain}.[MB_SYNCSTATE] L
                                        LEFT JOIN {targetDb.dboChain}.[MB_SYNCSTATE] R ON R.ObjectName = L.ObjectName AND R.PkValue = L.PkValue
                                        LEFT JOIN {targetDb.dboChain}.[F_ARTICLE] A ON A.AR_Ref = L.PkValue
                                        WHERE L.ObjectName = 'ARTICLE' AND (R.LastUpdate < L.LastUpdate OR R.LastUpdate IS NULL)
                                        %PKFILTER%
                                        ORDER BY A.AR_Nomencl, L.PkValue";
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
                                      ObjectName = "ARTICLE",
                                      PKValue = (string)row["PkValue"],
                                      IsInsert = row["AR_Ref"] == DBNull.Value
                                  }).ToList();

                        return lignes;
                    }
                }
            }
        }

        // Champs à renseigner obligatoirement lors de l’ajout
        //   AR_Ref
        //   AR_Design
        //   FA_CodeFamille
        //   AR_UniteVen
        // Champs non modifiables en modification d’enregistrement
        //   AR_Ref
        //   AR_Type
        //   AR_PUNet
        //   AR_SuiviStock : Si mouvement
        //   AR_Gamme1 : Si article présent dans F_Docligne, F_Nomenclat, F_TarifGam ou F_ArtGamme
        //   AR_Gamme2 : Si article présent dans F_Docligne, F_Nomenclat, F_TarifGam ou F_ArtGamme
        //   AR_Condition : Si article présent dans F_Condition, F_TarifCond
        public override bool MajLigne(Ligne ligne)
        {
            var updBlackList = new List<string>() { "AR_Ref", "AR_Type", "AR_PUNet", "AR_SuiviStock", "CO_No", "AR_Condition"};
            RaiseLogEvent($"[{ligne.PKValue}] " + (ligne.IsInsert ? "INSERT" : "UPDATE"));

            using (var cnxTarget = new SqlConnection(targetDb.cnxString))
            {
                cnxTarget.Open();
                var transaction = cnxTarget.BeginTransaction();

                try
                {
                    // On stocke temporairement le nouveau conditionnement pour le mettre à jour plus tard
                    string arCondition;
                    using (var cnxSource = new SqlConnection(sourceDb.cnxString))
                    {
                        cnxSource.Open();

                        //
                        // F_ARTICLE
                        //
                        using (var cmdTarget = GetSelectCmdFromCnx(
                            cnxSource,
                            "F_ARTICLE",
                            new List<string>() {"AR_Ref", "AR_Design", "FA_CodeFamille", "AR_Substitut", "AR_Raccourci", "AR_Garantie", "AR_UnitePoids", "AR_PoidsNet",
                                "AR_PoidsBrut", "AR_UniteVen", "AR_PrixAch", "AR_Coef", "AR_PrixVen", "AR_PrixTTC", "AR_Gamme1", "AR_Gamme2", "AR_Nomencl", "AR_Escompte",
                                "AR_Delai", "AR_HorsStat", "AR_VteDebit", "AR_NotImp", "AR_Sommeil", "AR_SuiviStock", "AR_Langue1", "AR_Langue2", "AR_EdiCode",
                                "AR_CodeBarre", "AR_CodeFiscal", "AR_Pays", "AR_Frais01FR_Denomination", "AR_Frais01FR_Rem01REM_Valeur", "AR_Frais01FR_Rem01REM_Type",
                                "AR_Frais01FR_Rem02REM_Valeur", "AR_Frais01FR_Rem02REM_Type", "AR_Frais01FR_Rem03REM_Valeur", "AR_Frais01FR_Rem03REM_Type", "AR_Frais02FR_Denomination",
                                "AR_Frais02FR_Rem01REM_Valeur", "AR_Frais02FR_Rem01REM_Type", "AR_Frais02FR_Rem02REM_Valeur", "AR_Frais02FR_Rem02REM_Type", "AR_Frais02FR_Rem03REM_Valeur",
                                "AR_Frais02FR_Rem03REM_Type", "AR_Frais03FR_Denomination", "AR_Frais03FR_Rem01REM_Valeur", "AR_Frais03FR_Rem01REM_Type", "AR_Frais03FR_Rem02REM_Valeur",
                                "AR_Frais03FR_Rem02REM_Type", "AR_Frais03FR_Rem03REM_Valeur", "AR_Frais03FR_Rem03REM_Type", "AR_Condition", "AR_PUNet", "AR_Contremarque", "AR_FactPoids",
                                "AR_FactForfait", "AR_SaisieVar", "AR_Transfere", "AR_Publie", "AR_Photo", "AR_PrixAchNouv", "AR_CoefNouv", "AR_PrixVenNouv", "AR_DateModif", "AR_DateCreation",
                                "AR_DateApplication", "AR_CoutStd", "AR_QteComp", "AR_QteOperatoire", "AR_Prevision", "CL_No1", "CL_No2", "CL_No3", "CL_No4", "AR_Type",
                                "RP_CodeDefaut", "AR_Nature", "AR_DelaiFabrication", "AR_NbColis", "AR_DelaiPeremption", "AR_DelaiSecurite", "AR_Fictif", "AR_SousTraitance", "AR_TypeLancement",
                                "SUPPRIME", "SUPPRIME_USINE", "Code_douanier"},
                            ligne.PKValue))
                        {
                            using (var reader = cmdTarget.ExecuteReader(CommandBehavior.SingleRow))
                            {
                                reader.Read();
                                arCondition = reader["AR_Condition"].ToString();

                                // Vérif doublons de code barre
                                if (reader["AR_CodeBarre"].ToString() != "")
                                {
                                    // F_ARTICLE
                                    using (var cmd = cnxTarget.CreateCommand())
                                    {
                                        cmd.Transaction = transaction;
                                        cmd.CommandText = $"UPDATE {cnxTarget.Database}.[dbo].[F_ARTICLE] SET AR_CodeBarre = NULL WHERE AR_CodeBarre = @codeBarre";
                                        cmd.Parameters.AddWithValue("@codeBarre", reader["AR_CodeBarre"]);
                                        cmd.Parameters.AddWithValue("@ref", ligne.PKValue);
                                        cmd.CommandTimeout = CmdTimeOut;
                                        RaiseLogEvent(cmd.CommandText.Replace("@ref", ligne.PKValue));
                                        cmd.ExecuteNonQuery();
                                    }
                                    // F_CONDITION
                                    using (var cmd = cnxTarget.CreateCommand())
                                    {
                                        cmd.Transaction = transaction;
                                        cmd.CommandText = $"UPDATE {cnxTarget.Database}.[dbo].[F_CONDITION] SET CO_CodeBarre = NULL WHERE CO_CodeBarre = @codeBarre";
                                        cmd.Parameters.AddWithValue("@codeBarre", reader["AR_CodeBarre"]);
                                        cmd.Parameters.AddWithValue("@ref", ligne.PKValue);
                                        cmd.CommandTimeout = CmdTimeOut;
                                        RaiseLogEvent(cmd.CommandText.Replace("@ref", ligne.PKValue));
                                        cmd.ExecuteNonQuery();
                                    }
                                    // F_TARIFGAM
                                    using (var cmd = cnxTarget.CreateCommand())
                                    {
                                        cmd.Transaction = transaction;
                                        cmd.CommandText = $"UPDATE {cnxTarget.Database}.[dbo].[F_TARIFGAM] SET TG_CodeBarre = NULL WHERE TG_CodeBarre = @codeBarre";
                                        cmd.Parameters.AddWithValue("@codeBarre", reader["AR_CodeBarre"]);
                                        cmd.Parameters.AddWithValue("@ref", ligne.PKValue);
                                        cmd.CommandTimeout = CmdTimeOut;
                                        RaiseLogEvent(cmd.CommandText.Replace("@ref", ligne.PKValue));
                                        cmd.ExecuteNonQuery();
                                    }
                                }

                                if (ligne.IsInsert)
                                {
                                    InsertRowFromReader("F_ARTICLE", reader, transaction);
                                }
                                else
                                {
                                    UpdateRowFromReader("F_ARTICLE", reader, transaction, updBlackList);
                                }
                            }
                        }

                        //
                        // F_NOMENCLAT
                        //
                        // DELETE NOMENCLATURES
                        Delete("F_NOMENCLAT", transaction, "AR_Ref", ligne.PKValue);

                        //
                        // F_TARIFGAM
                        // Attention lien avec F_ARTICLE, F_ARTGAMME, F_ARTFOURNISS et F_ARTCLIENT
                        // si diff, on supprime maintenant et recrée à la fin
                        Delete("F_TARIFGAM", transaction, "AR_Ref", ligne.PKValue);

                        //
                        // F_ARTENUMREF
                        // Attention lien avec F_ARTICLE, F_ARTGAMME
                        // si diff on supprime maintenant et on recrée à la fin
                        Delete("F_ARTENUMREF", transaction, "AR_Ref", ligne.PKValue);

                        //
                        // F_ARTGLOSS
                        //
                        // DELETE glossaires articles
                        Delete("F_ARTGLOSS", transaction, "AR_Ref", ligne.PKValue);
                        // Recréation des glossaires articles
                        CopySourceToTarget(
                            cnxSource,
                            "F_ARTGLOSS",
                            new List<string>() { "AR_Ref", "GL_No", "AGL_Num" },
                            ligne.PKValue,
                            transaction
                        );

                        //
                        // F_ARTGAMME
                        //
                        //
                        //Delete("F_ARTGAMME", transaction, "AR_Ref", arRef)
                        //CopySourceToTarget(cnxSource, "F_ARTGAMME", New List(Of String) From {"AR_Ref", "AG_No", "EG_Enumere", "AG_Type"}, arRef, transaction)
                        // Impossible de delete + recréation des gammes si utilisées dans les docs
                        UpdateOrInsertArtGamme(cnxSource, transaction, ligne);

                        //
                        // F_NOMENCLAT
                        //
                        CopySourceToTarget(
                            cnxSource,
                            "F_NOMENCLAT",
                            new List<string> {"AR_Ref", "NO_RefDet", "NO_Qte", "AG_No1", "AG_No2", "NO_Type", "NO_Repartition",
                                                      "NO_Operation", "NO_Commentaire", "DE_No", "NO_Ordre", "AG_No1Comp", "AG_No2Comp", "NO_SousTraitance"},
                            ligne.PKValue,
                            transaction);

                        //
                        // F_TARIFQTE & F_ARTCLIENT
                        //
                        // DELETE COND
                        Delete("F_CONDITION", transaction, "AR_Ref", ligne.PKValue);
                        // DELETE TARIFS COND
                        Delete("F_TARIFCOND", transaction, "AR_Ref", ligne.PKValue);
                        // DELETE TARIF QTE
                        Delete("F_TARIFQTE", transaction, "AR_Ref", ligne.PKValue);
                        // DELETE CAT TARIFS
                        Delete("F_ARTCLIENT", transaction, "AR_Ref", ligne.PKValue, "AC_Categorie <> 0");
                        // Update AR_Condition
                        using (var cmd = cnxTarget.CreateCommand())
                        {
                            cmd.Transaction = transaction;
                            cmd.CommandText = $"UPDATE {cnxTarget.Database}.[dbo].[F_ARTICLE] Set AR_Condition = @arCond WHERE AR_Ref = @ref";
                            cmd.Parameters.AddWithValue("@arCond", arCondition);
                            cmd.Parameters.AddWithValue("@ref", ligne.PKValue);
                            cmd.CommandTimeout = CmdTimeOut;
                            RaiseLogEvent(cmd.CommandText.Replace("@ref", ligne.PKValue).Replace("@arCond", arCondition));
                            cmd.ExecuteNonQuery();
                        }

                        // Recréation des CAT TARIFS
                        CopySourceToTarget(
                            cnxSource,
                            "F_ARTCLIENT",
                            new List<string>() {"AR_Ref", "AC_Categorie", "AC_PrixVen", "AC_Coef", "AC_PrixTTC", "AC_Arrondi", "AC_QteMont",
                                        "EG_Champ", "AC_PrixDev", "AC_Devise", "CT_Num", "AC_Remise", "AC_Calcul", "AC_TypeRem", "AC_RefClient",
                                        "AC_CoefNouv", "AC_PrixVenNouv", "AC_PrixDevNouv", "AC_RemiseNouv", "AC_DateApplication"},
                            ligne.PKValue,
                            transaction,
                            "AC_Categorie <> 0");

                        // Recréation des tarifs par quantite
                        CopySourceToTarget(
                            cnxSource,
                            "F_TARIFQTE",
                            new List<string>() {"AR_Ref", "TQ_RefCF", "TQ_BorneSup", "TQ_Remise01REM_Valeur", "TQ_Remise01REM_Type",
                                        "TQ_Remise02REM_Valeur", "TQ_Remise02REM_Type", "TQ_Remise03REM_Valeur", "TQ_Remise03REM_Type", "TQ_PrixNet"},
                            ligne.PKValue,
                            transaction
                        );

                        // Conditionnement
                        // Supprime les CO_No pouvant être en conflit de clé unique
                        using (var cmd = GetSelectCmdFromCnx(cnxSource, "F_CONDITION", new List<string>() { "AR_Ref", "CO_No", "CO_CodeBarre" }, ligne.PKValue))
                        {
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Delete("F_CONDITION", transaction, "CO_No", reader["CO_No"].ToString());

                                    if (reader["CO_CodeBarre"].ToString() != "")
                                    {
                                        // Supprime doublons codes barres dans F_ARTICLE
                                        using (var cmdCo = cnxTarget.CreateCommand())
                                        {
                                            cmdCo.Transaction = transaction;
                                            cmdCo.CommandText = $"UPDATE {cnxTarget.Database}.[dbo].[F_ARTICLE] Set AR_CodeBarre = NULL WHERE AR_CodeBarre = @codeBarre And AR_Ref <> @ref";
                                            cmdCo.Parameters.AddWithValue("@codeBarre", reader["CO_CodeBarre"]);
                                            cmdCo.Parameters.AddWithValue("@ref", ligne.PKValue);
                                            cmdCo.CommandTimeout = CmdTimeOut;
                                            RaiseLogEvent(cmdCo.CommandText.Replace("@ref", ligne.PKValue).Replace("@codeBarre", reader["CO_CodeBarre"].ToString()));
                                            cmdCo.ExecuteNonQuery();
                                        }

                                        // Supprime doublons codes barres dans F_CONDITION
                                        using (var cmdCo = cnxTarget.CreateCommand())
                                        {
                                            cmdCo.Transaction = transaction;
                                            cmdCo.CommandText = $"UPDATE {cnxTarget.Database}.[dbo].[F_CONDITION] Set CO_CodeBarre = NULL WHERE CO_CodeBarre = @codeBarre And AR_Ref <> @ref";
                                            cmdCo.Parameters.AddWithValue("@codeBarre", reader["CO_CodeBarre"]);
                                            cmdCo.Parameters.AddWithValue("@ref", ligne.PKValue);
                                            cmdCo.CommandTimeout = CmdTimeOut;
                                            RaiseLogEvent(cmdCo.CommandText.Replace("@ref", ligne.PKValue).Replace("@codeBarre", reader["CO_CodeBarre"].ToString()));
                                            cmdCo.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                        }

                        // Recréation des tarifs cond
                        CopySourceToTarget(cnxSource, "F_CONDITION", new List<string>() { "AR_Ref", "CO_No", "EC_Enumere", "EC_Quantite", "CO_Ref", "CO_CodeBarre", "CO_Principal"}, ligne.PKValue, transaction);

                        // Update le CO_No de l'article une fois que tout les F_CONDITION sont créés/supprimé
                        using (var cmdSource = GetSelectCmdFromCnx(cnxSource, "F_ARTICLE", new List<string>() { "AR_Ref", "CO_No"}, ligne.PKValue))
                        {
                            using (var reader = cmdSource.ExecuteReader(CommandBehavior.SingleRow))
                            {
                                reader.Read();

                                using (var cmdTarget = cnxTarget.CreateCommand())
                                {
                                    cmdTarget.Transaction = transaction;
                                    cmdTarget.CommandText = $"UPDATE {cnxTarget.Database}.[dbo].[F_ARTICLE] SET CO_No = @CO_No WHERE AR_Ref = @AR_Ref";
                                    cmdTarget.Parameters.AddWithValue("@CO_No", reader["CO_No"]);
                                    cmdTarget.Parameters.AddWithValue("@AR_Ref", ligne.PKValue);
                                    cmdTarget.CommandTimeout = CmdTimeOut;
                                    RaiseLogEvent(cmdTarget.CommandText);
                                    cmdTarget.ExecuteNonQuery();
                                }
                            }
                        }

                        CopySourceToTarget(cnxSource, "F_TARIFCOND", new List<string>() { "AR_Ref", "TC_RefCF", "CO_No", "TC_Prix", "TC_PrixNouv" }, ligne.PKValue, transaction);

                        //
                        // F_ARTFOURNISS
                        //
                        // DELETE TARIF(S) FOURNISSEUR
                        Delete("F_ARTFOURNISS", transaction, "AR_Ref", ligne.PKValue);

                        // Recréation des tarifs fournisseurs
                        using (var cmdSource = GetSelectCmdFromCnx(
                            cnxSource,
                            "F_ARTFOURNISS",
                            new List<string>() {"AR_Ref", "CT_Num", "AF_RefFourniss", "AF_PrixAch", "AF_Unite", "AF_Conversion", "AF_DelaiAppro", "AF_Garantie",
                                "AF_Colisage", "AF_QteMini", "AF_QteMont", "EG_Champ", "AF_Principal", "AF_PrixDev", "AF_Devise", "AF_Remise", "AF_ConvDiv", "AF_TypeRem",
                                "AF_CodeBarre", "AF_PrixAchNouv", "AF_PrixDevNouv", "AF_RemiseNouv", "AF_DateApplication"},
                            ligne.PKValue
                            ))
                        {
                            using (var reader = cmdSource.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    // Vérif code barre
                                    if (reader["AF_CodeBarre"].ToString() != "")
                                    {
                                        using (var cmd = cnxTarget.CreateCommand())
                                        {
                                            cmd.Transaction = transaction;
                                            cmd.CommandText = $"UPDATE {cnxTarget.Database}.[dbo].[F_ARTFOURNISS] Set AF_CodeBarre = NULL WHERE AF_CodeBarre = @codeBarre AND AR_Ref <> @ref AND CT_Num = @ctNum";
                                            cmd.Parameters.AddWithValue("@codeBarre", reader["AF_CodeBarre"]);
                                            cmd.Parameters.AddWithValue("@ref", reader["AR_Ref"]);
                                            cmd.Parameters.AddWithValue("@ctNum", reader["CT_Num"]);
                                            cmd.CommandTimeout = CmdTimeOut;
                                            cmd.ExecuteNonQuery();
                                        }
                                    }

                                    // Vérif Ref Fourn
                                    if (reader["AF_RefFourniss"].ToString() != "")
                                    {
                                        using (var cmd = cnxTarget.CreateCommand())
                                        {
                                            cmd.Transaction = transaction;
                                            cmd.CommandText = $"UPDATE {cnxTarget.Database}.[dbo].[F_ARTFOURNISS] Set AF_RefFourniss = NULL WHERE AF_RefFourniss = @refFourn AND AR_Ref <> @ref And CT_Num = @ctNum";
                                            cmd.Parameters.AddWithValue("@refFourn", reader["AF_RefFourniss"]);
                                            cmd.Parameters.AddWithValue("@ref", reader["AR_Ref"]);
                                            cmd.Parameters.AddWithValue("@ctNum", reader["CT_Num"]);
                                            cmd.CommandTimeout = CmdTimeOut;
                                            cmd.ExecuteNonQuery();
                                        }
                                    }

                                    InsertRowFromReader("F_ARTFOURNISS", reader, transaction);
                                }
                            }
                        }

                        //
                        // F_ARTCOMPTA
                        //
                        // DELETE PASSERELLES COMPTA
                        Delete("F_ARTCOMPTA", transaction, "AR_Ref", ligne.PKValue);

                        // Recréation des passerelles comptables
                        CopySourceToTarget(
                            cnxSource,
                            "F_ARTCOMPTA",
                            new List<string>() {
                                "AR_Ref", "ACP_Type", "ACP_Champ", "ACP_ComptaCPT_CompteG", "ACP_ComptaCPT_CompteA",
                                "ACP_ComptaCPT_Taxe1", "ACP_ComptaCPT_Taxe2", "ACP_ComptaCPT_Taxe3", "ACP_ComptaCPT_Date1", "ACP_ComptaCPT_Date2",
                                "ACP_ComptaCPT_Date3", "ACP_ComptaCPT_TaxeAnc1", "ACP_ComptaCPT_TaxeAnc2", "ACP_ComptaCPT_TaxeAnc3", "ACP_TypeFacture"},
                            ligne.PKValue,
                            transaction
                        );

                        //
                        // F_ARTENUMREF
                        //
                        // Les tarifs gammes ont déjà été supprimés plus haut, on les recrée juste
                        CopySourceToTarget(cnxSource, "F_ARTENUMREF", new List<string>() { "AR_Ref", "AG_No1", "AG_No2", "AE_Ref", "AE_PrixAch", "AE_CodeBarre", "AE_PrixAchNouv", "AE_EdiCode", "AE_Sommeil" }, ligne.PKValue, transaction);

                        //
                        // F_TARIFGAM
                        //
                        // Les tarifs gammes ont déjà été supprimés plus haut, on les recrée juste
                        CopySourceToTarget(cnxSource, "F_TARIFGAM", new List<string>() { "AR_Ref", "TG_RefCF", "AG_No1", "AG_No2", "TG_Prix", "TG_Ref", "TG_CodeBarre", "TG_PrixNouv" }, ligne.PKValue, transaction);
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

        protected void UpdateOrInsertArtGamme(SqlConnection cnxSource, SqlTransaction transaction, Ligne ligne)
        {
            using (var cmdSource = GetSelectCmdFromCnx(cnxSource, "F_ARTGAMME", new List<string> { "AR_Ref", "AG_No", "EG_Enumere", "AG_Type"}, ligne.PKValue))
            {
                using (var reader = cmdSource.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        using (var cmdTarget = transaction.Connection.CreateCommand())
                        {
                            cmdTarget.Transaction = transaction;
                            cmdTarget.CommandText = $@"UPDATE {transaction.Connection.Database}.[dbo].[F_ARTGAMME] SET EG_Enumere = @egEnum, AG_Type = @agType WHERE AR_Ref = @arRef AND AG_No = @agNo
                                                       IF @@ROWCOUNT = 0
                                                       INSERT INTO {transaction.Connection.Database}.[dbo].[F_ARTGAMME] (AR_Ref, AG_No, EG_Enumere, AG_Type) VALUES(@arRef, @agNo, @egEnum, @agType)";
                            cmdTarget.Parameters.AddWithValue("@arRef", ligne.PKValue);
                            cmdTarget.Parameters.AddWithValue("@agNo", reader["AG_No"]);
                            cmdTarget.Parameters.AddWithValue("@egEnum", reader["EG_Enumere"]);
                            cmdTarget.Parameters.AddWithValue("@agType", reader["AG_Type"]);
                            cmdTarget.CommandTimeout = CmdTimeOut;
                            RaiseLogEvent(cmdTarget.CommandText);
                            cmdTarget.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
