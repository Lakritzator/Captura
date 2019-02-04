using System.Drawing;
using System.Drawing.Imaging;
using Captura.Base.Services;
using Captura.Native;
using Captura.Native.Structs;

namespace Screna
{
    /// <summary>
    /// Collection of utility methods.
    /// </summary>
    public static class Extensions
    {
        public static Rectangle Even(this Rectangle rect)
        {
            if (rect.Width % 2 == 1)
                --rect.Width;

            if (rect.Height % 2 == 1)
                --rect.Height;

            return rect;
        }

        public static void WriteToClipboard(this string text)
        {
            if (text == null)
            {
                return;
            }

            var clipboard = ServiceProvider.Get<IClipboardService>();

            clipboard.SetText(text);
        }

        /// <summary>
        /// Removes the Pixels on Edges matching TrimColor(default is Transparent) from the Image
        /// </summary>
        internal static unsafe Bitmap CropEmptyEdges(this Bitmap image, Color trimColor = default(Color))
        {
            if (image == null)
                return null;

            int sizeX = image.Width,
                sizeY = image.Height;

            var r = new RECT(-1);

            using (var b = new UnsafeBitmap(image))
            {
                for (int x = 0, y = 0; ;)
                {
                    var pixel = b[x, y];

                    bool Condition()
                    {
                        return trimColor.A == 0
                               && pixel->Alpha != 0
                               ||
                               trimColor.R != pixel->Red
                               && trimColor.G != pixel->Green
                               && trimColor.B != pixel->Blue;
                    }

                    if (r.Left == -1)
                    {
                        if (Condition())
                        {
                            r.Left = x;

                            x = 0;
                            y = 0;

                            continue;
                        }

                        if (y == sizeY - 1)
                        {
                            x++;
                            y = 0;
                        }
                        else y++;

                        continue;
                    }

                    if (r.Top == -1)
                    {
                        if (Condition())
                        {
                            r.Top = y;

                            x = sizeX - 1;
                            y = 0;

                            continue;
                        }

                        if (x == sizeX - 1)
                        {
                            y++;
                            x = 0;
                        }
                        else x++;

                        continue;
                    }

                    if (r.Right == -1)
                    {
                        if (Condition())
                        {
                            r.Right = x + 1;

                            x = 0;
                            y = sizeY - 1;

                            continue;
                        }

                        if (y == sizeY - 1)
                        {
                            x--;
                            y = 0;
                        }
                        else y++;

                        continue;
                    }

                    if (r.Bottom != -1)
                        continue;

                    if (Condition())
                    {
                        r.Bottom = y + 1;
                        break;
                    }

                    if (x == sizeX - 1)
                    {
                        y--;
                        x = 0;
                    }
                    else x++;
                }
            }

            if (r.Left >= r.Right || r.Top >= r.Bottom)
                return null;

            var final = image.Clone(r.ToRectangle(), image.PixelFormat);

            image.Dispose();

            return final;
        }

        /// <summary>
        /// Creates a Transparent Bitmap from a combination of a Bitmap on a White Background and another on a Black Background
        /// </summary>
        internal static unsafe Bitmap DifferentiateAlpha(Bitmap whiteBitmap, Bitmap blackBitmap)
        {
            if (whiteBitmap == null || blackBitmap == null || whiteBitmap.Size != blackBitmap.Size)
                return null;

            int sizeX = whiteBitmap.Width,
                sizeY = whiteBitmap.Height;

            var final = new Bitmap(sizeX, sizeY, PixelFormat.Format32bppArgb);

            var empty = true;

            using (var a = new UnsafeBitmap(whiteBitmap))
            using (var b = new UnsafeBitmap(blackBitmap))
            using (var f = new UnsafeBitmap(final))
            {
                byte ToByte(int I) => (byte)(I > 255 ? 255 : (I < 0 ? 0 : I));

                for (var y = 0; y < sizeY; ++y)
                for (var x = 0; x < sizeX; ++x)
                {
                    PixelData* pixelA = a[x, y],
                        pixelB = b[x, y],
                        pixelF = f[x, y];
                    
                    pixelF->Alpha = ToByte((pixelB->Red - pixelA->Red + 255
                                            + pixelB->Green - pixelA->Green + 255
                                            + pixelB->Blue - pixelA->Blue + 255) / 3);

                    if (pixelF->Alpha <= 0)
                    {
                        continue;
                    }

                    // Following math creates an image optimized to be displayed on a black background
                    pixelF->Red = ToByte(255 * pixelB->Red / pixelF->Alpha);
                    pixelF->Green = ToByte(255 * pixelB->Green / pixelF->Alpha);
                    pixelF->Blue = ToByte(255 * pixelB->Blue / pixelF->Alpha);

                    if (empty)
                    {
                        empty = false;
                    }
                }
            }

            return empty ? null : final;
        }
    }
}