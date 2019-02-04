using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Captura.Base.Images;
using Captura.Base.Services;
using Screna.Frames;

namespace Screna.Services
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ClipboardService : IClipboardService
    {
        public void SetText(string text)
        {
            if (text == null)
                return;

            try { Clipboard.SetText(text); }
            catch (ExternalException)
            {
                ServiceProvider.MessageProvider?.ShowError($"Copy to Clipboard failed:\n\n{text}");
            }
        }

        public string GetText() => Clipboard.GetText();

        public bool HasText => Clipboard.ContainsText();

        public void SetImage(IBitmapImage bitmapImage)
        {
            using (var pngStream = new MemoryStream())
            {
                bitmapImage.Save(pngStream, ImageFormats.Png);
                var pngClipboardData = new DataObject("PNG", pngStream);

                using (var whiteS = new Bitmap(bitmapImage.Width, bitmapImage.Height, PixelFormat.Format24bppRgb))
                {
                    Image drawingImg;

                    if (bitmapImage is DrawingImage drawingImage)
                        drawingImg = drawingImage.Image;
                    else drawingImg = Image.FromStream(pngStream);

                    using (var graphics = Graphics.FromImage(whiteS))
                    {
                        graphics.Clear(Color.White);
                        graphics.DrawImage(drawingImg, 0, 0, bitmapImage.Width, bitmapImage.Height);
                    }

                    // Add fallback for applications that don't support PNG from clipboard (eg. PhotoShop or Paint)
                    pngClipboardData.SetData(DataFormats.Bitmap, whiteS);

                    Clipboard.Clear();
                    Clipboard.SetDataObject(pngClipboardData, true);
                }
            }
        }

        public IBitmapImage GetImage() => new DrawingImage(Clipboard.GetImage());

        public bool HasImage => Clipboard.ContainsImage();
    }
}