using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ListeCommandes.Model;
using MBCore.Model;
using System.Collections.ObjectModel;
using System.Timers;
using MBCore.ViewModel;

namespace ListeCommandes.ViewModel
{
    #region variables declaration
    public class MainViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Liste des commandes
        /// </summary>
        private ListCollectionView _commandes;

        public ListCollectionView Commandes
        {
            get { return _commandes; }
            set
            {
                _commandes = value;
                if (_commandes != null)
                {
                    TotalCommandes = _commandes.Cast<Commande>().Sum(c => c.TotalHT);
                    _commandes.GroupDescriptions?.Add(new PropertyGroupDescription("Fournisseur"));
                }
                
                OnPropertyChanged("Commandes");
            }
        }

        private double _totalCommandes;

        public double TotalCommandes
        {
            get { return _totalCommandes; }
            set
            {
                _totalCommandes = value;
                OnPropertyChanged("TotalCommandes");
            }
        }

        /// <summary>
        /// Liste des commandes avec contremarque
        /// </summary>
        private ListCollectionView _cmCommandes;

        public ListCollectionView CmCommandes
        {
            get { return _cmCommandes; }
            set
            {
                _cmCommandes = value;
                if (_cmCommandes != null)
                {
                    TotalCmCommandes = _cmCommandes.Cast<Commande>().Sum(c => c.TotalHT);
                    _cmCommandes.GroupDescriptions?.Add(new PropertyGroupDescription("Fournisseur"));
                }
                
                OnPropertyChanged("CmCommandes");
            }
        }

        private ListCollectionView _recepCommandes;
        public ListCollectionView RecepCommandes
        {
            get { return _recepCommandes; }
            set
            {
                _recepCommandes = value;
                OnPropertyChanged("RecepCommandes");
            }
        }

        private double _totalCmCommandes;
        public double TotalCmCommandes
        {
            get { return _totalCmCommandes; }
            set
            {
                _totalCmCommandes = value;
                OnPropertyChanged("TotalCmCommandes");
            }
        }

        private int _dateLivPeriode;

        public int DateLivPeriode
        {
            get { return _dateLivPeriode; }
            set
            {
                _dateLivPeriode = value;
                OnPropertyChanged("DateLivPeriode");
            }
        }

        private DateTime? _dateLivFrom;

        public DateTime? DateLivFrom
        {
            get { return _dateLivFrom; }
            set
            {
                _dateLivFrom = value;
                OnPropertyChanged("DateLivFrom");
            }
        }

        private DateTime? _dateLivTo;

        public DateTime? DateLivTo
        {
            get { return _dateLivTo; }
            set
            {
                _dateLivTo = value;
                OnPropertyChanged("DateLivTo");
            }
        }

        private DateTime? _dateDocFrom;

        public DateTime? DateDocFrom
        {
            get { return _dateDocFrom; }
            set
            {
                _dateDocFrom = value;
                OnPropertyChanged("DateDocFrom");
            }
        }

        private CommandeRepository cdeRepos;

        private Collection<Collaborateur> _collaborateurs;
        public Collection<Collaborateur> Collaborateurs
        {
            get { return _collaborateurs; }
            set
            {
                _collaborateurs = value;
                _collaborateurs.Insert(0, new Collaborateur() { Id = 0, Nom = "----" });
                
                OnPropertyChanged("Collaborateurs");
                Collabo = _collaborateurs.First();
            }
        }

        private Collaborateur _collaborateur;

        public Collaborateur Collabo
        {
            get { return _collaborateur; }
            set
            {
                _collaborateur = value;
                OnPropertyChanged("Collaborateur");
            }
        }

        public Dictionary<CommandeType, string> CommandeTypeList => Commande.TypesArrayLabels;

        private List<CommandeType> _selectedCommandeTypes;
        public List<CommandeType> SelectedCommandeTypes
        {
            get { return _selectedCommandeTypes; }
            set
            {
                _selectedCommandeTypes = value;
            }
        }

        public Dictionary<ApcType, string> ApcTypeList => Commande.ApcTypesArrayLabels;

        private List<ApcType> _selectedApcTypes = new List<ApcType>();
                
        public List<ApcType> SelectectedApcTypes
        {
            get { return _selectedApcTypes; }
            set
            {
                _selectedApcTypes = value;
            }
        }

        public Dictionary<CommandeStatut, string> CommandeStatutList => Commande.StatutsArrayLabels;

        private List<CommandeStatut> _selectedCommandeStatuts;
        public List<CommandeStatut> SelectedCommandeStatuts
        {
            get { return _selectedCommandeStatuts; }
            set
            {
                _selectedCommandeStatuts = value;
            }
        }

        private string _selectedTab;

        public string SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                if (_selectedTab == value) { return; }

                _selectedTab = value;
                LoadDatas();
            }
        }

        public string Title { get; set; }

        private bool _loading;

        public bool Loading
        {
            get { return _loading; }
            set
            {
                _loading = value;
                OnPropertyChanged("Loading");
            }
        }

#if DEBUG
        private Timer _reloadTimer = new Timer(5000);
#else
        private Timer _reloadTimer = new Timer(30000);
#endif
        private bool _autoReload;
        public bool AutoReload
        {
            get { return _autoReload; }
            set
            {
                _autoReload = value;
                _reloadTimer.Enabled = value;
                OnPropertyChanged("AutoReload");
            }
        }

        public ICommand LoadDatasClick { get; set; }
        public ICommand OpenVbcDoubleCLick { get; set; }
        public ICommand OpenAbcDoubleCLick { get; set; }
        public ICommand OpenFournRightClick { get; set; }

        #endregion

        public MainViewModel()
        {
            try
            {
                Debug.Print("Init");
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Charge le sample en mode design
                    BaseCialAbstract.setDefaultParams("G:\\_SOCIETES\\DEV\\SMODEV.gcm");
                    SelectedCommandeStatuts = new List<CommandeStatut>()
                    {
                        CommandeStatut.Saisi,
                        CommandeStatut.Confirme,
                        CommandeStatut.Accepte
                    };

                    SelectedCommandeTypes = new List<CommandeType>()
                    {
                        CommandeType.PreparationCommande
                    };
                }
                else
                {
                    string[] args = Environment.GetCommandLineArgs();
                    if (args.Length < 2)
                    {
                        throw new Exception("Nombre d'argument insuffisant");
                    }
                    BaseCialAbstract.setDefaultParams(args[1], args[2]);
                }

                LoadDatasClick = new Command(LoadDatasClickAction);
                OpenVbcDoubleCLick = new Command(OpenVbcAction);
                OpenAbcDoubleCLick = new Command(OpenAbcAction);
                OpenFournRightClick = new Command(OpenFournAction);
                Title = "Liste des commandes Fournisseurs";

                CollaborateurRepository CoRepos = new CollaborateurRepository();
                Collaborateurs = CoRepos.getAll();

                _reloadTimer.Elapsed += LoadDatasOnTick;

                Debug.Print("End init");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void LoadDatasOnTick(object sender, ElapsedEventArgs e)
        {
            LoadDatas();
        }

        public void LoadDatas()
        {
            if (Loading)
            {
                return;
            }
            Debug.Print($"Loading from {SelectedTab} ...");
            Loading = true;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += Fetch;
            worker.RunWorkerCompleted += (s, ev) => {
                Loading = false;
            };
            worker.RunWorkerAsync();
        }

        private void Fetch(object sender, DoWorkEventArgs e)
        {
            Debug.Print("Fetch");
            cdeRepos = new CommandeRepository();
            CommandeFiltre filtre = new CommandeFiltre()
            {
                CommandeTypes = SelectedCommandeTypes,
                CommandeStatuts = SelectedCommandeStatuts,
                DateLivPeriode = DateLivPeriode,
                DateLivFrom = DateLivFrom,
                DateLivTo = DateLivTo,
                DateDocFrom = DateDocFrom,
                Collabo = Collabo,
                ApcTypes = SelectectedApcTypes
            };

            switch (SelectedTab)
            {
                case "AllTab":
                    Commandes = null;
                    Commandes = new ListCollectionView(cdeRepos.GetAll(filtre));
                    break;
                case "CmTab":
                    CmCommandes = null;
                    CmCommandes = new ListCollectionView(cdeRepos.GetAllCm(filtre));
                    break;
                case "RecepTab":
                    RecepCommandes = null;
                    RecepCommandes = new ListCollectionView(cdeRepos.GetAllRecep());
                    break;
            }

            Debug.Print("End Fetch");
        }

        private void LoadDatasClickAction(object obj)
        {
            LoadDatas();
        }

        private void OpenFournAction(object parameters)
        {
            GroupItem groupItem = (GroupItem)parameters;
            CollectionViewGroup collectionViewGroup = (CollectionViewGroup)groupItem.Content;

            Commande commande = (Commande)collectionViewGroup.Items[0];
            Process.Start(cdeRepos.getSagePath(), $"{cdeRepos.fichierGescom} -u={cdeRepos.user} -cmd=\"Tiers.Show(Tiers='{commande.CtNum}')\"");
        }

        private void OpenVbcAction(object parameters)
        {
            LigneCommande ligneCommande = (LigneCommande) parameters;
            Process.Start(cdeRepos.getSagePath(), $"{cdeRepos.fichierGescom} -u={cdeRepos.user} -cmd=\"Document.Show(Type=BonCommandeClient,Piece='{ligneCommande.Piece}')\"");
        }

        private void OpenAbcAction(object parameters)
        {
            Commande commande;
            if (parameters.GetType() != typeof(Commande))
            {
                IList<DataGridCellInfo> list = (IList<DataGridCellInfo>) parameters;
                commande = (Commande) list.First().Item;
            }
            else
            {
                commande = (Commande)parameters;
            }
            
            string args = $"\"{cdeRepos.fichierGescom}\" -u={cdeRepos.user} -cmd=\"Document.Show(Type={commande.Type.ToString()},Piece='{commande.Piece}')\"";
            Process.Start(cdeRepos.getSagePath(), args);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
