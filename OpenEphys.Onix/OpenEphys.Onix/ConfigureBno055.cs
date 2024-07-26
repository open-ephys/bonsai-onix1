using System;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    /// <summary>
    /// A class for configuring a Bosch BNO055 9-axis inertial measurement unit (IMU).
    /// </summary>
    /// <remarks>
    /// This configuration class can be linked to a <see cref="Bno055Data"/> instance to stream orientation data from the IMU.
    /// </remarks>
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
        /// Get or set the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, a <see cref="Bno055Data"/> instance that is linked to this configuration will produce data. If set to false,
        /// it will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the BNO055 device is enabled.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Configure a BNO055 device.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> instance
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that holds configuration actions.</param>
        /// <returns>The original sequence modified by adding additional configuration actions required to configure a BNO055 device.</returns>
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
