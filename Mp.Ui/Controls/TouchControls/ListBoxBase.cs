using System;
using System.Threading;
using System.Collections;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Mp.Ui.Primitives;
using Mp.Ui.Managers;

namespace Mp.Ui.Controls.TouchControls
{
    public abstract class ListBoxBase : TouchControl
    {
        #region properties
        protected abstract int ItemHeight { get; }
        public override int Height
        {
            get { return base.Height; }
            set
            {
                _hasExactHeight = (value - 2) % ItemHeight == 0;
                _itemsMaxInView = (value - 2) / ItemHeight + (_hasExactHeight ? 1 : 2);

                int lastPossibleInView = System.Math.Max(0, _items.Count - _itemsMaxInView + (_hasExactHeight ? 0 : 1));
                if (_itemsFirstInView > lastPossibleInView)
                {
                    _itemsFirstInView = lastPossibleInView;
                    _firstItemDelta = ItemHeight - (value - 2) % ItemHeight;
                }

                _height = value;
                RecomputeScrollbar();
                RefreshParent();
            }
        }
        public int ItemsCount { get { return _items.Count; } }
        public IEnumerable Items { get { return _items; } }
        #endregion

        #region listbox functionality members
        protected ArrayList _items;

        private int _firstItemDelta;
        private int _itemsFirstInView;
        private int _itemsMaxInView;
        private bool _hasExactHeight;

        private bool _pressed = false;
        #endregion

        #region scroll bar members
        private bool _showScrollBar;
        private int _scrollBarY;
        private int _scrollBarHeight;
        private Timer _scrollHideTimer;
        #endregion

        #region inertial scroll members
        private const int _scrollSpeedMultiple = 2;
        private const int _scrollTimerPeriod = 40;
        private const int _scrollDeceleration = 2;
        private const int _scrollThresholdSpeed = 5;

        private int _lastY = -1;
        private int _scrollSpeed;
        private DateTime _lastSpeedGetTime;
        private Timer _scrollTimer;
        #endregion

        #region items management functionality (add/remove/clear)
        public void RemoveItemAt(int index)
        {
            if (index >= 0 && index < _items.Count) RemoveItem_(_items[index]);
        }

        protected void AddItem_(object item)
        {
            _items.Add(item);
            RecomputeScrollbar();
            Refresh();
        }

        protected void AddItems_(IEnumerable items)
        {
            foreach (object item in items)
                _items.Add(item);
            RecomputeScrollbar();
            Refresh();
        }

        protected void RemoveItem_(object item)
        {
            if (!_items.Contains(item)) return;

            _items.Remove(item);

            int lastPossibleInView = _items.Count - _itemsMaxInView + (_hasExactHeight ? 0 : 1);
            if (_itemsFirstInView > lastPossibleInView && _itemsMaxInView <= _items.Count)
            {
                _firstItemDelta = ItemHeight - (_height - 2) % ItemHeight;
                _itemsFirstInView = lastPossibleInView;
            }
            else
            {
                _itemsFirstInView = 0;
                _firstItemDelta = 0;
            }
            RecomputeScrollbar();

            Refresh();
        }

        protected void ClearItems_()
        {
            _items.Clear();

            _itemsFirstInView = 0;
            _firstItemDelta = 0;
            RecomputeScrollbar();

            Refresh();
        }

        protected object GetItemAt_(int index)
        {
            return index >= 0 && index < _items.Count ? _items[index] : null;
        }

        protected int GetIndexOf(object item)
        {
            return _items.IndexOf(item);
        }
        #endregion

        #region functionality
        public bool IsItemInView(int index)
        {
            if (index < 0 || index >= _items.Count) return true;

            int itemHeight = ItemHeight;

            //check if already in view
            int yPos = (index - _itemsFirstInView) * itemHeight - _firstItemDelta;
            if (yPos >= 0 && yPos + itemHeight <= _height - 2) return true;

            return false;
        }

        public void BringItemIntoView(int index)
        {
            if (IsItemInView(index)) return;

            else if (index <= _itemsFirstInView)
            {
                _itemsFirstInView = System.Math.Min(index, _items.Count - _itemsMaxInView + (_hasExactHeight ? 0 : 1));
                _firstItemDelta = 0;
                RecomputeScrollbar();
                Refresh();
            }
            else if (index > _itemsFirstInView)
            {
                _itemsFirstInView = index - _itemsMaxInView + (_hasExactHeight ? 1 : 2);
                if (_itemsFirstInView < 0)
                {
                    _itemsFirstInView = 0;
                    _firstItemDelta = 0;
                }
                else
                {
                    int itemHeight = ItemHeight;
                    _firstItemDelta = itemHeight - (_height - 2) % itemHeight;
                }
                RecomputeScrollbar();
                Refresh();
            }
        }

        protected void ScrollByDelta(int delta)
        {
            if (_items.Count < _itemsMaxInView - (_hasExactHeight ? 0 : 1)) return;

            int itemHeight = ItemHeight;
            int newItemDelta = _firstItemDelta + delta;

            if (newItemDelta < 0)
            {
                _itemsFirstInView += newItemDelta / itemHeight;
                newItemDelta = newItemDelta % itemHeight + itemHeight;

                if (_itemsFirstInView < 0)
                {
                    _itemsFirstInView = 0;
                    newItemDelta = 0;

                    _scrollSpeed = 0;
                }
                else if (_itemsFirstInView != 0) _itemsFirstInView--;
                else
                {
                    newItemDelta = 0;
                    _scrollSpeed = 0;
                }
            }
            else if (newItemDelta > itemHeight)
            {
                _itemsFirstInView += newItemDelta / itemHeight;
                newItemDelta = newItemDelta % itemHeight;

                int lastPossibleInView = _items.Count - _itemsMaxInView + (_hasExactHeight ? 0 : 1);
                if (_itemsFirstInView > lastPossibleInView)
                {
                    _scrollSpeed = 0;
                    _itemsFirstInView = lastPossibleInView;
                    newItemDelta = itemHeight - (_height - 2) % itemHeight;
                }
                else if (_itemsFirstInView == lastPossibleInView)
                {
                    int lastDelta = itemHeight - (_height - 2) % itemHeight;
                    if (newItemDelta > lastDelta)
                    {
                        newItemDelta = lastDelta;
                        _scrollSpeed = 0;
                    }
                }
            }
            else
            {
                int lastPossibleInView = _items.Count - _itemsMaxInView + (_hasExactHeight ? 0 : 1);
                if (_itemsFirstInView == lastPossibleInView)
                {
                    int lastDelta = itemHeight - (_height - 2) % itemHeight;
                    if (newItemDelta > lastDelta)
                    {
                        newItemDelta = lastDelta;
                        _scrollSpeed = 0;
                    }
                }
            }

            _firstItemDelta = newItemDelta;

            RecomputeScrollbar();

            Refresh();
        }

        private void RecomputeScrollbar()
        {
            int itemHeight = ItemHeight;
            int totalItemsHeight = _items.Count * itemHeight;
            int availableHeight = _height - 2;
            if (totalItemsHeight > availableHeight)
            {
                _showScrollBar = true;

                const int minScrollSize = 20;

                int scrollY = _itemsFirstInView * itemHeight + _firstItemDelta;
                int maxScrollY = totalItemsHeight - availableHeight - 1;

                availableHeight -= 2;
                _scrollBarHeight = System.Math.Max(minScrollSize, availableHeight * availableHeight / (totalItemsHeight - 4));
                _scrollBarY = scrollY * (availableHeight - _scrollBarHeight) / maxScrollY;

                if (_scrollHideTimer == null) _scrollHideTimer = new Timer((o) =>
                    {
                        _showScrollBar = false;
                        Refresh();
                    }, null, Timeout.Infinite, Timeout.Infinite);
                _scrollHideTimer.Change(2000, Timeout.Infinite);
            }
            else
            {
                _showScrollBar = false;
            }
        }

        protected void RefreshItem(int index)
        {
            if (!CanRefresh() && !IsItemInView(index)) return;

            Bitmap screen = _desktopOwner._screenBuffer;

            int left = ScreenLeft, top = ScreenTop;
            Style cStyle = StyleManager.CurrentStyle;

            int x = left + 1, y = top + 1 - _firstItemDelta + (index - _itemsFirstInView) * ItemHeight;
            DrawItem(screen, x, y, _width - 2, ItemHeight, ref index, _items[index], cStyle);

            if (_showScrollBar)
            {
                screen.SetClippingRectangle(left, top, _width, _height);
                screen.DrawRectangle(Colors.Black, 0, left + _width - 4, top + 2 + _scrollBarY, 2, _scrollBarHeight, 0, 0,
                    cStyle.ListBoxScrollBarColor, 0, 0, cStyle.ListBoxScrollBarColor, 0, 0, 256);
            }

            base.Refresh();
        }

        internal override void Refresh(bool flush)
        {
            if (!CanRefresh()) return;

            Bitmap screen = _desktopOwner._screenBuffer;
            int left = ScreenLeft, top = ScreenTop;

            Style cStyle = StyleManager.CurrentStyle;

            screen.SetClippingRectangle(left, top, _width, _height);
            screen.DrawRectangle(
                _enabled ? cStyle.ListBoxEnabledBorder : cStyle.ListBoxDisabledBorder, 1, left, top, _width, _height, 0, 0,
                _enabled ? cStyle.ListBoxEnabledBack1 : cStyle.ListBoxDisabledBack1, left, top,
                _enabled ? cStyle.ListBoxEnabledBack2 : cStyle.ListBoxDisabledBack2, left, top + _height, 256);

            int itemHeight = ItemHeight;
            if (_items.Count > 0)
            {
                int itemWidth = _width - 2;

                int x = left + 1, y = top + 1;
                screen.SetClippingRectangle(x, y, itemWidth, _height - 2);
                y -= _firstItemDelta;

                int lastItemInView = System.Math.Min(_itemsFirstInView + _itemsMaxInView, _items.Count);
                for (int i = _itemsFirstInView; i < lastItemInView; i++, y += itemHeight)
                    DrawItem(screen, x, y, itemWidth, itemHeight, ref i, _items[i], cStyle);
            }

            if (_showScrollBar)
            {
                screen.SetClippingRectangle(left, top, _width, _height);
                screen.DrawRectangle(Colors.Black, 0, left + _width - 4, top + 2 + _scrollBarY, 2, _scrollBarHeight, 0, 0,
                    cStyle.ListBoxScrollBarColor, 0, 0, cStyle.ListBoxScrollBarColor, 0, 0, 256);
            }

            base.Refresh(flush);
        }

        protected abstract void DrawItem(Bitmap screen, int x, int y, int width, int height, ref int index, object item, Style cStyle);

        public override void Dispose()
        {
            if (_scrollTimer != null)
            {
                _scrollTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _scrollTimer.Dispose();
                _scrollTimer = null;
            }

            if (_scrollHideTimer != null)
            {
                _scrollHideTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _scrollHideTimer.Dispose();
                _scrollHideTimer = null;
            }

            _items.Clear();
            _items = null;

            base.Dispose();
        }
        #endregion

        #region touch functionality
        protected virtual bool OnItemTouchDown(int x, int y, int index, object item)
        { return false; }

        protected virtual bool OnItemTouchMove(int x, int y, int index, object item)
        { return false; }

        protected virtual bool OnItemTouchUp(int x, int y, int index, object item)
        { return false; }

        internal override bool OnTouchDown(int x, int y)
        {
            if (!CaptureTouch()) return false;

            int relativeY = (y - 1 + _firstItemDelta - ScreenTop);
            int itemIndex = relativeY / ItemHeight + _itemsFirstInView;

            _lastY = y;
            _scrollSpeed = 0;
            _lastSpeedGetTime = DateTime.Now;

            int auxItemIndex = itemIndex >= _items.Count ? -1 : itemIndex;
            if (OnItemTouchDown(x - ScreenLeft - 1, relativeY % ItemHeight, auxItemIndex, auxItemIndex == -1 ? null : _items[auxItemIndex])) return true;

            _pressed = true;
            return true;
        }

        internal override bool OnTouchMove(int x, int y)
        {
            if (_pressed)
            {
                DateTime cTime = DateTime.Now;
                int delta = _lastY - y;
                ScrollByDelta(delta);

                _scrollSpeed = (_scrollSpeed + delta * 120 / (cTime - _lastSpeedGetTime).Milliseconds) / 2;
                _lastY = y;
                _lastSpeedGetTime = cTime;
                return true;
            }

            int relativeY = (y - 1 + _firstItemDelta - ScreenTop);
            int itemIndex = relativeY / ItemHeight + _itemsFirstInView;
            if (itemIndex >= _items.Count) itemIndex = -1;
            return OnItemTouchMove(x - ScreenLeft - 1, relativeY % ItemHeight, itemIndex, itemIndex == -1 ? null : _items[itemIndex]);
        }

        internal override bool OnTouchUp(int x, int y)
        {
            ReleaseCapture();

            if (_pressed)
            {
                int delta = _lastY - y;
                DateTime cTime = DateTime.Now;
                _scrollSpeed = (_scrollSpeed + delta * 120 / (cTime - _lastSpeedGetTime).Milliseconds) / 2;
                _scrollSpeed *= _scrollSpeedMultiple;
            }

            try
            {
                if (_pressed)
                {
                    _pressed = false;
                    return true;
                }

                int relativeY = (y - 1 + _firstItemDelta - ScreenTop);
                int itemIndex = relativeY / ItemHeight + _itemsFirstInView;
                if (itemIndex >= _items.Count) itemIndex = -1;
                return OnItemTouchUp(x - ScreenLeft - 1, relativeY % ItemHeight, itemIndex, itemIndex == -1 ? null : _items[itemIndex]);
            }
            finally
            {
                if (_scrollTimer == null)
                    _scrollTimer = new Timer((o) =>
                    {
                        ScrollByDelta(_scrollSpeed / _scrollSpeedMultiple);

                        if (_scrollSpeed < 0) _scrollSpeed += _scrollDeceleration;
                        else _scrollSpeed -= _scrollDeceleration;

                        if (_scrollSpeed > -_scrollThresholdSpeed && _scrollSpeed < _scrollThresholdSpeed)
                        {
                            _scrollTimer.Change(Timeout.Infinite, Timeout.Infinite);
                            _scrollSpeed = 0;
                        }
                    }, null, Timeout.Infinite, Timeout.Infinite);

                if (_scrollSpeed < -_scrollThresholdSpeed * 2 || _scrollSpeed > _scrollThresholdSpeed * 2)
                    _scrollTimer.Change(0, _scrollTimerPeriod);
            }
        }
        #endregion

        #region constructors
        public ListBoxBase()
            : this(0, 0, 200, 100)
        { }

        public ListBoxBase(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            _items = new ArrayList();

            _itemsFirstInView = 0;

            _hasExactHeight = (_height - 2) % ItemHeight == 0;
            _itemsMaxInView = (_height - 2) / ItemHeight + (_hasExactHeight ? 1 : 2);
            _firstItemDelta = 0;
        }
        #endregion
    }
}
