using System;
using Microsoft.SPOT;

namespace Mp.Ui.Controls
{
    public class UserControl : TouchControl
    {
        private RefreshCallback _refreshMethod;

        public delegate void RefreshCallback(Bitmap screen);

        protected RefreshCallback RefreshMethod
        {
            get { return _refreshMethod; }
            set { _refreshMethod = value; RefreshParent(); }
        }

        internal override void Refresh(bool flush)
        {
            if (_refreshMethod != null)
            {
                if (!CanRefresh()) return;
                _desktopOwner._screenBuffer.SetClippingRectangle(_parent.ScreenLeft, _parent.ScreenTop, _parent.Width, _parent.Height);
                _refreshMethod(_desktopOwner._screenBuffer);
                base.Refresh(flush);
            }
        }

        protected virtual bool TouchDown(int x, int y)
        { return false; }

        protected virtual bool TouchMove(int x, int y)
        { return false; }

        protected virtual bool TouchUp(int x, int y)
        { return false; }

        internal override bool OnTouchDown(int x, int y)
        { return TouchDown(x, y); }

        internal override bool OnTouchMove(int x, int y)
        { return TouchMove(x, y); }

        internal override bool OnTouchUp(int x, int y)
        { return TouchUp(x, y); }

        public UserControl(int X, int Y, int Width, int Height)
            : base(X, Y, Width, Height)
        {
        }

        public override void Dispose()
        {
            _refreshMethod = null;
            base.Dispose();
        }
    }
}
