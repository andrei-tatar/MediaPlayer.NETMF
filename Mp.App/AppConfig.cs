#define noUSE_LED_PWM

using System;
using System.Net;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Net.NetworkInformation;

using GHIElectronics.NETMF.System;
using GHIElectronics.NETMF.Hardware;
using GHIElectronics.NETMF.Native;

using Mp.App.Audio;
using Mp.App.Resources;
using GHIElectronics.NETMF.Net;

namespace Mp.App
{
    delegate void EventHandler();

    static class AppConfig
    {
        private static uint[] _outBuf;
        private static RLP.Procedure _lcdInit, _lcdSet;
        private static byte _lcdBackLevel;

        public static event EventHandler OnNetworkConnected;
        public static event EventHandler OnNetworkDisconnected;

#if USE_LED_PWM
        private static PWM _ledRed, _ledGreen;
        private static byte _ledRedValue, _ledGreenValue;
        private const int PWM_FREQ = 40000; //in Hz
#else
        private static OutputPort _ledRed, _ledGreen;
#endif

        /// <summary>0..100</summary>
        public static byte LedGreen
        {
            get
            {
#if USE_LED_PWM
                return _ledGreenValue; 
#else
                return _ledGreen.Read() ? (byte)1 : (byte)0;
#endif
            }
            set
            {
#if USE_LED_PWM
                if (value > 100) value = 100;
                if (value != _ledGreenValue) return;
                _ledGreenValue = value;
                _ledGreen.Set(PWM_FREQ, _ledGreenValue);
#else
                _ledGreen.Write(value != 0);
#endif
            }
        }

        /// <summary>0..100</summary>
        public static byte LedRed
        {
            get
            {
#if USE_LED_PWM
                return _ledRedValue;
#else
                return _ledRed.Read() ? (byte)1 : (byte)0;
#endif
            }
            set
            {
#if USE_LED_PWM
                if (value > 100) value = 100;
                if (value != _ledRedValue) return;
                _ledRedValue = value;
                _ledRed.Set(PWM_FREQ, _ledRedValue);
#else
                _ledRed.Write(value != 0);
#endif
            }
        }

        /// <summary>0..32, where 0 - most bright, 32 - off</summary>
        public static byte LcdBacklightLevel
        {
            get { return _lcdBackLevel; }
            set
            {
                if (value > 32) value = 32;
                if (_lcdBackLevel == value || _lcdSet == null) return;
                _lcdBackLevel = value;
                _lcdSet.Invoke(_lcdBackLevel);
            }
        }

        static AppConfig()
        {
            _outBuf = new uint[63];
            for (int i = 0; i < _outBuf.Length; i++)
                _outBuf[i] = 2;
        }

        public static void Init()
        {
            try
            {
                if (RealTimeClock.GetTime().Year < 2012) RealTimeClock.SetTime(new DateTime(2012, 01, 01, 12, 0, 0));
                Utility.SetLocalTime(RealTimeClock.GetTime());

                bool res = InitLcd();
                res |= SetHeap();
                res |= SetBoot();
                if (res) PowerState.RebootDevice(false);

#if USE_LED_PWM
                _ledGreen = new PWM(PWM.Pin.PWM3);
                _ledGreenValue = 0;
                _ledGreen.Set(false);

                _ledRed = new PWM(PWM.Pin.PWM2);
                _ledRedValue = 0;
                _ledRed.Set(false);
#else
                _ledGreen = new OutputPort(EMX.Pin.IO49, false);
                _ledRed = new OutputPort(EMX.Pin.IO50, false);
#endif

                InitNetwork();

                LcdBacklightLevel = AppSettings.Instance.LcdBacklightLevel;

                VsDriver.Init(out _lcdInit, out _lcdSet);//Mp.App.Resources.Plugins.GetBytes(Mp.App.Resources.Plugins.BinaryResources.Flac_v140));
                if (_lcdInit.Invoke(48) != 0) throw new Exception("Could not init LCD backlight"); //IO48 is for LCD backlight
                if (_lcdSet.Invoke(LcdBacklightLevel) != 0) throw new Exception("Could not set LCD backlight level");

                VsDriver.Volume = AppSettings.Instance.Audio.Volume;
                VsDriver.Balance = AppSettings.Instance.Audio.Balance;
                VsDriver.Bass = AppSettings.Instance.Audio.Bass;
                VsDriver.Treble = AppSettings.Instance.Audio.Treble;

                PowerState.OnRebootEvent += (soft) =>
                {
                    AppSettings.Instance.Save();
                };
            }
            catch (Exception)
            { }
        }

        private static void SetNetwokSettings()
        {
            NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[0];

            NetworkSettings netSettings = AppSettings.Instance.Network;
            if (netSettings.DhcpEnabled)
            {
                if (ni.IsDhcpEnabled)
                    ni.RenewDhcpLease();
                else
                    ni.EnableDhcp();

                if (netSettings.DnsAddresses != null)
                    ni.EnableStaticDns(netSettings.DnsAddresses);
            }
            else
            {
                ni.EnableStaticIP(
                    AppSettings.IpToString(netSettings.IpAddress),
                    AppSettings.IpToString(netSettings.NetMask),
                    AppSettings.IpToString(netSettings.Gateway));
                if (netSettings.DnsAddresses != null)
                    ni.EnableStaticDns(netSettings.DnsAddresses);
            }

            if (netSettings.UseProxy && netSettings.Proxy.UseForRadio)
                WebRequest.DefaultWebProxy = new WebProxy(netSettings.Proxy.Address);
        }

        private static void InitNetwork()
        {
            if (!Ethernet.IsEnabled) Ethernet.Enable();

            Thread checkNetwork = new Thread(new ThreadStart(() =>
            {
                bool wasNetworkAvailable = false;
                while (true)
                {
                    if (Ethernet.IsCableConnected)
                    {
                        if (!wasNetworkAvailable)
                        {
                            wasNetworkAvailable = true;
                            SetNetwokSettings();
                            if (OnNetworkConnected != null) OnNetworkConnected();
                        }
                    }
                    else
                    {
                        if (wasNetworkAvailable)
                        {
                            wasNetworkAvailable = false;
                            if (OnNetworkDisconnected != null) OnNetworkDisconnected();
                        }
                    }
                    Thread.Sleep(3000);
                }
            }));
            checkNetwork.Start();
        }

        private static bool InitLcd()
        {
            //PSP lcd config 4.3" - 480x272
            Configuration.LCD.Configurations lcdConfig = new Configuration.LCD.Configurations();
            lcdConfig.PriorityEnable = false;

            lcdConfig.OutputEnableIsFixed = true;
            lcdConfig.OutputEnablePolarity = true;

            lcdConfig.PixelClockDivider = 8; //9MHz clock
            lcdConfig.PixelPolarity = false; //falling edge

            //clocks per line = 525
            lcdConfig.HorizontalSyncPolarity = false;
            lcdConfig.HorizontalSyncPulseWidth = 41;
            lcdConfig.Width = 480;
            lcdConfig.HorizontalBackPorch = 2;
            lcdConfig.HorizontalFrontPorch = 2;

            //lines per frame = 286
            lcdConfig.VerticalSyncPolarity = false;
            lcdConfig.VerticalSyncPulseWidth = 10;
            lcdConfig.Height = 272;
            lcdConfig.VerticalBackPorch = 2;
            lcdConfig.VerticalFrontPorch = 2;

            return Configuration.LCD.Set(lcdConfig);
        }

        private static bool SetHeap()
        {
            return Configuration.Heap.SetCustomHeapSize(2 * 1024 * 1024);
        }

        private static bool SetBoot()
        {
            //Bitmap bmp = Images.GetBitmap(Images.BitmapResources.bootLogo);
            //bool res = Configuration.StartUpLogo.Set(bmp.GetBitmap(), (int)(((float)Mp.Ui.Managers.DesktopManager.ScreenWidth / 2) - 90), (int)(((float)Mp.Ui.Managers.DesktopManager.ScreenHeight / 2) - 90));
            //Configuration.StartUpLogo.Enabled = true;

            //bool res2 = Configuration.LCD.EnableLCDBootupMessages(false);

            //return res || res2;

            //Configuration.StartUpLogo.Enabled = false;
            //Configuration.LCD.EnableLCDBootupMessages(true);
            return false;
        }
    }
}

