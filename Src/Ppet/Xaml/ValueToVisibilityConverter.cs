using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ppet.Xaml
{
    public class ValueToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = IsNotEmptyConverter.Convert(value);
            return (Equals(parameter, true) ? !result : result)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
