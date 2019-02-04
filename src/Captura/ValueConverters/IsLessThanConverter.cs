using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Captura.ValueConverters
{
    public class IsLessThanConverter : OneWayConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value?.ToString(), out var left) && double.TryParse(parameter?.ToString(), out var right))
            {
                var b =  left < right;

                if (targetType == typeof(Visibility))
                    return b ? Visibility.Visible : Visibility.Collapsed;

                return b;
            }

            return Binding.DoNothing;
        }
    }
}