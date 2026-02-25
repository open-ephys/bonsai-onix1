using System;
using System.ComponentModel;
using Bonsai;
using Bonsai.Reactive;

namespace OpenEphys.Onix1
{
    /// <inheritdoc cref = "ConfigureDigitalIO"/>
    [Obsolete("Use ConfigureDigitalIO instead. This operator will be removed in version 1.0.0v")]
    public class ConfigureBreakoutDigitalIO : ConfigureDigitalIO { }

    /// <summary>
    /// Configures the ONIX breakout board's digital inputs and outputs.
    /// </summary>
    /// <remarks>
    /// This configuration operator can be linked to data IO operators, such as <see
    /// cref="DigitalInput"/> and <see cref="DigitalOutput"/>, using a shared
    /// <c>DeviceName</c>.
    /// </remarks>
    [Description("Configures the ONIX breakout board's digital inputs and outputs.")]
    public class ConfigureDigitalIO : SingleDeviceFactory
    {

        double? sampleRate = null;
        double deadTime = 0;

        /// <summary>
        /// Initialize a new instance of the <see cref="ConfigureDigitalIO"/> class.
        /// </summary>
        public ConfigureDigitalIO()
            : base(typeof(DigitalIO))
        {
            DeviceAddress = 7;
        }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, <see cref="DigitalInput"/> will produce data. If set to false, <see
        /// cref="DigitalInput"/> will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the digital IO device is enabled.")]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Gets or sets the dead time, in μsec, between event detections when in asynchronous
        /// sampling mode.
        /// </summary>
        /// <remarks>
        /// This property is useful for filtering "glitches" due to rapidly changing port states, for instance
        /// from switch bounce. This property has no effect when <see cref="SampleRate"/> is set and periodic
        /// sampling mode is active.
        /// </remarks>
        [Range(0, 1e6)]
        [Category(ConfigurationCategory)]
        [Description("Specifies dead time, in μsec, between digital event detections when in asynchronous " +
            "sampling mode")]
        public double DeadTime
        {
            get => deadTime;
            set => deadTime = (value >= 0 && value <= 1e6)
            ? value
            : throw new ArgumentOutOfRangeException(nameof(DeadTime), value,
                $"{nameof(DeadTime)} must be between 0 and 1e6 μsec.");
        }

        /// <summary>
        /// Gets or sets the optional sample rate, in Hz, of the digital inputs.
        /// </summary>
        /// <remarks>
        /// If specified, digital inputs will be sampled periodically at the specified rate in Hz. If not
        /// specified, digital input data will be produced asynchronously upon changes in digital input state
        /// as long as the changes do not occur in the <see cref="DeadTime"/> with respect to the last
        /// detected digital event.
        /// </remarks>
        [Range(10, 1e6)]
        [Category(ConfigurationCategory)]
        [Description("Specifies the optional sample rate, in Hz, of digital inputs. If not specified, digital " +
            "data will be produced asynchronously upon changes in digital input state.")]
        public double? SampleRate { 
            get => sampleRate; 
            set => sampleRate = (value >= 10 && value <= 1e6) | value is null
            ? value
            : throw new ArgumentOutOfRangeException(nameof(SampleRate), value,
                $"{nameof(SampleRate)} must be between 10 Hz and 1 MHz.");
        }

        /// <summary>
        /// Configures the digital input and output device in the ONIX breakout board.
        /// </summary>
        /// <remarks>
        /// This will schedule digital IO hardware configuration actions that can be applied by a <see
        /// cref="StartAcquisition"/> object prior to data collection.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that hold configuration
        /// actions.</param>
        /// <returns>
        /// The original sequence modified by adding additional configuration actions required to configure a
        /// digital IO device.
        /// </returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            var dt = deadTime;
            var sr = SampleRate;
            return source.ConfigureAndLatchDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);
                device.WriteRegister(DigitalIO.ENABLE, Enable ? 1u : 0);

                var baseFreqHz = device.ReadRegister(DigitalIO.BASE_FREQ_HZ);
                device.WriteRegister(DigitalIO.DEAD_TICKS, (uint)(dt / 1e6 * baseFreqHz));

                if (sr is not null)
                {
                    var periodTicks = (uint)(baseFreqHz / sr) - 2; // NB: -2 results from known issue in version 2 of DigitalIO device
                    device.WriteRegister(DigitalIO.SAMPLE_PERIOD, periodTicks);
                } else
                {
                    device.WriteRegister(DigitalIO.SAMPLE_PERIOD, 0);
                }

                return DeviceManager.RegisterDevice(deviceName, device, DeviceType);
            });
        }
    }

    static class DigitalIO
    {
        public const int ID = 18;
        public const uint MinimumVersion = 2;

        // managed registers
        public const uint ENABLE = 0x0;
        public const uint BASE_FREQ_HZ = 0x5;
        public const uint DEAD_TICKS = 0x6;
        public const uint SAMPLE_PERIOD = 0x7;

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(DigitalIO))
            {
            }
        }
    }

    /// <summary>
    /// Specifies the state of the ONIX breakout board's digital input pins.
    /// </summary>
    [Flags]
    public enum DigitalPortState : byte
    {
        /// <summary>
        /// Specifies that pin 0 is high.
        /// </summary>
        Pin0 = 0x1,
        /// <summary>
        /// Specifies that pin 1 is high.
        /// </summary>
        Pin1 = 0x2,
        /// <summary>
        /// Specifies that pin 2 is high.
        /// </summary>
        Pin2 = 0x4,
        /// <summary>
        /// Specifies that pin 3 is high.
        /// </summary>
        Pin3 = 0x8,
        /// <summary>
        /// Specifies that pin 4 is high.
        /// </summary>
        Pin4 = 0x10,
        /// <summary>
        /// Specifies that pin 5 is high.
        /// </summary>
        Pin5 = 0x20,
        /// <summary>
        /// Specifies that pin 6 is high.
        /// </summary>
        Pin6 = 0x40,
        /// <summary>
        /// Specifies that pin 7 is high.
        /// </summary>
        Pin7 = 0x80,
    }

    /// <summary>
    /// Specifies the state of the ONIX breakout board's switches and buttons.
    /// </summary>
    [Flags]
    public enum BreakoutButtonState : ushort
    {
        /// <summary>
        /// Specifies that no buttons or switches are active.
        /// </summary>
        None = 0x0,
        /// <summary>
        /// Specifies that the ☾ key is depressed.
        /// </summary>
        Moon = 0x1,
        /// <summary>
        /// Specifies that the △ key is depressed.
        /// </summary>
        Triangle = 0x2,
        /// <summary>
        /// Specifies that the × key is depressed.
        /// </summary>
        X = 0x4,
        /// <summary>
        /// Specifies that the ✓ key is depressed.
        /// </summary>
        Check = 0x8,
        /// <summary>
        /// Specifies that the ◯ key is depressed.
        /// </summary>
        Circle = 0x10,
        /// <summary>
        /// Specifies that the □ key is depressed.
        /// </summary>
        Square = 0x20,
        /// <summary>
        /// Specifies that reserved bit 0 is high.
        /// </summary>
        Reserved0 = 0x40,
        /// <summary>
        /// Specifies that reserved bit 1 is high.
        /// </summary>
        Reserved1 = 0x80,
        /// <summary>
        /// Specifies that port D power switch is set to on.
        /// </summary>
        PortDOn = 0x100,
        /// <summary>
        /// Specifies that port C power switch is set to on.
        /// </summary>
        PortCOn = 0x200,
        /// <summary>
        /// Specifies that port B power switch is set to on.
        /// </summary>
        PortBOn = 0x400,
        /// <summary>
        /// Specifies that port A power switch is set to on.
        /// </summary>
        PortAOn = 0x800,
    }
}
