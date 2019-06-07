using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using LiaisonsDocVente.Model;
using LiaisonsDocVente.ViewModel;
using System.Windows.Markup;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LiaisonsDocVente.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public RelierWindow RelierWindow;

        public MainWindow()
        {
            this.Language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentCulture.Name);
            InitializeComponent();
            _viewModel = (MainViewModel) DataContext;
            Loaded += _viewModel.OnWindowLoaded;
            Closing += _viewModel.OnWindowClosing;
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

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        //private void MainWindow_OnStateChanged(object sender, EventArgs e)
        //{

        //    if (WindowState == WindowState.Minimized ||WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
        //}

        private void RelierBtn_Click(object sender, RoutedEventArgs e)
        {
            Button s = (Button)sender;
            if (RelierWindow != null)
            {
                RelierWindow.Close();
                RelierWindow = null;
            }

            RelierWindow = new RelierWindow((Contremarque) s.DataContext);
            RelierWindow.Show();
            RelierWindow.Closed += RelierWindow_Closed;
            _viewModel.MainGridEnabled = false;
        }

        private void RelierWindow_Closed(object sender, EventArgs e)
        {
            _viewModel.MainGridEnabled = true;
        }

        private void logBox_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            MainScroll.ScrollToBottom();
        }

        private void ValiderFournList_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ApplyFournList(GrdLignes.SelectedItems);
        }
    }
}
