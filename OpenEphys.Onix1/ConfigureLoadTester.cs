using System;
using System.ComponentModel;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a load tester device for measuring system performance.
    /// </summary>
    /// <remarks>
    /// This configuration operator can be linked to a data IO operator, such as <see cref="LoadTesterData"/>,
    /// using a shared <c>DeviceName</c>. The load tester device can be configured to produce and consume data
    /// at user-defined sizes and rates to stress test various communication links and measure closed-loop
    /// response latency using a high-resolution hardware timer.
    /// </remarks>
    [Description("Configures a load testing device.")]
    public class ConfigureLoadTester : SingleDeviceFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureLoadTester"/> class.
        /// </summary>
        public ConfigureLoadTester()
            : base(typeof(LoadTester))
        {
        }

        /// <summary>
        /// Gets or sets a value specifying whether the load testing device is enabled.
        /// </summary>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the load testing device is enabled.")]
        public bool Enable { get; set; } = false;

        /// <summary>
        /// Gets or sets the number of incrementing, unsigned 16-bit integers sent with each read frame.
        /// </summary>
        /// <remarks>
        /// These data are produced by the controller and are used to impose a load on the controller to host
        /// communication. These data can be used in downstream computational operations that model the
        /// computational load imposed by a closed-loop algorithm.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Number of incrementing, unsigned 16-bit integers sent with each read-frame.")]
        [Range(0, 10e6)]
        public uint ReceivedWords { get; set; }

        /// <summary>
        /// Gets or sets the number of repetitions of the 32-bit integer dummy words sent with each write
        /// frame.
        /// </summary>
        /// <remarks>
        /// These data are produced by the host and are used to impose a load on host to controller
        /// communication. They are discarded by the controller when they are received.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Number of repetitions of the 32-bit integer dummy words sent with each write frame.")]
        [Range(0, 10e6)]
        public uint TransmittedWords { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the rate at which frames are produced in Hz.
        /// </summary>
        [Category(ConfigurationCategory)]
        [Description("Specifies the rate at which frames are produced (Hz).")]
        public uint FramesPerSecond { get; set; }

        /// <summary>
        /// Configures a load testing device.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/> instance
        /// prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that hold configuration actions.</param>
        /// <returns>
        /// The original sequence modified by adding additional configuration actions required to configure
        /// a load testing device.
        /// </returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            var receivedWords = ReceivedWords;
            var transmittedWords = TransmittedWords;
            var framesPerSecond = FramesPerSecond;

            return source.ConfigureAndLatchDevice((context, observer) =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);
                device.WriteRegister(LoadTester.ENABLE, enable ? 1u : 0u);

                var clockHz = device.ReadRegister(LoadTester.CLK_HZ);

                const int OverheadCycles = 9; // 4 cycles to produce hub clock, and 5 state machine overhead per the datasheet

                var maxFramesPerSecond = clockHz / OverheadCycles;
                if (framesPerSecond > maxFramesPerSecond)
                {
                    throw new ArgumentOutOfRangeException(nameof(FramesPerSecond), $"{nameof(FramesPerSecond)} must be less than {maxFramesPerSecond}.");
                }

                device.WriteRegister(LoadTester.CLK_DIV, clockHz / framesPerSecond);

                var maxSize = device.ReadRegister(LoadTester.CLK_DIV) - OverheadCycles;

                if (receivedWords > maxSize)
                {
                    throw new ArgumentOutOfRangeException(nameof(ReceivedWords), 
                        $"{nameof(ReceivedWords)} must be less than {maxSize} for the requested frame rate of {framesPerSecond} Hz.");
                }

                device.WriteRegister(LoadTester.DT0H16_WORDS, receivedWords);
                device.WriteRegister(LoadTester.HTOD32_WORDS, transmittedWords);
                
                var deviceInfo = new LoadTesterDeviceInfo(context, DeviceType, deviceAddress, ReceivedWords, TransmittedWords);
                return DeviceManager.RegisterDevice(deviceName, deviceInfo);
            });
        }
    }

    static class LoadTester
    {
        public const int ID = 27;
        public const uint MinimumVersion = 2;

        public const uint ENABLE = 0;
        public const uint CLK_DIV = 1;
        public const uint CLK_HZ = 2;
        public const uint DT0H16_WORDS = 3;
        public const uint HTOD32_WORDS = 4;
        public const uint DTOH_START = 5;

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(LoadTester))
            {
            }
        }
    }
}
