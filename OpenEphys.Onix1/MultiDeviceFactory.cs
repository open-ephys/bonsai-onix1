using System;
using System.ComponentModel;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Provides an abstract base class for configuration operators responsible for
    /// registering all devices in an ONI device aggregate in the context device table.
    /// </summary>
    /// <remarks>
    /// <para>
    /// ONI devices are often grouped into multi-device aggregates connected to hubs or
    /// headstages. These aggregates provide access to multiple devices through hub-specific
    /// addresses and usually require a specific sequence of configuration steps to determine
    /// operational port voltages and other link-specific settings.
    /// </para>
    /// <para>
    /// These multi-device aggregates are the most common starting point for configuration
    /// of an ONI system, and the <see cref="MultiDeviceFactory"/> provides a modular abstraction
    /// for flexible assembly and sequencing of multiple such aggregates.
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
        /// Gets or sets a unique hub device name.
        /// </summary>
        /// <inheritdoc cref = "SingleDeviceFactory.DeviceName"/>
        [Description("The unique hub device name.")]
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
        /// Configure all the ONI devices in the multi-device aggregate.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> instance
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that hold configuration actions.</param>
        /// <returns>
        /// The original sequence modified by adding additional configuration actions required to configure
        /// all the ONI devices in the multi-device aggregate.
        /// </returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            if (string.IsNullOrEmpty(_name))
            {
                throw new InvalidOperationException("A valid hub device name must be specified.");
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
