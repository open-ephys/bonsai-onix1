using System.Collections.Generic;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    public class ConfigureUclaMiniscopeV4 : HubDeviceFactory
    {
        PortName port;
        readonly ConfigureUclaMiniscopeV4LinkController LinkController = new();

        public ConfigureUclaMiniscopeV4()
        {
            Port = PortName.PortA;
            LinkController.HubConfiguration = HubConfiguration.Passthrough;
        }

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureUclaMiniscopeV4Camera Camera { get; set; } = new();

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureUclaMiniscopeV4Bno055 Bno055 { get; set; } = new();

        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                var offset = (uint)port << 8;
                LinkController.DeviceAddress = (uint)port;
                Camera.DeviceAddress = offset + 0;
                Bno055.DeviceAddress = offset + 1;
            }
        }

        [Description("If defined, it will override automated voltage discovery and apply the specified voltage" +
                     "to the headstage. Warning: this device requires 5.0V to 6.0V for proper operation." +
                     "Supplying higher voltages may result in damage to the headstage.")]
        public double? PortVoltage
        {
            get => LinkController.PortVoltage;
            set => LinkController.PortVoltage = value;
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return LinkController;
            yield return Camera;
            yield return Bno055;
        }
    }
}
