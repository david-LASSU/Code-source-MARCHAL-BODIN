using MBCore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Objets100cLib;
using Logistique.Model;
using System.Collections.ObjectModel;
using System.Data.SqlClient;

namespace Logistique.Repository
{
    public class LogistiqueRepository : BaseCialAbstract
    {
        public IBODepotEmplacement GetEmplacement(string dpCode)
        {
            if (!openBaseCial())
            {
                throw new Exception("Impossible de se connecter à la base");
            }
            if (!GetInstance().DepotPrincipal.FactoryDepotEmplacement.ExistCode(dpCode))
            {
                throw new Exception($"Emplacement {dpCode} non trouvé");
            }

            return GetInstance().DepotPrincipal.FactoryDepotEmplacement.ReadCode(dpCode);
        }

        internal Collection<Article> GetArticles(IBODepotEmplacement emplacement)
        {
            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();

                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = @"SELECT A.AR_Ref, A.AR_Design, A.AR_Ref, AE.AE_Ref, AG1.EG_Enumere AS Gamme1, AG2.EG_Enumere AS Gamme2,
                            CASE WHEN AG1.EG_Enumere IS NOT NULL THEN ISNULL(GS.GS_QteSto, 0) ELSE ISNULL(S.AS_QteSto, 0) END AS QteSto, 
                            CASE WHEN AG1.EG_Enumere IS NOT NULL THEN ISNULL(GS.GS_QteCom, 0) ELSE ISNULL(S.AS_QteCom, 0) END AS QteCom, 
                            CASE WHEN AG1.EG_Enumere IS NOT NULL THEN ISNULL(GS.GS_QteRes, 0) ELSE ISNULL(S.AS_QteRes, 0) END AS QteRes
                            FROM F_ARTICLE A
                            LEFT JOIN F_ARTGAMME AG1 ON AG1.AR_Ref = A.AR_Ref AND AG1.AG_Type = 0
                            LEFT JOIN F_ARTGAMME AG2 ON AG2.AR_Ref = A.AR_Ref AND AG2.AG_Type = 1
                            LEFT JOIN F_ARTENUMREF AE ON AE.AG_No1 = AG1.AG_No AND AE.AG_No2 = ISNULL(AG2.AG_No, 0)
                            LEFT JOIN F_ARTSTOCK S ON S.AR_Ref = A.AR_Ref 
                            LEFT JOIN F_GAMSTOCK GS ON GS.AR_Ref = A.AR_Ref AND GS.AG_No1 = AG1.AG_No AND GS.AG_No2 = ISNULL(AG2.AG_No, 0)
                            JOIN P_UNITE AS UV ON UV.cbIndice = A.AR_UniteVen 
                            JOIN F_ARTCLIENT AC ON AC.AR_Ref = A.AR_Ref AND AC.AC_Categorie = 1 
                            LEFT JOIN F_DEPOTEMPL DP ON S.DE_No = DP.DE_No AND S.DP_NoPrincipal = DP.DP_No 
                            JOIN F_DEPOT DE ON DE.DE_No = S.DE_No AND DE.DE_Code = @deCode
                            WHERE DP.DP_Code = @dpCode";
                    cmd.Parameters.AddWithValue("@deCode", emplacement.Depot.DE_Code);
                    cmd.Parameters.AddWithValue("@dpCode", emplacement.DP_Code);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        Collection<Article> list = new Collection<Article>();
                        while (reader.Read())
                        {
                            list.Add(new Article() {
                                ArRef = (string)reader["AR_Ref"],
                                AeRef = (reader["AE_Ref"] == DBNull.Value) ? null : (string)reader["AE_Ref"],
                                Gamme1 = (reader["Gamme1"] == DBNull.Value) ? null : (string)reader["Gamme1"],
                                Gamme2 = (reader["Gamme2"] == DBNull.Value) ? null : (string)reader["Gamme2"],
                                Designation = reader["AR_Design"].ToString(),
                                Stock = double.Parse(reader["QteSto"].ToString())
                            });
                        }

                        return list;
                    }
                }
            }
        }

        internal IBOArticle3 SetDefaultEmpl(string str, IBODepotEmplacement empl)
        {
            if(!openBaseCial())
            {
                throw new Exception("Impossible de se connecter à la base");
            }

            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();
                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = @"SELECT DISTINCT A.AR_Ref, AE.AE_Ref, CO.CO_Ref, AG1.EG_Enumere AS Gamme1, AG2.EG_Enumere AS Gamme2
                                        FROM F_ARTICLE A
                                        LEFT JOIN F_ARTGAMME AG1 ON AG1.AR_Ref = A.AR_Ref AND AG1.AG_Type = 0
                                        LEFT JOIN F_ARTGAMME AG2 ON AG2.AR_Ref = A.AR_Ref AND AG2.AG_Type = 1
                                        LEFT JOIN F_ARTENUMREF AE ON AE.AG_No1 = AG1.AG_No AND AE.AG_No2 = ISNULL(AG2.AG_No, 0)
                                        JOIN F_ARTFOURNISS AF ON AF.AR_Ref = A.AR_Ref 
                                        LEFT JOIN F_TARIFGAM TG ON TG.AG_No1 = AG1.AG_No AND TG.AG_No2 = ISNULL(AG2.AG_No, 0) AND TG.TG_RefCF = AF.CT_Num
                                        LEFT JOIN F_CONDITION CO ON CO.AR_Ref = A.AR_Ref 
                                        WHERE A.AR_Ref = @str
                                        OR AE.AE_Ref = @str
                                        OR CO.CO_Ref = @str
                                        OR A.AR_CodeBarre = @str
                                        OR AF.AF_CodeBarre = @str
                                        OR AE.AE_CodeBarre = @str
                                        OR TG.TG_CodeBarre = @str
                                        OR CO.CO_CodeBarre = @str";
                    cmd.Parameters.AddWithValue("@str", str);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            throw new Exception("Article non trouvé");
                        }

                        reader.Read();
                        IBOArticle3 article = GetInstance().FactoryArticle.ReadReference(reader["AR_Ref"].ToString());

                        if (article.ArticleDepotPrincipal == null)
                        {
                            IBOArticleDepot3 ad = (IBOArticleDepot3)article.FactoryArticleDepot.Create();
                            ad.Depot = empl.Depot;
                            ad.EmplacementPrincipal = empl;
                            ad.WriteDefault();

                            article.Refresh();
                        }
                        else
                        {
                            article.ArticleDepotPrincipal.EmplacementPrincipal = empl;
                            article.ArticleDepotPrincipal.Write();
                            article.Refresh();
                        }

                        return article;
                    }
                }
            }
        }

        public Article DeclareStock(double noteStock, Article article)
        {
            IBOArticle3 OMArticle = GetInstance().FactoryArticle.ReadReference(article.ArRef);
            double stockReel;
            double mouvQt;
            IBODocument3 dayDocIn = null, dayDocOut = null;

            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();
                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = @"SELECT DO_Piece, DO_Type FROM F_DOCENTETE 
                        WHERE DO_Date = CAST(CURRENT_TIMESTAMP AS DATE) AND DO_Ref = 'REGUL STOCK' AND DO_Type IN (20,21)";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                short doType = (short)reader["DO_Type"];
                                string doPiece = (string)reader["DO_Piece"];

                                switch (doType)
                                {
                                    case 20:
                                        dayDocIn = GetInstance().FactoryDocumentStock.ReadPiece(DocumentType.DocumentTypeStockMouvIn, doPiece);
                                        dayDocIn.CouldModified();
                                        removeLigneFromDoc(dayDocIn, OMArticle, article.Gamme1, article.Gamme2);
                                        break;
                                    case 21:
                                        dayDocOut = GetInstance().FactoryDocumentStock.ReadPiece(DocumentType.DocumentTypeStockMouvOut, doPiece);
                                        dayDocOut.CouldModified();
                                        removeLigneFromDoc(dayDocOut, OMArticle, article.Gamme1, article.Gamme2);
                                        break;
                                    default:
                                        throw new Exception($"Type {doType} non pris en charge");
                                }
                            }
                        }
                    }
                }

                stockReel = getStockReel(OMArticle, article.Gamme1, article.Gamme2);

                // Mouvement d'entrée
                if (noteStock > stockReel)
                {
                    mouvQt = noteStock - stockReel;
                    if (dayDocIn == null)
                    {
                        IPMDocument procDocIn = GetInstance().CreateProcess_Document(DocumentType.DocumentTypeStockMouvIn);
                        dayDocIn = procDocIn.Document;
                        procDocIn.Document.DO_Ref = "REGUL STOCK";
                        addArticleToLigne((IBODocumentLigne3)dayDocIn.FactoryDocumentLigne.Create(), OMArticle, article.Gamme1, article.Gamme2, mouvQt).WriteDefault();
                        procDocIn.Process();
                    }
                    addArticleToLigne((IBODocumentLigne3)dayDocIn.FactoryDocumentLigne.Create(), OMArticle, article.Gamme1, article.Gamme2, mouvQt).WriteDefault();
                }

                // Mouvement de sortie
                if (noteStock < stockReel)
                {
                    mouvQt = stockReel - noteStock;
                    if (dayDocOut == null)
                    {
                        IPMDocument procDocOut = GetInstance().CreateProcess_Document(DocumentType.DocumentTypeStockMouvOut);
                        dayDocOut = procDocOut.Document;
                        procDocOut.Document.DO_Ref = "REGUL STOCK";
                        addArticleToLigne((IBODocumentLigne3)dayDocOut.FactoryDocumentLigne.Create(), OMArticle, article.Gamme1, article.Gamme2, mouvQt).WriteDefault();
                        procDocOut.Process();
                    }
                    addArticleToLigne((IBODocumentLigne3)dayDocOut.FactoryDocumentLigne.Create(), OMArticle, article.Gamme1, article.Gamme2, mouvQt).WriteDefault();
                }
            }

            article.Stock = getStockReel(OMArticle, article.Gamme1, article.Gamme2);
            return article;
        }

        private void removeLigneFromDoc(IBODocument3 doc, IBOArticle3 article, string gamme1, string gamme2)
        {
            foreach (IBODocumentLigne3 ligne in doc.FactoryDocumentLigne.List)
            {
                if (ligne.Article == null)
                {
                    continue;
                }
                if (gamme2 != null
                    && ligne.Article == article
                    && (ligne.ArticleGammeEnum1 != null && ligne.ArticleGammeEnum1.EG_Enumere == gamme1)
                    && (ligne.ArticleGammeEnum2 != null && ligne.ArticleGammeEnum2.EG_Enumere == gamme2))
                {
                    ligne.Remove();
                    return;
                }
                if (gamme1 != null
                    && ligne.Article == article
                    && (ligne.ArticleGammeEnum1 != null && ligne.ArticleGammeEnum1.EG_Enumere == gamme1))
                {
                    ligne.Remove();
                    return;
                }
                if (ligne.Article == article)
                {
                    ligne.Remove();
                    return;
                }
            }

        }

        private IBODocumentLigne3 addArticleToLigne(IBODocumentLigne3 ligne, IBOArticle3 article, string gamme1, string gamme2, double qt)
        {
            if (gamme2 != null)
            {
                ligne.SetDefaultArticleDoubleGamme(
                    article.FactoryArticleGammeEnum1.ReadEnumere(gamme1),
                    article.FactoryArticleGammeEnum2.ReadEnumere(gamme2),
                    qt
                );
                return ligne;
            }
            if (gamme1 != null)
            {
                ligne.SetDefaultArticleMonoGamme(
                    article.FactoryArticleGammeEnum1.ReadEnumere(gamme1),
                    qt
                );
                return ligne;
            }
            ligne.SetDefaultArticle(article, qt);
            return ligne;
        }

        private double getStockReel(IBOArticle3 article, string gamme1, string gamme2)
        {
            if (gamme2 != null)
            {
                return article.StockReelDoubleGamme(
                    article.FactoryArticleGammeEnum1.ReadEnumere(gamme1),
                    article.FactoryArticleGammeEnum2.ReadEnumere(gamme2));
            }
            if (gamme1 != null)
            {
                return article.StockReelMonoGamme(article.FactoryArticleGammeEnum1.ReadEnumere(gamme1));
            }
            return article.StockReel();
        }
    }
}
