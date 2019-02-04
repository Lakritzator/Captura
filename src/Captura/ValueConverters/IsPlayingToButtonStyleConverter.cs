using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Captura.Base.Services;
using Captura.Controls;

namespace Captura.ValueConverters
{
    public class IsPlayingToButtonStyleConverter : OneWayConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                var icons = ServiceProvider.Get<IIconSet>();

                var icon = Geometry.Parse(b ? icons.Stop : icons.Play);
                var color = b ? Colors.OrangeRed : Colors.LimeGreen;

                return new Style(typeof(ModernButton), (Style) Application.Current.Resources[typeof(ModernButton)])
                {
                    Setters =
                    {
                        new Setter(ModernButton.IconDataProperty, icon),
                        new Setter(Control.ForegroundProperty, new SolidColorBrush(color))
                    }
                };
            }

            return Binding.DoNothing;
        }
    }
}