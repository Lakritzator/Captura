using System.Linq;
using System.Windows.Ink;
using System.Windows.Input;
using Captura.ImageEditor.DynamicRenderers;

namespace Captura.ImageEditor.Strokes
{
    public class LineStroke : Stroke
    {
        private static StylusPointCollection Points(StylusPointCollection stylusPoints)
        {
            var start = stylusPoints.First().ToPoint();
            var end = stylusPoints.Last().ToPoint();

            LineDynamicRenderer.Prepare(ref start, ref end);

            return new StylusPointCollection(new[] { start, end });
        }

        public LineStroke(StylusPointCollection stylusPoints, DrawingAttributes drawingAttributes) : base(Points(stylusPoints), drawingAttributes) { }
    }
}