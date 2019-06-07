using DeconnexionUtilisateur.Model;
using MBCore.Model;
using MBCore.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections;
using DeconnexionUtilisateur.IntermagService;
using System.ServiceModel;

namespace DeconnexionUtilisateur.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Init Vars
        private UserRepository Repos => new UserRepository();

        private ObservableCollection<User> users;
        public ObservableCollection<User> Users
        {
            get { return users; }
            set
            {
                users = value;
                OnPropertyChanged("Users");
            }
        }

        private ObservableCollection<Database> dbs;
        public ObservableCollection<Database> Dbs
        {
            get { return dbs; }
            set
            {
                dbs = value;
                OnPropertyChanged("Dbs");
            }
        }

        private string message;
        public string Message
        {
            get { return message; }
            set
            {
                message = value;
                OnPropertyChanged("Message");
            }
        }

        private Brush bgMessage;
        public Brush BgMessage
        {
            get { return bgMessage; }
            set
            {
                bgMessage = value;
                OnPropertyChanged("BgMessage");
            }
        }

        private bool canDisconnect;
        public bool CanDisconnect
        {
            get { return canDisconnect; }
            set
            {
                canDisconnect = value;
                OnPropertyChanged("CanDisconnect");
            }
        }

        public ICommand CommandRafraichir { get; set; }
        public ICommand CommandGhost { get; set; }

        private Database currentDb;
        #endregion

        public MainViewModel()
        {
            CommandRafraichir = new Command(Rafraichir);
            CommandGhost = new Command(Ghost);
        }

        internal void SelectionChanged(Database db)
        {
            currentDb = db;

            if (currentDb.id == 0)
            {
                Users = new ObservableCollection<User>();
                CanDisconnect = Users.Count > 0;
            }
            else
            {
                BaseCialAbstract.setDefaultParams(Repos.getFichierGescom(db.name));
                Load();
            }
        }

        private void Rafraichir(object obj)
        {
            if (currentDb.id == 0)
            {
                return;
            }
            Load();
        }

        internal void Deconnecter(IList selectedItems)
        {
            if (currentDb.id == 0)
            {
                return;
            }
            try
            {
                Repos.Disconnect(selectedItems);
                Load();
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("Application", e.ToString(), EventLogEntryType.Error);
                MessageBox.Show(e.Message);
            }
        }

        private void Ghost(object obj)
        {
            if (currentDb.id == 0)
            {
                return;
            }
            string message = $@"ATTENTION!{Environment.NewLine}Utiliser cette procédure uniquement si la déconnexion classique n'a pas aboutie!{Environment.NewLine}Êtes-vous sûr de vouloir effectuer cette opération?";
            if (MessageBox.Show(message,"", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Repos.Ghost();
            }

            Load();
        }

        private void Load()
        {
            if (currentDb.id == 0)
            {
                return;
            }
            try
            {
                Users = new ObservableCollection<User>(Repos.GetUsers());
                CanDisconnect = Users.Count > 0;
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("Application", e.ToString(), EventLogEntryType.Error);
                MessageBox.Show(e.ToString());
            }
        }

        internal void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    Users = new ObservableCollection<User>(Repos.GetUsersSample());
                }
                else
                {
                    // On récupère la lsite des bases
                    var dbss = new ObservableCollection<Database>(Repos.dbList);
                    dbss.Insert(0, new Database() { name = "Choisir..." });
                    Dbs = dbss;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Shutdown();
            }
        }

        private void setMessage(string message, Color c)
        {
            Message = message;
            BgMessage = new SolidColorBrush(c);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
