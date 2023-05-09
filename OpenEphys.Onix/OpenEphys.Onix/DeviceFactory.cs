using System;
using System.Collections.Generic;
using Bonsai;

namespace OpenEphys.Onix
{
    public abstract class DeviceFactory : Sink<ContextTask>
    {
        internal abstract IEnumerable<IDeviceConfiguration> GetDevices();
    }

    public abstract class SingleDeviceFactory : DeviceFactory, IDeviceConfiguration
    {
        internal SingleDeviceFactory(Type deviceType)
        {
            DeviceType = deviceType ?? throw new ArgumentNullException(nameof(deviceType));
        }

        public string DeviceName { get; set; }

        protected Type DeviceType { get; }

        string IDeviceConfiguration.Name => DeviceName;

        Type IDeviceConfiguration.Type => DeviceType;

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return this;
        }
    }
}
