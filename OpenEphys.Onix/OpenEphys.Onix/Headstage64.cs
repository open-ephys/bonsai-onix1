using System.Collections.Generic;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    public class Headstage64 : HubDeviceFactory
    {
        PortName port;
        readonly ConfigureFmcLinkController LinkController = new();

        public Headstage64()
        {
            Port = PortName.PortA;
            LinkController.Passthrough = false;
            LinkController.MinVoltage = 3.3;
            LinkController.MaxVoltage = 8.0;
        }

        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureRhd2164 Rhd2164 { get; set; } = new();

        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureBno055 Bno055 { get; set; } = new();

        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                LinkController.DeviceAddress = (uint)port;
                Rhd2164.DeviceAddress = ((uint)port << 8) + 0;
                Bno055.DeviceAddress = ((uint)port << 8) + 1;
            }
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return LinkController;
            yield return Rhd2164;
            yield return Bno055;
        }
    }

    public enum PortName
    {
        PortA = 1,
        PortB = 2
    }
}
