using Microsoft.SPOT;

using System;
using System.Collections;

using Mp.Ui.Managers;
using Mp.Ui.Resources;
using Mp.Ui.Desktops;
using Mp.Ui.Primitives;

namespace Mp.Ui.Controls.TouchControls
{
    public class ComboBox : TouchControl
    {
        #region static members
        private static Bitmap _arrowImage = null;
        private static int _arrowImageInUse = 0;
        #endregion

        #region members
        private ArrayList _items;
        private int _selIndex;
        private object _selItem;
        private bool _pressed;
        #endregion

        #region events
        public event UiEventHandler OnSelectedItemChanged;
        #endregion

        #region properties
        public IEnumerable Items { get { return _items; } }

        public int ItemsCount { get { return _items.Count; } }

        public int SelectedIndex
        {
            get { return _selIndex; }
            set
            {
                if (value < -1 || value >= _items.Count) throw new ArgumentOutOfRangeException("Invalid index");
                _selIndex = value;
                _selItem = _selIndex == -1 ? null : _items[_selIndex];
                Refresh();
                if (OnSelectedItemChanged != null) OnSelectedItemChanged(this);
            }
        }

        public object SelectedItem
        {
            get { return _items[SelectedIndex]; }
            set
            {
                if (value == null)
                {
                    _selItem = null;
                    _selIndex = -1;
                }
                else
                {
                    int newIndex = _items.IndexOf(value);
                    if (newIndex < 0) throw new ArgumentException("Object not found in the items list");

                    _selIndex = newIndex;
                    _selItem = value;
                }

                Refresh();

                if (OnSelectedItemChanged != null) OnSelectedItemChanged(this);
            }
        }
        #endregion

        #region items management functionality (add/remove/clear)
        public void AddItem(object item)
        {
            _items.Add(item);
            if (_selItem == null && _items.Count == 1)
            {
                _selItem = item;
                _selIndex = 0;

                Refresh();
                if (OnSelectedItemChanged != null) OnSelectedItemChanged(this);
                return;
            }

            Refresh();
        }

        public void RemoveItem(object item)
        {
            if (!_items.Contains(item)) return;

            _items.Remove(item);
            if (_items.Count == 0)
            {
                _selItem = null;
                _selIndex = -1;

                Refresh();
                if (OnSelectedItemChanged != null) OnSelectedItemChanged(this);

                return;
            }
            else if (_selIndex >= _items.Count)
            {
                _selIndex = _items.Count - 1;
                _selItem = _items[_selIndex];

                Refresh();
                if (OnSelectedItemChanged != null) OnSelectedItemChanged(this);

                return;
            }
            else
            {
                _selItem = _items[_selIndex];
            }

            Refresh();
        }

        public void RemoveItemAt(int index)
        {
            if (index >= 0 && index < _items.Count) RemoveItem(_items[index]);
        }

        public void ClearItems()
        {
            _items.Clear();
            _selIndex = -1;
            _selItem = null;
            Refresh();
            if (OnSelectedItemChanged != null) OnSelectedItemChanged(this);
        }

        public object GetItemAt(int index)
        {
            return index >= 0 && index < _items.Count ? _items[index] : null;
        }

        public int GetIndexOf(object item)
        {
            return _items.IndexOf(item);
        }
        #endregion

        #region functionality
        internal override void Refresh(bool flush)
        {
            if (!CanRefresh()) return;

            int left = ScreenLeft, top = ScreenTop;

            const int margin = 5;

            Style cStyle = StyleManager.CurrentStyle;

            _desktopOwner._screenBuffer.SetClippingRectangle(left, top, _width, _height);
            _desktopOwner._screenBuffer.DrawRectangle(
                _enabled ? (_pressed ? cStyle.ComboBoxPressedBorder : cStyle.ComboBoxEnabledBorder) : cStyle.ComboBoxDisabledBorder, 1, left, top, _width, _height, 0, 0,
                _enabled ? (_pressed ? cStyle.ComboBoxPressedBack1 : cStyle.ComboBoxEnabledBack1) : cStyle.ComboBoxDisabledBack1, left, top,
                _enabled ? (_pressed ? cStyle.ComboBoxPressedBack2 : cStyle.ComboBoxEnabledBack2) : cStyle.ComboBoxDisabledBack2, left, top + _height, 256);

            _desktopOwner._screenBuffer.DrawImage(left + _width - margin - _arrowImage.Width, top + (_height - _arrowImage.Height) / 2,
                _arrowImage, 0, 0, _arrowImage.Width, _arrowImage.Height, _enabled ? (ushort)256 : (ushort)28);

            if (_selItem != null)
            {
                _desktopOwner._screenBuffer.DrawTextInRect(_selItem.ToString(), left + margin, top + (_height - cStyle.ComboBoxFont.Height) / 2, _width - margin * 2 - _arrowImage.Width, _height,
                    Bitmap.DT_AlignmentLeft | Bitmap.DT_TrimmingCharacterEllipsis,
                    _enabled ? (_pressed ? cStyle.ComboBoxPressedTextColor : cStyle.ComboBoxEnabledTextColor) : cStyle.ComboBoxDisabledTextColor, cStyle.ComboBoxFont);
            }

            base.Refresh(flush);
        }

        internal override bool OnTouchDown(int x, int y)
        {
            _pressed = CaptureTouch();
            if (!_pressed) return false;
            Refresh();
            return true;
        }

        internal override bool OnTouchMove(int x, int y)
        {
            return _pressed;
        }

        internal override bool OnTouchUp(int x, int y)
        {
            if (_pressed)
            {
                _pressed = false;
                Refresh();
                ReleaseCapture();

                if (ContainsScreenPoint(x, y))
                {
                    ComboBoxListViewDesktop listDesktop = new ComboBoxListViewDesktop(this);
                    listDesktop.Show();
                    listDesktop.DesktopClosed += (s) =>
                    {
                        listDesktop.Dispose();
                    };
                }

                return true;
            }
            return false;
        }

        public override void Dispose()
        {
            _items.Clear();

            _arrowImageInUse--;
            if (_arrowImageInUse == 0 && _arrowImage != null)
            {
                _arrowImage.Dispose();
                _arrowImage = null;
            }

            base.Dispose();
        }
        #endregion

        #region constructors
        public ComboBox()
            : this(0, 0, 100, 25)
        { }

        public ComboBox(int X, int Y, int Width, int Height)
            : base(X, Y, Width, System.Math.Max(Height, 20))
        {
            _items = new ArrayList();
            _selIndex = -1;
            _pressed = false;

            if (_arrowImage == null)
                _arrowImage = Images.GetBitmap(Images.BitmapResources.cmb_arrow);
            _arrowImageInUse++;
        }
        #endregion
    }
}
