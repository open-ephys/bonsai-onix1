using System;
using System.ComponentModel;
using System.IO;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a parallel serial bus decoder for a NeuropixelsV1 probe.
    /// </summary>
    /// <remarks>
    /// This is a low-level device that is only useful within the context of an appropriate <see
    /// cref="MultiDeviceFactory"/>, e.g. <see cref="ConfigureHeadstageNeuropixelsV1e"/>.
    /// </remarks>
    [Description("Configures a parallel serial bus decoder for a NeuropixelsV1 probe.")]
    [Editor("OpenEphys.Onix1.Design.NeuropixelsV1Editor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    public class ConfigureNeuropixelsV1PsbDecoder : SingleDeviceFactory, IConfigureNeuropixelsV1
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureNeuropixelsV1PsbDecoder"/> class.
        /// </summary>
        public ConfigureNeuropixelsV1PsbDecoder()
            : base(typeof(NeuropixelsV1))
        {
        }

        /// <inheritdoc/>
        /// <remarks>
        /// If set to true, <see cref="NeuropixelsV1eData"/> will produce data. If set to false,
        /// <see cref="NeuropixelsV1eData"/> will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the NeuropixelsV1 device is enabled.")]
        public bool Enable { get; set; } = true;

        /// <inheritdoc/>
        [Category(ConfigurationCategory)]
        [Description("NeuropixelsV1 probe configuration.")]
        [TypeConverter(typeof(GenericPropertyConverter))]
        public NeuropixelsV1ProbeConfiguration ProbeConfiguration { get; set; } = new();

        /// <summary>
        /// Configures a NeuropixelsV1 device.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> node
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> that holds all configuration actions.</param>
        /// <returns>
        /// The original sequence with the side effect of an additional configuration action to configure
        /// a NeuropixelsV1 device.
        /// </returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            var probeConfiguration = new NeuropixelsV1ProbeConfiguration(ProbeConfiguration);
            return source.ConfigureAndLatchDevice(context =>
            {
                NeuropixelsV1eProbeGroup probeGroup = new();

                if (File.Exists(probeConfiguration.ProbeInterfaceFileName))
                {
                    probeGroup = ProbeInterfaceHelper.LoadExternalProbeInterfaceFile(probeConfiguration.ProbeInterfaceFileName, typeof(NeuropixelsV1eProbeGroup)) as NeuropixelsV1eProbeGroup;
                }

                // configure device via the DS90UB9x deserializer device
                var device = context.GetPassthroughDeviceContext(deviceAddress, typeof(DS90UB9x));
                var serializer = new I2CRegisterContext(device, DS90UB9x.SER_ADDR);

                // NB: the DS90UB9x supports multiple streams and we don't want to overwrite other streams'
                // enable state
                if (enable)
                {
                    device.WriteRegister(DS90UB9x.ENABLE, 1u);
                }

                // read probe metadata
                var probeMetadata = new NeuropixelsV1eMetadata(device);

                // program shift registers
                var probeControl = new NeuropixelsV1RegisterContext(device, NeuropixelsV1.ProbeI2CAddress,
                    probeMetadata.ProbeSerialNumber, probeConfiguration, probeGroup);
                probeControl.InitializeProbe();
                probeControl.WriteConfiguration();
                probeControl.StartAcquisition();

                var deviceInfo = new NeuropixelsV1PsbDecoderDeviceInfo(context, DeviceType, deviceAddress, probeControl, probeConfiguration, probeGroup);
                return DeviceManager.RegisterDevice(deviceName, deviceInfo);
            });
        }
    }
}
