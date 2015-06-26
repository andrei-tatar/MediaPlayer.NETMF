using System;
using Microsoft.SPOT;
using Mp.Ui.Desktops;
using Mp.Ui.Managers;
using Mp.App.Controls;
using Mp.Ui.Controls.TouchControls.Buttons;
using Mp.Ui;
using Mp.Ui.Controls;
using Mp.Ui.Controls.TouchControls;

namespace Mp.App.Dialogs
{
    class EditStationDialog : MessageBox
    {
        const int margin = 5;
        const int butHeight = 35;
        TextBox txtName, txtAddress;

        public bool Show(string title, RadioStationItem radioItem)
        {
            _title = title;
            txtName.Text = radioItem.Name;
            txtAddress.Text = radioItem.Address;
            if (base.Show() == MessageBoxResult.Ok)
            {
                radioItem.Name = txtName.Text;
                radioItem.Address = txtAddress.Text;
                return true;
            }
            return false;
        }

        public EditStationDialog()
            : base(string.Empty, string.Empty, MessageBoxButtons.OkCancel, MessageBoxIcons.Information, DesktopManager.ScreenWidth / 2, 170)
        {
            int txtWidth = _mWidth * 2 / 3;

            AddChild(new Label("Name", margin * 2, 55 + 15 - Fonts.Arial.Height / 2, 60, 25));
            AddChild(new Label("Address", margin * 2, 90 + 15 - Fonts.Arial.Height / 2, 60, 25));
            txtName = new TextBox(string.Empty, _mWidth - txtWidth - margin * 2, 55, txtWidth, 30);
            txtAddress = new TextBox(string.Empty, _mWidth - txtWidth - margin * 2, 90, txtWidth, 30);
            AddChild(txtName);
            AddChild(txtAddress);
        }
    }
}
