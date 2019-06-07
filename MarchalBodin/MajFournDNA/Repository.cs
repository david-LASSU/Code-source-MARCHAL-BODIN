using MBCore.Model;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MajFournDNA
{
    public class Repository : BaseCialAbstract
    {
        // TODO Deplacer dans MBCore, utiliser des enum à la place?
        private Dictionary<int, string> unitesPoids = new Dictionary<int, string>() {
            {0, "Tonne"},
            {1, "Quintal"},
            {2, "Kilogramme"},
            {3, "Gramme"},
            {4, "Milligramme"}
        };

        public string EnregistrerArticle(ListRow row, string ctNum)
        {
            try
            {
                using (SqlConnection cnx = new SqlConnection(cnxString))
                {
                    cnx.Open();

                    SqlTransaction transaction = cnx.BeginTransaction();
                    try
                    {
                        #region init vars
                        ListObject tableArticle = row.Parent;
                        string refMag = (string)row.Range.Columns[tableArticle.ListColumns["REF MAG"].Index].value;
                        string arRef = (string)row.Range.Columns[tableArticle.ListColumns["AR_Ref"].Index].value;
                        int sommeil = $"{row.Range.Columns[tableArticle.ListColumns["SOMMEIL"].Index].value}" == "M" ? 1 : 0;
                        string supp = $"{row.Range.Columns[tableArticle.ListColumns["SUPP"].Index].value}" == "S" ? "Oui" : "Non";
                        string suppUsine = $"{row.Range.Columns[tableArticle.ListColumns["SUPP USINE"].Index].value}" == "U" ? "Oui" : "Non";
                        int publieWeb = $"{row.Range.Columns[tableArticle.ListColumns["PUBLIE WEB"].Index].value}" == "W" ? 1 : 0;
                        string designation = (string)row.Range.Columns[tableArticle.ListColumns["DESIGNATION"].Index].value;
                        string famille = (string)row.Range.Columns[tableArticle.ListColumns["FAMILLE"].Index].value;
                        bool isPrincipal = $"{row.Range.Columns[tableArticle.ListColumns["PRINCIPAL"].Index].value}" == "P";
                        double nvPxNetAchat = row.Range.Columns[tableArticle.ListColumns["NV PX NET ACHAT"].Index].value;
                        double arCoef = row.Range.Columns[tableArticle.ListColumns["NV COEF SUR PX NET"].Index].value;
                        double nvPxVente = row.Range.Columns[tableArticle.ListColumns["NV PX VENTE"].Index].value;
                        double arPoidsNet = row.Range.Columns[tableArticle.ListColumns["POIDS"].Index].value;
                        int unitePoids = unitesPoids.First(u => u.Value == row.Range.Columns[tableArticle.ListColumns["UNITE POIDS"].Index].value).Key;
                        string gencodArt = row.Range.Columns[tableArticle.ListColumns["GENCOD ART"].Index].value != null ? row.Range.Columns[tableArticle.ListColumns["GENCOD ART"].Index].value.ToString() : string.Empty;
                        string uniteVen = row.Range.Columns[tableArticle.ListColumns["UNITE VENTE"].Index].value;
                        int coNo = int.Parse($"{row.Range.Columns[tableArticle.ListColumns["CO_No"].Index].value}");
                        string enumCond = (string)row.Range.Columns[tableArticle.ListColumns["ENUM COND"].Index].value;
                        double condQt = row.Range.Columns[tableArticle.ListColumns["COND QT"].Index].value;
                        int ag1 = int.Parse($"{row.Range.Columns[tableArticle.ListColumns["AG1"].Index].value}");
                        int ag2 = int.Parse($"{row.Range.Columns[tableArticle.ListColumns["AG2"].Index].value}");
                        bool isGamme = ag1 != 0 || ag2 != 0;
                        bool isCond = coNo > 0;
                        bool isCondPrinc = row.Range.Columns[tableArticle.ListColumns["CO_Principal"].Index].value == 1;
                        bool isGammeOrCond = isGamme | isCond;
                        string refFourn = row.Range.Columns[tableArticle.ListColumns["REF FOURN"].Index].value != null ? row.Range.Columns[tableArticle.ListColumns["REF FOURN"].Index].value.ToString() : string.Empty;
                        double nvPxFourn = row.Range.Columns[tableArticle.ListColumns["NV PX FOURN"].Index].value;
                        double afRemise = row.Range.Columns[tableArticle.ListColumns["NV REMISE"].Index].value;
                        double afConv = row.Range.Columns[tableArticle.ListColumns["CONV VEN"].Index].value;
                        double afConvDiv = row.Range.Columns[tableArticle.ListColumns["CONV ACH"].Index].value;
                        string gencodFourn = row.Range.Columns[tableArticle.ListColumns["GENCOD FOURN"].Index].value != null ? row.Range.Columns[tableArticle.ListColumns["GENCOD FOURN"].Index].value.ToString() : string.Empty;
                        double afColisage = row.Range.Columns[tableArticle.ListColumns["COLISAGE"].Index].value;
                        double afQteMin = row.Range.Columns[tableArticle.ListColumns["QEC"].Index].value;
                        string afUnite = row.Range.Columns[tableArticle.ListColumns["UNITE ACHAT"].Index].value;
                        bool majArticle = row.Range.Columns[tableArticle.ListColumns["MAJ ARTICLE"].Index].value == 1;
                        double nvPxTTC = row.Range.Columns[tableArticle.ListColumns["NV PX TTC"].Index].value;
                        string ecoTaxe = row.Range.Columns[tableArticle.ListColumns["ECO TAXE"].Index].value??string.Empty;
                        double ecoTaxeQt = row.Range.Columns[tableArticle.ListColumns["QT ECO TAXE"].Index].value??0;
                        string refFournBase = row.Range.Columns[tableArticle.ListColumns["REF FOURN BASE"].Index].value;
                        string gamme1 = row.Range.Columns[tableArticle.ListColumns["GAMME 1"].Index].value;
                        string gamme2 = row.Range.Columns[tableArticle.ListColumns["GAMME 2"].Index].value;
                        double nvPxAch = row.Range.Columns[tableArticle.ListColumns["NV PX ACHAT"].Index].value;
                        string codeDouanier = row.Range.Columns[tableArticle.ListColumns["CODE DOUANIER"].Index].value != null ? row.Range.Columns[tableArticle.ListColumns["CODE DOUANIER"].Index].value.ToString() : string.Empty;
                        #endregion

                        #region Protection lignes Gamme/Cond manquantes
                        // Vérifie que l'article à gamme/cond a bien toutes les lignes de présentes dans le fichier excel
                        if (isGammeOrCond)
                        {
                            using (SqlConnection cnx2 = new SqlConnection(cnxString))
                            {
                                cnx2.Open();

                                if (isGamme)
                                {
                                    using (SqlCommand cmd = cnx2.CreateCommand())
                                    {
                                        cmd.CommandText = "SELECT COUNT(*) AS Total FROM F_ARTENUMREF WHERE AR_Ref = @arRef";
                                        cmd.Parameters.AddWithValue("@arRef", arRef);
                                        using (SqlDataReader reader = cmd.ExecuteReader())
                                        {
                                            reader.Read();
                                            if (reader["Total"].ToString() != row.Application.WorksheetFunction.CountIf(tableArticle.ListColumns["AR_Ref"].Range, arRef).ToString())
                                            {
                                                throw new Exception("Des lignes de gamme sont manquantes dans le fichier.");
                                            }
                                        }
                                    }
                                }

                                if (isCond)
                                {
                                    using (SqlCommand cmd = cnx2.CreateCommand())
                                    {
                                        cmd.CommandText = "SELECT COUNT(*) AS Total FROM F_CONDITION WHERE AR_Ref = @arRef";
                                        cmd.Parameters.AddWithValue("@arRef", arRef);
                                        using (SqlDataReader reader = cmd.ExecuteReader())
                                        {
                                            reader.Read();
                                            if (reader["Total"].ToString() != row.Application.WorksheetFunction.CountIf(tableArticle.ListColumns["AR_Ref"].Range, arRef).ToString())
                                            {
                                                throw new Exception("Des lignes de conditionnement sont manquantes dans le fichier.");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        // Mise à jour de la fiche article, fiche fournisseur
                        // Préparation des autres tables si conditionnements/gammes
                        if (majArticle)
                        {
                            #region F_ARTICLE
                            using (SqlCommand cmd = cnx.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandText = @"UPDATE F_ARTICLE SET [AR_Sommeil] = @sommeil, [SUPPRIME] = @supprime, [SUPPRIME_USINE] = @suppUsine, 
                                                        [AR_Design] = @design, [FA_CodeFamille] = @famille, Code_douanier = @codeDouanier ";
                                cmd.Parameters.AddWithValue("@sommeil", sommeil);
                                cmd.Parameters.AddWithValue("@supprime", supp);
                                cmd.Parameters.AddWithValue("@suppUsine", suppUsine);
                                cmd.Parameters.AddWithValue("@design", designation);
                                cmd.Parameters.AddWithValue("@famille", famille);
                                cmd.Parameters.AddWithValue("@codeDouanier", codeDouanier);

                                if (isPrincipal)
                                {
                                    cmd.CommandText += @", [AR_PrixAch] = ROUND(@arPrixAch, 2), [AR_Coef] = @arCoef, [AR_PrixVen] = ROUND(@arPrixVen, 2), 
                                                             [AR_PoidsNet] = @poids, [AR_UnitePoids] = @unitePoids, [AR_CodeBarre] = @arCodeBarre,
                                                             [AR_UniteVen] = (SELECT cbMarq FROM P_UNITE WHERE U_Intitule = @uniteVen) ";
                                    cmd.Parameters.AddWithValue("@arPrixAch", nvPxNetAchat);
                                    cmd.Parameters.AddWithValue("@arCoef", arCoef);
                                    cmd.Parameters.AddWithValue("@arPrixVen", nvPxVente);
                                    cmd.Parameters.AddWithValue("@poids", arPoidsNet);
                                    cmd.Parameters.AddWithValue("@unitePoids", unitePoids);
                                    cmd.Parameters.AddWithValue("@arCodeBarre", isGammeOrCond ? string.Empty : gencodArt);
                                    cmd.Parameters.AddWithValue("@uniteVen", uniteVen);
                                }
                                cmd.CommandText += " WHERE AR_Ref = @arRef";
                                cmd.Parameters.AddWithValue("@arRef", arRef);
                                cmd.ExecuteNonQuery();
                            }
                            #endregion

                            #region F_ARTFOURNISS
                            using (SqlCommand cmd = cnx.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandText = @"UPDATE F_ARTFOURNISS SET [AF_RefFourniss] = @refFourn, [AF_PrixAch] = @afPrixAch, [AF_Remise] = @afRemise,
                                                        [AF_Conversion] = @conv, [AF_ConvDiv] = @convDiv, [AF_CodeBarre] = @afCodeBarre, [AF_Colisage] = @afColisage,
                                                        [AF_QteMini] = @afQteMin, [AF_Unite] = (SELECT cbMarq FROM P_UNITE WHERE U_Intitule = @afUnite)
                                                        WHERE [AR_Ref] = @arRef AND [CT_Num] = @ctNum;";
                                cmd.Parameters.AddWithValue("@refFourn", refFourn ?? string.Empty);
                                cmd.Parameters.AddWithValue("@afPrixAch", nvPxFourn);
                                cmd.Parameters.AddWithValue("@afRemise", afRemise);
                                cmd.Parameters.AddWithValue("@conv", afConv);
                                cmd.Parameters.AddWithValue("@convDiv", afConvDiv);
                                cmd.Parameters.AddWithValue("@afCodeBarre", gencodFourn ?? string.Empty);
                                cmd.Parameters.AddWithValue("@afColisage", afColisage);
                                cmd.Parameters.AddWithValue("@afQteMin", afQteMin);
                                cmd.Parameters.AddWithValue("@afUnite", afUnite);
                                cmd.Parameters.AddWithValue("@arRef", arRef);
                                cmd.Parameters.AddWithValue("@ctNum", ctNum);

                                cmd.ExecuteNonQuery();
                            }

                            // Set principal
                            if (isPrincipal)
                            {
                                using (SqlCommand cmd = cnx.CreateCommand())
                                {
                                    cmd.Transaction = transaction;
                                    cmd.CommandText = "UPDATE F_ARTFOURNISS SET [AF_Principal] = 0 WHERE [AR_Ref] = @arRef;";
                                    cmd.Parameters.AddWithValue("@arRef", arRef);
                                    cmd.ExecuteNonQuery();
                                    cmd.CommandText = "UPDATE F_ARTFOURNISS SET [AF_Principal] = 1 WHERE [AR_Ref] = @arRef AND [CT_Num] = @ctNum;";
                                    cmd.Parameters.AddWithValue("@ctNum", ctNum);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            #endregion

                            #region DELETE F_TARIFCOND, F_TARIFGAM & F_ARTCLIENT
                            if (isPrincipal)
                            {
                                using (SqlCommand cmd = cnx.CreateCommand())
                                {
                                    cmd.Transaction = transaction;
                                    cmd.Parameters.AddWithValue("@arRef", arRef);
                                    cmd.CommandText = "DELETE FROM F_TARIFCOND WHERE AR_Ref = @arRef";
                                    cmd.ExecuteNonQuery();
                                    cmd.CommandText = "DELETE FROM F_TARIFGAM WHERE AR_Ref = @arRef";
                                    cmd.ExecuteNonQuery();
                                    cmd.CommandText = "DELETE FROM F_ARTCLIENT WHERE AR_Ref = @arRef";
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            #endregion

                            #region F_ARTCLIENT
                            if (isPrincipal)
                            {
                                double remise;
                                for (int cat = 0; cat < 14; cat++)
                                {
                                    using (SqlCommand cmd = cnx.CreateCommand())
                                    {
                                        cmd.Transaction = transaction;
                                        cmd.Parameters.AddWithValue("@arRef", arRef);
                                        switch (cat)
                                        {
                                            case 0:
                                                cmd.CommandText = "INSERT INTO F_ARTCLIENT(AR_Ref, AC_Categorie, AC_PrixVen, AC_PrixTTC) VALUES (@arRef, 1, @acPrixVen, 1)";
                                                cmd.Parameters.AddWithValue("@acPrixVen", nvPxTTC);
                                                cmd.ExecuteNonQuery();
                                                break;
                                            case 6:
                                                cmd.CommandText = "INSERT INTO F_ARTCLIENT(AR_Ref, AC_Categorie, AC_Coef) VALUES (@arRef, 7, 1.1)";
                                                cmd.ExecuteNonQuery();
                                                break;
                                            case 13:
                                                cmd.CommandText = "INSERT INTO F_ARTCLIENT(AR_Ref, AC_Categorie, AC_Coef) VALUES (@arRef, 14, 1)";
                                                cmd.ExecuteNonQuery();
                                                break;
                                            default:
                                                remise = row.Range.Columns[tableArticle.ListColumns[$"REMISE CAT {cat}"].Index].value??0;
                                                if (remise > 0 || isGammeOrCond)
                                                {
                                                    cmd.CommandText = "INSERT INTO F_ARTCLIENT(AR_Ref, AC_Categorie, AC_Remise) VALUES (@arRef, @acCat, @remise)";
                                                    cmd.Parameters.AddWithValue("@acCat", cat+1);
                                                    cmd.Parameters.AddWithValue("@remise", remise);
                                                    cmd.ExecuteNonQuery();
                                                }
                                                break;
                                        }
                                        
                                    }
                                }
                            }
                            #endregion

                            #region ECO TAXE
                            // Supprime d'abord les eco-taxes
                            using (SqlCommand cmd = cnx.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                // TODO Centraliser le listing des eco-taxes
                                cmd.CommandText = "DELETE FROM F_NOMENCLAT WHERE AR_Ref = @arRef AND NO_RefDet IN('ECO-TAXE','ECO_TAXE_MOBILIER','TGAP','TGAP_2')";
                                cmd.Parameters.AddWithValue("@arRef", arRef);
                                cmd.ExecuteNonQuery();

                                if (ecoTaxe != string.Empty)
                                {
                                    cmd.CommandText = @"INSERT INTO F_NOMENCLAT(AR_Ref, NO_RefDet, NO_Qte, AG_No1, AG_No2, NO_Type, NO_Repartition, NO_Operation, NO_Commentaire, DE_No, NO_Ordre, AG_No1Comp, AG_No2Comp, NO_SousTraitance) 
                                                        VALUES (@arRef,@ecoTaxe,@qte,0,0,1,0,'','',0,ISNULL((SELECT MAX(NO_Ordre) FROM F_NOMENCLAT WHERE AR_Ref = @arRef), 0)+1,0,0,0)";
                                    cmd.Parameters.AddWithValue("@ecoTaxe", ecoTaxe);
                                    cmd.Parameters.AddWithValue("@qte", ecoTaxeQt);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            #endregion

                            #region CLEAR EAN
                            if (isCondPrinc)
                            {
                                // Efface le EAN de la fiche article (sera deplacé dans le conditionnement principal)
                                using (SqlCommand cmd = cnx.CreateCommand())
                                {
                                    cmd.Transaction = transaction;
                                    cmd.CommandText = "UPDATE F_ARTICLE SET AR_CodeBarre = NULL WHERE AR_Ref = @arRef";
                                    cmd.Parameters.AddWithValue("@arRef", arRef);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            #endregion
                        }

                        #region GAMME
                        if (isGamme)
                        {
                            // Efface le code barre fournisseur car deplacé dans chaque tarif gamme
                            // Ajoute une ref fourn de base commune à tous les articles de la gamme
                            using (SqlCommand cmd = cnx.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandText = "UPDATE F_ARTFOURNISS SET AF_RefFourniss = @refFournBase, AF_CodeBarre = '' WHERE AR_Ref = @arRef AND CT_Num = @ctNum";
                                cmd.Parameters.AddWithValue("@refFournBase", refFournBase);
                                cmd.Parameters.AddWithValue("@arRef", arRef);
                                cmd.Parameters.AddWithValue("@ctNum", ctNum);
                                cmd.ExecuteNonQuery();
                            }

                            // F_ARTGAMME
                            using (SqlCommand cmd = cnx.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandText = "UPDATE F_ARTGAMME SET EG_Enumere = @egEnum WHERE AR_Ref = @arRef AND AG_No = @agNo";
                                cmd.Parameters.AddWithValue("@egEnum", gamme1);
                                cmd.Parameters.AddWithValue("@arRef", arRef);
                                cmd.Parameters.AddWithValue("@agNo", ag1);
                                cmd.ExecuteNonQuery();
                                if (ag2 > 0)
                                {
                                    cmd.Parameters["@egEnum"].Value = gamme2;
                                    cmd.Parameters["@agNo"].Value = ag2;
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            // F_ARTENUMREF
                            if (isPrincipal)
                            {
                                using (SqlCommand cmd = cnx.CreateCommand())
                                {
                                    cmd.Transaction = transaction;
                                    cmd.CommandText = "UPDATE F_ARTENUMREF SET AE_Ref = @aeRef, AE_PrixAch = @aePxAch, AE_CodeBarre = @aeCodeBarre WHERE AR_Ref = @arRef AND AG_No1 = @ag1 AND AG_No2 = @ag2";
                                    cmd.Parameters.AddWithValue("@aeRef", refMag);
                                    cmd.Parameters.AddWithValue("@aePxAch", nvPxAch);
                                    cmd.Parameters.AddWithValue("@aeCodeBarre", gencodArt);
                                    cmd.Parameters.AddWithValue("@arRef", arRef);
                                    cmd.Parameters.AddWithValue("@ag1", ag1);
                                    cmd.Parameters.AddWithValue("@ag2", ag2);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            // F_TARIFGAM (Tarif fournisseur)
                            using (SqlCommand cmd = cnx.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandText = @"INSERT INTO F_TARIFGAM(AR_Ref, TG_RefCF, AG_No1, AG_No2, TG_Prix, TG_Ref, TG_CodeBarre) 
                                                    VALUES (@arRef,@ctNum,@ag1,@ag2,@nvPxFourn,@refFourn,@gencodFourn)";
                                cmd.Parameters.AddWithValue("@arRef", arRef);
                                cmd.Parameters.AddWithValue("@ctNum", ctNum);
                                cmd.Parameters.AddWithValue("@ag1", ag1);
                                cmd.Parameters.AddWithValue("@ag2", ag2);
                                cmd.Parameters.AddWithValue("@nvPxFourn", nvPxFourn);
                                cmd.Parameters.AddWithValue("@refFourn", refFourn);
                                cmd.Parameters.AddWithValue("@gencodFourn", ((object)gencodFourn)??DBNull.Value);
                                cmd.ExecuteNonQuery();
                            }

                            // F_TARIFGAM (Tarif client)
                            if (isPrincipal)
                            {
                                // Attention! Ici on reference la cat tarif par TG_RefCF 
                                // ex a01 => AC_Categorie 0 => Tarif TTC
                                // ex a02 => AC_Categorie 1 => Tarif cat 1
                                string tgRefCF;
                                for (int i = 1; i < 15; i++)
                                {
                                    tgRefCF = $"a{i.ToString().PadLeft(2, '0')}";
                                    using (SqlCommand cmd = cnx.CreateCommand())
                                    {
                                        cmd.Transaction = transaction;
                                        cmd.CommandText = @"INSERT INTO F_TARIFGAM(AR_Ref, TG_RefCF, AG_No1, AG_No2, TG_Prix, TG_Ref, TG_CodeBarre) 
                                                            VALUES (@arRef,@tgRefCF,@ag1,@ag2,@prix,'','')";
                                        cmd.Parameters.AddWithValue("@arRef", arRef);
                                        cmd.Parameters.AddWithValue("@tgRefCF", tgRefCF);
                                        cmd.Parameters.AddWithValue("@ag1", ag1);
                                        cmd.Parameters.AddWithValue("@ag2", ag2);
                                        switch (i)
                                        {
                                            case 1:
                                                // Tarif TTC
                                                cmd.Parameters.AddWithValue("@prix", nvPxTTC);
                                                break;
                                            case 7:
                                                // Tarif 6
                                                cmd.Parameters.AddWithValue("@prix", nvPxNetAchat * 1.1);
                                                break;
                                            case 14:
                                                // Tarif 13 prix coutant
                                                cmd.Parameters.AddWithValue("@prix", nvPxNetAchat);
                                                break;
                                            default:
                                                cmd.Parameters.AddWithValue("@prix", nvPxVente);
                                                break;
                                        }
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        #endregion

                        #region CONDITIONNEMENT
                        if (isCond)
                        {
                            using (var cmd = cnx.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandText = "UPDATE F_CONDITION SET CO_CodeBarre = '' WHERE CO_CodeBarre = @gencodArt AND CO_No <> @coNo";
                                cmd.Parameters.AddWithValue("@gencodArt", gencodArt);
                                cmd.Parameters.AddWithValue("@arRef", arRef);
                                cmd.Parameters.AddWithValue("@coNo", coNo);
                                cmd.ExecuteNonQuery();

                                cmd.CommandText = "UPDATE F_CONDITION SET EC_Enumere = @enumCond, EC_Quantite = @condQt, CO_CodeBarre = @gencodArt WHERE AR_Ref = @arRef AND CO_No = @coNo";
                                cmd.Parameters.AddWithValue("@enumCond", enumCond);
                                cmd.Parameters.AddWithValue("@condQt", condQt);
                                cmd.ExecuteNonQuery();
                            }

                            if (isPrincipal)
                            {
                                // Attention! Ici on reference la cat tarif par tcRefCF 
                                // ex a01 => AC_Categorie 0 => Tarif TTC
                                // ex a02 => AC_Categorie 1 => Tarif cat 1
                                string tcRefCF;
                                for (int i = 1; i < 15; i++)
                                {
                                    tcRefCF = $"a{i.ToString().PadLeft(2, '0')}";
                                    using (SqlCommand cmd = cnx.CreateCommand())
                                    {
                                        cmd.Transaction = transaction;
                                        cmd.CommandText = "INSERT INTO F_TARIFCOND (AR_Ref, TC_RefCF, CO_No, TC_Prix) VALUES (@arRef, @tcRefCF, @coNo, @prix)";
                                        cmd.Parameters.AddWithValue("@arRef", arRef);
                                        cmd.Parameters.AddWithValue("@tcRefCF", tcRefCF);
                                        cmd.Parameters.AddWithValue("@coNo", coNo);

                                        switch (i)
                                        {
                                            case 1:
                                                // Tarif TTC
                                                cmd.Parameters.AddWithValue("@prix", nvPxTTC);
                                                break;
                                            case 7:
                                                // Tarif 6
                                                cmd.Parameters.AddWithValue("@prix", nvPxNetAchat * 1.1);
                                                break;
                                            case 14:
                                                // Tarif 13 prix coutant
                                                cmd.Parameters.AddWithValue("@prix", nvPxNetAchat);
                                                break;
                                            default:
                                                cmd.Parameters.AddWithValue("@prix", nvPxVente);
                                                break;
                                        }
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }

                        }
                        #endregion

                        transaction.Commit();
                        return "OK";
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            transaction.Rollback();
                            return ex.ToString();
                        }
                        catch (Exception ex2)
                        {
                            return ex2.ToString();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        /// <summary>
        /// V3.2
        /// </summary>
        /// <returns></returns>
        public static string GetBaseSelect() => @"SELECT NULL AS [MAJ],
CASE
  WHEN AE.AE_Ref IS NOT NULL THEN AE.AE_Ref
  WHEN CO.CO_Ref IS NOT NULL THEN CO.CO_Ref
  ELSE A.AR_Ref
END AS [REF MAG],
CASE WHEN A.AR_Sommeil = 1 THEN 'M' ELSE '' END AS SOMMEIL, 
CASE WHEN A.SUPPRIME = 'Oui' THEN 'S' ELSE '' END AS SUPP, 
CASE WHEN A.SUPPRIME_USINE = 'Oui' THEN 'U' ELSE '' END AS [SUPP USINE],
CASE WHEN AF.AF_Principal = 1 THEN 'P' ELSE '' END AS PRINCIPAL,
CASE WHEN ISNULL(S.AS_QteMini, 0) > 0 THEN 'X' ELSE '' END AS [SUIVI MAG],
CASE WHEN A.AR_Publie = 1 THEN 'W' ELSE '' END AS [PUBLIE WEB],
CASE WHEN ISNULL(TG.TG_Ref, '') = '' THEN AF.AF_RefFourniss ELSE TG.TG_Ref END AS [REF FOURN], 
CASE WHEN ISNULL(TG.TG_Ref, '') = '' THEN AF.AF_RefFourniss ELSE TG.TG_Ref END AS [ANC REF FOURN], 
NULL AS [DESIGN FOURN], NULL AS [TARIF TROUVE], A.AR_Design AS [DESIGNATION],
AG1.EG_Enumere AS [GAMME 1], AG2.EG_Enumere AS [GAMME 2],
CO.EC_Enumere AS [ENUM COND], 
CASE WHEN TG.TG_Prix > 0 THEN TG.TG_Prix ELSE AF.AF_PrixAch END AS [ANC PX FOURN],
CASE WHEN TG.TG_Prix > 0 THEN TG.TG_Prix ELSE AF.AF_PrixAch END AS [NV PX FOURN], 
AF.AF_Remise AS [ANC REMISE], AF.AF_Remise AS [NV REMISE], NULL AS [ANC PX ACHAT], NULL AS [NV PX ACHAT],
AF.AF_Colisage AS [COLISAGE], AF.AF_QteMini AS [QEC], AF.AF_ConvDiv AS [CONV ACH], UA.U_Intitule AS [UNITE ACHAT],
AF.AF_Conversion AS [CONV VEN], UV.U_Intitule AS [UNITE VENTE], 
CASE 
  WHEN ISNULL(TG.TG_CodeBarre, '') <> '' THEN TG.TG_CodeBarre
  WHEN ISNULL(CO.CO_No,'') <> '' AND CO.CO_Ref LIKE '%/%' THEN ''
  ELSE AF.AF_CodeBarre END AS [GENCOD FOURN],
CASE 
  WHEN ISNULL(AE.AE_CodeBarre, '') <> '' THEN AE.AE_CodeBarre
  WHEN ISNULL(CO.CO_CodeBarre, '') <> '' THEN CO.CO_CodeBarre
  ELSE A.AR_CodeBarre 
  END AS [GENCOD ART], 
NULL AS [GENCOD COND], ISNULL(CO.EC_Quantite, 1) AS [COND QT],
NULL AS [INFO], A.AR_PoidsNet AS [POIDS],
CASE A.AR_UnitePoids
  WHEN 0 THEN 'Tonne'
  WHEN 1 THEN 'Quintal'
  WHEN 2 THEN 'Kilogramme'
  WHEN 3 THEN 'Gramme'
  WHEN 4 THEN 'Milligramme'
  ELSE NULL
END AS [UNITE POIDS], ECO.NO_RefDet AS [ECO TAXE], ECO.NO_Qte AS [QT ECO TAXE],
CASE WHEN (CASE WHEN ISNULL(AE.AE_PrixAch, 0) > 0 THEN AE.AE_PrixAch ELSE A.AR_PrixAch END * ISNULL(CO.EC_Quantite, 1)) > 0.01 
     THEN (CASE WHEN ISNULL(AE.AE_PrixAch, 0) > 0 THEN AE.AE_PrixAch ELSE A.AR_PrixAch END * ISNULL(CO.EC_Quantite, 1))
     ELSE 0.01 END AS [ANC PX NET ACHAT],
NULL AS [NV PX NET ACHAT], NULL AS [VAR PX NET ACHAT],
A.FA_CodeFamille AS [FAMILLE], 
CASE
  WHEN CO.CO_No IS NOT NULL AND ISNULL(CO.CO_Ref, '') <> '' THEN TC.TC_Prix / (CASE WHEN ((CO.EC_Quantite * A.AR_PrixAch) > 0.01) THEN (CO.EC_Quantite * A.AR_PrixAch) ELSE 0.01 END)
  WHEN TGCT.TG_RefCF IS NOT NULL THEN (TGCT.TG_Prix/CASE WHEN ISNULL(AE.AE_PrixAch, 0) > 0 THEN AE.AE_PrixAch ELSE A.AR_PrixAch END)
  ELSE A.AR_Coef
END AS [ANC COEFF SUR PX NET],
NULL AS [NV COEFF SUR PX NET],
NULL AS [ANC PX VENTE], NULL AS [NV PX VENTE], NULL AS [VAR VENTE], 
AC.[2] AS [REMISE CAT 1], AC.[3] AS [REMISE CAT 2], AC.[4] AS [REMISE CAT 3], AC.[5] AS [REMISE CAT 4], AC.[6] AS [REMISE CAT 5],
AC.[8] AS [REMISE CAT 7], AC.[9] AS [REMISE CAT 8], AC.[10] AS [REMISE CAT 9], AC.[11] AS [REMISE CAT 10], AC.[12] AS [REMISE CAT 11],
AC.[13] AS [REMISE CAT 12], NULL AS [REMISE CAT 13], dbo.GET_TVA_TTC_ARTICLE(A.AR_Ref) AS TVA, NULL AS [ANC PX TTC],
NULL AS [NV PX TTC], NULL AS [COEF T1], NULL AS [COEF T2], NULL AS [COEF T3], NULL AS [COEF T4], NULL AS [COEF T5],NULL AS [COEF T7],
NULL AS [COEF T8], NULL AS [COEF T9], NULL AS [COEF T10], NULL AS [COEF T11],NULL AS [COEF T12], ISNULL(CO.CO_No, 0), 
ISNULL(AG1.AG_No,0) AS [AG1], ISNULL(AG2.AG_No, 0) AS [AG2], A.AR_Ref, AF.AF_RefFourniss AS [REF FOURN BASE],
ISNULL(CO.CO_Principal,0) AS CO_Principal, NULL AS [MAJ ARTICLE]
FROM F_ARTICLE A
LEFT JOIN F_ARTFOURNISS AF ON AF.AR_Ref = A.AR_Ref
LEFT JOIN F_ARTSTOCK S ON A.AR_Ref = S.AR_Ref AND S.AS_Principal = 1
LEFT JOIN P_UNITE AS UA ON UA.cbIndice = AF.AF_Unite 
LEFT JOIN P_UNITE AS UV ON UV.cbIndice = A.AR_UniteVen 
LEFT JOIN (
  SELECT * 
  FROM (
    SELECT AR_Ref, AC_Categorie, CASE WHEN AC_Remise = 0 THEN NULL ELSE AC_Remise END AS AC_Remise 
    FROM F_ARTCLIENT) AC PIVOT (MAX(AC_Remise) FOR AC_Categorie IN([2], [3], [4], [5], [6], [8], [9], [10], [11], [12], [13])) P
) AC ON AC.AR_Ref = A.AR_Ref
LEFT JOIN F_NOMENCLAT ECO ON ECO.AR_Ref = A.AR_Ref AND NO_RefDet IN('ECO-TAXE','ECO_TAXE_MOBILIER','TGAP','TGAP_2')
LEFT JOIN F_CONDITION CO ON CO.AR_Ref = A.AR_Ref
LEFT JOIN F_TARIFCOND TC ON TC.AR_Ref = A.AR_Ref AND TC.CO_No = CO.CO_No AND TC.TC_RefCF = 'a13'
LEFT JOIN F_ARTGAMME AG1 ON AG1.AR_Ref = A.AR_Ref AND AG1.AG_Type = 0
LEFT JOIN F_ARTGAMME AG2 ON AG2.AR_Ref = A.AR_Ref AND AG2.AG_Type = 1
LEFT JOIN F_ARTENUMREF AE ON AE.AG_No1 = AG1.AG_No AND AE.AG_No2 = ISNULL(AG2.AG_No,0)
LEFT JOIN F_TARIFGAM TG ON TG.AG_No1 = AG1.AG_No AND TG.AG_No2 = ISNULL(AG2.AG_No, 0) AND TG.TG_RefCF = AF.CT_Num
LEFT JOIN F_TARIFGAM TGCT ON TGCT.AG_No1 = AG1.AG_No AND TGCT.AG_No2 = ISNULL(AG2.AG_No, 0) AND TGCT.TG_RefCF = 'a13'
WHERE AF.CT_Num = '{0}' {1} {2}
ORDER BY A.AR_Ref ASC, AG1.EG_Enumere ASC, AG2.EG_Enumere ASC, CO.CO_Principal DESC;";
    }
}
