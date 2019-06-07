using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using MBCore.Model;
using PrepaCommande.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PrepaCommande.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Initialize properties
        private readonly IDataService _dataService;

        /// <summary>
        /// The <see cref="WelcomeTitle" /> property's name.
        /// </summary>
        public const string WelcomeTitlePropertyName = "WelcomeTitle";

        private string _welcomeTitle = string.Empty;

        /// <summary>
        /// Gets the WelcomeTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WelcomeTitle
        {
            get
            {
                return _welcomeTitle;
            }
            set
            {
                Set(ref _welcomeTitle, value);
            }
        }

        private bool _isImport;

        private bool _isWaiting = false;

        public bool IsWaiting
        {
            get { return _isWaiting; }
            set { Set(ref _isWaiting, value); }
        }

        private string _doPiece;
        /// <summary>
        /// Numéro de pièce
        /// </summary>
        public string DoPiece
        {
            get
            {
                return _doPiece;
            }
            set
            {
                Set(ref _doPiece, value);
            }
        }

        private double _globQt;

        /// <summary>
        /// Champ texte quantité
        /// </summary>
        public double GlobQt
        {
            get { return _globQt; }
            set { Set(ref _globQt, value); }
        }

        private Commande _commande;

        public Commande Commande
        {
            get
            {
                return _commande;
            }
            set
            {
                Set(ref _commande, value);
            }
        }

        private LigneCommande _selectedItem;

        public LigneCommande SelectedItem
        {
            get { return _selectedItem; }
            set { Set(ref _selectedItem, value); }
        }

        private IList _selectedItems;

        public IList SelectedItems
        {
            get { return _selectedItems; }
            set { Set(ref _selectedItems, value); }
        }

        private string _codeBarre;

        public string CodeBarre
        {
            get { return _codeBarre; }
            set
            {
                Debug.Print($"Set: {value}");
                var val = value;
                if (val != null)
                {
                    val = val.ToUpper();
                }
                Set(ref _codeBarre, val);
            }
        }

        private int _nbRowState0;
        public int NbRowState0
        {
            get { return _nbRowState0; }
            set { Set(ref _nbRowState0, value); }
        }

        private int _nbRowState1;
        public int NbRowState1
        {
            get { return _nbRowState1; }
            set { Set(ref _nbRowState1, value); }
        }

        private int _nbRowState2;
        public int NbRowState2
        {
            get { return _nbRowState2; }
            set { Set(ref _nbRowState2, value); }
        }

        private bool _hideRowState0 = false;
        public bool HideRowState0
        {
            get { return _hideRowState0; }
            set { Set(ref _hideRowState0, value); }
        }

        private bool _hideRowState1 = false;
        public bool HideRowState1
        {
            get { return _hideRowState1; }
            set { Set(ref _hideRowState1, value); }
        }

        private bool _hideRowState2 = false;
        public bool HideRowState2
        {
            get { return _hideRowState2; }
            set { Set(ref _hideRowState2, value); }
        }

        public RelayCommand WindowLoadedCommand { get; private set; }
        public RelayCommand<DataGrid> CodeBarreKeyDownCommand { get; private set; }
        public RelayCommand<int> SelectionCommandClick { get; private set; }
        public RelayCommand<IList> SelectionChangedCommand { get; private set; }
        public RelayCommand UpdateTotalsCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand CloseCommand { get; private set; }
        public RelayCommand<object> DoubleClickCommand { get; private set; }
        public RelayCommand<object> CellEditEndingCommand { get; private set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            try
            {
                _dataService = dataService;

                if (IsInDesignMode)
                {
                    //DoPiece = "APC04560";
                    //BaseCialAbstract.setDefaultParams("\\\\SRVSQL04\\SI\\Sage\\DEV\\BODINDEV.gcm");
                    //WindowLoaded();
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
                    _isImport = args.ElementAtOrDefault(5) == "import";
                }

                WindowLoadedCommand = new RelayCommand(WindowLoaded);
                CodeBarreKeyDownCommand = new RelayCommand<DataGrid>(CodeBarreKeyDown);
                SelectionCommandClick = new RelayCommand<int>((s) => SelectionCommand(s));
                SelectionChangedCommand = new RelayCommand<IList>((s) => SelectionChanged(s));
                UpdateTotalsCommand = new RelayCommand(UpdateTotals);
                SaveCommand = new RelayCommand(Save);
                CloseCommand = new RelayCommand(Close);
                DoubleClickCommand = new RelayCommand<object>(DoubleCLick);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Ouvre la fiche article sur l'interrogation de stock
        /// </summary>
        /// <param name="obj"></param>
        private void DoubleCLick(object param)
        {
            var cellInfo = (DataGridCellInfo)param;
            if (cellInfo.Column == null)
            {
                return;
            }

            switch (cellInfo.Column.Header.ToString())
            {
                case "Quantité":
                case "Stock":
                    return;
                default:
                    _dataService.OpenArticle((cellInfo.Item as LigneCommande).ArRef);
                    break;
            }
        }

        private void UpdateTotals()
        {
            // TODO bind Commande prop directly ?
            NbRowState0 = Commande.NbRowState0;
            NbRowState1 = Commande.NbRowState1;
            NbRowState2 = Commande.NbRowState2;
        }

        /// <summary>
        /// Cherche la ligne en fonction de la référence article ou du code barre
        /// </summary>
        /// <param name="dg"></param>
        private void CodeBarreKeyDown(DataGrid dg)
        {
            Debug.Print($"Code barre: {CodeBarre}");

            if (CodeBarre == null)
            {
                return;
            }

            IEnumerable<LigneCommande> r;

            // Ref Gamme ?
            r = Commande.Lignes.Where(s => s.AeRef == CodeBarre.ToUpper());

            // Ref Mag ?
            if (!r.Any()) r = Commande.Lignes.Where(s => s.ArRef == CodeBarre.ToUpper());

            // Ref Fourn ?
            if (!r.Any()) r = Commande.Lignes.Where(s => s.RefFourn == CodeBarre.ToUpper());

            // EAN?
            if (!r.Any())
            {
                _dataService.GetRefByGencod(CodeBarre,
                    (dt, error) => {
                        if (error != null)
                        {
                            // TODO
                            return;
                        }
                        if (dt.Rows.Count == 1)
                        {
                            r = Commande.Lignes.Where(s => s.Ref == dt.Rows[0]["Ref"].ToString());
                        }
                    });
            }

            if (r.Any())
            {
                LigneCommande ligne = r.First();
                ligne.RowHidden = false;
                dg.Focus();
                dg.SelectedItem = ligne;
                dg.UpdateLayout();
                dg.ScrollIntoView(dg.SelectedItem);
                dg.CurrentCell = new DataGridCellInfo(dg.SelectedItem, dg.Columns.Single(c => c.Header.ToString() == "Quantité"));
                dg.BeginEdit();
                CodeBarre = string.Empty;
                return;
            }

            CodeBarre = string.Empty;
            //MessageBox.Show("Article non trouvé.");
        }

        private void SelectionChanged(IList s)
        {
            SelectedItems = s;
            UpdateTotals();
        }

        private void WindowLoaded()
        {
            try
            {
                _dataService.GetCommande(DoPiece,
                    (cmd, error) => {
                        if (error != null)
                        {
                            throw new Exception(error.Message);
                        }
                        CheckDocumentIsClosed();
                        Commande = cmd;
                    });

                WelcomeTitle = $"Préparation de la commande {DoPiece} - Fournisseur {Commande.Fournisseur} - {Commande.CtNum}";
                UpdateTotals();
                if (_isImport)
                {
                    Task.Factory.StartNew(() => {
                        IsWaiting = true;
                        _dataService.Import(Commande,
                            (result, error) =>
                            {
                                if (result)
                                {
                                    MessageBox.Show("Import terminé");
                                }
                                else
                                {
                                    MessageBox.Show(error.ToString());
                                }
                                IsWaiting = false;
                            });
                    });
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Vide le champs code barre ou
        /// Met à jour la quantité de manière automatique en fonction du bouton cliqué
        /// Filtre les lignes selon son état
        /// </summary>
        /// <param name="s"></param>
        private void SelectionCommand(int s)
        {
            if (s == 0)
            {
                CodeBarre = string.Empty;
                return;
            }

            // Filtre les lignes selon leur etat
            switch (s)
            {
                case 7:
                    HideRowState0 = !HideRowState0;
                    Commande.Lignes.Where(l => l.RowState == 0 && l.Qte == null).ToList().ForEach((l) => l.RowHidden = HideRowState0);
                    break;
                case 8:
                    HideRowState1 = !HideRowState1;
                    Commande.Lignes.Where(l => l.RowState == 1 && l.Qte == null).ToList().ForEach((l) => l.RowHidden = HideRowState1);
                    break;
                case 9:
                    HideRowState2 = !HideRowState2;
                    Commande.Lignes.Where(l => l.RowState == 2 && l.Qte == null).ToList().ForEach((l) => l.RowHidden = HideRowState2);
                    break;
                default:
                    break;
            }

            if (SelectedItems == null || SelectedItems.Count == 0)
            {
                return;
            }

            foreach (LigneCommande ligne in SelectedItems)
            {
                switch (s)
                {
                    case 1:
                        ligne.Qte = ligne.Colisage;
                        break;
                    case 2:
                        ligne.Qte = ligne.Qec;
                        break;
                    case 3:
                        ligne.Qte = ligne.StockMin > ligne.StockAterme ? ligne.StockMin - ligne.StockAterme : 0;
                        break;
                    case 4:
                        ligne.Qte = ligne.StockMax > ligne.StockAterme ? ligne.StockMax - ligne.StockAterme : 0;
                        break;
                    case 5:
                        ligne.Qte = 0;
                        break;
                    case 6:
                        ligne.Qte = GlobQt;
                        break;
                    default:
                        return;
                }
            }

            UpdateTotals();
        }

        private void Save()
        {
            CheckDocumentIsClosed();

            _dataService.SaveAll(Commande,
                (result, error) => {
                    if (error != null)
                    {
                        Debug.Print(error.ToString());
                        MessageBox.Show("Erreur pendant l'enregistrement en base");
                        return;
                    }
                    MessageBox.Show("Enregistrement terminé");
                });
        }

        private void Close()
        {
            Application.Current.Shutdown();
        }

        private void CheckDocumentIsClosed()
        {
            try
            {
                _dataService.DocumentIsClosed(DoPiece);
            }
            catch (Exception e)
            {
                if (MessageBox.Show(
                e.Message,
                "Document",
                MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    CheckDocumentIsClosed();
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }
        }

        public override void Cleanup()
        {
            // Clean up if needed
            _dataService.CloseDocument();
            _dataService.ReopenDoc(DoPiece);
            base.Cleanup();
        }
    }
}