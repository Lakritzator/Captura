using System;
using System.Globalization;
using System.Windows.Data;
using Captura.Core.Models;

namespace Captura.ValueConverters
{
    public class NotRecordingConverter : OneWayConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RecorderState state)
                return state == RecorderState.NotRecording;

            return Binding.DoNothing;
        }
    }
}