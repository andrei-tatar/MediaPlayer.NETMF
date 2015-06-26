using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Mp.Ui
{
    public static class Colors
    {
        public static Color InvertColor(Color color)
        {
            byte
                r = (byte)(255 - ((int)color >> 8 * 0) & 0xFF),
                g = (byte)(255 - ((int)color >> 8 * 1) & 0xFF),
                b = (byte)(255 - ((int)color >> 8 * 2) & 0xFF);
            return ColorUtility.ColorFromRGB(r, g, b);
        }

        public static Color GetMedianColor(Color c1, Color c2)
        {
            byte
                r1 = (byte)(((int)c1 >> 8 * 0) & 0xFF),
                g1 = (byte)(((int)c1 >> 8 * 1) & 0xFF),
                b1 = (byte)(((int)c1 >> 8 * 2) & 0xFF),

                r2 = (byte)(((int)c2 >> 8 * 0) & 0xFF),
                g2 = (byte)(((int)c2 >> 8 * 1) & 0xFF),
                b2 = (byte)(((int)c2 >> 8 * 2) & 0xFF);

            return ColorUtility.ColorFromRGB((byte)((r1 + r2) / 2), (byte)((g1 + g2) / 2), (byte)((b1 + b2) / 2));
        }

        public static Color GetMedianColor(Color c1, Color c2, Color c3)
        {
            byte
                r1 = (byte)(((int)c1 >> 8 * 0) & 0xFF),
                g1 = (byte)(((int)c1 >> 8 * 1) & 0xFF),
                b1 = (byte)(((int)c1 >> 8 * 2) & 0xFF),

                r2 = (byte)(((int)c2 >> 8 * 0) & 0xFF),
                g2 = (byte)(((int)c2 >> 8 * 1) & 0xFF),
                b2 = (byte)(((int)c2 >> 8 * 2) & 0xFF),

                r3 = (byte)(((int)c3 >> 8 * 0) & 0xFF),
                g3 = (byte)(((int)c3 >> 8 * 1) & 0xFF),
                b3 = (byte)(((int)c3 >> 8 * 2) & 0xFF);

            return ColorUtility.ColorFromRGB((byte)((r1 + r2 + r3) / 3), (byte)((g1 + g2 + g3) / 3), (byte)((b1 + b2 + b3) / 3));
        }

        public const Color AliceBlue = (Color)0xFFF8F0;
        public const Color AntiqueWhite = (Color)0xD7EBFA;
        public const Color Aqua = (Color)0xFFFF00;
        public const Color Aquamarine = (Color)0xD4FF7F;
        public const Color Azure = (Color)0xFFFFF0;
        public const Color Beige = (Color)0xDCF5F5;
        public const Color Bisque = (Color)0xC4E4FF;
        public const Color Black = (Color)0x000000;
        public const Color BlanchedAlmond = (Color)0xCDEBFF;
        public const Color Blue = (Color)0xFF0000;
        public const Color BlueViolet = (Color)0xE22B8A;
        public const Color Brown = (Color)0x2A2AA5;
        public const Color BurlyWood = (Color)0x87B8DE;
        public const Color CadetBlue = (Color)0xA09E5F;
        public const Color Chartreuse = (Color)0x00FF7F;
        public const Color Chocolate = (Color)0x1E69D2;
        public const Color Coral = (Color)0x507FFF;
        public const Color CornflowerBlue = (Color)0xED9564;
        public const Color Cornsilk = (Color)0xDCF8FF;
        public const Color Crimson = (Color)0x3C14DC;
        public const Color Cyan = (Color)0xFFFF00;
        public const Color DarkBlue = (Color)0x8B0000;
        public const Color DarkCyan = (Color)0x8B8B00;
        public const Color DarkGoldenrod = (Color)0x0B86B8;
        public const Color DarkGray = (Color)0xA9A9A9;
        public const Color DarkGreen = (Color)0x006400;
        public const Color DarkKhaki = (Color)0x6BB7BD;
        public const Color DarkMagenta = (Color)0x8B008B;
        public const Color DarkOliveGreen = (Color)0x2F6B55;
        public const Color DarkOrange = (Color)0x008CFF;
        public const Color DarkOrchid = (Color)0xCC3299;
        public const Color DarkRed = (Color)0x00008B;
        public const Color DarkSalmon = (Color)0x7A96E9;
        public const Color DarkSeaGreen = (Color)0x8FBC8F;
        public const Color DarkSlateBlue = (Color)0x8B3D48;
        public const Color DarkSlateGray = (Color)0x4F4F2F;
        public const Color DarkTurquoise = (Color)0xD1CE00;
        public const Color DarkViolet = (Color)0xD30094;
        public const Color DeepPink = (Color)0x9314FF;
        public const Color DeepSkyBlue = (Color)0xFFBF00;
        public const Color DimGray = (Color)0x696969;
        public const Color DodgerBlue = (Color)0xFF901E;
        public const Color Firebrick = (Color)0x2222B2;
        public const Color FloralWhite = (Color)0xF0FAFF;
        public const Color ForestGreen = (Color)0x228B22;
        public const Color Fuchsia = (Color)0xFF00FF;
        public const Color Gainsboro = (Color)0xDCDCDC;
        public const Color GhostWhite = (Color)0xFFF8F8;
        public const Color Gold = (Color)0x00D7FF;
        public const Color Goldenrod = (Color)0x20A5DA;
        public const Color Gray = (Color)0x808080;
        public const Color Green = (Color)0x008000;
        public const Color GreenYellow = (Color)0x2FFFAD;
        public const Color Honeydew = (Color)0xF0FFF0;
        public const Color HotPink = (Color)0xB469FF;
        public const Color IndianRed = (Color)0x5C5CCD;
        public const Color Indigo = (Color)0x82004B;
        public const Color Ivory = (Color)0xF0FFFF;
        public const Color Khaki = (Color)0x8CE6F0;
        public const Color Lavender = (Color)0xFAE6E6;
        public const Color LavenderBlush = (Color)0xF5F0FF;
        public const Color LawnGreen = (Color)0x00FC7C;
        public const Color LemonChiffon = (Color)0xCDFAFF;
        public const Color LightBlue = (Color)0xE6D8AD;
        public const Color LightCoral = (Color)0x8080F0;
        public const Color LightCyan = (Color)0xFFFFE0;
        public const Color LightGoldenrodYellow = (Color)0xD2FAFA;
        public const Color LightGray = (Color)0xD3D3D3;
        public const Color LightGreen = (Color)0x90EE90;
        public const Color LightPink = (Color)0xC1B6FF;
        public const Color LightSalmon = (Color)0x7AA0FF;
        public const Color LightSeaGreen = (Color)0xAAB220;
        public const Color LightSkyBlue = (Color)0xFACE87;
        public const Color LightSlateGray = (Color)0x998877;
        public const Color LightSteelBlue = (Color)0xDEC4B0;
        public const Color LightYellow = (Color)0xE0FFFF;
        public const Color Lime = (Color)0x00FF00;
        public const Color LimeGreen = (Color)0x32CD32;
        public const Color Linen = (Color)0xE6F0FA;
        public const Color Magenta = (Color)0xFF00FF;
        public const Color Maroon = (Color)0x000080;
        public const Color MediumAquamarine = (Color)0xAACD66;
        public const Color MediumBlue = (Color)0xCD0000;
        public const Color MediumOrchid = (Color)0xD355BA;
        public const Color MediumPurple = (Color)0xDB7093;
        public const Color MediumSeaGreen = (Color)0x71B33C;
        public const Color MediumSlateBlue = (Color)0xEE687B;
        public const Color MediumSpringGreen = (Color)0x9AFA00;
        public const Color MediumTurquoise = (Color)0xCCD148;
        public const Color MediumVioletRed = (Color)0x8515C7;
        public const Color MidnightBlue = (Color)0x701919;
        public const Color MintCream = (Color)0xFAFFF5;
        public const Color MistyRose = (Color)0xE1E4FF;
        public const Color Moccasin = (Color)0xB5E4FF;
        public const Color NavajoWhite = (Color)0xADDEFF;
        public const Color Navy = (Color)0x800000;
        public const Color OldLace = (Color)0xE6F5FD;
        public const Color Olive = (Color)0x008080;
        public const Color OliveDrab = (Color)0x238E6B;
        public const Color Orange = (Color)0x00A5FF;
        public const Color OrangeRed = (Color)0x0045FF;
        public const Color Orchid = (Color)0xD670DA;
        public const Color PaleGoldenrod = (Color)0xAAE8EE;
        public const Color PaleGreen = (Color)0x98FB98;
        public const Color PaleTurquoise = (Color)0xEEEEAF;
        public const Color PaleVioletRed = (Color)0x9370DB;
        public const Color PapayaWhip = (Color)0xD5EFFF;
        public const Color PeachPuff = (Color)0xB9DAFF;
        public const Color Peru = (Color)0x3F85CD;
        public const Color Pink = (Color)0xCBC0FF;
        public const Color Plum = (Color)0xDDA0DD;
        public const Color PowderBlue = (Color)0xE6E0B0;
        public const Color Purple = (Color)0x800080;
        public const Color Red = (Color)0x0000FF;
        public const Color RosyBrown = (Color)0x8F8FBC;
        public const Color RoyalBlue = (Color)0xE16941;
        public const Color SaddleBrown = (Color)0x13458B;
        public const Color Salmon = (Color)0x7280FA;
        public const Color SandyBrown = (Color)0x60A4F4;
        public const Color SeaGreen = (Color)0x578B2E;
        public const Color SeaShell = (Color)0xEEF5FF;
        public const Color Sienna = (Color)0x2D52A0;
        public const Color Silver = (Color)0xC0C0C0;
        public const Color SkyBlue = (Color)0xEBCE87;
        public const Color SlateBlue = (Color)0xCD5A6A;
        public const Color SlateGray = (Color)0x908070;
        public const Color Snow = (Color)0xFAFAFF;
        public const Color SpringGreen = (Color)0x7FFF00;
        public const Color SteelBlue = (Color)0xB48246;
        public const Color Tan = (Color)0x8CB4D2;
        public const Color Teal = (Color)0x808000;
        public const Color Thistle = (Color)0xD8BFD8;
        public const Color Tomato = (Color)0x4763FF;
        public const Color Turquoise = (Color)0xD0E040;
        public const Color Violet = (Color)0xEE82EE;
        public const Color Wheat = (Color)0xB3DEF5;
        public const Color White = (Color)0xFFFFFF;
        public const Color WhiteSmoke = (Color)0xF5F5F5;
        public const Color Yellow = (Color)0x00FFFF;
        public const Color YellowGreen = (Color)0x32CD9A;
    }
}
