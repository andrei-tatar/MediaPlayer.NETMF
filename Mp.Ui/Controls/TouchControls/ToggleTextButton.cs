using System;
using Microsoft.SPOT;

namespace Mp.Ui.Controls.TouchControls
{
    public class ToggleTextButton : ToggleButton
    {
        private string _text, _textToggled;

        protected override void DrawContent(int left, int top, int width, int height, Style cStyle)
        {
            string toDraw = _isChecked ? _textToggled : _text;

            Font textFont = cStyle.ButtonFont;

            _desktopOwner._screenBuffer.DrawTextInRect(_text, left, top + (height - textFont.Height) / 2, width, textFont.Height, Bitmap.DT_AlignmentCenter | Bitmap.DT_TrimmingCharacterEllipsis,
                _enabled ? (_pressed || _isChecked ? cStyle.ButtonPressedTextColor : cStyle.ButtonEnabledTextColor) : cStyle.ButtonDisabledTextColor, textFont);
        }

        public ToggleTextButton(string text, int x, int y, int width, int height)
            : this(text, text, x, y, width, height) { }

        public ToggleTextButton(string text, string textToggled, int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            _text = text;
            _textToggled = textToggled;
        }
    }
}
