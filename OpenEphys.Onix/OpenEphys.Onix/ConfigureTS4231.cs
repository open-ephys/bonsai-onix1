using System;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    public class ConfigureTS4231 : SingleDeviceFactory
    {
        public ConfigureTS4231()
            : base(typeof(TS4231))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the TS4231 device is enabled.")]
        public bool Enable { get; set; } = true;

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDevice(deviceAddress, TS4231.ID);
                context.WriteRegister(deviceAddress, TS4231.ENABLE, Enable ? 1u : 0);

                var deviceInfo = new DeviceInfo(context, DeviceType, deviceAddress);
                var disposable = DeviceManager.RegisterDevice(deviceName, deviceInfo);
                return disposable;
            });
        }
    }

    static class TS4231
    {
        public const int ID = 25;

        // managed registers
        public const uint ENABLE = 0x0; // Enable or disable the data output stream

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(TS4231))
            {
            }
        }
    }
}
