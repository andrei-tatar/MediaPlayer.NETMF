using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.Hardware;
using GHIElectronics.NETMF.IO;
using Microsoft.SPOT.IO;
using GHIElectronics.NETMF.USBHost;
using System.Collections;
using System.Threading;

namespace Mp.App
{
    sealed class StorageManager
    {
        private static StorageManager _instance;
        public static StorageManager Instance { get { return _instance ?? (_instance = new StorageManager()); } }

        private InterruptPort _sdDetectPort;
        private PersistentStorage _cardStorage = null;
        private Hashtable _usbStorages;
        private Thread _cardMounter;

        private StorageManager()
        {
            _usbStorages = new Hashtable();

            _sdDetectPort = new InterruptPort(EMX.Pin.IO24, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);
            _sdDetectPort.OnInterrupt += new NativeEventHandler(_sdDetectPort_OnInterrupt);

            try
            {
                MountCardStorage();
            }
            catch (Exception)
            {
                //TODO: emulator throws exception
                return;
            }

            USBHostController.DeviceConnectedEvent += new USBH_DeviceConnectionEventHandler(USBHostController_DeviceConnectedEvent);
            USBHostController.DeviceDisconnectedEvent += new USBH_DeviceConnectionEventHandler(USBHostController_DeviceDisconnectedEvent);

            USBH_Device[] connectedDevices = USBHostController.GetDevices();
            foreach (USBH_Device connected in connectedDevices)
                USBHostController_DeviceConnectedEvent(connected);
        }

        void USBHostController_DeviceDisconnectedEvent(USBH_Device device)
        {
            if (device.TYPE == USBH_DeviceType.MassStorage && _usbStorages.Contains(device.ID))
            {
                PersistentStorage usbStorage = (PersistentStorage)_usbStorages[device.ID];
                usbStorage.UnmountFileSystem();
                usbStorage.Dispose();
                _usbStorages.Remove(device.ID);
            }
        }

        void USBHostController_DeviceConnectedEvent(USBH_Device device)
        {
            if (device.TYPE == USBH_DeviceType.MassStorage)
            {
                PersistentStorage usbStorage = new PersistentStorage(device);
                usbStorage.MountFileSystem();
                _usbStorages.Add(device.ID, usbStorage);
            }
        }

        private void _sdDetectPort_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (_sdDetectPort.Read())
            {
                ReleaseCardStorage();
            }
            else
            {
                if (_cardMounter == null)
                {
                    _cardMounter = new Thread(new ThreadStart(() =>
                    {
                        int tries = 5;
                        while (tries-- != 0)
                        {
                            if (MountCardStorage() || !_sdDetectPort.Read()) break;
                            Thread.Sleep(100);
                        }
                        _cardMounter = null;
                    }));
                    _cardMounter.Start();
                }
            }
        }

        private void ReleaseCardStorage()
        {
            lock (this)
            {
                if (_cardStorage != null)
                {
                    _cardStorage.UnmountFileSystem();
                    _cardStorage.Dispose();
                    _cardStorage = null;
                }
            }
        }

        private bool MountCardStorage()
        {
            if (PersistentStorage.DetectSDCard())
            {
                try
                {
                    lock (this)
                    {
                        _cardStorage = new PersistentStorage("SD");
                        _cardStorage.MountFileSystem();
                    }
                    return true;
                }
                catch (Exception)
                {
                    ReleaseCardStorage();
                    return false;
                }
            }
            return false;
        }
    }
}
