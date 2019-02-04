using System;
using Captura.Base.Images;

namespace Captura.Base.Services
{
    public interface IPreviewWindow : IDisposable
    {
        void Init(int width, int height);

        void Display(IBitmapFrame frame);

        void Show();
    }
}