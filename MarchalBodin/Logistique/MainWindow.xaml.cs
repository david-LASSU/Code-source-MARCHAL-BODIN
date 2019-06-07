using System.Windows;
using Logistique.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using Logistique.Model;

namespace Logistique
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
            Messenger.Default.Register<string>(this, "DoFocus", doFocus);
            Closing += (s, e) => ViewModelLocator.Cleanup();
        }

        private void doFocus(string msg)
        {
            switch (msg)
            {
                case "GencodEmpl":
                    this.GencodEmpl.Focus();
                    break;
                case "GencodArt":
                    this.GencodArt.Focus();
                    break;
                case "ScrollToSelectedItem":
                    GrdLignes.UpdateLayout();
                    GrdLignes.ScrollIntoView(GrdLignes.SelectedItem);
                    break;
                default:
                    break;
            }
        }
    }
}