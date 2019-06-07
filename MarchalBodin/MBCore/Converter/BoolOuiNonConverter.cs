using System;
using System.Globalization;
using System.Windows.Data;

namespace MBCore.Converter
{
    public class BoolOuiNonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool) value) ? "Oui" : "Non";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value) == "Oui";
        }
    }
}
