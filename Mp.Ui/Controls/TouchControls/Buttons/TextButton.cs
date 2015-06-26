using System;

using Mp.Ui.Managers;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Mp.Ui.Controls.TouchControls.Buttons
{
    public sealed class TextButton : Button
    {
        private string _text;
        private Font _font;
        private Color _textColor;
        private bool _useStyleColor;

        public string Text { get { return _text; } set { _text = value; Refresh(); } }
        public Font Font { get { return _font; } set { _font = value; Refresh(); } }
        public Color Color { get { return _textColor; } set { _textColor = value; if (!_useStyleColor) Refresh(); } }
        public bool UseStyleColor { get { return _useStyleColor; } set { _useStyleColor = value; Refresh(); } }

        protected override void DrawContent(int left, int top, int width, int height, Style cStyle)
        {
            Font textFont = _font ?? cStyle.ButtonFont;

            _desktopOwner._screenBuffer.DrawTextInRect(_text, left, top + (height - textFont.Height) / 2, width, textFont.Height, Bitmap.DT_AlignmentCenter | Bitmap.DT_TrimmingCharacterEllipsis,
                _enabled ? (_useStyleColor ? (_pressed ? cStyle.ButtonPressedTextColor : cStyle.ButtonEnabledTextColor) : _textColor) :
                cStyle.ButtonDisabledTextColor, textFont);
        }

        public TextButton()
            : this(string.Empty, 0, 0, 50, 25)
        { }

        public TextButton(string Text, int X, int Y, int Width, int Height)
            : base(X, Y, Width, Height)
        {
            _font = null;
            _text = Text;
            _textColor = StyleManager.CurrentStyle.ButtonEnabledTextColor;
            _useStyleColor = true;
        }

        public override void Dispose()
        {
            _font = null;
            base.Dispose();
        }
    }
}
