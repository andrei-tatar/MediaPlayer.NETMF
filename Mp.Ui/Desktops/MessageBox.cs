using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Mp.Ui.Managers;
using Mp.Ui.Resources;
using Mp.Ui.Controls;
using Mp.Ui.Controls.TouchControls.Buttons;

namespace Mp.Ui.Desktops
{
    public class MessageBox : ModalDesktop
    {
        #region members
        const int margin = 5;
        const int butHeight = 35;

        private MessageBoxResult _result;
        protected string _text, _title;
        private Bitmap _img;
        #endregion

        #region functionality
        public MessageBoxResult Show()
        {
            base.Show(true);
            return _result;
        }

        protected override void RefreshBackground(int x, int y, int width, int height)
        {
            base.RefreshBackground(x, y, width, height);

            Style cStyle = StyleManager.CurrentStyle;
            _screenBuffer.DrawRectangle(cStyle.BackgroundBorderColor, 0, x, y + height - (butHeight + margin * 2), width, butHeight + margin * 2, 0, 0,
                cStyle.BackgroundBorderColor, x, y, cStyle.BackgroundBorderColor, x, y, 256);

            int _x = x + margin, _y = y + margin;

            Font titleFont = Fonts.ArialBold;

            int titleWidth, titleHeight;
            titleFont.ComputeTextInRect(_title, out titleWidth, out titleHeight);

            if (_img != null)
            {
                _screenBuffer.DrawImage(_x, _y, _img, 0, 0, _img.Width, _img.Height);
                _x += _img.Width + margin;
                _y += _img.Height / 2 - titleHeight / 2;
            }

            _screenBuffer.DrawTextInRect(_title, _x, _y, titleWidth, titleHeight, Bitmap.DT_TrimmingWordEllipsis, cStyle.LabelEnabledColor, titleFont);
            _y = y + (_img != null ? _img.Height : titleHeight) + 2 * margin;


            _screenBuffer.DrawLine(Colors.GetMedianColor(cStyle.BackgroundColor, cStyle.BackgroundBorderColor), 1, x + 1, _y, x + width - 2, _y);
            _x = x + margin;

            Font textFont = Fonts.Arial;
            int rWidth, rHeight;
            textFont.ComputeTextInRect(_text, out rWidth, out rHeight, _width - 2 * margin);

            int availableHeight = (y + _mHeight) - _y - butHeight - 2 * margin;
            _screenBuffer.DrawTextInRect(_text, _x, _y + (availableHeight - rHeight) / 2, _mWidth - 2 * margin, 0, Bitmap.DT_IgnoreHeight | Bitmap.DT_AlignmentCenter | Bitmap.DT_WordWrap, cStyle.LabelEnabledColor, textFont);
        }

        private void AddButtons(MessageBoxButtons buttons)
        {
            int aux = (int)buttons;
            int noButtons = 0;
            while (aux != 0)
            {
                if ((aux % 2) == 1) noButtons++;
                aux = aux / 2;
            }

            int butWidth = (_mWidth - margin * (noButtons + 1)) / noButtons;

            int x = margin, y = _mHeight - butHeight - margin;

            if ((buttons & MessageBoxButtons.Ok) != 0)
            {
                TextButton butOk = new TextButton("Ok", x, y, butWidth, butHeight) { Tag = MessageBoxResult.Ok };
                butOk.ButtonPressed += but_ButtonPressed;
                base.AddChild(butOk);
                x += butWidth + margin;
            }
            if ((buttons & MessageBoxButtons.Yes) != 0)
            {
                TextButton butYes = new TextButton("Yes", x, y, butWidth, butHeight) { Tag = MessageBoxResult.Yes };
                butYes.ButtonPressed += but_ButtonPressed;
                base.AddChild(butYes);
                x += butWidth + margin;
            }
            if ((buttons & MessageBoxButtons.No) != 0)
            {
                TextButton butNo = new TextButton("No", x, y, butWidth, butHeight) { Tag = MessageBoxResult.No };
                butNo.ButtonPressed += but_ButtonPressed;
                base.AddChild(butNo);
                x += butWidth + margin;
            }
            if ((buttons & MessageBoxButtons.Retry) != 0)
            {
                TextButton butRetry = new TextButton("Retry", x, y, butWidth, butHeight) { Tag = MessageBoxResult.Retry };
                butRetry.ButtonPressed += but_ButtonPressed;
                base.AddChild(butRetry);
                x += butWidth + margin;
            }
            if ((buttons & MessageBoxButtons.Cancel) != 0)
            {
                TextButton butCancel = new TextButton("Cancel", x, y, butWidth, butHeight) { Tag = MessageBoxResult.Cancel };
                butCancel.ButtonPressed += but_ButtonPressed;
                base.AddChild(butCancel);
                x += butWidth + margin;
            }
        }

        public override void Dispose()
        {
            if (_img != null)
            {
                _img.Dispose();
                _img = null;
            }
            base.Dispose();
        }
        #endregion

        #region event handlers
        private void but_ButtonPressed(Controls.Control sender)
        {
            _result = (MessageBoxResult)sender.Tag;
            Close();
        }
        #endregion

        #region constructor
        public MessageBox(string text, string title, MessageBoxButtons buttons = MessageBoxButtons.Ok, MessageBoxIcons icon = MessageBoxIcons.None, int height = 160)
            : this(text, title, buttons, icon, DesktopManager.ScreenWidth / 2, height)
        { }

        protected MessageBox(string text, string title, MessageBoxButtons buttons = MessageBoxButtons.Ok, MessageBoxIcons icon = MessageBoxIcons.None, int width = 240, int height = 160)
            : base(width, height)
        {
            _result = MessageBoxResult.Cancel;
            _text = text;
            _title = title;

            AddButtons(buttons);

            switch (icon)
            {
                case MessageBoxIcons.Error: _img = Images.GetBitmap(Images.BitmapResources.msg_error); break;
                case MessageBoxIcons.Delete: _img = Images.GetBitmap(Images.BitmapResources.msg_delete); break;
                case MessageBoxIcons.Information: _img = Images.GetBitmap(Images.BitmapResources.msg_info); break;
                case MessageBoxIcons.Question: _img = Images.GetBitmap(Images.BitmapResources.msg_question); break;
                case MessageBoxIcons.Save: _img = Images.GetBitmap(Images.BitmapResources.msg_save); break;
                case MessageBoxIcons.Search: _img = Images.GetBitmap(Images.BitmapResources.msg_search); break;
                default: _img = null; break;
            }
        }
        #endregion
    }

    public enum MessageBoxIcons
    {
        None,
        Error,
        Delete,
        Information,
        Question,
        Save,
        Search
    }

    [Flags]
    public enum MessageBoxButtons : int
    {
        Ok = 0x01,
        Yes = 0x02,
        No = 0x04,
        Retry = 0x08,
        Cancel = 0x10,

        OkCancel = Ok | Cancel,
        YesNo = Yes | No,
        YesNoCancel = YesNo | Cancel,
        RetryCancel = Retry | Cancel,
    }

    [Flags]
    public enum MessageBoxResult : int
    {
        Ok = 0x01,
        Yes = 0x02,
        No = 0x04,
        Retry = 0x08,
        Cancel = 0x10,
    }
}
