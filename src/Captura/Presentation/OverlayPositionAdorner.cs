using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Captura.Models;

namespace Captura.Presentation
{
    public class OverlayPositionAdorner : Adorner
    {
        #region Thumbs

        private readonly Thumb _topLeft;
        private readonly Thumb _topRight;
        private readonly Thumb _bottomLeft;
        private readonly Thumb _bottomRight;

        private readonly Thumb _top;
        private readonly Thumb _left;
        private readonly Thumb _right;
        private readonly Thumb _bottom;

        private readonly Thumb _center;
        #endregion

        private readonly bool _canResize;

        private readonly VisualCollection _visualChildren;

        public OverlayPositionAdorner(UIElement element, bool canResize = true) : base(element)
        {
            _canResize = canResize;

            _visualChildren = new VisualCollection(this);

            BuildAdornerThumb(ref _center, Cursors.Hand);
            _center.Opacity = 0;

            _center.DragDelta += (sender, e) => HandleDrag(HitType.Body, e);

            if (canResize)
            {
                BuildAdornerThumb(ref _topLeft, Cursors.SizeNWSE);
                BuildAdornerThumb(ref _topRight, Cursors.SizeNESW);
                BuildAdornerThumb(ref _bottomLeft, Cursors.SizeNESW);
                BuildAdornerThumb(ref _bottomRight, Cursors.SizeNWSE);

                BuildAdornerThumb(ref _top, Cursors.SizeNS);
                BuildAdornerThumb(ref _left, Cursors.SizeWE);
                BuildAdornerThumb(ref _right, Cursors.SizeWE);
                BuildAdornerThumb(ref _bottom, Cursors.SizeNS);

                _topLeft.DragDelta += (sender, e) => HandleDrag(HitType.UpperLeft, e);
                _topRight.DragDelta += (sender, e) => HandleDrag(HitType.UpperRight, e);
                _bottomLeft.DragDelta += (sender, e) => HandleDrag(HitType.LowerLeft, e);
                _bottomRight.DragDelta += (sender, e) => HandleDrag(HitType.LowerRight, e);

                _top.DragDelta += (sender, e) => HandleDrag(HitType.Top, e);
                _left.DragDelta += (sender, e) => HandleDrag(HitType.Left, e);
                _right.DragDelta += (sender, e) => HandleDrag(HitType.Right, e);
                _bottom.DragDelta += (sender, e) => HandleDrag(HitType.Bottom, e);
            }

            Opacity = 0.01;

            MouseEnter += (sender, e) => Opacity = 1;
            MouseLeave += (sender, e) => Opacity = 0.01;
        }

        private void HandleDrag(HitType mouseHitType, DragDeltaEventArgs args)
        {
            if (!(AdornedElement is FrameworkElement fel))
                return;

            var offsetX = (int) args.HorizontalChange;
            var offsetY = (int) args.VerticalChange;

            var har = fel.HorizontalAlignment == HorizontalAlignment.Right;
            var vab = fel.VerticalAlignment == VerticalAlignment.Bottom;

            var newX = (int)(har ? fel.Margin.Right : fel.Margin.Left);
            var newY = (int)(vab ? fel.Margin.Bottom : fel.Margin.Top);
            var newWidth = (int) fel.ActualWidth;
            var newHeight = (int) fel.ActualHeight;

            void ModifyX(bool positive)
            {
                if (positive)
                {
                    newX += offsetX;
                }
                else
                {
                    newX -= offsetX;
                }
            }

            void ModifyY(bool positive)
            {
                if (positive)
                {
                    newY += offsetY;
                }
                else
                {
                    newY -= offsetY;
                }
            }

            void ModifyWidth(bool positive)
            {
                if (positive)
                {
                    newWidth += offsetX;
                }
                else
                {
                    newWidth -= offsetX;
                }
            }

            void ModifyHeight(bool positive)
            {
                if (positive)
                {
                    newHeight += offsetY;
                }
                else
                {
                    newHeight -= offsetY;
                }
            }

            switch (mouseHitType)
            {
                case HitType.Body:
                    ModifyX(!har);
                    ModifyY(!vab);
                    break;

                case HitType.UpperLeft:
                    if (har)
                    {
                        ModifyWidth(false);
                    }
                    else
                    {
                        ModifyX(true);
                        ModifyWidth(false);
                    }

                    if (vab)
                    {
                        ModifyHeight(false);
                    }
                    else
                    {
                        ModifyY(true);
                        ModifyHeight(false);
                    }
                    break;

                case HitType.UpperRight:
                    if (har)
                    {
                        ModifyX(false);
                        ModifyWidth(true);
                    }
                    else ModifyWidth(true);

                    if (vab)
                    {
                        ModifyHeight(false);
                    }
                    else
                    {
                        ModifyY(true);
                        ModifyHeight(false);
                    }
                    break;

                case HitType.LowerRight:
                    if (har)
                    {
                        ModifyX(false);
                        ModifyWidth(true);
                    }
                    else ModifyWidth(true);

                    if (vab)
                    {
                        ModifyY(false);
                        ModifyHeight(true);
                    }
                    else ModifyHeight(true);
                    break;

                case HitType.LowerLeft:
                    if (har)
                    {
                        ModifyWidth(false);
                    }
                    else
                    {
                        ModifyX(true);
                        ModifyWidth(false);
                    }

                    if (vab)
                    {
                        ModifyY(false);
                        ModifyHeight(true);
                    }
                    else ModifyHeight(true);
                    break;

                case HitType.Left:
                    if (har)
                    {
                        ModifyWidth(false);
                    }
                    else
                    {
                        ModifyX(true);
                        ModifyWidth(false);
                    }
                    break;

                case HitType.Right:
                    if (har)
                    {
                        ModifyX(false);
                        ModifyWidth(true);
                    }
                    else ModifyWidth(true);
                    break;

                case HitType.Bottom:
                    if (vab)
                    {
                        ModifyY(false);
                        ModifyHeight(true);
                    }
                    else ModifyHeight(true);
                    break;

                case HitType.Top:
                    if (vab)
                    {
                        ModifyHeight(false);
                    }
                    else
                    {
                        ModifyY(true);
                        ModifyHeight(false);
                    }
                    break;
            }

            if (newWidth > 0 && newHeight > 0)
            {
                if (newX < 0)
                {
                    newX = 0;
                }

                if (newY < 0)
                {
                    newY = 0;
                }

                double left = 0, top = 0, right = 0, bottom = 0;

                if (har)
                    right = newX;
                else left = newX;

                if (vab)
                    bottom = newY;
                else top = newY;

                fel.Margin = new Thickness(left, top, right, bottom);

                PositionUpdated?.Invoke(new Rect(newX, newY, newWidth, newHeight));

                if (mouseHitType != HitType.Body)
                {
                    fel.Width = newWidth;
                    fel.Height = newHeight;
                }
            }
        }

        public event Action<Rect> PositionUpdated;

        private void BuildAdornerThumb(ref Thumb cornerThumb, Cursor customizedCursors)
        {
            if (cornerThumb != null)
                return;

            cornerThumb = new Thumb
            {
                Cursor = customizedCursors,
                Height = 10,
                Width = 10,
                Opacity = 0.5,
                Background = new SolidColorBrush(Colors.Red)
            };

            _visualChildren.Add(cornerThumb);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            base.ArrangeOverride(finalSize);

            var desireWidth = AdornedElement.RenderSize.Width;
            var desireHeight = AdornedElement.RenderSize.Height;

            var adornerWidth = DesiredSize.Width;
            var adornerHeight = DesiredSize.Height;

            _center.Height = desireHeight;
            _center.Width = desireWidth;
            _center.Arrange(new Rect(0, 0, desireWidth, desireHeight));

            if (_canResize)
            {
                _topLeft.Arrange(new Rect(-adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
                _topRight.Arrange(new Rect(desireWidth - adornerWidth / 2, -adornerHeight / 2, adornerWidth,
                    adornerHeight));
                _bottomLeft.Arrange(new Rect(-adornerWidth / 2, desireHeight - adornerHeight / 2, adornerWidth,
                    adornerHeight));
                _bottomRight.Arrange(new Rect(desireWidth - adornerWidth / 2, desireHeight - adornerHeight / 2,
                    adornerWidth, adornerHeight));

                _top.Arrange(new Rect(desireWidth / 2 - adornerWidth / 2, -adornerHeight / 2, adornerWidth,
                    adornerHeight));
                _left.Arrange(new Rect(-adornerWidth / 2, desireHeight / 2 - adornerHeight / 2, adornerWidth,
                    adornerHeight));
                _right.Arrange(new Rect(desireWidth - adornerWidth / 2, desireHeight / 2 - adornerHeight / 2,
                    adornerWidth, adornerHeight));
                _bottom.Arrange(new Rect(desireWidth / 2 - adornerWidth / 2, desireHeight - adornerHeight / 2,
                    adornerWidth, adornerHeight));
            }

            return finalSize;
        }

        protected override int VisualChildrenCount => _visualChildren.Count;

        protected override Visual GetVisualChild(int index) => _visualChildren[index];
    }
}