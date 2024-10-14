using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Threading;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a NeuropixelsV1 device attached to an ONIX NeuropixelsV1e headstage
    /// </summary>
    /// <remarks>
    /// This configuration operator can be linked to a data IO operator, such as <see cref="NeuropixelsV1eData"/>,
    /// using a shared <c>DeviceName</c>.
    /// </remarks>
    [Description("Configures a NeuropixelsV1 device attached to an ONIX NeuropixelsV1e headstage.")]
    [Editor("OpenEphys.Onix1.Design.NeuropixelsV1Editor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    public class ConfigureNeuropixelsV1e : SingleDeviceFactory, IConfigureNeuropixelsV1
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureNeuropixelsV1e"/> class.
        /// </summary>
        public ConfigureNeuropixelsV1e()
            : base(typeof(NeuropixelsV1e))
        {
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="ConfigureNeuropixelsV1e"/> class with public
        ///  properties copied from the specified configuration.
        /// </summary>
        /// <param name="configureNeuropixelsV1e">Existing <see cref="ConfigureNeuropixelsV1e"/> instance.</param>
        public ConfigureNeuropixelsV1e(ConfigureNeuropixelsV1e configureNeuropixelsV1e)
            : base(typeof(NeuropixelsV1e))
        {
            Enable = configureNeuropixelsV1e.Enable;
            EnableLed = configureNeuropixelsV1e.EnableLed;
            GainCalibrationFile = configureNeuropixelsV1e.GainCalibrationFile;
            AdcCalibrationFile = configureNeuropixelsV1e.AdcCalibrationFile;
            ProbeConfiguration = new(configureNeuropixelsV1e.ProbeConfiguration);
            DeviceName = configureNeuropixelsV1e.DeviceName;
            DeviceAddress = configureNeuropixelsV1e.DeviceAddress;
        }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, <see cref="NeuropixelsV1eData"/> will produce data. If set to false, 
        /// <see cref="NeuropixelsV1eData"/> will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the Neuropixels data stream is enabled.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Gets or sets the LED enable state.
        /// </summary>
        /// <remarks>
        /// If true, the headstage LED will turn on during data acquisition. If false, the LED will not turn on.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("If true, the headstage LED will turn on during data acquisition. If false, the LED will not turn on.")]
        public bool EnableLed { get; set; } = true;

        /// <summary>
        /// Gets or sets the path to the gain calibration file.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each probe is linked to a gain calibration file that contains gain adjustments determined by IMEC during
        /// factory testing. Electrode voltages are scaled using these values to ensure they can be accurately compared
        /// across probes. Therefore, using the correct gain calibration file is mandatory to create standardized recordings.
        /// </para>
        /// <para>
        /// Calibration files are probe-specific and not interchangeable across probes. Calibration files must contain the 
        /// serial number of the corresponding probe on their first line of text. If you have lost track of a calibration 
        /// file for your probe, email IMEC at neuropixels.info@imec.be with the probe serial number to retrieve a new copy.
        /// </para>
        /// </remarks>
        [FileNameFilter("Gain calibration files (*_gainCalValues.csv)|*_gainCalValues.csv")]
        [Description("Path to the Neuropixels 1.0 gain calibration file.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Category(ConfigurationCategory)]
        public string GainCalibrationFile { get; set; }

        /// <summary>
        /// Gets or sets the path to the ADC calibration file.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each probe must be provided with an ADC calibration file that contains probe-specific hardware settings that is
        /// created by IMEC during factory calibration. These files are used to set internal bias currents, correct for ADC
        /// nonlinearities, correct ADC-zero crossing non-monotonicities, etc. Using the correct calibration file is mandatory
        /// for the probe to operate correctly. 
        /// </para>
        /// <para>
        /// Calibration files are probe-specific and not interchangeable across probes. Calibration files must contain the 
        /// serial number of the corresponding probe on their first line of text. If you have lost track of a calibration 
        /// file for your probe, email IMEC at neuropixels.info@imec.be with the probe serial number to retrieve a new copy.
        /// </para>
        /// </remarks>
        [FileNameFilter("ADC calibration files (*_ADCCalibration.csv)|*_ADCCalibration.csv")]
        [Description("Path to the Neuropixels 1.0 ADC calibration file.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Category(ConfigurationCategory)]
        public string AdcCalibrationFile { get; set; }

        /// <summary>
        /// Gets or sets the NeuropixelsV1 probe configuration.
        /// </summary>
        [Category(ConfigurationCategory)]
        [Description("Neuropixels 1.0e probe configuration")]
        public NeuropixelsV1ProbeConfiguration ProbeConfiguration { get; set; } = new();

        /// <summary>
        /// Configures a NeuropixelsV1e device.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> node
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> that holds all configuration actions.</param>
        /// <returns>
        /// The original sequence with the side effect of an additional configuration action to configure
        /// a NeuropixelsV1e device.
        /// </returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            var ledEnabled = EnableLed;
            return source.ConfigureDevice(context =>
            {
                // configure device via the DS90UB9x deserializer device
                var device = context.GetPassthroughDeviceContext(deviceAddress, typeof(DS90UB9x));
                device.WriteRegister(DS90UB9x.ENABLE, enable ? 1u : 0);

                // configure deserializer aliases and serializer power supply
                ConfigureDeserializer(device);
                var serializer = new I2CRegisterContext(device, DS90UB9x.SER_ADDR);

                // set I2C clock rate to ~400 kHz
                serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.SCLHIGH, 20);
                serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.SCLLOW, 20);

                // read probe metadata
                var probeMetadata = new NeuropixelsV1eMetadata(serializer);

                // issue full mux reset to the probe
                var gpo10Config = NeuropixelsV1e.DefaultGPO10Config;
                ResetProbe(serializer, gpo10Config);

                // program shift registers
                var probeControl = new NeuropixelsV1eRegisterContext(device, NeuropixelsV1.ProbeI2CAddress,
                                        probeMetadata.ProbeSerialNumber, ProbeConfiguration, GainCalibrationFile, AdcCalibrationFile);
                probeControl.InitializeProbe();
                probeControl.WriteConfiguration();
                probeControl.StartAcquisition();

                // turn on LED
                if (ledEnabled)
                {
                    TurnOnLed(serializer, NeuropixelsV1e.DefaultGPO32Config);
                }

                var deviceInfo = new NeuropixelsV1eDeviceInfo(context, DeviceType, deviceAddress, probeControl);
                var shutdown = Disposable.Create(() =>
                {
                    serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO10, NeuropixelsV1e.DefaultGPO10Config);
                    serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO32, NeuropixelsV1e.DefaultGPO32Config);
                });
                return new CompositeDisposable(
                    DeviceManager.RegisterDevice(deviceName, deviceInfo),
                    shutdown);
            });
        }

        static void ConfigureDeserializer(DeviceContext device)
        {
            // configure deserializer trigger mode
            device.WriteRegister(DS90UB9x.TRIGGEROFF, 0);
            device.WriteRegister(DS90UB9x.TRIGGER, (uint)DS90UB9xTriggerMode.Continuous);
            device.WriteRegister(DS90UB9x.SYNCBITS, 0);
            device.WriteRegister(DS90UB9x.DATAGATE, 0b0000_0001_0001_0011_0000_0000_0000_0001);
            device.WriteRegister(DS90UB9x.MARK, (uint)DS90UB9xMarkMode.Disabled);

            // configure one magic word-triggered stream for the PSB bus
            device.WriteRegister(DS90UB9x.READSZ, 851973); // 13 frames/superframe,  7x 140-bit words on each serial line per frame
            device.WriteRegister(DS90UB9x.MAGIC_MASK, 0b11000000000000000000001111111111); // Enable inverse, wait for non-inverse, 10-bit magic word
            device.WriteRegister(DS90UB9x.MAGIC, 816); // Super-frame sync word
            device.WriteRegister(DS90UB9x.MAGIC_WAIT, 0);
            device.WriteRegister(DS90UB9x.DATAMODE, 913);
            device.WriteRegister(DS90UB9x.DATALINES0, 0x3245106B); // Sync, psb[0], psb[1], psb[2], psb[3], psb[4], psb[5], psb[6],
            device.WriteRegister(DS90UB9x.DATALINES1, 0xFFFFFFFF);

            // configure deserializer I2C aliases
            var deserializer = new I2CRegisterContext(device, DS90UB9x.DES_ADDR);
            uint coaxMode = 0x4 + (uint)DS90UB9xMode.Raw12BitHighFrequency; // 0x4 maintains coax mode
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.PortMode, coaxMode);

            uint alias = NeuropixelsV1.ProbeI2CAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID1, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias1, alias);

            alias = NeuropixelsV1.FlexEepromI2CAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID2, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias2, alias);
        }

        static void ResetProbe(I2CRegisterContext serializer, uint gpo10Config)
        {
            gpo10Config &= ~NeuropixelsV1e.Gpo10ResetMask;
            serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO10, gpo10Config);
            Thread.Sleep(1);
            gpo10Config |= NeuropixelsV1e.Gpo10ResetMask;
            serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO10, gpo10Config);
        }

        static uint TurnOnLed(I2CRegisterContext serializer, uint gpo23Config)
        {
            gpo23Config &= ~NeuropixelsV1e.Gpo32LedMask;
            serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO32, gpo23Config);

            return gpo23Config;
        }
    }

    static class NeuropixelsV1e
    {
        public const byte DefaultGPO10Config = 0b0001_0001; // GPIO0 Low, NP in MUX reset
        public const byte DefaultGPO32Config = 0b1001_0001; // LED off, GPIO1 Low
        public const uint Gpo10ResetMask = 1 << 3; // Used to issue mux reset command to probe
        public const uint Gpo32LedMask = 1 << 7; // Used to turn on and off LED

        // unmanaged registers
        public const uint OP_MODE = 0X00;
        public const uint REC_MOD = 0X01;
        public const uint CAL_MOD = 0X02;
        public const uint TEST_CONFIG1 = 0x03;
        public const uint TEST_CONFIG2 = 0x04;
        public const uint TEST_CONFIG3 = 0x05;
        public const uint TEST_CONFIG4 = 0x06;
        public const uint TEST_CONFIG5 = 0x07;
        public const uint STATUS = 0X08;
        public const uint SYNC = 0X09;
        public const uint SR_CHAIN1 = 0X0E; // Shank configuration
        public const uint SR_CHAIN3 = 0X0C; // Odd channels
        public const uint SR_CHAIN2 = 0X0D; // Even channels
        public const uint SR_LENGTH2 = 0X0F;
        public const uint SR_LENGTH1 = 0X10;
        public const uint SOFT_RESET = 0X11;

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(NeuropixelsV1e))
            {
            }
        }
    }

    [Flags]
    enum NeuropixelsV1CalibrationRegisterValues : uint
    {
        CAL_OFF = 0,
        OSC_ACTIVE = 1 << 4, // 0 = external osc inactive, 1 = activate the external calibration oscillator
        ADC_CAL = 1 << 5, // Enable ADC calibration
        CH_CAL = 1 << 6, // Enable channel gain calibration
        PIX_CAL = 1 << 7, // Enable pixel + channel gain calibration

        // Useful combinations
        OSC_ACTIVE_AND_ADC_CAL = OSC_ACTIVE | ADC_CAL,
        OSC_ACTIVE_AND_CH_CAL = OSC_ACTIVE | CH_CAL,
        OSC_ACTIVE_AND_PIX_CAL = OSC_ACTIVE | PIX_CAL,

    };

    [Flags]
    enum NeuropixelsV1RecordRegisterValues : uint
    {
        RESET_ALL = 1 << 5, // 1 = Set analog SR chains to default values
        DIG_ENABLE = 1 << 6, // 0 = Reset the MUX, ADC, and PSB counter, 1 = Disable reset
        CH_ENABLE = 1 << 7, // 0 = Reset channel pseudo-registers, 1 = Disable reset

        // Useful combinations
        SR_RESET = RESET_ALL | CH_ENABLE | DIG_ENABLE,
        DIG_CH_RESET = 0,  // Yes, this is actually correct
        ACTIVE = DIG_ENABLE | CH_ENABLE,
    };

    [Flags]
    enum NeuropixelsV1OperationRegisterValues : uint
    {
        TEST = 1 << 3, // Enable Test mode
        DIG_TEST = 1 << 4, // Enable Digital Test mode
        CALIBRATE = 1 << 5, // Enable calibration mode
        RECORD = 1 << 6, // Enable recording mode
        POWER_DOWN = 1 << 7, // Enable power down mode

        // Useful combinations
        RECORD_AND_DIG_TEST = RECORD | DIG_TEST,
        RECORD_AND_CALIBRATE = RECORD | CALIBRATE,
    };

    /// <summary>
    /// Specifies the reference source for all electrodes.
    /// </summary>
    public enum NeuropixelsV1ReferenceSource : byte
    {
        /// <summary>
        /// Specifies that the reference should be External.
        /// </summary>
        External = 0b001,
        /// <summary>
        /// Specifies that the reference should be the Tip.
        /// </summary>
        Tip = 0b010
    }

    /// <summary>
    /// Specifies the gain for all electrodes
    /// </summary>
    public enum NeuropixelsV1Gain : byte
    {
        /// <summary>
        /// Specifies that the gain should be 50x.
        /// </summary>
        Gain50 = 0b000,
        /// <summary>
        /// Specifies that the gain should be 125x.
        /// </summary>
        Gain125 = 0b001,
        /// <summary>
        /// Specifies that the gain should be 250x.
        /// </summary>
        Gain250 = 0b010,
        /// <summary>
        /// Specifies that the gain should be 500x.
        /// </summary>
        Gain500 = 0b011,
        /// <summary>
        /// Specifies that the gain should be 1000x.
        /// </summary>
        Gain1000 = 0b100,
        /// <summary>
        /// Specifies that the gain should be 1500x.
        /// </summary>
        Gain1500 = 0b101,
        /// <summary>
        /// Specifies that the gain should be 2000x.
        /// </summary>
        Gain2000 = 0b110,
        /// <summary>
        /// Specifies that the gain should be 3000x.
        /// </summary>
        Gain3000 = 0b111
    }
}
