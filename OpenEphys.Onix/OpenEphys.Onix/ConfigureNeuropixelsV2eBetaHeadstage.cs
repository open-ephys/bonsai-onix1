using System.Collections.Generic;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    public class ConfigureNeuropixelsV2eBetaHeadstage : HubDeviceFactory
    {
        PortName port;
        readonly ConfigureNeuropixelsV2eLinkController LinkController = new();

        public ConfigureNeuropixelsV2eBetaHeadstage()
        {
            Port = PortName.PortA;
            LinkController.HubConfiguration = HubConfiguration.Passthrough;
        }

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureNeuropixelsV2eBeta NeuropixelsV2Beta { get; set; } = new();

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
                NeuropixelsV2Beta.DeviceAddress = offset + 0;
                Bno055.DeviceAddress = offset + 1;
            }
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return LinkController;
            yield return NeuropixelsV2Beta;
            yield return Bno055;
        }
    }
}
