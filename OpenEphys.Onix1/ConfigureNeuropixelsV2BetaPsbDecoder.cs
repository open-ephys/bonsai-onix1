using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Xml.Serialization;
using System.IO;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a parallel serial bus decoder for a single NeuropixelsV2-Beta probe.
    /// </summary>
    /// <remarks>
    /// This is a low-level device that is only useful within the context of an appropriate <see
    /// cref="MultiDeviceFactory"/>, e.g. <see cref="ConfigureHeadstageNeuropixelsV2eBeta"/>.
    /// </remarks>
    [Editor("OpenEphys.Onix1.Design.NeuropixelsV2eEditor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    [Description("Configures a parallel serial bus decoder for a single NeuropixelsV2-Beta probe.")]
    public class ConfigureNeuropixelsV2BetaPsbDecoder : SingleDeviceFactory
    {
        internal Action<I2CRegisterContext> SelectProbe { private get; set; } = _ => { };
        internal Action<I2CRegisterContext> SyncProbes { private get; set; } = _ => { };
        internal ushort StreamIndex { private get; set; } = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureNeuropixelsV2BetaPsbDecoder"/> class.
        /// </summary>
        public ConfigureNeuropixelsV2BetaPsbDecoder()
            : base(typeof(NeuropixelsV2Beta))
        {
        }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, <see cref="NeuropixelsV2eBetaData"/> will produce data. If set to false,
        /// <see cref="NeuropixelsV2eBetaData"/> will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the NeuropixelsV2-Beta device is enabled.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Gets or sets the LED enable state.
        /// </summary>
        /// <remarks>
        /// If true, the headstage LED will turn on during data acquisition. If false, the LED will not turn on.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the headstage LED will turn on during acquisition.")]
        public bool EnableLed { get; set; } = true;

        /// <summary>
        /// Gets or sets the probe configuration.
        /// </summary>
        [Category(ConfigurationCategory)]
        [Description("Probe configuration.")]
        [Editor("OpenEphys.Onix1.Design.NeuropixelsV2eProbeConfigurationEditor, OpenEphys.Onix1.Design", typeof(UITypeEditor))]
        [XmlElement(nameof(ProbeConfiguration), typeof(NeuropixelsV2QuadShankProbeConfiguration))]
        [TypeConverter(typeof(GenericPropertyConverter))]
        public NeuropixelsV2ProbeConfiguration ProbeConfiguration { get; set; }
            = new NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2QuadShankReference.External);

        /// <summary>
        /// Configures a NeuropixelsV2-Beta device.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> node
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> that holds all configuration actions.</param>
        /// <returns>
        /// The original sequence with the side effect of an additional configuration action to configure
        /// a NeuropixelsV2-Beta device.
        /// </returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var enableLed = EnableLed;
            var probeConfiguration = ProbeConfiguration;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            var streamIndex = StreamIndex;
            return source.ConfigureAndLatchDevice(context =>
            {
                // configure device via the DS90UB9x deserializer device
                var device = context.GetPassthroughDeviceContext(deviceAddress, typeof(DS90UB9x));
                var serializer = new I2CRegisterContext(device, DS90UB9x.SER_ADDR);

                // read probe metadata
                SelectProbe(serializer);
                var probeMetadata = new NeuropixelsV2eBetaMetadata(serializer);

                NeuropixelsV2GainCorrection? gainCorrection = null;
                var probeControl = new NeuropixelsV2eBetaRegisterContext(device, NeuropixelsV2Beta.ProbeAddress);

                if (probeMetadata.ProbeSerialNumber != null)
                {
                    if (probeConfiguration.IsGroundReference())
                    {
                        throw new InvalidOperationException(
                            "Neuropixels 2.0-Beta probes do not provide a Ground reference selection. " +
                            "Please select a different reference.");
                    }

                    if (enable)
                    {
                        // NB: the DS90UB9x supports multiple streams and we don't want to overwrite other
                        // streams' enable state. So, only enable, do not disable.
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

                        probeControl.WriteConfiguration(probeConfiguration);

                        // configure probe streaming
                        probeControl.WriteByte(NeuropixelsV2Beta.OP_MODE, 0b0100_0000);
                        probeControl.WriteByte(NeuropixelsV2Beta.ADC_CONFIG, 0b0000_1000);
                    }
                }

                SyncProbes(serializer);

                var deviceInfo = new NeuropixelsV2BetaPsbDecoderDeviceInfo(
                    context, DeviceType, deviceAddress, streamIndex,
                    gainCorrection?.GainCorrectionFactor ?? 1.0, probeConfiguration);
                return DeviceManager.RegisterDevice(deviceName, deviceInfo);
            });
        }
    }
}
