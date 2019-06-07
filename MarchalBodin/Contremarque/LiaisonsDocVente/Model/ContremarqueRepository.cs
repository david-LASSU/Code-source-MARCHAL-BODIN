using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using Objets100cLib;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using MBCore.Model;
using EnvoyerCommande;
using System.Diagnostics;
using Divers.Model;

namespace LiaisonsDocVente.Model
{
    public class ContremarqueRepository : BaseCialAbstract
    {
        public event LogEventHandler Log;
        public delegate void LogEventHandler(string message);
        private List<string> fournList = new List<string> { "Principal" };
        public List<string> FournList => fournList;
        public ContremarqueRepository()
        {
            fournList.AddRange(from Database db in getDbListFromContext() where db.name != dbName && db.isMag == true select db.name);
        }
        public Collection<Contremarque> getAll(string doPiece)
        {
            Collection<Contremarque> list = new Collection<Contremarque>();

            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();
                
                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = @"SELECT A.FA_CodeFamille, P.U_Intitule, DLV.DO_Type, DLV.AR_Ref, ISNULL(AE.AE_Ref, DLV.AR_Ref) AS RefMag, DLV.DL_Design, DLV.DL_Qte, AF.CT_Num, 
                                        DLA.DO_Piece, DLV.AF_RefFourniss, DLV.DL_No, DT.DT_Text, DLV.AG_No1, DLV.AG_No2, ISNULL(A.AR_Nomencl,0) AS AR_Nomencl,
                                        CASE WHEN CM.CM_Qte IS NULL THEN DLV.DL_Qte ELSE CM.CM_Qte END AS QTE,
                                        CASE 
                                            WHEN DLA.CT_Num = AF.CT_Num OR DLA.CT_Num IS NULL THEN 'Principal'
                                            ELSE CT.CT_Classement
                                        END AS Fournisseur,
                                        CASE WHEN DLA.DO_Piece IS NOT NULL THEN 1 ELSE 0 END AS rowChecked,
                                        (SELECT ISNULL(SUM(QteDispo.val), 0) AS QteDispo
                                            FROM F_DOCLIGNE DL
                                            JOIN F_COMPTET CT ON CT.CT_Num = DL.CT_Num
                                            JOIN F_DOCENTETE DE ON DE.DO_Piece = DL.DO_Piece
                                            LEFT JOIN F_CMLIEN CM ON CM.DL_NoIn = DL.DL_No
                                            CROSS APPLY (SELECT DL_QteBL - ISNULL(CM_Qte, 0)) AS QteDispo(val)
                                            WHERE DL.DO_Domaine = 1 
                                            AND DL.DO_Type IN(12)
                                            AND DE.DO_Statut IN (1,2)
                                            AND QteDispo.val > 0
                                            AND AR_Ref = DLV.AR_Ref AND AG_No1 = DLV.AG_No1 AND AG_No2 = DLV.AG_No2
                                            ) AS QteDispo
                                        FROM F_DOCLIGNE DLV
                                        LEFT JOIN F_CMLIEN CM ON CM.DL_NoOut = DLV.DL_No
                                        LEFT JOIN F_DOCLIGNE DLA ON DLA.DL_No = CM.DL_NoIn
                                        LEFT JOIN F_ARTENUMREF AE ON AE.AG_No1 = DLV.AG_No1 AND AE.AG_No2 = DLV.AG_No2
                                        LEFT JOIN F_ARTFOURNISS AF ON AF.AR_Ref = DLV.AR_Ref AND AF.AF_Principal = 1
                                        LEFT JOIN F_COMPTET CT ON CT.CT_Num = DLA.CT_Num
                                        LEFT JOIN F_DOCLIGNETEXT DT ON DT.DT_No = DLV.DT_No
                                        LEFT JOIN F_ARTICLE A ON A.AR_Ref = DLV.AR_Ref
                                        LEFT JOIN P_UNITE P ON P.cbMarq = A.AR_UniteVen
                                        WHERE DLV.DO_Piece = @doPiece
                                        AND (AF.CT_Num IS NOT NULL 
                                                OR A.FA_CodeFamille = 'UNIQUE'
                                                OR DLV.AR_Ref IN ('DIVERS', 'PT', 'RI') 
                                                OR (DLV.AR_Ref IS NULL AND DLV.DL_Design <> ''))
                                        AND CM.cbMarq IS NULL
                                        ORDER BY DLV.DL_Ligne";

                    cmd.Parameters.AddWithValue("@doPiece", doPiece);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            string currentFourn = string.Empty;
                            while (reader.Read())
                            {
                                string ctNum = reader["CT_Num"].ToString();
                                // Permet d'assigner une proposition de fournisseur
                                // aux faux articles
                                if (!string.IsNullOrEmpty(ctNum))
                                {
                                    currentFourn = ctNum;
                                }

                                Contremarque ct = new Contremarque();

                                ct.Type = (short)reader["DO_Type"];
                                ct.DlNo = (int)reader["DL_No"];
                                ct.NumPiece = reader["DO_Piece"].ToString();
                                ct.RowChecked = (int)reader["rowChecked"] == 1;
                                ct.RefMag = reader["RefMag"].ToString();
                                ct.ArRef = reader["AR_Ref"].ToString();
                                ct.RefFourn = reader["AF_RefFourniss"].ToString();
                                ct.Design = reader["DL_Design"].ToString();
                                ct.QteOrigin = double.Parse(reader["QTE"].ToString());
                                ct.Qte = double.Parse(reader["QTE"].ToString());
                                ct.Unite = reader["U_Intitule"].ToString();
                                ct.SelectedFourn = reader["Fournisseur"].ToString();
                                ct.FournPrinc = currentFourn;
                                ct.FournList = FournList;
                                ct.Liaisons = new Collection<LiaisonCde>();
                                ct.QteDispo = double.Parse(reader["QteDispo"].ToString());
                                ct.ArNomencl = (short)reader["AR_Nomencl"];
                                ct.AgNo1 = (int)reader["AG_No1"];
                                ct.AgNo2 = (int)reader["AG_No2"];
                                ct.CodeFamille = reader["FA_CodeFamille"] == DBNull.Value ? null : (string)reader["FA_CodeFamille"];
                                

                                if ((ct.IsArticleDivers ||ct.IsUnique) && reader["DT_Text"] != DBNull.Value)
                                {
                                    ct.TexteComplementaire = reader["DT_Text"].ToString();
                                    using (StringReader r = new StringReader(reader["DT_Text"].ToString()))
                                    {
                                        string[] line;
                                        string l;
                                        while ((l = r.ReadLine()) != null)
                                        {
                                            line = l.Split(':');
                                            switch (line[0])
                                            {
                                                case "Num Fourn":
                                                    ct.FournPrinc = line[1].Trim();
                                                    break;
                                                case "Ref Fourn":
                                                    ct.RefFourn = line[1].Trim();
                                                    break;
                                            }
                                        }
                                    }

                                    currentFourn = ct.FournPrinc;
                                }

                                list.Add(ct);
                            }
                        }
                    }
                }

                return list;
            }
        }

        /// <summary>
        /// Génère des APC en contremarque
        /// Envoi les APC au magasin cible
        /// </summary>
        /// <param name="CmLignes"></param>
        /// <param name="doPiece"></param>
        /// <param name="disableIntermag"></param>
        public void saveAll(Collection<Contremarque> CmLignes, string doPiece, bool disableIntermag)
        {
            var docV = (IBODocumentVente3)GetDocument(doPiece);
            if (!openBaseCial()) throw new Exception("Impossible de se connecter à Sage :-(");

            #region Ajout ABC Existant
            // Prétraitement Si Ajout à ABC existant
            foreach (IBODocumentVenteLigne3 ligneV in docV.FactoryDocumentLigne.List)
            {
                int DLNo = int.Parse(ligneV.InfoLibre["DLNo"]);

                if (CmLignes.Count(c=>c.DlNo == DLNo) == 0)
                {
                    continue;
                }

                Contremarque cm = CmLignes.Where(c => c.DlNo == DLNo).First();
                if (!cm.RowChecked)
                {
                    continue;
                }

                // Ajout a ABC existant (1 seul possible)
                double liaisonTot = cm.Liaisons.Sum(l => l.Qte);
                if (liaisonTot > 0)
                {
                    IEnumerable<LiaisonCde> liaisons = cm.Liaisons.Where(l => l.Qte > 0);
                    LiaisonCde liaison = liaisons.First();
                    Log($"Réserve {liaison.Qte} x {cm.RefMag} sur {liaison.NumPiece} existant");
                    liaison.DlNoOut = int.Parse(ligneV.InfoLibre["DLNo"]);
                    addVbcLigneToAbcExistant(ligneV, liaison);
                }
            }
            #endregion Ajout ABC Existant

            #region Groupe les lignes par fournisseur
            var dict = new Dictionary<string, Collection<Contremarque>>();
            foreach (Contremarque cm in CmLignes)
            {
                // Ne prend pas en compte les lignes déjà en contremarque ou avec liaison
                if (!cm.RowChecked || cm.NumPiece != "" || cm.Liaisons.Sum(l => l.Qte) > 0) continue;

                string key = cm.SelectedFourn == "Principal" ? cm.FournPrinc : cm.SelectedFourn;
                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, new Collection<Contremarque>());
                }

                dict[key].Add(cm);
            }
            if (dict.Count == 0)
            {
                return;
            }
            #endregion Groupe les lignes par fournisseur

            #region Ajout contremarque

            foreach (KeyValuePair<string, Collection<Contremarque>> entry in dict)
            {
                IPMDocument procDocA;
                IBOFournisseur3 fourn;

                if (!entry.Value[0].IsInterMag)
                {
                    // Génération d'un APC Fournisseur
                    Log($"Génération d'un APC Fournisseur principal {entry.Value[0].FournPrinc}");
                    procDocA = GetInstance().CreateProcess_Document(DocumentType.DocumentTypeAchatCommande);
                    fourn = GetInstance().CptaApplication.FactoryFournisseur.ReadNumero(entry.Value[0].FournPrinc);
                }
                else
                {
                    // Génération d'un ABC InterMag
                    Log($"Génération d'un ABC Fournisseur intermag {entry.Key}");
                    procDocA = GetInstance().CreateProcess_Document(DocumentType.DocumentTypeAchatCommandeConf);
                    using (SqlConnection cnx = new SqlConnection(cnxString))
                    {
                        cnx.Open();

                        using (SqlCommand cmd = cnx.CreateCommand())
                        {
                            cmd.CommandText = "SELECT CT_Num FROM F_COMPTET WHERE CT_Classement = @abrege AND CT_Type = 1";
                            cmd.Parameters.AddWithValue("@abrege", entry.Value[0].SelectedFourn);
                            using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                            {
                                reader.Read();
                                fourn =
                                    GetInstance()
                                        .CptaApplication.FactoryFournisseur.ReadNumero((string)reader["CT_Num"]);
                            }
                        }
                    }
                }

                IBODocumentAchat3 docAEntete = (IBODocumentAchat3)procDocA.Document;
                docAEntete.DO_DateLivr = docV.DO_DateLivr;
                docAEntete.SetDefaultFournisseur(fourn);
                docAEntete.Collaborateur = docV.Collaborateur;

                foreach (Contremarque cm in entry.Value)
                {
                    IBODocumentAchatLigne3 docALigne = null;

                    if (cm.IsCommentaire)
                    {
                        docALigne = (IBODocumentAchatLigne3)docAEntete.FactoryDocumentLigne.Create();
                        docALigne.DL_Design = cm.Design;
                        docALigne.WriteDefault();
                    }
                    else
                    {
                        // Retrouve la ligne de vente pour ajouter l'article
                        foreach (IBODocumentVenteLigne3 ligneV in docV.FactoryDocumentLigne.List)
                        {
                            int DLNo = int.Parse(ligneV.InfoLibre["DLNo"]);
                            if (DLNo == cm.DlNo)
                            {
                                double qt = cm.QteTotal;
                                docALigne = (IBODocumentAchatLigne3)addArticleFromLigne(procDocA, ligneV, qt);
                                docALigne.DL_Design = cm.Design;
                                // Applique par defaut le conditionnement de vente
                                docALigne.DL_Qte = qt;
                                docALigne.EU_Qte = qt;
                                docALigne.EU_Enumere = ligneV.Article.Unite.Intitule;
                                docALigne.AF_RefFourniss = cm.RefFourn;

                                // Tente de convertir si fourn princ
                                foreach (IBOArticleTarifFournisseur3 tarif in docALigne.Article.FactoryArticleTarifFournisseur.List)
                                {
                                    if (tarif.Fournisseur == fourn && tarif.AF_ConvDiv != tarif.AF_Conversion)
                                    {
                                        //Debug.Print($"{tarif.Article.AR_Ref}, {tarif.AF_Conversion}, {tarif.AF_ConvDiv}");
                                        double qtColisee = qt * (tarif.AF_ConvDiv / tarif.AF_Conversion);
                                        docALigne.EU_Qte = qtColisee;
                                        docALigne.EU_Enumere = tarif.Unite.Intitule;
                                    }
                                }

                                docALigne.SetDefaultRemise();
                                docALigne.Collaborateur = docV.Collaborateur;
                                docALigne.DO_Ref = cm.DlNo.ToString();
                                docALigne.DO_DateLivr = ligneV.DO_DateLivr;
                                docALigne.TxtComplementaire = cm.TexteComplementaire;
                                docALigne.Write();
                            }
                        }
                    }
                }

                // Si devis => Demande de prix
                // On applique après lacréation des ligne car sinon le DO_Ref va se copier sur chaque ligne
                if (CmLignes.First().Type == 0)
                {
                    docAEntete.DO_Ref = "DEMANDE PRIX";
                    docAEntete.DO_Statut = DocumentStatutType.DocumentStatutTypeSaisie;
                }
                else
                {
                    docAEntete.DO_Ref = "RESERVATION";
                    docAEntete.DO_Statut = DocumentStatutType.DocumentStatutTypeAPrepare;
                }

                // On vire les articles liés en doublons
                // on le sait car le champs DO_Ref est vide
                foreach (IBODocumentAchatLigne3 ligne in procDocA.Document.FactoryDocumentLigne.List)
                {
                    if (ligne.DO_Ref == "")
                    {
                        ligne.Remove();
                    }
                }

                if (procDocA.CanProcess)
                {
                    procDocA.Process();

                    foreach (IBODocumentAchatLigne3 ligne in GetDocument(docAEntete.DO_Piece).FactoryDocumentLigne.List)
                    {
                        if (ligne.Article != null && (ligne.Article.AR_Ref == "DIVERS" || ligne.Article.Famille.FA_CodeFamille == "UNIQUE"))
                        {
                            new DiversRepository().saveLigneAchat(ligne);
                        }
                    }
                }
                else
                {
                    Log(GetProcessError(procDocA));
                    return;
                }
                
                Log($"Document {docAEntete.DO_Piece} créé.");

                // Création des lignes de contremarque
                // Log("Création des lignes de contremarque");
                using (SqlConnection cnx = new SqlConnection(cnxString))
                {
                    cnx.Open();

                    foreach (Contremarque cm in entry.Value)
                    {
                        if (cm.IsCommentaire || cm.IsRemarqueInterne) continue;

                        using (SqlCommand cmd = cnx.CreateCommand())
                        {
                            cmd.CommandText = @"INSERT INTO F_CMLIEN(DL_NoOut, DL_NoIn, CM_Qte) 
                                                    VALUES(@dlNoOut, (SELECT DL_No FROM F_DOCLIGNE WHERE DO_Ref = @dlNoOut AND DO_Piece = @doPiece), @cmQte)";
                            cmd.Parameters.AddWithValue("@dlNoOut", cm.DlNo.ToString());
                            cmd.Parameters.AddWithValue("@doPiece", docAEntete.DO_Piece);
                            cmd.Parameters.AddWithValue("@cmQte", Convert.ToDouble(cm.Qte, CultureInfo.CurrentCulture));
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Maj les NumPiece du doc achat
                    using (SqlCommand cmd = cnx.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE F_DOCLIGNE SET DO_Ref = NULL, NumPiece = dbo.GET_CM_PIECE(DL_No) WHERE DO_Piece = @doPiece";
                        cmd.Parameters.AddWithValue("@doPiece", docAEntete.DO_Piece);
                        cmd.ExecuteNonQuery();
                    }

                    // Si c'est une commande intermag on envoie en intermag
                    if (entry.Value[0].IsInterMag)
                    {
                        // TODO A surveiller, reload le doc depuis la base car erreur Accès refusé
                        var docAchat = (IBODocumentAchat3) GetDocument(docAEntete.DO_Piece);
                        docAchat.DO_Statut = DocumentStatutType.DocumentStatutTypeSaisie;
                        docAchat.Write();

                        if (disableIntermag)
                        {
                            Log("La commande ne sera pas envoyée par intermag");

                        }
                        else
                        {
                            Log("Envoi de la commande intermag");

                            InterMagRepository imRepos = new InterMagRepository();
                            imRepos.Log += new InterMagRepository.LogEventHandler(Log);
                            imRepos.sendSqlCde(imRepos.getCommande(docAEntete.DO_Piece), entry.Value[0].SelectedFourn);
                        }
                    }
                }
                Log("------");
            } // endforeach dict

            // Maj les numPieces du doc vente
            using (var cnx = new SqlConnection(cnxString))
            {
                cnx.Open();
                using (var cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = "UPDATE F_DOCLIGNE SET DO_Ref = NULL, NumPiece = dbo.GET_CM_PIECE(DL_No) WHERE DO_Piece = @doPiece";
                    cmd.Parameters.AddWithValue("@doPiece", docV.DO_Piece);
                    cmd.ExecuteNonQuery();
                }
            }
            #endregion Ajout contremarque
        }

        private void addVbcLigneToAbcExistant(IBODocumentVenteLigne3 ligneV, LiaisonCde liaison)
        {
            IBODocumentAchat3 docA = GetInstance().FactoryDocumentAchat.ReadPiece(DocumentType.DocumentTypeAchatCommandeConf, liaison.NumPiece);
            foreach (IBODocumentAchatLigne3 ligneA in docA.FactoryDocumentLigne.List)
            {
                int dlNoIn = int.Parse(ligneA.InfoLibre["DLNo"]);
                if (dlNoIn == liaison.DLNoIn)
                {
                    // Si la quantité demandée est inférieure à la quantité de la ligne
                    if (liaison.Qte < liaison.DlQteBL)
                    {
                        IBODocumentAchatLigne3 liaisonALigne = (IBODocumentAchatLigne3)docA.FactoryDocumentLigne.Create();
                        addArticleFromLigneAToLigneB(ligneA, liaisonALigne, liaison.Qte);

                        // Force le prix d'achat de la ligne d'origine (peut être une cde depot)
                        if (liaisonALigne.Fournisseur.CT_Num != ligneA.Article.FournisseurPrincipal.Fournisseur.CT_Num)
                        {
                            liaisonALigne.DL_PrixUnitaire = ligneA.DL_PrixUnitaire;
                        }

                        double conversion = 1;
                        double convDiv = 1;
                        foreach (IBOArticleTarifFournisseur3 tarif in ligneA.Article.FactoryArticleTarifFournisseur.List)
                        {
                            if (tarif.Fournisseur == docA.Fournisseur)
                            {
                                conversion = tarif.AF_Conversion;
                                convDiv = tarif.AF_ConvDiv;
                            }
                        }

                        liaisonALigne.DL_Qte = liaison.Qte;
                        liaisonALigne.EU_Qte = (liaison.Qte / conversion) * convDiv;
                        liaisonALigne.SetDefaultRemise();
                        liaisonALigne.Collaborateur = ligneV.Collaborateur;
                        liaisonALigne.DO_Ref = liaison.DlNoOut.ToString();
                        liaisonALigne.DL_Design = ligneV.DL_Design;
                        liaisonALigne.TxtComplementaire = ligneV.TxtComplementaire;
                        liaisonALigne.Write();
                        liaisonALigne.Refresh();
                        dlNoIn = int.Parse(liaisonALigne.InfoLibre["DLNo"]);

                        // Si la ligne d'origine avait déjà une contremarque
                        if (liaison.CMQteTotal > 0)
                        {
                            ligneA.DL_Qte = liaison.DlQteBL - liaison.Qte;
                            ligneA.EU_Qte = (ligneA.DL_Qte / conversion) * convDiv;
                            ligneA.SetDefaultRemise();
                            ligneA.Write();
                        }
                        else
                        {
                            // Crée une nouvelle ligne avec le delta à partir de la ligne d'origine
                            IBODocumentAchatLigne3 ligneACopy = (IBODocumentAchatLigne3)docA.FactoryDocumentLigne.Create();
                            double delta = liaison.DlQteBL - liaison.Qte;
                            addArticleFromLigneAToLigneB(ligneA, ligneACopy, delta);
                            if (ligneACopy.Fournisseur.CT_Num != ligneA.Article.FournisseurPrincipal.Fournisseur.CT_Num)
                            {
                                ligneACopy.DL_PrixUnitaire = ligneA.DL_PrixUnitaire;
                            }
                            ligneACopy.DL_Qte = delta;
                            ligneACopy.EU_Qte = (delta / conversion) * convDiv;
                            ligneACopy.SetDefaultRemise();
                            ligneACopy.Collaborateur = ligneA.Collaborateur;
                            ligneACopy.DL_Design = ligneA.DL_Design;
                            ligneACopy.TxtComplementaire = ligneA.TxtComplementaire;
                            ligneACopy.Write();
                            ligneACopy.Refresh();

                            // Supprime la ligne d'origine
                            ligneA.Remove();
                        }
                    }

                    // Ajout de la CM
                    using (SqlConnection cnx = new SqlConnection(cnxString)) {
                        cnx.Open();

                        using (SqlCommand cmd =  cnx.CreateCommand())
                        {
                            cmd.CommandText = @"INSERT INTO F_CMLIEN(DL_NoOut, DL_NoIn, CM_Qte) VALUES(@dlNoOut, @dlNoIn, @cmQte)";
                            cmd.Parameters.AddWithValue("@dlNoOut", ligneV.InfoLibre["DLNo"]);
                            cmd.Parameters.AddWithValue("@dlNoIn", dlNoIn);
                            cmd.Parameters.AddWithValue("@cmQte", liaison.Qte);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public string DocumentIsValid(string doPiece)
        {
            if (openBaseCial())
            {
                string message = string.Empty;

                var doc = (IBODocumentVente3)GetDocument(doPiece);

                switch (doc.DO_Type)
                {
                    case DocumentType.DocumentTypeVenteDevis:
                    case DocumentType.DocumentTypeVenteCommande:
                        break;
                    default:
                        message += "Le document doit être un VDE ou VBC";
                        break;
                }

                if (doc.Collaborateur == null)
                {
                    message += "Le collaborateur est obligatoire!" + Environment.NewLine;
                }

                return message;
            }

            return "Impossible d'accéder la base pour vérifier le document";
        }

        /// <summary>
        /// Vérifie que le doc en cours ainsi que lees éventuels docs
        /// liés ne sont pas en cours d'utilisation
        /// </summary>
        /// <param name="CmLignes"></param>
        /// <param name="doPiece"></param>
        /// <returns></returns>
        public bool DocumentIsClosed(Collection<Contremarque> CmLignes, string doPiece)
        {
            if (openBaseCial())
            {
                try
                {
                    var doc = (IBODocumentVente3) GetDocument(doPiece);
                    doc.CouldModified();
                    // Sinon le doc reste ouvert pour OM
                    doc.Write();
                }
                catch (Exception)
                {
                    throw new Exception($"Veuillez fermer le document {doPiece}");
                }

                foreach (Contremarque cm in CmLignes)
                {
                    if (cm.Liaisons != null)
                    {
                        foreach (LiaisonCde li in cm.Liaisons)
                        {
                            if (li.Qte > 0)
                            {
                                try
                                {
                                    IBODocumentAchat3 doc = GetInstance().FactoryDocumentAchat.ReadPiece(DocumentType.DocumentTypeAchatCommandeConf, li.NumPiece);
                                    doc.CouldModified();
                                    // TODO doc.Read()
                                    doc.Write();
                                }
                                catch (Exception)
                                {
                                    throw new Exception($"Veuillez fermer le document {li.NumPiece}");
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        public string GetArticleFromRefFourn(string refFourn, string ctNum)
        {
            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();

                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = "SELECT AR_Ref FROM F_ARTFOURNISS WHERE AF_RefFourniss = @refFourn AND CT_Num = @ctNum";
                    cmd.Parameters.AddWithValue("@refFourn", refFourn);
                    cmd.Parameters.AddWithValue("@ctNum", ctNum);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            return (string) reader["AR_Ref"];
                        }
                    }
                }
            }

            return null;
        }

        public Collection<LiaisonCde> getLiaisonsCde(Contremarque cm)
        {
            Collection<LiaisonCde> list = new Collection<LiaisonCde>();

            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();

                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = @"SELECT DL.AR_Ref, DL.AG_No1, DL.AG_No2, DL.DO_Piece, DL.DL_QteBL, ISNULL(SUM(CM_Qte), 0) AS CMQteTotal, CM.DL_NoIn, CT.CT_Intitule, DL.DL_No
                                        FROM F_DOCLIGNE DL
                                        JOIN F_COMPTET CT ON CT.CT_Num = DL.CT_Num
                                        JOIN F_DOCENTETE DE ON DE.DO_Piece = DL.DO_Piece
                                            AND DL.DO_Domaine = 1 
                                            AND DL.DO_Type IN (12)
                                            AND DE.DO_Statut IN (1,2)
                                        LEFT JOIN F_CMLIEN CM ON CM.DL_NoIn = DL.DL_No
                                        GROUP BY CM.DL_NoIn, DL.AR_Ref, AG_No1, AG_No2, DL.DL_QteBL, DL.DO_Piece, CT.CT_Intitule, DL.DL_No
                                        HAVING DL.AR_Ref = @arRef AND AG_No1 = @agNo1 AND AG_No2 = @agNo2 AND DL.DL_QteBL > ISNULL(SUM(CM_Qte), 0)";
                    cmd.Parameters.AddWithValue("@arRef", cm.ArRef);
                    cmd.Parameters.AddWithValue("@agNo1", cm.AgNo1);
                    cmd.Parameters.AddWithValue("@agNo2", cm.AgNo2);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new LiaisonCde()
                            {
                                Cm = cm,
                                NumPiece = reader["DO_Piece"].ToString(),
                                Fournisseur = reader["CT_Intitule"].ToString(),
                                DlQteBL = double.Parse(reader["DL_QteBL"].ToString()),
                                Unite = cm.Unite,
                                CMQteTotal = double.Parse(reader["CMQteTotal"].ToString()),
                                DLNoIn = (int) reader["DL_No"],
                                IsEnabled = true
                            });
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Ajoute un article depuis une ligne de doc dans processDocument
        /// TODO Conditionnement
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="ligne"></param>
        /// <param name="qte"></param>
        /// <returns></returns>
        private IBODocumentLigne3 addArticleFromLigne(IPMDocument doc, IBODocumentLigne3 ligne, double qte)
        {
            if (ligne.ArticleGammeEnum2 != null)
            {
                return doc.AddArticleDoubleGamme(ligne.ArticleGammeEnum1, ligne.ArticleGammeEnum2, qte);
            }
            if (ligne.ArticleGammeEnum1 != null)
            {
                return doc.AddArticleMonoGamme(ligne.ArticleGammeEnum1, qte);
            }
            return doc.AddArticle(ligne.Article, qte);
        }

        /// <summary>
        /// Ajoute un article depuis une ligne de doc vers une autre ligne de doc
        /// TODO Conditionnement
        /// Une partie du conditionnement est traitée dans addVbcLigneToAbcExistant()
        /// </summary>
        /// <param name="ligneA"></param>
        /// <param name="ligneB"></param>
        /// <param name="qte"></param>
        private void addArticleFromLigneAToLigneB(IBODocumentLigne3 ligneA, IBODocumentLigne3 ligneB, double qte)
        {
            if (ligneA.ArticleGammeEnum2 != null)
            {
                ligneB.SetDefaultArticleDoubleGamme(ligneA.ArticleGammeEnum1, ligneA.ArticleGammeEnum2, qte);
                return;
            }
            if (ligneA.ArticleGammeEnum1 != null)
            {
                ligneB.SetDefaultArticleMonoGamme(ligneA.ArticleGammeEnum1, qte);
                return;
            }
            ligneB.SetDefaultArticle(ligneA.Article, qte);
        }
    }
}
