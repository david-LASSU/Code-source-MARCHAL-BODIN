using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ListeCommandes.View
{
    class StatutToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (null == value)
                return DependencyProperty.UnsetValue;

            int input = (int)value;
            switch (input)
            {
                case 0:
                    return Brushes.LightBlue;
                case 1:
                    return Brushes.LightSalmon;
                case 2:
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
