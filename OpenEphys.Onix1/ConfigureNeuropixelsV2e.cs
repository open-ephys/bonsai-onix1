using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A class that configures a NeuropixelsV2e device.
    /// </summary>
    [Description("Configures a NeuropixelsV2e device.")]
    public class ConfigureNeuropixelsV2e : SingleDeviceFactory
    {
        /// <summary>
        /// Initialize a new instance of a <see cref="ConfigureNeuropixelsV2e"/> class.
        /// </summary>
        public ConfigureNeuropixelsV2e()
            : base(typeof(NeuropixelsV2e))
        {
        }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, <see cref="NeuropixelsV2eData"/> will produce data. If set to false, 
        /// <see cref="NeuropixelsV2eData"/> will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the NeuropixelsV2 device is enabled.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Gets or sets the electrode configuration for Probe A.
        /// </summary>
        [Category(ConfigurationCategory)]
        [Description("Probe A electrode configuration.")]
        public NeuropixelsV2QuadShankProbeConfiguration ProbeConfigurationA { get; set; } = new();

        /// <summary>
        /// Gets or sets the path to the gain calibration file for Probe A.
        /// </summary>
        /// <remarks>
        /// Each probe must be provided with a gain calibration file that contains calibration data
        /// specific to each probe. This file is mandatory for accurate recordings.
        /// </remarks>
        [FileNameFilter("Gain calibration files (*_gainCalValues.csv)|*_gainCalValues.csv")]
        [Description("Path to the gain calibration file for probe A.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string GainCalibrationFileA { get; set; }

        /// <summary>
        /// Gets or sets the electrode configuration for Probe B.
        /// </summary>
        [Category(ConfigurationCategory)]
        [Description("Probe B electrode configuration.")]
        public NeuropixelsV2QuadShankProbeConfiguration ProbeConfigurationB { get; set; } = new();

        /// <summary>
        /// Gets or sets the path to the gain calibration file for Probe B.
        /// </summary>
        /// <remarks>
        /// Each probe must be provided with a gain calibration file that contains calibration data
        /// specific to each probe. This file is mandatory for accurate recordings.
        /// </remarks>
        [FileNameFilter("Gain calibration files (*_gainCalValues.csv)|*_gainCalValues.csv")]
        [Description("Path to the gain calibration file for probe B.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string GainCalibrationFileB { get; set; }

        /// <summary>
        /// Configures a NeuropixelsV2e device.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> node
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> that holds all configuration actions.</param>
        /// <returns>
        /// The original sequence with the side effect of an additional configuration action to configure
        /// a NeuropixelsV2e device.
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
                device.WriteRegister(DS90UB9x.ENABLE, enable ? 1u : 0);

                // configure deserializer aliases and serializer power supply
                ConfigureDeserializer(device);
                var serializer = new I2CRegisterContext(device, DS90UB9x.SER_ADDR);
                var gpo10Config = EnableProbeSupply(serializer);

                // set I2C clock rate to ~400 kHz
                serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.SCLHIGH, 20);
                serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.SCLLOW, 20);

                // read probe metadata
                var probeAMetadata = ReadProbeMetadata(serializer, NeuropixelsV2e.ProbeASelected);
                var probeBMetadata = ReadProbeMetadata(serializer, NeuropixelsV2e.ProbeBSelected);

                if (probeAMetadata.ProbeSerialNumber == null && probeBMetadata.ProbeSerialNumber == null)
                {
                    throw new InvalidOperationException("No probes were detected. Ensure that the " +
                        "flex connection is properly seated.");
                }

                // issue full reset to both probes
                ResetProbes(serializer, gpo10Config);

                // configure probe streaming
                double? gainCorrectionA = null;
                double? gainCorrectionB = null;
                var probeControl = new NeuropixelsV2eRegisterContext(device, NeuropixelsV2e.ProbeAddress);

                // configure probe A streaming
                if (probeAMetadata.ProbeSerialNumber != null)
                {
                    gainCorrectionA = NeuropixelsV2.ReadGainCorrection(
                        GainCalibrationFileA, (ulong)probeAMetadata.ProbeSerialNumber, NeuropixelsV2Probe.ProbeA);
                    SelectProbe(serializer, NeuropixelsV2e.ProbeASelected);
                    probeControl.WriteConfiguration(ProbeConfigurationA);
                    ConfigureProbeStreaming(probeControl);
                }

                // configure probe B streaming
                if (probeBMetadata.ProbeSerialNumber != null)
                {
                    gainCorrectionB = NeuropixelsV2.ReadGainCorrection(
                        GainCalibrationFileB, (ulong)probeBMetadata.ProbeSerialNumber, NeuropixelsV2Probe.ProbeB);
                    SelectProbe(serializer, NeuropixelsV2e.ProbeBSelected);
                    probeControl.WriteConfiguration(ProbeConfigurationB);
                    ConfigureProbeStreaming(probeControl);
                }

                // disconnect i2c bus from both probes to prevent digital interference during acquisition
                SelectProbe(serializer, NeuropixelsV2e.NoProbeSelected);

                var deviceInfo = new NeuropixelsV2eDeviceInfo(context, DeviceType, deviceAddress, gainCorrectionA, gainCorrectionB);
                var shutdown = Disposable.Create(() =>
                {
                    serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO10, NeuropixelsV2e.DefaultGPO10Config);
                    SelectProbe(serializer, NeuropixelsV2e.NoProbeSelected);
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

            // configure deserializer I2C aliases
            var deserializer = new I2CRegisterContext(device, DS90UB9x.DES_ADDR);
            uint coaxMode = 0x4 + (uint)DS90UB9xMode.Raw12BitHighFrequency; // 0x4 maintains coax mode
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.PortMode, coaxMode);

            uint alias = NeuropixelsV2e.ProbeAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID1, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias1, alias);

            alias = NeuropixelsV2e.FlexEEPROMAddress << 1;
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveID2, alias);
            deserializer.WriteByte((uint)DS90UB9xDeserializerI2CRegister.SlaveAlias2, alias);
        }

        static uint EnableProbeSupply(I2CRegisterContext serializer)
        {
            var gpo10Config = NeuropixelsV2e.DefaultGPO10Config | NeuropixelsV2e.GPO10SupplyMask;
            SelectProbe(serializer, NeuropixelsV2e.NoProbeSelected);

            // turn on analog supply and wait for boot
            serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO10, gpo10Config);
            System.Threading.Thread.Sleep(20);
            return gpo10Config;
        }

        static NeuropixelsV2eMetadata ReadProbeMetadata(I2CRegisterContext serializer, byte probeSelect)
        {
            SelectProbe(serializer, probeSelect);
            return new NeuropixelsV2eMetadata(serializer);
        }
        static void SelectProbe(I2CRegisterContext serializer, byte probeSelect)
        {
            serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO32, probeSelect);
            System.Threading.Thread.Sleep(20);
        }

        static void ResetProbes(I2CRegisterContext serializer, uint gpo10Config)
        {
            gpo10Config &= ~NeuropixelsV2e.GPO10ResetMask;
            serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO10, gpo10Config);
            gpo10Config |= NeuropixelsV2e.GPO10ResetMask;
            serializer.WriteByte((uint)DS90UB9xSerializerI2CRegister.GPIO10, gpo10Config);
        }

        static void ConfigureProbeStreaming(I2CRegisterContext i2cNP)
        {
            // Write super sync bits into ASIC
            i2cNP.WriteByte(NeuropixelsV2e.SUPERSYNC11, 0b00011000);
            i2cNP.WriteByte(NeuropixelsV2e.SUPERSYNC10, 0b01100001);
            i2cNP.WriteByte(NeuropixelsV2e.SUPERSYNC9, 0b10000110);
            i2cNP.WriteByte(NeuropixelsV2e.SUPERSYNC8, 0b00011000);
            i2cNP.WriteByte(NeuropixelsV2e.SUPERSYNC7, 0b01100001);
            i2cNP.WriteByte(NeuropixelsV2e.SUPERSYNC6, 0b10000110);
            i2cNP.WriteByte(NeuropixelsV2e.SUPERSYNC5, 0b00011000);
            i2cNP.WriteByte(NeuropixelsV2e.SUPERSYNC4, 0b01100001);
            i2cNP.WriteByte(NeuropixelsV2e.SUPERSYNC3, 0b10000110);
            i2cNP.WriteByte(NeuropixelsV2e.SUPERSYNC2, 0b00011000);
            i2cNP.WriteByte(NeuropixelsV2e.SUPERSYNC1, 0b01100001);
            i2cNP.WriteByte(NeuropixelsV2e.SUPERSYNC0, 0b10111001);

            // Activate recording mode on NP
            i2cNP.WriteByte(NeuropixelsV2e.OP_MODE, 0b0100_0000);

            // Set global ADC settings
            // TODO: Undocumented
            i2cNP.WriteByte(NeuropixelsV2e.ADC_CONFIG, 0b0000_1000);
        }
    }

    static class NeuropixelsV2e
    {
        public const int ProbeAddress = 0x10;
        public const int FlexEEPROMAddress = 0x50;

        public const uint GPO10SupplyMask = 1 << 3; // Used to turn on VDDA analog supply
        public const uint GPO10ResetMask = 1 << 7; // Used to issue full reset commands to probes
        public const byte DefaultGPO10Config = 0b0001_0001; // NPs in reset, VDDA not enabled
        public const byte NoProbeSelected = 0b0001_0001; // No probes selected
        public const byte ProbeASelected = 0b0001_1001; // TODO: Changes in Rev. B of headstage
        public const byte ProbeBSelected = 0b1001_1001;

        public const int FramesPerSuperFrame = 16;
        public const int ADCsPerProbe = 24;
        public const int ChannelCount = 384;
        public const int FrameWords = 36; // TRASH TRASH TRASH 0 ADC0 ADC8 ADC16 0 ADC1 ADC9 ADC17 0 ... ADC7 ADC15 ADC23 0

        // unmanaged register map
        public const uint OP_MODE = 0x00;
        public const uint REC_MODE = 0x01;
        public const uint CAL_MODE = 0x02;
        public const uint ADC_CONFIG = 0x03;
        public const uint TEST_CONFIG1 = 0x04;
        public const uint TEST_CONFIG2 = 0x05;
        public const uint TEST_CONFIG3 = 0x06;
        public const uint TEST_CONFIG4 = 0x07;
        public const uint TEST_CONFIG5 = 0x08;
        public const uint STATUS = 0x09;
        public const uint SUPERSYNC0 = 0x0A;
        public const uint SUPERSYNC1 = 0x0B;
        public const uint SUPERSYNC2 = 0x0C;
        public const uint SUPERSYNC3 = 0x0D;
        public const uint SUPERSYNC4 = 0x0E;
        public const uint SUPERSYNC5 = 0x0F;
        public const uint SUPERSYNC6 = 0x10;
        public const uint SUPERSYNC7 = 0x11;
        public const uint SUPERSYNC8 = 0x12;
        public const uint SUPERSYNC9 = 0x13;
        public const uint SUPERSYNC10 = 0x14;
        public const uint SUPERSYNC11 = 0x15;
        public const uint SR_CHAIN6 = 0x16; // Odd channel base config
        public const uint SR_CHAIN5 = 0x17; // Even channel base config
        public const uint SR_CHAIN4 = 0x18; // Shank 4
        public const uint SR_CHAIN3 = 0x19; // Shank 3
        public const uint SR_CHAIN2 = 0x1A; // Shank 2
        public const uint SR_CHAIN1 = 0x1B; // Shank 1
        public const uint SR_LENGTH2 = 0x1C;
        public const uint SR_LENGTH1 = 0x1D;
        public const uint PROBE_ID = 0x1E;
        public const uint SOFT_RESET = 0x1F;

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(NeuropixelsV2e))
            {
            }
        }
    }


}
