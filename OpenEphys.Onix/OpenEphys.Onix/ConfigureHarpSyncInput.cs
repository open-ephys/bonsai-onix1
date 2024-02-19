using System;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    public class ConfigureHarpSyncInput : SingleDeviceFactory
    {
        public ConfigureHarpSyncInput()
            : base(typeof(HarpSyncInput))
        {
            DeviceAddress = 12;
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the Harp sync input device is enabled.")]
        public bool Enable { get; set; } = true;

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, HarpSyncInput.ID);
                device.WriteRegister(HarpSyncInput.ENABLE, Enable ? 1u : 0);
                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }
    }

    static class HarpSyncInput
    {
        public const int ID = 30;

        // managed registers
        public const uint ENABLE = 0x0; // Enable or disable the data stream

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(HarpSyncInput))
            {
            }
        }
    }
}
