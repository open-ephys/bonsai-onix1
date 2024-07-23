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
        /// Gets or sets a unique device name.
        /// </summary>
        /// <remarks>
        /// The device name provides a unique, human-readable identifier that is used to link software
        /// elements for configuration, control, and data streaming to hardware. This is often a one-to-one
        /// representation of an ONI device, but can also represent abstract ONI device aggregates or virtual devices.
        /// </remarks>
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the device address.
        /// </summary>
        /// <remarks>
        /// This address provides a fully-qualified location of a device within the device table. This is often a one-to-one
        /// representation of a ONI address, but can also represent abstract device addresses.
        /// </remarks>
        public uint DeviceAddress { get; set; }

        /// <summary>
        /// Gets or sets the device identity.
        /// </summary>
        /// <remarks>
        /// This type provides a device identity to each device within the device table. This is often a one-to-one
        /// representation of a ONI device ID, but can also represent abstract device identities.
        /// </remarks>
        [Browsable(false)]
        public Type DeviceType { get; }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return this;
        }
    }
}
