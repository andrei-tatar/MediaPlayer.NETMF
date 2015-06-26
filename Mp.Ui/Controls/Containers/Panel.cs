using System;

using Microsoft.SPOT;

using Mp.Ui.Managers;

namespace Mp.Ui.Controls.Containers
{
    public class Panel : Container
    {
        public Panel()
        { }

        internal override void Refresh(bool flush)
        {
            if (!CanRefresh()) return;

            Style cStyle = StyleManager.CurrentStyle;

            int left = ScreenLeft, top = ScreenTop;
            _desktopOwner._screenBuffer.SetClippingRectangle(left, top, _width, _height);
            _desktopOwner._screenBuffer.DrawRectangle(cStyle.BackgroundColor, 0, left, top, _width, _height, 0, 0,
                cStyle.BackgroundColor, left, top, cStyle.BackgroundColor, left, top + _height, 256);
            base.Refresh(flush);
        }

        public Panel(int X, int Y, int Width, int Height)
            : base(X, Y, Width, Height)
        { }
    }
}
