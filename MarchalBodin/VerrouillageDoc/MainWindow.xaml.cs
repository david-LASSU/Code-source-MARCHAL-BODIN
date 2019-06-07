using MBCore.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VerrouillageDoc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string doPiece;

        private LockRepository repos;

        private bool isVerrou;

        public bool IsVerrou
        {
            get { return isVerrou; }
            set
            {
                isVerrou = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
#if !DEBUG
            this.Activated += window_Activated;
            this.Deactivated += window_Deactivated;
#endif
            try
            {
                string[] args = Environment.GetCommandLineArgs();
                if (args.Length < 3)
                {
                    throw new Exception("Nombre d'argument insuffisant");
                }

                BaseCialAbstract.setDefaultParams(args[1], args[2]);
                doPiece = args[3];

                repos = new LockRepository();

                Title = $"Activer/Désactiver doc {doPiece}";
                IsVerrou = repos.isVerrou(doPiece);
                updateUi();
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                MessageBox.Show(e.ToString());
            }
        }

        private void window_Activated(object sender, EventArgs e)
        {
            //this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            //this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            this.Topmost = true;
            //this.Top = 0;
            //this.Left = 0;
        }

        private void window_Deactivated(object sender, EventArgs e)
        {
            this.Topmost = true;
            this.Activate();
        }

        private void updateUi()
        {
            if (IsVerrou)
            {
                StatutLabel.Background = Brushes.Red;
                StatutLabel.Content = "Document verrouillé";
                StatutBtn.Content = "Déverrouiller";
            }
            else
            {
                StatutLabel.Background = Brushes.GreenYellow;
                StatutLabel.Content = "Document déverrouillé";
                StatutBtn.Content = "Verrouiller";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CheckDocumentIsClosed();
            try
            {
                string result = repos.Toggle(doPiece);
                if (result == "OK")
                {
                    IsVerrou = !IsVerrou;
                    updateUi();
                }
                else
                {
                    throw new Exception(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void CheckDocumentIsClosed()
        {
            try
            {
                repos.DocumentIsClosed(doPiece);
            }
            catch (Exception e)
            {
                if (MessageBox.Show(
                e.Message,
                "Envoi du document",
                MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    CheckDocumentIsClosed();
                }
                else
                {
                    throw new Exception("Action annulée par l'utilisateur.");
                }
            }
        }
    }
}
