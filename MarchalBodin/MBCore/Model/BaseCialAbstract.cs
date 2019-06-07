using System;
using Microsoft.Win32;
using Objets100cLib;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.DirectoryServices;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Data.SqlClient;

namespace MBCore.Model
{
    public abstract class BaseCialAbstract : DbManager
    {
        private static readonly BSCIALApplication100c Bscial = new BSCIALApplication100c();

        public static readonly string ADMINUSR = "<Administrateur>";
        public static readonly string ADMINPWD = "supercoop*";

        public static BSCIALApplication100c GetInstance()
        {
            return Bscial;
        }

        public string cnxString => getCnxString(Bscial.Name);

        public string dbName => getDbName(Bscial.Name);

        public string fichierGescom => Bscial.Name;

        /// <summary>
        /// Utilisateur Sage
        /// </summary>
        private static string _user;

        /// <summary>
        /// Utilisateur Sage
        /// </summary>
        public string user => _user;

        /// <summary>
        /// TODO recup le password utilisateur qui n'est plus en argument des programmes externes
        /// Pour le moment exécute les programmes en admin
        /// </summary>
        /// <param name="fichierGescom"></param>
        /// <param name="user"></param>
        public static void setDefaultParams(string fichierGescom, string user)
        {
            _user = user;
            setDefaultParams(fichierGescom);
            //Bscial.Name = fichierGescom;
            //Bscial.Loggable.UserName = user;
            //Bscial.Loggable.UserPwd = "";
        }

        public static void setDefaultParams(string fichierGescom)
        {
            Bscial.Name = fichierGescom;
            Bscial.Loggable.UserName = ADMINUSR;
            Bscial.Loggable.UserPwd = ADMINPWD;
        }

        /// <summary>
        /// Retourne la liste des databases en fonction du contexte
        /// eg. Si la base est une base de dev, renverra que les bases de dev
        /// </summary>
        /// <returns></returns>
        public List<Database> getDbListFromContext()
        {
            return dbList.Where(d => d.context == getDb(dbName).context).ToList();
        }

        public bool openBaseCial()
        {
            try
            {
                if (Bscial.IsOpen)
                {
                    return true;
                }
                Bscial.Open();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool closeBaseCial()
        {
            try
            {
                Bscial.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetProcessError(IPMProcess proc)
        {
            try
            {
                string message = "";
                foreach (IFailInfo f in proc.Errors)
                {
                    message += string.Format("Code Erreur: {0} Indice: {1} Description: {2}. {3}", f.ErrorCode, f.Indice, f.Text, Environment.NewLine);
                }

                return message;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public static string FindSqlServer()
        {
            IPAddress[] ips = System.Net.Dns.GetHostAddresses(Dns.GetHostName());
            DirectoryEntry rootEntry = new DirectoryEntry();
            DirectorySearcher searcher = new DirectorySearcher(rootEntry);
            searcher.Filter = "";
            searcher.Filter = "objectClass=Computer";
            SearchResultCollection results = searcher.FindAll();
            Regex regex = new Regex("^CN=SRVSQL[0-9]{2}$");
            foreach (SearchResult r in results)
            {
                // Debug.Print(r.GetDirectoryEntry.Name)
                if (regex.IsMatch(r.GetDirectoryEntry().Name))
                {
                    string hostname = r.GetDirectoryEntry().Name.Replace("CN=", "");
                    IPHostEntry host = Dns.GetHostEntry(hostname);
                    //Debug.Print(hostname);
                    foreach (IPAddress ip in ips)
                    {
                        string[] ipStrs = ip.ToString().Split('.');
                        if (ipStrs[0] == "192"
                            && "SRVSQL" + ipStrs[2].PadLeft(2, '0') == hostname)
                        {
                            return host.HostName.ToUpper();
                        }
                    }
                }
            }

            throw new Exception("Serveur non trouvé");
        }

        public void OpenDocument(string DoPiece)
        {
            OpenDocument(GetDocument(DoPiece));
        }

        public void OpenDocument(IBODocument3 doc)
        {
            try
            {
                Process.Start(getSagePath(), $"{fichierGescom} -u=\"{user}\" -cmd=\"Document.Show(Type={GetProgExtDocType(doc.DO_Type)},Piece='{doc.DO_Piece}')\"");
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
            }
        }

        public IBODocument3 GetDocument(string DoPiece)
        {
            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();
                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = "SELECT DO_Type FROM F_DOCENTETE WHERE DO_Piece = @doPiece";
                    cmd.Parameters.AddWithValue("@doPiece", DoPiece);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            return null;
                        }
                        reader.Read();
                        short DoType = (short)reader["DO_Type"];
                        var bsc = GetInstance();
                        if (!openBaseCial())
                        {
                            throw new Exception("Impossible de se connecter à Sage");
                        }
                        switch (GetDocTypeFromSqlValue(DoType))
                        {
                            case DocumentType.DocumentTypeVenteDevis:
                                return bsc.FactoryDocumentVente.ReadPiece(DocumentType.DocumentTypeVenteDevis, DoPiece);
                            case DocumentType.DocumentTypeVenteCommande:
                                return bsc.FactoryDocumentVente.ReadPiece(DocumentType.DocumentTypeVenteCommande, DoPiece);
                            case DocumentType.DocumentTypeVentePrepaLivraison:
                                return bsc.FactoryDocumentVente.ReadPiece(DocumentType.DocumentTypeVentePrepaLivraison, DoPiece);
                            case DocumentType.DocumentTypeVenteLivraison:
                                return bsc.FactoryDocumentVente.ReadPiece(DocumentType.DocumentTypeVenteLivraison, DoPiece);
                            case DocumentType.DocumentTypeVenteReprise:
                                return bsc.FactoryDocumentVente.ReadPiece(DocumentType.DocumentTypeVenteReprise, DoPiece);
                            case DocumentType.DocumentTypeVenteAvoir:
                                return bsc.FactoryDocumentVente.ReadPiece(DocumentType.DocumentTypeVenteAvoir, DoPiece);
                            case DocumentType.DocumentTypeVenteFacture:
                                return bsc.FactoryDocumentVente.ReadPiece(DocumentType.DocumentTypeVenteFacture, DoPiece);
                            case DocumentType.DocumentTypeVenteFactureCpta:
                                return bsc.FactoryDocumentVente.ReadPiece(DocumentType.DocumentTypeVenteFactureCpta, DoPiece);
                            case DocumentType.DocumentTypeAchatCommande:
                                return bsc.FactoryDocumentAchat.ReadPiece(DocumentType.DocumentTypeAchatCommande, DoPiece);
                            case DocumentType.DocumentTypeAchatCommandeConf:
                                return bsc.FactoryDocumentAchat.ReadPiece(DocumentType.DocumentTypeAchatCommandeConf, DoPiece);
                            case DocumentType.DocumentTypeAchatLivraison:
                                return bsc.FactoryDocumentAchat.ReadPiece(DocumentType.DocumentTypeAchatLivraison, DoPiece);
                            case DocumentType.DocumentTypeAchatReprise:
                                return bsc.FactoryDocumentAchat.ReadPiece(DocumentType.DocumentTypeAchatReprise, DoPiece);
                            case DocumentType.DocumentTypeAchatAvoir:
                                return bsc.FactoryDocumentAchat.ReadPiece(DocumentType.DocumentTypeAchatAvoir, DoPiece);
                            case DocumentType.DocumentTypeAchatFacture:
                                return bsc.FactoryDocumentAchat.ReadPiece(DocumentType.DocumentTypeAchatFacture, DoPiece);
                            case DocumentType.DocumentTypeAchatFactureCpta:
                                return bsc.FactoryDocumentAchat.ReadPiece(DocumentType.DocumentTypeAchatFactureCpta, DoPiece);
                            case DocumentType.DocumentTypeStockMouvIn:
                                return bsc.FactoryDocumentStock.ReadPiece(DocumentType.DocumentTypeStockMouvIn, DoPiece);
                            case DocumentType.DocumentTypeStockMouvOut:
                                return bsc.FactoryDocumentStock.ReadPiece(DocumentType.DocumentTypeStockMouvOut, DoPiece);
                            case DocumentType.DocumentTypeStockDeprec:
                                return bsc.FactoryDocumentStock.ReadPiece(DocumentType.DocumentTypeStockDeprec, DoPiece);
                            case DocumentType.DocumentTypeStockVirement:
                                return bsc.FactoryDocumentStock.ReadPiece(DocumentType.DocumentTypeStockVirement, DoPiece);
                            case DocumentType.DocumentTypeStockPreparation:
                                return bsc.FactoryDocumentStock.ReadPiece(DocumentType.DocumentTypeStockPreparation, DoPiece);
                            case DocumentType.DocumentTypeStockOrdreFabrication:
                                return bsc.FactoryDocumentStock.ReadPiece(DocumentType.DocumentTypeStockOrdreFabrication, DoPiece);
                            case DocumentType.DocumentTypeStockFabrication:
                                return bsc.FactoryDocumentStock.ReadPiece(DocumentType.DocumentTypeStockFabrication, DoPiece);
                            case DocumentType.DocumentTypeInterne1:
                                return bsc.FactoryDocumentInterne.ReadPiece(DocumentType.DocumentTypeInterne1, DoPiece);
                            case DocumentType.DocumentTypeInterne2:
                                return bsc.FactoryDocumentInterne.ReadPiece(DocumentType.DocumentTypeInterne2, DoPiece);
                            case DocumentType.DocumentTypeInterne3:
                                return bsc.FactoryDocumentInterne.ReadPiece(DocumentType.DocumentTypeInterne3, DoPiece);
                            case DocumentType.DocumentTypeInterne4:
                                return bsc.FactoryDocumentInterne.ReadPiece(DocumentType.DocumentTypeInterne4, DoPiece);
                            case DocumentType.DocumentTypeInterne5:
                                return bsc.FactoryDocumentInterne.ReadPiece(DocumentType.DocumentTypeInterne5, DoPiece);
                            case DocumentType.DocumentTypeInterne6:
                                return bsc.FactoryDocumentInterne.ReadPiece(DocumentType.DocumentTypeInterne6, DoPiece);
                            case DocumentType.DocumentTypeInterne7:
                                return bsc.FactoryDocumentInterne.ReadPiece(DocumentType.DocumentTypeInterne7, DoPiece);
                            default:
                                throw new Exception("Type invalide");
                        }
                    }
                }

            }
        }

        public static DocumentType GetDocTypeFromSqlValue(int type)
        {
            switch (type)
            {
                case 0:
                    return DocumentType.DocumentTypeVenteDevis;
                case 1:
                    return DocumentType.DocumentTypeVenteCommande;
                case 2:
                    return DocumentType.DocumentTypeVentePrepaLivraison;
                case 3:
                    return DocumentType.DocumentTypeVenteLivraison;
                case 4:
                    return DocumentType.DocumentTypeVenteReprise;
                case 5:
                    return DocumentType.DocumentTypeVenteAvoir;
                case 6:
                    return DocumentType.DocumentTypeVenteFacture;
                case 7:
                    return DocumentType.DocumentTypeVenteFactureCpta;
                case 11:
                    return DocumentType.DocumentTypeAchatCommande;
                case 12:
                    return DocumentType.DocumentTypeAchatCommandeConf;
                case 13:
                    return DocumentType.DocumentTypeAchatLivraison;
                case 14:
                    return DocumentType.DocumentTypeAchatReprise;
                case 15:
                    return DocumentType.DocumentTypeAchatAvoir;
                case 16:
                    return DocumentType.DocumentTypeAchatFacture;
                case 17:
                    return DocumentType.DocumentTypeAchatFactureCpta;
                default:
                    throw new Exception("Type invalide");
            }
        }

        /// <summary>
        /// Retourne le type de document conforme au programme externe
        /// </summary>
        /// <param name="type"></param>
        public static string GetProgExtDocType(DocumentType type)
        {
            switch (type)
            {
                case DocumentType.DocumentTypeVenteDevis:
                    return "Devis";
                case DocumentType.DocumentTypeVenteCommande:
                    return "BonCommandeClient";
                case DocumentType.DocumentTypeVentePrepaLivraison:
                    return "PreparationLivraison";
                case DocumentType.DocumentTypeVenteLivraison:
                    return "BonLivraisonClient";
                case DocumentType.DocumentTypeVenteReprise:
                    return "BonRetourClient";
                case DocumentType.DocumentTypeVenteAvoir:
                    return "BonAvoirClient";
                case DocumentType.DocumentTypeVenteFacture:
                    return "FactureClient";
                case DocumentType.DocumentTypeVenteFactureCpta:
                    return "FactureComptaClient";
                case DocumentType.DocumentTypeAchatCommande:
                    return "PreparationCommande";
                case DocumentType.DocumentTypeAchatCommandeConf:
                    return "BonCommandeFournisseur";
                case DocumentType.DocumentTypeAchatLivraison:
                    return "BonLivraisonFournisseur";
                case DocumentType.DocumentTypeAchatReprise:
                    return "BonRetourFournisseur";
                case DocumentType.DocumentTypeAchatAvoir:
                    return "BonAvoirFournisseur";
                case DocumentType.DocumentTypeAchatFacture:
                    return "FactureFournisseur";
                case DocumentType.DocumentTypeAchatFactureCpta:
                    return "FactureComptaFournisseur";
                case DocumentType.DocumentTypeStockMouvIn:
                    return "MouvementEntree";
                case DocumentType.DocumentTypeStockMouvOut:
                    return "MouvementSortie";
                case DocumentType.DocumentTypeStockDeprec:
                    return "DepreciationStock";
                case DocumentType.DocumentTypeStockVirement:
                    return "MouvementTransfert";
                case DocumentType.DocumentTypeStockPreparation:
                    return "PreparationFabrication";
                case DocumentType.DocumentTypeStockOrdreFabrication:
                    return "OrdreFabrication";
                case DocumentType.DocumentTypeStockFabrication:
                    return "BonFabrication";
                case DocumentType.DocumentTypeInterne1:
                    return "Type1";
                case DocumentType.DocumentTypeInterne2:
                    return "Type2";
                case DocumentType.DocumentTypeInterne3:
                    return "Type3";
                case DocumentType.DocumentTypeInterne4:
                    return "Type4";
                case DocumentType.DocumentTypeInterne5:
                    return "Type5";
                case DocumentType.DocumentTypeInterne6:
                    return "Type6";
                case DocumentType.DocumentTypeInterne7:
                    return "Type7";
                default:
                    throw new Exception("Type invalide");
            }
        }

    }
}
