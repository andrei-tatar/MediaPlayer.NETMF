using System;
using Microsoft.SPOT;
using GHIElectronics.NETMF.Hardware;
using System.Runtime.CompilerServices;
using System.Collections;
using Mp.App.Controls;

namespace Mp.App
{
    [Serializable]
    class AppSettings
    {
        [NonSerialized]
        private static AppSettings _instance;
        public static AppSettings Instance
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return _instance ?? (_instance = Load()); }
        }

        public NetworkSettings Network;
        public AudioSettings Audio;
        public byte LcdBacklightLevel;
        public int ShowScreenSaverAfterSec;
        public ArrayList RadioStations;

        private AppSettings()
        {
            //default settings in constructor
            Network.DhcpEnabled = false;
            Network.IpAddress = new byte[] { 192, 168, 137, 2 };
            Network.NetMask = new byte[] { 255, 255, 255, 0 };
            Network.Gateway = new byte[] { 192, 168, 137, 1 };

            Network.DnsAddresses = new string[] { "8.8.8.8" };

            Network.UseProxy = false;
            Network.Proxy.Address = "http://10.11.1.1:3128";
            Network.Proxy.UseForRadio = false;

            Audio.Volume = 255;
            Audio.Balance = 0;
            Audio.Bass = 0;
            Audio.Treble = 0;

            LcdBacklightLevel = 0;
            ShowScreenSaverAfterSec = 20; //sec

            RadioStations = new ArrayList();
            //RadioStations.Add(new RadioStationItem("http://10.7.255.254:8000/kissfm.mp3", "KissFM Romania"));
            //RadioStations.Add(new RadioStationItem("http://10.7.255.254:8000/vibefm.mp3", "VIBE FM"));
            //RadioStations.Add(new RadioStationItem("http://10.7.255.254:8000/magicfm.mp3", "Magic FM Romania"));
            //RadioStations.Add(new RadioStationItem("http://10.7.255.254:8000/radioguerrilla.mp3", "Radio Guerrilla"));
            //RadioStations.Add(new RadioStationItem("http://10.7.255.254:8000/radio21.mp3", "Radio21 Romania"));
            //RadioStations.Add(new RadioStationItem("http://10.7.255.254:8000/europafm.mp3", "EuropaFM Romania"));

            RadioStations.Add(new RadioStationItem("http://stream.radiozu.ro:8020", "Radio ZU"));
            RadioStations.Add(new RadioStationItem("http://80.86.106.136:80", "KissFM Romania"));
            RadioStations.Add(new RadioStationItem("http://89.238.252.138:8000", "Radio21 Romania"));
            RadioStations.Add(new RadioStationItem("http://89.238.252.130:7000", "EuropaFM Romania"));
        }

        public static string IpToString(byte[] address)
        {
            return address[0] + "." + address[1] + "." + address[2] + "." + address[3];
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Save()
        {
            byte[] settings = Reflection.Serialize(this, typeof(AppSettings));
            if (settings.Length > 2044)
                //EMX has 2048 bytes of battery backed-up RAM. 4 bytes are reserved for size
                throw new Exception("Settings size exceed memory limit");

            byte[] settingsSize = new byte[4] { (byte)(settings.Length / 256), (byte)(settings.Length % 256), 0, 0 };
            int pages = settings.Length / 4 + (settings.Length % 4 == 0 ? 0 : 1);

            byte[] settingsPages = new byte[4 * pages];
            Array.Copy(settings, settingsPages, settings.Length);

            try
            {
                BatteryRAM.Write(0, settingsSize, 0, 4);
                BatteryRAM.Write(4, settingsPages, 0, pages * 4);
            }
            catch (Exception)
            {
                //TODO: emulator throws exception
            }
        }

        private static AppSettings Load()
        {
            try
            {
                throw new Exception();

                byte[] settingsSize = new byte[4];
                BatteryRAM.Read(0, settingsSize, 0, 4);

                int size = settingsSize[0] * 256 + settingsSize[1];
                if (size == 0 || size > 2044) throw new Exception();

                int pages = size / 4 + (size % 4 == 0 ? 0 : 1);

                byte[] aux = new byte[pages * 4];
                BatteryRAM.Read(4, aux, 0, pages * 4);

                byte[] settings = new byte[size];
                Array.Copy(aux, settings, size);

                return (AppSettings)Reflection.Deserialize(settings, typeof(AppSettings));
            }
            catch
            {
                return new AppSettings();
            }
        }
    }

    [Serializable]
    struct ProxySettings
    {
        public string Address;
        public bool UseForRadio;
    }

    [Serializable]
    struct NetworkSettings
    {
        public byte[] IpAddress;
        public byte[] NetMask;
        public byte[] Gateway;

        public bool DhcpEnabled;

        public string[] DnsAddresses;

        public bool UseProxy;
        public ProxySettings Proxy;
    }

    [Serializable]
    struct AudioSettings
    {
        public byte Volume;
        public sbyte Balance;
        public byte Bass;
        public sbyte Treble;
    }
}
