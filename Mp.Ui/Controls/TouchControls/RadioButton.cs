using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Mp.Ui.Managers;
using Mp.Ui.Primitives;

namespace Mp.Ui.Controls.TouchControls
{
    public sealed class RadioButton : TouchControl
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

                if (_isChecked && _parent != null)
                {
                    foreach (Control c in _parent.Childs)
                    {
                        RadioButton otherRadioButton = c as RadioButton;
                        if (otherRadioButton == null || otherRadioButton == this) continue;
                        otherRadioButton.IsChecked = false;
                    }
                }
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
                if (ContainsScreenPoint(x, y) && !_isChecked)
                    IsChecked = !_isChecked;
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
            Color bColor = _enabled ? (_pressed ? cStyle.RadioButtonPressedBack : cStyle.RadioButtonEnabledBack) : cStyle.RadioButtonDisabledBack;

            int rectSize = System.Math.Min(_width, _height);
            int radius = rectSize / 2 - 1;

            _desktopOwner._screenBuffer.DrawEllipse(
                _enabled ? (_pressed ? cStyle.RadioButtonPressedBorder : cStyle.RadioButtonEnabledBorder) : cStyle.RadioButtonDisabledBorder, 1, left + radius, top + radius, radius, radius,
                bColor, 0, 0, bColor, 0, 0, 256);

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
                Color pointColor = _enabled ? (_pressed ? cStyle.RadioButtonPressedPoint : cStyle.RadioButtonEnabledPoint) : cStyle.RadioButtonDisabledPoint;

                _desktopOwner._screenBuffer.DrawEllipse(pointColor, 0, left + radius, top + radius, radius - margin, radius - margin,
                    pointColor, 0, 0, pointColor, 0, 0, 256);
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
        public RadioButton()
            : this(string.Empty, 0, 0, 25, 25)
        { }

        public RadioButton(string label, int X, int Y, int Width, int Height)
            : base(X, Y, Width, Height)
        {
            _isChecked = false;
            _pressed = false;
            _label = label;
        }
        #endregion
    }
}
