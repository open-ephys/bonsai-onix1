using System.Collections.Generic;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    public class ConfigureHeadstageRhs2116 : HubDeviceFactory
    {
        PortName port;
        readonly ConfigureFmcLinkController LinkController = new();

        public ConfigureHeadstageRhs2116()
        {
            // TODO: The issue with this headstage is that its locking voltage is far, far lower than the votlage requried for full
            // functionality. Locking occurs at aroud 2V on the headstage (enough to turn 1.8V on). Full functionality is 5.0 volts.
            // Whats worse: the port voltage can only go down to 3.3V, which means that its very hard to find a the true lowest voltage
            // for a lock and then add a large offset to that.
            Port = PortName.PortA;
            LinkController.HubConfiguration = HubConfiguration.Standard;
            LinkController.MinVoltage = 3.3;
            LinkController.MaxVoltage = 5.0;
            LinkController.VoltageOffset = 2.5;
            LinkController.VoltageOffSettleMillis = 500;
            LinkController.VoltageOnSettleMillis = 100;
        }

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureRhs2116 Rhs2116A { get; set; } = new();

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureRhs2116 Rhs2116B { get; set; } = new();

        internal override void UpdateDeviceNames(string hubName)
        {
            LinkController.DeviceName = $"{hubName}/{nameof(LinkController)}";
            Rhs2116A.DeviceName = $"{hubName}/{nameof(Rhs2116A)}";
            Rhs2116B.DeviceName = $"{hubName}/{nameof(Rhs2116B)}";
        }

        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                var offset = (uint)port << 8;
                LinkController.DeviceAddress = (uint)port;
                Rhs2116A.DeviceAddress = offset + 0;
                Rhs2116B.DeviceAddress = offset + 1;
            }
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return LinkController;
            yield return Rhs2116A;
            yield return Rhs2116B;
        }
    }
}
