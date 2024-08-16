using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using oni;

namespace OpenEphys.Onix1
{
    public class ConfigureRhs2116Dual : SingleDeviceFactory
    {

        public ConfigureRhs2116Dual()
            : base(typeof(Rhs2116Dual))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the RHS2116 device is enabled.")]
        public bool Enable { get; set; } = true;

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {

            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            var enable = Enable;
            return source.ConfigureDevice(context =>
            {
                var rhs2116A = context.GetDeviceContext(deviceAddress + Rhs2116Dual.Rhs2116AAddressOffset, typeof(Rhs2116));
                var rhs2116B = context.GetDeviceContext(deviceAddress + Rhs2116Dual.Rhs2116BAddressOffset, typeof(Rhs2116));

                // configure stuff or assume they come preconfigured ...
                // TODO: make helper class that will perform the same config on both headstages for every call
                rhs2116A.WriteRegister(Rhs2116.ENABLE, enable ? 1u : 0);
                rhs2116B.WriteRegister(Rhs2116.ENABLE, enable ? 1u : 0);

                var deviceInfo = new Rhs2116DualDeviceInfo(
                    context,
                    rhs2116A,
                    rhs2116B,
                    typeof(Rhs2116Dual),
                    deviceAddress);
                return DeviceManager.RegisterDevice(deviceName, deviceInfo);
            });
        }
    }
    class Rhs2116DualDeviceInfo : DeviceInfo
    {
        public Rhs2116DualDeviceInfo(
            ContextTask context,
            DeviceContext rhs2116A,
            DeviceContext rhs2116B,
            Type deviceType,
            uint deviceAddress)
            : base(context, deviceType, deviceAddress)
        {
            Rhs2116A = rhs2116A;
            Rhs2116B = rhs2116B;
        }

        public DeviceContext Rhs2116A { get; }

        public DeviceContext Rhs2116B { get; }
    }

    static class Rhs2116Dual
    {

        public const int Rhs2116AAddressOffset = 0;
        public const int Rhs2116BAddressOffset = 1;

        public const int ChannelsPerChip = 16;
        public const int TotalChannels = 32;

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Rhs2116Dual))
            {
            }
        }
    }

}
