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
    public class LineDynamicRenderer : DynamicRenderer, IDynamicRenderer
    {
        private bool _isManipulating;

        private Point _firstPoint;

        public LineDynamicRenderer()
        {
            _firstPoint = new Point(double.NegativeInfinity, double.NegativeInfinity);
        }

        protected override void OnStylusDown(RawStylusInput rawStylusInput)
        {
            _firstPoint = rawStylusInput.GetStylusPoints().First().ToPoint();
            base.OnStylusDown(rawStylusInput);
        }

        public static void Prepare(ref Point start, ref Point end)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                var sdx = end.X - start.X;
                var sdy = end.Y - start.Y;

                var dx = Math.Abs(sdx);
                var dy = Math.Abs(sdy);

                if (dx < dy / 2)
                {
                    end.X = start.X;
                }
                else if (dy < dx / 2)
                {
                    end.Y = start.Y;
                }
                else
                {
                    var d = Math.Min(dx, dy);

                    end.X = start.X + Math.Sign(sdx) * d;
                    end.Y = start.Y + Math.Sign(sdy) * d;
                }
            }
        }

        private static void Draw(DrawingContext drawingContext, Point start, Point end, Pen pen)
        {
            Prepare(ref start, ref end);

            drawingContext.DrawLine(pen, start, end);
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
            return new LineStroke(stylusPoints, drawingAttribs);
        }
    }
}
