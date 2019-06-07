using LiaisonsDocVente.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace LiaisonsDocVente.ViewModel
{
    class RelierViewModel : INotifyPropertyChanged
    {
        private ContremarqueRepository cmRepos;

        private Contremarque _contremarque;

        public Contremarque Contremarque
        {
            get { return _contremarque; }
            set
            {
                _contremarque = value;
                OnPropertyChanged("Contremarque");
            }
        }

        private ObservableCollection<LiaisonCde> _liaisons;
        public ObservableCollection<LiaisonCde> Liaisons
        {
            get { return _liaisons; }
            set
            {
                _liaisons = value;
                OnPropertyChanged("Liaisons");
            }
        }

        internal void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                {
                    // Design time logic
                    return;
                }

                cmRepos = new ContremarqueRepository();
                if (Contremarque.Liaisons.Count == 0)
                {
                    Contremarque.Liaisons = cmRepos.getLiaisonsCde(Contremarque);
                    Liaisons = new ObservableCollection<LiaisonCde>(Contremarque.Liaisons);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
