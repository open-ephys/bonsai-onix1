using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Threading;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a NeuropixelsV2eBeta headstage on the specified port.
    /// </summary>
    /// <remarks>
    /// The NeuropixelsV2eBeta Headstage is a 0.64g serialized, multifunction headstage designed to
    /// function with IMEC Neuropixels V2Beta probes. It provides the following features:
    /// <list type="bullet">
    /// <item><description>Support for dual IMEC Neuropixels 2.0-Beta probes, each of which features:
    /// <list type="bullet">
    /// <item><description>4x silicon shanks with a 70 x 24 µm cross-section.</description></item>
    /// <item><description>1280 electrodes low-impedance TiN electrodes per shank.</description></item>
    /// <item><description>384 parallel, full-band (AP, LFP), low-noise recording channels.</description></item>
    /// </list>
    /// </description></item>
    /// <item><description>A BNO055 9-axis IMU for real-time, 3D orientation tracking.</description></item>
    /// </list>
    /// </remarks>
    [Editor("OpenEphys.Onix1.Design.NeuropixelsV2eHeadstageEditor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    [Description("Configures a NeuropixelsV2eBeta headstage.")]
    public class ConfigureHeadstageNeuropixelsV2eBeta : MultiDeviceFactory
    {
        PortName port;
        readonly ConfigureHeadstageNeuropixelsV2eBetaPortController PortControl = new();
        readonly ConfigureHeadstageNeuropixelsV2eBetaDS90UB9x Serdes = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureHeadstageNeuropixelsV2eBeta"/> class.
        /// </summary>
        public ConfigureHeadstageNeuropixelsV2eBeta()
        {
            Port = PortName.PortA;
            PortControl.HubConfiguration = HubConfiguration.Passthrough;
        }

        /// <summary>
        /// Gets or sets the NeuropixelsV2-Beta probe A configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for NeuropixelsV2-Beta probe A.")]
        public ConfigureNeuropixelsV2BetaPsbDecoder NeuropixelsV2A { get; set; } = new();

        /// <summary>
        /// Gets or sets the NeuropixelsV2-Beta probe B configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for NeuropixelsV2-Beta probe B.")]
        public ConfigureNeuropixelsV2BetaPsbDecoder NeuropixelsV2B { get; set; } = new();

        /// <summary>
        /// Gets or sets the Bno055 9-axis inertial measurement unit configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Bno055 device.")]
        public ConfigurePolledBno055 Bno055 { get; set; } =
            new ConfigurePolledBno055 { AxisMap = Bno055AxisMap.YZX, AxisSign = Bno055AxisSign.MirrorX | Bno055AxisSign.MirrorY };

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
        /// empty. Warning: This device requires 3.0V to 5.0V for proper operation. Voltages higher than 5.0V can
        /// damage the headstage
        /// </remarks>
        [Description("If defined, overrides automated voltage discovery and applies " +
            "the specified voltage to the headstage. Warning: this device requires 3.0V to 5.0V " +
            "for proper operation. Higher voltages can damage the headstage.")]
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(PortVoltageConverter))]
        public AutoPortVoltage PortVoltage
        {
            get => PortControl.PortVoltage;
            set => PortControl.PortVoltage = value;
        }

        /// <summary>
        /// Gets or sets the LED enable state.
        /// </summary>
        /// <remarks>
        /// If true, the headstage LED will turn on during data acquisition. If false, the LED will not turn on.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the headstage LED will turn on during acquisition.")]
        public bool TrackingLed
        {
            get => Serdes.EnableLed;
            set => Serdes.EnableLed = value;
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
            NeuropixelsV2A.SelectProbe = serializer => ConfigureHeadstageNeuropixelsV2eBetaDS90UB9x.SelectProbeA(serializer);
            NeuropixelsV2A.SyncProbes = _ => { };

            NeuropixelsV2B.StreamIndex = 1;
            NeuropixelsV2B.SelectProbe = serializer => ConfigureHeadstageNeuropixelsV2eBetaDS90UB9x.SelectProbeB(serializer);
            NeuropixelsV2B.SyncProbes = serializer => ConfigureHeadstageNeuropixelsV2eBetaDS90UB9x.SyncProbes(serializer);
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

    class ConfigureHeadstageNeuropixelsV2eBetaDS90UB9x : ConfigureDS90UB9x
    {
        readonly Version MinimumRevision = new(1, 0);
        const uint HeadstageId = 7;

        const uint GPO10ProbeEnableMask = 1 << 3;
        const uint Gpo10EnableMuxMask = 1 << 7;
        const uint DefaultGPO10Config = 0b0001_0001; // NP Mux in reset, NPs in reset
        const uint DefaultGPO32Config = 0b1001_1001; // LED off, ProbeA selected
        const uint ProbeSelectMask = 1 << 3;
        const uint Gpo32DisableLedMask = 1 << 7;

        public ConfigureHeadstageNeuropixelsV2eBetaDS90UB9x()
            : base(typeof(DS90UB9x))
        {
        }

        public bool EnableLed { get; set; } = true;

        override private protected void ConfigureSerdes(DeviceContext device)
        {   
            // default to disabled and let individual instances re-enable
            device.WriteRegister(DS90UB9x.ENABLE, 0);

            // configure deserializer trigger mode
            device.WriteRegister(DS90UB9x.TRIGGEROFF, 0);
            device.WriteRegister(DS90UB9x.TRIGGER, (uint)DS90UB9xTriggerMode.Continuous);
            device.WriteRegister(DS90UB9x.SYNCBITS, 0);
            device.WriteRegister(DS90UB9x.DATAGATE, (uint)DS90UB9xDataGate.Disabled);
            device.WriteRegister(DS90UB9x.MARK, (uint)DS90UB9xMarkMode.Disabled);

            // configure two 4-bit magic word-triggered streams, one for each probe
            device.WriteRegister(DS90UB9x.READSZ, 0x0010_0007); // 16 frames/superframe, 8x 12-bit words + magic bits
            device.WriteRegister(DS90UB9x.MAGIC_MASK, 0b1100000000000000_0011111111111111); // Enable inverse, wait for non-inverse, 14-bit magic word
            device.WriteRegister(DS90UB9x.MAGIC, 0b0011_0011_0011_0000); // Super-frame sync word
            device.WriteRegister(DS90UB9x.MAGIC_WAIT, 0);
            device.WriteRegister(DS90UB9x.DATAMODE, 0b10_1101_0101);
            device.WriteRegister(DS90UB9x.DATALINES0, 0x00007654); // NP A
            device.WriteRegister(DS90UB9x.DATALINES1, 0x00000123); // NP B

            DS90UB9x.Initialize933SerDesLink(device, DS90UB9xMode.Raw12BitHighFrequency);

            // configure deserializer I2C aliases
            var deserializer = new I2CRegisterContext(device, DS90UB9x.DES_ADDR);

            uint alias = HeadstageEeprom.I2CAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID1, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias1, alias);

            alias = NeuropixelsV2Beta.ProbeAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID2, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias2, alias);

            alias = NeuropixelsV2Beta.FlexEEPROMAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID3, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias3, alias);

            alias = PolledBno055.I2CAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID4, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias4, alias);

            // initialize GPIO to default state
            var serializer = new I2CRegisterContext(device, DS90UB9x.SER_ADDR);
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, DefaultGPO10Config);
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio32, DefaultGPO32Config);

            // set I2C clock rate to ~400 kHz
            DS90UB9x.Set933I2CRate(device, 400e3);

            // read and validate headstage EEPROM
            ValidateHeadstage(new HeadstageEeprom(device));

            if (EnableLed)
            {
                TurnOnLed(serializer);
            }

            ResetProbes(serializer);
        }

        void ValidateHeadstage(HeadstageEeprom metadata)
        {
            if (metadata.Id != HeadstageId)
            {
                throw new InvalidOperationException(
                    $"Expected Headstage-NeuropixelsV2.0e-Beta but found '{metadata.Name}' (ID: {metadata.Id}).");
            }

            if (metadata.Revision < MinimumRevision)
            {
                throw new InvalidOperationException(
                    $"Headstage version {MinimumRevision} is required but version {metadata.Revision} was detected.");
            }
        }

        override private protected IDisposable ShutdownSerdes(DeviceContext device)
        {
            var serializer = new I2CRegisterContext(device, DS90UB9x.SER_ADDR);
            return Disposable.Create(() =>
            {
                serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, DefaultGPO10Config);
                serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio32, DefaultGPO32Config);
            });
        }

        internal static void SelectProbeA(I2CRegisterContext serializer)
        {
            uint gpo32Config = serializer.ReadByte((uint)DS90UB933SerializerI2CRegister.Gpio32);
            gpo32Config |= ProbeSelectMask;
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio32, (byte)gpo32Config);
            Thread.Sleep(20);
        }

        internal static void SelectProbeB(I2CRegisterContext serializer)
        {
            uint gpo32Config = serializer.ReadByte((uint)DS90UB933SerializerI2CRegister.Gpio32);
            gpo32Config &= ~ProbeSelectMask;
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio32, (byte)gpo32Config);
            Thread.Sleep(20);
        }

        static void TurnOnLed(I2CRegisterContext serializer)
        {
            var gpo32Config = serializer.ReadByte((uint)DS90UB933SerializerI2CRegister.Gpio32);
            gpo32Config = (byte)(gpo32Config & ~Gpo32DisableLedMask);
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio32, gpo32Config);
        }

        static void ResetProbes(I2CRegisterContext serializer)
        {
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, DefaultGPO10Config);
            var gpo10Config = DefaultGPO10Config | GPO10ProbeEnableMask | Gpo10EnableMuxMask;
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, (byte)gpo10Config);
        }

        internal static void SyncProbes(I2CRegisterContext serializer)
        {
            uint gpo10Config = serializer.ReadByte((uint)DS90UB933SerializerI2CRegister.Gpio10);
            gpo10Config &= ~Gpo10EnableMuxMask;
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, (byte)gpo10Config);
            gpo10Config |= Gpo10EnableMuxMask;
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, (byte)gpo10Config);
        }
    }

    class ConfigureHeadstageNeuropixelsV2eBetaPortController : ConfigurePortController
    {
        public ConfigureHeadstageNeuropixelsV2eBetaPortController()
            : base(typeof(PortController))
        {
        }

        protected override bool ConfigurePortVoltage(DeviceContext device, out double voltage)
        {
            const double MinVoltage = 3.3;
            const double MaxVoltage = 5.5;
            const double VoltageOffset = 1.0;
            const double VoltageIncrement = 0.2;

            voltage = MinVoltage;
            for (; voltage <= MaxVoltage; voltage += VoltageIncrement)
            {
                SetVoltage(device, voltage);

                if (CheckLinkState(device))
                {
                    voltage += VoltageOffset;
                    SetVoltage(device, voltage);
                    return CheckLinkState(device);
                }
            }

            return false;
        }

        void SetVoltage(DeviceContext device, double voltage)
        {
            device.WriteRegister(PortController.PORTVOLTAGE, 0);
            Thread.Sleep(200);
            device.WriteRegister(PortController.PORTVOLTAGE, (uint)(10 * voltage));
            Thread.Sleep(200);
        }
    }

    /// <inheritdoc cref="ConfigureHeadstageNeuropixelsV2eBeta"/>
    [Obsolete("This operator is obsolete. Use ConfigureHeadstageNeuropixelsV2eBeta instead. Will be removed in version 1.0.0.")]
    public class ConfigureNeuropixelsV2eBetaHeadstage : ConfigureHeadstageNeuropixelsV2eBeta { }
}
