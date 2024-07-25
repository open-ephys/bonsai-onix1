using System.Collections.Generic;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    /// <summary>
    /// A class that configures an ONIX breakout board.
    /// </summary>
    public class ConfigureBreakoutBoard : HubDeviceFactory
    {
        /// <summary>
        /// Gets or sets the heartbeat configuration.
        /// </summary>
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureHeartbeat Heartbeat { get; set; } = new();

        /// <summary>
        /// Gets or sets the breakout board's analog IO configuration.
        /// </summary>
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureAnalogIO AnalogIO { get; set; } = new();

        /// <summary>
        /// Gets or sets the breakout board's digital IO configuration.
        /// </summary>
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureDigitalIO DigitalIO { get; set; } = new();

        /// <summary>
        /// Gets or sets the hardware memory monitor configuration.
        /// </summary>
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureMemoryMonitor MemoryMonitor { get; set; } = new();

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return Heartbeat;
            yield return AnalogIO;
            yield return DigitalIO;
            yield return MemoryMonitor; 
        }
    }
}
