using System;
using System.ComponentModel;
using System.Linq;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a Bosch Bno055 9-axis inertial measurement unit (IMU) that is polled by the host computer.
    /// </summary>
    /// <remarks>
    /// This configuration operator can be linked to a data IO operator, such as <see
    /// cref="PolledBno055Data"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
    [Editor("OpenEphys.Onix1.Design.PolledBno055Editor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    [Description("Configures a PolledBno055 device.")]
    public class ConfigurePolledBno055 : SingleDeviceFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurePolledBno055"/> class.
        /// </summary>
        public ConfigurePolledBno055()
            : base(typeof(PolledBno055))
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ConfigurePolledBno055"/> with its properties copied from an
        /// existing instance.
        /// </summary>
        /// <param name="configurePolledBno055">A pre-existing <see cref="ConfigurePolledBno055"/> object.</param>
        public ConfigurePolledBno055(ConfigurePolledBno055 configurePolledBno055)
            : base(typeof(PolledBno055))
        {
            Enable = configurePolledBno055.Enable;
            DeviceName = configurePolledBno055.DeviceName;
            DeviceAddress = configurePolledBno055.DeviceAddress;
            AxisMap = configurePolledBno055.AxisMap;
            AxisSign = configurePolledBno055.AxisSign;
        }

        /// <summary>
        /// Gets or sets a value specifying whether the Bno055 device is enabled.
        /// </summary>
        /// <remarks>
        /// If set to true, <see cref="PolledBno055Data"/> will produce data. If set to false, 
        /// <see cref="PolledBno055Data"/> will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the Bno055 device is enabled.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Gets or sets the axis map that will be applied during configuration.
        /// </summary>
        /// <remarks>
        /// This value can be changed to compensate for the Bno055's mounting orientation.
        /// Specifically, this value can be set to rotate the Bno055's coordinate system compared
        /// to the default orientation presented on page 24 of the Bno055 datasheet.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies the axis map that will be applied during configuration.")]
        public Bno055AxisMap AxisMap { get; set; } = Bno055AxisMap.XYZ;

        /// <summary>
        /// Gets or sets the axis sign that will be applied during configuration.
        /// </summary>
        /// <remarks>
        /// This value can be changed to compensate for the Bno055's mounting orientation.
        /// Specifically, this value can be set to mirror specific axes in the Bno055's coordinate
        /// system compared to the default orientation presented on page 24 of the Bno055 datasheet.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies axis sign that will be applied during configuration.")]
        public Bno055AxisSign AxisSign { get; set; } = Bno055AxisSign.Default;

        /// <summary>
        /// Configures a PolledBno055 device.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> node
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> that holds all configuration actions.</param>
        /// <returns>
        /// The original sequence with the side effect of an additional configuration action to configure
        /// a PolledBno055 device.
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
                var deviceInfo = new PolledBno055DeviceInfo(context, DeviceType, deviceAddress, enable);
                return DeviceManager.RegisterDevice(deviceName, deviceInfo);
            });
        }

        static void ConfigureDeserializer(DeviceContext device)
        {
            // configure deserializer I2C aliases
            var deserializer = new I2CRegisterContext(device, DS90UB9x.DES_ADDR);
            uint alias = PolledBno055.BNO055Address << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID4, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias4, alias);
        }

        void ConfigureBno055(DeviceContext device)
        {
            // setup Bno055 device
            var i2c = new I2CRegisterContext(device, PolledBno055.BNO055Address);
            i2c.WriteByte(0x3E, 0x00); // Power mode normal
            i2c.WriteByte(0x07, 0x00); // Page ID address 0
            i2c.WriteByte(0x3F, 0x00); // Internal oscillator
            i2c.WriteByte(0x41, (uint)AxisMap);  // Axis map config
            i2c.WriteByte(0x42, (uint)AxisSign); // Axis sign
            i2c.WriteByte(0x3D, 8); // Operation mode is NOF
        }
    }

    static class PolledBno055
    {
        public const int BNO055Address = 0x28;
        public const int EulerHeadingLsbAddress = 0x1A;

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(PolledBno055))
            {
            }
        }
    }

    class PolledBno055DeviceInfo : DeviceInfo
    {
        public PolledBno055DeviceInfo(ContextTask context, Type deviceType, uint deviceAddress, bool enable)
            : base(context, deviceType, deviceAddress)
        {
            Enable = enable;
        }

        public bool Enable { get; }
    }

    /// <summary>
    /// Specifies the axis map of a Bno055 compared to the default orientation.
    /// the datasheet.
    /// </summary>
    /// <remarks>
    /// The axis of the device can be reconfigured to the new reference axis to account for
    /// differences in its mounting position. The following values can be applied to the Bno055's
    /// AXIS_MAP_CONFIG register at address 0x41 in order to rotate the Bno055's coordinate system
    /// compared to the default orientation presented on page 24 of the Bno055 datasheet.
    /// </remarks>
    public enum Bno055AxisMap : uint
    {
        /// <summary>
        /// Specifies X->X, Y->Y, Z->Z (chip default).
        /// </summary>
        XYZ = 0b00_10_01_00,
        /// <summary>
        /// Specifies X->X, Y->Z, Z->Y.
        /// </summary>
        XZY = 0b00_01_10_00,
        /// <summary>
        /// Specifies X->Y, Y->X, Z->Z.
        /// </summary>
        YXZ = 0b00_10_00_01,
        /// <summary>
        /// Specifies X->Y, Y->Z, Z->X.
        /// </summary>
        YZX = 0b00_00_10_01,
        /// <summary>
        /// Specifies X->Z, Y->X, Z->Y.
        /// </summary>
        ZXY = 0b00_01_00_10,
        /// <summary>
        /// Specifies X->Z, Y->Y, Z->X.
        /// </summary>
        ZYX = 0b00_00_01_10,
    }

    /// <summary>
    /// Specifies the axis map sign of a Bno055 IMU
    /// </summary>
    /// <remarks>
    /// The axis of the device can be reconfigured to the new reference axis to account for
    /// differences in its mounting position. The following values can be applied to the Bno055's
    /// AXIS_MAP_SIGN register at address 0x42 to mirror specific axes in the Bno055's coordinate
    /// system compared to the default orientation presented on page 24 of the Bno055 datasheet.
    /// </remarks>
    [Flags]
    public enum Bno055AxisSign : uint
    {
        /// <summary>
        /// Specifies that all axes are positive (chip default).
        /// </summary>
        Default = 0b00000_000,
        /// <summary>
        /// Specifies that Z axis should be mirrored.
        /// </summary>
        MirrorZ = 0b00000_001,
        /// <summary>
        /// Specifies that Y axis should be mirrored.
        /// </summary>
        MirrorY = 0b00000_010,
        /// <summary>
        /// Specifies that X axis should be mirrored.
        /// </summary>
        MirrorX = 0b00000_100,
    }

    // NB: Can be used to remove axis map and sign properties from MultiDeviceFactories that include a
    // ConfigurePolledBno055 when having those options would cause confusion and potential
    // commutator malfunction
    internal class PolledBno055SingleDeviceFactoryConverter : SingleDeviceFactoryConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var properties = (from property in base.GetProperties(context, value, attributes).Cast<PropertyDescriptor>()
                              where !property.IsReadOnly &&
                                    !(property.PropertyType == typeof(Bno055AxisMap)) &&
                                    !(property.PropertyType == typeof(Bno055AxisSign)) &&
                                    property.ComponentType != typeof(SingleDeviceFactory)
                              select property)
                              .ToArray();
            return new PropertyDescriptorCollection(properties).Sort(properties.Select(p => p.Name).ToArray());
        }
    }

    /// <inheritdoc cref = "ConfigurePolledBno055"/>
    [Obsolete("This operator is obsolete. Use ConfigurePolledBno055 instead. Will be removed in version 1.0.0.")]
    public class ConfigureNeuropixelsV1eBno055 : ConfigurePolledBno055
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureNeuropixelsV1eBno055"/> class.
        /// </summary>
        public ConfigureNeuropixelsV1eBno055()
        {
            AxisMap = Bno055AxisMap.ZXY;
            AxisSign = Bno055AxisSign.MirrorZ | Bno055AxisSign.MirrorY;
        }
    }

    /// <inheritdoc cref = "ConfigurePolledBno055"/>
    [Obsolete("This operator is obsolete. Use ConfigurePolledBno055 instead. Will be removed in version 1.0.0.")]
    public class ConfigureNeuropixelsV2eBno055 : ConfigurePolledBno055
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureNeuropixelsV2eBno055"/> class.
        /// </summary>
        public ConfigureNeuropixelsV2eBno055()
        {
            AxisMap = Bno055AxisMap.ZXY;
            AxisSign = Bno055AxisSign.Default;
        }
    }

    /// <inheritdoc cref = "ConfigurePolledBno055"/>
    [Obsolete("This operator is obsolete. Use ConfigurePolledBno055 instead. Will be removed in version 1.0.0.")]
    public class ConfigureNeuropixelsV2eBetaBno055 : ConfigurePolledBno055
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureNeuropixelsV2eBetaBno055"/> class.
        /// </summary>
        public ConfigureNeuropixelsV2eBetaBno055()
        {
            AxisMap = Bno055AxisMap.ZXY;
            AxisSign = Bno055AxisSign.Default;
        }
    }
}

