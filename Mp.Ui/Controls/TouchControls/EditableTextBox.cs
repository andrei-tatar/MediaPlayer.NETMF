using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Mp.Ui.Primitives;
using Mp.Ui.Managers;

namespace Mp.Ui.Controls.TouchControls
{
    public sealed class EditableTextBox : TouchControl
    {
        #region members
        private string _text;
        private bool _isValid;
        private int _carretPos;
        private StringValidator _validator;
        #endregion

        #region properties
        public bool Valid { get { return _isValid; } }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                _carretPos = _text.Length;
                if (!RevalidateText()) Refresh();
            }
        }

        public int CarretPosition
        {
            get { return _carretPos; }
            set
            {
                _carretPos = value;
                if (_carretPos < 0) _carretPos = 0;
                else if (_carretPos > _text.Length) _carretPos = _text.Length;
                Refresh();
            }
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

            int relX, relY;
            string toCarret = _text.Length == 0 ? _text : _text.Substring(0, _carretPos);
            bool doAnother = toCarret.Length == 0 ? false : toCarret[toCarret.Length - 1] == '\n';

            Style cStyle = StyleManager.CurrentStyle;

            //TODO: optimize drawing a little?
            do
            {
                relX = relY = 0;

                if (toCarret == null && doAnother)
                {
                    doAnother = false;
                }

                _desktopOwner._screenBuffer.DrawRectangle(
                    _enabled ? (_isValid ? cStyle.TextBoxEnabledBorder : cStyle.TextBoxEnabledInvalidBorder) : cStyle.TextBoxDisabledBorder, 1, left, top, _width, _height, 0, 0,
                    _enabled ? (_isValid ? cStyle.TextBoxEnabledBack1 : cStyle.TextBoxEnabledInvalidBack) : cStyle.TextBoxDisabledBack1, left, top,
                    _enabled ? (_isValid ? cStyle.TextBoxEnabledBack2 : cStyle.TextBoxEnabledInvalidBack) : cStyle.TextBoxDisabledBack2, left, left + _height, 256);

                _desktopOwner._screenBuffer.DrawTextInRect(ref toCarret, ref relX, ref relY, left + 2, top + 2, _width - 4, _height - 4,
                    Bitmap.DT_AlignmentLeft | Bitmap.DT_WordWrap,
                    _enabled ? cStyle.TextBoxEnabledTextColor : cStyle.TextBoxDisabledTextColor, cStyle.TextBoxFont);

                if (toCarret == null && doAnother)
                {
                    relX = 0;
                    relY += cStyle.TextBoxFont.Height;
                    if (relY < _height - 4 - cStyle.TextBoxFont.Height) doAnother = false;
                }

            } while (toCarret != null || doAnother);

            _desktopOwner._screenBuffer.DrawLine(cStyle.TextBoxEnabledTextColor, 1, left + 2 + relX, top + 2 + relY, left + 2 + relX, top + 2 + relY + cStyle.TextBoxFont.Height);

            string rest = _text.Substring(_carretPos, _text.Length - _carretPos);
            _desktopOwner._screenBuffer.DrawTextInRect(ref rest, ref relX, ref relY, left + 2, top + 2, _width - 4, _height - 4,
                Bitmap.DT_AlignmentLeft | Bitmap.DT_WordWrap,
                _enabled ? cStyle.TextBoxEnabledTextColor : cStyle.TextBoxDisabledTextColor, cStyle.TextBoxFont);

            if (rest != null) { relX = 0; relY += cStyle.TextBoxFont.Height; }
            _desktopOwner._screenBuffer.DrawTextInRect(ref rest, ref relX, ref relY, left + 2, top + 2, _width - 4, _height - 4,
                Bitmap.DT_AlignmentLeft | Bitmap.DT_WordWrap,
                _enabled ? cStyle.TextBoxEnabledTextColor : cStyle.TextBoxDisabledTextColor, cStyle.TextBoxFont);

            base.Refresh(flush);
        }

        internal override bool OnTouchDown(int x, int y)
        {
            //TODO: Get new carret position from coordinate
            return true;
        }

        public void InsertAtCaret(char ch)
        {
            InsertAtCaret(ch.ToString(), true);
        }

        public void InsertAtCaret(string text)
        {
            InsertAtCaret(text, true);
        }

        public void InsertAtCaret(string text, bool updateCarretPosition)
        {
            _text = _text.Substring(0, _carretPos) + text + _text.Substring(_carretPos, _text.Length - _carretPos);
            if (updateCarretPosition) _carretPos += text.Length;
            if (!RevalidateText()) Refresh();
        }

        public void BackspaceAtCarret()
        {
            if (_carretPos == 0) return;
            _text = _text.Substring(0, _carretPos - 1) + _text.Substring(_carretPos, _text.Length - _carretPos);
            _carretPos--;
            if (!RevalidateText()) Refresh();
        }

        public override void Dispose()
        {
            _validator = null;
            base.Dispose();
        }
        #endregion

        #region constructors
        public EditableTextBox()
        {
            Text = string.Empty;
            _carretPos = 0;
            _isValid = true;
        }

        public EditableTextBox(string Text, int X, int Y, int Width, int Height)
            : base(X, Y, Width, Height)
        {
            this.Text = Text;
            _isValid = true;
        }
        #endregion
    }
}
