using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using Captura.Presentation;
using WpfColor = System.Windows.Media.Color;

namespace Captura.ValueConverters
{
    public class DrawingToWpfColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color c)
                return c.ToWpfColor();

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case string s:
                    return ColorTranslator.FromHtml(s);

                case WpfColor c:
                    return c.ToString();

                default:
                    return Binding.DoNothing;
            }
        }
    }
}