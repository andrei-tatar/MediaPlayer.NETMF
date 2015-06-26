using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Mp.Ui
{
    internal static class Utils
    {
        public static void ChangeBitmapColor(Bitmap img, Color anyDifferentColor, Color changeTo)
        {
            for (int x = 0; x < img.Width; x++)
                for (int y = 0; y < img.Height; y++)
                    if (img.GetPixel(x, y) != anyDifferentColor)
                        img.SetPixel(x, y, changeTo);
        }
    }
}
