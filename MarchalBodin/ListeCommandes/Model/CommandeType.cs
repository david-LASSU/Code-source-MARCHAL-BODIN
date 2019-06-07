using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ListeCommandes.Model
{
    public enum CommandeType
    {
        PreparationCommande = 11,
        BonCommandeFournisseur = 12,
        BonLivraisonFournisseur = 13,
        BonRetourFournisseur = 14,
        BonAvoirFournisseur = 15,
        FactureFournisseur = 16,
        FactureComptaFournisseur = 17,
        AchatArchive = 18
    }
}
