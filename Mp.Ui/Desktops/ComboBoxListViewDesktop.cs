using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Mp.Ui.Primitives;
using Mp.Ui.Managers;
using Mp.Ui.Controls;
using Mp.Ui.Controls.TouchControls;
using Mp.Ui.Controls.TouchControls.Buttons;

namespace Mp.Ui.Desktops
{
    internal sealed class ComboBoxListViewDesktop : ModalDesktop
    {
        #region functionality
        public override void AddChild(Control control)
        { throw new NotSupportedException("Can not modify the desktop"); }
        public override void ClearChilds()
        { throw new NotSupportedException("Can not modify the desktop"); }
        public override Control FindChild(int id)
        { throw new NotSupportedException("Can not modify the desktop"); }
        public override void RemoveChild(Control control)
        { throw new NotSupportedException("Can not modify the desktop"); }

        public void Show()
        {
            base.Show(false);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion

        #region constructor
        public ComboBoxListViewDesktop(ComboBox cmbBox)
            : base(DesktopManager.ScreenWidth - 50, DesktopManager.ScreenHeight - 50)
        {
            _suspended = true;

            ComboBoxList _itemsList = new ComboBoxList(5, 5, _mWidth - 10, _mHeight - 40);
            foreach (object item in cmbBox.Items)
                _itemsList.AddItem(item);

            object initialSelectedItem = _itemsList.SelectedItem = cmbBox.SelectedItem;
            _itemsList.BringItemIntoView(cmbBox.SelectedIndex);
            _itemsList.SelectedItemChanged += (s) =>
                {
                    cmbBox.SelectedItem = _itemsList.SelectedItem;
                };

            base.AddChild(_itemsList);

            TextButton cancelBut = new TextButton("Cancel", _mWidth - 105, _mHeight - 30, 100, 25);
            cancelBut.ButtonPressed += (s) => { cmbBox.SelectedItem = initialSelectedItem; Close(); };
            base.AddChild(cancelBut);

            TextButton okBut = new TextButton("Ok", cancelBut.X - 105, _mHeight - 30, 100, 25);
            okBut.ButtonPressed += (s) => Close();
            base.AddChild(okBut);

            _suspended = false;
        }
        #endregion

        #region combobox list
        private class ComboBoxList : ListBoxBase
        {
            private object _selItem;

            public object SelectedItem
            {
                get { return _selItem; }
                set
                {
                    int index = GetIndexOf(value);
                    if (index < 0) return;

                    _selItem = value;
                    Refresh();

                    if (SelectedItemChanged != null) SelectedItemChanged(this);
                }
            }

            public event UiEventHandler SelectedItemChanged;

            protected override int ItemHeight { get { return 30; } }

            protected override void DrawItem(Bitmap screen, int x, int y, int width, int height, ref int index, object item, Style cStyle)
            {
                bool selected = _selItem == item;

                if (selected)
                {
                    screen.DrawRectangle(Colors.Blue, 0, x, y, width, height, 0, 0,
                        cStyle.ListBoxSelectedItemBack1, x, y,
                        cStyle.ListBoxSelectedItemBack2, x, y + height, 256);
                }

                Font textFont = cStyle.LabelFont;
                Color textColor = selected ? cStyle.TextBoxPressedTextColor : cStyle.TextBoxEnabledTextColor;
                screen.DrawTextInRect(item.ToString(), x + 5, y + (height - textFont.Height) / 2, width - 10, height, Bitmap.DT_AlignmentLeft, textColor, textFont);

                const int rdMargin = 3;
                int rdX = x + _width - ItemHeight - rdMargin,
                    rdY = y + rdMargin,
                    rdRadius = (ItemHeight - rdMargin * 2) / 2;

                Color bColor = cStyle.RadioButtonEnabledBack;

                screen.DrawEllipse(cStyle.RadioButtonEnabledBorder, 1, rdX + rdRadius, rdY + rdRadius, rdRadius, rdRadius, bColor, 0, 0, bColor, 0, 0, 256);

                if (selected)
                {
                    const int margin = 6;
                    Color pointColor = cStyle.RadioButtonEnabledPoint;

                    _desktopOwner._screenBuffer.DrawEllipse(pointColor, 0, rdX + rdRadius, rdY + rdRadius, rdRadius - margin, rdRadius - margin,
                        pointColor, 0, 0, pointColor, 0, 0, 256);
                }
            }

            protected override bool OnItemTouchDown(int x, int y, int index, object item)
            {
                if (x > _width - ItemHeight - 3)
                {
                    if (_selItem == item) return false;
                    _selItem = item;
                    if (SelectedItemChanged != null) SelectedItemChanged(this);
                    Refresh();
                    return true;
                }

                return false;
            }

            public void AddItem(object item)
            {
                base.AddItem_(item);
            }

            public ComboBoxList(int x, int y, int width, int height)
                : base(x, y, width, height)
            {
                _selItem = null;
            }

            public override void Dispose()
            {
                _selItem = null;
                SelectedItemChanged = null;
                base.Dispose();
            }
        }
        #endregion
    }

}
