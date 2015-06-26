using System;
using Microsoft.SPOT;
using Mp.Ui.Desktops;
using Mp.Ui.Controls.TouchControls;
using Mp.Ui.Managers;
using Mp.Ui.Controls;
using Mp.Ui;

namespace Mp.App.Dialogs
{
    class ProxyDialog : MessageBox
    {
        const int margin = 5;
        const int butHeight = 35;
        CheckBox chkUseProxy, chkForRadio;
        TextBox txtAddress;

        public bool UseProxy { get { return chkUseProxy.IsChecked; } }
        public bool UseForRadio { get { return chkForRadio.IsChecked; } }
        public string ProxyAddress { get { return txtAddress.Text; } }

        public bool Show(bool useProxy, bool useForRadio, string address)
        {
            chkUseProxy.IsChecked = useProxy;
            chkForRadio.IsChecked = useForRadio;
            txtAddress.Text = address;

            if (base.Show() == MessageBoxResult.Ok)
            {
                return true;
            }
            return false;
        }

        public ProxyDialog()
            : base(string.Empty, "Proxy", MessageBoxButtons.OkCancel, MessageBoxIcons.Information, DesktopManager.ScreenWidth / 2, 210)
        {
            int txtWidth = _mWidth * 2 / 3;

            AddChild(chkUseProxy = new CheckBox(false, "Enable Proxy", 10, 58, 120, 30));
            AddChild(chkForRadio = new CheckBox(false, "Use For Radio", 10, chkUseProxy.Y + chkUseProxy.Height + 5, 120, 30) { Enabled = false });
            AddChild(txtAddress = new TextBox("", 60, chkForRadio.Y + chkForRadio.Height + 5, _mWidth - 60 - 10, 30) { Enabled = false, EditTextLabel = "Proxy Address:", AllowMultiline = false });
            AddChild(new Label("Address", 10, txtAddress.Y + txtAddress.Height / 2 - StyleManager.CurrentStyle.LabelFont.Height / 2, 50, 30));

            chkUseProxy.IsCheckedChanged += (s) =>
            {
                chkForRadio.Enabled = chkUseProxy.IsChecked;
                txtAddress.Enabled = chkUseProxy.IsChecked;
            };
        }
    }
}
