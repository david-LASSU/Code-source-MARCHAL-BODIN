using System;
using System.Collections.Generic;
using System.Windows;
using ListeCommandes.ViewModel;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using ListeCommandes.Model;
using System.Windows.Threading;

namespace ListeCommandes.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();
            //DocStatutBox.SelectAll();
            ApcTypeBox?.SelectAll();
            //ApcTypeBox.SelectedIndex = 0;
            //ApcTypeBox.IsEnabled = true;
        }

        private DataGrid _getDataGridByName(string name)
        {
            if (name == AllDatagrid.Name)
            {
                return AllDatagrid;
            }

            if (name == CmDatagrid.Name)
            {
                return CmDatagrid;
            }

            throw new Exception($"Datagrid {name} not found");
        }

        private void ExpandAll_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button) sender;
            Collection<Expander> collection = VisualTreeHelper.GetVisualChildren<Expander>(_getDataGridByName(button.CommandParameter.ToString()));

            foreach (Expander expander in collection)
                expander.IsExpanded = true;
        }

        private void CollapseAll_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Collection<Expander> collection = VisualTreeHelper.GetVisualChildren<Expander>(_getDataGridByName(button.CommandParameter.ToString()));

            foreach (Expander expander in collection)
                expander.IsExpanded = false;
        }

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer) sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void DocTypeBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = (ListBox)sender;
            // Empêche de ne rien sélectionner
            if (listBox.SelectedItems.Count == 0)
            {
                listBox.SelectedIndex = 0;
            }
            
            if (ApcTypeBox != null)
            {
                ApcTypeBox.IsEnabled = (listBox.SelectedItems.Count == 1 && listBox.SelectedIndex == 0);

            }

            ApcTypeBox?.SelectAll();
            DocStatutBox?.SelectAll();

            ViewModel.SelectedCommandeTypes = (from KeyValuePair<CommandeType, string> item in listBox.SelectedItems select item.Key).ToList();
        }

        private void DocStatutBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = (ListBox)sender;
            // Empêche de ne rien sélectionner
            if (listBox.SelectedItems.Count == 0)
            {
                listBox.SelectAll();
            }

            ViewModel.SelectedCommandeStatuts = (from KeyValuePair<CommandeStatut, string> item in listBox.SelectedItems select item.Key).ToList();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tc = (TabControl)sender;
            TabItem item = (TabItem) tc.SelectedItem;
            
            if (item.Name == "RecepTab")
            {
                DocTypeBox.SelectedIndex = 1;
                ApcTypeBox.SelectAll();
                DocStatutBox.SelectedIndex = 2;
                AutoLoadCheckBox.IsChecked = true;
                FournFiltreGrid.IsEnabled = false;
                DateLivBox.SelectedIndex = 0;
                Acheteur.SelectedIndex = 0;
                ViewModel.DateLivFrom = null;
                ViewModel.DateLivTo = null;
                ViewModel.DateDocFrom = null;
            }
            else
            {
                FournFiltreGrid.IsEnabled = true;
                AutoLoadCheckBox.IsChecked = false;
            }

            ViewModel.SelectedTab = item.Name;
        }

        private void ApcTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //DocTypeBox.IsEnabled = (ApcTypeBox.SelectedIndex == 0);
            ViewModel.SelectectedApcTypes.Clear();
            foreach (KeyValuePair<ApcType,string> selected in ApcTypeBox.SelectedItems)
            {
                ViewModel.SelectectedApcTypes.Add(selected.Key);
            }
        }
    }
}
