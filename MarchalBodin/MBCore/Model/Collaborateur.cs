using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBCore.Model
{
    public class Collaborateur
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }

        public string NomPrenom
        {
            get { return $"{Nom} {Prenom}"; }
        }

        public string PrenomNom
        {
            get { return $"{Prenom} {Nom}"; }
        }

        public override string ToString()
        {
            return NomPrenom;
        }
    }
}
