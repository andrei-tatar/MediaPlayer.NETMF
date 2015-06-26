using System;
using Microsoft.SPOT;

namespace Mp.App.Controls
{
    [Serializable]
    class RadioStationItem
    {
        public string Address;
        public string Name;

        [NonSerialized]
        public bool IsPlaying;
        [NonSerialized]
        public bool IsConnecting;

        public RadioStationItem(string address)
            : this(address, address) { }

        public RadioStationItem(string address, string name)
        {
            Address = address;
            Name = name;
            IsPlaying = false;
        }
    }
}
