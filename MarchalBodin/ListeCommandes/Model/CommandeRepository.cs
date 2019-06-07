using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MBCore.Model;
using System.Data;

namespace ListeCommandes.Model
{
    public class CommandeRepository : BaseCialAbstract
    {
        /// <summary>
        /// Retourne la liste des commandes
        /// </summary>
        /// <returns></returns>
        public Collection<Commande> GetAll(CommandeFiltre filtre)
        {
            Collection<Commande> list = new Collection<Commande>();

            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();

                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    // Date livraison
                    cmd.CommandText =
                        $@"SELECT DE.DO_Piece, DE.DO_Date, DE.DO_Type, DE.DO_Statut, DE.[Date Statut], DE.DO_TotalHT, DE.DO_Ref, DE.DO_Reliquat,
                            CT.CT_Num, CT.CT_Intitule, CONCAT(CO.CO_Prenom, ' ', CO.CO_Nom) AS Collaborateur,
                            CASE WHEN YEAR(DE.DO_DateLivr) < 2000 THEN NULL ELSE DE.DO_DateLivr END AS DO_DateLivr,
                            (SELECT COUNT(DISTINCT DL.NumPiece)
                                FROM F_DOCLIGNE DL
                                JOIN F_CMLIEN CM ON CM.DL_NoIn = DL.DL_No
                                WHERE DL.DO_Piece = DE.DO_Piece) AS TotalCM,
                            CASE 
                                WHEN ISNULL(DE.RETRO_FOURN, '') = '' THEN NULL
                                ELSE (SELECT CT_Intitule FROM F_COMPTET WHERE CT_Num = DE.DO_Tiers)
                            END AS MagFourn, 
                            (SELECT CONCAT(CO_Prenom, ' ', CO_Nom) FROM F_COLLABORATEUR WHERE CO_No = CT.CO_No) AS AcheteurPrinc
                            , DE.RETRO_FOURN, DE.DO_Coord01
                            FROM F_DOCENTETE DE
                            LEFT JOIN F_COLLABORATEUR CO ON CO.CO_No = DE.CO_No
                            JOIN F_COMPTET CT ON CT.CT_Num = 
                                CASE 
                                    WHEN ISNULL(DE.RETRO_FOURN, '') <> '' THEN DE.RETRO_FOURN
                                    ELSE DE.DO_Tiers
                                END
                            WHERE DE.DO_Domaine = 1
                            {filtre.ToString()}
                            ORDER BY CT.CT_Intitule, DE.DO_Piece";

                    if (filtre.DateLivFrom != null)
                    {
                        cmd.Parameters.Add("@DateLivFrom", SqlDbType.Date).Value = filtre.DateLivFrom;
                    }
                    if (filtre.DateLivTo != null)
                    {
                        cmd.Parameters.Add("@DateLivTo", SqlDbType.Date).Value = filtre.DateLivTo;
                    }
                    if (filtre.DateDocFrom != null)
                    {
                        cmd.Parameters.Add("@DateDocFrom", SqlDbType.Date).Value = filtre.DateDocFrom;
                    }

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return list;

                        while (reader.Read())
                        {
                            list.Add(new Commande()
                            {
                                Piece = (string) reader["DO_Piece"],
                                Type = (CommandeType) int.Parse(reader["DO_Type"].ToString()) ,
                                Statut = (CommandeStatut) int.Parse(reader["DO_Statut"].ToString()),
                                DateStatut = reader["Date Statut"] == DBNull.Value ? (DateTime?)null:(DateTime) reader["Date Statut"],
                                Fournisseur = (string) reader["CT_Intitule"],
                                AcheteurPrinc = (reader["AcheteurPrinc"] == DBNull.Value) ? "" : (string) reader["AcheteurPrinc"],
                                MagFourn = (reader["MagFourn"] == DBNull.Value) ? "": (string) reader["MagFourn"],
                                TotalHT = Convert.ToDouble(reader["DO_TotalHT"]),
                                CtNum = (string) reader["CT_Num"],
                                TotalCm = (int) reader["TotalCM"],
                                DateLivraison = reader["DO_DateLivr"] == DBNull.Value ? (DateTime?)null:(DateTime) reader["DO_DateLivr"],
                                Date = (DateTime) reader["Do_Date"],
                                DoRef = (string) reader["DO_Ref"],
                                Collaborateur = (string) reader["Collaborateur"],
                                Relicat = int.Parse(reader["DO_Reliquat"].ToString()) == 1,
                                Entete1 = (string) reader["DO_Coord01"]
                            });
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Retourne la liste des commandes avec contremarque
        /// </summary>
        /// <returns></returns>
        public Collection<Commande> GetAllCm(CommandeFiltre filtre)
        {
            var list = new Collection<Commande>();

            using (var cnx = new SqlConnection(cnxString))
            {
                cnx.Open();

                using (var cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = $@"SELECT DEA.DO_Piece AS APiece, DEA.DO_TotalHT AS ATotal, DEA.DO_Type AS AType, DEA.DO_Statut AS AStatus, DEA.[Date Statut] AS ADate, FOURN.CT_Intitule AS Fournisseur, CONCAT(COA.CO_Prenom, ' ', COA.CO_Nom) AS Acheteur,
                        CASE WHEN YEAR(DEA.DO_DateLivr) = 1900 THEN NULL ELSE DEA.DO_DateLivr END AS DO_DateLivr, DEA.DO_Reliquat,
                        DLV.DO_Piece AS VPiece, DLV.DL_MontantHT AS VTotal, DEV.[Date Statut] AS VDate, CLI.CT_Intitule AS Client, DLV.AR_Ref, DLA.AF_RefFourniss, DLV.DL_Design, CM.CM_Qte AS CMQte, DLV.DL_Qte AS DLQte, CONCAT(COV.CO_Prenom, ' ', COV.CO_Nom) AS Vendeur
                        FROM F_CMLIEN CM
                        JOIN F_DOCLIGNE DLV ON DLV.DL_No = CM.DL_NoOut
                        JOIN F_DOCLIGNE DLA ON DLA.DL_No = CM.DL_NoIn
                        JOIN F_DOCENTETE DEA ON DEA.DO_Piece = DLA.DO_Piece
                        JOIN F_DOCENTETE DEV ON DEV.DO_Piece = DLV.DO_Piece
                        JOIN F_COMPTET CLI ON CLI.CT_Num = DLV.CT_Num
                        JOIN F_COMPTET FOURN ON FOURN.CT_Num = DLA.CT_Num
                        LEFT JOIN F_COLLABORATEUR COA ON COA.CO_No = DLA.CO_No
                        LEFT JOIN F_COLLABORATEUR COV ON COV.CO_No = DLV.CO_No
                        WHERE DEV.DO_Type IN(1,2)
                        {filtre.ToString().Replace("DE.", "DEA.")}
                        ORDER BY Fournisseur, AType, APiece";

                    if (filtre.DateLivFrom != null)
                    {
                        cmd.Parameters.Add("@DateLivFrom", SqlDbType.Date).Value = filtre.DateLivFrom;
                    }
                    if (filtre.DateLivTo != null)
                    {
                        cmd.Parameters.Add("@DateLivTo", SqlDbType.Date).Value = filtre.DateLivTo;
                    }
                    if (filtre.DateDocFrom != null)
                    {
                        cmd.Parameters.Add("@DateDocFrom", SqlDbType.Date).Value = filtre.DateDocFrom;
                    }
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            var dict = new Dictionary<string, Commande>();

                            while (reader.Read())
                            {
                                string piece = reader["APiece"].ToString();

                                if (!dict.ContainsKey(piece))
                                {
                                    dict.Add(piece, new Commande()
                                    {
                                        Piece = piece,
                                        TotalHT = Convert.ToDouble(reader["ATotal"]),
                                        Type = (CommandeType) int.Parse(reader["AType"].ToString()),
                                        Statut = (CommandeStatut) int.Parse(reader["AStatus"].ToString()),
                                        DateStatut = reader["ADate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["ADate"],
                                        Fournisseur = (string) reader["Fournisseur"],
                                        Lignes = new Collection<LigneCommande>(),
                                        Relicat = int.Parse(reader["DO_Reliquat"].ToString()) == 1
                                    });
                                }

                                dict[piece].Lignes.Add(new LigneCommande()
                                {
                                    Acheteur = (string)reader["Acheteur"],
                                    Piece = (string)reader["VPiece"],
                                    DateStatut = reader["VDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["VDate"],
                                    Client = (string)reader["Client"],
                                    ArRef = (string)reader["AR_Ref"],
                                    RefFourn = (string)reader["AF_RefFourniss"],
                                    Designation = (string)reader["DL_Design"],
                                    CmQte = Convert.ToDouble(reader["CMQte"]),
                                    CliQte = Convert.ToDouble(reader["DLQte"]),
                                    Vendeur = (string)reader["Vendeur"],
                                    TotalHT = Convert.ToDouble(reader["VTotal"])
                                });
                            }

                            foreach (KeyValuePair<string, Commande> commande in dict)
                            {
                                list.Add(commande.Value);
                            }
                        }
                    }
                }
            }

            return list;
        }

        public Collection<Commande> GetAllRecep()
        {
            Collection<Commande> list = new Collection<Commande>();

            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();

                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    // Date livraison
                    cmd.CommandText =
                        $@"SELECT DE.DO_Piece, DE.DO_Date, DE.DO_Type, DE.DO_Statut, DE.[Date Statut], DE.DO_TotalHT, DE.DO_Ref, DE.DO_Reliquat,
                            CT.CT_Num, CT.CT_Intitule, CONCAT(CO.CO_Prenom, ' ', CO.CO_Nom) AS Collaborateur,
                            CASE WHEN YEAR(DE.DO_DateLivr) = 1900 THEN NULL ELSE DE.DO_DateLivr END AS DO_DateLivr,
                            (SELECT COUNT(DISTINCT DL.NumPiece)
                                FROM F_DOCLIGNE DL
                                JOIN F_CMLIEN CM ON CM.DL_NoIn = DL.DL_No
                                WHERE DL.DO_Piece = DE.DO_Piece) AS TotalCM,
                            (SELECT COUNT(DISTINCT DL.NumPiece)
                                FROM F_DOCLIGNE DL
                                JOIN F_CMLIEN CM ON CM.DL_NoIn = DL.DL_No
                                WHERE DL.DO_Piece = DE.DO_Piece AND DL.AR_Ref = 'PT') AS TotalCMPT,
                            CASE 
                                WHEN ISNULL(DE.RETRO_FOURN, '') = '' THEN NULL
                                ELSE (SELECT CT_Intitule FROM F_COMPTET WHERE CT_Num = DE.DO_Tiers)
                            END AS MagFourn, 
                            (SELECT CONCAT(CO_Prenom, ' ', CO_Nom) FROM F_COLLABORATEUR WHERE CO_No = CT.CO_No) AS AcheteurPrinc
                            , DE.RETRO_FOURN, DE.DO_Coord01
                            FROM F_DOCENTETE DE
                            LEFT JOIN F_COLLABORATEUR CO ON CO.CO_No = DE.CO_No
                            JOIN F_COMPTET CT ON CT.CT_Num = 
                                CASE 
                                    WHEN ISNULL(DE.RETRO_FOURN, '') <> '' THEN DE.RETRO_FOURN
                                    ELSE DE.DO_Tiers
                                END
                            WHERE DE.DO_Domaine = 1
                            AND DE.DO_Type = 12
                            AND DE.DO_Statut = 2
                            ORDER BY TotalCMPT DESC, TotalCM DESC, DE.DO_Date, DE.[Date Statut]";

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return list;

                        while (reader.Read())
                        {
                            list.Add(new Commande()
                            {
                                Piece = (string)reader["DO_Piece"],
                                Type = (CommandeType)int.Parse(reader["DO_Type"].ToString()),
                                Statut = (CommandeStatut)int.Parse(reader["DO_Statut"].ToString()),
                                DateStatut = reader["Date Statut"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["Date Statut"],
                                Fournisseur = (string)reader["CT_Intitule"],
                                AcheteurPrinc = (reader["AcheteurPrinc"] == DBNull.Value) ? "" : (string)reader["AcheteurPrinc"],
                                MagFourn = (reader["MagFourn"] == DBNull.Value) ? "" : (string)reader["MagFourn"],
                                TotalHT = Convert.ToDouble(reader["DO_TotalHT"]),
                                CtNum = (string)reader["CT_Num"],
                                TotalCm = (int)reader["TotalCM"],
                                DateLivraison = reader["DO_DateLivr"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["DO_DateLivr"],
                                Date = (DateTime)reader["Do_Date"],
                                DoRef = (string)reader["DO_Ref"],
                                Collaborateur = (string)reader["Collaborateur"],
                                Relicat = int.Parse(reader["DO_Reliquat"].ToString()) == 1,
                                Entete1 = (string)reader["DO_Coord01"]
                            });
                        }
                    }
                }
            }
            return list;
        }

        internal Collection<Commande> GetSample()
        {
            return new Collection<Commande>()
            {
                new Commande()
                {
                    Piece = "APC00097",
                    Type = CommandeType.PreparationCommande,
                    Statut = CommandeStatut.Accepte,
                    Fournisseur = "STANLEY",
                    MagFourn = "BODIN",
                    TotalHT = 666.66,
                    AcheteurPrinc = "EMMANUEL BELAIR"
                },
                new Commande()
                {
                    Piece = "APC00098",
                    Type = CommandeType.BonCommandeFournisseur,
                    Statut = CommandeStatut.Accepte,
                    Fournisseur = "STANLEY",
                    MagFourn = null,
                    TotalHT = 777.77,
                    AcheteurPrinc = "EMMANUEL BELAIR"
                },
                new Commande()
                {
                    Piece = "APC00099",
                    Type = CommandeType.PreparationCommande,
                    Statut = CommandeStatut.Accepte,
                    Fournisseur = "FOOBAR",
                    MagFourn = "SMO",
                    TotalHT = 888.88,
                    AcheteurPrinc = "EMMANUEL BELAIR"
                },
                new Commande()
                {
                    Piece = "APC00100",
                    Type = CommandeType.BonCommandeFournisseur,
                    Statut = CommandeStatut.Accepte,
                    Fournisseur = "FOOBAR",
                    MagFourn = null,
                    TotalHT = 777.77,
                    AcheteurPrinc = "EMMANUEL BELAIR"
                },
            };
        }
    }
}
