using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Bonsai;

namespace OpenEphys.Onix
{
    [TypeDescriptionProvider(typeof(DeviceHubTypeDescriptionProvider<BreakoutBoard>))]
    public class BreakoutBoard : DeviceFactory, INamedElement
    {
        string name;

        public BreakoutBoard() { Name = null; }

        [XmlArrayItem(typeof(ConfigureHeartbeat))]
        [XmlArrayItem(typeof(ConfigureAnalogIO))]
        public List<SingleDeviceFactory> Devices = new List<SingleDeviceFactory>
        {
            new ConfigureHeartbeat { DeviceIndex = 0 },
            new ConfigureAnalogIO { DeviceIndex = 6 }
        };

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                var baseName = string.IsNullOrWhiteSpace(name) ? nameof(BreakoutBoard) : name;
                foreach (var device in Devices)
                {
                    device.DeviceName = $"{baseName}/{device.DeviceType.Name}";
                }
            }
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            return Devices;
        }

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var output = source;
            foreach (var device in Devices)
            {
                output = device.Process(output);
            }

            return output;
        }
    }
}
