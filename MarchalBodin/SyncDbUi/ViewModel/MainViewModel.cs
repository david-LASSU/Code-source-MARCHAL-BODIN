using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using SyncDbUi.Model;
using System.Diagnostics;
using MBCore.Model;
using System.Linq;
using System.Collections.ObjectModel;
using SyncDb.Model;
using System.Collections;
using GalaSoft.MvvmLight.Messaging;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SyncDbUi.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Init Vars
        private readonly IDataService _dataService;

        private Database _sourceDb;
        private Database _targetDb;

        private string _title;
        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }

        private IList<string> _objectNameList;
        public IList<string> ObjectNameList
        {
            get { return _objectNameList; }
            set { Set(ref _objectNameList, value); }
        }

        private string _currentObjectName;
        public string CurrentObjectName
        {
            get { return _currentObjectName; }
            set
            {
                Set(ref _currentObjectName, value);
                Items = null;
            }
        }

        private string _pkFilter;
        public string PkFilter
        {
            get { return _pkFilter; }
            set { Set(ref _pkFilter, value); }
        }

        public RelayCommand LoadCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }

        private ObservableCollection<Ligne> _items;

        public ObservableCollection<Ligne> Items
        {
            get { return _items; }
            set { Set(ref _items, value); }
        }

        private string _logText;
        public string LogText
        {
            get { return _logText; }
            set { Set(ref _logText, value); }
        }
        public RelayCommand<IList> SelectionChangedCommand { get; private set; }
        private IList _selectedItems;
        public IList SelectedItems
        {
            get { return _selectedItems; }
            set { Set(ref _selectedItems, value); }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            try
            {
                _dataService = dataService;
                LoadCommand = new RelayCommand(Load);
                SaveCommand = new RelayCommand(Save);
                SelectionChangedCommand = new RelayCommand<IList>(SelectionChanged);
                var dbList = new DatabaseList();
                if (IsInDesignMode)
                {
                    _sourceDb = dbList.First(d => d.name == "TARIF");
                    _targetDb = dbList.First(d => d.name == "SMODEV");
                }
                else
                {
                    string[] args = Environment.GetCommandLineArgs();
                    if (args.Length < 2)
                    {
                        throw new Exception("Nombre d'argument insuffisant");
                    }
                    _sourceDb = dbList.First(d => d.name == args[1]);
                    _targetDb = dbList.First(d => d.name == args[2]);
                }
                _dataService.SetDatabases(_targetDb, _sourceDb);
                ObjectNameList = _dataService.GetSyncablesObjects();
                _dataService.Log += Log;
                CurrentObjectName = "ARTICLE";
                Title = $"Synchronisation de {_sourceDb.name} vers {_targetDb.name}";
            }
            catch (Exception ex)
            {
                Messenger.Default.Send(ex.Message, "LambdaMessage");
            }
        }

        private void Log(string message, Database targetDb)
        {
            Debug.Print(message);
            LogText += message + Environment.NewLine;
        }

        private void Save()
        {
            if (SelectedItems == null)
            {
                return;
            }
            Task.Factory.StartNew(() => _dataService.Save(CurrentObjectName, SelectedItems, Items, () => Messenger.Default.Send("Mise à jour terminée", "LambdaMessage")));
        }

        private void SelectionChanged(IList s)
        {
            SelectedItems = s;
        }

        private void Load()
        {
            _dataService.Load(CurrentObjectName, PkFilter, 
                (result, error) => {
                    if (error != null)
                    {
                        LogText += error.Message;
                        return;
                    }

                    Items = new ObservableCollection<Ligne>(result);
                    if (Items.Count == 0)
                    {
                        Messenger.Default.Send("Rien à sync", "LambdaMessage");
                    }
                });
            
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}