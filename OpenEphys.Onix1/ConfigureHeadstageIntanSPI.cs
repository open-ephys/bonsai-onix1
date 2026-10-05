using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Threading;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures an Intan SPI adapter headstage.
    /// </summary>
    [Description("Configures an Intan SPI adapter headstage")]
    public class ConfigureHeadstageIntanSPI : MultiDeviceFactory
    {
        PortName port;
        readonly ConfigureHeadstageIntanSPIPortController PortControl = new();
        readonly ConfigureHeadstageIntanSPIDS90UB9x Serdes = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureHeadstageIntanSPI"/> class.
        /// </summary>
        public ConfigureHeadstageIntanSPI()
        {
            Port = PortName.PortA;
            PortControl.HubConfiguration = HubConfiguration.Passthrough;
        }

        /// <summary>
        /// Gets or sets the Intan SPI configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Intan SPI device.")]
        public ConfigureIntanSPI IntanSPI { get; set; } = new();

        /// <summary>
        /// Gets or sets the Bno055 9-axis inertial measurement unit configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Bno055 device.")]
        public ConfigurePolledBno055 Bno055 { get; set; } = new();

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
                IntanSPI.DeviceAddress = offset + 0;
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
        /// </remarks>
        [Description("If defined, overrides automated voltage discovery and applies " +
            "the specified voltage to the headstage.")]
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(PortVoltageConverter))]
        public AutoPortVoltage PortVoltage
        {
            get => PortControl.PortVoltage;
            set => PortControl.PortVoltage = value;
        }

        private protected override void PrepareDevices()
        {
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return PortControl;
            yield return Serdes; // must come before dependent devices that follow
            yield return IntanSPI;
            yield return Bno055;
        }
    }

    class ConfigureHeadstageIntanSPIDS90UB9x : ConfigureDS90UB9x
    {
        readonly Version MinimumRevision = new(1, 0);
        const int HeadstageId = 14;
        const byte GP0Reset = 0b0000_1001;
        const byte GP0Normal = 0b0000_0001;

        public ConfigureHeadstageIntanSPIDS90UB9x()
            : base(typeof(DS90UB9x))
        {
        }
        private protected override void ConfigureSerdes(DeviceContext device)
        {
            var serializer = new I2CRegisterContext(device, DS90UB9x.SER_ADDR);
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, GP0Reset);
            device.WriteRegister(DS90UB9x.ENABLE, 0); // default to disabled and let individual devices re-enable if needed

            // configure deserializer trigger mode
            device.WriteRegister(DS90UB9x.TRIGGEROFF, 0);
            device.WriteRegister(DS90UB9x.TRIGGER, (uint)DS90UB9xTriggerMode.VsyncEdgePositive);
            device.WriteRegister(DS90UB9x.SYNCBITS, 0);
            device.WriteRegister(DS90UB9x.DATAGATE, (uint)DS90UB9xDataGate.HsyncPositive);
            device.WriteRegister(DS90UB9x.MARK, (uint)DS90UB9xMarkMode.Disabled);
            device.WriteRegister(DS90UB9x.READSZ, IntanSPI.NumAdcSamplesPerRoundRobbin);
            device.WriteRegister(DS90UB9x.MAGIC_MASK, 0);
            device.WriteRegister(DS90UB9x.MAGIC, 0);
            // Serial mode, 1 stream, 16 bits per word, 2 lines, LSB first
            device.WriteRegister(DS90UB9x.DATAMODE, 0b0000_0000_0000_0000_0000_0101_1111_0001);
            device.WriteRegister(DS90UB9x.DATALINES0, 0xFFFFFFAB);
            device.WriteRegister(DS90UB9x.DATALINES1, 0xFFFFFFFF);

            DS90UB9x.Initialize933SerDesLink(device, DS90UB9xMode.Raw12BitHighFrequency);

            // configure deserializer I2C aliases
            var deserializer = new I2CRegisterContext(device, DS90UB9x.DES_ADDR);
            uint alias = IntanSPI.I2CAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID1, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias1, alias);

            alias = PolledBno055.I2CAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID4, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias4, alias);

            alias = HeadstageEeprom.I2CAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID5, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias5, alias);

            // set I2C clock rate to ~400 kHz
            DS90UB9x.Set933I2CRate(device, 400e3);
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, GP0Normal);

            // read and validate headstage EEPROM
            var metadata = new HeadstageEeprom(device);
            if (metadata.Id != HeadstageId)
            {
                throw new InvalidOperationException($"Expected a Intan SPI dongle but found " +
                    $"'{metadata.Name}' (ID: {metadata.Id}).");
            }
            if (metadata.Revision < MinimumRevision)
            {
                ContextHelper.Validate(ValidationLevel.Permissive, new InvalidOperationException(
                    $"Headstage version {MinimumRevision} is required but version {metadata.Revision} was detected."));
            }
        }

        private protected override IDisposable ShutdownSerdes(DeviceContext device)
        {
            var serializer = new I2CRegisterContext(device, DS90UB9x.SER_ADDR);
            return Disposable.Create(() =>
            {
                serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, GP0Reset);
            });
        }
    }


    class ConfigureHeadstageIntanSPIPortController : ConfigurePortController
    {
        public ConfigureHeadstageIntanSPIPortController()
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
