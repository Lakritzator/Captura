﻿using System;
using System.Drawing;
using Captura.Base;
using Captura.Models;

// ReSharper disable MemberCanBePrivate.Global

namespace Captura.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class RegionSelectorViewModel : NotifyPropertyChanged
    {
        private int _left = 50,
            _top = 50,
            _width = 500,
            _height = 500;

        private const int MinWidth = 300,
            MinHeight = 300,
            BorderSize = 3;

        public int Left
        {
            get => _left;
            set
            {
                _left = value;

                OnPropertyChanged();
                RaisePropertyChanged(nameof(LeftDip));
            }
        }

        public double LeftDip
        {
            get => Left / Dpi.X - BorderSize;
            set => Left = (int)Math.Round((value + BorderSize) * Dpi.X);
        }

        public int Top
        {
            get => _top;
            set
            {
                _top = value;

                OnPropertyChanged();
                RaisePropertyChanged(nameof(TopDip));
            }
        }

        public double TopDip
        {
            get => Top / Dpi.Y - BorderSize;
            set => Top = (int)Math.Round((value + BorderSize) * Dpi.Y);
        }

        public int Width
        {
            get => _width;
            set
            {
                _width = Math.Max(value, MinWidth);

                OnPropertyChanged();
                RaisePropertyChanged(nameof(WidthDip));
            }
        }

        public double WidthDip
        {
            get => Width / Dpi.X + 2 * BorderSize;
            set => Width = (int)Math.Round((value - 2 * BorderSize) * Dpi.X);
        }

        public int Height
        {
            get => _height;
            set
            {
                _height = Math.Max(value, MinHeight);

                OnPropertyChanged();
                RaisePropertyChanged(nameof(HeightDip));
            }
        }

        public double HeightDip
        {
            get => Height / Dpi.Y + 2 * BorderSize;
            set => Height = (int)Math.Round((value - 2 * BorderSize) * Dpi.Y);
        }

        public Rectangle SelectedRegion
        {
            get => new Rectangle(Left, Top, Width, Height);
            set
            {
                Left = value.Left;
                Top = value.Top;
                Width = value.Width;
                Height = value.Height;
            }
        }

        public void ResizeFromTop(double verticalChangeDip)
        {
            var verticalChange = (int) (verticalChangeDip * Dpi.Y);

            var oldBottom = Top + Height;
            var top = Top + verticalChange;
            
            if (top <= 0)
            {
                Top = 0;
                Height = oldBottom;
                return;
            }

            var height = Height - verticalChange;

            if (height > MinHeight)
            {
                Top = top;
                Height = height;
            }
            else
            {
                Height = MinHeight;
                Top = oldBottom - MinHeight;
            }
        }

        public void ResizeFromLeft(double horizontalChangeDip)
        {
            var horizontalChange = (int) (horizontalChangeDip * Dpi.X);

            var oldRight = Left + Width;
            var left = Left + horizontalChange;

            if (left <= 0)
            {
                Left = 0;
                Width = oldRight;
                return;
            }

            var width = Width - horizontalChange;

            if (width > MinWidth)
            {
                Left = left;
                Width = width;
            }
            else
            {
                Width = MinWidth;
                Left = oldRight - MinWidth;
            }
        }
    }
}