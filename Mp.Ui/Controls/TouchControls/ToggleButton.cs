using System;
using Microsoft.SPOT;
using Mp.Ui.Primitives;
using Mp.Ui.Managers;

namespace Mp.Ui.Controls.TouchControls
{
    public abstract class ToggleButton : TouchControl
    {
        protected bool _pressed;
        protected bool _isChecked;

        public event UiEventHandler IsCheckedChanged;

        public bool ActAsRadiobutton { get; set; }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (value == _isChecked) return;
                _isChecked = value;

                if (value && ActAsRadiobutton && _parent != null)
                    foreach (Control c in _parent.Childs)
                        if (c != this && c is ToggleButton)
                        {
                            ToggleButton otherButton = c as ToggleButton;
                            if (otherButton.ActAsRadiobutton) otherButton.IsChecked = false;
                        }

                Refresh();
                if (IsCheckedChanged != null) IsCheckedChanged(this);
            }
        }

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
                    if (ActAsRadiobutton)
                    {
                        if (!_isChecked) IsChecked = true;
                    }
                    else
                        IsChecked = !IsChecked;
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

            _desktopOwner._screenBuffer.DrawRectangle(
                _enabled ? (_pressed ? Colors.GetMedianColor(cStyle.ButtonPressedBorder, cStyle.ButtonEnabledBorder) : _isChecked ? cStyle.ButtonPressedBorder : cStyle.ButtonEnabledBorder) : cStyle.ButtonDisabledBorder, 1, left, top, _width, _height, 0, 0,
                _enabled ? (_pressed ? Colors.GetMedianColor(cStyle.ButtonPressedBack1, cStyle.ButtonEnabledBack1) : _isChecked ? cStyle.ButtonPressedBack1 : cStyle.ButtonEnabledBack1) : cStyle.ButtonDisabledBack1, left, top,
                _enabled ? (_pressed ? Colors.GetMedianColor(cStyle.ButtonPressedBack2, cStyle.ButtonEnabledBack2) : _isChecked ? cStyle.ButtonPressedBack2 : cStyle.ButtonEnabledBack2) : cStyle.ButtonDisabledBack2, left, top + _height, 256);

            DrawContent(left, top, _width, _height, cStyle);

            base.Refresh(flush);
        }

        protected abstract void DrawContent(int left, int top, int width, int height, Style cStyle);

        public override void Dispose()
        {
            IsCheckedChanged = null;
            base.Dispose();
        }

        public ToggleButton(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            ActAsRadiobutton = true;
        }
    }
}
