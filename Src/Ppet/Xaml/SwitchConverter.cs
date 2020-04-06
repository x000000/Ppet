using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Ppet.Xaml
{
    [DefaultProperty(nameof(Conditions))]
    [ContentProperty(nameof(Conditions))]
    public class SwitchConverter : IValueConverter
    {
        public List<ValueCondition> Conditions { get; set; } = new List<ValueCondition>();

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var condition in Conditions) {
                if (Equals(condition.When, value)) {
                    return condition.Then;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    [DefaultProperty(nameof(Then))]
    [ContentProperty(nameof(Then))]
    public class ValueCondition
    {
        public object? When { get; set; }
        public object? Then { get; set; }
    }
}