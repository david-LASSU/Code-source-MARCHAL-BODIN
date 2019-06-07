using System.Windows;
using SyncDbUi.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using System;

namespace SyncDbUi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
            Messenger.Default.Register<string>(this, "LambdaMessage", showMessage);
        }

        private void showMessage(string message)
        {
            MessageBox.Show(message);
        }
    }
}