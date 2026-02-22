using System;
using System.ComponentModel;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures an array of Triad Semiconductor TS4231 lighthouse receivers for 3D position tracking using
    /// a pair of SteamVR V1 base stations.
    /// </summary>
    /// <remarks>
    /// This configuration operator can be linked to a data IO operator, such as <see
    /// cref="TS4231V1PositionData"/>, using a shared <c>DeviceName</c> to stream 3D
    /// position data from light-house receivers when SteamVR V1 base stations have been installed above the
    /// arena.
    /// </remarks>
    [Description("Configures a TS4231 receiver array.")]
    public class ConfigureTS4231V1 : SingleDeviceFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureTS4231V1"/> class.
        /// </summary>
        public ConfigureTS4231V1()
            : base(typeof(TS4231V1))
        {
        }

        /// <summary>
        /// Initializes a copy instance of the <see cref="ConfigureTS4231V1"/> class with the given values.
        /// </summary>
        /// <param name="configureTS4231V1">Existing configuration settings.</param>
        public ConfigureTS4231V1(ConfigureTS4231V1 configureTS4231V1)
            : this()
        {
            Enable = configureTS4231V1.Enable;
            DeviceAddress = configureTS4231V1.DeviceAddress;
            DeviceName = configureTS4231V1.DeviceName;
        }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, a <see cref="TS4231V1Data"/> instance that is linked to this configuration will produce data. If set to false,
        /// it will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the TS4231 device is enabled.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Configures a TS4231 receiver array.
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
            return source.ConfigureAndLatchDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);
                device.WriteRegister(TS4231V1.ENABLE, Enable ? 1u : 0);
                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }
    }

    static class TS4231V1
    {
        public const int ID = 25;
        public const uint MinimumVersion = 2;

        // managed registers
        public const uint ENABLE = 0x0; // Enable or disable the data output stream

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(TS4231V1))
            {
            }
        }
    }
}
