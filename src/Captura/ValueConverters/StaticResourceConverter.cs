using System;
using System.Globalization;
using System.Windows;

namespace Captura.ValueConverters
{
    public class StaticResourceConverter : OneWayConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Application.Current.Resources[value] : null;
        }
    }
}