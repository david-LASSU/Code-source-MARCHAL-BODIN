using LiaisonsDocVente.Model;
using LiaisonsDocVente.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;

namespace LiaisonsDocVente.View
{
    /// <summary>
    /// Interaction logic for RelierWindow.xaml
    /// </summary>
    public partial class RelierWindow : Window
    {
        private RelierViewModel _viewModel;
        public RelierWindow(Contremarque cm)
        {
            this.Language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentCulture.Name);
            InitializeComponent();
            _viewModel = (RelierViewModel)DataContext;
            _viewModel.Contremarque = cm;
            Title = $"Liaison de la Ref {cm.RefMag} à des doc existants";
            Loaded += _viewModel.OnWindowLoaded;
            Closing += RelierWindow_Closing;
#if !DEBUG
            this.Activated += window_Activated;
            this.Deactivated += window_Deactivated;
#endif
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

        private void RelierWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_viewModel.Contremarque.Liaisons.Sum(l=>l.Qte) > Convert.ToDouble(_viewModel.Contremarque.Qte, CultureInfo.CurrentCulture))
            {
                e.Cancel = true;
                return;
            }
        }

        private void LiaisonsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void fermerBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
