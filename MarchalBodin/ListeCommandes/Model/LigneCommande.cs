using System;

namespace ListeCommandes.Model
{
    public class LigneCommande
    {
        public string Acheteur { get; set; }

        public string Piece { get; set; }

        public string Client { get; set; }

        public string ArRef { get; set; }

        public string RefFourn { get; set; }

        public string Designation { get; set; }

        public double CmQte { get; set; }

        public double CliQte { get; set; }

        public string Vendeur { get; set; }
        public double TotalHT { get; set; }
        public DateTime? DateStatut { get; set; }
    }
}
