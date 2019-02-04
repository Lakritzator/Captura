using Captura.Base.Images;

namespace Captura.Base.Services
{
    public interface IClipboardService
    {
        void SetText(string text);

        string GetText();

        bool HasText { get; }

        void SetImage(IBitmapImage bitmapImage);

        IBitmapImage GetImage();

        bool HasImage { get; }
    }
}