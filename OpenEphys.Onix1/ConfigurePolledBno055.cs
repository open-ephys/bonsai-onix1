using System;
using System.ComponentModel;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a Bosch Bno055 9-axis inertial measurement unit (IMU) to operate in nine degrees of freedom
    /// (NDOF) fusion mode that is polled by the host computer.
    /// </summary>
    /// <remarks>
    /// See page. 25 of the <a
    /// href="https://www.bosch-sensortec.com/media/boschsensortec/downloads/datasheets/bst-bno055-ds000.pdf">datasheet</a>
    /// for an explanation of NDOF mode. This configuration operator can be linked to a data IO operator,
    /// such as <see cref="PolledBno055Data"/>, using a shared <c>DeviceName</c>.
    /// </remarks>
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
        [Browsable(false)]
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
        [Browsable(false)]
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
            return source.ConfigureAndLatchDevice(context =>
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
            i2c.WriteByte(0x3D, 0x0C); // Operation mode is NDOF
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
    /// </summary>
    /// <remarks>
    /// The axes of the Bno055 can be reconfigured to account for differences in its mounting position. The
    /// following values can be applied to the Bno055's AXIS_MAP_CONFIG register at address 0x41 to
    /// rotate the Bno055's coordinate system compared to the default orientation presented on page 26 of the
    /// Bno055 <a
    /// href="https://www.bosch-sensortec.com/media/boschsensortec/downloads/datasheets/bst-bno055-ds000.pdf">datasheet</a>.
    /// Remapping is documented using the following notion: <b>Original Axis->New Axis'</b>. For instance,
    /// X->Z', indicates that what was the X-axis in the default configuration will be the Z axis in the remapped
    /// configuration.
    /// </remarks>
    public enum Bno055AxisMap : uint
    {
        /// <summary>
        /// Specifies that X->X', Y->Y', Z->Z' (chip default). 
        /// </summary>
        XYZ = 0b00_10_01_00,
        /// <summary>
        /// Specifies that X->X', Z->Y', Y->Z'
        /// </summary>
        XZY = 0b00_01_10_00,
        /// <summary>
        /// Specifies that Y->X', X->Y', Z->Z'
        /// </summary>
        YXZ = 0b00_10_00_01,
        /// <summary>
        /// Specifies that Y->X', Z->Y', X->Z'
        /// </summary>
        YZX = 0b00_00_10_01,
        /// <summary>
        /// Specifies that Z->X', X->Y', Y->Z'
        /// </summary>
        ZXY = 0b00_01_00_10,
        /// <summary>
        /// Specifies that Z->X', Y->Y', X->Z'
        /// </summary>
        ZYX = 0b00_00_01_10,
    }

    /// <summary>
    /// Specifies the axis map sign of a Bno055 IMU
    /// </summary>
    /// <remarks>
    /// <para>
    /// The axes of the Bno055 can be reconfigured to account for differences in its mounting position. The
    /// following values can be applied to the Bno055's AXIS_MAP_SIGN register at address 0x42 to mirror
    /// specific axes in the Bno055's coordinate system compared to the default directions presented on page
    /// 26 of the Bno055
    /// <a href="https://www.bosch-sensortec.com/media/boschsensortec/downloads/datasheets/bst-bno055-ds000.pdf">datasheet</a>.
    /// </para>
    /// <para>
    /// Note that this setting is applied after the axis map specified by <see cref="Bno055AxisMap"/> and
    /// applies to the new axis definitions.
    /// </para>
    /// </remarks>
    [Flags]
    public enum Bno055AxisSign : uint
    {
        /// <summary>
        /// Specifies that all axes are positive (chip default).
        /// </summary>
        Default = 0b00000_000,
        /// <summary>
        /// Specifies that Z' axis should be mirrored.
        /// </summary>
        MirrorZ = 0b00000_001,
        /// <summary>
        /// Specifies that Y' axis should be mirrored.
        /// </summary>
        MirrorY = 0b00000_010,
        /// <summary>
        /// Specifies that X' axis should be mirrored.
        /// </summary>
        MirrorX = 0b00000_100,
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

