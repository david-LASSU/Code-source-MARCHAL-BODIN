using System;
using System.Collections.Generic;
using System.Data;
using MBCore.Model;
using System.Data.SqlClient;
using Objets100cLib;
using System.IO;
using EnvoyerCommande.IntermagService;
using System.ServiceModel;
using System.Diagnostics;
using System.Windows.Forms;
using Divers.Model;

namespace EnvoyerCommande
{
    public class InterMagRepository : BaseCialAbstract
    {
        public const int TYPE_DEPOT = 0;
        public const int TYPE_RETRO = 1;

        public IDictionary<int, string> typeCdeLabels = new Dictionary<int, string>()
        {
            {TYPE_DEPOT, "Dépot"},
            {TYPE_RETRO, "Rétro"}
        };

        public event LogEventHandler Log;

        public delegate void LogEventHandler(string message);

        public DataSet getCommande(string doPiece)
        {
            DataSet dsCde = new DataSet();
            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();

                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = @"SELECT DB_NAME(DB_ID()) AS DBCLIENT, CT.CT_Classement, DE.CT_NumPayeur, ISNULL(CT.magasin_referent, '') AS magasin_referent, 
                                        DE.DO_Coord01 ,DE.DO_Domaine, DE.DO_Type, DE.DO_Piece, DE.DO_Ref, DE.DO_Tiers, DE.DO_Souche, DE.DO_Statut,
                                        DL.AR_Ref, ISNULL(AG1.EG_Enumere, '') AS Gamme1, ISNULL(AG2.EG_Enumere, '') AS Gamme2, DL.AG_No2, 
                                        DL.AF_RefFourniss, DL.DL_Design, DL.DL_Qte, DL.DL_Ligne, DL.DL_PrixUnitaire, DL.DL_MontantHT, DL.EU_Enumere,
                                        DE.CO_No AS DECONo, DL.CO_No AS DLCONo, CONCAT(CO.CO_Prenom, ' ', CO.CO_Nom) AS Collaborateur, ISNULL(DT.DT_Text, '') AS DT_Text
                                        FROM F_DOCENTETE DE 
                                        JOIN F_DOCLIGNE DL ON DL.DO_Piece = DE.DO_Piece
                                        LEFT JOIN F_DOCLIGNETEXT DT ON DT.DT_No = DL.DT_No
                                        JOIN F_COMPTET CT ON CT.CT_Num = DE.DO_Tiers
                                        LEFT JOIN F_COLLABORATEUR CO ON CO.CO_No = DE.CO_No
                                        LEFT JOIN F_ARTGAMME AG1 ON AG1.AR_Ref = DL.AR_Ref AND AG1.AG_No = DL.AG_No1
                                        LEFT JOIN F_ARTGAMME AG2 ON AG2.AR_Ref = DL.AR_Ref AND AG2.AG_No = DL.AG_No2
                                        WHERE DE.DO_Piece = @doPiece
                                        ORDER BY DL.DL_Ligne";
                    cmd.Parameters.AddWithValue("@doPiece", doPiece);

                    using (SqlDataAdapter adp = new SqlDataAdapter())
                    {
                        adp.SelectCommand = cmd;
                        adp.Fill(dsCde);
                    }
                }
            }

            return dsCde;
        }

        public string valideCommande(DataTable table)
        {
            if (table.Rows.Count == 0)
            {
                return "Commande non trouvée";
            }

            string message = string.Empty;

            DataRow row = table.Rows[0];

            if (row["CT_Classement"].ToString() == getDb(dbName).originalName)
            {
                return "Nous ne pouvez pas être votre propre fournisseur";
            }

            if (row["Do_Domaine"].ToString() != "1" || row["DO_Type"].ToString() != "12")
            {
                message += "Le document doit être un bon de commande d'achat" + Environment.NewLine;
            }

            if (row["DO_Statut"].ToString() == "2")
            {
                message += "La commande a déjà été envoyée (Statut sur 'Envoyé')" + Environment.NewLine;
            }

            if (row["DO_Ref"].ToString() != "")
            {
                message += "La commande a déjà été envoyée (Champs référence non vide)" + Environment.NewLine;
            }

            if (row["DECONo"].ToString() == "" || row["DECONo"].ToString() == "0")
            {
                message += "Le collaborateur est obligatoire." + Environment.NewLine;
            }

            return message;
        }

        public int getTypeCde(DataSet dsCde)
        {
            return dbExiste((string) dsCde.Tables[0].Rows[0]["CT_Classement"]) ? TYPE_DEPOT : TYPE_RETRO;
        }

        public bool DocumentIsClosed(string doPiece)
        {
            openBaseCial();
            IBODocumentAchat3 docA = GetInstance().FactoryDocumentAchat.ReadPiece(DocumentType.DocumentTypeAchatCommandeConf,
                doPiece);
            docA.CouldModified();
            return true;
        }

        /// <summary>
        /// Envoie la commande via le service intermag
        /// </summary>
        /// <param name="dsCde"></param>
        /// <param name="targetDb"></param>
        public bool sendSqlCde(DataSet dsCde, string targetDb)
        {
            try
            {
                int typeCde = getTypeCde(dsCde);
                StringWriter sw = new StringWriter();
                dsCde.WriteXml(sw, XmlWriteMode.IgnoreSchema);

                Log($"Base sélectionnée : {targetDb}");

                // Si ok call le service distant
                IntermagServiceClient client = createClient(targetDb);
                if (client.IsAlive())
                {
                    Log("Serveur en ligne ...");
                    client.Close();

                    client = createClient(targetDb);
                    string result = client.CreateCommand(sw.ToString(), targetDb, typeCde);

                    if (result.StartsWith("[OK]"))
                    {
                        string[] datas = result.Split(';');
                        string magPiece = datas[1];
                        Log(datas[2]);

                        // Modifie le doc
                        editCde((string) dsCde.Tables[0].Rows[0]["DO_Piece"], magPiece, dsCde, targetDb, typeCde);
                        client.Close();
                        return true;
                    }
                    else
                    {
                        Log($"{targetDb}::{result}");
                        client.Close();
                        return false;
                    }
                }
                else
                {
                    Log($"{dbName}::ERREUR : Serveur '{targetDb}' indisponible");

                    return false;
                }
            }
            catch (Exception e)
            {
                Log($"{dbName}::ERREUR : {e.Message}");
                EventLog.WriteEntry("Application", e.ToString(), EventLogEntryType.Error);
                return false;
            }
        }

        /// <summary>
        /// Edite la commande en fonction du type
        /// après retour du service intermag
        /// </summary>
        /// <param name="doPieceOrigin">N° de l'ABC qui a été envoyé au founisseur</param>
        /// <param name="magPiece"></param>
        /// <param name="ds"></param>
        /// <param name="targetDb"></param>
        /// <param name="typeIndex"></param>
        private void editCde(string doPieceOrigin, string magPiece, DataSet ds, string targetDb, int typeIndex)
        {
            try
            {
                if (openBaseCial() == false)
                {
                    throw new Exception("Connexion à la base impossible");
                }

                IBODocumentAchat3 docAOrigin = GetInstance().FactoryDocumentAchat.ReadPiece(
                    DocumentType.DocumentTypeAchatCommandeConf,
                    doPieceOrigin
                );

                // Vérouille le doc
                docAOrigin.CouldModified();

                if (typeIndex == 0)
                {
                    editCommandeDepot(docAOrigin, magPiece);
                }
                else if (typeIndex == 1)
                {
                    editCommandeRetro(docAOrigin, magPiece, targetDb);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        /// <summary>
        /// Cde dépot: Met à jour les prix, statut ...
        /// </summary>
        /// <param name="docAOrigin"></param>
        /// <param name="magPiece"></param>
        private void editCommandeDepot(IBODocumentAchat3 docAOrigin, string magPiece)
        {
            try
            {
                // Commande Dépôt
                docAOrigin.DO_Ref = magPiece;
                docAOrigin.DO_Statut = DocumentStatutType.DocumentStatutTypeConfirme;
                //docAOrigin.InfoLibre["Date Statut"] = DateTime.Now;
                docAOrigin.Write();

                using (SqlConnection cnx = new SqlConnection(cnxString))
                {
                    cnx.Open();
                    using (SqlCommand cmd = cnx.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE F_DOCENTETE SET [Date Statut] = GETDATE() WHERE DO_Piece = @doPiece";
                        cmd.Parameters.AddWithValue("@doPiece", docAOrigin.DO_Piece);
                        
                        cmd.ExecuteNonQuery();
                    }
                }

                docAOrigin.Refresh();

                // Simule une commande client Tarif 6 pour recup les prix remisés
                // et les appliquer au doc d'achat
                IPMDocument procDocV = GetInstance().CreateProcess_Document(DocumentType.DocumentTypeVenteCommande);
                IBODocumentVente3 docVEntete = (IBODocumentVente3) procDocV.Document;

                docVEntete.SetDefaultClient(GetInstance().CptaApplication.FactoryClient.ReadNumero("DEVEL"));
                docVEntete.CategorieTarif = GetInstance().FactoryCategorieTarif.ReadIntitule("Tarif Article N° 6");

                IBODocumentVenteLigne3 docVLigne;
                foreach (IBODocumentAchatLigne3 ligneOrigin in docAOrigin.FactoryDocumentLigne.List)
                {
                    if (ligneOrigin.Article != null)
                    {
                        Debug.Print(ligneOrigin.Article.AR_Ref);
                        docVLigne = (IBODocumentVenteLigne3) docVEntete.FactoryDocumentLigne.Create();
                        IBOArticle3 a = ligneOrigin.Article;
                        double qte = ligneOrigin.DL_Qte;
                        docVLigne.SetDefaultArticle(a, qte);
                        docVLigne.SetDefaultRemise();

                        ligneOrigin.DL_PrixUnitaire = docVLigne.DL_PrixUnitaire;
                        ligneOrigin.Remise.FromString(docVLigne.Remise.ToString());
                        ligneOrigin.DO_Ref = magPiece;
                        ligneOrigin.Write();

                        if (ligneOrigin.Article.AR_Ref == "DIVERS" || ligneOrigin.Article.Famille.FA_CodeFamille == "UNIQUE")
                        {
                            new DiversRepository().saveLigneAchat(ligneOrigin);
                        }
                    }
                }

                // Déverrouille le doc
                docAOrigin.Read();
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        /// <summary>
        /// Cde Retro: Duplique le bon de commande fournisseur en cde magasin
        /// </summary>
        /// <param name="docOrigin"></param>
        /// <param name="magPieces"></param>
        /// <param name="targetDb"></param>
        private void editCommandeRetro(IBODocumentAchat3 docOrigin, string magPieces, string targetDb)
        {
            try
            {
                // Commande RETRO
                IBOFournisseur3 fourn;

                using (SqlConnection cnx = new SqlConnection(cnxString))
                {
                    cnx.Open();
                    using (SqlCommand cmd = cnx.CreateCommand())
                    {
                        cmd.CommandText =
                            "SELECT CT_Num FROM F_COMPTET WHERE CT_Classement = @ctClassement AND CT_Type = 1";
                        cmd.Parameters.AddWithValue("@ctClassement", targetDb);
                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                fourn = GetInstance().CptaApplication.FactoryFournisseur.ReadNumero(reader["CT_Num"].ToString());
                            }
                            else
                            {
                                throw new Exception($"Fournisseur '{targetDb}' non trouvé");
                            }
                        }
                    }
                }

                // Duplication du doc d'origine en document d'achat magasin
                IPMDocument procDoc = GetInstance().CreateProcess_Document(DocumentType.DocumentTypeAchatCommandeConf);
                IBODocumentAchat3 docNew = (IBODocumentAchat3) procDoc.Document;

                docNew.SetDefaultDO_Piece();
                docNew.DO_Statut = DocumentStatutType.DocumentStatutTypeConfirme;
                docNew.DO_Ref = magPieces;
                docNew.SetDefaultFournisseur(fourn);
                docNew.Collaborateur = docOrigin.Collaborateur;

                IBODocumentAchatLigne3 docNewLigne;

                foreach (IBODocumentAchatLigne3 docOrigineLigne in docOrigin.FactoryDocumentLigne.List)
                {
                    if (docOrigineLigne.Article == null)
                    {
                        // Ligne de commentaire
                        docNewLigne = (IBODocumentAchatLigne3) docNew.FactoryDocumentLigne.Create();
                        docNewLigne.DL_Design = docOrigineLigne.DL_Design;
                        docNewLigne.Write();
                    }
                    else
                    {
                        docNewLigne = (IBODocumentAchatLigne3) addArticleFromLigne(procDoc, docOrigineLigne, docOrigineLigne.DL_Qte);
                        docNewLigne.AF_RefFourniss = docOrigineLigne.AF_RefFourniss;
                        docNewLigne.Taxe[0] = GetInstance().CptaApplication.FactoryTaxe.ReadCode("8");
                        docNewLigne.DL_PrixUnitaire = docOrigineLigne.DL_MontantHT / docOrigineLigne.DL_Qte;
                        docNewLigne.SetDefaultRemise();
                        docNewLigne.DO_Ref = docOrigineLigne.InfoLibre["DLNo"];
                        docNewLigne.TxtComplementaire = docOrigineLigne.TxtComplementaire;
                        docNewLigne.DL_Design = docOrigineLigne.DL_Design;
                        docNewLigne.Write();
                    }
                }

                if (procDoc.CanProcess)
                {
                    procDoc.Process();

                    foreach (IBODocumentAchatLigne3 ligne in GetDocument(docNew.DO_Piece).FactoryDocumentLigne.List)
                    {
                        if (ligne.Article != null && (ligne.Article.AR_Ref == "DIVERS" || ligne.Article.Famille.FA_CodeFamille == "UNIQUE"))
                        {
                            new DiversRepository().saveLigneAchat(ligne);
                        }
                    }

                    using (SqlConnection cnx = new SqlConnection(cnxString))
                    {
                        cnx.Open();
                        // Met à jour les infos libres
                        using (SqlCommand cmd = cnx.CreateCommand())
                        {
                            cmd.CommandText = @"UPDATE F_DOCENTETE 
                                                SET [Date Statut] = GETDATE(), [RETRO_FOURN] = @retroFourn 
                                                WHERE DO_Piece = @doPiece";
                            cmd.Parameters.AddWithValue("@doPiece", docNew.DO_Piece);
                            cmd.Parameters.AddWithValue("@retroFourn", docOrigin.Fournisseur.CT_Num);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    docNew.Refresh();
                }
                else
                {
                    throw new Exception(GetProcessError(procDoc));
                }

                // Met à jour la référence du doc origine
                docOrigin.DO_Ref = $"Voir {docNew.DO_Piece}";
                docOrigin.DO_Statut = DocumentStatutType.DocumentStatutTypeConfirme;
                docOrigin.Write();

                // Contremarque
                using (SqlConnection cnx = new SqlConnection(cnxString))
                {
                    cnx.Open();
                    SqlTransaction transaction = cnx.BeginTransaction();

                    try
                    {
                        using (SqlCommand cmd = cnx.CreateCommand())
                        {
                            cmd.Transaction = transaction;
                            // Recup les lignes du doc d'origine
                            cmd.CommandText = @"SELECT CM.DL_NoIn, CM.DL_NoOut, CM_Qte
                                                FROM F_CMLIEN CM
                                                JOIN F_DOCLIGNE DL ON DL.DL_No = CM.DL_NoIn
                                                WHERE DL.DO_PIECE = @doPiece  ";
                            cmd.Parameters.AddWithValue("@doPiece", docOrigin.DO_Piece);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    // Y a de la contremarque sur le doc
                                    while (reader.Read())
                                    {
                                        int dlNoIn = (int)reader["DL_NoIn"];
                                        using (SqlCommand cmd3 = cnx.CreateCommand())
                                        {
                                            cmd3.Transaction = transaction;
                                            cmd3.CommandText =
                                                "UPDATE F_CMLIEN SET DL_NoIn = (SELECT DL_No FROM F_DOCLIGNE WHERE DO_Ref = @dlNoIn) WHERE DL_NoIn = @dlNoIn";
                                            cmd3.Parameters.AddWithValue("@dlNoIn", dlNoIn.ToString());
                                            cmd3.ExecuteNonQuery();
                                        }

                                        // On remplace DO_Ref par sa valeur réelle
                                        using (SqlCommand cmdUpd = cnx.CreateCommand())
                                        {
                                            cmdUpd.Transaction = transaction;
                                            cmdUpd.CommandText = "UPDATE F_DOCLIGNE SET DO_Ref = @doRef WHERE DO_Ref = @uid";
                                            cmdUpd.Parameters.AddWithValue("@doRef", magPieces);
                                            cmdUpd.Parameters.AddWithValue("@uid", docOrigin.DO_Piece + dlNoIn);
                                            cmdUpd.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }

                            transaction.Commit();
                        }
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        throw new Exception(e.ToString());
                    }
                }

                Log($"{dbName} :: Doc Achat {docOrigin.DO_Piece} dupliqué vers le doc achat {docNew.DO_Piece}");

                // Met à jour la commande distante
                IntermagServiceClient client = createClient(targetDb);
                client.SetDoRef(docNew.DO_Piece, targetDb, magPieces.Split('/')[0]);
                client.Close();

                // Suppression du document fournisseur d'origine
                if (MessageBox.Show(
                    "Souhaitez-vous supprimer le document fournisseur d'origine?",
                    "Suppression doc origine",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    docOrigin.Remove();
                }
                else
                {
                    Log($"Vous devrez supprimer le document d'achat n° {docOrigin.DO_Piece} manuellement.");
                    // Dévérouille le doc
                    docOrigin.Read();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
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
        /// Crée un client InterMagService
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        private IntermagServiceClient createClient(string dbName)
        {
            string server;
#if DEBUG
            server = Environment.MachineName;
            //server = getDb(dbName).server;
#else
            server = getDb(dbName).server;
#endif
            Log(string.Format("Connexion à {0}", server));

            server = $"http://{server}:8001/InterMagService";

            try
            {
                BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
                binding.Name = "BasicHttpBinding_" + Environment.MachineName;
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
                binding.OpenTimeout = new TimeSpan(0, 2, 0);
                binding.CloseTimeout = new TimeSpan(0, 2, 0);
                binding.SendTimeout = new TimeSpan(0, 2, 0);
                binding.ReceiveTimeout = new TimeSpan(0, 2, 0);

                IntermagServiceClient client = new IntermagServiceClient(binding, new EndpointAddress(new Uri(server)));

                return client;
            }
            catch (Exception e)
            {
                Log(e.ToString());
                throw new Exception(e.ToString());
            }
        }
    }
}
