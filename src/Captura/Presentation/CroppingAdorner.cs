using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Captura.Base.Services;
using Captura.Controls;
using Point = System.Drawing.Point;

namespace Captura.Presentation
{
    public class CroppingAdorner : Adorner
    {
        #region Private variables
        // Width of the thumbs. I know these really aren't "pixels", but px is still a good mnemonic.
        private const int CpxThumbWidth = 6;

        // PuncturedRect to hold the "Cropping" portion of the adorner
        private readonly PuncturedRect _prCropMask;

        // Canvas to hold the thumbs so they can be moved in response to the user
        private readonly Canvas _cnvThumbs;

        // Cropping adorner uses Thumbs for visual elements.  
        // The Thumbs have built-in mouse input handling.
        private readonly CropThumb _crtTopLeft;

        private readonly CropThumb _crtTopRight;
        private readonly CropThumb _crtBottomLeft;
        private readonly CropThumb _crtBottomRight;

        private readonly CropThumb _crtTop;
        private readonly CropThumb _crtLeft;
        private readonly CropThumb _crtBottom;
        private readonly CropThumb _crtRight;

        private readonly Thumb _crtMove;

        private readonly Border _checkButton;

        // To store and manage the adorner's visual children.
        private readonly VisualCollection _vc;
        #endregion
        
        #region Routed Events
        public static readonly RoutedEvent CropChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(CropChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(CroppingAdorner));

        public event RoutedEventHandler CropChanged
        {
            add => AddHandler(CropChangedEvent, value);
            remove => RemoveHandler(CropChangedEvent, value);
        }

        public event Action Checked;
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty FillProperty = Shape.FillProperty.AddOwner(typeof(CroppingAdorner));

        public Brush Fill
        {
            get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        private static void FillPropChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyObject is CroppingAdorner crp)
            {
                crp._prCropMask.Fill = (Brush)dependencyPropertyChangedEventArgs.NewValue;
            }
        }
        #endregion

        #region Constructor
        static CroppingAdorner()
        {
            var clr = Colors.Red;
            
            clr.A = 80;
            FillProperty.OverrideMetadata(typeof(CroppingAdorner),
                new PropertyMetadata(
                    new SolidColorBrush(clr),
                    FillPropChanged));
        }

        public CroppingAdorner(UIElement adornedElement, Rect rectInit)
            : base(adornedElement)
        {
            _vc = new VisualCollection(this);
            _prCropMask = new PuncturedRect
            {
                IsHitTestVisible = false,
                RectInterior = rectInit,
                Fill = Fill
            };
            _vc.Add(_prCropMask);
            _cnvThumbs = new Canvas
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            _vc.Add(_cnvThumbs);
            
            _crtMove = new Thumb
            {
                Cursor = Cursors.Hand,
                Opacity = 0
            };

            _cnvThumbs.Children.Add(_crtMove);

            BuildCorner(ref _crtTop, Cursors.SizeNS);
            BuildCorner(ref _crtBottom, Cursors.SizeNS);
            BuildCorner(ref _crtLeft, Cursors.SizeWE);
            BuildCorner(ref _crtRight, Cursors.SizeWE);
            BuildCorner(ref _crtTopLeft, Cursors.SizeNWSE);
            BuildCorner(ref _crtTopRight, Cursors.SizeNESW);
            BuildCorner(ref _crtBottomLeft, Cursors.SizeNESW);
            BuildCorner(ref _crtBottomRight, Cursors.SizeNWSE);

            var btn = new ModernButton
            {
                IconData = Geometry.Parse(ServiceProvider.Get<IIconSet>().Check),
                Cursor = Cursors.Hand,
                Foreground = new SolidColorBrush(Colors.White)
            };

            _checkButton = new Border
            {
                CornerRadius = new CornerRadius(20),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = new SolidColorBrush(Colors.LimeGreen),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0.3),
                Child = btn
            };

            _cnvThumbs.Children.Add(_checkButton);

            btn.Click += (sender, e) => Checked?.Invoke();
            
            // Add handlers for Cropping.
            _crtBottomLeft.DragDelta += (sender, e) => HandleDrag(sender, e, 1, 0, -1, 1);
            _crtBottomRight.DragDelta += (sender, e) => HandleDrag(sender, e, 0, 0, 1, 1);
            _crtTopLeft.DragDelta += (sender, e) => HandleDrag(sender, e, 1, 1, -1, -1);
            _crtTopRight.DragDelta += (sender, e) => HandleDrag(sender, e, 0, 1, 1, -1);
            _crtTop.DragDelta += (sender, e) => HandleDrag(sender, e, 0, 1, 0, -1);
            _crtBottom.DragDelta += (sender, e) => HandleDrag(sender, e, 0, 0, 0, 1);
            _crtRight.DragDelta += (sender, e) => HandleDrag(sender, e, 0, 0, 1, 0);
            _crtLeft.DragDelta += (sender, e) => HandleDrag(sender, e, 1, 0, -1, 0);

            _crtMove.DragDelta += HandleMove;

            // We have to keep the clipping interior within the bounds of the adorned element
            // so we have to track it's size to guarantee that...

            if (adornedElement is FrameworkElement fel)
            {
                fel.SizeChanged += AdornedElement_SizeChanged;
            }
        }
        #endregion

        #region Thumb handlers
        // Generic handler for Cropping
        private void HandleThumb(
            double deltaRatioLeft,
            double deltaRatioTop,
            double deltaRatioWidth,
            double deltaRatioHeight,
            double deltaX,
            double deltaY)
        {
            var rcInterior = _prCropMask.RectInterior;

            if (rcInterior.Width + deltaRatioWidth * deltaX < 0)
            {
                deltaX = -rcInterior.Width / deltaRatioWidth;
            }

            if (rcInterior.Height + deltaRatioHeight * deltaY < 0)
            {
                deltaY = -rcInterior.Height / deltaRatioHeight;
            }

            rcInterior = new Rect(
                rcInterior.Left + deltaRatioLeft * deltaX,
                rcInterior.Top + deltaRatioTop * deltaY,
                rcInterior.Width + deltaRatioWidth * deltaX,
                rcInterior.Height + deltaRatioHeight * deltaY);

            _prCropMask.RectInterior = rcInterior;
            SetThumbs(_prCropMask.RectInterior);
            RaiseEvent(new RoutedEventArgs(CropChangedEvent, this));
        }

        private void HandleMove(object sender, DragDeltaEventArgs args)
        {
            if (AdornedElement is FrameworkElement fel)
            {
                var rcInterior = _prCropMask.RectInterior;

                var left = rcInterior.Left + args.HorizontalChange;
                var top = rcInterior.Top + args.VerticalChange;

                if (left < 0)
                    left = 0;

                if (left + rcInterior.Width > fel.ActualWidth)
                    left = fel.ActualWidth - rcInterior.Width;

                if (top < 0)
                    top = 0;

                if (top + rcInterior.Height > fel.ActualHeight)
                    top = fel.ActualHeight - rcInterior.Height;

                rcInterior = new Rect(left, top, rcInterior.Width, rcInterior.Height);

                _prCropMask.RectInterior = rcInterior;

                SetThumbs(_prCropMask.RectInterior);

                RaiseEvent(new RoutedEventArgs(CropChangedEvent, this));
            }
        }

        private void HandleDrag(object sender, DragDeltaEventArgs args, int l, int T, int w, int h)
        {
            if (sender is CropThumb)
            {
                HandleThumb(l, T, w, h, args.HorizontalChange, args.VerticalChange);
            }
        }
        #endregion

        private void AdornedElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var ratio = e.NewSize.Width / e.PreviousSize.Width;

            var rcInterior = _prCropMask.RectInterior;
            
            double intLeft = rcInterior.Left * ratio,
                intTop = rcInterior.Top * ratio,
                intWidth = rcInterior.Width * ratio,
                intHeight = rcInterior.Height * ratio;
            
            _prCropMask.RectInterior = new Rect(intLeft, intTop, intWidth, intHeight);
        }
        
        #region Arranging/positioning

        private void SetThumbs(Rect rect)
        {
            _crtBottomRight.SetPos(rect.Right, rect.Bottom);
            _crtTopLeft.SetPos(rect.Left, rect.Top);
            _crtTopRight.SetPos(rect.Right, rect.Top);
            _crtBottomLeft.SetPos(rect.Left, rect.Bottom);
            _crtTop.SetPos(rect.Left + rect.Width / 2, rect.Top);
            _crtBottom.SetPos(rect.Left + rect.Width / 2, rect.Bottom);
            _crtLeft.SetPos(rect.Left, rect.Top + rect.Height / 2);
            _crtRight.SetPos(rect.Right, rect.Top + rect.Height / 2);

            _crtMove.Width = rect.Width;
            _crtMove.Height = rect.Height;
            Canvas.SetLeft(_crtMove, rect.Left);
            Canvas.SetTop(_crtMove, rect.Top);

            Canvas.SetLeft(_checkButton, rect.Right - _checkButton.ActualWidth - 15);
            Canvas.SetTop(_checkButton, rect.Bottom - _checkButton.ActualHeight - 10);
        }

        // Arrange the Adorners.
        protected override Size ArrangeOverride(Size finalSize)
        {
            var rcExterior = new Rect(0, 0, AdornedElement.RenderSize.Width, AdornedElement.RenderSize.Height);
            _prCropMask.RectExterior = rcExterior;
            var rcInterior = _prCropMask.RectInterior;
            _prCropMask.Arrange(rcExterior);

            SetThumbs(rcInterior);
            _cnvThumbs.Arrange(rcExterior);

            return finalSize;
        }
        #endregion

        public Rect SelectedRegion => _prCropMask.RectInterior;

        public BitmapSource BpsCrop(BitmapSource bmp)
        {
            var ratio = bmp.PixelWidth / AdornedElement.RenderSize.Width;

            var rcInterior = _prCropMask.RectInterior;

            Point ToPoint(double x, double y)
            {
                return new Point((int)(x * ratio), (int)(y * ratio));
            }

            var pxFromSize = ToPoint(rcInterior.Width, rcInterior.Height);
            
            var pxFromPos = ToPoint(rcInterior.Left, rcInterior.Top);
            var pxWhole = ToPoint(AdornedElement.RenderSize.Width, AdornedElement.RenderSize.Height);

            pxFromSize.X = Math.Max(Math.Min(pxWhole.X - pxFromPos.X, pxFromSize.X), 0);
            pxFromSize.Y = Math.Max(Math.Min(pxWhole.Y - pxFromPos.Y, pxFromSize.Y), 0);

            if (pxFromSize.X == 0 || pxFromSize.Y == 0)
            {
                return null;
            }

            var rcFrom = new Int32Rect(pxFromPos.X, pxFromPos.Y, pxFromSize.X, pxFromSize.Y);

            return new CroppedBitmap(bmp, rcFrom);
        }

        private void BuildCorner(ref CropThumb thumb, Cursor customCursor)
        {
            if (thumb != null)
                return;

            thumb = new CropThumb(CpxThumbWidth)
            {
                Cursor = customCursor
            };

            _cnvThumbs.Children.Add(thumb);
        }

        #region Visual tree overrides
        // Override the VisualChildrenCount and GetVisualChild properties to interface with 
        // the adorner's visual collection.
        protected override int VisualChildrenCount => _vc.Count;

        protected override Visual GetVisualChild(int index) => _vc[index];
        #endregion

        private class CropThumb : Thumb
        {
            private readonly int _width;

            public CropThumb(int width)
            {
                _width = width;
            }
            
            protected override Visual GetVisualChild(int index) => null;

            protected override void OnRender(DrawingContext drawingContext)
            {
                drawingContext.DrawRoundedRectangle(Brushes.White, new Pen(Brushes.Black, 1), new Rect(new Size(_width, _width)), 1, 1);
            }

            public void SetPos(double x, double y)
            {
                Canvas.SetTop(this, y - _width / 2.0);
                Canvas.SetLeft(this, x - _width / 2.0);
            }
        }
    }
}