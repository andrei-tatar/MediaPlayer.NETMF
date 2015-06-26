using System;
using Microsoft.SPOT;

namespace Mp.Ui.Controls.TouchControls
{
    public class ToggleImageButton : ToggleButton
    {
        private Bitmap _image, _imageToggled;

        public override void Dispose()
        {
            if (_image != null) _image.Dispose();
            if (_imageToggled != null) _imageToggled.Dispose();
            base.Dispose();
        }

        protected override void DrawContent(int left, int top, int width, int height, Style cStyle)
        {
            Bitmap toDraw = _isChecked ? _imageToggled : _image;
            int imgWidth = toDraw.Width, imgHeight = toDraw.Height;
            _desktopOwner._screenBuffer.DrawImage(left + (width - imgWidth) / 2, top + (height - imgHeight) / 2, toDraw, 0, 0, imgWidth, imgHeight, _enabled ? (ushort)256 : (ushort)100);
        }

        public ToggleImageButton(Bitmap image, int x, int y, int width, int height)
            : this(image, image, x, y, width, height) { }

        public ToggleImageButton(Bitmap image, Bitmap imageToggled, int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            this._image = image;
            this._imageToggled = imageToggled;
        }
    }
}
