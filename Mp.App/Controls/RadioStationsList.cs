using System;
using Microsoft.SPOT;
using Mp.Ui.Controls.TouchControls;
using System.Collections;
using Mp.Ui;
using Microsoft.SPOT.Presentation.Media;
using Mp.Ui.Managers;

namespace Mp.App.Controls
{
    class RadioStationsList : ListBoxBase
    {
        protected override int ItemHeight { get { return 40; } }

        #region members
        private int _selIndex;
        #endregion

        #region properties
        public int SelectedIndex { get { return _selIndex; } }
        public RadioStationItem SelectedItem { get { return _selIndex == -1 ? null : (RadioStationItem)GetItemAt(_selIndex); } }
        #endregion

        #region functionality
        public void AddItem(RadioStationItem item)
        {
            base.AddItem_(item);
        }

        public void AddItems(IEnumerable items)
        {
            base.AddItems_(items);
        }

        public void RemoveItem(RadioStationItem item)
        {
            _selIndex = -1;
            base.RemoveItem_(item);
        }

        public RadioStationItem GetItemAt(int index)
        {
            return (RadioStationItem)base.GetItemAt_(index);
        }

        protected override bool OnItemTouchDown(int x, int y, int index, object item)
        {
            if (index == -1) return false;

            if (_selIndex != index)
            {
                _selIndex = index;
                Refresh();
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

        Font nameFont = Fonts.ArialMediumBold;
        Font textFont = StyleManager.CurrentStyle.LabelFont;

        protected override void DrawItem(Bitmap screen, int x, int y, int width, int height, ref int index, object it, Style cStyle)
        {
            bool selected = _selIndex == index;
            RadioStationItem item = (RadioStationItem)it;

            if (selected)
                screen.DrawRectangle(Colors.Blue, 0, x, y, width, height, 0, 0,
                    cStyle.ListBoxSelectedItemBack1, x, y,
                    cStyle.ListBoxSelectedItemBack2, x, y + height, Enabled ? (ushort)256 : (ushort)128);

            Color textColor = item.IsConnecting ? Colors.Yellow : (item.IsPlaying ? Colors.LightGreen : (Enabled ? (selected ? cStyle.TextBoxPressedTextColor : cStyle.TextBoxEnabledTextColor) : cStyle.TextBoxDisabledTextColor));
            screen.DrawText(item.Name, nameFont, textColor, x + 5, y + 2);
            screen.DrawText(item.Address, textFont, textColor, x + 5, y + 2 + nameFont.Height + 2);
        }
        #endregion

        public RadioStationsList(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            _selIndex = -1;
        }
    }
}
