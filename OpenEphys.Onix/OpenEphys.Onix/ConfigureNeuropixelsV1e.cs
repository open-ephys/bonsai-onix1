using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Threading;
using Bonsai;

namespace OpenEphys.Onix
{
    public class ConfigureNeuropixelsV1e : SingleDeviceFactory
    {
        public ConfigureNeuropixelsV1e()
            : base(typeof(NeuropixelsV1e))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the Neuropixels data stream is enabled.")]
        public bool Enable { get; set; } = true;

        [Category(ConfigurationCategory)]
        [Description("If true, the headstage LED will illuminate during acquisition. Otherwise it will remain off.")]
        public bool EnableLed { get; set; } = true;

        [Category(ConfigurationCategory)]
        [Description("Amplifier gain for spike-band.")]
        public NeuropixelsV1Gain SpikeAmplifierGain { get; set; } = NeuropixelsV1Gain.Gain1000;

        [Category(ConfigurationCategory)]
        [Description("Amplifier gain for LFP-band.")]
        public NeuropixelsV1Gain LfpAmplifierGain { get; set; } = NeuropixelsV1Gain.Gain50;

        [Category(ConfigurationCategory)]
        [Description("Reference selection.")]
        public NeuropixelsV1ReferenceSource Reference { get; set; } = NeuropixelsV1ReferenceSource.External;

        [Category(ConfigurationCategory)]
        [Description("If true, activates a 300 Hz high-pass filter in the spike-band data stream.")]
        public bool SpikeFilter { get; set; } = true;

        [FileNameFilter("Gain calibration files (*_gainCalValues.csv)|*_gainCalValues.csv")]
        [Description("Path to the Neuropixels 1.0 gain calibration file.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string GainCalibrationFile { get; set; }

        [FileNameFilter("ADC calibration files (*_ADCCalibration.csv)|*_ADCCalibration.csv")]
        [Description("Path to the Neuropixels 1.0 ADC calibration file.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string AdcCalibrationFile { get; set; }

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            var ledEnabled = EnableLed;
            return source.ConfigureDevice(context =>
            {
                // configure device via the DS90UB9x deserializer device
                var device = context.GetPassthroughDeviceContext(deviceAddress, DS90UB9x.ID);
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
                var probeControl = new NeuropixelsV1eRegisterContext(device, NeuropixelsV1e.ProbeAddress, probeMetadata.ProbeSerialNumber, SpikeAmplifierGain, LfpAmplifierGain, Reference, SpikeFilter, GainCalibrationFile, AdcCalibrationFile);
                probeControl.InitializeProbe();
                probeControl.WriteConfiguration();
                probeControl.StartAcquisition();

                // turn on LED
                if (ledEnabled)
                {
                    TurnOnLed(serializer, NeuropixelsV1e.DefaultGPO32Config);
                }

                var deviceInfo = new NeuropixelsV1eDeviceInfo(context, DeviceType, deviceAddress, probeControl);
                var disposable = DeviceManager.RegisterDevice(deviceName, deviceInfo);
                var shutdown = Disposable.Create(() =>
                {
                    serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO10, NeuropixelsV1e.DefaultGPO10Config);
                    serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO32, NeuropixelsV1e.DefaultGPO32Config);
                });
                return new CompositeDisposable(
                    shutdown,
                    disposable);
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

            uint alias = NeuropixelsV1e.ProbeAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID1, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias1, alias);

            alias = NeuropixelsV1e.FlexEEPROMAddress << 1;
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
        public const int ProbeAddress = 0x70;
        public const int FlexEEPROMAddress = 0x50;

        public const byte DefaultGPO10Config = 0b0001_0001; // GPIO0 Low, NP in MUX reset
        public const byte DefaultGPO32Config = 0b1001_0001; // LED off, GPIO1 Low
        public const uint Gpo10ResetMask = 1 << 3; // Used to issue mux reset command to probe
        public const uint Gpo32LedMask = 1 << 7; // Used to turn on and off LED

        public const int FramesPerSuperFrame = 13;
        public const int FramesPerRoundRobin = 12;
        public const int AdcCount = 32;
        public const int ChannelCount = 384;
        public const int FrameWords = 40;

        // unmanaged regiseters
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

    public enum NeuropixelsV1ReferenceSource : byte
    {
        External = 0b001,
        Tip = 0b010
    }

    public enum NeuropixelsV1Gain : byte
    {
        Gain50 = 0b000,
        Gain125 = 0b001,
        Gain250 = 0b010,
        Gain500 = 0b011,
        Gain1000 = 0b100,
        Gain1500 = 0b101,
        Gain2000 = 0b110,
        Gain3000 = 0b111
    }
}
