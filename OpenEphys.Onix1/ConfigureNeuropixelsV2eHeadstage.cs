using System.Collections.Generic;
using System.ComponentModel;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A class that configures a NeuropixelsV2e headstage.
    /// </summary>
    [Description("configures a NeuropixelsV2e headstage.")]
    public class ConfigureNeuropixelsV2eHeadstage : MultiDeviceFactory
    {
        PortName port;
        readonly ConfigureNeuropixelsV2eLinkController LinkController = new();

        /// <summary>
        /// Initialize a new instance of a <see cref="ConfigureNeuropixelsV2e"/> class.
        /// </summary>
        public ConfigureNeuropixelsV2eHeadstage()
        {
            Port = PortName.PortA;
            LinkController.HubConfiguration = HubConfiguration.Passthrough;
        }

        /// <summary>
        /// Gets or sets the NeuropixelsV2e configuration.
        /// </summary>
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the NeuropixelsV2e device.")]
        public ConfigureNeuropixelsV2e NeuropixelsV2e { get; set; } = new();

        /// <summary>
        /// Gets or sets the Bno055 9-axis inertial measurement unit configuration.
        /// </summary>
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Bno055 device.")]
        public ConfigureNeuropixelsV2eBno055 Bno055 { get; set; } = new();

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <remarks>
        /// The port is the physical connection to the ONIX breakout board and must be specified prior to operation.
        /// </remarks>
        [Description("Specifies the physical connection of the headstage to the ONIX breakout board.")]
        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                var offset = (uint)port << 8;
                LinkController.DeviceAddress = (uint)port;
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
        public double? PortVoltage
        {
            get => LinkController.PortVoltage;
            set => LinkController.PortVoltage = value;
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return LinkController;
            yield return NeuropixelsV2e;
            yield return Bno055;
        }
    }
}
