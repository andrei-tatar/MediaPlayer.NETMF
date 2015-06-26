using System;
using Microsoft.SPOT;
using Mp.Ui.Controls.TouchControls;
using System.Collections;
using Mp.Ui;
using Microsoft.SPOT.Presentation.Media;
using Mp.Ui.Managers;
using Mp.App.Resources;
using Mp.Ui.Primitives;

namespace Mp.App.Controls
{
    class FsList : ListBoxBase
    {
        protected override int ItemHeight { get { return 30; } }

        private static Bitmap fsMusic, fsNone, fsFolder, fsBack;

        #region members
        private int _selIndex;
        private FsItem _playing;
        #endregion

        #region properties
        public int SelectedIndex { get { return _selIndex; } }
        public FsItem SelectedItem { get { return _selIndex == -1 ? null : (FsItem)GetItemAt(_selIndex); } }
        public FsItem Folder { get; set; }
        public event UiEventHandler OnDoubleClick;

        public FsItem CurrentPlaying
        {
            get { return _playing; }
            set
            {
                if (_playing == value) return;
                _playing = value;
                if (_items.Contains(_playing)) Refresh();
            }
        }
        #endregion

        #region functionality
        public void AddItem(FsItem item)
        {
            base.AddItem_(item);
        }

        public void AddItems(IEnumerable items)
        {
            base.AddItems_(items);
        }

        public void RemoveItem(FsItem item)
        {
            _selIndex = -1;
            base.RemoveItem_(item);
        }

        public FsItem GetItemAt(int index)
        {
            return (FsItem)base.GetItemAt_(index);
        }

        protected override bool OnItemTouchDown(int x, int y, int index, object item)
        {
            if (index == -1) return false;

            if (_selIndex != index)
            {
                _selIndex = index;
                Refresh();
            }
            else
            {
                if (OnDoubleClick != null) OnDoubleClick(this);
            }
            return false;
        }

        public void ClearItems()
        {
            _selIndex = -1;
            base.ClearItems_();
        }

        public new void RefreshItem(int index)
        {
            base.RefreshItem(index);
        }

        private Font nameFont = Fonts.ArialMediumBold;
        private Font textFont = StyleManager.CurrentStyle.LabelFont;

        protected override void DrawItem(Bitmap screen, int x, int y, int width, int height, ref int index, object it, Style cStyle)
        {
            bool selected = _selIndex == index;
            FsItem item = (FsItem)it;

            if (selected)
                screen.DrawRectangle(Colors.Blue, 0, x, y, width, height, 0, 0,
                    cStyle.ListBoxSelectedItemBack1, x, y,
                    cStyle.ListBoxSelectedItemBack2, x, y + height, Enabled ? (ushort)256 : (ushort)128);

            Color textColor = _playing == it ? Colors.Green : (Enabled ? (selected ? cStyle.TextBoxPressedTextColor : cStyle.TextBoxEnabledTextColor) : cStyle.TextBoxDisabledTextColor);
            Bitmap toDraw = item.IsBack ? fsBack : (item.IsDirectory ? fsFolder : (item.IsMusicFile ? fsMusic : fsNone));

            screen.DrawImage(x + 5, y + (ItemHeight - 16) / 2, toDraw, 0, 0, 16, 16);
            screen.DrawText(item.Name, nameFont, textColor, x + 26, y + (ItemHeight - nameFont.Height) / 2);
        }
        #endregion

        public FsList(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            _selIndex = -1;

            if (fsNone == null) fsNone = Images.GetBitmap(Images.BitmapResources.fsNone);
            if (fsMusic == null) fsMusic = Images.GetBitmap(Images.BitmapResources.fsMusic);
            if (fsFolder == null) fsFolder = Images.GetBitmap(Images.BitmapResources.fsFolder);
            if (fsBack == null) fsBack = Images.GetBitmap(Images.BitmapResources.fsBack);
        }
    }
}
