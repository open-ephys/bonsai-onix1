using System.Collections.Generic;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    public class ConfigureNeuropixelsV2BetaHeadstage : HubDeviceFactory
    {
        PortName port;
        readonly ConfigureFmcLinkController LinkController = new();

        public ConfigureNeuropixelsV2BetaHeadstage()
        {
            Port = PortName.PortA;
            LinkController.HubConfiguration = HubConfiguration.Passthrough;
            LinkController.MinVoltage = 5.0;
            LinkController.MaxVoltage = 7.0;
        }

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureNeuropixelsV2Beta NeuropixelsV2Beta { get; set; } = new();

        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                var offset = (uint)port << 8;
                LinkController.DeviceAddress = (uint)port;
                NeuropixelsV2Beta.DeviceAddress = offset + 0;
            }
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return LinkController;
            yield return NeuropixelsV2Beta;
        }
    }
}
