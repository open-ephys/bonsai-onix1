using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Bonsai;

namespace OpenEphys.Onix1
{

    internal interface IDeviceFactory
    {
        IEnumerable<IDeviceConfiguration> GetDevices();
    }

    /// <summary>
    /// Provides an abstract base class for all device configuration operators.
    /// </summary>
    /// <remarks>
    /// ONI devices usually require a specific sequence of configuration and parameterization
    /// steps before they can be interacted with. The <see cref="DeviceFactory"/> provides
    /// a modular abstraction for flexible assembly and sequencing of both single and multi-
    /// device configuration.
    /// </remarks>
    [WorkflowElementCategory(ElementCategory.Sink)]
    [Combinator]
    public abstract class DeviceFactory<TContext> : IDeviceFactory where TContext : ContextTask
    {

        internal abstract IEnumerable<IDeviceConfiguration<TContext>> GetDevices();

        IEnumerable<IDeviceConfiguration> IDeviceFactory.GetDevices() => GetDevices().Cast<IDeviceConfiguration>();

        /// <summary>
        /// Configure all devices in the device group.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> instance
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that hold configuration actions.</param>
        /// <returns>
        /// The original sequence modified by adding additional configuration actions required to configure
        /// all the devices in the device group.
        /// </returns>
        public abstract IObservable<TContext> Process(IObservable<TContext> source);


        internal const string ConfigurationCategory = DeviceFactory.ConfigurationCategory;
        internal const string AcquisitionCategory = DeviceFactory.AcquisitionCategory;
    }

    /// <summary>
    /// Static strings constant for DeviceFactory 
    /// </summary>
    public static class DeviceFactory
    {
        internal const string ConfigurationCategory = "Configuration";
        internal const string AcquisitionCategory = "Acquisition";
    }

    /// <summary>
    /// Static strings constant for SingleDeviceFactory
    /// </summary>
    public static class SingleDeviceFactory
    {
        internal const string DeviceNameDescription = "The unique device name.";
        internal const string DeviceAddressDescription = "The device address.";
    }

    /// <summary>
    /// Abstract base for configuration operators responsible for registering a single device within the
    /// internal device manager.
    /// </summary>
    /// <remarks>
    /// ONI devices usually require a specific sequence of configuration and parameterization steps before
    /// they can be interacted with. The <see cref="SingleDeviceFactory"/> provides a modular abstraction
    /// allowing flexible assembly and sequencing of of all device-specific configuration code.
    /// </remarks>
    public abstract class SingleDeviceFactory<TContext> : DeviceFactory<TContext>, IDeviceConfiguration<TContext>, IAddressableDevice where TContext : ContextTask
    {
        internal const string DeviceNameDescription = SingleDeviceFactory.DeviceNameDescription;
        internal const string DeviceAddressDescription = SingleDeviceFactory.DeviceAddressDescription;

        internal SingleDeviceFactory(Type deviceType)
        {
            DeviceType = deviceType ?? throw new ArgumentNullException(nameof(deviceType));
        }

        /// <summary>
        /// Gets or sets a unique device name.
        /// </summary>
        /// <remarks>
        /// The device name provides a unique, human-readable identifier that is used to link software
        /// elements for configuration, control, and data streaming to hardware. For instance, it can be used
        /// to link configuration operators to data IO operators within a workflow. This value is
        /// usually not set manually, but is assigned in a <see cref="MultiDeviceFactory"/> to correspond to a
        /// fixed address with a piece of hardware such as a headstage. This address is used for software
        /// communication.
        /// </remarks>
        [Description(DeviceNameDescription)]
        [Category(ConfigurationCategory)]
        [DeviceTableProperty(true)]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the device address.
        /// </summary>
        /// <remarks>
        /// This is a fully-qualified numerical hardware address of a device within the device table produced
        /// by an <see href="https://open-ephys.github.io/ONI/">ONI</see>-compliant
        /// acquisition system. This value is usually not set manually, but is assigned in a <see
        /// cref="MultiDeviceFactory"/> to correspond to a fixed address with a piece of hardware such as a
        /// headstage. This address is used for hardware communication.
        /// </remarks>
        [Description(DeviceAddressDescription)]
        [Category(ConfigurationCategory)]
        [DeviceTableProperty(true)]
        public uint DeviceAddress { get; set; }

        /// <summary>
        /// Gets or sets the device identity.
        /// </summary>
        /// <remarks>
        /// This type provides a device identity to each device within the device table produced by an <see
        /// href="https://open-ephys.github.io/ONI/">ONI</see>-compliant acquisition
        /// system.
        /// </remarks>
        [Browsable(false)]
        public Type DeviceType { get; }

        internal override IEnumerable<IDeviceConfiguration<TContext>> GetDevices()
        {
            yield return this;
        }

        IObservable<T> IDeviceConfiguration<TContext>.Process<T>(IObservable<T> source) => Process(source).Cast<T>();
    }
}
