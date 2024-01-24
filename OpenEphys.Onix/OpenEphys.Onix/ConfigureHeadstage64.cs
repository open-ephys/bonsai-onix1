using System.Collections.Generic;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    public class ConfigureHeadstage64 : HubDeviceFactory
    {
        PortName port;
        readonly ConfigureFmcLinkController LinkController = new();

        public ConfigureHeadstage64()
        {
            // TODO: The issue with this headstage is that its locking voltage is far, far lower than the votlage requried for full
            // functionality. Locking occurs at aroud 2V on the headstage (enough to turn 1.8V on). Full functionality is 5.0 volts.
            // Whats worse: the port voltage can only go down to 3.3V, which means that its very hard to find a the true lowest voltage
            // for a lock and then add a large offset to that.
            Port = PortName.PortA;
            LinkController.HubConfiguration = HubConfiguration.Standard;
            LinkController.MinVoltage = 3.3;
            LinkController.MaxVoltage = 5.0;
            LinkController.VoltageOffset = 3.0;

        }
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureRhd2164 Rhd2164 { get; set; } = new();

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureBno055 Bno055 { get; set; } = new();

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureTS4231 TS4231 { get; set; } = new() { Enable = false };

        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                var offset = (uint)port << 8;
                LinkController.DeviceAddress = (uint)port;
                Rhd2164.DeviceAddress = offset + 0;
                Bno055.DeviceAddress = offset + 1;
                TS4231.DeviceAddress = offset + 2;
            }
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return LinkController;
            yield return Rhd2164;
            yield return Bno055;
            yield return TS4231;
        }
    }

    public enum PortName
    {
        PortA = 1,
        PortB = 2
    }
}
