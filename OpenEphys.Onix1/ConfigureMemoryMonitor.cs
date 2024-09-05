using System;
using System.ComponentModel;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a hardware memory monitor.
    /// </summary>
    /// <remarks>
    /// This configuration operator can be linked to a data IO operator, such as <see
    /// cref="MemoryMonitorData"/>, using a shared <see cref="SingleDeviceFactory.DeviceName"/>.The memory
    /// monitor produces periodic snapshots of the system's first in, first out (FIFO) data buffer. This can
    /// be useful for:
    /// <list type="bullet">
    /// <item>
    /// <description>Ensuring that data is being read by the host PC quickly enough to prevent real-time
    /// delays or overflows. In the case that the PC is not keeping up with data collection, FIFO memory use
    /// will increase monotonically.</description>
    /// </item>
    /// <item>
    /// <description>Tuning the value of <see cref="StartAcquisition.ReadSize"/> to optimize real-time
    /// performance. For optimal real-time performance, <see cref="StartAcquisition.ReadSize"/> should be as
    /// small as possible and the FIFO should be bypassed (memory usage should remain at 0). However, these
    /// requirements are in conflict. The memory monitor provides a way to find the minimal value of value of
    /// <see cref="StartAcquisition.ReadSize"/> that does not result in excessive FIFO data buffering. This
    /// tradeoff will depend on the bandwidth of data being acquired, the performance of the host PC, and
    /// downstream real-time processing.</description>
    /// </item>
    /// </list>
    /// </remarks>
    [Description("Configures a hardware memory monitor.")]
    public class ConfigureMemoryMonitor : SingleDeviceFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureMemoryMonitor"/> class.
        /// </summary>
        public ConfigureMemoryMonitor()
            : base(typeof(MemoryMonitor))
        {
            DeviceAddress = 10;
        }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, <see cref="MemoryMonitorData"/> will produce data. If set to false, <see cref="MemoryMonitorData"/> will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the memory monitor device is enabled.")]
        public bool Enable { get; set; } = false;

        /// <summary>
        /// Gets or sets the frequency at which memory use is recorded in Hz.
        /// </summary>
        [Range(1, 1000)]
        [Category(ConfigurationCategory)]
        [Description("Frequency at which memory use is recorded (Hz).")]
        public uint SamplesPerSecond { get; set; } = 10;

        /// <summary>
        /// Configures a memory monitor device.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> instance
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that holds configuration actions.</param>
        /// <returns>The original sequence modified by adding additional configuration actions required to configure a memory monitor device.</returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            var samplesPerSecond = SamplesPerSecond;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);
                device.WriteRegister(MemoryMonitor.ENABLE, enable ? 1u : 0u);
                device.WriteRegister(MemoryMonitor.CLK_DIV, device.ReadRegister(MemoryMonitor.CLK_HZ) / samplesPerSecond);
                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }
    }

    static class MemoryMonitor
    {
        public const int ID = 28;

        public const uint ENABLE = 0; // Enable the monitor
        public const uint CLK_DIV = 1; // Sample clock divider ratio. Values less than CLK_HZ / 10e6 Hz will result in 1kHz.
        public const uint CLK_HZ = 2; // The frequency parameter, CLK_HZ, used in the calculation of CLK_DIV
        public const uint TOTAL_MEM = 3; // Total available memory in 32-bit words

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(MemoryMonitor))
            {
            }
        }
    }
}
