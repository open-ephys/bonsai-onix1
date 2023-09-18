using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Bonsai;

namespace OpenEphys.Onix
{
    public class ConfigureHeartbeat : SingleDeviceFactory
    {
        readonly BehaviorSubject<uint> beatsPerSecond = new BehaviorSubject<uint>(10);

        public ConfigureHeartbeat()
            : base(typeof(Heartbeat))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the heartbeat device is enabled.")]
        public bool Enable { get; set; } = true;

        [Range(1, 10e6)]
        [Category(ConfigurationCategory)]
        [Description("Rate at which beats are produced.")]
        public uint BeatsPerSecond
        {
            get { return beatsPerSecond.Value; }
            set { beatsPerSecond.OnNext(value); }
        }

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDevice(deviceAddress, Heartbeat.ID);
                context.WriteRegister(deviceAddress, Heartbeat.ENABLE, 1);
                var subscription = beatsPerSecond.Subscribe(newValue =>
                {
                    var clkHz = context.ReadRegister(deviceAddress, Heartbeat.CLK_HZ);
                    context.WriteRegister(deviceAddress, Heartbeat.CLK_DIV, clkHz / newValue);
                });

                var deviceInfo = new DeviceInfo(context, DeviceType, deviceAddress);
                var disposable = DeviceManager.RegisterDevice(deviceName, deviceInfo);
                return new CompositeDisposable(
                    disposable,
                    subscription
                );
            });
        }
    }

    static class Heartbeat
    {
        public const int ID = 12;

        public const uint ENABLE = 0;  // Enable the heartbeat
        public const uint CLK_DIV = 1;  // Heartbeat clock divider ratio. Default results in 10 Hz heartbeat. Values less than CLK_HZ / 10e6 Hz will result in 1kHz.
        public const uint CLK_HZ = 2; // The frequency parameter, CLK_HZ, used in the calculation of CLK_DIV

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Heartbeat))
            {
            }
        }
    }
}
