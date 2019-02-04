using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using Captura.ImageEditor.DynamicRenderers;

namespace Captura.ImageEditor.Strokes
{
    public class RectangleStroke : Stroke
    {
        private static StylusPointCollection Points(StylusPointCollection stylusPointCollection)
        {
            var start = stylusPointCollection.First().ToPoint();
            var end = stylusPointCollection.Last().ToPoint();

            RectangleDynamicRenderer.Prepare(ref start, ref end, out var _, out var _);

            return new StylusPointCollection(new []
            {
                start,
                new Point(start.X, end.Y),
                end,
                new Point(end.X, start.Y),
                start
            });
        }

        private static DrawingAttributes ModifyAttribs(DrawingAttributes drawingAttributes)
        {
            drawingAttributes.FitToCurve = false;

            return drawingAttributes;
        }

        public RectangleStroke(StylusPointCollection stylusPoints, DrawingAttributes drawingAttributes)
            : base(Points(stylusPoints), ModifyAttribs(drawingAttributes)) { }
    }
}