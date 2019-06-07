using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrepaCommande.Model
{
    public class Commande : ObservableObject
    {
        public Commande()
        {
            Lignes = new ObservableCollection<LigneCommande>();
        }

        public string Piece { get; set; }
        public string Fournisseur { get; set; }
        public string CtNum { get; set; }
        public string Franco { get; set; }
        public ObservableCollection<LigneCommande> Lignes { get; }
        public double Total => Lignes.Sum(l => l.Total);
        
        //TODO mettre sous forme de tableau ?
        public int NbRowState0 => Lignes.Count(l => l.RowState == 0);
        public int NbRowState1 => Lignes.Count(l => l.RowState == 1);
        public int NbRowState2 => Lignes.Count(l => l.RowState == 2);

        public void Update()
        {
            RaisePropertyChanged("Total");
        }
    }
}
