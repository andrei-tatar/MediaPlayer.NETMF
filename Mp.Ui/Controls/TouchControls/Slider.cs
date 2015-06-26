using System;
using Microsoft.SPOT;
using Mp.Ui.Managers;
using Mp.Ui.Primitives;

namespace Mp.Ui.Controls.TouchControls
{
    public sealed class Slider : TouchControl
    {
        public const int SliderSize = 20;

        #region members
        private int _min, _max, _pos;
        private Orientation _orientation;
        private bool _pressed;
        #endregion

        #region events
        public event UiEventHandler PositionChanged;
        #endregion

        #region properties
        public Orientation Orientation
        {
            get { return _orientation; }
            set
            {
                _orientation = value;
                Refresh();
            }
        }

        public int Minimum
        {
            get { return _min; }
            set
            {
                if (value >= _max) throw new ArgumentOutOfRangeException("Minimum must be less than maximum");
                _min = value;
                _pos = System.Math.Max(_min, _pos);
                Refresh();
            }
        }

        public int Maximum
        {
            get { return _max; }
            set
            {
                if (value <= _min) throw new ArgumentOutOfRangeException("Maximum must be greater than minimum");
                _max = value;
                _pos = System.Math.Min(_pos, _max);
                Refresh();
            }
        }

        public int Position
        {
            get { return _pos; }
            set
            {
                _pos = System.Math.Min(_max, System.Math.Max(value, _min));
                Refresh();
                if (PositionChanged != null) PositionChanged(this);
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
                _enabled ? cStyle.ProgressBarEnabledBorder : cStyle.ProgressBarDisabledBorder, 1, left, top, _width, _height, 0, 0,
                _enabled ? cStyle.ProgressBarEnabledBack1 : cStyle.ProgressBarDisabledBack1, left, top,
                _enabled ? cStyle.ProgressBarEnabledBack2 : cStyle.ProgressBarDisabledBack2, left, top + _height, 256);

            if (_orientation == Orientation.Horizontal)
            {
                int avWidth = _width - SliderSize;
                int pBarWidth = (_pos - _min) * avWidth / (_max - _min) + SliderSize / 2;

                left++;
                top++;
                _desktopOwner._screenBuffer.DrawRectangle(Colors.Black, 0, left, top, pBarWidth, _height - 2, 0, 0,
                    cStyle.ProgressBarBack1, left, top, cStyle.ProgressBarBack2, left, top + _height - 2, _enabled ? (ushort)256 : (ushort)128);

                left = left - 1 + pBarWidth - SliderSize / 2;
                top--;
                _desktopOwner._screenBuffer.DrawRectangle(
                    _enabled ? (_pressed ? cStyle.ButtonPressedBorder : cStyle.ButtonEnabledBorder) : cStyle.ButtonDisabledBorder, 1, left, top, SliderSize, _height, 0, 0,
                    _enabled ? (_pressed ? cStyle.ButtonPressedBack1 : cStyle.ButtonEnabledBack1) : cStyle.ButtonDisabledBack1, left, top,
                    _enabled ? (_pressed ? cStyle.ButtonPressedBack2 : cStyle.ButtonEnabledBack2) : cStyle.ButtonDisabledBack2, left, top + _height, 256);
            }
            else
            {
                int avHeight = _height - SliderSize;
                int pBarHeight = (_pos - _min) * avHeight / (_max - _min) + SliderSize / 2;

                left++;
                top++;
                _desktopOwner._screenBuffer.DrawRectangle(Colors.Black, 0, left, top + avHeight - pBarHeight + SliderSize, _width - 2, pBarHeight, 0, 0,
                    cStyle.ProgressBarBack1, left, top, cStyle.ProgressBarBack2, left, top + _height - 2, _enabled ? (ushort)256 : (ushort)128);

                left--;
                top = top - 1 + _height - pBarHeight - SliderSize / 2;
                _desktopOwner._screenBuffer.DrawRectangle(
                    _enabled ? (_pressed ? cStyle.ButtonPressedBorder : cStyle.ButtonEnabledBorder) : cStyle.ButtonDisabledBorder, 1, left, top, _width, SliderSize, 0, 0,
                    _enabled ? (_pressed ? cStyle.ButtonPressedBack1 : cStyle.ButtonEnabledBack1) : cStyle.ButtonDisabledBack1, left, top,
                    _enabled ? (_pressed ? cStyle.ButtonPressedBack2 : cStyle.ButtonEnabledBack2) : cStyle.ButtonDisabledBack2, left, top + SliderSize, 256);
            }

            base.Refresh(flush);
        }

        private bool ComputePositionFromScreenCoords(int x, int y)
        {
            if (_orientation == Controls.Orientation.Horizontal)
            {
                int domain = _max - _min;
                int avWidth = _width - 2 - SliderSize;
                int newPos = (x - ScreenLeft - SliderSize / 2 + avWidth / domain / 2) * domain / avWidth + _min;
                newPos = System.Math.Min(_max, System.Math.Max(_min, newPos));
                if (_pos == newPos) return false;
                _pos = newPos;
            }
            else
            {
                int domain = _max - _min;
                int avHeight = _height - 2 - SliderSize;
                int newPos = (_height - (y - ScreenTop) - SliderSize / 2 + avHeight / domain / 2) * domain / avHeight + _min;
                newPos = System.Math.Min(_max, System.Math.Max(_min, newPos));
                if (_pos == newPos) return false;
                _pos = newPos;
            }
            if (PositionChanged != null) PositionChanged(this);
            return true;
        }

        internal override bool OnTouchDown(int x, int y)
        {
            _pressed = CaptureTouch();
            if (!_pressed) return false;

            ComputePositionFromScreenCoords(x, y);
            Refresh();
            return true;
        }

        internal override bool OnTouchMove(int x, int y)
        {
            if (_pressed)
            {
                if (ComputePositionFromScreenCoords(x, y)) Refresh();
                return true;
            }
            return false;
        }

        internal override bool OnTouchUp(int x, int y)
        {
            if (_pressed)
            {
                _pressed = false;
                ComputePositionFromScreenCoords(x, y);
                ReleaseCapture();
                Refresh();
                return true;
            }
            return false;
        }
        #endregion

        #region constructors
        public Slider()
            : this(0, 0, 100, 30)
        { }

        public Slider(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            _min = 0;
            _max = 100;
            _pos = 0;
            _pressed = false;
        }
        #endregion
    }
}
