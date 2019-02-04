using System;
using System.Globalization;
using System.Windows.Data;

namespace Captura.ValueConverters
{
    public class IntegralTimeSpanConverter : OneWayConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan t)
                return TimeSpan.FromSeconds((int) t.TotalSeconds);

            return Binding.DoNothing;
        }
    }
}
