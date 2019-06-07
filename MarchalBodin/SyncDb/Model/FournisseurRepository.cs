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
    public class FournisseurRepository : AbstractRepository
    {
        private List<string> vCols = new List<string>() { "CT_Num", "csTiers", "CS", "csContactTAgg", "csBanqueTAgg", "csReglementTAgg", "ctNumPos" };
        public FournisseurRepository(Database TargetDb, Database SourceDb = null) : base(TargetDb, SourceDb)
        {
            targetDb = TargetDb;
            sourceDb = SourceDb;
        }

        public override List<Ligne> GetLignes(string PkFilter = null)
        {
            using (var cnx = new SqlConnection(sourceDb.cnxString))
            {
                cnx.Open();
                using (var cmd = cnx.CreateCommand())
                {
                    // ctNumPos: Avoir d'abord les comptes payeurs si différent et éviter ainsi les contraintes d'integrité
                    cmd.CommandText = $@"SELECT L.PkValue, CTR.CT_Num,
                                        CASE WHEN  CT.CT_Num <> CT.CT_NumPayeur OR CT.CT_Num <> CT.CT_NumCentrale THEN 2 ELSE 1 END AS ctNumPos
                                        FROM {sourceDb.dboChain}.[MB_SYNCSTATE] L
                                        JOIN {sourceDb.dboChain}.[F_COMPTET] CT ON CT.CT_Num = L.PkValue AND CT.CT_Type = 1
                                        LEFT JOIN {targetDb.dboChain}.[MB_SYNCSTATE] R ON R.ObjectName = L.ObjectName AND R.PkValue = L.PkValue
                                        LEFT JOIN {targetDb.dboChain}.[F_COMPTET] CTR ON CTR.CT_Num = L.PkValue
                                        WHERE L.ObjectName = 'TIERS' AND (R.LastUpdate < L.LastUpdate OR R.LastUpdate IS NULL)
                                        ORDER BY ctNumPos, L.PkValue";

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
                                      ObjectName = "TIERS",
                                      PKValue = (string)row["PkValue"],
                                      IsInsert = row["CT_Num"] == DBNull.Value
                                  }).ToList();

                        return lignes;
                    }
                }
            }
        }

        /// <summary>
        /// @INFO:
        /// Champs à renseigner obligatoirement lors de l’ajout
        ///
        /// CT_Num
        /// CT_Intitule
        /// CT_Type
        /// CG_NumPrinc
        /// CT_NumPayeur
        /// N_Risque
        /// N_CatTarif
        /// N_CatCompta
        /// N_Period
        /// N_Expedition
        /// N_Condition
        ///
        /// Champs non modifiables en modification d’enregistrement
        ///
        /// CT_Num
        /// CT_Type
        /// CT_DateCreate
        /// N_Analytique
        /// </summary>
        /// <param name="ligne"></param>
        /// <returns></returns>
        public override bool MajLigne(Ligne ligne)
        {
            // Liste des champs à exclure de l'update
            // Le BT_Num sera géré après la création des banques
            var updBlackList = new List<string>() { "BT_Num", "CT_Num", "CT_Type", "CT_DateCreate", "N_Analytique"};

            RaiseLogEvent(string.Format("[{0}] {1}", ligne.PKValue, ligne.IsInsert ? "INSERT" : "UPDATE"));

            using (var cnxTarget = new SqlConnection(targetDb.cnxString))
            {
                cnxTarget.Open();
                SqlTransaction transaction = cnxTarget.BeginTransaction();
                try
                {
                    using (var cnxSource = new SqlConnection(sourceDb.cnxString))
                    {
                        cnxSource.Open();

                        //
                        // F_COMPTET
                        //
                        using (var cmdSource = GetSelectCmdFromCnx(
                            cnxSource,
                            "F_COMPTET",
                            new List<string>() {"CT_Num", "CT_Intitule", "CT_Type", "CG_NumPrinc", "CT_Qualite", "CT_Classement", "CT_Contact", "CT_Adresse", "CT_Complement", "CT_CodePostal",
                            "CT_Ville", "CT_CodeRegion", "CT_Pays", "CT_Raccourci", "N_Devise", "CT_Ape", "CT_Identifiant", "CT_Siret", "CT_Commentaire", "CT_Encours",
                            "CT_Assurance", "CT_NumPayeur", "N_Risque", "N_CatTarif", "CT_Taux01", "CT_Taux02", "CT_Taux03", "CT_Taux04", "N_CatCompta", "N_Period",
                            "CT_Facture", "CT_BLFact", "CT_Langue", "N_Expedition", "N_Condition", "CT_DateCreate", "CT_Saut", "CT_Lettrage", "CT_ValidEch", "CT_Sommeil", "DE_No",
                            "CT_ControlEnc", "CT_NotRappel", "N_Analytique", "CA_Num", "CT_Telephone", "CT_Telecopie", "CT_EMail", "CT_Site", "CT_Coface", "CT_Surveillance",
                            "CT_SvDateCreate", "CT_SvFormeJuri", "CT_SvEffectif", "CT_SvCA", "CT_SvResultat", "CT_SvIncident", "CT_SvDateIncid", "CT_SvPrivil", "CT_SvRegul",
                            "CT_SvCotation", "CT_SvDateMaj", "CT_SvObjetMaj", "CT_SvDateBilan", "CT_SvNbMoisBilan", "CT_PrioriteLivr", "CT_LivrPartielle", "MR_No", "CT_NotPenal",
                            "EB_No", "CT_NumCentrale", "CT_DateFermeDebut", "CT_DateFermeFin", "CT_FactureElec", "CT_TypeNIF", "CT_RepresentInt", "CT_RepresentNIF", "Identifiant",
                            "1", "2", "3", "4", "5", "6", "7", "8", "9", "GESTION DES RELIQUATS", "CT_EdiCodeType", "CT_EdiCode", "CT_EdiCodeSage", "CT_ProfilSoc", "N° de compte",
                            "CT_StatutContrat", "CT_DateMAJ", "CT_EchangeRappro", "CT_EchangeCR", "PI_NoEchange", "CT_BonAPayer", "CT_DelaiTransport", "CT_DelaiAppro", "INFO_MAJ_TARIF",
                            "CT_LangueISO2", "Pratique escompte", "DATE_MAJ_TARIF", "FRANCO", "MAJ_URGENTE", "EN_COURS_DE_MAJ", "magasin_referent", "magasin_livraison", "MAJ_PX_VENTE_A_DATE"},
                            ligne.PKValue
                            ))
                        {
                            using (var reader = cmdSource.ExecuteReader(CommandBehavior.SingleRow))
                            {
                                reader.Read();
                                if (ligne.IsInsert)
                                {
                                    InsertRowFromReader("F_COMPTET", reader, transaction);
                                }
                                else
                                {
                                    UpdateRowFromReader("F_COMPTET", reader, transaction, updBlackList);
                                }
                            }
                        }
                        // END F_COMPTET

                        //
                        // F_CONTACTT
                        //
                        // Champs à renseigner obligatoirement lors de l’ajout
                        // CT_Num, CT_Nom, N_Service, N_Contact
                        Delete("F_CONTACTT", transaction, "CT_Num", ligne.PKValue);
                        CopySourceToTarget(
                            cnxSource,
                            "F_CONTACTT",
                            new List<string>() { "CT_Num", "CT_Nom", "CT_Prenom", "N_Service", "CT_Fonction", "CT_Telephone", "CT_TelPortable", "CT_Telecopie", "CT_EMail", "CT_Civilite", "N_Contact" },
                            ligne.PKValue,
                            transaction
                        );

                        //
                        // F_BANQUET
                        //
                        Delete("F_BANQUET", transaction, "CT_Num", ligne.PKValue);
                        CopySourceToTarget(
                            cnxSource,
                            "F_BANQUET",
                            new List<string>() { "CT_Num", "BT_Num", "BT_Intitule", "BT_Banque", "BT_Guichet", "BT_Compte", "BT_Cle", "BT_Commentaire", "BT_Struct", "N_Devise", "BT_Adresse", "BT_Complement", "BT_CodePostal", "BT_Ville", "BT_Pays", "BT_BIC", "BT_IBAN", "BT_CalculIBAN", "BT_NomAgence", "BT_CodeRegion", "BT_PaysAgence", "MD_No" },
                            ligne.PKValue,
                            transaction
                        );

                        // Met à jour la banque principale
                        using (var cmdSource = cnxSource.CreateCommand())
                        {
                            cmdSource.CommandText = $"SELECT [CT_Num], [BT_Num] FROM {sourceDb.dboChain}.[F_COMPTET] WHERE CT_Num = @ctNum";
                            cmdSource.Parameters.AddWithValue("@ctNum", ligne.PKValue);
                            cmdSource.CommandTimeout = CmdTimeOut;

                            using (var reader = cmdSource.ExecuteReader(CommandBehavior.SingleRow))
                            {
                                reader.Read();
                                UpdateRowFromReader("F_COMPTET", reader, transaction);
                            }
                        }

                        //
                        // F_REGLEMENTT
                        //
                        Delete("F_REGLEMENTT", transaction, "CT_Num", ligne.PKValue);
                        CopySourceToTarget(
                            cnxSource,
                            "F_REGLEMENTT",
                            new List<string>() { "CT_Num", "N_Reglement", "RT_Condition", "RT_NbJour", "RT_JourTb01", "RT_JourTb02", "RT_JourTb03", "RT_JourTb04", "RT_JourTb05", "RT_JourTb06", "RT_TRepart", "RT_VRepart" },
                            ligne.PKValue,
                            transaction
                        );
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
            }

            return false;
        }
    }
}
