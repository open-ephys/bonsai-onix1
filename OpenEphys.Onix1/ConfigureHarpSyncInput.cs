using System;
using System.ComponentModel;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures the ONIX breakout board's <see href="https://harp-tech.org/">Harp</see>
    /// sync input.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This configuration operator can be linked to a data IO operator, such as <see
    /// cref="HarpSyncInputData"/>, using a shared <c>DeviceName</c>.
    /// </para>
    /// <para>
    /// Harp is a standard for asynchronous real-time data acquisition and experimental
    /// control in neuroscience. It includes a clock synchronization protocol which allows
    /// Harp devices to be connected to a shared clock line and continuously self-synchronize
    /// their clocks to a precision of tens of microseconds. This means that all experimental
    /// events are timestamped on the same clock and no post-hoc alignment of timing is necessary.
    /// </para>
    /// <para>
    /// The Harp clock signal is transmitted over a serial line every second.
    /// Every time the Harp sync input device in the ONIX breakout board detects a full Harp
    /// synchronization packet, a new data frame is emitted pairing the current value of the
    /// Harp clock with the local ONIX acquisition clock.
    /// </para>
    /// <para>
    /// Logging the sequence of all Harp synchronization packets can greatly facilitate post-hoc
    /// analysis and interpretation of timing signals. For more information see
    /// <see href="https://harp-tech.org/"/>.
    /// </para>
    /// </remarks>
    [Description("Configures a ONIX breakout board Harp sync input.")]
    public class ConfigureHarpSyncInput : SingleDeviceFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureHarpSyncInput"/> class.
        /// </summary>
        public ConfigureHarpSyncInput()
            : base(typeof(HarpSyncInput))
        {
            DeviceAddress = 12;
        }

        /// <summary>
        /// Gets or sets a value specifying whether the Harp sync input device is enabled.
        /// </summary>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the Harp sync input device is enabled.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Gets or sets a value specifying the physical Harp clock input source.
        /// </summary>
        /// <remarks>
        /// In standard ONIX breakout boards, the Harp mini-jack connector on the side of the
        /// breakout is configured to receive Harp clock synchronization signals.
        /// 
        /// In early access versions of the ONIX breakout board, the Harp mini-jack connector is
        /// configured for output only, so a special adapter is needed to transmit the
        /// Harp clock synchronization signal to the breakout clock input zero.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies the physical Harp clock input source.")]
        public HarpSyncSource Source { get; set; } = HarpSyncSource.Breakout;

        /// <summary>
        /// Configures a ONIX breakout board Harp sync input device.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> instance
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that hold configuration actions.</param>
        /// <returns>
        /// The original sequence modified by adding additional configuration actions required to configure
        /// a ONIX breakout board Harp sync input device.
        /// </returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureAndLatchDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);
                device.WriteRegister(HarpSyncInput.ENABLE, Enable ? 1u : 0);
                device.WriteRegister(HarpSyncInput.SOURCE, (uint)Source);
                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }
    }

    static class HarpSyncInput
    {
        public const int ID = 30;
        public const uint MinimumVersion = 2;

        // managed registers
        public const uint ENABLE = 0x0; // Enable or disable the data stream
        public const uint SOURCE = 0x1; // Select the clock input source

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(HarpSyncInput))
            {
            }
        }
    }

    /// <summary>
    /// Specifies the physical Harp clock input source.
    /// </summary>
    public enum HarpSyncSource
    {
        /// <summary>
        /// Specifies the Harp 3.5-mm audio jack connector on the side of the ONIX breakout board.
        /// </summary>
        Breakout = 0,

        /// <summary>
        /// Specifies SMA clock input 0 on the ONIX breakout board.
        /// </summary>
        /// <remarks>
        /// In early access versions of the ONIX breakout board, Harp 3.5-mm audio jack connector was
        /// configured for output only, so a special adapter was needed to transmit the Harp clock
        /// synchronization signal to the breakout clock input zero.
        /// </remarks>
        ClockAdapter = 1
    }
}
