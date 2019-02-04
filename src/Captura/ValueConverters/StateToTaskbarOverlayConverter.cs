using System;
using System.Globalization;
using Captura.Core.Models;

namespace Captura.ValueConverters
{
    public class StateToTaskbarOverlayConverter : OneWayConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch(value)
            {
                case RecorderState.Recording:
                    return "/Images/record.ico";

                case RecorderState.Paused:
                    return "/Images/pause.ico";

                default:
                    return null;
            }
        }
    }
}