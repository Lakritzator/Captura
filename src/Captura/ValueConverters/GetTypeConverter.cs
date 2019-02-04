using System;
using System.Globalization;

namespace Captura.ValueConverters
{
    public class GetTypeConverter : OneWayConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.GetType();
        }
    }
}