using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Media;
using Captura.Base.Services;
using Captura.ImageEditor.Controls;

namespace Captura.ValueConverters
{
    public class InkToolToIconConverter : OneWayConverter
    {
        private static string GetPath(object value)
        {
            var icons = ServiceProvider.Get<IIconSet>();

            switch (value)
            {
                case InkCanvasEditingMode.Ink:
                case ExtendedInkTool.Pen:
                    return icons.Pencil;

                case InkCanvasEditingMode.EraseByPoint:
                case ExtendedInkTool.Eraser:
                    return icons.Eraser;

                case InkCanvasEditingMode.EraseByStroke:
                case ExtendedInkTool.StrokeEraser:
                    return icons.StrokeEraser;

                case InkCanvasEditingMode.Select:
                case ExtendedInkTool.Select:
                    return icons.Select;

                case ExtendedInkTool.Line:
                    return icons.Line;

                case ExtendedInkTool.Rectangle:
                    return icons.Rectangle;

                case ExtendedInkTool.Ellipse:
                    return icons.Ellipse;

                case ExtendedInkTool.Arrow:
                    return icons.Arrow;
            }

            return icons.Cursor;
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Geometry.Parse(GetPath(value));
        }
    }
}