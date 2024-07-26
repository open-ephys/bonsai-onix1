using System;
using System.ComponentModel;
using Bonsai;

namespace OpenEphys.Onix
{
    public class ConfigureMemoryMonitor : SingleDeviceFactory
    {
        public ConfigureMemoryMonitor()
            : base(typeof(MemoryMonitor))
        {
            DeviceAddress = 10;
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the monitor device is enabled.")]
        public bool Enable { get; set; } = false;

        [Range(1, 1000)]
        [Category(ConfigurationCategory)]
        [Description("Frequency at which hardware memory use is recorded (Hz).")]
        public uint SamplesPerSecond { get; set; } = 10;

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
