using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReceptionCommande
{
    public static class KeyDownBehaviour
    {
        public static readonly DependencyProperty KeyDownCommandProperty = DependencyProperty.RegisterAttached(
                "KeyDownCommand",
                typeof(ICommand),
                typeof(KeyDownBehaviour),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnKeyDownCommandPropertyChanged)));

        public static ICommand GetKeyDownCommand(DependencyObject d)
        {
            return (ICommand)d.GetValue(KeyDownCommandProperty);
        }

        public static void SetKeyDownCommand(DependencyObject d, ICommand value)
        {
            d.SetValue(KeyDownCommandProperty, value);
        }

        private static void OnKeyDownCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = d as UIElement;
            if (element != null)
            {
                if ((e.OldValue == null) && (e.NewValue != null))
                {
                    element.PreviewKeyDown += new KeyEventHandler(OnUIElementKeyDownEvent);
                    //element.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(OnUIElementKeyDownEvent), true);
                }
                else if ((e.OldValue != null) && (e.NewValue == null))
                {
                    element.PreviewKeyDown -= new KeyEventHandler(OnUIElementKeyDownEvent);
                    //element.RemoveHandler(UIElement.KeyDownEvent, new KeyEventHandler(OnUIElementKeyDownEvent));
                }
            }
        }

        static void OnUIElementKeyDownEvent(object sender, KeyEventArgs e)
        {
            UIElement element = sender as UIElement;

            if (element == null) { return; }
            
            DataGrid dg = (DataGrid)e.Source;
            if ((dg.CurrentCell.Column.GetCellContent(dg.SelectedItem).Parent as DataGridCell).IsEditing) { return; }

            ICommand iCommand = (ICommand)element.GetValue(KeyDownCommandProperty);
            if (iCommand != null)
            {
                iCommand.Execute(e);
            }
            e.Handled = true;
        }
    }
}
