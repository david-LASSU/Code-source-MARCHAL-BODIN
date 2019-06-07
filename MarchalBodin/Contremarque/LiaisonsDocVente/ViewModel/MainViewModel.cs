using System;
using System.ComponentModel;
using LiaisonsDocVente.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MBCore.Model;
using System.Collections.Generic;
using System.Collections;
using Objets100cLib;
using MBCore.ViewModel;

namespace LiaisonsDocVente.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Init vars
        private ContremarqueRepository cmRepos;

        private ObservableCollection<Contremarque> cmLignes;
        public ObservableCollection<Contremarque> CmLignes
        {
            get { return cmLignes; }
            set
            {
                cmLignes = value;
                OnPropertyChanged("CmLignes");
            }
        }

        private string logText;
        public string LogText
        {
            get { return logText; }
            set
            {
                logText = value;
                OnPropertyChanged("LogText");
            }
        }

        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged("Title");
            }
        }

        private bool _mainGridEnabled;

        public bool MainGridEnabled
        {
            get { return _mainGridEnabled; }
            set
            {
                _mainGridEnabled = value;
                OnPropertyChanged("MainGridEnabled");
            }
        }

        private bool _isValid;

        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                _isValid = value;
                OnPropertyChanged("IsValid");
            }
        }

        // N° de VBC sur lequel s'effectue la contremarque
        private readonly string _doPiece = string.Empty;

        private bool _disableIntermag;

        public bool DisableIntermag
        {
            get { return _disableIntermag; }
            set
            {
                _disableIntermag = value;
                OnPropertyChanged("DisableIntermag");
            }
        }

        private bool _disableIntermagEnabled = true;

        public bool DisableIntermagEnabled
        {
            get { return _disableIntermagEnabled; }
            set
            {
                _disableIntermagEnabled = value;
                OnPropertyChanged("DisableIntermagEnabled");
            }
        }

        private string _message;

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged("Message");
            }
        }

        private List<string> _fournList;
        public List<string> FournList
        {
            get
            {
                return _fournList;
            }
            set
            {
                _fournList = value;
                OnPropertyChanged("FournList");
            }
        }

        private string _selectedFourn;
        public string SelectedFourn
        {
            get
            {
                return _selectedFourn;
            }
            set
            {
                _selectedFourn = value;
                OnPropertyChanged("SelectedFourn");
            }
        }
        public ICommand CommandValider { get; set; }
        public ICommand ToggleCheckAll { get; set; }
        #endregion
        public MainViewModel()
        {
            try
            {
                string[] args = Environment.GetCommandLineArgs();
                if (args.Length < 3)
                {
                    throw new Exception("Nombre d'argument insuffisant");
                }

                BaseCialAbstract.setDefaultParams(args[1], args[2]);
                _doPiece = args[3];

                CommandValider = new Command(ActionValider);
                MainGridEnabled = true;
                ToggleCheckAll = new Command(ActionToggleCheckAll);
                Title = $"Contremarque {_doPiece}";
            }
            catch (Exception e)
            {
                //EventLog.WriteEntry("Application", e.ToString(), EventLogEntryType.Error);
                Debug.Print(e.ToString());
                MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// Charge les données on load pour éviter
        /// l'appel du repos dans visual studio
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                cmRepos.openBaseCial();
                cmRepos.Log += Log;
                FournList = cmRepos.FournList;

                string message = cmRepos.DocumentIsValid(_doPiece);
                if (!string.Equals(message, string.Empty))
                {
                    Log($"ERREUR::{message}");
                    Log("Vous pouvez fermer cette fenêtre");
                    MainGridEnabled = false;
                    return;
                }

                CmLignes = new ObservableCollection<Contremarque>(cmRepos.getAll(_doPiece));

                if (CmLignes.Any(c=>c.Type == 0) == true)
                {
                    DisableIntermagEnabled = false;
                    DisableIntermag = true;
                    Message = "Attention! Vous êtes sur un devis, cela va donc générer un APC de demande de prix";
                }

                foreach (Contremarque contremarque in CmLignes)
                {
                    contremarque.PropertyChanged += Contremarque_PropertyChanged;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Contremarque_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            bool hasRowChecked = false;

            foreach (Contremarque contremarque in CmLignes)
            {
                if (!contremarque.RowChecked) continue;
                hasRowChecked = true;

                if (contremarque.IsValid) continue;
                IsValid = false;
                return;
            }

            IsValid = hasRowChecked;
        }

        private void ActionToggleCheckAll(object param)
        {
            bool isChecked = (bool)param;
            foreach (Contremarque contremarque in CmLignes)
            {
                contremarque.RowChecked = isChecked;
            }
        }

        private void ActionValider(object param)
        {
            try
            {
                CheckDocumentIsClosed();
            }
            catch (Exception e)
            {
                Log(e.Message);
                return;
            }

            MainGridEnabled = false;
            try
            {
                cmRepos.saveAll(CmLignes, _doPiece, DisableIntermag);
            }
            catch (Exception e)
            {
                Log($"ERREUR::{e.ToString()}");
            }
            finally
            {
                cmRepos.closeBaseCial();
            }

            MainGridEnabled = false;
            Log("Vous pouvez fermer cette fenêtre");
        }

        private void CheckDocumentIsClosed()
        {
            try
            {
                cmRepos.DocumentIsClosed(cmLignes, _doPiece);
            }
            catch (Exception e)
            {
                if (MessageBox.Show(
                e.Message,
                "Envoi du document",
                MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    CheckDocumentIsClosed();
                }
                else
                {
                    throw new Exception("Action annulée par l'utilisateur.");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void Log(string message)
        {
            Debug.Print(message);
            Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Background, new Action(delegate
                {
                    LogText += message + Environment.NewLine;
                })
            );
        }

        internal void OnWindowClosing(object sender, CancelEventArgs e)
        {
            cmRepos.OpenDocument(_doPiece);
        }

        public void ApplyFournList(IList items)
        {
            items.Cast<Contremarque>().Where(c => c.IsFournListEditable == true).Select(
                c =>
                {
                    c.SelectedFourn = SelectedFourn;
                    return c;
                }).ToList();
        }
    }
}
