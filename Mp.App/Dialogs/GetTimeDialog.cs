using System;
using Microsoft.SPOT;
using Mp.Ui.Desktops;
using Mp.Ui.Controls.TouchControls;
using Mp.Ui.Managers;
using Mp.Ui.Controls;
using Mp.Ui;

namespace Mp.App.Dialogs
{
    class GetTimeDialog : MessageBox
    {
        const int margin = 5;
        const int butHeight = 35;
        ComboBox cmbHour, cmbMinute;

        public int SelectedHour { get { return (int)cmbHour.SelectedItem; } }
        public int SelectedMinute { get { return (int)cmbMinute.SelectedItem; } }

        public bool Show(string title, int initialHour, int initialMinute)
        {
            cmbHour.SelectedItem = initialHour;
            cmbMinute.SelectedItem = initialMinute;
            
            _title = title;
            if (base.Show() == MessageBoxResult.Ok)
                return true;
            return false;
        }

        public GetTimeDialog()
            : base(string.Empty, string.Empty, MessageBoxButtons.OkCancel, MessageBoxIcons.Information, DesktopManager.ScreenWidth / 2, 170)
        {
            int txtWidth = (_mWidth - margin * 3) / 2;

            AddChild(new Label("Hour", margin, 55 + 15 - Fonts.ArialBold.Height / 2, txtWidth, 25) { CenterText = true, Font = Fonts.ArialBold });
            AddChild(new Label("Minute", margin * 2 + txtWidth, 55 + 15 - Fonts.Arial.Height / 2, txtWidth, 25) { CenterText = true, Font = Fonts.ArialBold });

            AddChild(cmbHour = new ComboBox(margin, 85, txtWidth, 30));
            AddChild(cmbMinute = new ComboBox(margin * 2 + txtWidth, 85, txtWidth, 30));

            for (int i = 0; i < 24; i++) cmbHour.AddItem(i);
            for (int i = 0; i < 60; i++) cmbMinute.AddItem(i);
        }
    }
}
