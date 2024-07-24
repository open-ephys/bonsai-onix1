using System.Collections.Generic;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    /// <summary>
    /// A class that configures a NeuropixelsV2eBeta headstage.
    /// </summary>
    public class ConfigureNeuropixelsV2eBetaHeadstage : HubDeviceFactory
    {
        PortName port;
        readonly ConfigureNeuropixelsV2eLinkController LinkController = new();

        /// <summary>
        /// Initialize a new instance of a <see cref="ConfigureNeuropixelsV2eBetaHeadstage"/> object.
        /// </summary>
        public ConfigureNeuropixelsV2eBetaHeadstage()
        {
            Port = PortName.PortA;
            LinkController.HubConfiguration = HubConfiguration.Passthrough;
        }

        /// <summary>
        /// Get or set a <see cref="ConfigureNeuropixelsV2eBeta"/> object.
        /// </summary>
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureNeuropixelsV2eBeta NeuropixelsV2Beta { get; set; } = new();

        /// <summary>
        /// Get or set a <see cref="ConfigureNeuropixelsV2eBno055"/> object.
        /// </summary>
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
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
                NeuropixelsV2Beta.DeviceAddress = offset + 0;
                Bno055.DeviceAddress = offset + 1;
            }
        }

        /// <summary>
        /// Get or set the port voltage.
        /// </summary>
        /// <remarks>
        /// If a port voltage is defined this will override the automated voltage discovery and applies
        /// the specified voltage to the headstage. To enable automated voltage discovery, leave this field 
        /// empty. Warning: This device requires 3.0V to 5.0V for proper operation. Voltages higher than 5.0V can 
        /// damage the headstage
        /// </remarks>
        [Description("If defined, overrides automated voltage discovery and applies " +
            "the specified voltage to the headstage. Warning: this device requires 3.0V to 5.0V " +
            "for proper operation. Higher voltages can damage the headstage.")]
        public double? PortVoltage
        {
            get => LinkController.PortVoltage;
            set => LinkController.PortVoltage = value;
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return LinkController;
            yield return NeuropixelsV2Beta;
            yield return Bno055;
        }
    }
}
