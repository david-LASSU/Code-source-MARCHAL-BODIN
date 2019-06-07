using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ListeCommandes.Model;

namespace ListeCommandes.View
{
    class EmergencyToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() == typeof(GroupItem))
            {
                GroupItem groupItem = (GroupItem)value;
                CollectionViewGroup collectionViewGroup = (CollectionViewGroup)groupItem.Content;

                if (collectionViewGroup.Items.Cast<Commande>().Count(c => c.IsNonEnvoye && !c.Relicat) > 3)
                {
                    return Brushes.Red;
                }
                
                if (collectionViewGroup.Items.Cast<Commande>().Any(c => c.EmergencyLevel == Commande.EMERGENCY_HIGHT))
                {
                    return Brushes.Red;
                }

                if (collectionViewGroup.Items.Cast<Commande>().Any(c => c.EmergencyLevel == Commande.EMERGENCY_MEDIUM))
                {
                    return Brushes.Coral;
                }
            }

            if (value.GetType() == typeof(Commande))
            {
                Commande commande = (Commande) value;

                if (commande.EmergencyLevel == Commande.EMERGENCY_HIGHT)
                {
                    return Brushes.Red;
                }

                if (commande.EmergencyLevel == Commande.EMERGENCY_MEDIUM)
                {
                    return Brushes.Coral;
                }
            }

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
