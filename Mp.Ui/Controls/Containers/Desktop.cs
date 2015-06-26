using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Touch;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Media;

using Mp.Ui.Managers;
using Mp.Ui.Primitives;

namespace Mp.Ui.Controls.Containers
{
    public class Desktop : Container
    {
        #region members
        private static TouchControl _captured;
        
        internal readonly Bitmap _screenBuffer;
        protected DrawBackgroundHandler _drawBackground;
        #endregion

        #region events
        public event TouchEventHandler TouchUp, TouchDown, TouchMove;
        #endregion

        #region properties
        internal TouchControl Captured { get { return _captured; } }

        public DrawBackgroundHandler DrawBackground
        {
            get { return _drawBackground; }
            set { _drawBackground = value; Refresh(); }
        }

        public override int X
        {
            get { return base.X; }
            set { throw new NotSupportedException("X coordinate can not be set on a Desktop"); }
        }

        public override int Y
        {
            get { return base.Y; }
            set { throw new NotSupportedException("Y coordinate can not be set on a Desktop"); }
        }

        public override int Width
        {
            get { return base.Width; }
            set { throw new NotSupportedException("Width can not be set on a Desktop"); }
        }

        public override int Height
        {
            get { return base.Height; }
            set { throw new NotSupportedException("Height can not be set on a Desktop"); }
        }
        #endregion

        #region protected/internal functionality
        protected override bool CanRefresh()
        {
            return _visible && !_suspended;
        }

        protected void Refresh(bool flush, bool onlyControls)
        {
            if (!CanRefresh()) return;

            if (!onlyControls)
            {
                _screenBuffer.SetClippingRectangle(_x, _y, _width, _height);
                if (_drawBackground != null)
                    _drawBackground(_screenBuffer, _width, _height);
                else
                {
                    Style cStyle = StyleManager.CurrentStyle;
                    _screenBuffer.DrawRectangle(cStyle.BackgroundColor, 1, _x, _y, _width, _height, 0, 0, cStyle.BackgroundColor, 0, 0, cStyle.BackgroundColor, 0, 0, 256);
                }
            }

            base.Refresh(flush);
        }

        internal override void Refresh(bool flush)
        {
            Refresh(flush, false);
        }

        internal bool CaptureTouch(TouchControl control)
        {
            if (_captured == null)
            {
                _captured = control;
                return true;
            }
            return false;
        }

        internal void ReleaseTouchCapture()
        {
            _captured = null;
        }
        #endregion

        #region public functionality
        public override void Dispose()
        {
            _screenBuffer.Dispose();
            _drawBackground = null;
            _captured = null;

            TouchUp = TouchDown = TouchMove = null;

            base.Dispose();
        }

        internal override bool OnTouchDown(int x, int y)
        {
            bool result = _captured != null ? _captured.OnTouchDown(x, y) : base.OnTouchDown(x, y);
            if (!result && TouchDown != null) result = TouchDown(this, x, y);
            return result;
        }

        internal override bool OnTouchMove(int x, int y)
        {
            bool result = _captured != null ? _captured.OnTouchMove(x, y) : base.OnTouchMove(x, y);
            if (!result && TouchMove != null) result = TouchMove(this, x, y);
            return result;
        }

        internal override bool OnTouchUp(int x, int y)
        {
            bool result = _captured != null ? _captured.OnTouchUp(x, y) : base.OnTouchUp(x, y);
            if (!result && TouchUp != null) result = TouchUp(this, x, y);
            return result;
        }
        #endregion

        #region constructors
        internal Desktop()
            : this(new Bitmap(DesktopManager.ScreenWidth, DesktopManager.ScreenHeight))
        { }

        internal Desktop(Bitmap screenBuffer)
        {
            _screenBuffer = screenBuffer;
            _width = screenBuffer.Width;
            _height = screenBuffer.Height;
            _x = _y = 0;
            _parent = null;
            _desktopOwner = this;
            _captured = null;
        }
        #endregion
    }
}
