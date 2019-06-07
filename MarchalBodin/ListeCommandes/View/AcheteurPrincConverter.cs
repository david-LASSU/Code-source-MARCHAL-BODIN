using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using ListeCommandes.Model;

namespace ListeCommandes.View
{
    class AcheteurPrincConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GroupItem groupItem = (GroupItem) value;
            CollectionViewGroup collectionViewGroup = (CollectionViewGroup) groupItem.Content;

            Commande commande = (Commande) collectionViewGroup.Items[0];

            return (string.IsNullOrEmpty(commande.AcheteurPrinc)) ? string.Empty : $"{commande.AcheteurPrinc}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
