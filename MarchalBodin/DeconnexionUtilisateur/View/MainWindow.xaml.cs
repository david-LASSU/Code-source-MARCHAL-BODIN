using DeconnexionUtilisateur.ViewModel;
using MBCore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeconnexionUtilisateur
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            this.Language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentCulture.Name);
            InitializeComponent();
            _viewModel = (MainViewModel)DataContext;
            Loaded += _viewModel.OnWindowLoaded;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox s = (ComboBox)sender;
            _viewModel.SelectionChanged((Database) s.SelectedItem);
        }

        private void DecoBtn_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Deconnecter(UsersGrid.SelectedItems);
        }
    }
}
