using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures the ONIX breakout board's output clock.
    /// </summary>
    /// <remarks>
    /// The output clock provides physical, 3.3V logic level, 50 Ohm output impedance, frequency divided copy
    /// of the <see cref="ContextTask.AcquisitionClockHz">Acquisition Clock</see> that is used to generate
    /// <see cref="DataFrame.Clock"/> values for all data streams within an ONIX system. This clock runs a
    /// user defined rate, duty cycle, and start delay. It can be used to drive external hardware or can be
    /// logged by external recording systems for post-hoc synchronization with ONIX data.
    /// </remarks>
    [Description("Configures the ONIX breakout board's output clock.")]
    public class ConfigureBreakoutOutputClock : SingleDeviceFactory
    {
        readonly BehaviorSubject<bool> gate = new(false);
        double frequencyHz = 1e6;
        double dutyCycle = 50;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureBreakoutOutputClock"/> class.
        /// </summary>
        public ConfigureBreakoutOutputClock()
            : base(typeof(BreakoutOutputClock))
        {
            DeviceAddress = 5;
        }

        /// <summary>
        /// Gets or sets a value specifying if the output clock is active.
        /// </summary>
        /// <remarks>
        /// If set to true, the clock output will connected to the clock output line. If set to false, the
        /// clock output line will be held low. This value can be toggled in real time to gate acquisition of
        /// external hardware.
        /// </remarks>
        [Category(AcquisitionCategory)]
        [Description("Clock gate control signal.")]
        public bool ClockGate
        {
            get => gate.Value;
            set => gate.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the output clock frequency in Hz.
        /// </summary>
        /// <remarks>
        /// Valid values are between 0.1 Hz and 10 MHz. The output clock high and low times must each be an
        /// integer multiple of the <see cref="ContextTask.AcquisitionClockHz">Acquisition Clock</see>
        /// frequency. Therefore, the true clock frequency will be set to a value that is as close as possible
        /// to the requested setting while respecting this constraint. The value as actualized in hardware is
        /// reported by <see cref="BreakoutOutputClockData"/>.
        /// </remarks>
        [Range(0.1, 10e6)]
        [Category(ConfigurationCategory)]
        [Description("Frequency of the output clock (Hz).")]
        public double Frequency
        {
            get => frequencyHz;
            set => frequencyHz = value >= 0.1 && value <= 10e6
                    ? value
                    : throw new ArgumentOutOfRangeException(nameof(Frequency), value,
                        $"{nameof(Frequency)} must be between 0.1 Hz and 10 MHz.");
        }

        /// <summary>
        /// Gets or sets the output clock duty cycle in percent.
        /// </summary>
        /// <remarks>
        /// Valid values are between 10% and 90%. The output clock high and low times must each be an integer
        /// multiple of the <see cref="ContextTask.AcquisitionClockHz">Acquisition Clock</see> frequency.
        /// Therefore, the true duty cycle will be set to a value that is as close as possible to the
        /// requested setting while respecting this constraint. The value as actualized in hardware is
        /// reported by <see cref="BreakoutOutputClockData"/>.
        /// </remarks>
        [Range(10, 90)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Category(ConfigurationCategory)]
        [Precision(1, 1)]
        [Description("Duty cycle of output clock (%).")]
        public double DutyCycle
        {
            get => dutyCycle;
            set => dutyCycle = value >= 10 && value <= 90
                    ? value
                    : throw new ArgumentOutOfRangeException(nameof(DutyCycle), value,
                       $"{nameof(DutyCycle)} must be between 10% and 90%.");
        }

        /// <summary>
        /// Gets or sets the delay following acquisition commencement before the clock becomes active in
        /// seconds.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Valid values are between 0 and and 3600 seconds. Setting to a value greater than 0 can be useful
        /// for ensuring data sources that are driven by the output clock start significantly after ONIX has
        /// begun aquisition for the purposes of ordering acquisition start times.
        /// </para>
        /// <para>
        /// The delay must each be an integer multiple of the <see
        /// cref="ContextTask.AcquisitionClockHz">Acquisition Clock</see> frequency. Therefore, the true delay
        /// cycle will be set to a value that is as close as possible to the requested setting while
        /// respecting this constraint. The value as actualized in hardware is reported by <see
        /// cref="BreakoutOutputClockData"/>.
        /// </para>
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies a delay following acquisition start before the clock becomes active (sec).")]
        [Range(0, 3600)]
        public double Delay { get; set; } = 0;

        /// <summary>
        /// Configures a clock output.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/>
        /// instance prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that holds configuration
        /// actions.</param>
        /// <returns>The original sequence modified by adding additional configuration actions required to
        /// configure a clock output device./></returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var clkFreqHz = Frequency;
            var dutyCycle = DutyCycle;
            var delaySeconds = Delay;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;

            // TODO: Dispose action that turns clock off
            return source.ConfigureDevice((context, observer) =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);

                var baseFreqHz = device.ReadRegister(BreakoutOutputClock.BASE_FREQ_HZ);
                var periodTicks = (uint)(baseFreqHz / clkFreqHz);
                var h = (uint)(periodTicks * (dutyCycle / 100));
                var l = periodTicks - h;
                var delayTicks = (uint)(delaySeconds * baseFreqHz);
                device.WriteRegister(BreakoutOutputClock.HIGH_CYCLES, h);
                device.WriteRegister(BreakoutOutputClock.LOW_CYCLES, l);
                device.WriteRegister(BreakoutOutputClock.DELAY_CYCLES, delayTicks);

                var deviceInfo = new BreakoutOutputClockDeviceInfo(device, DeviceType,
                    new(baseFreqHz / (h + l), 100 * h / periodTicks, delaySeconds, h + l, h, l, delayTicks));

                var shutdown = Disposable.Create(() =>
                {
                    device.WriteRegister(BreakoutOutputClock.CLOCK_GATE, 0u);
                });

                return new CompositeDisposable(
                    DeviceManager.RegisterDevice(deviceName, deviceInfo),
                    gate.SubscribeSafe(observer, value => device.WriteRegister(BreakoutOutputClock.CLOCK_GATE, value ? 1u : 0u)),
                    shutdown
                );
            });
        }
    }

    static class BreakoutOutputClock
    {
        public const int ID = 20;

        public const uint NULL = 0; // No command
        public const uint CLOCK_GATE = 1;  // Output enable. Bit 0 = 0 is disabled, Bit 0 = 1 is enabled.
        public const uint HIGH_CYCLES = 2;  // Number of input clock cycles output clock should be high. Valid values are 1 or greater.
        public const uint LOW_CYCLES = 3; // Number of input clock cycles output clock should be low. Valid values are 1 or greater.
        public const uint DELAY_CYCLES = 4; // Number of input clock cycles output clock should be low. Valid values are 1 or greater.
        public const uint GATE_RUN = 5; // Number of input clock cycles output clock should be low. Valid values are 1 or greater.
        public const uint BASE_FREQ_HZ = 6; // Number of input clock cycles output clock should be low. Valid values are 1 or greater.

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(BreakoutOutputClock))
            {
            }
        }
    }

    /// <summary>
    /// Hardware-verified output clock parameters.
    /// </summary>
    /// <param name="Frequency">Gets the exact clock frequency as actualized by the clock synthesizer in
    /// Hz.</param>
    /// <param name="DutyCycle">Gets the exact clock duty cycle as actualized by the clock synthesizer
    /// in percent.</param>
    /// <param name="Delay">Gets the exact clock delay as actualized by the clock synthesizer in
    /// seconds.</param>
    /// <param name="PeriodTicks">Gets the exact clock period as actualized by the clock synthesizer in units
    /// of ticks of the of the <see cref="ContextTask.AcquisitionClockHz">Acquisition Clock</see>.</param>
    /// <param name="HighTicks">Gets the exact clock high time per period as actualized by the clock
    /// synthesizer in units of ticks of the of the <see cref="ContextTask.AcquisitionClockHz">Acquisition
    /// Clock</see>.</param>
    /// <param name="LowTicks">Gets the exact clock low time per period as actualized by the clock synthesizer
    /// in units of ticks of the of the <see cref="ContextTask.AcquisitionClockHz">Acquisition
    /// Clock</see>.</param>
    /// <param name="DelayTicks">Gets the exact clock delay as actualized by the clock synthesizer in units of
    /// ticks of the of the <see cref="ContextTask.AcquisitionClockHz">Acquisition Clock</see>.</param>
    public readonly record struct BreakoutOutputClockParameters(double Frequency,
        double DutyCycle, double Delay, uint PeriodTicks, uint HighTicks, uint LowTicks, uint DelayTicks);

    class BreakoutOutputClockDeviceInfo : DeviceInfo
    {
        public BreakoutOutputClockDeviceInfo(DeviceContext device, Type deviceType, BreakoutOutputClockParameters parameters)
            : base(device, deviceType)
        {
            Parameters = parameters;
        }

        public BreakoutOutputClockParameters Parameters { get; }
    }
}
