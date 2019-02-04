using System;
using System.Windows;
using System.Windows.Interop;
using Captura.Base.Images;
using Captura.Base.Services;
using Captura.Presentation;
using Captura.Windows;
using DesktopDuplication;
using Screna.Frames;
using SharpDX.Direct3D9;

namespace Captura.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PreviewWindowService : IPreviewWindow
    {
        private readonly PreviewWindow _previewWindow = new PreviewWindow();

        private D3D9PreviewAssister _d3D9PreviewAssister;
        private IntPtr _backBufferPtr;
        private Texture _texture;

        private bool _visible;

        public PreviewWindowService()
        {
            _previewWindow.IsVisibleChanged += (sender, e) => _visible = _previewWindow.IsVisible;

            _visible = _previewWindow.IsVisible;

            // Prevent Closing by User
            _previewWindow.Closing += (sender, e) =>
            {
                e.Cancel = true;

                _previewWindow.Hide();
            };
        }

        public void Init(int width, int height) { }

        private IBitmapFrame _lastFrame;

        public void Display(IBitmapFrame frame)
        {
            if (frame is RepeatFrame)
                return;

            if (!_visible)
            {
                frame.Dispose();
                return;
            }

            _previewWindow.Dispatcher.Invoke(() =>
            {
                _previewWindow.DisplayImage.Image = null;

                _lastFrame?.Dispose();
                _lastFrame = frame;

                if (frame is MultiDisposeFrame frameWrapper)
                {
                    switch (frameWrapper.Frame)
                    {
                        case DrawingFrameBase drawingFrame:
                            _previewWindow.WinFormsHost.Visibility = Visibility.Visible;
                            _previewWindow.DisplayImage.Image = drawingFrame.Bitmap;
                            break;

                        case Texture2DFrame texture2DFrame:
                            _previewWindow.WinFormsHost.Visibility = Visibility.Collapsed;
                            if (_d3D9PreviewAssister == null)
                            {
                                _d3D9PreviewAssister = new D3D9PreviewAssister(ServiceProvider.Get<IPlatformServices>());
                                _texture = _d3D9PreviewAssister.GetSharedTexture(texture2DFrame.PreviewTexture);

                                using (var surface = _texture.GetSurfaceLevel(0))
                                {
                                    _backBufferPtr = surface.NativePointer;
                                }
                            }

                            Invalidate(_backBufferPtr, texture2DFrame.Width, texture2DFrame.Height);
                            break;
                    }
                }
            });
        }

        private void Invalidate(IntPtr backBufferPtr, int width, int height)
        {
            _previewWindow.D3DImage.Lock();
            _previewWindow.D3DImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, backBufferPtr);

            if (backBufferPtr != IntPtr.Zero)
                _previewWindow.D3DImage.AddDirtyRect(new Int32Rect(0, 0, width, height));

            _previewWindow.D3DImage.Unlock();
        }

        public void Dispose()
        {
            _previewWindow.Dispatcher.Invoke(() =>
            {
                _previewWindow.DisplayImage.Image = null;

                _lastFrame?.Dispose();
                _lastFrame = null;

                if (_d3D9PreviewAssister != null)
                {
                    Invalidate(IntPtr.Zero, 0, 0);

                    _texture.Dispose();

                    _d3D9PreviewAssister.Dispose();

                    _d3D9PreviewAssister = null;
                }
            });
        }

        public void Show()
        {
            _previewWindow.Dispatcher.Invoke(() => _previewWindow.ShowAndFocus());
        }
    }
}