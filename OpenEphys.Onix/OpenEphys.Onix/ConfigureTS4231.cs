using System;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    /// <summary>
    /// A class for configuring an array of Triad Semiconductor TS4231 lighthouse receivers for 3D position tracking using
    /// a pair of SteamVR V1 base stations.
    /// </summary>
    /// <remarks>
    /// This configuration class can be linked to a <see cref="TS4231V1GeometricData"/> instance to stream 3D position data from
    /// light-house receivers when SteamVR V1 base stations have been installed above the arena.
    /// </remarks>
    public class ConfigureTS4231 : SingleDeviceFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureTS4231"/> class.
        /// </summary>
        public ConfigureTS4231()
            : base(typeof(TS4231))
        {
        }

        /// <summary>
        /// Get or set the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, a <see cref="TS4231Data"/> instance that is linked to this configuration will produce data. If set to false,
        /// it will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the TS4231 device is enabled.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Configure a TS4231 receiver array.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> instance
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that holds configuration actions.</param>
        /// <returns>The original sequence modified by adding additional configuration actions required to configure a TS4231 array.</returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);
                device.WriteRegister(TS4231.ENABLE, Enable ? 1u : 0);
                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }
    }

    static class TS4231
    {
        public const int ID = 25;

        // managed registers
        public const uint ENABLE = 0x0; // Enable or disable the data output stream

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(TS4231))
            {
            }
        }
    }
}
