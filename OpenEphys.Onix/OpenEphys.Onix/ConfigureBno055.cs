using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace OpenEphys.Onix
{
    public class ConfigureBno055 : SingleDeviceFactory
    {
        public ConfigureBno055()
            : base(typeof(Bno055))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the BNO055 device is enabled.")]
        public bool Enable { get; set; } = true;

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDevice(deviceAddress, Bno055.ID);
                context.WriteRegister(deviceAddress, Bno055.ENABLE, 1);

                var deviceInfo = new DeviceInfo(context, DeviceType, deviceAddress);
                var disposable = DeviceManager.RegisterDevice(deviceName, deviceInfo);
                return disposable;
            });
        }
    }

    static class Bno055
    {
        public const int ID = 9;

        public const uint ENABLE = 0x10000;  // Enable the heartbeat

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Bno055))
            {
            }
        }
    }
}
