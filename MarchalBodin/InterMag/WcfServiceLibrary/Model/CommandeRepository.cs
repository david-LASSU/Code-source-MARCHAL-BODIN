using Divers.Model;
using LiaisonsDocVente.Model;
using MBCore.Model;
using Objets100cLib;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace WcfServiceLibrary.Model
{
    class CommandeRepository : BaseCialAbstract
    {
        private string log = "Application";

        private string cmLogMessage = string.Empty;

        private void Log(string message)
        {
            Debug.Print(message);
            cmLogMessage += message + Environment.NewLine;
        }

        public CommandeRepository(string targetDb)
        {
            setDefaultParams(getDb(targetDb).gcmFile, ADMINUSR);
        }

        /// <summary>
        /// Vérifie par le biais de la référence de pièce si la commande existe
        /// </summary>
        /// <param name="ctNum"></param>
        /// <param name="doRef"></param>
        /// <param name="targetDb"></param>
        /// <returns></returns>
        private void CheckCommandeClientExiste(string ctNum, string doRef, string targetDb)
        {
            using (SqlConnection cnx = new SqlConnection(getCnxString(targetDb)))
            {
                cnx.Open();
                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = @"SELECT DE.DO_Piece 
                        FROM F_DOCENTETE DE
                        JOIN F_DOCLIGNE DL ON DE.DO_Piece = DL.DO_Piece
                        WHERE CT_Num = @ctNum
                        AND (DE.DO_Ref LIKE '%' + @doRef + '%' OR DL.DO_Ref LIKE '%' + @doRef + '%')";
                    cmd.Parameters.AddWithValue("@doRef", doRef);
                    cmd.Parameters.AddWithValue("@ctNum", ctNum);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            throw new Exception($"La commande avec la référence {doRef} existe déjà dans notre doc {reader["DO_Piece"]}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Crée une commande dépot
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="targetDb"></param>
        /// <returns></returns>
        public string createCommandeDepot(DataTable dt, string targetDb)
        {
            BSCIALApplication100c bsc = GetInstance();
            try
            {
                bsc.Open();
                IBOClient3 client = getClient(bsc, dt.Rows[0]["DBCLIENT"].ToString() + " DEPOT");

                string doRef = dt.Rows[0]["DO_Piece"].ToString();

                CheckCommandeClientExiste(client.CT_Num, doRef, targetDb);

                // Crée le bon de commande client
                IPMDocument procDocV = bsc.CreateProcess_Document(DocumentType.DocumentTypeVenteCommande);
                IBODocumentVente3 docVEntete = (IBODocumentVente3)procDocV.Document;

                docVEntete.DO_Ref = doRef;

                // Affecte le numéro de pièce
                docVEntete.SetDefaultDO_Piece();

                docVEntete.SetDefaultClient(client);
                docVEntete.CategorieTarif = bsc.FactoryCategorieTarif.ReadIntitule("Tarif Article N° 6");

                IBODocumentVenteLigne3 docVLigne;
                foreach (DataRow row in dt.Rows)
                {
                    string arRef = row["AR_Ref"].ToString();
                    string gamme1 = row["Gamme1"].ToString();
                    string gamme2 = row["Gamme2"].ToString();
                    double qt = double.Parse(row["DL_Qte"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                    string refFourn = row["AF_RefFourniss"].ToString();
                    string design = row["DL_Design"].ToString();
                    string txtComplementaire = row["DT_Text"].ToString();
                    string unite = row["EU_Enumere"].ToString();

                    if (arRef != "")
                    {
                        docVLigne = (IBODocumentVenteLigne3)addArticleToLigne(procDocV, arRef, gamme1, gamme2, qt, unite);
                        // Si pas de ref fourn, on essaie de recup la ref fourn principal
                        if (refFourn == "" && docVLigne.Article.FournisseurPrincipal != null)
                        {
                            refFourn = docVLigne.Article.FournisseurPrincipal.Reference;
                        }
                        docVLigne.AF_RefFourniss = refFourn;
                        docVLigne.SetDefaultRemise();
                        docVLigne.DO_Ref = doRef;
                        docVLigne.DL_Design = design;
                        docVLigne.TxtComplementaire = txtComplementaire;
                        docVLigne.Write();
                    }
                    else
                    {
                        // Sinon c'est une ligne de commentaire
                        docVLigne = (IBODocumentVenteLigne3)docVEntete.FactoryDocumentLigne.Create();
                        docVLigne.DL_Design = design;
                        docVLigne.Write();
                    }
                }

                if (procDocV.CanProcess)
                {
                    procDocV.Process();
                    // Cherche les divers pour forcer la maj du texte complémentaire
                    // Ne peut pas être effectué pendant le process car les info libres n'existent pas encore

                    foreach (IBODocumentVenteLigne3 ligne in GetDocument(docVEntete.DO_Piece).FactoryDocumentLigne.List)
                    {
                        if (ligne.Article != null && (ligne.Article.AR_Ref == "DIVERS" || ligne.Article.Famille.FA_CodeFamille == "UNIQUE"))
                        {
                            new DiversRepository().saveLigneVente(ligne);
                        }
                    }
                }
                else
                {
                    throw new Exception(GetProcessError(procDocV));
                }

                string subject = $"[INTERMAG] Commande Dépôt {client.CT_Classement} {docVEntete.DO_Piece}";
                string body = $@"<p>Le magasin {client.CT_Classement} vous a passé une commande dépôt n° <b>{docVEntete.DO_Piece}</b></p>
                                 <p>Collaborateur : {dt.Rows[0]["Collaborateur"].ToString()}</p>
                                 <p>Merci de vous référer à Sage pour en connaître le contenu.</p>";
                sendMail(dt.Rows[0]["DBCLIENT"].ToString(), bsc.DatabaseInfo.DatabaseName, subject, body);

                EventLog.WriteEntry(log, subject, EventLogEntryType.Information, 100);

                return string.Format(
                    "[OK];{0};{1}: Commande client {2} créée",
                    docVEntete.DO_Piece,
                    bsc.DatabaseInfo.DatabaseName,
                    docVEntete.DO_Piece
                );
            }
            catch (Exception e)
            {
                EventLog.WriteEntry(log, e.ToString(), EventLogEntryType.Error, 100);
                throw new Exception(e.Message);
            }
            finally
            {
                if (bsc != null)
                {
                    bsc.Close();
                }
            }
        }

        /// <summary>
        /// Crée une commande Retro
        /// TODO utiliser le programme Contremarque pour générer l'APC? eg crée le document de vente en premier puis ajoute une contremarque sur toute les lignes
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public string createCommandeRetro(DataTable dt, string targetDb)
        {
            BSCIALApplication100c bsc = GetInstance();
            try
            {
                bsc.Open();
                IBOClient3 client = getClient(bsc, dt.Rows[0]["DBCLIENT"].ToString() + " RETRO");
                string doRef = dt.Rows[0]["DO_Piece"].ToString();
                CheckCommandeClientExiste(client.CT_Num, doRef, targetDb);

                // Crée le bon de commande client
                IPMDocument procDocV = bsc.CreateProcess_Document(DocumentType.DocumentTypeVenteCommande);
                IBODocumentVente3 docVEntete = (IBODocumentVente3)procDocV.Document;

                // Affecte le numéro de pièce
                docVEntete.SetDefaultDO_Piece();

                docVEntete.SetDefaultClient(client);
                docVEntete.CategorieTarif = bsc.FactoryCategorieTarif.ReadIntitule("Tarif Article N° 13");

                IBODocumentVenteLigne3 docVLigne;
                foreach (DataRow row in dt.Rows)
                {
                    string arRef = row["AR_Ref"].ToString();
                    string gamme1 = row["Gamme1"].ToString();
                    string gamme2 = row["Gamme2"].ToString();
                    double montantHT = double.Parse(row["DL_MontantHT"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                    double qt = double.Parse(row["DL_Qte"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                    double prixUNet = montantHT / qt;
                    string refFourn = row["AF_RefFourniss"].ToString();
                    string design = row["DL_Design"].ToString();
                    string txtComplementaire = row["DT_Text"].ToString();
                    string unite = row["EU_Enumere"].ToString();

                    docVLigne = null;

                    if (arRef != "")
                    {
                        docVLigne = (IBODocumentVenteLigne3) addArticleToLigne(procDocV, arRef, gamme1, gamme2, qt, unite);
                        docVLigne.DL_PrixUnitaire = prixUNet;
                        docVLigne.DL_Design = design;
                        docVLigne.TxtComplementaire = txtComplementaire;
                        docVLigne.Write();
                    }
                    else
                    {
                        // Sinon c'est une ligne de commentaire
                        docVLigne = (IBODocumentVenteLigne3)docVEntete.FactoryDocumentLigne.Create();
                        docVLigne.DL_Design = design;
                        docVLigne.Write();
                    }
                }

                if (procDocV.CanProcess)
                {
                    procDocV.Process();
                    
                    // Cherche les divers pour forcer la maj du texte complémentaire
                    // Ne peut pas être effectué pendant le process car les info libres n'existent pas encore
                    foreach (IBODocumentVenteLigne3 ligne in GetDocument(docVEntete.DO_Piece).FactoryDocumentLigne.List)
                    {
                        if (ligne.Article != null && (ligne.Article.AR_Ref == "DIVERS" || ligne.Article.Famille.FA_CodeFamille == "UNIQUE"))
                        {
                            new DiversRepository().saveLigneVente(ligne);
                        }
                    }

                    // Ajoute les lignes en contremarque
                    ContremarqueRepository cmRepos = new ContremarqueRepository();
                    cmRepos.Log += Log;
                    Collection<Contremarque> cms = cmRepos.getAll(docVEntete.DO_Piece);
                    cms.Select(c => {
                        c.RowChecked = true;
                        c.SelectedFourn = "Principal";
                        // On force pour chaque ligne le fournisseur demandé
                        // Cela permet de faire du retro sur un fournisseur secondaire
                        c.FournPrinc = dt.Rows[0]["DO_Tiers"].ToString();
                        return c; }).ToList();
                    cmRepos.saveAll(cms, docVEntete.DO_Piece, true);
                }
                else
                {
                    throw new Exception(GetProcessError(procDocV));
                }

                string subject = $"[INTERMAG] Commande Retro {client.CT_Classement} {docVEntete.DO_Piece}";
                string body = $@"<p>Le magasin {client.CT_Classement} vous a passé une commande Retro n° <b>{docVEntete.DO_Piece}</b></p>
                                 <p>Collaborateur : {dt.Rows[0]["Collaborateur"].ToString()}</p>
                                 <p>Merci de vous référer à Sage pour en connaître le contenu.</p>";
                sendMail(dt.Rows[0]["DBCLIENT"].ToString(), bsc.DatabaseInfo.DatabaseName, subject, body);
                EventLog.WriteEntry(log, subject, EventLogEntryType.Information, 100);
                return $"[OK];{docVEntete.DO_Piece};{bsc.DatabaseInfo.DatabaseName}: Commande client {docVEntete.DO_Piece} créée {Environment.NewLine}{cmLogMessage}";
            }
            catch (Exception e)
            {
                EventLog.WriteEntry(log, e.ToString(), EventLogEntryType.Error, 100);
                throw new Exception(e.Message);
            }
            finally
            {
                if (bsc != null)
                {
                    bsc.Close();
                }
            }
        }

        private IBODocumentLigne3 addArticleToLigne(IPMDocument doc, string arRef, string gamme1, string gamme2, double qt, string unite)
        {
            IBOArticle3 article;
            if (GetInstance().FactoryArticle.ExistReference(arRef))
            {
                article = GetInstance().FactoryArticle.ReadReference(arRef);
            }
            else if (DiversRepository.UniqueRegex.IsMatch(arRef))
            {
                article = new DiversRepository().getUniqueArticle(arRef);
                article.Unite = GetInstance().FactoryUnite.ReadIntitule(unite);
                article.Write();
            }
            else
            {
                throw new Exception($"Article '{arRef}' non trouvé");
            }

            if (gamme2 != "")
            {
                return doc.AddArticleDoubleGamme(
                    article.FactoryArticleGammeEnum1.ReadEnumere(gamme1),
                    article.FactoryArticleGammeEnum2.ReadEnumere(gamme2),
                    qt
                );
            }
            if (gamme1 != "")
            {
                return doc.AddArticleMonoGamme(
                    article.FactoryArticleGammeEnum1.ReadEnumere(gamme1),
                    qt
                );
            }
            return doc.AddArticle(article, qt);
        }

        private void addArticleToLigne(IBODocumentLigne3 ligne, string arRef, string gamme1, string gamme2, double qt, string unite)
        {
            IBOArticle3 article;
            if (GetInstance().FactoryArticle.ExistReference(arRef))
            {
                article = GetInstance().FactoryArticle.ReadReference(arRef);
            }
            else if (DiversRepository.UniqueRegex.IsMatch(arRef))
            {
                article = new DiversRepository().getUniqueArticle(arRef);
                article.Unite = GetInstance().FactoryUnite.ReadIntitule(unite);
                article.Write();
            }
            else
            {
                throw new Exception($"Article '{arRef}' non trouvé");
            }
            if (gamme2 != "")
            {
                ligne.SetDefaultArticleDoubleGamme(
                    article.FactoryArticleGammeEnum1.ReadEnumere(gamme1),
                    article.FactoryArticleGammeEnum2.ReadEnumere(gamme2),
                    qt
                );
                return;
            }
            if (gamme1 != "")
            {
                ligne.SetDefaultArticleMonoGamme(
                    article.FactoryArticleGammeEnum1.ReadEnumere(gamme1),
                    qt
                );
                return;
            }
            ligne.SetDefaultArticle(article, qt);
        }

        /// <summary>
        /// Met à jour la pièce après duplication coté client
        /// </summary>
        /// <param name="clientPiece"></param>
        /// <param name="targetDb"></param>
        /// <param name="fournPiece"></param>
        /// <returns></returns>
        public string setDoRef(string clientPiece, string targetDb, string fournPiece)
        {
            try
            {
                using (SqlConnection cnx = new SqlConnection(getCnxString(targetDb)))
                {
                    cnx.Open();
                    using (SqlCommand cmd = cnx.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE F_DOCENTETE SET DO_Ref = @doRef WHERE DO_Piece = @doPiece";
                        cmd.Parameters.AddWithValue("@doRef", clientPiece);
                        cmd.Parameters.AddWithValue("@doPiece", fournPiece);
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "UPDATE F_DOCLIGNE SET DO_Ref = @doRef WHERE DO_Piece = @doPiece AND AR_Ref IS NOT NULL";
                        cmd.ExecuteNonQuery();
                    }
                }
                return "[OK]";
            }
            catch (Exception e)
            {
                EventLog.WriteEntry(log, e.ToString(), EventLogEntryType.Error, 100);
                return string.Format("ERREUR : {0}", e.Message);
            }
        }

        /// <summary>
        /// Retourne un client
        /// </summary>
        /// <param name="bsc"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        private IBOClient3 getClient(BSCIALApplication100c bsc, string abrege)
        {
            using (SqlConnection cnx = new SqlConnection(getCnxString(bsc)))
            {
                cnx.Open();
                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = "SELECT CT_Num FROM F_COMPTET WHERE CT_Classement = @ctClassement AND CT_Type = 0";
                    cmd.Parameters.AddWithValue("@ctClassement", abrege);
                    using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            return bsc.CptaApplication.FactoryClient.ReadNumero(reader["CT_Num"].ToString());
                        }
                        else
                        {
                            throw new Exception(string.Format("Client '{0}' non trouvé", abrege));
                        }
                    }
                }
            }
        }

        private void sendMail(string dbFrom, string dbTo, string subject, string body)
        {
            try
            {
                string from = (string)getDb(dbFrom).email;
                string to = (string)getDb(dbTo).email;

                SmtpClient client = new SmtpClient();
                client.Port = 587;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("admin@marchal-bodin.fr", "r0itYv0LcXfhAd1QJot0");
                client.Host = "auth.smtp.1and1.fr";
                client.EnableSsl = true;

                MailMessage mail = new MailMessage(from, to, subject, body);
                mail.IsBodyHtml = true;

                mail.Bcc.Add("admin@marchal-bodin.fr");
                client.Send(mail);
            }
            catch (Exception e)
            {
                EventLog.WriteEntry(log, e.ToString(), EventLogEntryType.Error, 100);
            }
        }

        public string toggleVerrou(string doPiece)
        {
            try
            {
                using (SqlConnection cnx = new SqlConnection(cnxString))
                {
                    cnx.Open();

                    using (SqlTransaction tr = cnx.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand cmd = cnx.CreateCommand())
                            {
                                cmd.Transaction = tr;
                                cmd.CommandText = "ALTER TABLE F_DOCENTETE DISABLE TRIGGER CTRL_VERROU_F_DOCENTETE";
                                cmd.ExecuteNonQuery();

                                cmd.CommandText = "UPDATE F_DOCENTETE SET VERROU = CASE WHEN VERROU LIKE 'Oui' THEN 'Non' ELSE 'Oui' END, DO_Statut = 0 WHERE DO_PIECE = @doPiece";
                                cmd.Parameters.AddWithValue("@doPiece", doPiece);
                                cmd.ExecuteNonQuery();

                                cmd.Parameters.Clear();
                                cmd.CommandText = "ALTER TABLE F_DOCENTETE ENABLE TRIGGER CTRL_VERROU_F_DOCENTETE";
                                cmd.ExecuteNonQuery();
                            }

                            tr.Commit();
                            return "OK";
                        }
                        catch (Exception e)
                        {
                            tr.Rollback();
                            EventLog.WriteEntry(log, e.ToString(), EventLogEntryType.Error, 100);
                            return $"ERREUR : {e.Message}";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                EventLog.WriteEntry(log, e.ToString(), EventLogEntryType.Error, 100);
                return $"ERREUR : {e.Message}";
            }
        }
    }
}
