using System.Collections.Generic;
using System.ComponentModel;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A class that configures an ONIX breakout board.
    /// </summary>
    [Description("Configures an ONIX breakout board.")]
    public class ConfigureBreakoutBoard : MultiDeviceFactory
    {
        /// <summary>
        /// Gets or sets the heartbeat configuration.
        /// </summary>
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the heartbeat device in the ONIX breakout board.")]
        [Category(ConfigurationCategory)]
        public ConfigureHeartbeat Heartbeat { get; set; } = new();

        /// <summary>
        /// Gets or sets the breakout board's analog IO configuration.
        /// </summary>
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the analog IO device in the ONIX breakout board.")]
        [Category(ConfigurationCategory)]
        public ConfigureBreakoutAnalogIO AnalogIO { get; set; } = new();

        /// <summary>
        /// Gets or sets the breakout board's digital IO configuration.
        /// </summary>
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the digital IO device in the ONIX breakout board.")]
        [Category(ConfigurationCategory)]
        public ConfigureBreakoutDigitalIO DigitalIO { get; set; } = new();

        /// <summary>
        /// Gets or sets the hardware memory monitor configuration.
        /// </summary>
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the memory monitor device in the ONIX breakout board.")]
        [Category(ConfigurationCategory)]
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
