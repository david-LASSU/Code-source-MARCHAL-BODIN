using MBCore.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Objets100cLib;
using System.Diagnostics;

namespace Divers.Model
{
    public class DiversRepository : BaseCialAbstract
    {
        public const char UniqueChar = 'Z';
        public static readonly System.Text.RegularExpressions.Regex UniqueRegex = new System.Text.RegularExpressions.Regex($"^{UniqueChar}\\d+-\\d+$");
        public readonly DocumentType[] AuthorizedTypes = new DocumentType[]
        {
            DocumentType.DocumentTypeVenteDevis,
            DocumentType.DocumentTypeVenteCommande,
            DocumentType.DocumentTypeAchatCommande,
            DocumentType.DocumentTypeAchatCommandeConf
        };

        public Document GetDoc(string DoPiece, IEnumerable<Fournisseur> fourns, IEnumerable<Unite> unites)
        {
            var document = new Document();
            var lignes = new Collection<Ligne>();

            using (var cnx = new SqlConnection(cnxString))
            {
                cnx.Open();
                using (var cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = "SELECT DO_Tiers, DO_Piece, DO_Reliquat, DO_Domaine, DO_Type FROM F_DOCENTETE WHERE DO_Piece = @doPiece";
                    cmd.Parameters.AddWithValue("@doPiece", DoPiece);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            throw new Exception($"Document '{DoPiece}' non trouvé.");
                        }
                        reader.Read();
                        document.DoPiece = DoPiece;
                        document.IsRelicat = (short)reader["DO_Reliquat"] != 0;
                        document.Domaine = (short)reader["DO_Domaine"];
                        document.Type = (short)reader["DO_Type"];
                        if (document.Domaine == 1)
                        {
                            document.Fournisseur = fourns.First(f => f.CtNum == reader["DO_Tiers"].ToString());
                        }
                    }
                }

                using (var cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = @"SELECT A.AR_Ref, DE.DO_Domaine, DE.DO_Type, DE.DO_Reliquat, DL.DL_No, DL.DL_Design, DL.DL_Qte, DT.DT_Text, DL.EU_Enumere, DL.CT_Num, 
                                        (SELECT COUNT(*) FROM F_CMLIEN CM WHERE CASE WHEN DE.DO_Domaine = 0 THEN CM.DL_NoOut ELSE CM.DL_NoIn END = DL.DL_No) AS nbCm
                                        FROM F_DOCLIGNE DL
                                        JOIN F_DOCENTETE DE ON DE.DO_Piece = DL.DO_Piece
                                        LEFT JOIN F_DOCLIGNETEXT DT ON DT.DT_No = DL.DT_No
                                        JOIN F_ARTICLE A ON A.AR_Ref = DL.AR_Ref
                                        WHERE DL.DO_Piece = @DoPiece
                                        AND (DL.AR_Ref = 'DIVERS' OR A.FA_CodeFamille = 'UNIQUE')";
                    cmd.Parameters.AddWithValue("@DoPiece", DoPiece);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // NE PREND EN COMPTE QUE LES ANCIENS DIVERS
                            // POUR LE COTE RETROACTIF
                            if (reader["AR_Ref"].ToString() == "DIVERS" && reader["DT_Text"] == DBNull.Value)
                            {
                                continue;
                            }
                            Ligne ligne = new Ligne(this)
                            {
                                ArRef = reader["AR_Ref"].ToString(),
                                IsRelicat = (short)reader["DO_Reliquat"] != 0,
                                DlNo = reader["DL_No"] == DBNull.Value ? (int?)null : (int)reader["DL_No"],
                                Domaine = (short)reader["DO_Domaine"],
                                Type = (short)reader["DO_Type"],
                                Designation = (string)reader["DL_Design"],
                                Quantite = double.Parse(reader["DL_Qte"].ToString()),
                                Unite = unites.DefaultIfEmpty(unites.First(u => u.Intitule == "PCE")).First(u=>u.Intitule == (string)reader["EU_Enumere"]),
                                NbCm = (int)reader["nbCm"]
                            };
                            
                            if (reader["DT_Text"] != DBNull.Value)
                            {
                                string dtText = reader["DT_Text"].ToString();
                                ligne.ParseTxtComplementaire(dtText);
                                // Todo le fait de parser ne renseigne pas complètement le fournisseur
                                if (ligne.Fournisseur != null)
                                {
                                    ligne.Fournisseur = fourns.First(f => f.CtNum == ligne.Fournisseur.CtNum);
                                }
                            }
                            // Si doc achat le fournisseur est forcément celui du doc achat
                            if (ligne.Domaine == 1)
                            {
                                ligne.Fournisseur = fourns.First(f => f.CtNum == (string)reader["CT_Num"]);
                            }
                            ligne.ValidAllProperties();
                            lignes.Add(ligne);
                        }
                    }
                }
            }
            document.Lignes = new ObservableCollection<Ligne>(lignes);
            return document;
        }

        internal bool Add(Document document, IEnumerable<Unite> unites)
        {
            var ligne = new Ligne(this)
            {
                IsRelicat = document.IsRelicat,
                Domaine = document.Domaine,
                Type = document.Type,
                Quantite = 1,
                Designation = "Article Divers",
                NbCm = 0,
                Unite = unites.First(u => u.Intitule == "PCE")
            };
            if (document.Domaine == 1)
            {
                ligne.Fournisseur = document.Fournisseur;
            }
            ligne.ValidAllProperties();
            document.Lignes.Add(ligne);
            return true;
        }

        /// <summary>
        /// TODO Transférer dans MBCore
        /// </summary>
        /// <param name="document"></param>
        internal void CheckDocumentsClosed(Document document)
        {
            if (openBaseCial())
            {
                IBODocument3 doc = GetDocument(document.DoPiece);
                try
                {
                    doc.CouldModified();
                    doc.Read();
                }
                catch (Exception)
                {
                    throw new Exception($"Veuillez fermer le document {doc.DO_Piece}");
                }

                foreach (IBODocumentLigne3 ligne in doc.FactoryDocumentLigne.List)
                {
                    if (ligne.Article != null 
                        && (ligne.Article.AR_Ref == "DIVERS" ||ligne.Article.Famille.FA_CodeFamille == "UNIQUE") 
                        && ligne.FactoryDocumentLigneLienCM.List.Count > 0)
                    {
                        foreach (IBODocumentLigneLienCM cmLigne in ligne.FactoryDocumentLigneLienCM.List)
                        {
                            try
                            {
                                cmLigne.DocumentLigneIn.Document.CouldModified();
                                cmLigne.DocumentLigneIn.Document.Read();
                            }
                            catch (Exception)
                            {
                                throw new Exception($"Veuillez fermer le document {cmLigne.DocumentLigneIn.Document.DO_Piece}");
                            }
                            try
                            {
                                cmLigne.DocumentLigneOut.Document.CouldModified();
                                cmLigne.DocumentLigneOut.Document.Read();
                            }
                            catch (Exception)
                            {
                                throw new Exception($"Veuillez fermer le document {cmLigne.DocumentLigneOut.Document.DO_Piece}");
                            }
                        }
                    }
                }
            }
        }

        public bool SaveAll(Document document)
        {
            // Reset les resultats d'enregistrement
            document.Lignes.ToList().ForEach(l => l.SaveResult = null);
            if (openBaseCial())
            {
                switch (document.Domaine)
                {
                    case 0:
                        saveDocVente(document);
                        break;
                    case 1:
                        saveDocAchat(document);
                        break;
                    default:
                        throw new Exception("Domaine invalide.");
                }
            }
            else
            {
                throw new Exception("Impossible de se connecter");
            }

            return true;
        }

        private void saveDocVente(Document document)
        {
            var doc = (IBODocumentVente3)GetDocument(document.DoPiece);
            foreach (Ligne ligne in document.Lignes)
            {
                ligne.ValidAllProperties();
                if (!ligne.IsValid)
                {
                    ligne.SaveResultTo(false);
                    continue;
                }

                // Création article unique ?
                CheckDocLigneForUniqueArticle(doc, ligne);

                foreach (IBODocumentVenteLigne3 docLigne in doc.FactoryDocumentLigne.List)
                {
                    if (docLigne.InfoLibre["DlNo"] == ligne.DlNo.ToString())
                    {
                        saveLigneVente(docLigne, ligne);

                        foreach (IBODocumentLigneLienCM cm in docLigne.FactoryDocumentLigneLienCM.List)
                        {
                            saveLigneAchat((IBODocumentAchatLigne3)cm.DocumentLigneIn, ligne);
                        }
                        ligne.SaveResultTo(true);
                    }
                }
            }
        }

        public void saveLigneVente(IBODocumentVenteLigne3 docLigne, Ligne ligne)
        {
            var doc = (IBODocumentVente3)docLigne.Document;
            if (doc.DO_Reliquat || !AuthorizedTypes.Contains(doc.DO_Type))
            {
                throw new Exception("Le document est un relicat ou le type est interdit");
            }
            docLigne.DL_Design = ligne.Designation;
            docLigne.EU_Qte = ligne.Quantite;
            docLigne.DL_Qte = ligne.Quantite;
            docLigne.TxtComplementaire = ligne.TxtComplementaire;

            if (ligne.DesMajDocVen == false)
            {
                docLigne.DL_PrixUnitaire = ligne.PxUVen??0;
                if (doc.CategorieTarif.CT_PrixTTC)
                {
                    docLigne.DL_PUTTC = ligne.PxUVenTTC ?? 0;
                }
                docLigne.Remise.FromString((ligne.RemiseVen ?? 0).ToString().Replace('.', ',') + '%');
            }
            docLigne.WriteDefault();
            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();
                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = "UPDATE F_DOCLIGNE SET AF_RefFourniss = @refFourn, EU_Enumere = @enum WHERE DL_No = @dlNo";
                    cmd.Parameters.AddWithValue("@enum", ligne.Unite.Intitule);
                    cmd.Parameters.AddWithValue("@refFourn", ligne.RefFourn ?? string.Empty);
                    cmd.Parameters.AddWithValue("@dlNo", docLigne.InfoLibre["DLNo"]);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void saveLigneVente(IBODocumentVenteLigne3 docLigne)
        {
            Ligne ligne = new Ligne(this)
            {
                Designation = docLigne.DL_Design,
                Unite = new Unite() { Intitule = docLigne.EU_Enumere },
                Quantite = docLigne.DL_Qte
            };
            ligne.ParseTxtComplementaire(docLigne.TxtComplementaire);
            saveLigneVente(docLigne, ligne);
        }

        private void saveDocAchat(Document document)
        {
            var doc = (IBODocumentAchat3) GetDocument(document.DoPiece);
            foreach (Ligne ligne in document.Lignes)
            {
                ligne.ValidAllProperties();
                if (!ligne.IsValid)
                {
                    ligne.SaveResultTo(false);
                    continue;
                }
                
                // Création article unique ?
                CheckDocLigneForUniqueArticle(doc, ligne);

                foreach (IBODocumentAchatLigne3 docLigne in doc.FactoryDocumentLigne.List)
                {
                    if (docLigne.InfoLibre["DlNo"] == ligne.DlNo.ToString())
                    {
                        saveLigneAchat(docLigne, ligne);

                        foreach (IBODocumentLigneLienCM cm in docLigne.FactoryDocumentLigneLienCM.List)
                        {
                            saveLigneVente((IBODocumentVenteLigne3)cm.DocumentLigneOut, ligne);
                        }
                        ligne.SaveResultTo(true);
                    }
                }
            }
        }

        public void saveLigneAchat(IBODocumentAchatLigne3 docLigne, Ligne ligne)
        {
            var doc = (IBODocumentAchat3)docLigne.Document;
            if (doc.DO_Reliquat || !AuthorizedTypes.Contains(doc.DO_Type))
            {
                throw new Exception("Le document est un relicat ou le type est interdit");
            }
            docLigne.DL_Design = ligne.Designation;
            docLigne.EU_Qte = ligne.Quantite;
            docLigne.DL_Qte = ligne.Quantite;
            if (ligne.PxBaseAch != null)
            {
                docLigne.DL_PrixUnitaire = (double)ligne.PxBaseAch;
                docLigne.Remise.FromString((ligne.RemiseAch ?? 0).ToString().Replace('.', ',') + '%');

            }
            else
            {
                docLigne.DL_PrixUnitaire = ligne.PxNetAch??0;
            }
            docLigne.TxtComplementaire = ligne.TxtComplementaire;
            docLigne.WriteDefault();
            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();
                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = "UPDATE F_DOCLIGNE SET AF_RefFourniss = @refFourn, EU_Enumere = @enum WHERE DL_No = @dlNo";
                    cmd.Parameters.AddWithValue("@enum", ligne.Unite.Intitule);
                    cmd.Parameters.AddWithValue("@refFourn", ligne.RefFourn??string.Empty);
                    cmd.Parameters.AddWithValue("@dlNo", docLigne.InfoLibre["DLNo"]);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Depuis LiaisonDocVente
        /// Suppose que le texte complémentaire a été renseigné
        /// et que le typage a été vérifié en amont
        /// TODO rendre plus compatible LiaisonDocVente
        /// </summary>
        /// <param name="docLigne"></param>
        public void saveLigneAchat(IBODocumentAchatLigne3 docLigne)
        {
            Ligne ligne = new Ligne(this) {
                Designation = docLigne.DL_Design,
                Unite = new Unite() { Intitule = docLigne.EU_Enumere },
                Quantite = docLigne.DL_Qte
            };
            ligne.ParseTxtComplementaire(docLigne.TxtComplementaire);
            saveLigneAchat(docLigne, ligne);
        }

        public string ExistRefFourn(Ligne ligne)
        {
            if (ligne.Fournisseur == null || (ligne.RefFourn??string.Empty).Length == 0)
            {
                return string.Empty;
            }
            using (var cnx = new SqlConnection(cnxString))
            {
                cnx.Open();
                using (var cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = "SELECT AR_Ref FROM F_ARTFOURNISS WHERE AF_RefFourniss = @refFourn AND CT_Num = @ctNum";
                    cmd.Parameters.AddWithValue("@refFourn", ligne.RefFourn);
                    cmd.Parameters.AddWithValue("@ctNum", ligne.Fournisseur.CtNum);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            return (string)reader["AR_Ref"];
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retourne ou génère un article unique
        /// </summary>
        /// <param name="arRef">Si provient d'un autre magasin</param>
        /// <returns></returns>
        public IBOArticle3 getUniqueArticle(string arRef = null)
        {
            var bsc = GetInstance();
            if (arRef != null)
            {
                if (bsc.FactoryArticle.ExistReference(arRef))
                {
                    return bsc.FactoryArticle.ReadReference(arRef);
                }
                else
                {
                    return createUniqueArticle(arRef);
                }
            }
            else
            {
                int id = getDb(dbName).id;
                string prefix = UniqueChar + id.ToString();
                using (var cnx = new SqlConnection(cnxString))
                {
                    cnx.Open();
                    using (var cmd = cnx.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT TOP 1 AR_Ref FROM F_ARTICLE WHERE AR_Ref LIKE '{prefix}-%' AND FA_CodeFamille = 'UNIQUE' ORDER BY LEN(AR_Ref) DESC, AR_Ref DESC";
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                arRef = $"{prefix}-" + (int.Parse(reader["AR_Ref"].ToString().Split('-')[1]) + 1);
                            }
                            else
                            {
                                arRef = $"{prefix}-1";
                            }

                            return createUniqueArticle(arRef);
                        }
                    }
                }
            }
        }

        private IBOArticle3 createUniqueArticle(string arRef)
        {
            using (var cnx = new SqlConnection(cnxString))
            {
                cnx.Open();
                using (var cmd = cnx.CreateCommand())
                {
                    var bsc = GetInstance();
                    var a = (IBOArticle3)bsc.FactoryArticle.Create();
                    a.Famille = bsc.FactoryFamille.ReadCode(FamilleType.FamilleTypeDetail, "UNIQUE");
                    a.AR_Ref = arRef;
                    // Désignation permettant de ne pas declencher le trigger MB_CTRL_UNIQUE_ARTICLES
                    a.AR_Design = "ArticleUniqueFromDiversProg";
                    a.WriteDefault();
                    a.AR_Design = "Article unique";
                    a.Write();

                    return a;
                }
            }
        }

        public void CheckDocLigneForUniqueArticle(IBODocument3 doc, Ligne ligne)
        {
            if (ligne.DlNo == null)
            {
                var newLigne = (IBODocumentLigne3)doc.FactoryDocumentLigne.Create();
                var article = getUniqueArticle();
                article.Unite = GetInstance().FactoryUnite.ReadIntitule(ligne.Unite.Intitule);
                article.Write();
                newLigne.SetDefaultArticle(article, ligne.Quantite);
                newLigne.WriteDefault();
                ligne.ArRef = newLigne.Article.AR_Ref;

                using (var cnx = new SqlConnection(cnxString))
                {
                    cnx.Open();
                    using (var cmd = cnx.CreateCommand())
                    {
                        cmd.CommandText = "SELECT DL_No FROM F_DOCLIGNE WHERE AR_Ref = @arRef";
                        cmd.Parameters.AddWithValue("@arRef", newLigne.Article.AR_Ref);
                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            ligne.DlNo = (int)reader["DL_No"];
                        }
                    }
                }
            }
        }
    }
}
