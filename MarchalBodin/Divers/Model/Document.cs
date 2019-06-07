using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MBCore.Model;

namespace Divers.Model
{
    public class Document
    {
        public string DoPiece { get; internal set; }
        public int Domaine { get; internal set; }
        public int Type { get; internal set; }
        public bool IsRelicat { get; internal set; }
        public ObservableCollection<Ligne> Lignes { get; internal set; }
        /// <summary>
        /// Uniquement si Doc Achat
        /// </summary>
        public Fournisseur Fournisseur { get; internal set; }
    }
}
