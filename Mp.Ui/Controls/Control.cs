using System;

using Microsoft.SPOT;

using Mp.Ui.Primitives;
using Mp.Ui.Controls.Containers;

namespace Mp.Ui.Controls
{
    public abstract class Control : IDisposable
    {
        private static int _idUtil = 0;

        #region internal members
        internal int _offsetX, _offsetY;
        internal Container _parent;
        internal Point _padding;
        internal virtual Desktop DesktopOwner { set { _desktopOwner = value; } }

        internal Desktop _desktopOwner;
        internal int _x, _y, _width, _height;
        internal bool _visible, _enabled, _suspended;
        #endregion

        #region protected members
        protected Bitmap ScreenBuffer { get { return _desktopOwner._screenBuffer; } }
        #endregion

        #region public properties
        public virtual int X
        {
            get { return _x; }
            set { _x = value; RefreshParent(); }
        }
        public virtual int Y
        {
            get { return _y; }
            set { _y = value; RefreshParent(); }
        }

        public virtual int Width
        {
            get { return _width; }
            set { _width = value; RefreshParent(); }
        }
        public virtual int Height
        {
            get { return _height; }
            set { _height = value; RefreshParent(); }
        }

        public object Tag { get; set; }

        public int ScreenLeft { get { return _x + _offsetX; } }
        public int ScreenTop { get { return _y + _offsetY; } }
        public int ScreenRight { get { return _x + _offsetX + _width; } }
        public int ScreenBottom { get { return _y + _offsetY + _height; } }

        public bool Visible
        {
            get { return _visible; }
            set
            {
                _visible = value;
                RefreshParent();
            }
        }
        public virtual bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                Refresh();
            }
        }
        public bool Suspended
        {
            get { return _suspended; }
            set
            {
                _suspended = value;
                if (!_suspended) Refresh();
            }
        }

        public Container Parent { get { return _parent; } }
        public int Id { get; private set; }
        #endregion

        #region functionality
        public bool ContainsScreenPoint(int x, int y)
        {
            return (x >= ScreenLeft && x < ScreenRight && y >= ScreenTop && y < ScreenBottom);
        }

        public virtual void Dispose()
        {
            _parent = null;
            _desktopOwner = null;
        }

        protected void RefreshParent()
        {
            if (_parent != null) _parent.Refresh();
        }

        protected virtual bool CanRefresh()
        {
            if (_suspended || !_visible || _parent == null || _desktopOwner == null) return false;
            return _parent.CanRefresh();
        }

        internal virtual void Refresh(bool flush)
        {
            if (flush) { _desktopOwner._screenBuffer.Flush(ScreenLeft, ScreenTop, _width, _height); }
        }

        public void Refresh()
        {
            Refresh(true);
        }
        #endregion

        #region constructors
        internal Control()
        {
            _padding = Point.Zero;
            _visible = _enabled = true;
            _suspended = false;
            Id = _idUtil++;
        }

        internal Control(int x, int y, int width, int height)
            : this()
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }
        #endregion
    }
}
