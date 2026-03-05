using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Xml.Serialization;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a parallel serial bus decoder for a NeuropixelsV2 probe. 
    /// </summary>
    /// <remarks>
    /// This is a low-leve device that requires coordinated configuration wof the <cref>D</cref>
    /// </remarks>
    [Editor("OpenEphys.Onix1.Design.NeuropixelsV2eEditor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    [Description("Configures a parallel serial bus decoder for a NeuropixelsV2 probe.")]
    public class ConfigureNeuropixelsV2PsbDecoder : SingleDeviceFactory
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
        [XmlElement(nameof(ProbeConfiguration), typeof(NeuropixelsV2QuadShankProbeConfiguration))] // NB: Needed for backward compatibility
        [TypeConverter(typeof(GenericPropertyConverter))]
        public NeuropixelsV2ProbeConfiguration ProbeConfiguration { get; set; } 
            = new NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2QuadShankReference.External);

        /// <summary>
        /// Configures a NeuropixelsV2 device.
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
            var probeConfiguration = ProbeConfiguration;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            var streamIndex = StreamIndex;
            return source.ConfigureAndLatchDevice(context =>
            {
                // configure device via the DS90UB9x deserializer device
                var device = context.GetPassthroughDeviceContext(deviceAddress, typeof(DS90UB9x));
                var serializer = new I2CRegisterContext(device, DS90UB9x.SER_ADDR);

                // NB: the DS90UB9x supports multiple streams and we don't want to overwrite other streams' enable state
                if (enable)
                {
                    device.WriteRegister(DS90UB9x.ENABLE, 1u);
                }

                // read probe metadata
                SelectProbe(serializer);
                var probeMetadata = new NeuropixelsV2Metadata(serializer);

                // configure probe streaming
                NeuropixelsV2GainCorrection? gainCorrection = null;
                var probeControl = new NeuropixelsV2RegisterContext(device, NeuropixelsV2.ProbeAddress);

                // configure probe A streaming
                if (probeMetadata.ProbeSerialNumber != null)
                {
                    if (!File.Exists(probeConfiguration.GainCalibrationFileName))
                    {
                        throw new ArgumentException($"A gain calibration file must be provided.");
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
                    ConfigureProbeStreaming(probeControl, enable);
                }

                // disconnect i2c bus from both probes to prevent digital interference during acquisition
                DeselectProbe(serializer);

                var deviceInfo = new NeuropixelsV2PsbDecoderDeviceInfo(context, DeviceType, deviceAddress, streamIndex, gainCorrection?.GainCorrectionFactor, probeConfiguration);
                return DeviceManager.RegisterDevice(deviceName, deviceInfo);
            });
        }

        static void ConfigureProbeStreaming(I2CRegisterContext i2cNP, bool probeEnabled)
        {
            if (probeEnabled)
            {
                // Write super sync bits into ASIC
                i2cNP.WriteByte(NeuropixelsV2.SUPERSYNC11, 0b00011000);
                i2cNP.WriteByte(NeuropixelsV2.SUPERSYNC10, 0b01100001);
                i2cNP.WriteByte(NeuropixelsV2.SUPERSYNC9, 0b10000110);
                i2cNP.WriteByte(NeuropixelsV2.SUPERSYNC8, 0b00011000);
                i2cNP.WriteByte(NeuropixelsV2.SUPERSYNC7, 0b01100001);
                i2cNP.WriteByte(NeuropixelsV2.SUPERSYNC6, 0b10000110);
                i2cNP.WriteByte(NeuropixelsV2.SUPERSYNC5, 0b00011000);
                i2cNP.WriteByte(NeuropixelsV2.SUPERSYNC4, 0b01100001);
                i2cNP.WriteByte(NeuropixelsV2.SUPERSYNC3, 0b10000110);
                i2cNP.WriteByte(NeuropixelsV2.SUPERSYNC2, 0b00011000);
                i2cNP.WriteByte(NeuropixelsV2.SUPERSYNC1, 0b01100001);
                i2cNP.WriteByte(NeuropixelsV2.SUPERSYNC0, 0b10111001);

                // Activate recording mode on NP
                i2cNP.WriteByte(NeuropixelsV2.OP_MODE, 0b0100_0000);
            }
            else
            {
                // Power down the probe
                i2cNP.WriteByte(NeuropixelsV2.OP_MODE, 0b1000_0000);
            }
        }
    }
}
