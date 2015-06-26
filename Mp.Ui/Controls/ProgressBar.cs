using System;
using Microsoft.SPOT;
using Mp.Ui.Managers;

namespace Mp.Ui.Controls
{
    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    public sealed class ProgressBar : Control
    {
        #region members
        private int _min, _max, _pos;
        private Orientation _orientation;
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
                int avWidth = _width - 2;
                int pBarWidth = (_pos - _min) * avWidth / (_max - _min);

                left++;
                top++;
                _desktopOwner._screenBuffer.DrawRectangle(Colors.Black, 0, left, top, pBarWidth, _height - 2, 0, 0,
                    cStyle.ProgressBarBack1, left, top, cStyle.ProgressBarBack2, left, top + _height - 2, _enabled ? (ushort)256 : (ushort)128);
            }
            else
            {
                int avHeight = _height - 2;
                int pBarHeight = (_pos - _min) * avHeight / (_max - _min);

                left++;
                top++;
                _desktopOwner._screenBuffer.DrawRectangle(Colors.Black, 0, left, top + avHeight - pBarHeight, _width - 2, pBarHeight, 0, 0,
                    cStyle.ProgressBarBack1, left, top, cStyle.ProgressBarBack2, left, top + _height - 2, _enabled ? (ushort)256 : (ushort)128);
            }

            base.Refresh(flush);
        }
        #endregion

        #region constructors
        public ProgressBar()
            : this(0, 0, 100, 30)
        { }

        public ProgressBar(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            _orientation = Orientation.Horizontal;
            _min = 0;
            _max = 100;
            _pos = 0;
        }
        #endregion
    }
}
