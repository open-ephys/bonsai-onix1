using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Threading;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a hybrid NeuropixelsV2/Rhd2000 headstage.
    /// </summary>
    /// <remarks>
    /// The NeuropixelsV2/Rhd2000 Hybrid Headstage is a 1.0g serialized, multifunction headstage that combines
    /// an Intan RHD2000 bioamplifier chip with an IMEC Neuropixels V2 probe. It provides the
    /// following features:
    /// <list type="bullet">
    /// <item><description>Support for an IMEC Neuropixels 2.0 probe, which features:</description></item>
    /// <list type="bullet">
    /// <item><description>Either 1x or 4x silicon shanks with a 70 x 24 µm
    /// cross-section.</description></item>
    /// <item><description>1280 electrodes low-impedance TiN electrodes per shank.</description></item>
    /// <item><description>384 parallel, full-band (AP, LFP), low-noise recording
    /// channels.</description></item>
    /// </list>
    /// <item><description>An Intan Rhd2000 bioamplifer chip for acquiring from 32 unipolar or 16 differential
    /// electrodes for neural or muscular recordings.</description></item>
    /// <item><description>A thermistor amplifier circuit with programmable gain for monitoring breathing
    /// rates acquired through the Aux 1 input of the Rhd2000.</description></item>
    /// <item><description>A Bno055 9-axis IMU for real-time, 3D orientation tracking.</description></item>
    /// </list>
    /// </remarks>
    [Editor("OpenEphys.Onix1.Design.NeuropixelsV2Rhd2000eHeadstageEditor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    [Description("Configures a hybrid NeuropixelsV2/Rhd2000 headstage.")]
    public class ConfigureHeadstageNeuropixelsV2Rhd2000e : MultiDeviceFactory
    {
        PortName port;
        readonly ConfigureHeadstageNeuropixelsV2Rhd2000ePortController PortControl = new();
        readonly ConfigureHeadstageNeuropixelsV2Rhd2000eDS90UB9x Serdes = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureHeadstageNeuropixelsV2Rhd2000e"/> class.
        /// </summary>
        public ConfigureHeadstageNeuropixelsV2Rhd2000e()
        {
            Port = PortName.PortA;
            PortControl.HubConfiguration = HubConfiguration.Passthrough;
        }

        /// <summary>
        /// Gets or sets the Neuropixels V2 configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Neuropixels V2.")]
        public ConfigureNeuropixelsV2PsbDecoder NeuropixelsV2 { get; set; } = new();

        /// <summary>
        /// Gets or sets the Rhd2000 configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Rhd2000 device.")]
        public ConfigureRhd2000PsbDecoderWithMax41400 Rhd2000 { get; set; } = new();

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
                NeuropixelsV2.DeviceAddress = offset + 0;
                Rhd2000.DeviceAddress = offset + 1;
                Bno055.DeviceAddress = offset + 2;
                Serdes.DeviceAddress = offset + 3;
            }
        }

        /// <summary>
        /// Gets or sets the port voltage.
        /// </summary>
        /// <remarks>
        /// If a port voltage is defined this will override the automated voltage discovery and applies the
        /// specified voltage to the headstage. To enable automated voltage discovery, leave this field empty.
        /// Warning: This device requires 3.5V to 5.5V for proper operation. Voltages higher than 6V can
        /// damage the headstage.
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

        private protected override void PrepareDevices()
        {
            NeuropixelsV2.StreamIndex = 0;
            NeuropixelsV2.SelectProbe = serializer => ConfigureHeadstageNeuropixelsV2Rhd2000eDS90UB9x.SelectProbe(serializer);
            NeuropixelsV2.DeselectProbe = serializer => ConfigureHeadstageNeuropixelsV2Rhd2000eDS90UB9x.DeselectProbe(serializer);

            Rhd2000.StreamIndex = 1;
            Rhd2000.EnableController = serializer => ConfigureHeadstageNeuropixelsV2Rhd2000eDS90UB9x.EnableRhd2000Controller(serializer);
            Rhd2000.GetChipId = () => Serdes.Rhd2000Chip;
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return PortControl;
            yield return Serdes; // must come before dependent devices that follow
            yield return NeuropixelsV2;
            yield return Rhd2000;
            yield return Bno055;
        }
    }

    class ConfigureHeadstageNeuropixelsV2Rhd2000eDS90UB9x : ConfigureDS90UB9x
    {
        readonly Version MinimumRevision = new(1, 0);
        const int HeadstageIdRhd2216Variant = 11;
        const int HeadstageIdRhd2132Variant = 12;

        const byte Gpo10NeuropixelVddaEnableMask = 1 << 3; // Used to turn on Probe VDDA supply
        const byte Gpo10NeuropixelResetMask = 1 << 7; // Used to issue full reset commands to probe
        const byte Gpo32NeuropixelI2cSelectionMask = 1 << 3; // Used to route I2C to probe
        const byte Gpo32Rhd2000VddaEnableMask = Gpo32NeuropixelI2cSelectionMask; // Used to enable Intan VDDA
        const byte Gpo32Rhd2000ControllerEnableMask = 1 << 7;

        const byte DefaultGPO10Config = 0b0001_0001; // NP in reset, NP VDDA not enabled
        const byte DefaultGPO32Config = 0b1001_0001; // FPGA in reset, Rhd2000 VDDA/probe deselect not enabled

        public ConfigureHeadstageNeuropixelsV2Rhd2000eDS90UB9x()
            : base(typeof(DS90UB9x))
        {
        }

        override private protected void ConfigureSerdes(DeviceContext device)
        {
            // configure device via the DS90UB9x deserializer device
            device.WriteRegister(DS90UB9x.ENABLE, 0); // default to disabled and let individual devices re-enable if needed

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
            device.WriteRegister(DS90UB9x.DATALINES0, 0xFFFFFAB9); // NP 2.0 probe
            device.WriteRegister(DS90UB9x.DATALINES1, 0xFFFFF768); // Rhd2000 embedded in NP2.0 PSB bus

            DS90UB9x.Initialize933SerDesLink(device, DS90UB9xMode.Raw12BitHighFrequency);

            // configure deserializer I2C aliases
            var deserializer = new I2CRegisterContext(device, DS90UB9x.DES_ADDR);

            uint alias = NeuropixelsV2.ProbeAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID1, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias1, alias);

            alias = NeuropixelsV2.FlexEEPROMAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID2, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias2, alias);

            alias = Rhd2000PsbDecoder.I2CAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID3, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias3, alias);

            alias = PolledBno055.I2CAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID4, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias4, alias);

            alias = HeadstageEeprom.I2CAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID5, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias5, alias);

            // set I2C clock rate to ~400 kHz
            DS90UB9x.Set933I2CRate(device, 400e3);

            // read and validate headstage EEPROM
            var metadata = new HeadstageEeprom(device);
            Rhd2000Chip = ValidateHeadstage(metadata);

            // initialize headstage hardware
            var serializer = new I2CRegisterContext(device, DS90UB9x.SER_ADDR);
            IntializeGpio(serializer);
            EnableProbeSupply(serializer);
            ResetProbe(serializer);
        }

        Rhd2000ChipId ValidateHeadstage(HeadstageEeprom metadata)
        {
            var chip = metadata.Id switch
            {
                HeadstageIdRhd2216Variant => Rhd2000ChipId.Rhd2216,
                HeadstageIdRhd2132Variant => Rhd2000ChipId.Rhd2132,
                _ => throw new InvalidOperationException($"Expected a Headstage-Neuropixels2.0/Rhd2000e but found " +
                    $"'{metadata.Name}' (ID: {metadata.Id}).")
            };

            if (metadata.Revision < MinimumRevision)
            {
                throw new InvalidOperationException($"Headstage version {MinimumRevision} is required " +
                    $"but version {metadata.Revision} was detected.");
            }

            return chip;
        }

        override private protected IDisposable ShutdownSerdes(DeviceContext device)
        {
            var serializer = new I2CRegisterContext(device, DS90UB9x.SER_ADDR);
            return Disposable.Create(() =>
            {
                IntializeGpio(serializer);
            });
        }

        internal Rhd2000ChipId Rhd2000Chip { get; private set; } = Rhd2000ChipId.Rhd2132;

        internal static void SelectProbe(I2CRegisterContext serializer)
        {
            var gpo32Config = serializer.ReadByte((uint)DS90UB933SerializerI2CRegister.Gpio32);
            gpo32Config = (byte)(gpo32Config & ~Gpo32Rhd2000VddaEnableMask);
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio32, gpo32Config);
            Thread.Sleep(10);
        }

        internal static void DeselectProbe(I2CRegisterContext serializer)
        {
            var gpo32Config = serializer.ReadByte((uint)DS90UB933SerializerI2CRegister.Gpio32);
            gpo32Config |= Gpo32NeuropixelI2cSelectionMask;
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio32, gpo32Config);
            Thread.Sleep(10);
        }

        internal static void EnableRhd2000Controller(I2CRegisterContext serializer)
        {
            var gpo32Config = serializer.ReadByte((uint)DS90UB933SerializerI2CRegister.Gpio32);
            gpo32Config |= Gpo32Rhd2000VddaEnableMask;
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio32, gpo32Config);
            Thread.Sleep(1);

            gpo32Config = serializer.ReadByte((uint)DS90UB933SerializerI2CRegister.Gpio32);
            gpo32Config = (byte)(gpo32Config & ~Gpo32Rhd2000ControllerEnableMask);
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio32, gpo32Config);
            Thread.Sleep(10);
        }

        static void EnableProbeSupply(I2CRegisterContext serializer)
        {
            var gpo10Config = serializer.ReadByte((uint)DS90UB933SerializerI2CRegister.Gpio10);
            gpo10Config |= Gpo10NeuropixelVddaEnableMask;
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, gpo10Config);
            Thread.Sleep(10);
        }

        static void ResetProbe(I2CRegisterContext serializer)
        {
            var gpo10Config = serializer.ReadByte((uint)DS90UB933SerializerI2CRegister.Gpio10);
            gpo10Config = (byte)(gpo10Config & ~Gpo10NeuropixelResetMask);
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, gpo10Config);
            gpo10Config |= Gpo10NeuropixelResetMask;
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, gpo10Config);
        }

        static void IntializeGpio(I2CRegisterContext serializer)
        {
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, DefaultGPO10Config);
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio32, DefaultGPO32Config);
        }
    }

    class ConfigureHeadstageNeuropixelsV2Rhd2000ePortController : ConfigurePortController
    {
        public ConfigureHeadstageNeuropixelsV2Rhd2000ePortController()
            : base(typeof(PortController))
        {
        }
        protected override bool ConfigurePortVoltage(DeviceContext device, out double voltage)
        {
            const double MinVoltage = 4.0;
            const double MaxVoltage = 6.0;
            const double VoltageOffset = 0.8;
            const double VoltageIncrement = 0.1;

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
}
