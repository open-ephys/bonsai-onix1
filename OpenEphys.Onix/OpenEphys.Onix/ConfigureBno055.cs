using System;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    public class ConfigureBno055 : SingleDeviceFactory
    {
        public ConfigureBno055()
            : base(typeof(Bno055))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the BNO055 device is enabled.")]
        public bool Enable { get; set; } = true;

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDevice(deviceAddress, Bno055.ID);
                context.WriteRegister(deviceAddress, Bno055.ENABLE, Enable ? 1u : 0);

                var deviceInfo = new DeviceInfo(context, DeviceType, deviceAddress);
                var disposable = DeviceManager.RegisterDevice(deviceName, deviceInfo);
                return disposable;
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
