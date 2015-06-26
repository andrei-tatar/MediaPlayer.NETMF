using System;
using System.Threading;

using Microsoft.SPOT;

using Mp.Ui.Managers;
using Mp.Ui.Primitives;
using Mp.Ui.Controls.Containers;

namespace Mp.Ui.Controls.TouchControls
{
    public abstract class Button : TouchControl
    {
        #region members
        private event UiEventHandler _buttonLongPressed;
        private int _longPressListeners;
        private bool _wasPressedOnce;

        protected bool _pressed;
        protected bool _repeatKeyPress;
        private Timer _periodicPressTimer;
        private Timer _longPressTimer;

        const int longTouchInterval = 500;
        const int periodicInterval = 50;
        #endregion

        #region events
        public event UiEventHandler ButtonPressed;
        public event UiEventHandler ButtonLongPressed
        {
            add
            {
                _longPressListeners++;
                _buttonLongPressed += value;

                if (_longPressListeners == 1)
                {
                    _longPressTimer = new Timer((o) =>
                        {
                            _longPressTimer.Change(Timeout.Infinite, Timeout.Infinite);
                            if (_wasPressedOnce) return;
                            _wasPressedOnce = true;
                            if (_pressed)
                            {
                                ReleaseCapture();
                                _pressed = false;
                                Refresh();
                            }
                            if (_buttonLongPressed != null) _buttonLongPressed(this);
                        }, null, Timeout.Infinite, Timeout.Infinite);
                }
            }
            remove
            {
                if (_longPressListeners == 0) return;

                _longPressListeners--;
                _buttonLongPressed -= value;

                if (_longPressListeners == 0)
                {
                    _longPressTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _longPressTimer.Dispose();
                    _longPressTimer = null;
                }
            }
        }
        #endregion

        #region properties
        public bool RepeatKeyPress
        {
            get { return _repeatKeyPress; }
            set
            {
                _repeatKeyPress = value;
                if (_repeatKeyPress)
                {
                    _periodicPressTimer = new Timer((o) =>
                    {
                        _wasPressedOnce = true;
                        if (ButtonPressed != null) ButtonPressed(this);
                    }, null, Timeout.Infinite, periodicInterval);
                }
                else
                {
                    if (_periodicPressTimer != null) _periodicPressTimer.Dispose();
                    _periodicPressTimer = null;
                }
            }
        }
        #endregion

        #region functionality
        internal override void Refresh(bool flush)
        {
            if (!CanRefresh()) return;

            int left = ScreenLeft, top = ScreenTop;
            _desktopOwner._screenBuffer.SetClippingRectangle(left, top, _width, _height);

            Style cStyle = StyleManager.CurrentStyle;

            _desktopOwner._screenBuffer.DrawRectangle(
                _enabled ? (_pressed ? cStyle.ButtonPressedBorder : cStyle.ButtonEnabledBorder) : cStyle.ButtonDisabledBorder, 1, left, top, _width, _height, 0, 0,
                _enabled ? (_pressed ? cStyle.ButtonPressedBack1 : cStyle.ButtonEnabledBack1) : cStyle.ButtonDisabledBack1, left, top,
                _enabled ? (_pressed ? cStyle.ButtonPressedBack2 : cStyle.ButtonEnabledBack2) : cStyle.ButtonDisabledBack2, left, top + _height, 256);

            DrawContent(left, top, _width, _height, cStyle);

            base.Refresh(flush);
        }

        protected abstract void DrawContent(int left, int top, int width, int height, Style cStyle);

        public override void Dispose()
        {
            if (_periodicPressTimer != null)
            {
                _periodicPressTimer.Dispose();
                _periodicPressTimer = null;
            }

            if (_longPressTimer != null)
            {
                _longPressTimer.Dispose();
                _longPressTimer = null;
            }

            ButtonPressed = _buttonLongPressed = null;
            base.Dispose();
        }
        #endregion

        #region touch functionality
        internal override bool OnTouchDown(int x, int y)
        {
            _wasPressedOnce = false;
            _pressed = CaptureTouch();
            if (!_pressed) return false;

            Refresh();
            if (_repeatKeyPress)
            {
                if (ButtonPressed != null) ButtonPressed(this);
                if (_periodicPressTimer != null) _periodicPressTimer.Change(longTouchInterval, periodicInterval);
            }
            if (_longPressListeners != 0)
            {
                _longPressTimer.Change(longTouchInterval, Timeout.Infinite);
            }
            return true;
        }

        internal override bool OnTouchMove(int x, int y)
        {
            return _pressed;
        }

        internal override bool OnTouchUp(int x, int y)
        {
            if (_pressed)
            {
                if (_periodicPressTimer != null) _periodicPressTimer.Change(Timeout.Infinite, Timeout.Infinite);
                ReleaseCapture();
                _pressed = false;
                Refresh();

                if (!_repeatKeyPress && !_wasPressedOnce && ButtonPressed != null && ContainsScreenPoint(x, y))
                {
                    _wasPressedOnce = true;
                    ButtonPressed(this);
                }
                if (_longPressListeners != 0) _longPressTimer.Change(Timeout.Infinite, Timeout.Infinite);
                return true;
            }
            return false;
        }
        #endregion

        public void TriggerButtonPressed()
        {
            if (ButtonPressed != null) ButtonPressed(this);
        }

        #region constructor
        public Button()
            : this(0, 0, 50, 20)
        { }

        public Button(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            _wasPressedOnce = false;
            _longPressListeners = 0;
            _repeatKeyPress = false;
            _pressed = false;
            _periodicPressTimer = null;
        }
        #endregion
    }
}
