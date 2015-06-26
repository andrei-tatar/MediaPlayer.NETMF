using System;
using System.Collections;

using Microsoft.SPOT;

using Mp.Ui.Controls.Containers;

namespace Mp.Ui.Controls
{
    public abstract class Container : TouchControl
    {
        #region members
        protected readonly ArrayList _childs;
        #endregion

        #region properties
        public IEnumerable Childs { get { return _childs; } }

        public new virtual int X
        {
            get { return _x; }
            set
            {
                _x = value;
                foreach (Control child in _childs)
                    child._offsetX = ScreenLeft + _padding.X;
                RefreshParent();
            }
        }

        public new virtual int Y
        {
            get { return _y; }
            set
            {
                _y = value;
                foreach (Control child in _childs)
                    child._offsetY = ScreenTop + _padding.Y;
                RefreshParent();
            }
        }

        internal override Desktop DesktopOwner
        {
            set
            {
                base.DesktopOwner = value;
                foreach (Control child in _childs)
                    child.DesktopOwner = value;
            }
        }
        #endregion

        #region functionality
        internal new virtual void Refresh(bool flush)
        {
            if (!CanRefresh()) return;
            foreach (Control child in _childs)
            {
                child.Refresh(false);
            }
            base.Refresh(flush);
        }

        public virtual void AddChild(Control control)
        {
            _childs.Add(control);
            control._parent = this;
            control.DesktopOwner = this._desktopOwner;
            control._offsetX = ScreenLeft + _padding.X;
            control._offsetY = ScreenTop + _padding.Y;
            Refresh();
        }

        public virtual void RemoveChild(Control control)
        {
            if (_childs.Contains(control))
            {
                _childs.Remove(control);
                control._parent = null;
                control.DesktopOwner = null;
                Refresh();
            }
        }

        public virtual void ClearChilds()
        {
            bool wasSuspended = _suspended;
            _suspended = true;
            while (_childs.Count != 0) RemoveChild((Control)_childs[0]);
            Suspended = wasSuspended;
        }

        public virtual Control FindChild(int id)
        {
            foreach (Control child in _childs)
            {
                if (child.Id == id) return child;
                if (child is Container)
                {
                    Control auxFound = ((Container)child).FindChild(id);
                    if (auxFound != null) return auxFound;
                }
            }
            return null;
        }

        internal override bool OnTouchDown(int x, int y)
        {
            for (int i = _childs.Count - 1; i >= 0; i--)
            {
                TouchControl child = _childs[i] as TouchControl;
                if (child == null || child._visible == false) continue;

                if (child.ContainsScreenPoint(x, y))
                    if (!child._suspended && child._visible && child._enabled) return child.OnTouchDown(x, y);
            }
            return false;
        }

        internal override bool OnTouchMove(int x, int y)
        {
            for (int i = _childs.Count - 1; i >= 0; i--)
            {
                TouchControl child = _childs[i] as TouchControl;
                if (child == null || child._visible == false) continue;

                if (child.ContainsScreenPoint(x, y))
                    if (!child._suspended && child._visible && child._enabled) return child.OnTouchMove(x, y);
            }
            return false;
        }

        internal override bool OnTouchUp(int x, int y)
        {
            for (int i = _childs.Count - 1; i >= 0; i--)
            {
                TouchControl child = _childs[i] as TouchControl;
                if (child == null || child._visible == false) continue;

                if (child.ContainsScreenPoint(x, y))
                    if (!child._suspended && child._visible && child._enabled) return child.OnTouchUp(x, y);
            }
            return false;
        }

        public override void Dispose()
        {
            foreach (Control child in _childs)
                child.Dispose();
            _childs.Clear();

            base.Dispose();
        }
        #endregion

        #region constructors
        public Container()
            : this(0, 0, 50, 50)
        { }

        public Container(int X, int Y, int Width, int Height)
            : base(X, Y, Width, Height)
        { _childs = new ArrayList(); }
        #endregion
    }
}
