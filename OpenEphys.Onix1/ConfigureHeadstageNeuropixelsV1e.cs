using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Threading;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a NeuropixelsV1e headstage on the specified port.
    /// </summary>
    /// <remarks>
    /// The NeuropixelsV1e Headstage is a 0.68g serialized, multifunction headstage designed to
    /// function with IMEC Neuropixels V1 probes. It provides the following features:
    /// <list type="bullet">
    /// <item><description>Support for a single IMEC Neuropixels 1.0 probe that features:
    /// <list type="bullet">
    /// <item><description>A single 1 cm long shank probe with a 70 x 24 µm shank cross-section.</description></item>
    /// <item><description>960-electrode low-impedance TiN electrodes.</description></item>
    /// <item><description>384 parallel, dual-band (AP, LFP), low-noise recording channels.</description></item>
    /// </list>
    /// </description></item>
    /// <item><description>A BNO055 9-axis IMU for real-time, 3D orientation tracking.</description></item>
    /// </list>
    /// </remarks>
    [Editor("OpenEphys.Onix1.Design.NeuropixelsV1eHeadstageEditor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    [Description("Configures a NeuropixelsV1e headstage.")]
    public class ConfigureHeadstageNeuropixelsV1e : MultiDeviceFactory
    {
        PortName port;
        readonly ConfigureNeuropixelsV1ePortController PortControl = new();
        readonly ConfigureHeadstageNeuropixelsV1eDS90UB9x Serdes = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureHeadstageNeuropixelsV1e"/> class.
        /// </summary>
        public ConfigureHeadstageNeuropixelsV1e()
        {
            Port = PortName.PortA;
            PortControl.HubConfiguration = HubConfiguration.Passthrough;
        }

        /// <summary>
        /// Gets or sets the NeuropixelsV1 configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Neuropixels V1.")]
        public ConfigureNeuropixelsV1PsbDecoder NeuropixelsV1 { get; set; } = new();

        /// <summary>
        /// Gets or sets the Bno055 9-axis inertial measurement unit configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Bno055 device.")]
        public ConfigurePolledBno055 Bno055 { get; set; } =
            new ConfigurePolledBno055 { AxisMap = Bno055AxisMap.YZX, AxisSign = Bno055AxisSign.MirrorX | Bno055AxisSign.MirrorZ };

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
                NeuropixelsV1.DeviceAddress = offset + 0;
                Bno055.DeviceAddress = offset + 1;
                Serdes.DeviceAddress = offset + 2;
            }
        }

        /// <summary>
        /// Gets or sets the port voltage.
        /// </summary>
        /// <remarks>
        /// If a port voltage is defined this will override the automated voltage discovery and applies the
        /// specified voltage to the headstage. To enable automated voltage discovery, leave this field empty.
        /// Warning: This device requires 3.8V to 5.0V, measured at the headstage, for proper operation.
        /// Voltages higher than 5.0V can damage the headstage.
        /// </remarks>
        [Description("If defined, overrides automated voltage discovery and applies " +
            "the specified voltage to the headstage. Warning: this device requires 3.8V to 5.0V " +
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

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return PortControl;
            yield return Serdes; // must come before dependent devices that follow
            yield return NeuropixelsV1;
            yield return Bno055;
        }

        class ConfigureNeuropixelsV1ePortController : ConfigurePortController
        {
            public ConfigureNeuropixelsV1ePortController()
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
    }

    class ConfigureHeadstageNeuropixelsV1eDS90UB9x : ConfigureDS90UB9x
    {
        readonly Version MinimumRevision = new(1, 0);
        const uint HeadstageId = 6;

        public const byte DefaultGPO10Config = 0b0001_0001; // GPIO0 Low, NP in MUX reset
        public const byte DefaultGPO32Config = 0b1001_0001; // LED off, GPIO1 Low
        public const byte Gpo10EnableMuxMask = 1 << 3; // Used to issue mux reset command to probe
        public const byte Gpo32DisableLedMask = 1 << 7; // Used to turn on and off LED

        public ConfigureHeadstageNeuropixelsV1eDS90UB9x()
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
            device.WriteRegister(DS90UB9x.DATAGATE, 0b0000_0001_0001_0011_0000_0000_0000_0001);
            device.WriteRegister(DS90UB9x.MARK, (uint)DS90UB9xMarkMode.Disabled);

            // configure one magic word-triggered stream for the PSB bus
            device.WriteRegister(DS90UB9x.READSZ, 851973); // 13 frames/superframe, 7x 140-bit words on each serial line per frame
            device.WriteRegister(DS90UB9x.MAGIC_MASK, 0b11000000000000000000001111111111); // Enable inverse, wait for non-inverse, 10-bit magic word
            device.WriteRegister(DS90UB9x.MAGIC, 816); // Super-frame sync word
            device.WriteRegister(DS90UB9x.MAGIC_WAIT, 0);
            device.WriteRegister(DS90UB9x.DATAMODE, 913);
            device.WriteRegister(DS90UB9x.DATALINES0, 0x3245106B); // Sync, psb[0], psb[1], psb[2], psb[3], psb[4], psb[5], psb[6]
            device.WriteRegister(DS90UB9x.DATALINES1, 0xFFFFFFFF);

            DS90UB9x.Initialize933SerDesLink(device, DS90UB9xMode.Raw12BitHighFrequency);

            // configure deserializer I2C aliases
            var deserializer = new I2CRegisterContext(device, DS90UB9x.DES_ADDR);

            uint alias = HeadstageEeprom.I2CAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID1, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias1, alias);

            alias = NeuropixelsV1.ProbeI2CAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID2, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias2, alias);

            alias = NeuropixelsV1.FlexEepromI2CAddress << 1;
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

            ResetProbe(serializer);
        }

        void ValidateHeadstage(HeadstageEeprom metadata)
        {
            if (metadata.Id != HeadstageId)
            {
                throw new InvalidOperationException(
                    $"Expected a Headstage-NeuropixelsV1.0e but found '{metadata.Name}' (ID: {metadata.Id}).");
            }

            if (metadata.Revision < MinimumRevision)
            {
                throw new InvalidOperationException(
                    $"Headstage version {MinimumRevision} is required but version {metadata.Revision} was detected.");
            }
        }
        static void TurnOnLed(I2CRegisterContext serializer)
        {
            var gpo32Config = serializer.ReadByte((uint)DS90UB933SerializerI2CRegister.Gpio32);
            gpo32Config = (byte)(gpo32Config & ~Gpo32DisableLedMask);
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio32, gpo32Config);
        }

        static void ResetProbe(I2CRegisterContext serializer)
        {
            var gpo10Config = serializer.ReadByte((uint)DS90UB933SerializerI2CRegister.Gpio10);
            gpo10Config &= (byte)(gpo10Config & ~Gpo10EnableMuxMask);
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, gpo10Config);
            gpo10Config |= Gpo10EnableMuxMask;
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, gpo10Config);
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
    }

    /// <inheritdoc cref="ConfigureHeadstageNeuropixelsV1e"/>
    [Obsolete("This operator is obsolete. Use ConfigureHeadstageNeuropixelsV1e instead. Will be removed in version 1.0.0.")]
    public class ConfigureNeuropixelsV1eHeadstage : ConfigureHeadstageNeuropixelsV1e { }
}
