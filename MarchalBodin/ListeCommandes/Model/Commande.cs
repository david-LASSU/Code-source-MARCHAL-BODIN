using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ListeCommandes.Model
{
    public class Commande
    {
        public const int EMERGENCY_HIGHT = 100;
        public const int EMERGENCY_MEDIUM = 50;
        public const int EMERGENCY_LOW = 10;

        public static Dictionary<CommandeType, string> TypesArrayLabels => new Dictionary<CommandeType, string>()
        {
            {CommandeType.PreparationCommande, "Préparation de commande"},
            {CommandeType.BonCommandeFournisseur, "Bon de commande"},
            {CommandeType.BonLivraisonFournisseur, "Bon de livraison"},
            {CommandeType.BonRetourFournisseur, "Bon de retour"},
            {CommandeType.BonAvoirFournisseur, "Bon d'avoir financier"},
            {CommandeType.FactureFournisseur, "Facture"},
            {CommandeType.FactureComptaFournisseur, "Facture comptabilisée"},
            //{CommandeType.AchatArchive, "Archive"}
        };

        public string TypeLabel => TypesArrayLabels[Type];

        public static Dictionary<CommandeStatut, string> StatutsArrayLabels => new Dictionary<CommandeStatut, string>()
        {
            {CommandeStatut.Saisi, "1 - Saisi" },
            {CommandeStatut.Confirme, "2 - Confirmé, Envoyé" },
            {CommandeStatut.Accepte, "3 - Accepté, Envoyé, Réceptionné ..." }
        };

        public static Dictionary<ApcType, string> ApcTypesArrayLabels => new Dictionary<ApcType, string>()
        {
            {ApcType.DemandeDePrixClient, "Demande de prix client" },
            {ApcType.DemandeDePrixStock, "Demande de prix stock" },
            {ApcType.Reservation, "Réservation client" },
            {ApcType.Stock, "Stock" }
        };

        public string StatutLabel
        {
            get
            {
                switch (Statut)
                {
                    case CommandeStatut.Saisi:
                        return "Saisi";
                    case CommandeStatut.Confirme:
                        if (Type == CommandeType.BonCommandeFournisseur)
                        {
                            return "Envoyé";
                        }
                        return "Confirmé";
                    case CommandeStatut.Accepte:
                        switch (Type)
                        {
                            case CommandeType.PreparationCommande:
                                return "Accepté";
                            case CommandeType.BonCommandeFournisseur:
                                return "Réceptionné";
                            case CommandeType.BonLivraisonFournisseur:
                                return "Réceptionné";
                            case CommandeType.BonRetourFournisseur:
                            case CommandeType.BonAvoirFournisseur:
                                return "A facturer";
                            case CommandeType.FactureFournisseur:
                                return "A comptabiliser";
                            case CommandeType.FactureComptaFournisseur:
                                return "Comptabilisé";
                        }
                        return "???";
                }

                return "???";
            }
        }

        public string Piece { get; set; }

        public bool Relicat { get; set; }

        /// <summary>
        /// todo Convertir Type en Enum
        /// </summary>
        public CommandeType Type { get; set; }

        public CommandeStatut Statut { get; set; }

        public DateTime? DateStatut { get; set; }

        public string Fournisseur { get; set; }

        public string CtNum { get; set; }

        public string AcheteurPrinc { get; set; }

        public string MagFourn { get; set; }

        public double TotalHT { get; set; }

        public DateTime? DateLivraison { get; set; }

        public DateTime Date { get; set; }

        /// <summary>
        /// Total des lignes en contremarque pour le document
        /// </summary>
        public int TotalCm { get; set; }

        public Collection<LigneCommande> Lignes { get; set; }

        public string DoRef { get; set; }
        
        public string Collaborateur { get; set; }

        public string Entete1 { get; set; }

        /// <summary>
        /// Check si la commande n'a pas été envoyée
        /// Retourne true si c'est un APC ou un ABC avec Statut != Envoyé
        /// </summary>
        public bool IsNonEnvoye => Type == CommandeType.PreparationCommande || (Type == CommandeType.BonCommandeFournisseur && Statut != CommandeStatut.Accepte);

        public int EmergencyLevel
        {
            get
            {
                if (
                    // APC/ABC non envoyé avec plus de 3 contremarques
                    (TotalCm > 3 && IsNonEnvoye && !Relicat)
                    // APC / ABC non envoyé avec livraison < Aujourd'hui
                    || (DateLivraison.HasValue && DateLivraison.Value < DateTime.Today && IsNonEnvoye && !Relicat)
                    // APC/ABC avec DO_Date > à 21 jours
                    || (Date < DateTime.Today.AddDays(-21) && !Relicat)
                    // Commande envoyée non livrée
                    || (Type == CommandeType.BonCommandeFournisseur && Statut == CommandeStatut.Confirme && DateLivraison.HasValue && DateLivraison.Value < DateTime.Today && !Relicat)
                    // Commande à quai > 1 semaine
                    || (Type == CommandeType.BonCommandeFournisseur && Statut == CommandeStatut.Accepte && DateLivraison.HasValue && DateLivraison.Value < DateTime.Today.AddDays(-7))
                    )
                {
                    return EMERGENCY_HIGHT;
                }

                if (
                    // Commande envoyée à 4 jours de la livraison
                    (Type == CommandeType.BonCommandeFournisseur && Statut == CommandeStatut.Confirme && DateLivraison.HasValue && DateLivraison.Value < DateTime.Today.AddDays(4) && !Relicat)
                    // APC date de livraison < 4 jours
                    || (Type == CommandeType.PreparationCommande && DateLivraison.HasValue && DateLivraison.Value < DateTime.Today.AddDays(4) && !Relicat)
                    )
                {
                    return EMERGENCY_MEDIUM;
                }

                return EMERGENCY_LOW;
            }
        }
    }
}
