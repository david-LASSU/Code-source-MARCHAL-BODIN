using GalaSoft.MvvmLight;
using Divers.Model;
using MBCore.Model;
using System;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Linq;
using System.Diagnostics;
using GalaSoft.MvvmLight.Messaging;

namespace Divers.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase, IDataErrorInfo
    {
#region Init vars
        private readonly IDataService _dataService;

        private string _title;
        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }

        private string _doPiece;
        public string DoPiece
        {
            get { return _doPiece; }
            set { Set(ref _doPiece, value); }
        }

        private bool _CanSaveDoc;
        public bool CanSaveDoc
        {
            get { return _CanSaveDoc; }
            set { Set(ref _CanSaveDoc, value); }
        }

        private List<Unite> _unites;
        public List<Unite> Unites
        {
            get { return _unites; }
            set { Set(ref _unites, value); }
        }

        private IEnumerable<Fournisseur> _fournisseurs;
        public IEnumerable<Fournisseur> Fournisseurs
        {
            get { return _fournisseurs; }
            set { Set(ref _fournisseurs, value); }
        }

        private Document _document;
        public Document Document
        {
            get { return _document; }
            set { Set(ref _document, value); }
        }

        private Ligne _selectedLigne;
        public Ligne SelectedLigne
        {
            get { return _selectedLigne; }
            set { Set(ref _selectedLigne, value); }
        }

        public RelayCommand WindowLoadedCommand
        {
            get; private set;
        }

        public string Error => string.Empty;

        public string this[string columnName] => string.Empty;
        public RelayCommand SaveCommand { get; private set; }

        public RelayCommand AddCommand { get; private set; }
        public RelayCommand<Ligne> RemoveCommand { get; private set; }
        #endregion Init vars

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            if (IsInDesignMode)
            {
                DoPiece = "VDE06191";
                BaseCialAbstract.setDefaultParams("\\\\SRVAD01\\GESTION\\_SOCIETES\\DEV\\SMODEV.gcm");
                WindowLoaded();
            }
            else
            {
                string[] args = Environment.GetCommandLineArgs();
                if (args.Length < 3)
                {
                    throw new Exception("Nombre d'argument insuffisant");
                }
                BaseCialAbstract.setDefaultParams(args[1], args[2]);
                DoPiece = args[3];
            }
            Title = $"Articles DIVERS pour le document {DoPiece}";
            WindowLoadedCommand = new RelayCommand(WindowLoaded);
            SaveCommand = new RelayCommand(Save);
            AddCommand = new RelayCommand(Add);
            RemoveCommand = new RelayCommand<Ligne>(Remove);
        }

        private void WindowLoaded()
        {
            try
            {
                _dataService.GetUnites(
                (unites, error) =>
                {
                    if (error != null)
                    {
                        throw new Exception(error.Message);
                    }
                    Unites = unites.ToList<Unite>();
                });
                _dataService.GetFournisseurs(
                (fourns, error) =>
                {
                    if (error != null)
                    {
                        throw new Exception(error.Message);
                    }
                    Fournisseurs = fourns;
                });
                _dataService.GetDocument(DoPiece, Fournisseurs, Unites,
                (doc, error) =>
                {
                    if (error != null)
                    {
                        throw new Exception(error.Message);
                    }
                    Document = doc;
                    //Lignes = new ObservableCollection<Ligne>(doc);
                    //if (Lignes.Count == 0)
                    //{
                    //    throw new Exception("Aucun article DIVERS n'est présent dans le document ou le document n'existe pas.");
                    //}
                    CanSaveDoc = new int[] { 0, 1, 11, 12 }.Contains(doc.Type) && !doc.IsRelicat;
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Application.Current.Shutdown();
            }
        }

        private void Add()
        {
            _dataService.Add(Document, Unites, (result, error) => {
                if (error != null)
                {
                    MessageBox.Show(error.Message);
                    return;
                }
                SelectedLigne = Document.Lignes.Last();
                Messenger.Default.Send("ScrollBottom", "DoFocus");
            });
        }

        private void Remove(Ligne ligne)
        {
            if (ligne.IsNew && MessageBox.Show("Etes-vous sûr?", "Supprimer ligne", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Document.Lignes.Remove(ligne);
            }
        }

        private void Save()
        {
            try
            {
                CheckDocumentsClosed();
                _dataService.SaveAll(Document, (result, error) => {
                    MessageBox.Show(error == null ? "Enregistrement terminé" : error.Message);
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void CheckDocumentsClosed()
        {
            try
            {
                _dataService.CheckDocumentsClosed(Document);
            }
            catch (Exception e)
            {
                if (MessageBox.Show(
                e.Message,
                "Enregistrement",
                MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    CheckDocumentsClosed();
                }
                else
                {
                    throw new Exception("Action annulée par l'utilisateur.");
                }
            }
        }

        public override void Cleanup()
        {
            _dataService.OpenDocument(DoPiece);
            base.Cleanup();
        }
    }
}