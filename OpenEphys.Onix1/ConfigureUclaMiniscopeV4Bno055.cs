﻿using System;
using System.ComponentModel;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures the Bno055 inertial measurement unit (IMU) on a UCLA Miniscope V4.
    /// </summary>
    public class ConfigureUclaMiniscopeV4Bno055 : SingleDeviceFactory
    {
        /// <summary>
        /// Initialize a new instance of a <see cref="ConfigureUclaMiniscopeV4Bno055"/> class.
        /// </summary>
        public ConfigureUclaMiniscopeV4Bno055()
            : base(typeof(UclaMiniscopeV4Bno055))
        {
        }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, <see cref="UclaMiniscopeV4Bno055Data"/> will produce data. If set to false, 
        /// <see cref="UclaMiniscopeV4Bno055Data"/> will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the BNO055 device is enabled.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Configures the Bno055 inertial measurement unit (IMU) on a UCLA Miniscope V4.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> node
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that holds all configuration actions.</param>
        /// <returns>
        /// The original sequence but with each <see cref="ContextTask"/> instance now containing configuration actions required to use the miniscope's Bno055 IMU.
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
                var deviceInfo = new UclaMiniscopeV4Bno055DeviceInfo(context, DeviceType, deviceAddress, enable);
                return DeviceManager.RegisterDevice(deviceName, deviceInfo);
            });
        }

        static void ConfigureDeserializer(DeviceContext device)
        {
            // configure deserializer I2C aliases
            var deserializer = new I2CRegisterContext(device, DS90UB9x.DES_ADDR);
            uint alias = UclaMiniscopeV4Bno055.BNO055Address << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID4, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias4, alias);
        }

        static void ConfigureBno055(DeviceContext device)
        {
            // setup BNO055 device
            // TODO: Correct orientation
            var i2c = new I2CRegisterContext(device, UclaMiniscopeV4Bno055.BNO055Address);
            i2c.WriteByte(0x3E, 0x00); // Power mode normal
            i2c.WriteByte(0x07, 0x00); // Page ID address 0
            i2c.WriteByte(0x3F, 0x00); // Internal oscillator
            i2c.WriteByte(0x41, 0b00000110);  // Axis map config (configured to match hs64; X => Z, Y => -Y, Z => X)
            i2c.WriteByte(0x42, 0b000000010); // Axis sign (negate Y)
            i2c.WriteByte(0x3D, 8); // Operation mode is NOF
        }
    }

    static class UclaMiniscopeV4Bno055
    {
        public const int BNO055Address = 0x28;
        public const int DataAddress = 0x1A;

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(UclaMiniscopeV4Bno055))
            {
            }
        }
    }

    class UclaMiniscopeV4Bno055DeviceInfo : DeviceInfo
    {
        public UclaMiniscopeV4Bno055DeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, bool enable)
            : base(context, deviceType, deviceAddress)
        {
            Enable = enable;
        }

        public bool Enable { get; }
    }
}