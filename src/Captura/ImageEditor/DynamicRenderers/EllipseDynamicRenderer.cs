using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Media;
using Captura.ImageEditor.Strokes;

namespace Captura.ImageEditor.DynamicRenderers
{
    public class EllipseDynamicRenderer : DynamicRenderer, IDynamicRenderer
    {
        private bool _isManipulating;

        private Point _firstPoint;

        public EllipseDynamicRenderer()
        {
            _firstPoint = new Point(double.NegativeInfinity, double.NegativeInfinity);
        }

        protected override void OnStylusDown(RawStylusInput rawStylusInput)
        {
            _firstPoint = rawStylusInput.GetStylusPoints().First().ToPoint();
            base.OnStylusDown(rawStylusInput);
        }

        public static void Draw(DrawingContext drawingContext, Point start, Point end, Pen pen)
        {
            RectangleDynamicRenderer.Prepare(ref start, ref end, out var w, out var h);

            var center = new Point(start.X + w / 2, start.Y + h / 2);

            drawingContext.DrawEllipse(null, pen, center, w / 2, h / 2);
        }

        protected override void OnDraw(DrawingContext drawingContext, StylusPointCollection stylusPoints, Geometry geometry, Brush fillBrush)
        {
            if (!_isManipulating)
            {
                _isManipulating = true;

                var currentStylus = Stylus.CurrentStylusDevice;
                Reset(currentStylus, stylusPoints);
            }

            _isManipulating = false;

            Draw(drawingContext,
                _firstPoint,
                stylusPoints.First().ToPoint(),
                new Pen(fillBrush, 2));
        }

        protected override void OnStylusUp(RawStylusInput rawStylusInput)
        {
            _firstPoint = new Point(double.NegativeInfinity, double.NegativeInfinity);
            base.OnStylusUp(rawStylusInput);
        }

        public Stroke GetStroke(StylusPointCollection stylusPoints, DrawingAttributes drawingAttribs)
        {
            return new EllipseStroke(stylusPoints, drawingAttribs);
        }
    }
}
