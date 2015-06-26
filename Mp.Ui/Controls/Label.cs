using System;

using Mp.Ui.Managers;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Mp.Ui.Controls
{
    public sealed class Label : Control
    {
        #region members
        private string _text;
        private Font _font;
        private Color _textColor;
        private bool _useStyleColor;
        private bool _autoHeight;
        private bool _centerText;
        #endregion

        #region properties
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                if (_autoHeight)
                {
                    Font textFont = _font ?? StyleManager.CurrentStyle.LabelFont;
                    int _auxWidth;
                    textFont.ComputeTextInRect(_text, out _auxWidth, out _height, _width);
                }
                RefreshParent();
            }
        }
        public Font Font { get { return _font; } set { _font = value; RefreshParent(); } }
        public Color Color { get { return _textColor; } set { _textColor = value; if (!_useStyleColor) RefreshParent(); } }
        public bool UseStyleColor { get { return _useStyleColor; } set { _useStyleColor = value; RefreshParent(); } }
        public bool AutoHeight
        {
            get { return _autoHeight; }
            set { _autoHeight = value; Text = _text; }
        }
        public bool CenterText { get { return _centerText; } set { _centerText = value; RefreshParent(); } }
        #endregion

        #region functionality
        internal override void Refresh(bool flush)
        {
            if (!CanRefresh()) return;

            Style cStyle = StyleManager.CurrentStyle;
            Font textFont = _font ?? cStyle.LabelFont;

            int left = ScreenLeft, top = ScreenTop;
            _desktopOwner._screenBuffer.SetClippingRectangle(left, top, _width, _height);
            _desktopOwner._screenBuffer.DrawTextInRect(_text, left, top, _width, _height, Bitmap.DT_TrimmingCharacterEllipsis | (_centerText ? Bitmap.DT_AlignmentCenter : 0),
                _enabled ? (_useStyleColor ? cStyle.LabelEnabledColor : _textColor) : cStyle.LabelDisabledColor, textFont);

            base.Refresh(flush);
        }

        public override void Dispose()
        {
            _font = null;
            base.Dispose();
        }
        #endregion

        #region constructors
        public Label()
            : this(string.Empty, 0, 0, 50, 10)
        { }

        public Label(string Text, int X, int Y, int Width, int Height)
            : base(X, Y, Width, Height)
        {
            _font = null;
            _text = Text;
            _textColor = StyleManager.CurrentStyle.LabelEnabledColor;
            _useStyleColor = true;
            _autoHeight = false;
        }
        #endregion
    }
}
