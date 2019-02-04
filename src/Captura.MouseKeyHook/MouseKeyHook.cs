using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Captura.Base;
using Captura.Base.Images;
using Captura.Base.Settings;
using Captura.MouseKeyHook.KeyRecord;
using Captura.MouseKeyHook.Models;
using Gma.System.MouseKeyHook;
using Screna;

namespace Captura.MouseKeyHook
{
    /// <summary>
    /// Draws Mouse Clicks and/or Keystrokes on an Image.
    /// </summary>
    public class MouseKeyHook : IOverlay
    {
        #region Fields
        readonly IKeyboardMouseEvents _hook;
        readonly MouseClickSettings _mouseClickSettings;
        readonly KeystrokesSettings _keystrokesSettings;

        bool _mouseClicked;
        MouseButtons _mouseButtons;
        
        readonly KeyRecords _records;

        readonly KeymapViewModel _keymap;

        readonly TextWriter _textWriter;
        #endregion
        
        /// <summary>
        /// Creates a new instance of <see cref="MouseKeyHook"/>.
        /// </summary>
        public MouseKeyHook(MouseClickSettings mouseClickSettings,
            KeystrokesSettings keystrokesSettings,
            KeymapViewModel keymap,
            string fileName,
            Func<TimeSpan> elapsed)
        {
            _mouseClickSettings = mouseClickSettings;
            _keystrokesSettings = keystrokesSettings;
            _keymap = keymap;

            _hook = Hook.GlobalEvents();
            
            _hook.MouseDown += (sender, e) =>
            {
                _mouseClicked = true;

                _mouseButtons = e.Button;
            };

            _hook.MouseUp += (sender, e) => _mouseClicked = false;

            if (keystrokesSettings.SeparateTextFile)
            {
                _textWriter = InitKeysToTextFile(fileName, elapsed);
            }
            else
            {
                _records = new KeyRecords(keystrokesSettings.HistoryCount);

                _hook.KeyDown += OnKeyDown;
                _hook.KeyUp += OnKeyUp;
            }
        }

        TextWriter InitKeysToTextFile(string fileName, Func<TimeSpan> elapsed)
        {
            var dir = Path.GetDirectoryName(fileName);
            var fileNameWoExt = Path.GetFileNameWithoutExtension(fileName);

            var targetName = $"{fileNameWoExt}.keys.txt";

            var path = dir == null ? targetName : Path.Combine(dir, targetName);

            var keystrokeFileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            var textWriter = new StreamWriter(keystrokeFileStream);

            _hook.KeyDown += (sender, e) =>
            {
                if (!_keystrokesSettings.Display)
                {
                    return;
                }

                var record = new KeyRecord.KeyRecord(e, _keymap);

                _textWriter.WriteLine($"{elapsed.Invoke()}: {record.Display}");
            };

            return textWriter;
        }

        void OnKeyUp(object sender, KeyEventArgs args)
        {
            if (!_keystrokesSettings.Display)
            {
                _records.Clear();

                return;
            }

            var record = new KeyRecord.KeyRecord(args, _keymap);

            var display = record.Display;

            if (display == _keymap.Control
                || display == _keymap.Alt
                || display == _keymap.Shift)
            {
                if (_records.Last?.Display == display)
                {
                    _records.Last = new RepeatKeyRecord(record);
                }
                else if (_records.Last is RepeatKeyRecord repeat && repeat.Repeated.Display == display)
                {
                    repeat.Increment();
                }
                else if (_modifierSingleDown)
                {
                    _records.Add(record);
                }
            }
        }

        bool _modifierSingleDown;

        void OnKeyDown(object sender, KeyEventArgs args)
        {
            if (!_keystrokesSettings.Display)
            {
                _records.Clear();

                return;
            }

            _modifierSingleDown = false;

            var record = new KeyRecord.KeyRecord(args, _keymap);
            
            if (_records.Last == null)
            {
                _records.Add(record);

                return;
            }

            var elapsed = (record.TimeStamp - _records.Last.TimeStamp).TotalSeconds;

            var display = record.Display;
            var lastDisplay = _records.Last.Display;

            if (display.Length == 1
                && (_records.Last is DummyKeyRecord || lastDisplay.Length == 1)
                && display.Length + lastDisplay.Length <= _keystrokesSettings.MaxTextLength
                && elapsed <= _keystrokesSettings.Timeout)
            {
                _records.Last = new DummyKeyRecord(lastDisplay + display);
            }
            else if (display == _keymap.Control
                || display == _keymap.Alt
                || display == _keymap.Shift)
            {
                // Handled on Key Up
                _modifierSingleDown = true;
            }
            else if (_records.Last is KeyRecord.KeyRecord keyRecord && keyRecord.Display == display)
            {
                _records.Last = new RepeatKeyRecord(record);
            }
            else if (_records.Last is RepeatKeyRecord repeatRecord && repeatRecord.Repeated.Display == display)
            {
                repeatRecord.Increment();
            }
            else
            {
                _records.Add(record);
            }
        }

        static float GetLeft(TextOverlaySettings keystrokesSettings, float fullWidth, float textWidth)
        {
            var x = keystrokesSettings.X;
            var padding = keystrokesSettings.HorizontalPadding;

            switch (keystrokesSettings.HorizontalAlignment)
            {
                case Alignment.Start:
                    return x;

                case Alignment.End:
                    return fullWidth - x - textWidth - 2 * padding;

                case Alignment.Center:
                    return fullWidth / 2 + x - textWidth / 2 - padding;

                default:
                    return 0;
            }
        }

        static float GetTop(TextOverlaySettings keystrokesSettings, float fullHeight, float textHeight, float offset = 0)
        {
            var y = keystrokesSettings.Y;
            var padding = keystrokesSettings.VerticalPadding;

            switch (keystrokesSettings.VerticalAlignment)
            {
                case Alignment.Start:
                    return y + offset;

                case Alignment.End:
                    return fullHeight - y - textHeight - 2 * padding - offset;

                case Alignment.Center:
                    return fullHeight / 2 + y - textHeight / 2 - padding + offset;

                default:
                    return 0;
            }
        }
        
        /// <summary>
        /// Draws overlay.
        /// </summary>
        public void Draw(IEditableFrame editor, Func<Point, Point> transform = null)
        {
            if (_mouseClickSettings.Display)
                DrawClicks(editor, transform);

            if (_keystrokesSettings.Display)
                DrawKeys(editor);
        }

        void DrawKeys(IEditableFrame editor)
        {
            if (_records?.Last == null)
                return;

            var offsetY = 0f;
            var fontSize = _keystrokesSettings.FontSize;
            byte opacity = 255;

            var index = 0;

            foreach (var keyRecord in _records)
            {
                ++index;

                if ((DateTime.Now - keyRecord.TimeStamp).TotalSeconds > _records.Size * _keystrokesSettings.Timeout)
                    continue;
                
                DrawKeys(_keystrokesSettings, editor, keyRecord.Display, Math.Max(1, fontSize), opacity, offsetY);

                var height = editor.MeasureString("A", fontSize).Height;

                offsetY += height + _keystrokesSettings.HistorySpacing;

                offsetY += _keystrokesSettings.VerticalPadding * 2 + _keystrokesSettings.BorderThickness * 2;

                if (index == 1)
                {
                    fontSize -= 5;
                    opacity = 200;
                }
            }
        }

        static void DrawKeys(KeystrokesSettings keystrokesSettings, IEditableFrame editor, string text, int fontSize, byte opacity, float offsetY)
        {
            var size = editor.MeasureString(text, fontSize);

            int paddingX = keystrokesSettings.HorizontalPadding, paddingY = keystrokesSettings.VerticalPadding;

            var rect = new RectangleF(GetLeft(keystrokesSettings, editor.Width, size.Width),
                GetTop(keystrokesSettings, editor.Height, size.Height, offsetY),
                size.Width + 2 * paddingX,
                size.Height + 2 * paddingY);
            
            editor.FillRectangle(Color.FromArgb(opacity, keystrokesSettings.BackgroundColor),
                rect,
                keystrokesSettings.CornerRadius);
            
            editor.DrawString(text,
                fontSize,
                Color.FromArgb(opacity, keystrokesSettings.FontColor),
                new RectangleF(rect.Left + paddingX, rect.Top + paddingY, size.Width, size.Height));

            var border = keystrokesSettings.BorderThickness;

            if (border > 0)
            {
                rect = new RectangleF(rect.Left - border / 2f, rect.Top - border / 2f, rect.Width + border, rect.Height + border);

                editor.DrawRectangle(Color.FromArgb(opacity, keystrokesSettings.BorderColor), border,
                    rect,
                    keystrokesSettings.CornerRadius);
            }
        }

        Color GetClickCircleColor()
        {
            if (_mouseButtons.HasFlag(MouseButtons.Right))
            {
                return _mouseClickSettings.RightClickColor;
            }

            if (_mouseButtons.HasFlag(MouseButtons.Middle))
            {
                return _mouseClickSettings.MiddleClickColor;
            }

            return _mouseClickSettings.Color;
        }

        float _currentMouseRatio;
        const float MouseRatioDeltaUp = 0.9f;
        const float MouseRatioDeltaDown = 0.25f;
        const float MouseRatioMin = 0.6f;
        const float MouseRatioMax = 1.2f;

        static byte ToByte(double value)
        {
            if (value > 255)
                return 255;

            if (value < 0)
                return 0;

            return (byte) value;
        }

        void DrawClicks(IEditableFrame editor, Func<Point, Point> transform)
        {
            if (_mouseClicked && _currentMouseRatio < MouseRatioMax)
            {
                _currentMouseRatio += MouseRatioDeltaUp;

                if (_currentMouseRatio > MouseRatioMax)
                {
                    _currentMouseRatio = MouseRatioMax;
                }
            }
            else if (!_mouseClicked && _currentMouseRatio > MouseRatioMin)
            {
                _currentMouseRatio -= MouseRatioDeltaDown;

                if (_currentMouseRatio < MouseRatioMin)
                {
                    _currentMouseRatio = MouseRatioMin;
                }
            }

            if (_currentMouseRatio > MouseRatioMin)
            {
                var clickRadius = _mouseClickSettings.Radius * _currentMouseRatio;

                var curPos = MouseCursor.CursorPosition;

                if (transform != null)
                    curPos = transform(curPos);

                var d = clickRadius * 2;
                
                var x = curPos.X - clickRadius;
                var y = curPos.Y - clickRadius;

                var color = GetClickCircleColor();

                color = Color.FromArgb(ToByte(color.A * _currentMouseRatio), color);

                editor.FillEllipse(color, new RectangleF(x, y, d, d));

                var border = _mouseClickSettings.BorderThickness * _currentMouseRatio;

                if (border > 0)
                {
                    x -= border / 2f;
                    y -= border / 2f;
                    d += border;

                    var borderColor = _mouseClickSettings.BorderColor;

                    borderColor = Color.FromArgb(ToByte(borderColor.A * _currentMouseRatio), borderColor);

                    editor.DrawEllipse(borderColor, border, new RectangleF(x, y, d, d));
                }
            }
        }

        /// <summary>
        /// Frees all resources used by this object.
        /// </summary>
        public void Dispose()
        {
            _hook?.Dispose();

            _textWriter?.Dispose();
        }
    }
}
