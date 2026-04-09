using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Xml.Serialization;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a parallel serial bus decoder for a Neuropixels V2 probe.
    /// </summary>
    /// <remarks>
    /// This is a low-level device that is only useful within the context of an appropriate <see
    /// cref="MultiDeviceFactory"/>, e.g. <see cref="ConfigureHeadstageNeuropixelsV2e"/>.
    /// </remarks>
    [Editor("OpenEphys.Onix1.Design.NeuropixelsV2eEditor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    [Description("Configures a parallel serial bus decoder for a Neuropixels V2 probe.")]
    public class ConfigureNeuropixelsV2PsbDecoder : SingleDeviceFactory, IConfigureNeuropixelsV2
    {
        internal Action<I2CRegisterContext> SelectProbe { private get; set; } = _ => { };
        internal Action<I2CRegisterContext> DeselectProbe { private get; set; } = _ => { };
        internal ushort StreamIndex { private get; set; } = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureNeuropixelsV2PsbDecoder"/> class.
        /// </summary>
        public ConfigureNeuropixelsV2PsbDecoder()
            : base(typeof(NeuropixelsV2))
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

        /// <inheritdoc/>
        [Category(ConfigurationCategory)]
        [Description("Probe configuration.")]
        [Editor("OpenEphys.Onix1.Design.NeuropixelsV2eProbeConfigurationEditor, OpenEphys.Onix1.Design", typeof(UITypeEditor))]
        [XmlElement(nameof(ProbeConfiguration), typeof(NeuropixelsV2QuadShankProbeConfiguration))] // NB: Needed for backward compatibility; TODO: remove in 1.0.0
        [TypeConverter(typeof(GenericPropertyConverter))]
        public NeuropixelsV2ProbeConfiguration ProbeConfiguration { get; set; } 
            = new NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2QuadShankReference.External);

        /// <summary>
        /// Configures a Neuropixels V2 device.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> node
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> that holds all configuration actions.</param>
        /// <returns>
        /// The original sequence with the side effect of an additional configuration action to configure
        /// a Neuropixels V2 device.
        /// </returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var probeConfiguration = ProbeConfiguration.Clone();
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            var streamIndex = StreamIndex;

            return source.ConfigureAndLatchDevice(context =>
            {
                NeuropixelsV2eProbeGroup probeGroup = new NeuropixelsV2eQuadShankProbeGroup();

                // configure device via the DS90UB9x deserializer device
                var device = context.GetPassthroughDeviceContext(deviceAddress, typeof(DS90UB9x));
                var serializer = new I2CRegisterContext(device, DS90UB9x.SER_ADDR);

                // read probe metadata
                SelectProbe(serializer);
                var probeMetadata = new NeuropixelsV2Metadata(serializer);

                // configure probe streaming
                NeuropixelsV2GainCorrection? gainCorrection = null;
                var probeControl = new NeuropixelsV2RegisterContext(device, NeuropixelsV2.ProbeAddress);

                // check for probe being present
                if (probeMetadata.ProbeSerialNumber != null)
                {
                    if (enable)
                    {
                        // NB: the DS90UB9x supports multiple streams and we don't want to overwrite other
                        // streams' enable state. Therefore the else clause does not contain the inverse of
                        // this call
                        device.WriteRegister(DS90UB9x.ENABLE, 1u);

                        if (!File.Exists(probeConfiguration.GainCalibrationFileName))
                        {
                            throw new ArgumentException($"A gain calibration file must be specified for the probe with serial number " +
                                $"{probeMetadata.ProbeSerialNumber}");
                        }

                        gainCorrection = NeuropixelsV2Helper.TryParseGainCalibrationFile(probeConfiguration.GainCalibrationFileName);

                        if (!gainCorrection.HasValue)
                        {
                            throw new ArgumentException($"The calibration file \"{probeConfiguration.GainCalibrationFileName}\" has an invalid format.");
                        }

                        if (gainCorrection.Value.SerialNumber != probeMetadata.ProbeSerialNumber)
                        {
                            throw new ArgumentException($"The probe serial number ({probeMetadata.ProbeSerialNumber}) does not " +
                                $"match the gain calibration file serial number: {gainCorrection.Value.SerialNumber}.");
                        }

                        if (File.Exists(probeConfiguration.ProbeInterfaceFileName))
                        {
                            probeGroup = ProbeInterfaceHelper.LoadExternalProbeInterfaceFile(probeConfiguration.ProbeInterfaceFileName, probeConfiguration.GetProbeGroupType()) as NeuropixelsV2eProbeGroup;
                        }

                        // configure base and shank
                        probeControl.WriteConfiguration(probeConfiguration, probeGroup);

                        // write super sync bits into ASIC
                        probeControl.WriteByte(NeuropixelsV2.SUPERSYNC11, 0b00011000);
                        probeControl.WriteByte(NeuropixelsV2.SUPERSYNC10, 0b01100001);
                        probeControl.WriteByte(NeuropixelsV2.SUPERSYNC9, 0b10000110);
                        probeControl.WriteByte(NeuropixelsV2.SUPERSYNC8, 0b00011000);
                        probeControl.WriteByte(NeuropixelsV2.SUPERSYNC7, 0b01100001);
                        probeControl.WriteByte(NeuropixelsV2.SUPERSYNC6, 0b10000110);
                        probeControl.WriteByte(NeuropixelsV2.SUPERSYNC5, 0b00011000);
                        probeControl.WriteByte(NeuropixelsV2.SUPERSYNC4, 0b01100001);
                        probeControl.WriteByte(NeuropixelsV2.SUPERSYNC3, 0b10000110);
                        probeControl.WriteByte(NeuropixelsV2.SUPERSYNC2, 0b00011000);
                        probeControl.WriteByte(NeuropixelsV2.SUPERSYNC1, 0b01100001);
                        probeControl.WriteByte(NeuropixelsV2.SUPERSYNC0, 0b10111001);

                        // activate recording mode on NP
                        probeControl.WriteByte(NeuropixelsV2.OP_MODE, 0b0100_0000);

                    }
                    else
                    {
                        // power down the probe
                        probeControl.WriteByte(NeuropixelsV2.OP_MODE, 0b1000_0000);
                    }
                }

                // disconnect i2c bus from both probes to prevent digital interference during acquisition
                DeselectProbe(serializer);

                var deviceInfo = new NeuropixelsV2PsbDecoderDeviceInfo(context, DeviceType, deviceAddress, streamIndex, gainCorrection?.GainCorrectionFactor ?? 1.0, probeConfiguration, probeGroup);
                return DeviceManager.RegisterDevice(deviceName, deviceInfo);
            });
        }
    }
}
