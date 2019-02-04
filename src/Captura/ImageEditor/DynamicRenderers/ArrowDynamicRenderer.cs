using System;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Media;
using Captura.ImageEditor.Strokes;

namespace Captura.ImageEditor.DynamicRenderers
{
    public class ArrowDynamicRenderer : DynamicRenderer, IDynamicRenderer
    {
        private bool _isManipulating;

        private Point _firstPoint;

        public ArrowDynamicRenderer()
        {
            _firstPoint = new Point(double.NegativeInfinity, double.NegativeInfinity);
        }

        protected override void OnStylusDown(RawStylusInput rawStylusInput)
        {
            _firstPoint = rawStylusInput.GetStylusPoints().First().ToPoint();
            base.OnStylusDown(rawStylusInput);
        }

        private const double ArrowAngle = Math.PI / 180 * 150;
        private const int ArrowLength = 10;

        public static void GetArrowPoints(Point start, Point end, out Point p1, out Point p2)
        {
            var theta = Math.Atan2(end.Y - start.Y, end.X - start.X);

            p1 = new Point(end.X + ArrowLength * Math.Cos(theta + ArrowAngle),
                end.Y + ArrowLength * Math.Sin(theta + ArrowAngle));

            p2 = new Point(end.X + ArrowLength * Math.Cos(theta - ArrowAngle),
                end.Y + ArrowLength * Math.Sin(theta - ArrowAngle));
        }

        private static void Draw(DrawingContext drawingContext, Point start, Point end, Pen pen)
        {
            LineDynamicRenderer.Prepare(ref start, ref end);

            drawingContext.DrawLine(pen, start, end);

            GetArrowPoints(start, end, out var p1, out var p2);

            drawingContext.DrawLine(pen, end, p1);
            drawingContext.DrawLine(pen, end, p2);
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

            Draw(drawingContext, _firstPoint, stylusPoints.First().ToPoint(), new Pen(fillBrush, 2));
        }

        protected override void OnStylusUp(RawStylusInput rawStylusInput)
        {
            _firstPoint = new Point(double.NegativeInfinity, double.NegativeInfinity);
            base.OnStylusUp(rawStylusInput);
        }

        public Stroke GetStroke(StylusPointCollection stylusPoints, DrawingAttributes drawingAttribs)
        {
            return new ArrowStroke(stylusPoints, drawingAttribs);
        }
    }
}
