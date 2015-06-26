using System;

using Microsoft.SPOT;

namespace Mp.Ui
{
    public static class Fonts
    {
        private static Font _arial, _arialBold, _arialItalic;
        private static Font _arialMedium, _arialMediumBold, _arialMediumItalic;
        private static Font _arialBig, _arialBigBold, _arialBigItalic;

        public static Font Arial { get { return _arial ?? (_arial = Resources.Fonts.GetFont(Resources.Fonts.FontResources.Arial_9_regular)); } }
        public static Font ArialBold { get { return _arialBold ?? (_arialBold = Resources.Fonts.GetFont(Resources.Fonts.FontResources.Arial_9_bold)); } }
        public static Font ArialItalic { get { return _arialItalic ?? (_arialItalic = Resources.Fonts.GetFont(Resources.Fonts.FontResources.Arial_9_italic)); } }

        public static Font ArialMedium { get { return _arialMedium ?? (_arialMedium = Resources.Fonts.GetFont(Resources.Fonts.FontResources.Arial_12_regular)); } }
        public static Font ArialMediumBold { get { return _arialMediumBold ?? (_arialMediumBold = Resources.Fonts.GetFont(Resources.Fonts.FontResources.Arial_12_bold)); } }
        public static Font ArialMediumItalic { get { return _arialMediumItalic ?? (_arialMediumItalic = Resources.Fonts.GetFont(Resources.Fonts.FontResources.Arial_12_italic)); } }

        public static Font ArialBig { get { return _arialBig ?? (_arialBig = Resources.Fonts.GetFont(Resources.Fonts.FontResources.Arial_18_regular)); } }
        public static Font ArialBigBold { get { return _arialBigBold ?? (_arialBigBold = Resources.Fonts.GetFont(Resources.Fonts.FontResources.Arial_18_bold)); } }
        public static Font ArialBigItalic { get { return _arialBigItalic ?? (_arialBigItalic = Resources.Fonts.GetFont(Resources.Fonts.FontResources.Arial_18_italic)); } }
    }
}
