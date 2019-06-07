using GalaSoft.MvvmLight;
using MBCore.Model;
using ReceptionCommande.Model;
using System;
using Objets100cLib;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Data;
using System.Linq;

namespace ReceptionCommande.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
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

        private string _codeBarre = string.Empty;

        public string CodeBarre
        {
            get { return _codeBarre; }
            set
            {
                Set(ref _codeBarre, value);
            }
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

        private Document _document;

        public Document Document
        {
            get
            {
                return _document;
            }
            set
            {
                Set(ref _document, value);
            }
        }

        public RelayCommand<object> DatagridKeyDownCommand
        {
            get; private set;
        }

        private DataTable _foundItems;
        public DataTable FoundItems
        {
            get { return _foundItems; }
            set { Set(ref _foundItems, value); }
        }

        public RelayCommand SaveAndCloseCommand
        {
            get; private set;
        }

        public RelayCommand WindowLoadedCommand
        {
            get; private set;
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length < 3)
            {
                throw new Exception("Nombre d'argument insuffisant");
            }

            BaseCialAbstract.setDefaultParams(args[1], args[2]);
            DoPiece = args[3];

            WelcomeTitle = $"Réception du Document {DoPiece}";
            CodeBarre = string.Empty;

            _dataService = dataService;

            DatagridKeyDownCommand = new RelayCommand<object>(datagridKeyDown);
            SaveAndCloseCommand = new RelayCommand(saveAndClose);
            WindowLoadedCommand = new RelayCommand(WindowLoaded);
        }

        private void WindowLoaded()
        {
            CheckDocumentIsClosed();
            _dataService.GetAll(DoPiece,
                (document, error) => {
                    if (error != null)
                    {
                        // TODO 
                        return;
                    }
                    Document = document;
                });
        }

        List<Key> keyList = new List<Key>() { Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9 };

        private void datagridKeyDown(object parameter)
        {
            KeyEventArgs e = (KeyEventArgs)parameter;
            DataGrid dg = (DataGrid)e.Source;

            if (e.Key == Key.S)
            {
                CodeBarre = string.Empty;
                return;
            }

            if (keyList.Contains(e.Key))
            {
                CodeBarre += e.Key.ToString().TrimStart('D');
            }

            if (e.Key == Key.Return || e.Key == Key.Tab)
            {
                if (CodeBarre == string.Empty)
                {
                    return;
                }

                var found = false;
                Document.Lignes.ToList().ForEach((ligne) => {
                    var il = ligne.IBOLigne;

                    if ((il.ArticleGammeEnum2 != null && il.ArticleGammeEnum2.FactoryArticleGammeEnumRef.List.Cast<IBOArticleGammeEnumRef3>().Any(ae => ae.AE_CodeBarre == CodeBarre))
                    || (il.ArticleGammeEnum1 != null && il.ArticleGammeEnum1.FactoryArticleGammeEnum.List.Cast<IBOArticleGammeEnumRef3>().Any(ae => ae.AE_CodeBarre == CodeBarre))
                    || (il.Article.Conditionnement != null && il.Article.FactoryArticleCond.List.Cast<IBOArticleCond3>().Any(c => c.CO_CodeBarre == CodeBarre))
                    || (il.Article.FactoryArticleTarifFournisseur.List.Cast<IBOArticleTarifFournisseur3>().Any(t => t.AF_CodeBarre == CodeBarre))
                    || (il.Article.AR_CodeBarre == CodeBarre))
                    {
                        dg.SelectedItem = ligne;
                        foreach (var c in dg.SelectedCells)
                        {
                            if (c.Column.Header.ToString() == "Quantité Livrée")
                            {
                                var cellContent = c.Column.GetCellContent(c.Item);
                                if (cellContent != null)
                                {
                                    var dc = (DataGridCell)cellContent.Parent;
                                    dc.Focus();
                                    dc.IsEditing = true;
                                }
                            }
                        }
                        found = true;
                        return;
                    }
                });
                // Not found
                if(!found)
                {
                    MessageBox.Show("Article non trouvé.");
                }
                CodeBarre = string.Empty;
            }
        }

        private bool canExecuteDatagridKeyDown(object parameter)
        {
            return true;
        }

        private void saveAndClose()
        {
            CheckDocumentIsClosed();

            _dataService.SaveAll(Document,
                (result, error) => {
                    if (error != null)
                    {
                        Debug.Print(error.ToString());
                        MessageBox.Show("Erreur pendant l'enregistrement en base");
                        return;
                    }

                    if (result == true)
                    {
                        //_dataService.ReopenDoc(DoPiece);
                        Application.Current.Shutdown();
                    }
                });
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
                "Enregistrement du document",
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
            _dataService.ReopenDoc(DoPiece);
            // Clean up if needed
            base.Cleanup();
        }
    }
}