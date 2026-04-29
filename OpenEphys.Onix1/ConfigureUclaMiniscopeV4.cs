using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a UCLA Miniscope V4 on the specified port.
    /// </summary>
    /// <remarks>
    /// The UCLA Miniscope V4 is a miniaturized fluorescent microscope for performing single-photon calcium
    /// imaging in freely moving animals. It has the following features:
    /// <list type="bullet">
    /// <item><description>A Python-480 0.48 Megapixel CMOS image sensor.</description></item>
    /// <item><description>A BNO055 9-axis IMU for real-time, 3D orientation tracking.</description></item>
    /// <item><description>An electrowetting lens for remote focal plane adjustment.</description></item>
    /// <item><description>An excitation LED with adjustable brightness control and optional exposure-driven
    /// interleaving to reduce photobleaching.</description></item>
    /// </list>
    /// </remarks>
    public class ConfigureUclaMiniscopeV4 : MultiDeviceFactory
    {

        PortName port;
        readonly ConfigureUclaMiniscopeV4PortController PortControl = new();
        readonly ConfigureUclaMiniscopeV4DS90UB9x Serdes = new();

        /// <summary>
        /// Initialize a new instance of a <see cref="ConfigureUclaMiniscopeV4"/> class.
        /// </summary>
        public ConfigureUclaMiniscopeV4()
        {
            Port = PortName.PortA;
            PortControl.HubConfiguration = HubConfiguration.Passthrough;
        }

        /// <summary>
        /// Gets or sets the Miniscope camera configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        public ConfigureUclaMiniscopeV4Camera Camera { get; set; } = new();

        /// <summary>
        /// Gets or sets the Bno055 9-axis inertial measurement unit configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Bno055 device.")]
        public ConfigurePolledBno055 Bno055 { get; set; } =
            new ConfigurePolledBno055 { AxisMap = Bno055AxisMap.ZYX, AxisSign = Bno055AxisSign.MirrorZ };

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <remarks>
        /// The port is the physical connection to the ONIX breakout board and must be specified prior to operation.
        /// </remarks>
        [Description("Specifies the physical connection of the miniscope to the ONIX breakout board.")]
        [Category(ConfigurationCategory)]
        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                var offset = (uint)port << 8;
                PortControl.DeviceAddress = (uint)port;
                Camera.DeviceAddress = offset + 0;
                Bno055.DeviceAddress = offset + 1;
                Serdes.DeviceAddress = offset + 2;
            }
        }

        /// <summary>
        /// Gets or sets the port voltage.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The port voltage applied to the miniscope. Because ONIX allows any coaxial tether to be used, some of
        /// which are thin enough to result in a significant voltage drop, its may be required to manually specify the
        /// port voltage.
        /// </para>
        /// <para>
        /// Warning: this device requires 4.0 to 5.0V, measured at the miniscope, for proper operation. Supplying higher
        /// voltages may result in damage.
        /// </para>
        /// </remarks>
        [Description("The port voltage applied to the miniscope. Warning: this device requires 4.0 to 5.0V, measured at the scope, " +
                     "for proper operation. Supplying higher voltages may result in damage to the miniscope.")]
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(PortVoltageConverter))]
        public AutoPortVoltage PortVoltage
        {
            get => PortControl.PortVoltage;
            set => PortControl.PortVoltage = value;
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return PortControl;
            yield return Serdes; // must come before dependent devices that follow
            yield return Camera;
            yield return Bno055;
        }
    }

    class ConfigureUclaMiniscopeV4DS90UB9x : ConfigureDS90UB9x
    {
        public ConfigureUclaMiniscopeV4DS90UB9x()
            : base(typeof(DS90UB9x))
        {
        }

        override private protected IDisposable ShutdownSerdes(DeviceContext device)
        {
            return Disposable.Empty;
        }

        override private protected void ConfigureSerdes(DeviceContext device)
        {
            // configure deserializer
            device.WriteRegister(DS90UB9x.TRIGGEROFF, 0);
            device.WriteRegister(DS90UB9x.READSZ, UclaMiniscopeV4.SensorColumns);
            device.WriteRegister(DS90UB9x.TRIGGER, (uint)DS90UB9xTriggerMode.HsyncEdgePositive);
            device.WriteRegister(DS90UB9x.SYNCBITS, 0);
            device.WriteRegister(DS90UB9x.DATAGATE, (uint)DS90UB9xDataGate.VsyncPositive);

            // NB: This is required because Bonsai is not guaranteed to capture every frame at the start of
            // acquisition. For this reason, the frame start needs to be marked.
            device.WriteRegister(DS90UB9x.MARK, (uint)DS90UB9xMarkMode.VsyncRising);

            // The camera does not rely on magic words for data alignment
            device.WriteRegister(DS90UB9x.MAGIC_MASK, 0);
            device.WriteRegister(DS90UB9x.MAGIC, 0);
            device.WriteRegister(DS90UB9x.MAGIC_WAIT, 0);
            device.WriteRegister(DS90UB9x.DATAMODE, 0);
            device.WriteRegister(DS90UB9x.DATALINES0, 0);
            device.WriteRegister(DS90UB9x.DATALINES1, 0);

            DS90UB9x.Initialize933SerDesLink(device, DS90UB9xMode.Raw12BitLowFrequency);

            var deserializer = new I2CRegisterContext(device, DS90UB9x.DES_ADDR);

            uint alias = UclaMiniscopeV4.AtMegaAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID1, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias1, alias);

            alias = UclaMiniscopeV4.Tpl0102Address << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID2, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias2, alias);

            alias = UclaMiniscopeV4.Max14574Address << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID3, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias3, alias);

            alias = PolledBno055.I2CAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID4, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias4, alias);

            // set I2C clock rate to ~200 kHz
            DS90UB9x.Set933I2CRate(device, UclaMiniscopeV4.NominalI2cClockRate);
        }
    }
}
