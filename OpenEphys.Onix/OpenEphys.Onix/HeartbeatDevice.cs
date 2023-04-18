using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Bonsai;

namespace OpenEphys.Onix
{
    public class HeartbeatDevice : Combinator<ContextTask, ManagedFrame<ushort>>
    {
        readonly BehaviorSubject<uint> beatsPerSecond = new BehaviorSubject<uint>(0);

        public uint DeviceIndex { get; set; }

        [Range(1, 10e6)]
        [Category("Configuration")]
        [Description("Rate at which beats are produced.")]
        public uint BeatsPerSecond
        {
            get { return beatsPerSecond.Value; }
            set { beatsPerSecond.OnNext(value); }
        }

        public override IObservable<ManagedFrame<ushort>> Process(IObservable<ContextTask> source)
        {
            var deviceIndex = DeviceIndex;
            return source.SelectMany(context =>
            {
                if (!context.DeviceTable.TryGetValue(deviceIndex, out oni.Device device))
                {
                    throw new InvalidOperationException("Selected device index is invalid.");
                }

                var bps = BeatsPerSecond;

                // Enable only takes effect after context reset
                context.WriteRegister(deviceIndex, Register.ENABLE, 1);
                if (bps != 0)
                {
                    var clkHz = context.ReadRegister(deviceIndex, Register.CLK_HZ);
                    context.WriteRegister(deviceIndex, Register.CLK_DIV, clkHz / bps);
                }

                var subscription = beatsPerSecond.Subscribe(newValue =>
                {
                    var clkHz = context.ReadRegister(deviceIndex, Register.CLK_HZ);
                    context.WriteRegister(deviceIndex, Register.CLK_DIV, clkHz / newValue);
                });

                subscription.Dispose();
                return context.FrameReceived
                    .Where(frame => frame.DeviceAddress == device.Address)
                    .Select(frame => new ManagedFrame<ushort>(frame));
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
