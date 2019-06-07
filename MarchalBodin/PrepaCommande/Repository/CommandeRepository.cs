using MBCore.Model;
using Objets100cLib;
using PrepaCommande.Model;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ExcelApp = Microsoft.Office.Interop.Excel;

namespace PrepaCommande.Repository
{
    public class CommandeRepository : BaseCialAbstract
    {
        private Logistique.Repository.LogistiqueRepository logistiqueRepos = new Logistique.Repository.LogistiqueRepository();
        private static IBODocumentAchat3 _OMDocument;
        private static Commande _Commande;

        public Commande GetCommande(string DoPiece)
        {
            if(openBaseCial())
            {
                var Doc = GetDocument(DoPiece);
                if (Doc == null)
                {
                    throw new Exception($"Document {DoPiece} non trouvé");
                }

                if (Doc.DO_Type != DocumentType.DocumentTypeAchatCommande)
                {
                    Doc.Read();
                    throw new Exception("Le document doit être un APC.");
                }

                foreach (IBODocumentAchatLigne3 ligne in Doc.FactoryDocumentLigne.List)
                {
                    if (ligne.FactoryDocumentLigneLienCM.List.Count > 0)
                    {
                        Doc.Read();
                        throw new Exception("Le document ne doit pas contenir de contremarque.");
                    }
                }

                _OMDocument = (IBODocumentAchat3) Doc;
                //_OMDocument = GetInstance().FactoryDocumentAchat.ReadPiece(DocumentType.DocumentTypeAchatCommande, DoPiece);
                //_OMDocument.CouldModified();
            }

            using (var cnx = new SqlConnection(cnxString))
            {
                cnx.Open();

                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = @"SELECT AF.CT_Num, CT.FRANCO, CT.CT_Intitule, AG1.AG_No AS AG_No1, AG2.AG_No AS AG_No2, A.AR_PrixAch,
                            A.AR_Ref, AE.AE_Ref, AG1.EG_Enumere AS Gamme1, AG2.EG_Enumere AS Gamme2,
                            (SELECT SUM(DL_Qte) FROM F_DOCLIGNE WHERE AR_Ref = A.AR_Ref AND DO_Type = 7 AND MONTH(DL_DateBL) = MONTH(GETDATE()) AND YEAR(DL_DateBL) = YEAR(GETDATE())-1) AS Stat,
                            DL.DL_Qte, DL.EU_Qte, A.AR_Sommeil, A.SUPPRIME, A.SUPPRIME_USINE, A.AR_Design, A.AR_CodeBarre,
                            CASE WHEN ISNULL(TG.TG_Ref, '') = '' THEN AF.AF_RefFourniss ELSE TG.TG_Ref END AS RefFourn,
                            AF.AF_Colisage, AF.AF_QteMini, D.DP_Code, AC.AC_PrixVen, UA.U_Intitule AS UniteAch,
                            ISNULL(S.AS_QteMini, 0) AS AS_QteMini, ISNULL(S.AS_QteMaxi, 0) AS AS_QteMaxi,
                            CASE WHEN AG1.EG_Enumere IS NOT NULL THEN ISNULL(GS.GS_QteSto, 0) ELSE ISNULL(S.AS_QteSto, 0) END AS QteSto, 
                            CASE WHEN AG1.EG_Enumere IS NOT NULL THEN ISNULL(GS.GS_QteCom, 0) ELSE ISNULL(S.AS_QteCom, 0) END AS QteCom, 
                            CASE WHEN AG1.EG_Enumere IS NOT NULL THEN ISNULL(GS.GS_QteRes, 0) ELSE ISNULL(S.AS_QteRes, 0) END AS QteRes, 
                            UV.U_Intitule AS UniteVen,
                            CASE WHEN ISNULL(S.AS_QteMini, 0) = 0 OR A.AR_Sommeil = 1 OR A.SUPPRIME LIKE 'oui' OR A.SUPPRIME_USINE LIKE 'oui' THEN 0 ELSE 1 END AS OBS
                            FROM F_ARTICLE A 
                            LEFT JOIN F_ARTGAMME AG1 ON AG1.AR_Ref = A.AR_Ref AND AG1.AG_Type = 0
                            LEFT JOIN F_ARTGAMME AG2 ON AG2.AR_Ref = A.AR_Ref AND AG2.AG_Type = 1
                            LEFT JOIN F_ARTENUMREF AE ON AE.AG_No1 = AG1.AG_No AND AE.AG_No2 = ISNULL(AG2.AG_No, 0)
                            JOIN F_ARTFOURNISS AF ON AF.AR_Ref = A.AR_Ref 
                            JOIN F_COMPTET CT ON CT.CT_Num = AF.CT_Num
                            LEFT JOIN F_TARIFGAM TG ON TG.AG_No1 = AG1.AG_No AND TG.AG_No2 = ISNULL(AG2.AG_No, 0) AND TG.TG_RefCF = AF.CT_Num
                            LEFT JOIN F_ARTSTOCK S ON S.AR_Ref = A.AR_Ref 
                            LEFT JOIN F_GAMSTOCK GS ON GS.AR_Ref = A.AR_Ref AND GS.AG_No1 = AG1.AG_No AND GS.AG_No2 = ISNULL(AG2.AG_No, 0)
                            JOIN P_UNITE AS UV ON UV.cbIndice = A.AR_UniteVen 
                            JOIN P_UNITE AS UA ON UA.cbIndice = AF.AF_Unite
                            JOIN F_ARTCLIENT AC ON AC.AR_Ref = A.AR_Ref AND AC.AC_Categorie = 1 
                            LEFT JOIN F_DEPOTEMPL D ON S.DE_No = D.DE_No AND S.DP_NoPrincipal = D.DP_No 
                            LEFT JOIN F_DOCLIGNE DL ON DL.AR_Ref = A.AR_Ref AND DL.DO_Piece = @doPiece 
                            WHERE AF.CT_Num = (SELECT DO_Tiers FROM F_DOCENTETE WHERE DO_Piece = @doPiece)
                            AND AF.AF_Principal = 1 
                            ORDER BY A.AR_Ref, Gamme1, Gamme2";
                    cmd.Parameters.AddWithValue("@doPiece", DoPiece);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            throw new Exception("Aucun résultat");
                        }

                       _Commande = new Commande();
                        while (reader.Read())
                        {
                            if(_Commande.CtNum == null)
                            {
                                _Commande.CtNum = reader["CT_Num"].ToString();
                                _Commande.Fournisseur = reader["CT_Intitule"].ToString();
                                _Commande.Piece = DoPiece;
                                _Commande.Franco = reader["FRANCO"].ToString();
                            }

                            var ligne = new LigneCommande() {
                                ArRef = (string)reader["AR_Ref"],
                                AeRef = (reader["AE_Ref"] == DBNull.Value) ? null : (string)reader["AE_Ref"],
                                Gamme1 = (reader["Gamme1"] == DBNull.Value) ? null : (string)reader["Gamme1"],
                                Gamme2 = (reader["Gamme2"] == DBNull.Value) ? null : (string)reader["Gamme2"],
                                AGNo1 = (reader["AG_No1"] == DBNull.Value) ? (int?)null : (int)reader["AG_No1"],
                                AGNo2 = (reader["AG_No2"] == DBNull.Value) ? (int?)null : (int)reader["AG_No2"],
                                Stat = double.Parse(reader["Stat"] == DBNull.Value ? "0" : reader["Stat"].ToString()),
                                Sommeil = (Int16)reader["AR_Sommeil"] == 1,
                                Supprime = (string)reader["SUPPRIME"] == "Oui",
                                SupprimeUsine = (string)reader["SUPPRIME_USINE"] == "Oui",
                                RefFourn = (string)reader["RefFourn"],
                                Designation = (string)reader["AR_Design"],
                                UniteVente = (string)reader["UniteVen"],
                                UniteAchat = (string)reader["UniteAch"],
                                Colisage = double.Parse(reader["AF_Colisage"].ToString()),
                                Qec = double.Parse(reader["AF_QteMini"].ToString()),
                                StockMin = double.Parse(reader["AS_QteMini"].ToString()),
                                StockMax = double.Parse(reader["AS_QteMaxi"].ToString()),
                                pxTTC = double.Parse(reader["AC_PrixVen"].ToString()),
                                Qte = (reader["DL_Qte"] == DBNull.Value) ? (double?)null : double.Parse(reader["DL_Qte"].ToString()),
                                CodeBarre = reader["AR_CodeBarre"].ToString(),
                                Emplacement = reader["DP_Code"].ToString(),
                                Stock = (reader["QteSto"] == DBNull.Value) ? 0 : double.Parse(reader["QteSto"].ToString()),
                                QteCom = double.Parse(reader["QteCom"].ToString()),
                                QteRes = double.Parse(reader["QteRes"].ToString()),
                                ArPrixAch = double.Parse(reader["AR_PrixAch"].ToString()),
                            };
                            ligne.PropertyChanged += Ligne_PropertyChanged;
                            _Commande.Lignes.Add(ligne);
                        }

                        return _Commande;
                    }
                }
            }
        }

        private void Ligne_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _Commande.Update();
            if (e.PropertyName == "Qte")
            {
                SaveLigne(sender as LigneCommande);
            }

            if (e.PropertyName == "Stock")
            {
                var ligne = (LigneCommande)sender;
                logistiqueRepos.DeclareStock(
                    ligne.Stock,
                    new Logistique.Model.Article() { ArRef = ligne.ArRef }
                    );
            }
        }

        internal void SaveLigne(LigneCommande ligne)
        {
            try
            {
                if (!openBaseCial())
                {
                    throw new Exception("Connexion base impossible, aucun enregistrement ne sera fait.");
                }

                var found = false;
                foreach (IBODocumentAchatLigne3 docLigne in _OMDocument.FactoryDocumentLigne.List)
                {
                    if (docLigne.Article == null)
                    {
                        continue;
                    }
                    // La ligne existe déjà dans le doc
                    if (docLigne.Article.AR_Ref == ligne.ArRef)
                    {
                        if (docLigne.ArticleGammeEnum1 != null && docLigne.ArticleGammeEnum1.EG_Enumere != ligne.Gamme1)
                        {
                            continue;
                        }
                        if (docLigne.ArticleGammeEnum2 != null && docLigne.ArticleGammeEnum2.EG_Enumere == ligne.Gamme2)
                        {
                            continue;
                        }

                        found = true;

                        if (ligne.Qte == null || ligne.Qte == 0)
                        {
                            docLigne.Remove();
                            break;
                        }
                        docLigne.DL_Qte = (double)ligne.Qte;
                        docLigne.EU_Qte = (double)ligne.Qte;
                        // Set defaut remise car visiblement ça ne se fait pas via write default
                        docLigne.SetDefaultRemise();
                        docLigne.WriteDefault();
                        updateLigneFromIboLigne(ligne, docLigne);
                        break;
                    }
                }
                if (ligne.Qte == null || ligne.Qte == 0)
                {
                    return;
                }
                // Il faut créer la ligne
                if (!found)
                {
                    var nl = (IBODocumentAchatLigne3)_OMDocument.FactoryDocumentLigne.Create();
                    IBOArticle3 a = GetInstance().FactoryArticle.ReadReference(ligne.ArRef);

                    if (ligne.Gamme2 != null)
                    {
                        // Article double gamme
                        nl.SetDefaultArticleDoubleGamme(
                            a.FactoryArticleGammeEnum1.ReadEnumere(ligne.Gamme1),
                            a.FactoryArticleGammeEnum2.ReadEnumere(ligne.Gamme2),
                            (double)ligne.Qte);
                    }
                    else if (ligne.Gamme1 != null)
                    {
                        // Article mono gamme
                        nl.SetDefaultArticleMonoGamme(
                            a.FactoryArticleGammeEnum1.ReadEnumere(ligne.Gamme1),
                            (double)ligne.Qte);
                    }
                    else
                    {
                        // Article normal
                        nl.SetDefaultArticle(
                            GetInstance().FactoryArticle.ReadReference(ligne.ArRef),
                            (double)ligne.Qte);
                    }
                    // Set defaut remise car visiblement ça ne se fait pas via write default
                    nl.SetDefaultRemise();
                    nl.WriteDefault();
                    updateLigneFromIboLigne(ligne, nl);
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show($"{e.Message} Veuillez relancer le programme.");
            }
        }

        private void updateLigneFromIboLigne(LigneCommande ligne, IBODocumentAchatLigne3 IBOLigne)
        {
            IBOLigne.Refresh();
            if (IBOLigne.ArticleGammeEnum2 != null)
            {
                ligne.QteCom = IBOLigne.Article.StockCommandeDoubleGamme(IBOLigne.ArticleGammeEnum1, IBOLigne.ArticleGammeEnum2);
                ligne.QteRes = IBOLigne.Article.StockReserveContremarqueDoubleGamme(IBOLigne.ArticleGammeEnum1, IBOLigne.ArticleGammeEnum2);
            }
            else if (IBOLigne.ArticleGammeEnum1 != null)
            {
                ligne.QteCom = IBOLigne.Article.StockCommandeMonoGamme(IBOLigne.ArticleGammeEnum1);
                ligne.QteRes = IBOLigne.Article.StockReserveContremarqueMonoGamme(IBOLigne.ArticleGammeEnum1);
            }
            else
            {
                ligne.QteCom = IBOLigne.Article.StockCommande();
                ligne.QteRes = IBOLigne.Article.StockReserve();
            }
        }

        internal bool SaveAll(Commande commande)
        {
            try
            {
                if (!openBaseCial())
                {
                    throw new Exception("Connexion base impossible, aucun enregistrement ne sera fait. Veuillez relancer le programme.");
                }

                foreach (LigneCommande ligne in commande.Lignes)
                {
                    SaveLigne(ligne);
                }

                return true;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
                return false;
            }
        }

        internal DataTable GetRefByGencod(string gencod)
        {
            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();

                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = @"SELECT ISNULL(AE.AE_Ref, A.AR_Ref) AS Ref
                        FROM F_ARTICLE A
                        LEFT JOIN F_ARTGAMME AG1 ON AG1.AR_Ref = A.AR_Ref AND AG1.AG_Type = 0
                        LEFT JOIN F_ARTGAMME AG2 ON AG2.AR_Ref = A.AR_Ref AND AG2.AG_Type = 1
                        LEFT JOIN F_ARTENUMREF AE ON AE.AG_No1 = AG1.AG_No AND AE.AG_No2 = ISNULL(AG2.AG_No, 0)
                        JOIN F_ARTFOURNISS AF ON AF.AR_Ref = A.AR_Ref 
                        LEFT JOIN F_TARIFGAM TG ON TG.AG_No1 = AG1.AG_No AND TG.AG_No2 = ISNULL(AG2.AG_No, 0) AND TG.TG_RefCF = AF.CT_Num
                        LEFT JOIN F_CONDITION CO ON CO.AR_Ref = A.AR_Ref 
                        WHERE A.AR_CodeBarre = @gencod
                        OR AF.AF_CodeBarre = @gencod
                        OR AE.AE_CodeBarre = @gencod
                        OR TG.TG_CodeBarre = @gencod
                        OR CO.CO_CodeBarre = @gencod";

                    cmd.Parameters.AddWithValue("@gencod", gencod);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        return dt;
                    }
                }
            }
        }

        internal bool Import(Commande cmd)
        {
            string path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string file = $"{path}\\{dbName}_{cmd.Piece}.xlsm";
            
            if (!File.Exists(file))
            {
                throw new Exception($"Fichier {file} non trouvé");
            }
            ExcelApp.Application app = new ExcelApp.Application();
            ExcelApp.Workbook workbook = app.Workbooks.Open(file);
            try
            {
                ExcelApp.Worksheet worksheet = workbook.Worksheets["Commande"];
                ExcelApp.ListObject listObject = worksheet.ListObjects["tableCommande"];
                foreach (ExcelApp.ListRow row in listObject.ListRows)
                {
                    try
                    {
                        string Ref = (string)row.Range[2].Value();
                        double? qt = (double?)row.Range[3].Value();
                        double stock = ((double?)row.Range[10].Value())??0;
                        Debug.Print($"{Ref} - {qt} - {stock}");

                        LigneCommande ligne = cmd.Lignes.Where(l => l.Ref == Ref).FirstOrDefault();
                        if (ligne == null)
                        {
                            continue;
                        }
                        if (qt != ligne.Qte)
                        {
                            ligne.Qte = qt;
                        }
                        if (stock != ligne.Stock)
                        {
                            ligne.Stock = stock;
                        }
                    }
                    catch (Exception)
                    {
                        // TODO Log
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (workbook != null)
                {
                    workbook.Close(false);
                }
                app.Quit();
            }
        }

        public bool DocumentIsClosed(string DoPiece)
        {
            if (openBaseCial())
            {
                try
                {
                    _OMDocument.CouldModified();
                }
                catch (Exception)
                {
                    throw new Exception($"Veuillez fermer le document {DoPiece}");
                }
            }

            return true;
        }

        internal void CloseDocument()
        {
            if (_OMDocument != null)
            {
                _OMDocument.Read();
            }
            closeBaseCial();
        }
    }
}
