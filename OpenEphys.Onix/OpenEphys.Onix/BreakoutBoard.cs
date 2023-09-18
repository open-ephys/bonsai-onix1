using System;
using System.Collections.Generic;
using System.ComponentModel;
using Bonsai;

namespace OpenEphys.Onix
{
    public class BreakoutBoard : HubDeviceFactory, INamedElement
    {
        string name;

        public BreakoutBoard() { Name = null; }

        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureHeartbeat Heartbeat { get; set; } = new();

        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureAnalogIO AnalogIO { get; set; } = new();

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return Heartbeat;
            yield return AnalogIO;
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                var baseName = string.IsNullOrWhiteSpace(name) ? nameof(BreakoutBoard) : name;
                foreach (var device in GetDevices())
                {
                    device.DeviceName = $"{baseName}/{device.DeviceType.Name}";
                }
            }
        }
    }
}
