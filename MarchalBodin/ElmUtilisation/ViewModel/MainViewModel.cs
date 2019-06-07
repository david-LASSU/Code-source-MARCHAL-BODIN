using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using ElmUtilisation.Model;
using MBCore.Model;
using System.Timers;
using System.Diagnostics;

namespace ElmUtilisation.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        private LockLoggingRepository _lockLoggingRepository;
        private Timer _timer;

        private ObservableCollection<LockLog> _lockLogs;

        public ObservableCollection<LockLog> LockLogs
        {
            get { return _lockLogs; }
            set
            {
                _lockLogs = value;
                OnPropertyChanged("LockLogs");
            }
        }

        public MainViewModel()
        {
            try
            {
                if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                {
                    // Design time logic
                    BaseCialAbstract.setDefaultParams("G:\\_SOCIETES\\DEV\\SMODEV.gcm");
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
                
                _lockLoggingRepository = new LockLoggingRepository();
                LoadDatas();

                _timer = new Timer(2000);
                _timer.Elapsed += OnTick;
                _timer.Enabled = true;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Application", ex.ToString());
                MessageBox.Show(ex.Message);
                throw;
            }
        }

        private void LoadDatas()
        {
            LockLogs = new ObservableCollection<LockLog>(_lockLoggingRepository.GetAll());
        }

        private void OnTick(object sender, EventArgs eventArgs)
        {
            LoadDatas();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
