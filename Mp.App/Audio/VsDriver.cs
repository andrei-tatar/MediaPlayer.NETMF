using System;
using System.Threading;
using System.Runtime.CompilerServices;

using Mp.App.Resources;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.Native;
using GHIElectronics.NETMF.Hardware;
using GHIElectronics.NETMF.Hardware.LowLevel;

namespace Mp.App.Audio
{
    enum VsStatus
    {
        Playing,
        Paused,
        Stopped,
        Connecting,
        PlayingRadio
    }

    delegate void VsStatusChangedHandler(VsStatus newStatus);
    delegate void VsExceptionRaisedHandler(string msg);

    static partial class VsDriver
    {
        private const int BUFFER_SIZE = 4 * 1024; //double buffer used, so total size is 2 * BUFFER_SIZE

        private static bool initialized;
        private static byte[][] buffer;
        private static byte[] loadedPlugin;
        private static byte[] volume;
        private static byte volumeGeneral;
        private static sbyte balanceGeneral;
        private static byte bassGeneral;
        private static sbyte trebleGeneral;

        private static RLP.Procedure vsStreamData, vsNoMoreData, vsLoadPlugin, vsSetVolume, vsSetBassAndTreble;
        private static AutoResetEvent wait;
        private static VsStatus status;
        private static Thread vsThread;

        public static event VsStatusChangedHandler VsStatusChanged;
        public static event VsExceptionRaisedHandler VsExceptionRaised;

        public static VsStatus Status { get { return status; } }

        public const byte VolumeMax = 255, VolumeMin = 128;
        public const sbyte BalanceMax = 127, BalanceMin = -127;

        public const sbyte TrebleMin = -7, TrebleMax = 7;
        public const byte BassMin = 0, BassMax = 15;

        public static byte Volume
        {
            get { return volumeGeneral; }
            set
            {
                if (value <= VolumeMin) value = 0;
                if (value == volumeGeneral) return;
                volumeGeneral = value;
                RemakeVolumes();
            }
        }

        public static sbyte Balance
        {
            get { return balanceGeneral; }
            set
            {
                if (value == -128) value = -127;
                if (value == balanceGeneral) return;
                balanceGeneral = value;
                RemakeVolumes();
            }
        }

        public static sbyte Treble
        {
            get { return trebleGeneral; }
            set
            {
                if (value > TrebleMax) value = TrebleMax;
                else if (value < TrebleMin) value = TrebleMin;
                if (value == trebleGeneral) return;

                trebleGeneral = value;

                if (!initialized) return;
                vsSetBassAndTreble.Invoke(bassGeneral, trebleGeneral);
            }
        }

        public static byte Bass
        {
            get { return bassGeneral; }
            set
            {
                if (value > BassMax) value = BassMax;
                if (value == bassGeneral) return;

                bassGeneral = value;

                if (!initialized) return;
                vsSetBassAndTreble.Invoke(bassGeneral, trebleGeneral);
            }
        }

        private static void RaiseException(string msg)
        {
            if (VsExceptionRaised != null) VsExceptionRaised(msg);
        }

        static VsDriver()
        {
            buffer = new byte[2][];
            for (int i = 0; i < 2; i++) buffer[i] = new byte[BUFFER_SIZE];

            volume = new byte[2];
            initialized = false;

            wait = new AutoResetEvent(false);
            status = VsStatus.Stopped;
        }

        private static void RemakeVolumes()
        {
            if (!initialized) return;
            int volLeft = 255 - volumeGeneral;
            int volRight = 255 - volumeGeneral;
            if (balanceGeneral < 0)
            {
                volRight -= balanceGeneral;
                if (volRight > 255) volRight = 255;
            }
            else if (balanceGeneral > 0)
            {
                volLeft += balanceGeneral;
                if (volLeft > 255) volLeft = 255;
            }

            volume[0] = (byte)volLeft;
            volume[1] = (byte)volRight;
            vsSetVolume.InvokeEx(volume);
        }

        public static void Init(out RLP.Procedure lcdInit, out RLP.Procedure lcdSet, byte[] loadPlugin = null)
        {
            if (initialized) throw new Exception("Already initialized");

            //NOTE: use your own unlock code
            RLP.Unlock("TATAR.ANDREI@YAHOO.CO.UK2B819565", new byte[] 
            { 
                0xFB, 0xCC, 0x7D, 0xDA, 0x8E, 0x87, 0x58, 0xAB,
                0x95, 0x8E, 0x68, 0x02, 0x1F, 0xE6, 0xD6, 0x14, 
                0x3C, 0xBD, 0x3E, 0xD1, 0xBE, 0xF0, 0xCE, 0xFB, 
                0x1D, 0xD9, 0xC4, 0xEC, 0xE2, 0x2A, 0xFD, 0xCD
            });

            byte[] elf_file = Extension.GetBytes(Extension.BinaryResources.Extension);
            RLP.LoadELF(elf_file);
            RLP.InitializeBSSRegion(elf_file);
            RLP.Procedure VsInit = RLP.GetProcedure(elf_file, "VsInit");
            vsStreamData = RLP.GetProcedure(elf_file, "VsStreamData");
            vsNoMoreData = RLP.GetProcedure(elf_file, "VsNoMoreData");
            vsLoadPlugin = RLP.GetProcedure(elf_file, "VsLoadPluginFromArray");
            vsSetVolume = RLP.GetProcedure(elf_file, "VsSetVolume");
            vsSetBassAndTreble = RLP.GetProcedure(elf_file, "VsSetBassAndTreble");

            lcdInit = RLP.GetProcedure(elf_file, "LcdInit");
            lcdSet = RLP.GetProcedure(elf_file, "LcdSetLevel");

            elf_file = null;

            int result;
            if ((result = loadPlugin == null ? VsInit.Invoke() : VsInit.InvokeEx(loadPlugin, loadPlugin.Length)) != 0)
                throw new Exception("Could not init VS Driver. Reason " + result.ToString());
            loadedPlugin = loadPlugin;

            RLP.RLPEvent += (data, time) =>
            {
                switch (data)
                {
                    case 0x01:
                        //data was streamed 
                        wait.Set(); //notify any waiter
                        break;

                    case 0x02:
                        //error when closing stream
                        //this means the VS was reseted, 
                        //reload plugin, set volume, bass and treble
                        if (loadedPlugin != null) vsLoadPlugin.InvokeEx(loadedPlugin, loadedPlugin.Length);
                        RaiseException("Error closing stream!");
                        break;
                }
            };
            initialized = true;

            balanceGeneral = 0;
            Volume = 255; //max volume
            trebleGeneral = 0;
            bassGeneral = 0;
        }
    }
}
