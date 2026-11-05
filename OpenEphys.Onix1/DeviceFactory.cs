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
    /// <typeparam name="TContext">
    /// A type derived from <see cref="ContextTask"/> that represents the 
    /// ONI controller that this device is compatible with.
    /// </typeparam>
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
        /// <param name="source">A sequence of <typeparamref name="TContext"/> instances that hold configuration actions.</param>
        /// <returns>
        /// The original sequence modified by adding additional configuration actions required to configure
        /// all the devices in the device group.
        /// </returns>
        public abstract IObservable<TContext> Process(IObservable<TContext> source);

        /// <summary>
        /// Configures all devices in the device group while preserving the derived context type of the input sequence.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> instance
        /// prior to data acquisition.
        /// </remarks>
        /// <typeparam name="T">
        /// The specific type of the context task, which must derive from <typeparamref name="TContext"/>.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of <typeparamref name="T"/> instances that hold configuration actions.
        /// </param>
        /// <returns>
        /// The original sequence of <typeparamref name="T"/> instances, modified by adding the configuration
        /// actions required to configure all the devices in the device group.
        /// </returns>
        public IObservable<T> Process<T>(IObservable<T> source) where T : TContext => Process((IObservable<TContext>)source).Cast<T>();


        internal const string ConfigurationCategory = DeviceFactoryStrings.ConfigurationCategory;
        internal const string AcquisitionCategory = DeviceFactoryStrings.AcquisitionCategory;
    }

    /// <summary>
    /// Static string constants shared by device factory classes.
    /// </summary>
    internal static class DeviceFactoryStrings
    {
        internal const string ConfigurationCategory = "Configuration";
        internal const string AcquisitionCategory = "Acquisition";
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
        internal const string DeviceNameDescription = DeviceFactoryStrings.DeviceNameDescription;
        internal const string DeviceAddressDescription = DeviceFactoryStrings.DeviceAddressDescription;

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
