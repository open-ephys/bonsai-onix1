using System.Collections.Generic;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    public class ConfigureNeuropixelsV2Headstage : HubDeviceFactory
    {
        PortName port;
        readonly ConfigureFmcLinkController LinkController = new();

        public ConfigureNeuropixelsV2Headstage()
        {
            Port = PortName.PortA;
            LinkController.HubConfiguration = HubConfiguration.Passthrough;
            LinkController.MinVoltage = 5.0;
            LinkController.MaxVoltage = 7.0;
        }

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureNeuropixelsV2 NeuropixelsV2 { get; set; } = new();

        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                var serializerAddress = ((uint)port - 1) + 8;
                LinkController.DeviceAddress = (uint)port;
                NeuropixelsV2.DeviceAddress = serializerAddress;
            }
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return LinkController;
            yield return NeuropixelsV2;
        }
    }
}
