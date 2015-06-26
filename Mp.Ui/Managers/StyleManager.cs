using System;
using System.Threading;

using Microsoft.SPOT;

using Mp.Ui.Styles;
using Mp.Ui.Primitives;
using Mp.Ui.Controls.Containers;

namespace Mp.Ui.Managers
{
    public static class StyleManager
    {
        private static Style _currentStyle = new StyleMetroLight();

        public static event StyleChangedHandler StyleChanged;

        public static Style CurrentStyle
        {
            get { return _currentStyle; }
            set
            {
                if (value == null) return;

                Style _oldStyle = _currentStyle;
                _currentStyle = value;

                DesktopManager.Instance.CurrentDesktop._suspended = true;
                if (StyleChanged != null) StyleChanged(_oldStyle, _currentStyle);
                DesktopManager.Instance.CurrentDesktop._suspended = false;
                DesktopManager.Instance.CurrentDesktop.Refresh();
            }
        }
    }
}
