using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Captura.Models;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Captura.Controls
{
    public partial class NotificationStack
    {
        private static readonly TimeSpan TimeoutToHide = TimeSpan.FromSeconds(5);
        private DateTime _lastMouseMoveTime;
        private readonly DispatcherTimer _timer;

        public NotificationStack()
        {
            InitializeComponent();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += TimerOnTick;

            _timer.Start();
        }

        private void TimerOnTick(object sender, EventArgs args)
        {
            var now = DateTime.Now;
            var elapsed = now - _lastMouseMoveTime;

            var unfinished = ItemsControl.Items
                .OfType<NotificationBalloon>()
                .Any(notificationBalloon => !notificationBalloon.Notification.Finished);

            if (unfinished)
            {
                _lastMouseMoveTime = now;
            }

            if (elapsed < TimeoutToHide)
                return;

            if (!unfinished)
            {
                OnClose();
            }
        }

        public void Hide()
        {
            BeginAnimation(OpacityProperty, new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(100))));

            if (_timer.IsEnabled)
                _timer.Stop();
        }

        public void Show()
        {
            _lastMouseMoveTime = DateTime.Now;

            BeginAnimation(OpacityProperty, new DoubleAnimation(1, new Duration(TimeSpan.FromMilliseconds(300))));

            if (!_timer.IsEnabled)
                _timer.Start();
        }

        private void OnClose()
        {
            Hide();

            var copy = ItemsControl.Items.OfType<FrameworkElement>().ToArray();

            foreach (var frameworkElement in copy)
            {
                Remove(frameworkElement);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => OnClose();

        /// <summary>
        /// Slides out element while decreasing opacity, then decreases height, then removes.
        /// </summary>
        private void Remove(FrameworkElement element)
        {
            var transform = new TranslateTransform();
            element.RenderTransform = transform;

            var translateAnim = new DoubleAnimation(500, new Duration(TimeSpan.FromMilliseconds(200)));
            var opactityAnim = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(200)));

            var heightAnim = new DoubleAnimation(element.ActualHeight, 0, new Duration(TimeSpan.FromMilliseconds(200)));

            heightAnim.Completed += (sender, e) =>
            {
                ItemsControl.Items.Remove(element);

                if (ItemsControl.Items.Count == 0)
                {
                    Hide();
                }
            };

            opactityAnim.Completed += (sender, e) => element.BeginAnimation(HeightProperty, heightAnim);

            transform.BeginAnimation(TranslateTransform.XProperty, translateAnim);
            element.BeginAnimation(OpacityProperty, opactityAnim);
        }

        private const int MaxItems = 5;

        public void Add(FrameworkElement element)
        {
            if (element is IRemoveRequester removeRequester)
            {
                removeRequester.RemoveRequested += () => Remove(element);
            }

            if (element is ScreenShotBalloon ssBalloon)
                ssBalloon.Expander.IsExpanded = true;

            foreach (var item in ItemsControl.Items)
            {
                if (item is ScreenShotBalloon screenShotBalloon)
                {
                    screenShotBalloon.Expander.IsExpanded = false;
                }
            }

            ItemsControl.Items.Insert(0, element);

            if (ItemsControl.Items.Count > MaxItems)
            {
                var itemsToRemove = ItemsControl.Items
                    .OfType<FrameworkElement>()
                    .Skip(MaxItems)
                    .ToArray();

                foreach (var frameworkElement in itemsToRemove)
                {
                    if (frameworkElement is NotificationBalloon progressBalloon && !progressBalloon.Notification.Finished)
                        continue;

                    Remove(frameworkElement);
                }
            }
        }

        private void NotificationStack_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (ItemsControl.Items.Count == 0)
                return;

            _lastMouseMoveTime = DateTime.Now;

            Show();
        }
    }
}