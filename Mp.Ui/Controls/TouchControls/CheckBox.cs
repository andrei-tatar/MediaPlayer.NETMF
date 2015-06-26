using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Mp.Ui.Managers;
using Mp.Ui.Primitives;

namespace Mp.Ui.Controls.TouchControls
{
    public sealed class CheckBox : TouchControl
    {
        #region members
        private bool _isChecked;
        private bool _pressed;
        private string _label;
        #endregion

        #region events
        public event UiEventHandler IsCheckedChanged;
        #endregion

        #region properties
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (value == _isChecked) return;
                _isChecked = value;
                Refresh();
                if (IsCheckedChanged != null) IsCheckedChanged(this);
            }
        }

        public string Label
        {
            get { return _label; }
            set
            {
                _label = value;
                RefreshParent();
            }
        }
        #endregion

        #region functionality
        internal override bool OnTouchDown(int x, int y)
        {
            _pressed = CaptureTouch();
            if (!_pressed) return false;
            Refresh();
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
                ReleaseCapture();
                if (ContainsScreenPoint(x, y))
                {
                    _isChecked = !_isChecked;
                    if (IsCheckedChanged != null) IsCheckedChanged(this);
                }
                _pressed = false;
                Refresh();
                return true;
            }
            return false;
        }

        internal override void Refresh(bool flush)
        {
            if (!CanRefresh()) return;

            int left = ScreenLeft, top = ScreenTop;

            _desktopOwner._screenBuffer.SetClippingRectangle(left, top, _width, _height);

            Style cStyle = StyleManager.CurrentStyle;

            int rectSize = System.Math.Min(_width, _height);

            _desktopOwner._screenBuffer.DrawRectangle(
                _enabled ? (_pressed ? cStyle.CheckBoxPressedBorder : cStyle.CheckBoxEnabledBorder) : cStyle.CheckBoxDisabledBorder, 1, left, top, rectSize, rectSize, 0, 0,
                _enabled ? (_pressed ? cStyle.CheckBoxPressedBack1 : cStyle.CheckBoxEnabledBack1) : cStyle.CheckBoxDisabledBack1, left, top,
                _enabled ? (_pressed ? cStyle.CheckBoxPressedBack2 : cStyle.CheckBoxEnabledBack2) : cStyle.CheckBoxDisabledBack2, left, top + rectSize, 256);

            if (_label.Length != 0)
            {
                Font textFont = cStyle.LabelFont;
                int tx = left + rectSize + 4, ty = top + (_height - textFont.Height) / 2;
                _desktopOwner._screenBuffer.DrawTextInRect(_label, tx, ty, left + _width - tx, _height, Bitmap.DT_TrimmingCharacterEllipsis,
                    _enabled ? cStyle.LabelEnabledColor : cStyle.LabelDisabledColor, textFont);
            }

            if (_pressed || _isChecked)
            {
                const int margin = 6;
                Color crossColor = _enabled ? (_pressed ? cStyle.CheckBoxPressedCross : cStyle.CheckBoxEnabledCross) : cStyle.CheckBoxDisabledCross;

                _desktopOwner._screenBuffer.DrawLine(crossColor, 2, left + margin, top + margin, left + rectSize - margin, top + rectSize - margin);
                _desktopOwner._screenBuffer.DrawLine(crossColor, 2, left + rectSize - margin, top + margin, left + margin, top + rectSize - margin);
            }

            base.Refresh(flush);
        }

        public override void Dispose()
        {
            IsCheckedChanged = null;
            base.Dispose();
        }
        #endregion

        #region constructors
        public CheckBox()
            : this(false, string.Empty, 0, 0, 25, 25)
        { }

        public CheckBox(bool isChecked, string label, int X, int Y, int Width, int Height)
            : base(X, Y, Width, Height)
        {
            _isChecked = isChecked;
            _pressed = false;
            _label = label;
        }
        #endregion
    }
}
