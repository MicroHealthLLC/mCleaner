using System;
using System.Windows;
using System.Windows.Data;

namespace mCleaner.Helpers.Converters
{
    public class Converter_BooleanToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility ret = Visibility.Visible;

            if (parameter != null)
            {
                ret = (bool)value ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                ret = (bool)value ? Visibility.Visible : Visibility.Collapsed;
            }

            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
