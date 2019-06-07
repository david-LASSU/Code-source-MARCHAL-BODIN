using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LiaisonsDocVente.Model
{
    public class LiaisonCde : INotifyPropertyChanged
    {
        public Contremarque Cm { get; set; }
        public string NumPiece { get; set; }
        public string Fournisseur { get; set; }
        public double DlQteBL { get; set; }
        public double CMQteTotal { get; set; }
        public double QteDispo => DlQteBL - CMQteTotal;
        private double _qte;
        public double Qte {
            get { return _qte; }
            set
            {
                if (_qte == value) { return; }
                // Bug IsEnabled editable after entered qt in line above and press enter
                // IsEnabled is false but cell is in editing mode so prevent to set a value on this disabled cell
                if (Cm.Liaisons.Any(l=>l.Qte > 0 && l.DLNoIn != DLNoIn)) { return; }
                _qte = value;

                if (_qte > QteDispo || _qte < 0)
                {
                    _qte = QteDispo;
                }

                OnPropertyChanged("Qte");
                Cm.LiaisonUpdate();
            }
        }
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }
        public int DLNoIn { get; set; }
        public int DlNoOut { get; set; }
        public int Statut { get; internal set; }
        public string Unite { get; internal set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
