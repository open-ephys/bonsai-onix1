using System.Collections.Generic;
using System.ComponentModel;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a NeuropixelsV2e headstage on the specified port.
    /// </summary>
    /// <remarks>
    /// The NeuropixelsV2e Headstage is a 0.64g serialized, multifunction headstage designed to
    /// function with IMEC Neuropixels V2 probes. It provides the following features:
    /// <list type="bullet">
    /// <item><description>Support for dual IMEC Neuropixels 2.0 probes, each of which features:
    /// <list type="bullet">
    /// <item><description>Either 1x or 4x silicon shanks with a 70 x 24 µm cross-section.</description></item>
    /// <item><description>1280 electrodes low-impedance TiN electrodes per shank.</description></item>
    /// <item><description>384 parallel, full-band (AP, LFP), low-noise recording channels.</description></item>
    /// </list>
    /// </description></item>
    /// <item><description>A BNO055 9-axis IMU for real-time, 3D orientation tracking.</description></item>
    /// </list>
    /// </remarks>
    [Editor("OpenEphys.Onix1.Design.NeuropixelsV2eHeadstageEditor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    [Description("Configures a NeuropixelsV2e headstage on the specified port.")]
    public class ConfigureNeuropixelsV2eHeadstage : MultiDeviceFactory
    {
        PortName port;
        readonly ConfigureNeuropixelsV2ePortController PortControl = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureNeuropixelsV2e"/> class.
        /// </summary>
        public ConfigureNeuropixelsV2eHeadstage()
        {
            Port = PortName.PortA;
            PortControl.HubConfiguration = HubConfiguration.Passthrough;
        }

        /// <summary>
        /// Gets or sets the NeuropixelsV2e configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the NeuropixelsV2e device.")]
        public ConfigureNeuropixelsV2e NeuropixelsV2e { get; set; } = new();

        /// <summary>
        /// Gets or sets the Bno055 9-axis inertial measurement unit configuration.
        /// </summary>
        [Category(DevicesCategory)]
        [TypeConverter(typeof(PolledBno055SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Bno055 device.")]
        public ConfigurePolledBno055 Bno055 { get; set; } =
            new ConfigurePolledBno055 { AxisMap = Bno055AxisMap.YZX, AxisSign = Bno055AxisSign.MirrorX | Bno055AxisSign.MirrorY };

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
                NeuropixelsV2e.DeviceAddress = offset + 0;
                Bno055.DeviceAddress = offset + 1;
            }
        }

        /// <summary>
        /// Gets or sets the port voltage.
        /// </summary>
        /// <remarks>
        /// If a port voltage is defined this will override the automated voltage discovery and applies
        /// the specified voltage to the headstage. To enable automated voltage discovery, leave this field 
        /// empty. Warning: This device requires 3.0V to 5.5V for proper operation. Voltages higher than 5.5V can 
        /// damage the headstage
        /// </remarks>
        [Description("If defined, overrides automated voltage discovery and applies " +
            "the specified voltage to the headstage. Warning: this device requires 3.0V to 5.5V " +
            "for proper operation. Higher voltages can damage the headstage.")]
        [Category(ConfigurationCategory)]
        public double? PortVoltage
        {
            get => PortControl.PortVoltage;
            set => PortControl.PortVoltage = value;
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return PortControl;
            yield return NeuropixelsV2e;
            yield return Bno055;
        }
    }
}
