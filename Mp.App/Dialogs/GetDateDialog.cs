using System;
using Microsoft.SPOT;
using Mp.Ui.Desktops;
using Mp.Ui.Controls.TouchControls;
using Mp.Ui.Controls;
using Mp.Ui;
using Mp.Ui.Managers;
using Mp.Ui.Primitives;
using System.Collections;

namespace Mp.App.Dialogs
{
    class GetDateDialog : MessageBox
    {
        private static Hashtable _monthNames;

        static GetDateDialog()
        {
            _monthNames = new Hashtable(12);
            _monthNames.Add(1, "January");
            _monthNames.Add(2, "February");
            _monthNames.Add(3, "March");
            _monthNames.Add(4, "April");
            _monthNames.Add(5, "May");
            _monthNames.Add(6, "June");
            _monthNames.Add(7, "July");
            _monthNames.Add(8, "August");
            _monthNames.Add(9, "September");
            _monthNames.Add(10, "October");
            _monthNames.Add(11, "November");
            _monthNames.Add(12, "December");
        }

        const int margin = 5;
        const int butHeight = 35;
        ComboBox cmbDay, cmbMonth, cmbYear;

        public int SelectedDay { get { return (int)cmbDay.SelectedItem; } }
        public int SelectedMonth { get { return (int)cmbMonth.SelectedIndex + 1; } }
        public int SelectedYear { get { return (int)cmbYear.SelectedItem; } }

        public bool Show(string title, int initialDay, int initialMonth, int initialYear)
        {
            _title = title;

            cmbYear.SelectedItem = initialYear;
            cmbMonth.SelectedIndex = initialMonth - 1;
            cmbDay.SelectedItem = initialDay;

            if (base.Show() == MessageBoxResult.Ok)
                return true;
            return false;
        }

        public GetDateDialog()
            : base(string.Empty, string.Empty, MessageBoxButtons.OkCancel, MessageBoxIcons.Information, DesktopManager.ScreenWidth / 2, 170)
        {
            int availableWidth = _mWidth - margin * 4;

            const int dayRatio = 2;
            const int monthRatio = 3;
            const int yearRatio = 2;
            const int totalRatio = dayRatio + monthRatio + yearRatio;

            int dayWidth = availableWidth * dayRatio / totalRatio;
            int monthWidth = availableWidth * monthRatio / totalRatio;
            int yearWidth = availableWidth * yearRatio / totalRatio;

            AddChild(new Label("Day", margin, 55 + 15 - Fonts.ArialBold.Height / 2, dayWidth, 25) { CenterText = true, Font = Fonts.ArialBold });
            AddChild(new Label("Month", margin * 2 + dayWidth, 55 + 15 - Fonts.Arial.Height / 2, monthWidth, 25) { CenterText = true, Font = Fonts.ArialBold });
            AddChild(new Label("Year", margin * 3 + dayWidth + monthWidth, 55 + 15 - Fonts.Arial.Height / 2, yearWidth, 25) { CenterText = true, Font = Fonts.ArialBold });

            AddChild(cmbDay = new ComboBox(margin, 85, dayWidth, 30));
            AddChild(cmbMonth = new ComboBox(margin * 2 + dayWidth, 85, monthWidth, 30));
            AddChild(cmbYear = new ComboBox(margin * 3 + dayWidth + monthWidth, 85, yearWidth, 30));

            for (int i = 2009; i <= 2030; i++) cmbYear.AddItem(i);
            for (int i = 1; i <= 12; i++) cmbMonth.AddItem(_monthNames[i]);

            UiEventHandler remakeDaysEvent = (s) =>
            {
                int prevSelected = cmbDay.SelectedIndex;
                cmbDay.ClearItems();
                for (int i = 1; i <= DateTime.DaysInMonth((int)cmbYear.SelectedItem, (int)cmbMonth.SelectedIndex + 1); i++) cmbDay.AddItem(i);
                if (prevSelected != -1 && prevSelected < cmbDay.ItemsCount) cmbDay.SelectedIndex = prevSelected;
                else cmbDay.SelectedIndex = cmbDay.ItemsCount - 1;
            };

            cmbMonth.OnSelectedItemChanged += remakeDaysEvent;
            cmbYear.OnSelectedItemChanged += remakeDaysEvent;
        }
    }
}
