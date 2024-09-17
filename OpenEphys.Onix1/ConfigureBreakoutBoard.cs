using System.Collections.Generic;
using System.ComponentModel;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures an ONIX breakout board.
    /// </summary>
    /// <remarks>
    /// The ONIX breakout board is a general purpose interface for neural data acquisition. It provides the
    /// following features on the headstage:
    /// <list type="bullet">
    /// <item><description>2x general purpose input ports for headstage, miniscopes, etc.</description></item>
    /// <item><description>12x configurable ±10V analog input/output channels sampled/updated at 100 kHz per
    /// channel.</description></item>
    /// <item><description>8x digital inputs</description></item>
    /// <item><description>8x digital outputs</description></item>
    /// <item><description>Hardware time-stamped buttons for manual event logging</description></item>
    /// <item><description>Indicator LEDs with dark mode for light-sensitive applications.</description></item>
    /// </list>
    /// </remarks>
    [Description("Configures an ONIX breakout board.")]
    public class ConfigureBreakoutBoard : MultiDeviceFactory
    {
        /// <summary>
        /// Gets or sets the heartbeat configuration.
        /// </summary>
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the heartbeat device in the ONIX breakout board.")]
        [Category(DevicesCategory)]
        public ConfigureHeartbeat Heartbeat { get; set; } = new();

        /// <summary>
        /// Gets or sets the breakout board's analog IO configuration.
        /// </summary>
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the analog IO device in the ONIX breakout board.")]
        [Category(DevicesCategory)]
        public ConfigureBreakoutAnalogIO AnalogIO { get; set; } = new();

        /// <summary>
        /// Gets or sets the breakout board's digital IO configuration.
        /// </summary>
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the digital IO device in the ONIX breakout board.")]
        [Category(DevicesCategory)]
        public ConfigureBreakoutDigitalIO DigitalIO { get; set; } = new();

        /// <summary>
        /// Gets or sets the breakout board's output clock configuration.
        /// </summary>
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the clock output in the ONIX breakout board.")]
        [Category(DevicesCategory)]
        public ConfigureOutputClock ClockOutput { get; set; } = new();

        /// <summary>
        /// Gets or sets the the Harp synchronization input configuration.
        /// </summary>
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the Harp synchronization input on the ONIX breakout board.")]
        [Category(DevicesCategory)]
        public ConfigureHarpSyncInput HarpInput { get; set; } = new();

        /// <summary>
        /// Gets or sets the hardware memory monitor configuration.
        /// </summary>
        [TypeConverter(typeof(SingleDeviceFactoryConverter))]
        [Description("Specifies the configuration for the memory monitor device in the ONIX breakout board.")]
        [Category(DevicesCategory)]
        public ConfigureMemoryMonitor MemoryMonitor { get; set; } = new();

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return Heartbeat;
            yield return AnalogIO;
            yield return DigitalIO;
            yield return ClockOutput;
            yield return HarpInput;
            yield return MemoryMonitor;
        }
    }
}
