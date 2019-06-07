using ListeCommandes.Model;
using System.Windows;
using System.Windows.Controls;

namespace ListeCommandes.View
{
    public class ReliquatTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null)
            {
                Commande commande = (Commande)item;

                FrameworkElement element = (FrameworkElement)container;
                if (commande.Relicat == true)
                {
                    return element.FindResource("ReliquatTemplate") as DataTemplate;
                }
            }

            return new DataTemplate();
        }
    }
}
