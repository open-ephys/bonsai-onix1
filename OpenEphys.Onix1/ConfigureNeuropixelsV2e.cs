using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Reactive.Disposables;
using System.Xml.Serialization;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a NeuropixelsV2e device.
    /// </summary>
    [Editor("OpenEphys.Onix1.Design.NeuropixelsV2eEditor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    [Description("Configures a NeuropixelsV2e device.")]
    public class ConfigureNeuropixelsV2e : SingleDeviceFactory, IConfigureNeuropixelsV2
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureNeuropixelsV2e"/> class.
        /// </summary>
        public ConfigureNeuropixelsV2e()
            : base(typeof(NeuropixelsV2e))
        {
        }

        /// <summary>
        /// Copy constructor for the <see cref="ConfigureNeuropixelsV2e"/> class.
        /// </summary>
        /// <param name="configureNode">A pre-existing <see cref="ConfigureNeuropixelsV2e"/> object.</param>
        public ConfigureNeuropixelsV2e(ConfigureNeuropixelsV2e configureNode)
            : base(typeof(NeuropixelsV2e))
        {
            Enable = configureNode.Enable;
            ProbeConfigurationA = configureNode.ProbeConfigurationA;
            ProbeConfigurationB = configureNode.ProbeConfigurationB;
            ProbeGroupFileNameA = configureNode.ProbeGroupFileNameA;
            ProbeGroupFileNameB = configureNode.ProbeGroupFileNameB;
            DeviceName = configureNode.DeviceName;
            DeviceAddress = configureNode.DeviceAddress;
            InvertPolarity = configureNode.InvertPolarity;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// If set to true, <see cref="NeuropixelsV2eData"/> will produce data. If set to false, 
        /// <see cref="NeuropixelsV2eData"/> will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the NeuropixelsV2 device is enabled.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Gets or sets a value determining if the polarity of the electrode voltages acquired by the probe
        /// should be inverted.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The analog channels on the probe ASIC have negative gain coefficients. This means that neural data
        /// that is captured by the probe will be inverted compared to the physical signal that occurs at the
        /// electrode: e.g. extracellular action potentials will tend to have positive deflections instead of
        /// negative. Setting this property to true will apply a gain of -1 to neural data to undo this
        /// effect.
        /// </para>
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Invert the polarity of the electrode voltages acquired by the probe.")]
        public bool InvertPolarity { get; set; } = true;

        /// <inheritdoc/>
        /// <remarks>
        /// Configuration is accomplished using a GUI to aid in channel selection and relevant configuration properties.
        /// To open a probe configuration GUI, select the ellipses next the <see cref="ProbeConfigurationA"/> variable
        /// in the property pane, or double-click <see cref="ConfigureHeadstageNeuropixelsV2e"/> to configure both
        /// probes and the <see cref="ConfigurePolledBno055"/> simultaneously.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Probe A electrode configuration.")]
        [Editor("OpenEphys.Onix1.Design.NeuropixelsV2eProbeConfigurationEditor, OpenEphys.Onix1.Design", typeof(UITypeEditor))]
        [TypeConverter(typeof(GenericPropertyConverter))]
        public NeuropixelsV2QuadShankProbeConfiguration ProbeConfigurationA { get; set; } = new(NeuropixelsV2Probe.ProbeA);

        /// <summary>
        /// Gets or sets the file path to a configuration file holding the Probe Group JSON specifications for this probe.
        /// </summary>
        [XmlIgnore]
        [Category(ConfigurationCategory)]
        [Description("File path to a configuration file holding the Probe Group JSON specifications for this probe. If left empty, a default file will be created next to the *.bonsai file when it is saved.")]
        [FileNameFilter(ProbeGroupHelper.ProbeGroupFileNameFilter)]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [TypeConverter(typeof(ProbeGroupHelper.ProbeGroupFileNameConverter))]
        public string ProbeGroupFileNameA { get; set; } = "";

        private string GetProbeGroupFileName(NeuropixelsV2Probe probe)
        {
            var name = probe switch
            {
                NeuropixelsV2Probe.ProbeA => DeviceType.Name + "_probeA",
                NeuropixelsV2Probe.ProbeB => DeviceType.Name + "_probeB",
                _ => throw new NotImplementedException(),
            };

            return ProbeGroupHelper.GenerateProbeGroupFileName(DeviceAddress, name);
        }

        /// <summary>
        /// Gets or sets a string defining the path to an external ProbeGroup JSON file.
        /// This variable is needed to properly save a workflow in Bonsai, but it is not
        /// directly accessible in the Bonsai editor.
        /// </summary>
        [Browsable(false)]
        [Externalizable(false)]
        [XmlElement(nameof(ProbeGroupFileNameA))]
        public string ProbeGroupFileNameSerializeA
        {
            get
            {
                var filename = string.IsNullOrEmpty(ProbeGroupFileNameA)
                                ? GetProbeGroupFileName(NeuropixelsV2Probe.ProbeA)
                                : ProbeGroupFileNameA;

                ProbeGroupHelper.SaveExternalProbeGroupFile(ProbeConfigurationA.ProbeGroup, filename);
                return ProbeGroupFileNameA;
            }
            set
            {
                ProbeGroupFileNameA = value;
                var filename = string.IsNullOrEmpty(ProbeGroupFileNameA)
                                ? GetProbeGroupFileName(NeuropixelsV2Probe.ProbeA)
                                : ProbeGroupFileNameA;

                // NB: If a file does not exist at the default file path, leave the default probe group settings as-is
                if (string.IsNullOrEmpty(ProbeGroupFileNameA) && !File.Exists(filename))
                {
                    return;
                }

                ProbeConfigurationA = new(ProbeGroupHelper.LoadExternalProbeGroupFile<NeuropixelsV2eProbeGroup>(filename),
                                        ProbeConfigurationA.Reference, ProbeConfigurationA.Probe, ProbeConfigurationA.GainCalibrationFile);
            }
        }

        /// <summary>
        /// Gets or sets the path to the gain calibration file for this probe.
        /// </summary>
        /// <remarks>
        /// [Obsolete]. Cannot tag this property with the Obsolete attribute due to https://github.com/dotnet/runtime/issues/100453
        /// </remarks>
        [Browsable(false)]
        [Externalizable(false)]
        public string GainCalibrationFileA
        {
            get => ProbeConfigurationA.GainCalibrationFile;
            set => ProbeConfigurationA.GainCalibrationFile = value;
        }

        /// <summary>
        /// Prevent the GainCalibrationFile property from being serialized. Will be removed in 1.0.0.
        /// </summary>
        /// <returns>False</returns>
        public bool ShouldSerializeGainCalibrationFileA()
        {
            return false;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Configuration is accomplished using a GUI to aid in channel selection and relevant configuration properties.
        /// To open a probe configuration GUI, select the ellipses next the <see cref="ProbeConfigurationB"/> variable
        /// in the property pane, or double-click <see cref="ConfigureHeadstageNeuropixelsV2e"/> to configure both
        /// probes and the <see cref="ConfigurePolledBno055"/> simultaneously.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Probe B electrode configuration.")]
        [Editor("OpenEphys.Onix1.Design.NeuropixelsV2eProbeConfigurationEditor, OpenEphys.Onix1.Design", typeof(UITypeEditor))]
        [TypeConverter(typeof(GenericPropertyConverter))]
        public NeuropixelsV2QuadShankProbeConfiguration ProbeConfigurationB { get; set; } = new(NeuropixelsV2Probe.ProbeB);

        /// <summary>
        /// Gets or sets the file path to a configuration file holding the Probe Group JSON specifications for this probe.
        /// </summary>
        [XmlIgnore]
        [Category(ConfigurationCategory)]
        [Description("File path to a configuration file holding the Probe Group JSON specifications for this probe. If left empty, a default file will be created next to the *.bonsai file when it is saved.")]
        [FileNameFilter(ProbeGroupHelper.ProbeGroupFileNameFilter)]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string ProbeGroupFileNameB { get; set; } = "";

        /// <summary>
        /// Gets or sets a string defining the path to an external ProbeGroup JSON file.
        /// This variable is needed to properly save a workflow in Bonsai, but it is not
        /// directly accessible in the Bonsai editor.
        /// </summary>
        [Browsable(false)]
        [Externalizable(false)]
        [XmlElement(nameof(ProbeGroupFileNameB))]
        public string ProbeGroupFileNameSerializeB
        {
            get
            {
                var filename = string.IsNullOrEmpty(ProbeGroupFileNameB)
                                ? GetProbeGroupFileName(NeuropixelsV2Probe.ProbeB)
                                : ProbeGroupFileNameB;

                ProbeGroupHelper.SaveExternalProbeGroupFile(ProbeConfigurationB.ProbeGroup, filename);
                return ProbeGroupFileNameB;
            }
            set
            {
                ProbeGroupFileNameB = value;
                var filename = string.IsNullOrEmpty(ProbeGroupFileNameB)
                                ? GetProbeGroupFileName(NeuropixelsV2Probe.ProbeB)
                                : ProbeGroupFileNameB;

                // NB: If a file does not exist at the default file path, leave the default probe group settings as-is
                if (string.IsNullOrEmpty(ProbeGroupFileNameB) && !File.Exists(filename))
                {
                    return;
                }

                ProbeConfigurationB = new(ProbeGroupHelper.LoadExternalProbeGroupFile<NeuropixelsV2eProbeGroup>(filename),
                                        ProbeConfigurationB.Reference, ProbeConfigurationB.Probe, ProbeConfigurationB.GainCalibrationFile);
            }
        }

        /// <summary>
        /// Gets or sets the path to the gain calibration file for this probe.
        /// </summary>
        /// <remarks>
        /// [Obsolete]. Cannot tag this property with the Obsolete attribute due to https://github.com/dotnet/runtime/issues/100453
        /// </remarks>
        [Browsable(false)]
        [Externalizable(false)]
        public string GainCalibrationFileB
        {
            get => ProbeConfigurationB.GainCalibrationFile;
            set => ProbeConfigurationB.GainCalibrationFile = value;
        }

        /// <summary>
        /// Prevent the GainCalibrationFile property from being serialized.
        /// </summary>
        /// <returns>False</returns>
        [Obsolete]
        public bool ShouldSerializeGainCalibrationFileB()
        {
            return false;
        }

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
            var invertPolarity = InvertPolarity;
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
                DS90UB9x.Set933I2CRate(device, 400e3);

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
                    var gainCorrection = NeuropixelsV2Helper.TryParseGainCalibrationFile(ProbeConfigurationA.GainCalibrationFile);

                    if (!gainCorrection.HasValue)
                    {
                        throw new ArgumentException($"{NeuropixelsV2Probe.ProbeA}'s calibration file \"{ProbeConfigurationA.GainCalibrationFile}\" is invalid.");
                    }

                    if (gainCorrection.Value.SerialNumber != probeAMetadata.ProbeSerialNumber)
                    {
                        throw new ArgumentException($"The probe serial number ({probeAMetadata.ProbeSerialNumber}) does not " +
                            $"match the gain calibration file serial number: {gainCorrection.Value.SerialNumber}.");
                    }

                    gainCorrectionA = gainCorrection.Value.GainCorrectionFactor;

                    SelectProbe(serializer, NeuropixelsV2e.ProbeASelected);
                    probeControl.WriteConfiguration(ProbeConfigurationA);
                    ConfigureProbeStreaming(probeControl);
                }

                // configure probe B streaming
                if (probeBMetadata.ProbeSerialNumber != null)
                {
                    var gainCorrection = NeuropixelsV2Helper.TryParseGainCalibrationFile(ProbeConfigurationB.GainCalibrationFile);

                    if (!gainCorrection.HasValue)
                    {
                        throw new ArgumentException($"{NeuropixelsV2Probe.ProbeB}'s calibration file \"{ProbeConfigurationB.GainCalibrationFile}\" is invalid.");
                    }

                    if (gainCorrection.Value.SerialNumber != probeBMetadata.ProbeSerialNumber)
                    {
                        throw new ArgumentException($"The probe serial number ({probeBMetadata.ProbeSerialNumber}) does not " +
                            $"match the gain calibration file serial number: {gainCorrection.Value.SerialNumber}.");
                    }

                    gainCorrectionB = gainCorrection.Value.GainCorrectionFactor;

                    SelectProbe(serializer, NeuropixelsV2e.ProbeBSelected);
                    probeControl.WriteConfiguration(ProbeConfigurationB);
                    ConfigureProbeStreaming(probeControl);
                }

                // disconnect i2c bus from both probes to prevent digital interference during acquisition
                SelectProbe(serializer, NeuropixelsV2e.NoProbeSelected);

                var deviceInfo = new NeuropixelsV2eDeviceInfo(context, DeviceType, deviceAddress, gainCorrectionA, gainCorrectionB, invertPolarity);
                var shutdown = Disposable.Create(() =>
                {
                    serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, NeuropixelsV2e.DefaultGPO10Config);
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

            DS90UB9x.Initialize933SerDesLink(device, DS90UB9xMode.Raw12BitHighFrequency);

            // configure deserializer I2C aliases
            var deserializer = new I2CRegisterContext(device, DS90UB9x.DES_ADDR);

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
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, gpo10Config);
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
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio32, probeSelect);
            System.Threading.Thread.Sleep(20);
        }

        static void ResetProbes(I2CRegisterContext serializer, uint gpo10Config)
        {
            gpo10Config &= ~NeuropixelsV2e.GPO10ResetMask;
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, gpo10Config);
            gpo10Config |= NeuropixelsV2e.GPO10ResetMask;
            serializer.WriteByte((uint)DS90UB933SerializerI2CRegister.Gpio10, gpo10Config);
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
        public const int AdcsPerProbe = 24;
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
