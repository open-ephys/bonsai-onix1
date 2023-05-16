using System;
using System.Collections.Generic;
using System.ComponentModel;
using Bonsai;

namespace OpenEphys.Onix
{
    public abstract class DeviceFactory : Sink<ContextTask>
    {
        internal const string ConfigurationCategory = "Configuration";
        internal const string AcquisitionCategory = "Acquisition";

        internal abstract IEnumerable<IDeviceConfiguration> GetDevices();
    }

    public abstract class SingleDeviceFactory : DeviceFactory, IDeviceConfiguration
    {
        internal SingleDeviceFactory(Type deviceType)
        {
            DeviceType = deviceType ?? throw new ArgumentNullException(nameof(deviceType));
        }

        public string DeviceName { get; set; }

        public uint DeviceIndex { get; set; }

        [Browsable(false)]
        public Type DeviceType { get; }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return this;
        }
    }

    public abstract class HubDeviceFactory : DeviceFactory
    {
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var output = source;
            foreach (var device in GetDevices())
            {
                output = device.Process(output);
            }

            return output;
        }
    }
}
