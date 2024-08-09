using System;
using System.ComponentModel;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Provides an abstract base class for configuration operators responsible for
    /// registering all devices within a logical group in the internal device manager.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class allows configuration of logical groups of devices that share some common functionality
    /// and/or require a specific sequence of interdependent configuration steps prior to acquisition. For
    /// instance, devices on a headstage can be combined with a device on the controller
    /// that is used to set the port voltage and monitor headstage communication status
    /// (e.g. <see cref="ConfigureHeadstage64"/>). Alternatively, devices that share some common functionality
    /// from the user's perspective, but share no actual interdependent configuration from the perspective of
    /// the hardware, can be grouped for ease of use (e.g. <see cref="ConfigureBreakoutBoard"/>).
    /// </para>
    /// <para>
    /// These device groups are the most common starting point for configuration
    /// of an ONIX system, and the <see cref="MultiDeviceFactory"/> provides a modular abstraction for flexible
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
        /// <remarks>
        /// A human-readable identifier that is used as a prefix for 
        /// the <see cref="SingleDeviceFactory.DeviceName"/> of each device in the the group. 
        /// </remarks>
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

