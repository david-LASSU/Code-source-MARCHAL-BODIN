using System.Windows;
using ReceptionCommande.ViewModel;
using System.Windows.Controls;
using ReceptionCommande.Model;
using System.Diagnostics;
using System.Data;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Input;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace ReceptionCommande
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
        }

        private void GrdLignes_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            ((sender as DataGrid).SelectedItem as DocumentLigne).RowChecked = true;
        }

        /// <summary>
        /// TODO 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void GrdLignes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    foreach (var c in GrdLignes.SelectedCells)
        //    {
        //        if (c.Column.Header.ToString() == "Quantité Livrée")
        //        {
        //            var cellContent = c.Column.GetCellContent(c.Item);
        //            if (cellContent != null)
        //            {
        //                var dc = (DataGridCell)cellContent.Parent;
        //                dc.Focus();
        //                dc.IsEditing = true;
        //            }
        //        }
        //    }
        //}
        private void TextBox_QteBL_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            Keyboard.Focus(textBox);
            textBox.CaretIndex = textBox.Text.Length;
            textBox.SelectAll();
        }
    }
}