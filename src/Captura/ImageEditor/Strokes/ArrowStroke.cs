using System.Linq;
using System.Windows.Ink;
using System.Windows.Input;
using Captura.ImageEditor.DynamicRenderers;

namespace Captura.ImageEditor.Strokes
{
    public class ArrowStroke : Stroke
    {
        private static StylusPointCollection Points(StylusPointCollection stylusPoints)
        {
            var start = stylusPoints.First().ToPoint();
            var end = stylusPoints.Last().ToPoint();

            LineDynamicRenderer.Prepare(ref start, ref end);

            ArrowDynamicRenderer.GetArrowPoints(start, end, out var p1, out var p2);

            return new StylusPointCollection(new[] { start, end, p1, end, p2 });
        }

        private static DrawingAttributes ModifyAttribs(DrawingAttributes drawingAttributes)
        {
            drawingAttributes.FitToCurve = false;

            return drawingAttributes;
        }

        public ArrowStroke(StylusPointCollection stylusPoints, DrawingAttributes drawingAttributes)
            : base(Points(stylusPoints), ModifyAttribs(drawingAttributes)) { }
    }
}