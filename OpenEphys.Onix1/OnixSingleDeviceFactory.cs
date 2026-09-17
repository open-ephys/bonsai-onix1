using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Abstract base for configuration operators responsible for registering a single device within the
    /// internal device manager. This is restricted to onix-specific devices which use <see cref="OnixContextTask"/>
    /// </summary>
    /// <remarks>
    /// ONI devices usually require a specific sequence of configuration and parameterization steps before
    /// they can be interacted with. The <see cref="OnixSingleDeviceFactory"/> provides a modular abstraction
    /// allowing flexible assembly and sequencing of of all device-specific configuration code.
    /// </remarks>
    [WorkflowElementCategory(ElementCategory.Sink)]
    [Combinator]
    public abstract class OnixSingleDeviceFactory : IDeviceCollection, IDeviceConfiguration, IAddressableDevice
    {
        internal const string DeviceNameDescription = SingleDeviceFactory.DeviceNameDescription;
        internal const string DeviceAddressDescription = SingleDeviceFactory.DeviceAddressDescription;
        internal const string ConfigurationCategory = SingleDeviceFactory.ConfigurationCategory;
        internal const string AcquisitionCategory = SingleDeviceFactory.AcquisitionCategory;
        internal OnixSingleDeviceFactory(Type deviceType)
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
        IEnumerable<IDeviceConfiguration> IDeviceCollection.GetDevices()
        {
            yield return this;
        }

        public abstract IObservable<OnixContextTask> Process(IObservable<OnixContextTask> context);

        IObservable<TContext> IDeviceConfiguration.Process<TContext>(IObservable<TContext> context)
        {
            // NB : This should never happen, this would mean an issue in the internal library
            if (!typeof(TContext).IsAssignableFrom(typeof(OnixContextTask)) && !typeof(OnixContextTask).IsAssignableFrom(typeof(TContext)))
            {
                throw new InvalidCastException($"Cannot cast {typeof(TContext).Name} to {nameof(OnixContextTask)}");
            }
            var onixContext = context.Cast<OnixContextTask>();
            var result = Process(onixContext);
            return (IObservable<TContext>)result;
        }
    }
}
