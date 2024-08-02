using System;
using System.ComponentModel;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Provides an abstract base class for configuration operators responsible for
    /// registering logical groups of <see cref="oni.Device"/>s.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The ONI standard states that devices are grouped into aggregates called hubs, each of which is
    /// governed by a single, potentially asynchronous clock and share a common base address. The devices on
    /// a headstage are an example of a hub. Devices within a hub are accessed through hub-specific addresses
    /// and usually require a specific sequence of configuration steps prior to acquisition.
    /// </para>
    /// <para>
    /// This class allows configuration of logical device groups of <see cref="oni.Device"/>s across ONI-defined
    /// hubs. For instance, the group of devices within a headstage (a single hub) can be combined with a device
    /// from another hub that is used to control its port voltage and communication status
    /// (e.g. <see cref="ConfigureHeadstage64"/>). Alternatively, diagnostic devices that are present within
    /// an ONI hub can be omitted from a device group to aid its useability (e.g. <see cref="ConfigureBreakoutBoard"/>).
    /// </para>
    /// <para>
    /// These device groups are the most common starting point for configuration
    /// of an ONI system, and the <see cref="MultiDeviceFactory"/> provides a modular abstraction for flexible
    /// assembly and sequencing of device groups.
    /// </para>
    /// </remarks>
    public abstract class MultiDeviceFactory : DeviceFactory, INamedElement
    {
        const string BaseTypePrefix = "Configure";
        string _name;

        internal MultiDeviceFactory()
        {
            var baseName = GetType().Name;
            var prefixIndex = baseName.IndexOf(BaseTypePrefix);
            Name = prefixIndex >= 0 ? baseName.Substring(prefixIndex + BaseTypePrefix.Length) : baseName;
        }

        /// <summary>
        /// Gets or sets a unique device group name.
        /// </summary>
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [Description("The unique device group name.")]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                UpdateDeviceNames();
            }
        }

        internal string GetFullDeviceName(string deviceName)
        {
            return !string.IsNullOrEmpty(_name) ? $"{_name}/{deviceName}" : string.Empty;
        }

        internal virtual void UpdateDeviceNames()
        {
            foreach (var device in GetDevices())
            {
                device.DeviceName = GetFullDeviceName(device.DeviceType.Name);
            }
        }

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
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            if (string.IsNullOrEmpty(_name))
            {
                throw new InvalidOperationException("A valid device group name must be specified.");
            }

            var output = source;
            foreach (var device in GetDevices())
            {
                output = device.Process(output);
            }

            return output;
        }
    }
}

