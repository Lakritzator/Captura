// ReSharper disable RedundantAssignment
namespace Captura.ImageEditor.Effects
{
    public static class PixelFunctionFactory
    {
        private static void Negative(ref byte red, ref byte green, ref byte blue)
        {
            red = (byte)(255 - red);
            green = (byte)(255 - green);
            blue = (byte)(255 - blue);
        }

        private static void Green(ref byte red, ref byte green, ref byte blue)
        {
            red = blue = 0;
        }

        private static void Red(ref byte red, ref byte green, ref byte blue)
        {
            green = blue = 0;
        }

        private static void Blue(ref byte red, ref byte green, ref byte blue)
        {
            red = green = 0;
        }

        private static void Grayscale(ref byte red, ref byte green, ref byte blue)
        {
            var pixel = 0.299 * red + 0.587 * green + 0.114 * blue;

            if (pixel > 255)
                pixel = 255;

            red = green = blue = (byte)pixel;
        }

        private static void Sepia(ref byte red, ref byte green, ref byte blue)
        {
            var newRed = 0.393 * red + 0.769 * green + 0.189 * blue;
            var newGreen = 0.349 * red + 0.686 * green + 0.168 * blue;
            var newBlue = 0.272 * red + 0.534 * green + 0.131 * blue;

            // Red
            red = (byte)(newRed > 255 ? 255 : newRed);

            // Green
            green = (byte)(newGreen > 255 ? 255 : newGreen);

            // Blue
            blue = (byte)(newBlue > 255 ? 255 : newBlue);
        }

        public static ModifyPixel GetEffectFunction(ImageEffect effect)
        {
            switch (effect)
            {
                case ImageEffect.Negative:
                    return Negative;

                case ImageEffect.Blue:
                    return Blue;

                case ImageEffect.Green:
                    return Green;

                case ImageEffect.Red:
                    return Red;

                case ImageEffect.Sepia:
                    return Sepia;

                case ImageEffect.Grayscale:
                    return Grayscale;

                default:
                    return null;
            }
        }
    }
}