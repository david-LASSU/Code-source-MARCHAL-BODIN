using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Objets100cLib;

namespace ReceptionCommande.Model
{
    public class Document
    {
        public IBODocument3 IBODoc { get; set; }
        public ObservableCollection<DocumentLigne> Lignes { get; set; }
    }
}
