using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Mp.Ui.Managers;
using Mp.Ui.Primitives;
using Mp.Ui.Controls;
using Mp.Ui.Controls.Containers;

namespace Mp.Ui.Desktops
{
    public abstract class ModalDesktop : Desktop
    {
        #region members
        protected readonly int _mX, _mY, _mWidth, _mHeight;
        private Desktop _prevDesktop;
        private AutoResetEvent _waitObject;
        private bool _blockThread;
        #endregion

        #region events
        public event UiEventHandler DesktopClosed;
        #endregion

        #region functionality
        protected void Show(bool blockThread = true)
        {
            _blockThread = blockThread;
            _prevDesktop = DesktopManager.Instance.CurrentDesktop;
            _screenBuffer.SetClippingRectangle(0, 0, _width, _height);
            _screenBuffer.DrawImage(0, 0, _prevDesktop._screenBuffer, 0, 0, _width, _height);
            Color overlayColor = Color.Black;
            _screenBuffer.DrawRectangle(overlayColor, 0, 0, 0, _width, _height, 0, 0, overlayColor, 0, 0, overlayColor, 0, _height - 1, 128);

            DesktopManager.Instance.AddDesktop(this);
            DesktopManager.Instance.SwitchDesktop(this);

            if (blockThread) _waitObject.WaitOne();
        }

        internal override void Refresh(bool flush)
        {
            if (!CanRefresh()) return;

            _screenBuffer.SetClippingRectangle(_mX, _mY, _mWidth, _mHeight);

            RefreshBackground(_mX, _mY, _mWidth, _mHeight);
            base.Refresh(false, true);

            if (flush) _screenBuffer.Flush(0, 0, _width, _height);
        }

        protected virtual void RefreshBackground(int x, int y, int width, int height)
        {
            Style cStyle = StyleManager.CurrentStyle;
            _screenBuffer.DrawRectangle(cStyle.BackgroundBorderColor, 1, _mX, _mY, _mWidth, _mHeight, 0, 0, cStyle.BackgroundColor, _mX, _mY, cStyle.BackgroundColor, _mX, _mY + _mHeight, 256);
        }

        protected void Close()
        {
            DesktopManager.Instance.RemoveDesktop(this, _prevDesktop);
            if (_blockThread) _waitObject.Set();
            else
            {
                if (DesktopClosed != null) DesktopClosed(this);
            }
        }
        #endregion

        #region constructor
        protected ModalDesktop(int width, int height)
        {
            _suspended = true;
            _mWidth = width;
            _mHeight = height;
            _mX = (DesktopManager.ScreenWidth - _mWidth) / 2;
            _mY = (DesktopManager.ScreenHeight - _mHeight) / 2;

            _padding = new Primitives.Point(_mX, _mY);

            _prevDesktop = null;
            _waitObject = new AutoResetEvent(false);
        }
        #endregion
    }
}
