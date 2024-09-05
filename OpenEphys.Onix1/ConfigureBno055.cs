using System;
using System.ComponentModel;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a Bosch Bno055 9-axis inertial measurement unit (IMU).
    /// </summary>
    /// <remarks>
    /// This configuration operator can be linked to a data IO operator, such as <see cref="Bno055Data"/>,
    /// using a shared <see cref="SingleDeviceFactory.DeviceName"/>.
    /// </remarks>
    [Description("Configures a Bosch Bno055 9-axis inertial measurement unit.")]
    [Editor("OpenEphys.Onix1.Design.Bno055Editor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    public class ConfigureBno055 : SingleDeviceFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureBno055"/> class.
        /// </summary>
        public ConfigureBno055()
            : base(typeof(Bno055))
        {
        }

        /// <summary>
        /// Copy constructor for the <see cref="ConfigureBno055"/> class.
        /// </summary>
        /// <param name="configureBno055">Existing <see cref="ConfigureBno055"/> object to be copied.</param>
        public ConfigureBno055(ConfigureBno055 configureBno055)
            : base(typeof(Bno055))
        {
            Enable = configureBno055.Enable;
            DeviceName = configureBno055.DeviceName;
            DeviceAddress = configureBno055.DeviceAddress;
        }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, a <see cref="Bno055Data"/> instance that is linked to this configuration will produce data. If set to false,
        /// it will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the Bno055 device is enabled.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Configures a Bosch Bno055 9-axis IMU device.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> instance
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that holds configuration actions.</param>
        /// <returns>The original sequence modified by adding additional configuration actions required to configure a Bno055 device.</returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);
                device.WriteRegister(Bno055.ENABLE, Enable ? 1u : 0);
                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }
    }

    static class Bno055
    {
        public const int ID = 9;

        // constants
        public const float EulerAngleScale = 1f / 16; // 1 degree = 16 LSB
        public const float QuaternionScale = 1f / (1 << 14); // 1 = 2^14 LSB
        public const float AccelerationScale = 1f / 100; // 1m / s^2 = 100 LSB

        // managed registers
        public const uint ENABLE = 0x0; // Enable or disable the data output stream

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Bno055))
            {
            }
        }
    }
}
