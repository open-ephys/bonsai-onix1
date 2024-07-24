using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Bonsai;

namespace OpenEphys.Onix
{
    public class ConfigureLoadTester : SingleDeviceFactory
    {
        readonly BehaviorSubject<uint> frameHz = new(1000);

        public ConfigureLoadTester()
            : base(typeof(LoadTester))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Number of repetitions of the 16-bit unsigned integer 42 sent with each read-frame.")]
        [Range(0, 10e6)]
        public uint ReceivedWords { get; set; }

        [Category(ConfigurationCategory)]
        [Description("Number of repetitions of the 32-bit integer 42 sent with each write frame.")]
        [Range(0, 10e6)]
        public uint TransmittedWords { get; set; }

        [Category(AcquisitionCategory)]
        [Description("Specifies the rate at which frames are produced (Hz).")]
        public uint FramesPerSecond
        {
            get { return frameHz.Value; }
            set { frameHz.OnNext(value); }
        }

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            var receivedWords = ReceivedWords;
            var transmittedWords = TransmittedWords;
            return source.ConfigureDevice((context, observer) =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);
                device.WriteRegister(LoadTester.ENABLE, 1);

                var clk_hz = device.ReadRegister(LoadTester.CLK_HZ);
                // Assumes 8-byte timer
                uint ValidSize()
                {
                    var clk_div = device.ReadRegister(LoadTester.CLK_DIV);
                    return clk_div - 4 - 10; // -10 is overhead hack
                }

                var max_size = ValidSize();
                var bounded = receivedWords > max_size ? max_size : receivedWords;
                device.WriteRegister(LoadTester.DT0H16_WORDS, bounded);

                var writeArray = Enumerable.Repeat((uint)42, (int)(transmittedWords + 2)).ToArray();
                device.WriteRegister(LoadTester.HTOD32_WORDS, transmittedWords);
                var frameHzSubscription = frameHz.SubscribeSafe(observer, newValue =>
                {
                    device.WriteRegister(LoadTester.CLK_DIV, clk_hz / newValue);
                    var max_size = ValidSize();
                    if (receivedWords > max_size)
                    {
                        receivedWords = max_size;
                    }
                });

                return new CompositeDisposable(
                    DeviceManager.RegisterDevice(deviceName, device, DeviceType),
                    frameHzSubscription
                );
            });
        }
    }

    static class LoadTester
    {
        public const int ID = 27;

        public const uint ENABLE = 0;
        public const uint CLK_DIV = 1;      // Heartbeat clock divider ratio. Default results in 10 Hz heartbeat.
                                            // Values less than CLK_HZ / 10e6 Hz will result in 1kHz.
        public const uint CLK_HZ = 2;       // The frequency parameter, CLK_HZ, used in the calculation of CLK_DIV
        public const uint DT0H16_WORDS = 3; // Number of repetitions of 16-bit unsigned integer 42 sent with each frame. 
                                            // Note: max here depends of CLK_HZ and CLK_DIV. There needs to be enough clock
                                            // cycles to push the data at the requested CLK_HZ. Specifically,
                                            // CLK_HZ / CLK_DIV >= TX16_WORDS + 9. Going above this will result in 
                                            // decreased bandwidth as samples will be skipped.
        public const uint HTOD32_WORDS = 4; // Number of 32-bit words in a write-frame. All write frame data is ignored except
                                            // the first 64-bits, which are looped back into the device to host data frame for   
                                            // testing loop latency. This value must be at least 2.

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(LoadTester))
            {
            }
        }
    }
}
