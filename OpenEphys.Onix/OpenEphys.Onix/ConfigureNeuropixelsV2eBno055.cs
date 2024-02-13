using System;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    public class ConfigureNeuropixelsV2eBno055 : SingleDeviceFactory
    {
        public ConfigureNeuropixelsV2eBno055()
            : base(typeof(NeuropixelsV2eBno055))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the BNO055 device is enabled.")]
        public bool Enable { get; set; } = true;

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                // configure device via the DS90UB9x deserializer device
                var device = context.GetPassthroughDeviceContext(deviceAddress, DS90UB9x.ID);
                ConfigureDeserializer(device);
                ConfigureBno055(device);
                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }

        static void ConfigureDeserializer(DeviceContext device)
        {
            // configure deserializer I2C aliases
            var deserializer = new I2CRegisterContext(device, DS90UB9x.DES_ADDR);
            uint alias = NeuropixelsV2eBno055.BNO055Address << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID4, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias4, alias);
        }

        static void ConfigureBno055(DeviceContext device)
        {
            // setup BNO055 device
            var i2c = new I2CRegisterContext(device, NeuropixelsV2eBno055.BNO055Address);
            i2c.WriteByte(0x3E, 0x00); // Power mode normal
            i2c.WriteByte(0x07, 0x00); // Page ID address 0
            i2c.WriteByte(0x3F, 0x00); // Internal oscillator
            i2c.WriteByte(0x41, 0b00000110);  // Axis map config (configured to match hs64; X => Z, Y => -Y, Z => X)
            i2c.WriteByte(0x42, 0b000000010); // Axis sign (negate Y)
            i2c.WriteByte(0x3D, 8); // Operation mode is NOF
        }
    }

    static class NeuropixelsV2eBno055
    {
        public const int BNO055Address = 0x28;
        public const int DataAddress = 0x1A;

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(NeuropixelsV2eBno055))
            {
            }
        }
    }
}
