using System.Collections.Generic;
using System.ComponentModel;
using Bonsai;

namespace OpenEphys.Onix
{
    public class ConfigureBreakoutBoard : HubDeviceFactory, INamedElement
    {
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureHeartbeat Heartbeat { get; set; } = new();

        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureAnalogIO AnalogIO { get; set; } = new();

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return Heartbeat;
            yield return AnalogIO;
        }
    }
}
