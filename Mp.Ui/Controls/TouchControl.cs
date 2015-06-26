using System;

using Microsoft.SPOT;

namespace Mp.Ui.Controls
{
    public abstract class TouchControl : Control
    {
        internal virtual bool OnTouchDown(int x, int y) { return false; }
        internal virtual bool OnTouchMove(int x, int y) { return false; }
        internal virtual bool OnTouchUp(int x, int y) { return false; }

        protected bool CaptureTouch()
        {
            return _desktopOwner.CaptureTouch(this);
        }

        protected void ReleaseCapture()
        {
            if (_desktopOwner != null && _desktopOwner.Captured == this)
                _desktopOwner.ReleaseTouchCapture();
        }

        public TouchControl()
        { }

        public TouchControl(int x, int y, int width, int height)
            : base(x, y, width, height)
        { }
    }
}
