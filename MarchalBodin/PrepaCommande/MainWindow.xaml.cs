using System.Windows;
using PrepaCommande.ViewModel;
using System.Windows.Controls;
using System.Diagnostics;

namespace PrepaCommande
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

        /// <summary>
        /// Retourne sur le champ de détection de code barre
        /// après édition de la quantité
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GrdLignes_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            //TxtCodeBarre.Focus();
        }

        /// <summary>
        /// Force la selection du texte quand on rentre dans une cellule
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GrdLignes_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            var txt = (TextBox)e.EditingElement;
            txt.SelectAll();
            return;
        }

        private void GrdLignes_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }
    }
}