using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace Ppet.Xaml
{
    public class IsNotEmptyConverter : IValueConverter
    {
        public static bool Convert(object value, bool trimText = false) => value switch {
            bool b => b,
            int  i => i != 0,
            long l => l != 0,
            string s => (trimText ? s.Trim().Length : s.Length) > 0,
            IEnumerable o => o.GetEnumerator().MoveNext(),
            _ => (value != null),
        };

        public bool TrimText { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => Convert(value, TrimText);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
