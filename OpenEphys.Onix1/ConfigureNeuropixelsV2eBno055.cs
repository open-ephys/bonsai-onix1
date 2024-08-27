using System;
using System.ComponentModel;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a NeuropixelsV2eBno055 device.
    /// </summary>
    [Editor("OpenEphys.Onix1.Design.NeuropixelsV2eBno055Editor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    [Description("Configures a NeuropixelsV2eBno055 device.")]
    public class ConfigureNeuropixelsV2eBno055 : SingleDeviceFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureNeuropixelsV2eBno055"/> class.
        /// </summary>
        public ConfigureNeuropixelsV2eBno055()
            : base(typeof(NeuropixelsV2eBno055))
        {
        }

        /// <summary>
        /// Copy constructor for the <see cref="ConfigureNeuropixelsV2eBno055"/> class.
        /// </summary>
        /// <param name="configureNeuropixelsV2eBno055">A pre-existing <see cref="ConfigureNeuropixelsV2eBno055"/> object.</param>
        public ConfigureNeuropixelsV2eBno055(ConfigureNeuropixelsV2eBno055 configureNeuropixelsV2eBno055)
            : base(typeof(NeuropixelsV2eBno055))
        {
            Enable = configureNeuropixelsV2eBno055.Enable;
            DeviceName = configureNeuropixelsV2eBno055.DeviceName;
            DeviceAddress = configureNeuropixelsV2eBno055.DeviceAddress;
        }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, <see cref="NeuropixelsV2eBno055Data"/> will produce data. If set to false, 
        /// <see cref="NeuropixelsV2eBno055Data"/> will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the Bno055 device is enabled.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Configures a NeuropixelsV2eBno055 device.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> node
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> that holds all configuration actions.</param>
        /// <returns>
        /// The original sequence with the side effect of an additional configuration action to configure
        /// a NeuropixelsV2eBno055 device.
        /// </returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                // configure device via the DS90UB9x deserializer device
                var device = context.GetPassthroughDeviceContext(deviceAddress, typeof(DS90UB9x));
                ConfigureDeserializer(device);
                ConfigureBno055(device);
                var deviceInfo = new NeuropixelsV2eBno055DeviceInfo(context, DeviceType, deviceAddress, enable);
                return DeviceManager.RegisterDevice(deviceName, deviceInfo);
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
            // setup Bno055 device
            var i2c = new I2CRegisterContext(device, NeuropixelsV2eBno055.BNO055Address);
            i2c.WriteByte(0x3E, 0x00); // Power mode normal
            i2c.WriteByte(0x07, 0x00); // Page ID address 0
            i2c.WriteByte(0x3F, 0x00); // Internal oscillator
            i2c.WriteByte(0x41, 0b00010010);  // Axis map config (configured to match headstage; X => Z, Y => X, Z => Y)
            i2c.WriteByte(0x42, 0b00000000); // Axis sign set to default
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

    class NeuropixelsV2eBno055DeviceInfo : DeviceInfo
    {
        public NeuropixelsV2eBno055DeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, bool enable)
            : base(context, deviceType, deviceAddress)
        {
            Enable = enable;
        }

        public bool Enable { get; }
    }
}
