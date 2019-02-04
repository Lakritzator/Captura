using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Captura.Base.Services;
using Captura.Core.Models;

namespace Captura.ValueConverters
{
    public class StateToRecordButtonGeometryConverter : OneWayConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var icons = ServiceProvider.Get<IIconSet>();

            if (value is RecorderState state)
            {
                return Geometry.Parse(state == RecorderState.NotRecording
                    ? icons.Record
                    : icons.Stop);
            }

            return Binding.DoNothing;
        }
    }
}