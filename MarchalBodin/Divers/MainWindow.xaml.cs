using System.Windows;
using Divers.ViewModel;
using System.Threading;
using System.Diagnostics;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media;
using System;
using System.Windows.Input;
using System.Linq;
using Divers.Model;
using GalaSoft.MvvmLight.Messaging;

namespace Divers
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
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.KeyDownEvent, new KeyEventHandler(this.TextBox_KeyDown));
            Messenger.Default.Register<string>(this, "DoFocus", doFocus);
#if !DEBUG
            this.Activated += window_Activated;
            this.Deactivated += window_Deactivated;
#endif

        }

        private void window_Activated(object sender, EventArgs e)
        {
            Topmost = true;
        }

        private void window_Deactivated(object sender, EventArgs e)
        {
            this.Topmost = true;
            this.Activate();
        }
        //private void MainWindow_OnStateChanged(object sender, EventArgs e)
        //{

        //    if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
        //}

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var box = (sender as TextBox);
            Debug.Print(box.Name);
            if (box.Name == "Commentaire")
            {
                e.Handled = false;
                return;
            }
            var focusedElement = Keyboard.FocusedElement as UIElement;
            switch (e.Key)
            {
                case Key.Enter:
                    if (focusedElement != null)
                    {
                        focusedElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    }
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            if (child == null) return null;

            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        /// <summary>
        /// Permet de refermer une ligne encliquant dessus
        /// Le fait de vérifier le parent résoud un problème de comportement
        /// de clic dans le rowDetailTemplate qui declenche l'évènement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GrdLignes_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource != null)
            {
                var c = FindParent<DataGridCell>(e.OriginalSource as DependencyObject);

                if (c != null)
                {
                    Visibility newVis = GrdLignes.GetDetailsVisibilityForItem(GrdLignes.SelectedItem) == Visibility.Visible
                        ? Visibility.Collapsed
                        : Visibility.Visible;

                    GrdLignes.Items.Cast<Ligne>().ToList().ForEach((l) => GrdLignes.SetDetailsVisibilityForItem(l, l == GrdLignes.SelectedItem ? newVis : Visibility.Collapsed));
                }
            }
        }

        private void doFocus(string msg)
        {
            switch (msg)
            {
                case "ScrollBottom":
                    ScrollGrd.ScrollToBottom();
                    break;
                default:
                    break;
            }
        }
    }
}