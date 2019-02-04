using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Captura.Base.Services;
using Captura.Core;
using Captura.Core.Settings;
using Captura.Core.Settings.Models;
using Captura.ImageEditor.Controls;
using Captura.ImageEditor.History;
using Captura.Presentation;
using FirstFloor.ModernUI.Windows.Controls;

namespace Captura.ImageEditor
{
    public partial class ImageEditorWindow
    {
        private readonly ImageEditorSettings _settings;

        public ImageEditorWindow()
        {
            InitializeComponent();

            if (DataContext is ImageEditorViewModel vm)
            {
                vm.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(vm.TransformedBitmap))
                        UpdateInkCanvas();
                };

                vm.InkCanvas = InkCanvas;

                InkCanvas.Strokes.StrokesChanged += (sender, e) =>
                {
                    var item = new StrokeHistory();
                    
                    item.Added.AddRange(e.Added);
                    item.Removed.AddRange(e.Removed);

                    vm.AddInkHistory(item);
                };

                void SelectionMovingOrResizing(object sender, InkCanvasSelectionEditingEventArgs args)
                {
                    vm.AddSelectHistory(new SelectHistory
                    {
                        NewRect = args.NewRectangle,
                        OldRect = args.OldRectangle,
                        Selection = InkCanvas.GetSelectedStrokes()
                    });
                }

                InkCanvas.SelectionMoving += SelectionMovingOrResizing;
                InkCanvas.SelectionResizing += SelectionMovingOrResizing;

                vm.Window = this;
            }

            Image.SizeChanged += (sender, e) => UpdateInkCanvas();

            ModesBox.SelectedIndex = 0;

            _settings = ServiceProvider.Get<Settings>().ImageEditor;

            ColorPicker.SelectedColor = _settings.BrushColor.ToWpfColor();
            SizeBox.Value = _settings.BrushSize;

            InkCanvas.DefaultDrawingAttributes.FitToCurve = true;
        }

        public void Open(string filePath)
        {
            if (DataContext is ImageEditorViewModel vm)
            {
                vm.OpenFile(filePath);
            }
        }

        public void Open(BitmapSource bmp)
        {
            if (DataContext is ImageEditorViewModel vm)
            {
                vm.Open(bmp);
            }
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SizeBox_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (InkCanvas != null && e.NewValue is int i)
            {
                InkCanvas.DefaultDrawingAttributes.Height = InkCanvas.DefaultDrawingAttributes.Width = i;

                _settings.BrushSize = i;
            }
        }

        private void ModesBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModesBox.SelectedValue is ExtendedInkTool tool)
            {
                InkCanvas.SetInkTool(tool);
            }
        }

        private void ColorPicker_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue != null && InkCanvas != null)
            {
                var color = e.NewValue.Value;

                InkCanvas.DefaultDrawingAttributes.Color = color;

                _settings.BrushColor = color.ToDrawingColor();
            }
        }

        private void UpdateInkCanvas()
        {
            if (DataContext is ImageEditorViewModel vm && vm.TransformedBitmap != null && Image.ActualWidth > 0)
            {
                InkCanvas.IsEnabled = true;

                InkCanvas.Width = vm.OriginalBitmap.PixelWidth;
                InkCanvas.Height = vm.OriginalBitmap.PixelHeight;

                var rotate = new RotateTransform(vm.Rotation, vm.OriginalBitmap.PixelWidth / 2.0, vm.OriginalBitmap.PixelHeight / 2.0);

                var tilted = Math.Abs(vm.Rotation / 90) % 2 == 1;
                
                var scale = new ScaleTransform(
                    ((tilted ? Image.ActualHeight : Image.ActualWidth) / InkCanvas.Width) * (vm.FlipX ? -1 : 1),
                    ((tilted ? Image.ActualWidth : Image.ActualHeight) / InkCanvas.Height) * (vm.FlipY ? -1 : 1)
                );

                InkCanvas.LayoutTransform = new TransformGroup
                {
                    Children =
                    {
                        rotate,
                        scale
                    }
                };
            }
        }

        private void InkCanvas_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ImageEditorViewModel vm)
            {
                vm.IncrementEditingOperationCount();
            }
        }

        private void ImageEditorWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (DataContext is ImageEditorViewModel vm)
            {
                if (vm.UnsavedChanges)
                {
                    var result = ModernDialog.ShowMessage("Do you want to save your changes before exiting?",
                        "Unsaved Changes",
                        MessageBoxButton.YesNoCancel,
                        this);

                    switch (result)
                    {
                        case MessageBoxResult.Yes when !vm.SaveToFile():
                        case MessageBoxResult.Cancel:
                            e.Cancel = true;
                            break;
                    }
                }
            }
        }

        private void NewWindow(object sender, RoutedEventArgs e)
        {
            new ImageEditorWindow().ShowAndFocus();
        }

        // Return false to cancel
        private bool ConfirmSaveBeforeNew(ImageEditorViewModel viewModel)
        {
            if (viewModel.UnsavedChanges)
            {
                var result = ModernDialog.ShowMessage("Do you want to save your changes?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNoCancel,
                    this);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        viewModel.SaveCommand.ExecuteIfCan();
                        break;

                    case MessageBoxResult.Cancel:
                        return false;
                }
            }

            return true;
        }

        private void NewBlank(object sender, ExecutedRoutedEventArgs e)
        {
            if (DataContext is ImageEditorViewModel vm)
            {
                if (!ConfirmSaveBeforeNew(vm))
                    return;

                vm.NewBlank();
            }
        }

        private void Open(object sender, ExecutedRoutedEventArgs e)
        {
            if (DataContext is ImageEditorViewModel vm)
            {
                if (!ConfirmSaveBeforeNew(vm))
                    return;

                vm.Open();
            }
        }

        private void OpenFromClipboard(object sender, RoutedEventArgs e)
        {
            if (DataContext is ImageEditorViewModel vm)
            {
                if (!ConfirmSaveBeforeNew(vm))
                    return;

                vm.OpenFromClipboard();
            }
        }
    }
}
