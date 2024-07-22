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

        /// <summary>
        /// Gets or sets a unique ONI device name.
        /// </summary>
        /// <remarks>
        /// The device name provides a unique, human-readable identifier that is used to link software
        /// elements for configuration, control, and data streaming to ONI devices.
        /// </remarks>
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the ONI device address.
        /// </summary>
        /// <remarks>
        /// This address provides a fully-qualified location of an ONI device within the device table.
        /// </remarks>
        public uint DeviceAddress { get; set; }

        /// <summary>
        /// Gets or sets the ONI device identity.
        /// </summary>
        [Browsable(false)]
        public Type DeviceType { get; }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return this;
        }
    }
}
