using LiaisonsDocVente.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace LiaisonsDocVente.View
{
    //
    //
    // CONVERTER NON UTILISE
    //
    //
    class TotalLiaisonsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Collection<LiaisonCde> liaisons = (Collection<LiaisonCde>)value;

            //string total = liaisons == null ? "0" : liaisons.Sum(l => double.Parse(l.Qte)).ToString();

            //return $"Total réservé : {total}";
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
