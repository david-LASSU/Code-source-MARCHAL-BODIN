using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ListeCommandes.View
{
    public class TypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (null == value)
                return DependencyProperty.UnsetValue;

            int input = (int) value;
            switch (input)
            {
                case 11:
                    return Brushes.LightBlue;
                case 12:
                    return Brushes.LightSalmon;
                case 13:
                    return Brushes.LightGreen;
                default:
                    return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
