using System.Collections.Generic;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    /// <summary>
    /// A class that configures a NeuropixelsV2e headstage.
    /// </summary>
    public class ConfigureNeuropixelsV2eHeadstage : HubDeviceFactory
    {
        PortName port;
        readonly ConfigureNeuropixelsV2eLinkController LinkController = new();

        /// <summary>
        /// Initialize a new instance of a <see cref="ConfigureNeuropixelsV2e"/> object.
        /// </summary>
        public ConfigureNeuropixelsV2eHeadstage()
        {
            Port = PortName.PortA;
            LinkController.HubConfiguration = HubConfiguration.Passthrough;
        }

        /// <summary>
        /// Get or set a <see cref="ConfigureNeuropixelsV2e"/> object.
        /// </summary>
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        [Description("Configure a NeuropixelsV2e device.")]
        public ConfigureNeuropixelsV2e NeuropixelsV2 { get; set; } = new();

        /// <summary>
        /// Get or set a <see cref="ConfigureNeuropixelsV2eBno055"/> object.
        /// </summary>
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        [Description("Configure a NeuropixelsV2eBno055 device.")]
        public ConfigureNeuropixelsV2eBno055 Bno055 { get; set; } = new();

        /// <summary>
        /// Get or set the <see cref="PortName"/>.
        /// </summary>
        /// <remarks>
        /// The port is the physical connection on the breakout board.
        /// </remarks>
        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                var offset = (uint)port << 8;
                LinkController.DeviceAddress = (uint)port;
                NeuropixelsV2.DeviceAddress = offset + 0;
                Bno055.DeviceAddress = offset + 1;
            }
        }

        /// <summary>
        /// Get or set the port voltage.
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
            yield return NeuropixelsV2;
            yield return Bno055;
        }
    }
}
