using System.Collections.Generic;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    [Editor("OpenEphys.Onix.Design.NeuropixelsV2eHeadstageEditor, OpenEphys.Onix.Design", typeof(ComponentEditor))]
    public class ConfigureNeuropixelsV2eHeadstage : HubDeviceFactory
    {
        PortName port;
        readonly ConfigureNeuropixelsV2eLinkController LinkController = new();

        public ConfigureNeuropixelsV2eHeadstage()
        {
            Port = PortName.PortA;
            LinkController.HubConfiguration = HubConfiguration.Passthrough;
        }

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureNeuropixelsV2e NeuropixelsV2 { get; set; } = new();

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureNeuropixelsV2eBno055 Bno055 { get; set; } = new();

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
