using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Bonsai;

namespace OpenEphys.Onix
{
    public class ConfigureHeartbeat : Sink<ContextTask>
    {
        readonly BehaviorSubject<uint> beatsPerSecond = new BehaviorSubject<uint>(10);

        public uint DeviceIndex { get; set; }

        [Range(1, 10e6)]
        [Category("Configuration")]
        [Description("Rate at which beats are produced.")]
        public uint BeatsPerSecond
        {
            get { return beatsPerSecond.Value; }
            set { beatsPerSecond.OnNext(value); }
        }

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceIndex = DeviceIndex;
            return source.ConfigureDevice(context =>
            {
                if (!context.DeviceTable.TryGetValue(deviceIndex, out oni.Device device))
                {
                    throw new InvalidOperationException("Selected device index is invalid.");
                }

                // Enable only takes effect after context reset
                context.WriteRegister(deviceIndex, Register.ENABLE, 1);
                var subscription = beatsPerSecond.Subscribe(newValue =>
                {
                    var clkHz = context.ReadRegister(deviceIndex, Register.CLK_HZ);
                    context.WriteRegister(deviceIndex, Register.CLK_DIV, clkHz / newValue);
                });
                return Disposable.Create(() => subscription.Dispose());
            });
        }

        private static class Register
        {
            public const uint ENABLE = 0;  // Enable the heartbeat
            public const uint CLK_DIV = 1;  // Heartbeat clock divider ratio. Default results in 10 Hz heartbeat. Values less than CLK_HZ / 10e6 Hz will result in 1kHz.
            public const uint CLK_HZ = 2; // The frequency parameter, CLK_HZ, used in the calculation of CLK_DIV
        }
    }
}
