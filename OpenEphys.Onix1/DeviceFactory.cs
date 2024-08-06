using System;
using System.Collections.Generic;
using System.ComponentModel;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Provides an abstract base class for all device configuration operators.
    /// </summary>
    /// <remarks>
    /// ONI devices usually require a specific sequence of configuration and parameterization
    /// steps before they can be interacted with. The <see cref="DeviceFactory"/> provides
    /// a modular abstraction for flexible assembly and sequencing of both single and multi-
    /// device configuration.
    /// </remarks>
    public abstract class DeviceFactory : Sink<ContextTask>
    {
        internal const string ConfigurationCategory = "Configuration";
        internal const string AcquisitionCategory = "Acquisition";

        internal abstract IEnumerable<IDeviceConfiguration> GetDevices();
    }

    /// <summary>
    /// Provides an abstract base class for configuration operators responsible for
    /// registering a single device within the internal device manager.
    /// </summary>
    /// <remarks>
    /// ONI devices usually require a specific sequence of configuration and parameterization
    /// steps before they can be interacted with. The <see cref="SingleDeviceFactory"/>
    /// provides a modular abstraction allowing flexible assembly and sequencing of
    /// of all device-specific configuration code.
    /// </remarks>
    public abstract class SingleDeviceFactory : DeviceFactory, IDeviceConfiguration
    {
        internal const string DeviceNameDescription = "The unique device name.";
        internal const string DeviceAddressDescription = "The device address.";

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
        /// representation of a single <see cref="oni.Device"/>, but can also represent abstract ONI device
        /// aggregates or virtual devices.
        /// </remarks>
        [Description(DeviceNameDescription)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the device address.
        /// </summary>
        /// <remarks>
        /// This address provides a fully-qualified location of a device within the device table. This is often a one-to-one
        /// representation of a <see cref="oni.Device.Address"/>, but can also represent abstract device addresses.
        /// </remarks>
        [Description(DeviceAddressDescription)]
        public uint DeviceAddress { get; set; }

        /// <summary>
        /// Gets or sets the device identity.
        /// </summary>
        /// <remarks>
        /// This type provides a device identity to each device within the device table. This is often a one-to-one
        /// representation of a a <see cref="oni.Device.ID"/>, but can also represent abstract device identities.
        /// </remarks>
        [Browsable(false)]
        public Type DeviceType { get; }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return this;
        }
    }
}
