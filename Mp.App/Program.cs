using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Net.NetworkInformation;

using GHIElectronics.NETMF;
using GHIElectronics.NETMF.IO;
using GHIElectronics.NETMF.Hardware;
using GHIElectronics.NETMF.Hardware.LowLevel;

using Mp.Input;
using Mp.Weather.Yahoo;
using Mp.Weather.Google;

using Mp.Ui;
using Mp.Ui.Styles;
using Mp.Ui.Managers;
using Mp.Ui.Desktops;
using Mp.Ui.Validators;
using Mp.Ui.Primitives;
using Mp.Ui.Controls;
using Mp.Ui.Controls.Containers;
using Mp.Ui.Controls.TouchControls;
using Mp.Ui.Controls.TouchControls.Buttons;

using Mp.App.Audio;
using Mp.App.Resources;
using Mp.App.Controls;
using Mp.App.Dialogs;
using GHIElectronics.NETMF.Net;

namespace Mp.App
{
    public class Program
    {
        private delegate string GetBasicWeatherInfoHandler();
        private delegate string GetRadioInfoHandler();

        private static GetBasicWeatherInfoHandler GetWeatherInfo;
        private static GetRadioInfoHandler GetRadioInfo;
        private static WebProxy proxyForWeather;

        private static void CreatePlayerPanel(Panel panel)
        {
            const int btSize = 50;
            const int margin = 5;

            Label title = new Label("Player", 5, 5, 200, Fonts.ArialMediumBold.Height) { Font = Fonts.ArialMediumBold };
            panel.AddChild(title);

            FsList fsList = new FsList(5, title.Y + title.Height + 5, panel.Width - margin * 3 - btSize, panel.Height - (title.Y + title.Height + margin * 3 + btSize));
            panel.AddChild(fsList);

            ArrayList rootItems = new ArrayList();

            FsItem currentPlaying = null;

            SimpleActionParamString playFile = (filePath) =>
            {
                switch (VsDriver.Status)
                {
                    case VsStatus.Connecting: return false;
                    case VsStatus.PlayingRadio: return false;
                    case VsStatus.Playing: VsDriver.StopFile(); break;
                    case VsStatus.Paused: VsDriver.ResumeFile(); VsDriver.StopFile(); break;
                }
                int tries = 5;
                Exception lastEx = null;
                while (tries-- != 0)
                {
                    try
                    {
                        FileStream file = File.Open(filePath, FileMode.Open);
                        VsDriver.PlayFile(file);
                        lastEx = null;
                        break;
                    }
                    catch (Exception ex) { lastEx = ex; }
                }

                if (lastEx != null)
                {
                    new Thread(() =>
                    {
                        using (MessageBox mb = new MessageBox(lastEx.Message, "Error", MessageBoxButtons.Ok, MessageBoxIcons.Error)) mb.Show();
                    }).Start();
                    return false;
                }
                return true;
            };

            #region list double click
            fsList.OnDoubleClick += (s) =>
            {
                FsItem selectedItem = fsList.SelectedItem;
                if (selectedItem == null) return;

                if (selectedItem.IsBack)
                {
                    fsList.ClearItems();
                    fsList.Folder = selectedItem.Parent;

                    if (fsList.Folder.Parent != null)
                        fsList.AddItems(fsList.Folder.Parent.Childs);
                    else
                        fsList.AddItems(rootItems);
                }
                else if (selectedItem.IsDirectory)
                {
                    if (selectedItem.Childs == null)
                    {
                        selectedItem.Childs = new ArrayList();
                        string[] dirs = Directory.GetDirectories(selectedItem.Path);
                        selectedItem.Childs.Add(new FsItem(selectedItem) { IsBack = true, Name = "Back" });
                        foreach (string dir in dirs)
                            selectedItem.Childs.Add(new FsItem(selectedItem) { IsBack = false, IsDirectory = true, IsMusicFile = false, Name = Path.GetFileName(dir), Path = dir });

                        string[] files = Directory.GetFiles(selectedItem.Path);
                        foreach (string file in files)
                        {
                            string extension = Path.GetExtension(file).ToLower();
                            bool isMusicFile = extension.Equals(".mp3");
                            selectedItem.Childs.Add(new FsItem(selectedItem) { IsBack = false, IsDirectory = false, IsMusicFile = isMusicFile, Name = Path.GetFileName(file), Path = file });
                        }
                    }

                    fsList.Folder = selectedItem;
                    fsList.ClearItems();
                    fsList.AddItems(selectedItem.Childs);
                }
                else if (selectedItem.IsMusicFile)
                {
                    if (playFile(selectedItem.Path))
                    {
                        currentPlaying = selectedItem;
                        fsList.CurrentPlaying = currentPlaying;
                    }
                }
            };
            #endregion

            #region removable media events
            RemovableMedia.Insert += (s, e) =>
            {
                FsItem newItem = new FsItem(null) { IsBack = false, IsDirectory = true, IsMusicFile = false, Name = e.Volume.Name, Path = e.Volume.RootDirectory };
                rootItems.Add(newItem);

                if (fsList.Folder == null)
                    fsList.AddItem(newItem);
            };

            RemovableMedia.Eject += (s, e) =>
            {
                FsItem toRemove = null;
                foreach (FsItem fsItem in rootItems) if (fsItem.Path == e.Volume.RootDirectory) { toRemove = fsItem; break; }
                if (toRemove != null)
                {
                    rootItems.Remove(toRemove);
                    if (fsList.Folder == null)
                        fsList.RemoveItem(toRemove);

                    else if (fsList.Folder.GetRootNode().Path == e.Volume.RootDirectory)
                    {
                        fsList.Folder = null;
                        fsList.ClearItems();
                        fsList.AddItems(rootItems);
                    }
                }
            };
            #endregion

            ImageButton btDelete;
            panel.AddChild(btDelete = new ImageButton(Images.GetBitmap(Images.BitmapResources.imgDelete), fsList.X + fsList.Width + margin, fsList.Y, btSize, btSize));
            btDelete.ButtonPressed += (s) =>
            {
                FsItem selectedItem = fsList.SelectedItem;
                if (selectedItem == null || selectedItem.IsBack || selectedItem.Parent == null) return;
                if (selectedItem == currentPlaying) VsDriver.StopFile();

                new Thread(new ThreadStart(() =>
                {
                    using (MessageBox mb = new MessageBox("Are you sure you want to delete\n" + selectedItem.Name + "?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcons.Delete))
                    {
                        if (mb.Show() == MessageBoxResult.Yes)
                        {
                            try
                            {
                                if (selectedItem.IsDirectory) Directory.Delete(selectedItem.Path);
                                else File.Delete(selectedItem.Path);

                                selectedItem.Parent.Childs.Remove(selectedItem);
                                fsList.RemoveItem(selectedItem);
                            }
                            catch (Exception)
                            {
                                using (MessageBox err = new MessageBox("Error deleting item", "Error", MessageBoxButtons.Ok, MessageBoxIcons.Error))
                                    err.Show();
                            }
                        }
                    }
                })).Start();
            };

            ImageButton btPlay, btStop, btNext, btPrev;
            int x = fsList.X, y = fsList.Y + fsList.Height + margin;

            Bitmap imgPlay = Images.GetBitmap(Images.BitmapResources.btPlay), imgPause = Images.GetBitmap(Images.BitmapResources.btPause);

            panel.AddChild(btPrev = new ImageButton(Images.GetBitmap(Images.BitmapResources.btBack), x, y, btSize, btSize)); x += margin + btSize;
            panel.AddChild(btPlay = new ImageButton(Images.GetBitmap(Images.BitmapResources.btPlay), x, y, btSize, btSize)); x += margin + btSize;
            panel.AddChild(btNext = new ImageButton(Images.GetBitmap(Images.BitmapResources.btForward), x, y, btSize, btSize)); x += margin + btSize;
            panel.AddChild(btStop = new ImageButton(Images.GetBitmap(Images.BitmapResources.btStop), x, y, btSize, btSize));

            #region prev/play/next/stop events
            bool ignore = false;
            UiEventHandler actionPrev = (s) =>
            {
                if (ignore) return;
                try
                {
                    ignore = true;

                    if (currentPlaying == null) return;
                    FsItem aux = currentPlaying;

                    int stIndex = aux.Parent.Childs.IndexOf(aux);
                    if (stIndex == -1) return;

                    FsItem nextToPlay;
                    do
                    {
                        stIndex--;
                        if (stIndex < 0) stIndex = aux.Parent.Childs.Count - 1;
                        nextToPlay = (FsItem)aux.Parent.Childs[stIndex];
                        if (nextToPlay.IsMusicFile) break;
                    } while (true);

                    if (playFile(nextToPlay.Path))
                    {
                        currentPlaying = nextToPlay;
                        fsList.CurrentPlaying = currentPlaying;
                    }
                }
                finally
                {
                    ignore = false;
                }
            };

            UiEventHandler actionNext = (s) =>
            {
                if (ignore) return;
                try
                {
                    ignore = true;

                    if (currentPlaying == null) return;
                    FsItem aux = currentPlaying;

                    int stIndex = aux.Parent.Childs.IndexOf(aux);
                    if (stIndex == -1) return;

                    FsItem nextToPlay;
                    do
                    {
                        stIndex++;
                        if (stIndex == aux.Parent.Childs.Count) stIndex = 0;
                        nextToPlay = (FsItem)aux.Parent.Childs[stIndex];
                        if (nextToPlay.IsMusicFile) break;
                    } while (true);

                    if (playFile(nextToPlay.Path))
                    {
                        currentPlaying = nextToPlay;
                        fsList.CurrentPlaying = currentPlaying;
                    }
                }
                finally
                {
                    ignore = false;
                }
            };

            btPrev.ButtonPressed += actionPrev;
            btNext.ButtonPressed += actionNext;

            btPlay.ButtonPressed += (s) =>
            {
                switch (VsDriver.Status)
                {
                    case VsStatus.Playing: VsDriver.PauseFile(); break;
                    case VsStatus.Paused: VsDriver.ResumeFile(); break;
                    case VsStatus.Stopped:
                        FsItem selectedItem = fsList.SelectedItem;
                        if (selectedItem != null && selectedItem.IsMusicFile)
                        {
                            if (playFile(selectedItem.Path))
                            {
                                currentPlaying = selectedItem;
                                fsList.CurrentPlaying = currentPlaying;
                            }
                        }
                        break;
                }
            };

            bool stopPressed = false;
            btStop.ButtonPressed += (s) =>
            {
                switch (VsDriver.Status)
                {
                    case VsStatus.Playing: stopPressed = true; VsDriver.StopFile(); break;
                    case VsStatus.Paused: stopPressed = true; VsDriver.ResumeFile(); VsDriver.StopFile(); break;
                }
            };
            #endregion

            VsDriver.VsStatusChanged += (status) =>
            {
                if (status == VsStatus.Playing)
                    btPlay.Image = imgPause;
                else
                {
                    if (status == VsStatus.Stopped)
                    {
                        if (!stopPressed)
                        {
                            if (currentPlaying != null)
                                actionNext(btNext);
                        }
                        else
                        {
                            currentPlaying = null;
                            fsList.CurrentPlaying = currentPlaying;
                            stopPressed = false;
                        }
                    }
                    btPlay.Image = imgPlay;
                }
            };
        }

        private static void CreateRadioPanel(Panel panel)
        {
            Label title = new Label("Radio", 5, 5, 200, Fonts.ArialMediumBold.Height) { Font = Fonts.ArialMediumBold };
            panel.AddChild(title);

            const int btSize = 50;
            const int margin = 5;

            int y = title.X + title.Height + margin;
            RadioStationsList radioList = new RadioStationsList(margin, y, panel.Width - margin * 3 - btSize, panel.Height - y - margin * 2 - btSize);

            foreach (RadioStationItem item in AppSettings.Instance.RadioStations)
                radioList.AddItem(item);

            Mp.Ui.Controls.TouchControls.Button btAdd, btRemove, btEdit;
            panel.AddChild(btAdd = new ImageButton(Images.GetBitmap(Images.BitmapResources.imgAdd), radioList.X + radioList.Width + margin, radioList.Y, btSize, btSize));
            panel.AddChild(btRemove = new ImageButton(Images.GetBitmap(Images.BitmapResources.imgDelete), btAdd.X, btAdd.Y + btSize + margin, btSize, btSize));
            panel.AddChild(btEdit = new ImageButton(Images.GetBitmap(Images.BitmapResources.imgEdit), btAdd.X, btRemove.Y + btSize + margin, btSize, btSize));

            EditStationDialog esd = new EditStationDialog();

            #region add/remove/edit events
            btAdd.ButtonPressed += (s) =>
            {
                new Thread(() =>
                {
                    RadioStationItem item = new RadioStationItem("http://", string.Empty);
                    if (esd.Show("New Radio Station", item))
                    {
                        radioList.AddItem(item);
                        AppSettings.Instance.RadioStations.Add(item);
                        AppSettings.Instance.Save();
                    }
                }).Start();
            };

            btRemove.ButtonPressed += (s) =>
            {
                if (radioList.SelectedIndex != -1)
                {
                    RadioStationItem selItem = radioList.SelectedItem;
                    new Thread(() =>
                    {
                        using (MessageBox mb = new MessageBox("Are you sure you want to delete\n" + selItem.Name + "?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcons.Delete))
                            if (mb.Show() == MessageBoxResult.Yes)
                            {
                                radioList.RemoveItem(selItem);
                                AppSettings.Instance.RadioStations.Remove(selItem);
                                AppSettings.Instance.Save();
                            }
                    }).Start();
                }
            };

            btEdit.ButtonPressed += (s) =>
            {
                if (radioList.SelectedIndex != -1)
                {
                    RadioStationItem selItem = radioList.SelectedItem;
                    new Thread(() =>
                    {
                        if (esd.Show("Edit Radio Station", selItem))
                        {
                            radioList.Refresh();
                            AppSettings.Instance.Save();
                        }
                    }).Start();
                }
            };
            #endregion

            ImageButton btPlay, btStop;
            panel.AddChild(btPlay = new ImageButton(Images.GetBitmap(Images.BitmapResources.btPlay), radioList.X, radioList.Y + radioList.Height + margin, btSize, btSize));
            panel.AddChild(btStop = new ImageButton(Images.GetBitmap(Images.BitmapResources.btStop), btPlay.X + btPlay.Width + margin, radioList.Y + radioList.Height + margin, btSize, btSize));

            RadioStationItem playingItem = null;
            int playingIndex = 0;

            VsDriver.VsStatusChanged += (status) =>
            {
                if (playingItem == null) return;
                switch (status)
                {
                    case VsStatus.Connecting:
                        playingItem.IsConnecting = true;
                        radioList.RefreshItem(playingIndex);
                        break;

                    case VsStatus.PlayingRadio:
                        playingItem.IsConnecting = false;
                        playingItem.IsPlaying = true;
                        radioList.RefreshItem(playingIndex);
                        break;

                    case VsStatus.Stopped:
                        playingItem.IsPlaying = false;
                        playingItem.IsConnecting = false;
                        radioList.RefreshItem(playingIndex);
                        break;
                }
            };

            string metaData = null;

            GetRadioInfo += () =>
            {
                return VsDriver.Status == VsStatus.PlayingRadio ? metaData : null;
            };

            char[] metadataSeparators = new char[] { ';' };
            VsDriver.VsRadioMetaDataReceived += (newMetaData) =>
            {
                string[] metaDataParams = newMetaData.Split(metadataSeparators);
                metaData = newMetaData;
            };

            btPlay.ButtonPressed += (s) =>
            {
                if (radioList.SelectedIndex == -1) return;
                if (playingItem == null)
                {
                    playingItem = radioList.SelectedItem;
                    playingIndex = radioList.SelectedIndex;
                }
                else
                {
                    VsDriver.StopRadio();
                    playingItem = radioList.SelectedItem;
                    playingIndex = radioList.SelectedIndex;
                }
                VsDriver.ConnectToStation(playingItem.Address);
            };

            btStop.ButtonPressed += (s) =>
            {
                VsDriver.StopRadio();
                playingItem = null;
            };

            panel.AddChild(radioList);
        }

        private static void CreateWeatherPanel(Panel panel)
        {
            //initial weather place
            const string initialPlace = "Timisoara";

            proxyForWeather = AppSettings.Instance.Network.UseProxy && !AppSettings.Instance.Network.Proxy.UseForRadio ? new WebProxy(AppSettings.Instance.Network.Proxy.Address) : null;

            //create a new Yahoo! Weather Provider
            YahooWeatherProvider yProvider = new YahooWeatherProvider();
            GoogleWeatherProvider gProvider = new GoogleWeatherProvider();

            RadioButton rdYahoo = null, rdGoogle = null;
            Ui.Controls.TouchControls.Button refreshButton = null;

            Label title = new Label("Weather", 5, 5, 200, Fonts.ArialMediumBold.Height) { Font = Fonts.ArialMediumBold };
            panel.AddChild(title);

            //create text box for weather place
            TextBox txtWeatherPlace = new TextBox(string.Empty, 5, title.ScreenBottom + 15, 170, 30) { Validator = (s) => s.Trim().Length != 0, EditTextLabel = "Place:" };
            panel.AddChild(txtWeatherPlace);

            //create a weather control and add it on the desktop (centered)
            GoogleWeatherControl gControl = new GoogleWeatherControl(gProvider.LastWeatherCondition, 0, 80) { Visible = false };
            YahooWeatherControl yControl = new YahooWeatherControl(yProvider.LastWeatherCondition, 0, 0);
            gControl.X = (panel.Width - gControl.Width) / 2;
            gControl.Y = txtWeatherPlace.ScreenBottom + (panel.Height - txtWeatherPlace.ScreenBottom - gControl.Height) / 2;
            yControl.X = (panel.Width - yControl.Width) / 2;
            yControl.Y = gControl.Y;
            panel.AddChild(gControl);
            panel.AddChild(yControl);

            //watch for text changed
            string oldPlace = string.Empty;
            const string noWeatherInfo = "No weather info";
            string basicWeatherInfo = null;
            GetWeatherInfo = () =>
            {
                if (basicWeatherInfo == null)
                {
                    basicWeatherInfo = null;
                    if (rdGoogle.IsChecked)
                    {
                        if (gProvider.LastWeatherCondition != null)
                            basicWeatherInfo = gProvider.LastWeatherCondition.City + " " + gProvider.LastWeatherCondition.CurrentCondition.Temp;
                    }
                    else if (rdYahoo.IsChecked)
                    {
                        if (yProvider.LastWeatherCondition != null)
                            basicWeatherInfo = yProvider.LastWeatherCondition.Location + " " + yProvider.LastWeatherCondition.Temperature;
                    }
                    if (basicWeatherInfo == null)
                        basicWeatherInfo = noWeatherInfo;
                }
                return basicWeatherInfo;
            };

            SimpleAction refreshWeather = () =>
            {
                try
                {
                    if (!Ethernet.IsCableConnected) return;
                }
                catch (Exception)
                {
                    //TODO: emulator throws exception
                }

                //disable the textbox
                txtWeatherPlace.Enabled = yControl.Enabled = gControl.Enabled = refreshButton.Enabled = false;
                //get the weather in a new Thread
                new Thread(() =>
                {
                    if (yProvider.SetWeatherPlace(oldPlace, proxyForWeather)) yProvider.GetWeather(proxyForWeather);
                    yControl.WeatherCondition = yProvider.LastWeatherCondition;

                    gProvider.GetWeather(oldPlace, proxyForWeather);
                    gControl.WeatherCondition = gProvider.LastWeatherCondition;

                    basicWeatherInfo = null;

                    txtWeatherPlace.Enabled = yControl.Enabled = gControl.Enabled = refreshButton.Enabled = true;
                }).Start();
            };

            AppConfig.OnNetworkConnected += () => refreshWeather();

            txtWeatherPlace.TextChanged += (newPlace, valid) =>
            {
                string nPlace = newPlace.Trim().ToLower();
                //if the new place is different
                if (nPlace != oldPlace)
                {
                    oldPlace = nPlace;
                    refreshWeather();
                }
            };

            //refresh the text colors according to the current style
            StyleManager.StyleChanged += (oldStyle, newStyle) =>
            {
                yControl.WeatherCondition = yProvider.LastWeatherCondition;
                gControl.WeatherCondition = gProvider.LastWeatherCondition;
            };


            panel.AddChild(refreshButton = new ImageButton(Images.GetBitmap(Images.BitmapResources.imgRefresh),
                txtWeatherPlace.X + txtWeatherPlace.Width + 5, txtWeatherPlace.Y, txtWeatherPlace.Height, txtWeatherPlace.Height));
            refreshButton.ButtonPressed += (s) => refreshWeather();

            panel.AddChild(rdYahoo = new RadioButton("Yahoo", refreshButton.X + refreshButton.Width + 15, refreshButton.ScreenTop + refreshButton.Height / 2 - 13, 80, 26) { IsChecked = true });
            panel.AddChild(rdGoogle = new RadioButton("Google", rdYahoo.X + rdYahoo.Width + 15, txtWeatherPlace.ScreenTop + txtWeatherPlace.Height / 2 - 13, rdYahoo.Width, 26));
            UiEventHandler checkChanged = (s) =>
            {
                //this will trigger once for each radio button
                if (!((RadioButton)s).IsChecked) return;

                bool oldValue = panel.Suspended;
                panel.Suspended = true;
                if (rdYahoo.IsChecked)
                {
                    yControl.Visible = true;
                    gControl.Visible = false;
                }
                else
                {
                    yControl.Visible = false;
                    gControl.Visible = true;
                }
                basicWeatherInfo = null;
                panel.Suspended = oldValue;
            };
            rdGoogle.IsCheckedChanged += checkChanged;
            rdYahoo.IsCheckedChanged += checkChanged;

            //set the text to trigger a get on the weather
            txtWeatherPlace.Text = initialPlace;
        }

        private static void CreateNetworkSettingsPanel(Panel panel)
        {
            const int leftMargin = 10;
            const int topMargin = 10;

            NetworkInterface net = NetworkInterface.GetAllNetworkInterfaces()[0];

            bool isDhcp = net.IsDhcpEnabled;
            int labelFontHeightDiv2 = StyleManager.CurrentStyle.LabelFont.Height / 2;

            RadioButton rdStatic, rdDynamic;
            panel.AddChild(new Label("Configuration", leftMargin, topMargin + 13 - labelFontHeightDiv2, 80, 25));
            panel.AddChild(rdStatic = new RadioButton("Static", 100, topMargin, 80, 26) { IsChecked = !isDhcp });
            panel.AddChild(rdDynamic = new RadioButton("Dynamic", rdStatic.X + rdStatic.Width + 10, topMargin, 80, 26) { IsChecked = isDhcp });

            TextBox txtIp, txtNetMask, txtGateway, txtDns;

            int y = rdStatic.Y + rdStatic.Height + 5;

            panel.AddChild(new Label("Address", leftMargin, y + 15 - labelFontHeightDiv2, 80, 25));
            panel.AddChild(txtIp = new TextBox(net.IPAddress.ToString(), 100, y, 175, 30)
            {
                EditTextLabel = "IP Address",
                Validator = NetValidators.IpValidator,
                AllowedChars = AllowedCharTypesEnum.Ip,
                Enabled = !isDhcp
            });
            y += 35;

            panel.AddChild(new Label("Netmask", leftMargin, y + 15 - labelFontHeightDiv2, 80, 25));
            panel.AddChild(txtNetMask = new TextBox(net.SubnetMask.ToString(), 100, y, 175, 30)
            {
                EditTextLabel = "Subnet Mask",
                Validator = NetValidators.NetMaskValidator,
                AllowedChars = AllowedCharTypesEnum.Ip,
                Enabled = !isDhcp
            });
            y += 35;

            panel.AddChild(new Label("Gateway", leftMargin, y + 15 - labelFontHeightDiv2, 80, 25));
            panel.AddChild(txtGateway = new TextBox(net.GatewayAddress.ToString(), 100, y, 175, 30)
            {
                EditTextLabel = "Gateway IP Address",
                Validator = NetValidators.IpValidator,
                AllowedChars = AllowedCharTypesEnum.Ip,
                Enabled = !isDhcp
            });
            y += 35;

            panel.AddChild(new Label("Dns", leftMargin, y + 15 - labelFontHeightDiv2, 80, 25));
            panel.AddChild(txtDns = new TextBox(net.DnsAddresses.Length >= 1 ? net.DnsAddresses[0] : string.Empty, 100, y, 175, 30)
            {
                EditTextLabel = "DNS Address",
                Validator = NetValidators.IpAllowEmptyValidator,
                AllowedChars = AllowedCharTypesEnum.Ip,
                Enabled = !isDhcp
            });
            y += 35;

            TextButton applyButton = null, proxyButton;

            UiEventHandler modeChangedHandler = (s) =>
            {
                bool dynamicSelected = rdDynamic.IsChecked;

                panel.Suspended = true;

                txtIp.Enabled = !dynamicSelected;
                txtNetMask.Enabled = !dynamicSelected;
                txtGateway.Enabled = !dynamicSelected;
                txtDns.Enabled = !dynamicSelected;

                if (!dynamicSelected)
                {
                    txtIp.Text = net.IPAddress.ToString();
                    txtNetMask.Text = net.SubnetMask.ToString();
                    txtGateway.Text = net.GatewayAddress.ToString();
                    txtDns.Text = net.DnsAddresses.Length >= 1 ? net.DnsAddresses[0].ToString() : string.Empty;
                }
                else applyButton.Enabled = true;

                panel.Suspended = false;
            };
            rdStatic.IsCheckedChanged += modeChangedHandler;

            int x = txtIp.X + txtIp.Width + 10;
            panel.AddChild(applyButton = new TextButton("Apply", x, txtIp.Y, panel.Width - x - 10, txtIp.Height * 2 + 5) { Enabled = false });
            panel.AddChild(proxyButton = new TextButton("Proxy", x, txtGateway.Y, panel.Width - x - 10, txtGateway.Height * 2 + 5));

            applyButton.ButtonPressed += (s) =>
            {
                applyButton.Enabled = false;

                AppSettings.Instance.Network.DhcpEnabled = rdDynamic.IsChecked;

                if (rdDynamic.IsChecked)
                {
                    net.EnableDhcp();
                }
                else
                {
                    net.EnableStaticIP(txtIp.Text, txtNetMask.Text, txtGateway.Text);

                    if (txtDns.Text.Length != 0)
                    {
                        AppSettings.Instance.Network.DnsAddresses = new string[] { txtDns.Text };
                        net.EnableStaticDns(AppSettings.Instance.Network.DnsAddresses);
                    }
                    AppSettings.Instance.Network.IpAddress = IPAddress.Parse(txtIp.Text).GetAddressBytes();
                    AppSettings.Instance.Network.NetMask = IPAddress.Parse(txtNetMask.Text).GetAddressBytes();
                    AppSettings.Instance.Network.Gateway = IPAddress.Parse(txtGateway.Text).GetAddressBytes();
                }

                AppSettings.Instance.Save();
            };

            ProxyDialog proxyDiag = new ProxyDialog();
            proxyButton.ButtonPressed += (s) =>
            {
                new Thread(new ThreadStart(() =>
                    {
                        if (proxyDiag.Show(AppSettings.Instance.Network.UseProxy, AppSettings.Instance.Network.Proxy.UseForRadio, AppSettings.Instance.Network.Proxy.Address))
                        {
                            AppSettings.Instance.Network.UseProxy = proxyDiag.UseProxy;
                            AppSettings.Instance.Network.Proxy.UseForRadio = proxyDiag.UseForRadio;
                            AppSettings.Instance.Network.Proxy.Address = proxyDiag.ProxyAddress;

                            proxyForWeather = AppSettings.Instance.Network.UseProxy && !AppSettings.Instance.Network.Proxy.UseForRadio ?
                                new WebProxy(AppSettings.Instance.Network.Proxy.Address) : null;

                            AppSettings.Instance.Save();
                        }
                    })).Start();
            };

            txtDns.TextChanged += (nt, valid) => applyButton.Enabled = valid;
            txtIp.TextChanged += (nt, valid) => applyButton.Enabled = valid;
            txtGateway.TextChanged += (nt, valid) => applyButton.Enabled = valid;
            txtNetMask.TextChanged += (nt, valid) => applyButton.Enabled = valid;
        }

        private static void CreateAudioSettingsPanel(Panel panel)
        {
            const int leftMargin = 5, topMargin = 8;
            int labelFontHeightDiv2 = StyleManager.CurrentStyle.LabelFont.Height / 2;

            int y = topMargin;

            Slider bassSlider, trebleSlider, volumeSlider, balanceSlider;

            panel.AddChild(volumeSlider = new Slider(leftMargin + 50, y, panel.Width - leftMargin * 2 - 50, 35));
            volumeSlider.Maximum = VsDriver.VolumeMax;
            volumeSlider.Minimum = VsDriver.VolumeMin;
            volumeSlider.Position = VsDriver.Volume;
            volumeSlider.PositionChanged += (s) => VsDriver.Volume = (byte)volumeSlider.Position;
            panel.AddChild(new Label("Volume", leftMargin, y + volumeSlider.Height / 2 - labelFontHeightDiv2, 50, 35));
            y += 45;

            panel.AddChild(balanceSlider = new Slider(leftMargin + 50, y, panel.Width - leftMargin * 2 - 50, 35));
            balanceSlider.Maximum = VsDriver.BalanceMax;
            balanceSlider.Minimum = VsDriver.BalanceMin;
            balanceSlider.Position = VsDriver.Balance;
            balanceSlider.PositionChanged += (s) => VsDriver.Balance = (sbyte)balanceSlider.Position;
            panel.AddChild(new Label("Balance", leftMargin, y + balanceSlider.Height / 2 - labelFontHeightDiv2, 50, 35));
            y += 45;

            panel.AddChild(bassSlider = new Slider(leftMargin + 50, y, panel.Width - leftMargin * 2 - 50, 35));
            bassSlider.Maximum = VsDriver.BassMax;
            bassSlider.Minimum = VsDriver.BassMin;
            bassSlider.Position = VsDriver.Bass;
            bassSlider.PositionChanged += (s) => VsDriver.Bass = (byte)bassSlider.Position;
            panel.AddChild(new Label("Bass", leftMargin, y + bassSlider.Height / 2 - labelFontHeightDiv2, 50, 35));
            y += 45;

            panel.AddChild(trebleSlider = new Slider(leftMargin + 50, y, panel.Width - leftMargin * 2 - 50, 35));
            trebleSlider.Maximum = VsDriver.TrebleMax;
            trebleSlider.Minimum = VsDriver.TrebleMin;
            trebleSlider.Position = VsDriver.Treble;
            trebleSlider.PositionChanged += (s) => VsDriver.Treble = (sbyte)trebleSlider.Position;
            panel.AddChild(new Label("Treble", leftMargin, y + trebleSlider.Height / 2 - labelFontHeightDiv2, 50, 35));
            y += 45;

        }

        private static void CreateDisplaySettingsPanel(Panel panel)
        {
            const int leftMargin = 5, topMargin = 8;
            int labelFontHeightDiv2 = StyleManager.CurrentStyle.LabelFont.Height / 2;

            int y = topMargin;

            Slider brightnessSlider, timeoutSlider;

            panel.AddChild(brightnessSlider = new Slider(leftMargin + 70, y, panel.Width - leftMargin * 2 - 70, 35));
            brightnessSlider.Maximum = 31;
            brightnessSlider.Minimum = 0;
            brightnessSlider.Position = 31 - AppConfig.LcdBacklightLevel;
            brightnessSlider.PositionChanged += (s) =>
            {
                AppSettings.Instance.LcdBacklightLevel = (byte)(31 - brightnessSlider.Position);
                AppSettings.Instance.Save();
                AppConfig.LcdBacklightLevel = AppSettings.Instance.LcdBacklightLevel;
            };
            panel.AddChild(new Label("Brightness", leftMargin, y + brightnessSlider.Height / 2 - labelFontHeightDiv2, 70, 35));
            y += 45;

            panel.AddChild(timeoutSlider = new Slider(leftMargin + 70, y, panel.Width - leftMargin * 2 - 70, 35));
            timeoutSlider.Maximum = 180;
            timeoutSlider.Minimum = 20;
            timeoutSlider.Position = AppSettings.Instance.ShowScreenSaverAfterSec;
            timeoutSlider.PositionChanged += (s) =>
            {
                AppSettings.Instance.ShowScreenSaverAfterSec = timeoutSlider.Position;
                AppSettings.Instance.Save();
                DesktopManager.Instance.SetInputTimeout(AppSettings.Instance.ShowScreenSaverAfterSec * 1000);
            };
            panel.AddChild(new Label("Timeout", leftMargin, y + brightnessSlider.Height / 2 - labelFontHeightDiv2, 70, 35));
            y += 50;

            panel.AddChild(new Label("Style", leftMargin, y, 70, 30));
            y += 30;
            RadioButton rdLight, rdDark, rdMetroDark, rdMetroLight;

            panel.AddChild(rdMetroLight = new RadioButton("Metro Light", leftMargin, y, (panel.Width - leftMargin * 2) / 4, 26) { IsChecked = true, Tag = new StyleMetroLight() });
            panel.AddChild(rdLight = new RadioButton("Light", leftMargin + rdMetroLight.Width, rdMetroLight.Y, rdMetroLight.Width, 26) { Tag = new StyleLight() });
            panel.AddChild(rdMetroDark = new RadioButton("Metro Dark", leftMargin + rdMetroLight.Width * 2, rdMetroLight.Y, rdMetroLight.Width, 26) { Tag = new StyleMetroDark() });
            panel.AddChild(rdDark = new RadioButton("Dark", leftMargin + rdMetroLight.Width * 3, rdMetroLight.Y, rdMetroLight.Width, 26) { Tag = new StyleDark() });

            UiEventHandler radioCheckedChanged = (s) =>
            {
                RadioButton sender = (RadioButton)s;
                if (sender.IsChecked) StyleManager.CurrentStyle = (Style)sender.Tag;
            };

            rdLight.IsCheckedChanged += radioCheckedChanged;
            rdMetroDark.IsCheckedChanged += radioCheckedChanged;
            rdMetroLight.IsCheckedChanged += radioCheckedChanged;
            rdDark.IsCheckedChanged += radioCheckedChanged;
        }

        private static void CreateOtherSettingsPanel(Panel panel)
        {
            const int leftMargin = 10, topMargin = 10;
            int width = (panel.Width - leftMargin * 3) / 2;

            TextButton setTime, setDate, butCalibrate;
            panel.AddChild(setTime = new TextButton("Set Time", leftMargin, topMargin, width, 35));
            panel.AddChild(setDate = new TextButton("Set Date", leftMargin * 2 + width, topMargin, width, 35));
            panel.AddChild(butCalibrate = new TextButton("Calibrate", leftMargin, topMargin * 2 + setTime.Height + 5, width, 35));

            GetTimeDialog getTimeDiag = new GetTimeDialog();
            GetDateDialog getDateDiag = new GetDateDialog();

            setTime.ButtonPressed += (s) =>
            {
                new Thread(new ThreadStart(() =>
                {
                    if (getTimeDiag.Show("Select new Time", DateTime.Now.Hour, DateTime.Now.Minute))
                    {
                        DateTime current = DateTime.Now;
                        DateTime newDateTime = new DateTime(current.Year, current.Month, current.Day, getTimeDiag.SelectedHour, getTimeDiag.SelectedMinute, 0);
                        try
                        {
                            //TODO: emulator throws exception
                            Utility.SetLocalTime(newDateTime);
                            RealTimeClock.SetTime(newDateTime);
                        }
                        catch (Exception) { }
                    }
                })).Start();
            };

            setDate.ButtonPressed += (s) =>
            {
                new Thread(new ThreadStart(() =>
                {
                    if (getDateDiag.Show("Select new Date", DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year))
                    {
                        DateTime current = DateTime.Now;
                        DateTime newDateTime = new DateTime(getDateDiag.SelectedYear, getDateDiag.SelectedMonth, getDateDiag.SelectedDay, current.Hour, current.Minute, current.Second);
                        try
                        {
                            //TODO: emulator throws exception
                            Utility.SetLocalTime(newDateTime);
                            RealTimeClock.SetTime(newDateTime);
                        }
                        catch (Exception) { }
                    }
                })).Start();
            };

            butCalibrate.ButtonPressed += (s) => DesktopManager.Instance.StartCalibration();
        }

        private static void CreateSettingsPanel(Panel panel)
        {
            Label title = new Label("Settings", 5, 5, 200, Fonts.ArialMediumBold.Height) { Font = Fonts.ArialMediumBold };
            panel.AddChild(title);

            const int btMargin = 10, btHeight = 50;
            int btWidth = (panel.Width - btMargin) / 4;

            int x = 5;
            ToggleButton stNetwork, stAudio, stDisplay, stOther;
            panel.AddChild(stNetwork = new ToggleTextButton("Network", x, title.ScreenBottom + 15, btWidth, btHeight) { IsChecked = true, Tag = 0 }); x += btWidth;
            panel.AddChild(stAudio = new ToggleTextButton("Audio", x, title.ScreenBottom + 15, btWidth, btHeight) { Tag = 1 }); x += btWidth;
            panel.AddChild(stDisplay = new ToggleTextButton("Display", x, title.ScreenBottom + 15, btWidth, btHeight) { Tag = 2 }); x += btWidth;
            panel.AddChild(stOther = new ToggleTextButton("Other", x, title.ScreenBottom + 15, btWidth, btHeight) { Tag = 3 }); x += btWidth;

            Panel[] settingsPanels = new Panel[4];
            int visiblePanel = 0;

            UiEventHandler btCheckChanged = (s) =>
            {
                if (!((ToggleButton)s).IsChecked) return;
                settingsPanels[visiblePanel].Suspended = true;
                visiblePanel = (int)s.Tag;
                settingsPanels[visiblePanel].Suspended = false;
            };

            stNetwork.IsCheckedChanged += btCheckChanged;
            stAudio.IsCheckedChanged += btCheckChanged;
            stDisplay.IsCheckedChanged += btCheckChanged;
            stOther.IsCheckedChanged += btCheckChanged;

            int panelWidth = panel.Width - btMargin;
            int panelHeight = panel.Height - stNetwork.ScreenBottom - btMargin / 2;
            for (int i = 0; i < 4; i++)
                panel.AddChild(settingsPanels[i] = new Panel(btMargin / 2, stNetwork.ScreenBottom, panelWidth, panelHeight) { Suspended = visiblePanel != i });

            CreateNetworkSettingsPanel(settingsPanels[0]);
            CreateAudioSettingsPanel(settingsPanels[1]);
            CreateDisplaySettingsPanel(settingsPanels[2]);
            CreateOtherSettingsPanel(settingsPanels[3]);
        }

        private static void CreateUi()
        {
            Desktop desktop = DesktopManager.Instance.DefaultDesktop;
            desktop.Suspended = true;

            Bitmap
                imgMusic = Resources.Images.GetBitmap(Resources.Images.BitmapResources.imgMusic),
                imgRadio = Resources.Images.GetBitmap(Resources.Images.BitmapResources.imgRadio),
                imgWeather = Resources.Images.GetBitmap(Resources.Images.BitmapResources.imgWeather),
                imgSettings = Resources.Images.GetBitmap(Resources.Images.BitmapResources.imgSettings);

            int btSize = DesktopManager.ScreenHeight / 4;

            Panel[] visiblePanels = new Panel[4];
            int visiblePanel = 0;

            ToggleButton btMusic, btRadio, btWeather, btSettings;

            desktop.AddChild(btMusic = new ToggleImageButton(imgMusic, 0, btSize * 0, btSize, btSize) { IsChecked = true, Tag = 0 });
            desktop.AddChild(btRadio = new ToggleImageButton(imgRadio, 0, btSize * 1, btSize, btSize) { Tag = 1 });
            desktop.AddChild(btWeather = new ToggleImageButton(imgWeather, 0, btSize * 2, btSize, btSize) { Tag = 2 });
            desktop.AddChild(btSettings = new ToggleImageButton(imgSettings, 0, btSize * 3, btSize, btSize) { Tag = 3 });

            UiEventHandler btCheckChanged = (s) =>
            {
                if (!((ToggleButton)s).IsChecked) return;
                visiblePanels[visiblePanel].Suspended = true;
                visiblePanel = (int)s.Tag;
                visiblePanels[visiblePanel].Suspended = false;
            };

            btMusic.IsCheckedChanged += btCheckChanged;
            btRadio.IsCheckedChanged += btCheckChanged;
            btWeather.IsCheckedChanged += btCheckChanged;
            btSettings.IsCheckedChanged += btCheckChanged;

            int panelWidth = DesktopManager.ScreenWidth - btSize;
            for (int i = 0; i < 4; i++)
                desktop.AddChild(visiblePanels[i] = new Panel(btSize, 0, panelWidth, DesktopManager.ScreenHeight) { Suspended = visiblePanel != i });

            CreatePlayerPanel(visiblePanels[0]);
            CreateRadioPanel(visiblePanels[1]);
            CreateWeatherPanel(visiblePanels[2]);
            CreateSettingsPanel(visiblePanels[3]);

            desktop.Suspended = false;
        }

        private static void CreateScreenSaver()
        {
            Font clockFont = LocalFonts.GetFont(LocalFonts.FontResources.clockfont_72);
            Font dateTimeFont = Fonts.ArialBigBold;
            Desktop screenSaver = DesktopManager.Instance.AddNewDesktop();
            Desktop previous = null;

            //close screensaver on touchup
            screenSaver.TouchUp += (s, x, y) =>
            {
                if (previous != null)
                {
                    DesktopManager.Instance.SwitchDesktop(previous); //switch back to last desktop
                    AppConfig.LcdBacklightLevel = AppSettings.Instance.LcdBacklightLevel; //switch back the backligh level
                    return true;
                }
                return false;
            };

            #region screen saver drawing
            screenSaver.DrawBackground = (src, width, height) =>
            {
                src.DrawRectangle(0, 0, 0, 0, width, height, 0, 0, Colors.Black, 0, 0, Colors.Black, 0, 0, 256);

                string timeText = Util.ToString2Digit(DateTime.Now.Hour) + ":" + Util.ToString2Digit(DateTime.Now.Minute);
                int timeTxtWidth, timeTxtHeight;
                clockFont.ComputeExtent(timeText, out timeTxtWidth, out timeTxtHeight);

                int txtX = (width - timeTxtWidth) / 2, txtY = (height - timeTxtHeight) / 2;

                string txtDate = Util.GetLongDateString(DateTime.Now);
                int dateTxtWidth, dateTxtHeight;
                dateTimeFont.ComputeExtent(txtDate, out dateTxtWidth, out dateTxtHeight);
                txtY -= dateTxtHeight / 2 + 5;

                if (GetWeatherInfo != null)
                {
                    string txtWeather = GetWeatherInfo();
                    int txtWeatherWidth, txtWeatherHeight;
                    Fonts.ArialBigBold.ComputeExtent(txtWeather, out txtWeatherWidth, out txtWeatherHeight);
                    txtY -= txtWeatherHeight / 2 + 10;
                    src.DrawText(txtWeather, Fonts.ArialBigBold, Colors.Gray, (width - txtWeatherWidth) / 2, txtY + timeTxtHeight + dateTxtHeight + 20);
                }

                /*string txtRadio;
                if (GetRadioInfo != null && (txtRadio = GetRadioInfo()) != null)
                {
                    src.DrawText(txtRadio, Fonts.Arial, Colors.White, 0, 0);
                }*/

                src.DrawText(timeText, clockFont, Colors.Gray, txtX, txtY);
                src.DrawText(txtDate, dateTimeFont, Colors.Gray, (width - dateTxtWidth) / 2, txtY + timeTxtHeight + 10);
            };
            #endregion

            Timer refreshScreenSaverTimer = new Timer((o) => screenSaver.Refresh(), null, 0, 60 * 1000); //refresh screen saver every minute

            //when no input has been detected, show the screensaver
            DesktopManager.Instance.InputTimeout += () =>
            {
                AppConfig.LcdBacklightLevel = 31; //set to lowest (but not off)
                AppSettings.Instance.Save(); //save settings
                previous = DesktopManager.Instance.CurrentDesktop; //save a ref to current desktop
                DesktopManager.Instance.SwitchDesktop(screenSaver); //show the screen saver
            };

            DesktopManager.Instance.SetInputTimeout(AppSettings.Instance.ShowScreenSaverAfterSec * 1000);
        }

        public static void Main()
        {
            AppConfig.Init();
            DesktopManager.Instance.InputManager = InputManager.Current;

            VsDriver.VsExceptionRaised += (msg) =>
            {
                new Thread(() =>
                {
                    using (MessageBox mb = new MessageBox(msg, "Error", MessageBoxButtons.Ok, MessageBoxIcons.Error)) mb.Show();
                }).Start();
            };

            CreateUi();
            CreateScreenSaver();
            StorageManager i = StorageManager.Instance;
            Thread.Sleep(Timeout.Infinite);
            Debug.Print(i.ToString());
        }
    }
}
