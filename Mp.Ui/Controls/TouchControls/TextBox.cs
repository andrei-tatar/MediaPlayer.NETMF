using System;

using Microsoft.SPOT;

using Mp.Ui.Primitives;
using Mp.Ui.Managers;
using Mp.Ui.Desktops;

namespace Mp.Ui.Controls.TouchControls
{
    public class TextBox : TouchControl
    {
        #region members
        private string _text;
        private bool _isValid;
        private bool _pressed;
        private bool _allowMultiline;
        private StringValidator _validator;
        private AllowedCharTypesEnum _allowedChars;
        private string _editTextLabel;
        #endregion
        
        #region events
        public event TextChangedHandler TextChanged;
        #endregion

        #region properties
        public bool Valid { get { return _isValid; } }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                if (!RevalidateText()) Refresh();
                if (TextChanged != null) TextChanged(_text, _isValid);
            }
        }

        public string EditTextLabel
        {
            get { return _editTextLabel; }
            set { _editTextLabel = value; }
        }

        public StringValidator Validator
        {
            get { return _validator; }
            set
            {
                _validator = value;
                RevalidateText();
            }
        }

        public AllowedCharTypesEnum AllowedChars
        {
            get { return _allowedChars; }
            set { _allowedChars = value; }
        }

        public bool AllowMultiline { get { return _allowMultiline; } set { _allowMultiline = value; } }
        #endregion

        #region functionality
        private bool RevalidateText()
        {
            bool _newValid = true;
            if (_validator != null)
                _newValid = _validator(_text);

            if (_newValid != _isValid)
            {
                _isValid = _newValid;
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
                _enabled ? (_isValid ? (_pressed ? cStyle.TextBoxPressedBorder : cStyle.TextBoxEnabledBorder) : cStyle.TextBoxEnabledInvalidBorder) : cStyle.TextBoxDisabledBorder, 1, left, top, _width, _height, 0, 0,
                _enabled ? (_pressed ? cStyle.TextBoxPressedBack1 : (_isValid ? cStyle.TextBoxEnabledBack1 : cStyle.TextBoxEnabledInvalidBack)) : cStyle.TextBoxDisabledBack1, left, top,
                _enabled ? (_pressed ? cStyle.TextBoxPressedBack2 : (_isValid ? cStyle.TextBoxEnabledBack2 : cStyle.TextBoxEnabledInvalidBack)) : cStyle.TextBoxDisabledBack2, left, top + _height, 256);

            int y = _allowMultiline ? top + 2 : top + (_height - cStyle.TextBoxFont.Height) / 2;

            _desktopOwner._screenBuffer.DrawTextInRect(_text, left + 2, y, _width - 4, _height - 4, Bitmap.DT_AlignmentLeft | Bitmap.DT_TrimmingCharacterEllipsis,
                _enabled ? (_pressed ? cStyle.TextBoxPressedTextColor : cStyle.TextBoxEnabledTextColor) : cStyle.TextBoxDisabledTextColor, cStyle.TextBoxFont);

            base.Refresh(flush);
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
                _pressed = false;
                ReleaseCapture();

                if (ContainsScreenPoint(x, y))
                {
                    EditDesktop.Current.AllowCloseOnlyWhenValid = _validator != null;
                    EditDesktop.Current.AllowMultiLine = _allowMultiline;
                    EditDesktop.Current.Text = _text;
                    EditDesktop.Current.TextValidator = _validator;
                    EditDesktop.Current.AllowedCharTypes = _allowedChars;
                    EditDesktop.Current.Closed += editDesktop_Closed;
                    EditDesktop.Current.EditLabelText = _editTextLabel;
                    EditDesktop.Current.Open();
                }
                else
                    Refresh();
                return true;
            }
            return false;
        }

        public override void Dispose()
        {
            _validator = null;
            TextChanged = null;
            base.Dispose();
        }
        #endregion

        #region event handlers
        private void editDesktop_Closed(Control sender)
        {
            EditDesktop.Current.Closed -= editDesktop_Closed;
            _isValid = EditDesktop.Current.TextValid;
            _text = EditDesktop.Current.Text;
            Refresh();
            if (TextChanged != null) TextChanged(_text, _isValid);
        }
        #endregion

        #region constructors
        public TextBox()
            : this(string.Empty, 0, 0, 50, 25)
        { }

        public TextBox(string Text, int X, int Y, int Width, int Height)
            : base(X, Y, Width, Height)
        {
            this.Text = Text;
            _isValid = true;
            _allowMultiline = false;
            _pressed = false;
            _allowedChars = AllowedCharTypesEnum.All;
            _editTextLabel = string.Empty;
        }
        #endregion
    }
}
