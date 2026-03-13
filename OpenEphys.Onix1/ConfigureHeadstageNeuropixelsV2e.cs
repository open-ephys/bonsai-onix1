using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Threading;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a NeuropixelsV2e headstage on the specified port.
    /// </summary>
    /// <remarks>
    /// The NeuropixelsV2e Headstage is a 0.64g serialized, multifunction headstage designed to
    /// function with IMEC Neuropixels V2 probes. It provides the following features:
    /// <list type="bullet">
    /// <item><description>Support for dual IMEC Neuropixels 2.0 probes, each of which features:
    /// <list type="bullet">
    /// <item><description>Either 1x or 4x silicon shanks with a 70 x 24 µm cross-section.</description></item>
    /// <item><description>1280 electrodes low-impedance TiN electrodes per shank.</description></item>
    /// <item><description>384 parallel, full-band (AP, LFP), low-noise recording channels.</description></item>
    /// </list>
    /// </description></item>
    /// <item><description>A BNO055 9-axis IMU for real-time, 3D orientation tracking.</description></item>
    /// </list>
    /// </remarks>
    [Editor("OpenEphys.Onix1.Design.NeuropixelsV2eHeadstageEditor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    [Description("Configures a NeuropixelsV2e headstage.")]
    public class ConfigureHeadstageNeuropixelsV2e : MultiDeviceFactory
    {
        PortName port;
        readonly ConfigureHeadstageNeuropixelsV2ePortController PortControl = new();
        readonly ConfigureHeadstageNeuropixelsV2eDS90UB9x Serdes = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureHeadstageNeuropixelsV2e"/> class.
        /// </summary>
        public ConfigureHeadstageNeuropixelsV2e()
        {
            Port = PortName.PortA;
            PortControl.HubConfiguration = HubConfiguration.Passthrough;
        }

        /// <summary>
        /// Gets or sets the NeuropixelsV2 A configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the NeuropixelsV2 A.")]
        public ConfigureNeuropixelsV2PsbDecoder NeuropixelsV2A { get; set; } = new();

        /// <summary>
        /// Gets or sets the NeuropixelsV2 B configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the NeuropixelsV2 B.")]
        public ConfigureNeuropixelsV2PsbDecoder NeuropixelsV2B { get; set; } = new();

        /// <summary>
        /// Gets or sets the Bno055 9-axis inertial measurement unit configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Bno055 device.")]
        public ConfigurePolledBno055 Bno055 { get; set; } =
            new() { AxisMap = Bno055AxisMap.YZX, AxisSign = Bno055AxisSign.MirrorX | Bno055AxisSign.MirrorY };

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <remarks>
        /// The port is the physical connection to the ONIX breakout board and must be specified prior to operation.
        /// </remarks>
        [Description("Specifies the physical connection of the headstage to the ONIX breakout board.")]
        [Category(ConfigurationCategory)]
        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                var offset = (uint)port << 8;
                PortControl.DeviceAddress = (uint)port;
                NeuropixelsV2A.DeviceAddress = offset + 0;
                NeuropixelsV2B.DeviceAddress = offset + 1;
                Bno055.DeviceAddress = offset + 2;
                Serdes.DeviceAddress = offset + 3;
            }
        }

        /// <summary>
        /// Gets or sets the port voltage.
        /// </summary>
        /// <remarks>
        /// If a port voltage is defined this will override the automated voltage discovery and applies
        /// the specified voltage to the headstage. To enable automated voltage discovery, leave this field 
        /// empty. Warning: This device requires 3.0V to 5.5V for proper operation. Voltages higher than 5.5V can 
        /// damage the headstage
        /// </remarks>
        [Description("If defined, overrides automated voltage discovery and applies " +
            "the specified voltage to the headstage. Warning: this device requires 3.0V to 5.5V " +
            "for proper operation. Higher voltages can damage the headstage.")]
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(PortVoltageConverter))]
        public AutoPortVoltage PortVoltage
        {
            get => PortControl.PortVoltage;
            set => PortControl.PortVoltage = value;
        }

        internal override void UpdateDeviceNames()
        {
            PortControl.DeviceName = GetFullDeviceName(PortControl.DeviceType.Name);
            Serdes.DeviceName = GetFullDeviceName(Serdes.DeviceType.Name);
            NeuropixelsV2A.DeviceName = GetFullDeviceName(nameof(NeuropixelsV2A));
            NeuropixelsV2B.DeviceName = GetFullDeviceName(nameof(NeuropixelsV2B));
            Bno055.DeviceName = GetFullDeviceName(Bno055.DeviceType.Name);
        }

        private protected override void PrepareDevices() 
        {
            NeuropixelsV2A.StreamIndex = 0;
            NeuropixelsV2A.SelectProbe = serializer => ConfigureHeadstageNeuropixelsV2eDS90UB9x.SelectProbeA(serializer);
            NeuropixelsV2A.DeselectProbe = serializer => ConfigureHeadstageNeuropixelsV2eDS90UB9x.DeselectProbes(serializer);

            NeuropixelsV2B.StreamIndex = 1;
            NeuropixelsV2B.SelectProbe = serializer => ConfigureHeadstageNeuropixelsV2eDS90UB9x.SelectProbeB(serializer);
            NeuropixelsV2B.DeselectProbe = serializer => ConfigureHeadstageNeuropixelsV2eDS90UB9x.DeselectProbes(serializer);
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return PortControl;
            yield return Serdes; // must come before dependent devices that follow
            yield return NeuropixelsV2A;
            yield return NeuropixelsV2B;
            yield return Bno055;
        }
    }

    class ConfigureHeadstageNeuropixelsV2eDS90UB9x : ConfigureDS90UB9x
    {

        // Headstage-specific constants
        const byte GPO10SupplyMask = 1 << 3; // Used to turn on VDDA analog supply
        const byte GPO10ResetMask = 1 << 7; // Used to issue full reset commands to probes
        const byte DefaultGPO10Config = 0b0001_0001; // NPs in reset, VDDA not enabled
        const byte NoProbeSelected = 0b0001_0001; // No probes selected
        const byte ProbeASelected = 0b0001_1001; // TODO: Changes in Rev. B of headstage
        const byte ProbeBSelected = 0b1001_1001;

        public ConfigureHeadstageNeuropixelsV2eDS90UB9x()
            : base(typeof(DS90UB9x))
        {
        }

        override private protected void ConfigureSerdes(DeviceContext device)
        {
            // configure device via the DS90UB9x deserializer device
            device.WriteRegister(DS90UB9x.ENABLE, 0); // Default to disabled and let individual instances re-enable if they want

            // configure deserializer aliases and serializer power supply
            // configure deserializer trigger mode
            device.WriteRegister(DS90UB9x.TRIGGEROFF, 0);
            device.WriteRegister(DS90UB9x.TRIGGER, (uint)DS90UB9xTriggerMode.Continuous);
            device.WriteRegister(DS90UB9x.SYNCBITS, 0);
            device.WriteRegister(DS90UB9x.DATAGATE, (uint)DS90UB9xDataGate.Disabled);
            device.WriteRegister(DS90UB9x.MARK, (uint)DS90UB9xMarkMode.Disabled);

            // configure two 4-bit magic word-triggered streams, one for each probe
            device.WriteRegister(DS90UB9x.READSZ, 0x0010_0009); // 16 frames/superframe, 8x 12-bit words + magic bits
            device.WriteRegister(DS90UB9x.MAGIC_MASK, 0xC000003F); // Enable inverse, wait for non-inverse, 14-bit magic word
            device.WriteRegister(DS90UB9x.MAGIC, 0b0000_0000_0010_1110); // Super-frame sync word
            device.WriteRegister(DS90UB9x.MAGIC_WAIT, 0);
            device.WriteRegister(DS90UB9x.DATAMODE, 0b0010_0000_0000_0000_0000_0010_1011_0101);
            device.WriteRegister(DS90UB9x.DATALINES0, 0xFFFFF8A6); // NP A
            device.WriteRegister(DS90UB9x.DATALINES1, 0xFFFFF97B); // NP B

            DS90UB9x.Initialize933SerDesLink(device, DS90UB9xMode.Raw12BitHighFrequency);

            // configure deserializer I2C aliases
            var deserializer = new I2CRegisterContext(device, DS90UB9x.DES_ADDR);

            uint alias = NeuropixelsV2.ProbeAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID1, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias1, alias);

            alias = NeuropixelsV2.FlexEEPROMAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID2, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias2, alias);

            alias = PolledBno055.I2CAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID3, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias3, alias);

            var serializer = new I2CRegisterContext(device, DS90UB9x.SER_ADDR);
            ShutdownProbes(serializer); // ensure probes are powered down and deselected before starting
            DeselectProbes(serializer);
            EnableProbeSupply(serializer);

            // set I2C clock rate to ~400 kHz
            DS90UB9x.Set933I2CRate(device, 400e3);

            // issue full reset to both probes
            ResetProbes(serializer);
        }

        override private protected IDisposable ShutdownSerdes(DeviceContext device)
        {
            var serializer = new I2CRegisterContext(device, DS90UB9x.SER_ADDR);
            return Disposable.Create(() =>
            {
                ShutdownProbes(serializer);
                DeselectProbes(serializer);
            });
        }

        internal static void SelectProbeA(I2CRegisterContext serializer)
        {
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio32, ProbeASelected);
            Thread.Sleep(20);
        }

        internal static void SelectProbeB(I2CRegisterContext serializer)
        {
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio32, ProbeBSelected);
            Thread.Sleep(20);
        }

        internal static void DeselectProbes(I2CRegisterContext serializer)
        {
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio32, NoProbeSelected);
            Thread.Sleep(20);
        }

        static void EnableProbeSupply(I2CRegisterContext serializer)
        {
            var gpo10Config = serializer.ReadByte((uint)DS90UB933SerializerI2CRegister.Gpio10);
            gpo10Config |= GPO10SupplyMask;
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, gpo10Config);
            Thread.Sleep(20);
        }

        static void ResetProbes(I2CRegisterContext serializer)
        {
            var gpo10Config = serializer.ReadByte((uint)DS90UB933SerializerI2CRegister.Gpio10);
            gpo10Config = (byte)(gpo10Config & ~GPO10ResetMask);
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, gpo10Config);
            gpo10Config |= GPO10ResetMask;
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, gpo10Config);
        }

        static void ShutdownProbes(I2CRegisterContext serializer)
        {
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, DefaultGPO10Config);
        }
    }
}
