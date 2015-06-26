using System;
using System.Collections;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Mp.Ui.Controls.TouchControls
{
    public class ListBox : ListBoxBase
    {
        #region members
        protected override int ItemHeight { get { return 30; } }
        private ArrayList _selectedItems = new ArrayList();
        private bool _allowMultipleSelection;
        private int _selIndex;
        #endregion

        #region properties
        public int SelectedIndex { get { return _selIndex; } }
        public object SelectedItem { get { return _selIndex == -1 ? null : GetItemAt(_selIndex); } }
        public IEnumerable SelectedItems { get { return _selectedItems; } }
        #endregion

        #region functionality
        public bool AllowMultipleSelection
        {
            get { return _allowMultipleSelection; }
            set
            {
                _allowMultipleSelection = value;
                if (!_allowMultipleSelection)
                    while (_selectedItems.Count > 1) _selectedItems.RemoveAt(1);
                Refresh();
            }
        }

        public void AddItem(object item)
        {
            base.AddItem_(item);
        }

        public void AddItems(IEnumerable items)
        {
            base.AddItems_(items);
        }

        public void RemoveItem(object item)
        {
            _selectedItems.Remove(item);
            if (_selectedItems.Count != 0)
                _selIndex = GetIndexOf(_selectedItems[0]);
            else
                _selIndex = -1;
            base.RemoveItem_(item);
        }

        public object GetItemAt(int index)
        {
            return base.GetItemAt_(index);
        }

        protected override bool OnItemTouchDown(int x, int y, int index, object item)
        {
            if (index == -1) return false;

            if (_allowMultipleSelection)
            {
                if (x > _width - ItemHeight - 6)
                {
                    if (_selectedItems.Contains(item))
                        _selectedItems.Remove(item);
                    else
                        _selectedItems.Add(item);
                    if (_selectedItems.Count != 0)
                        _selIndex = GetIndexOf(_selectedItems[0]);
                    else
                        _selIndex = -1;
                    Refresh();
                    return true;
                }

                return false;
            }
            else
            {
                if (_selIndex != index)
                {
                    _selectedItems.Clear();
                    _selectedItems.Add(item);
                    _selIndex = index;
                    Refresh();
                }
                return false;
            }
        }

        public void ClearItems()
        {
            _selectedItems.Clear();
            base.ClearItems_();
        }

        protected override void DrawItem(Bitmap screen, int x, int y, int width, int height, ref int index, object item, Style cStyle)
        {
            bool selected = _allowMultipleSelection ? _selectedItems.Contains(item) : _selIndex == index;

            if (selected)
            {
                screen.DrawRectangle(Colors.Blue, 0, x, y, width, height, 0, 0,
                    cStyle.ListBoxSelectedItemBack1, x, y,
                    cStyle.ListBoxSelectedItemBack2, x, y + height, _enabled ? (ushort)256 : (ushort)128);
            }

            Font textFont = cStyle.LabelFont;
            Color textColor = _enabled ? (selected ? cStyle.TextBoxPressedTextColor : cStyle.TextBoxEnabledTextColor) : cStyle.TextBoxDisabledTextColor;
            screen.DrawTextInRect(item.ToString(), x + 5, y + (height - textFont.Height) / 2, width - 10, height, Bitmap.DT_AlignmentLeft, textColor, textFont);

            if (_allowMultipleSelection)
            {
                const int chkMargin = 3;

                int cX = x + width - ItemHeight,
                    cY = y + chkMargin,
                    cWidth = ItemHeight - chkMargin * 2,
                    cHeight = cWidth;

                screen.DrawRectangle(
                    _enabled ? cStyle.CheckBoxEnabledBorder : cStyle.CheckBoxDisabledBorder, 1, cX, cY, cWidth, cHeight, 0, 0,
                    _enabled ? cStyle.CheckBoxEnabledBack1 : cStyle.CheckBoxDisabledBack1, cX, cY,
                    _enabled ? cStyle.CheckBoxEnabledBack2 : cStyle.CheckBoxDisabledBack2, cX, cY + cHeight, 256);

                if (selected)
                {
                    const int margin = 4;
                    Color crossColor = _enabled ? cStyle.CheckBoxEnabledCross : cStyle.CheckBoxDisabledCross;

                    screen.DrawLine(crossColor, 2, cX + margin, cY + margin, cX + cWidth - margin, cY + cHeight - margin - 1);
                    screen.DrawLine(crossColor, 2, cX + cWidth - margin, cY + margin, cX + margin, cY + cHeight - margin - 1);
                }
            }
        }

        public override void Dispose()
        {
            _selectedItems.Clear();
            _selectedItems = null;
            base.Dispose();
        }
        #endregion

        #region constructors
        public ListBox()
            : this(0, 0, 100, 100)
        { }

        public ListBox(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            _allowMultipleSelection = true;
            _selIndex = -1;
        }
        #endregion
    }
}
