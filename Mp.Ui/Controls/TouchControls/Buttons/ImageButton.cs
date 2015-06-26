using System;

using Microsoft.SPOT;

namespace Mp.Ui.Controls.TouchControls.Buttons
{
    public sealed class ImageButton : Button
    {
        private Bitmap _image;

        public Bitmap Image { get { return _image; } set { if (_image == value) return; _image = value; Refresh(); } }

        protected override void DrawContent(int left, int top, int width, int height, Style cStyle)
        {
            if (_image == null) return;

            int imgWidth = _image.Width, imgHeight = _image.Height;
            _desktopOwner._screenBuffer.DrawImage(left + (width - imgWidth) / 2, top + (height - imgHeight) / 2, _image, 0, 0, imgWidth, imgHeight, _enabled ? (ushort)256 : (ushort)100);
        }

        public ImageButton()
            : this(null, 0, 0, 50, 25)
        { }

        public ImageButton(Bitmap Image, int X, int Y, int Width, int Height)
            : base(X, Y, Width, Height)
        {
            _image = Image;
        }

        public override void Dispose()
        {
            if (_image != null)
            {
                _image.Dispose();
                _image = null;
            }
            base.Dispose();
        }
    }
}
