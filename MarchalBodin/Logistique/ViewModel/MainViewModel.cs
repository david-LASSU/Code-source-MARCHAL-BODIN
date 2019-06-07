using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Logistique.Model;
using MBCore.Model;
using Objets100cLib;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Logistique.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Init vars
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

        private string _gencodEmplStr;
        public string GencodEmplStr
        {
            get { return _gencodEmplStr; }
            set { Set(ref _gencodEmplStr, value); }
        }

        private string _gencodArtStr;
        public string GencodArtStr
        {
            get { return _gencodArtStr; }
            set { Set(ref _gencodArtStr, value); }
        }

        private bool _gencodArtEnabled = false;
        public bool GencodArtEnabled
        {
            get { return _gencodArtEnabled; }
            set { Set(ref _gencodArtEnabled, value); }
        }

        private IBODepotEmplacement _emplacement;
        public IBODepotEmplacement Emplacement
        {
            get { return _emplacement; }
            set { Set(ref _emplacement, value); }
        }

        private Article _selectedArticle;
        public Article SelectedArticle
        {
            get { return _selectedArticle; }
            set { Set(ref _selectedArticle, value); }
        }

        private ObservableCollection<Article> _articles;
        public ObservableCollection<Article> Articles
        {
            get { return _articles; }
            set { Set(ref _articles, value); }
        }

        public RelayCommand WindowLoadedCommand { get; private set; }
        public RelayCommand GencodEmplKeyDownCommand { get; private set; }
        public RelayCommand ClearGencodEmplCommand { get; private set; }
        public RelayCommand GencodArtKeyDownCommand { get; private set; }
        public RelayCommand<DataGridCellEditEndingEventArgs> CellEditEndingCommand { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length < 2)
            {
                throw new Exception("Nombre d'argument insuffisant");
            }

            BaseCialAbstract.setDefaultParams(args[1], args[2]);

            WindowLoadedCommand = new RelayCommand(WindowLoaded);
            GencodEmplKeyDownCommand = new RelayCommand(GencodEmplKeyDown);
            ClearGencodEmplCommand = new RelayCommand(ClearGencodEmpl);
            GencodArtKeyDownCommand = new RelayCommand(GencodArtKeyDown);
            CellEditEndingCommand = new RelayCommand<DataGridCellEditEndingEventArgs>(CellEditEnding);
        }

        private void CellEditEnding(DataGridCellEditEndingEventArgs obj)
        {
            if (obj.Column.Header.ToString() == "Stock Réel")
            {
                try
                {
                    // Declarer le nouveau stock
                    TextBox el = (TextBox) obj.EditingElement;
                    double noteStock = double.Parse(el.Text);
                    Article article = (Article)obj.Row.Item;
                    Debug.Print($"Change stock pour article: {article.Ref}, de {article.Stock} à {noteStock}");

                    _dataService.DeclareStock(noteStock, article, (error) => {
                        if (error != null)
                        {
                            MessageBox.Show(error.Message);
                        }
                        else
                        {
                            Messenger.Default.Send("GencodArt", "DoFocus");
                        }
                    });

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            
        }

        private void WindowLoaded()
        {
            Messenger.Default.Send("GencodEmpl", "DoFocus");
        }

        private void GencodEmplKeyDown()
        {
            if (GencodEmplStr == null)
            {
                ClearGencodEmpl();
            }

            _dataService.GetEmplacement(GencodEmplStr,
                (empl, error) => {
                    if (error != null)
                    {
                        MessageBox.Show(error.Message);
                        ClearGencodEmpl();
                        return;
                    }

                    Emplacement = empl;

                    _dataService.GetArticles(empl, 
                        (articles, error2) => {
                            if (error2 != null)
                            {
                                MessageBox.Show($"Impossible de recharger la liste: {error2.ToString()}");
                            }
                            Articles = new ObservableCollection<Article>(articles);
                            GencodArtEnabled = true;
                            Messenger.Default.Send("GencodArt", "DoFocus");
                    });
                });
        }

        private void ClearGencodEmpl()
        {
            GencodEmplStr = null;
            Emplacement = null;
            Articles = null;
            GencodArtEnabled = false;
            Messenger.Default.Send("GencodEmpl", "DoFocus");
        }

        private void GencodArtKeyDown()
        {
            Debug.Print(GencodArtStr);
            _dataService.SetDefaultEmpl(
                GencodArtStr,
                Emplacement,
                (article, error) => {
                    if (error != null)
                    {
                        MessageBox.Show(error.Message);
                        return;
                    }

                    // Todo Raffraichir avec les nouveaux articles plutôt que la totalité
                    _dataService.GetArticles(
                        Emplacement,
                        (articles, error2) => {
                            if (error2 != null)
                            {
                                MessageBox.Show($"Impossible de recharger la liste: {error2.ToString()}");
                            }

                            Articles = new ObservableCollection<Article>(articles);
                            SelectedArticle = Articles.Where(a => a.ArRef == article.AR_Ref).First();
                            Messenger.Default.Send("ScrollToSelectedItem", "DoFocus");
                        });
                });

            GencodArtStr = null;
            Messenger.Default.Send("GencodArt", "DoFocus");
        }

        //public override void Cleanup()
        //{
        //    Clean up if needed

        //    base.Cleanup();
        //}
    }
}