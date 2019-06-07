using GalaSoft.MvvmLight;
using PrepaCommande.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrepaCommande.Model
{
    public class LigneCommande : ObservableObject
    {
        public string Ref => (AeRef ?? ArRef);
        public string ArRef { get; set; }
        public string AeRef { get; set; }
        public string Gamme1 { get; set; }
        public string Gamme2 { get; set; }
        public int? AGNo1 { get; set; }
        public int? AGNo2 { get; set; }
        public double ArPrixAch { get; set; }
        public double Total => ArPrixAch * Qte??0;
        public bool Sommeil { get; set; }
        public bool Supprime { get; set; }
        public bool SupprimeUsine { get; set; }
        public string RefFourn { get; set; }
        public string CodeBarre { get; set; }
        public string Designation { get; set; }
        public string UniteAchat { get; set; }
        public string UniteVente { get; set; }
        public double Colisage { get; set; }
        public double Qec { get; set; }
        public string Emplacement { get; set; }
        private double _stock;
        public double Stock
        {
            get { return _stock; }
            set
            {
                Set(ref _stock, value);
                RaisePropertyChanged("StockAterme");
            }
        }
        private double _qteCom;
        public double QteCom
        {
            get { return _qteCom; }
            set
            {
                Set(ref _qteCom, value);
                RaisePropertyChanged("StockAterme");
            }
        }
        private double _qteRes;
        public double QteRes
        {
            get { return _qteRes; }
            set
            {
                Set(ref _qteRes, value);
                RaisePropertyChanged("StockAterme");
            }
        }
        public double StockAterme => Stock + QteCom - QteRes;
        public double StockMin { get; set; }
        public double StockMax { get; set; }
        public double pxTTC { get; set; }
        private double? _qte;
        public double? Qte {
            get {
                return _qte;
            }
            set
            {
                Set(ref _qte, value);
                RaisePropertyChanged("RowState");
                RaisePropertyChanged("Total");
            }
        }
        /// <summary>
        /// Sert à forcer le changement de couleur de la ligne
        /// </summary>
        public int RowState => (StockMin == 0 || Disabled == true) ? 0 : (Qte == null ? 1 : 2);
        private bool _rowhidden;
        public bool RowHidden
        {
            get { return _rowhidden; }
            set { Set(ref _rowhidden, value); }
        }
        public bool Disabled => Sommeil == true || Supprime == true || SupprimeUsine == true;

        public double Stat { get; internal set; }
    }
}
