using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using ListeCommandes.Model;

namespace ListeCommandes.View
{
    class SumCommandesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() == typeof(GroupItem))
            {
                GroupItem groupItem = (GroupItem) value;
                CollectionViewGroup collectionViewGroup = (CollectionViewGroup) groupItem.Content;
                double sum = collectionViewGroup.Items.Select(item => item as Commande).Select(commande => commande.TotalHT).Sum();
                
                return $"Total HT: {sum.ToString("C", CultureInfo.GetCultureInfo("fr-FR"))}";
            }

            //@deprecated
            //if (value.GetType() == typeof(DataGrid))
            //{
            //    DataGrid dataGrid = (DataGrid) value;
            //    double sum = dataGrid.Items.Cast<Commande>().Sum(commmande => commmande.TotalHT);

            //    return $"Total : {sum}";
            //}

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
