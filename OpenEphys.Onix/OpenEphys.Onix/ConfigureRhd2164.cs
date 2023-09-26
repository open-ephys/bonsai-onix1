using System;
using System.ComponentModel;

namespace OpenEphys.Onix
{
    public class ConfigureRhd2164 : SingleDeviceFactory
    {
        public ConfigureRhd2164()
            : base(typeof(Rhd2164))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the RHD2164 device is enabled.")]
        public bool Enable { get; set; } = true;

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDevice(deviceAddress, Rhd2164.ID);
                context.WriteRegister(deviceAddress, Rhd2164.ENABLE, enable ? 1u : 0);

                var deviceInfo = new DeviceInfo(context, DeviceType, deviceAddress);
                var disposable = DeviceManager.RegisterDevice(deviceName, deviceInfo);
                return disposable;
            });
        }
    }

    static class Rhd2164
    {
        public const int ID = 3;

        public const uint ENABLE = 0x10000;  // Enable the heartbeat

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Rhd2164))
            {
            }
        }
    }
}
