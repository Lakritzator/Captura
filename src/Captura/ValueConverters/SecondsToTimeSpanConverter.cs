using System;
using System.Globalization;
using System.Windows.Data;

namespace Captura.ValueConverters
{
    public class SecondsToTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int seconds)
                return TimeSpan.FromSeconds(seconds);

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan t)
                return t.TotalSeconds;

            return Binding.DoNothing;
        }
    }
}
