﻿using System;
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
    /// The output clock provides a 3.3V logic level, 50 Ohm output impedance, frequency divided copy
    /// of the <see cref="ContextTask.AcquisitionClockHz">Acquisition Clock</see> that is used to generate
    /// <see cref="DataFrame.Clock"/> values for all data streams within an ONIX system. This clock runs at a
    /// user defined rate, duty cycle, and start delay. It can be used to drive external hardware or can be
    /// logged by external recording systems for post-hoc synchronization with ONIX data.
    /// </remarks>
    [Description("Configures the ONIX breakout board's output clock.")]
    public class ConfigureOutputClock : SingleDeviceFactory
    {
        readonly BehaviorSubject<bool> gate = new(false);
        double frequencyHz = 1e6;
        double dutyCycle = 50;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureOutputClock"/> class.
        /// </summary>
        public ConfigureOutputClock()
            : base(typeof(OutputClock))
        {
            DeviceAddress = 5;
        }

        /// <summary>
        /// Gets or sets a value specifying if the output clock is active.
        /// </summary>
        /// <remarks>
        /// If set to true, the physical clock output will be connected to the internal clock signal. If set
        /// to false, the clock output line will be held low. This value can be toggled in real time to gate
        /// acquisition of external hardware without resetting the internal clock.
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
        /// reported by <see cref="OutputClockHardwareParameters"/>.
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
        /// reported by <see cref="OutputClockHardwareParameters"/>.
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
        /// begun acquisition for the purposes of ordering acquisition start times.
        /// </para>
        /// <para>
        /// The delay must be an integer multiple of the <see
        /// cref="ContextTask.AcquisitionClockHz">Acquisition Clock</see> frequency. Therefore, the true delay
        /// cycle will be set to a value that is as close as possible to the requested setting while
        /// respecting this constraint. The value as actualized in hardware is reported by <see
        /// cref="OutputClockHardwareParameters"/>.
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

            return source.ConfigureDevice((context, observer) =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);

                // NB: In this library the hardware run state and the workflow operation are linked.
                // Therefore, the clock should never be active outside the running state
                device.WriteRegister(OutputClock.GATE_RUN, 0b1);

                var baseFreqHz = device.ReadRegister(OutputClock.BASE_FREQ_HZ);
                var periodTicks = (uint)(baseFreqHz / clkFreqHz);
                var h = (uint)(periodTicks * (dutyCycle / 100));
                var l = periodTicks - h;
                var delayTicks = (uint)(delaySeconds * baseFreqHz);
                device.WriteRegister(OutputClock.HIGH_CYCLES, h);
                device.WriteRegister(OutputClock.LOW_CYCLES, l);
                device.WriteRegister(OutputClock.DELAY_CYCLES, delayTicks);

                var deviceInfo = new OutputClockDeviceInfo(device, DeviceType,
                    new((double)baseFreqHz / periodTicks, 100.0 * h / periodTicks, delaySeconds, h + l, h, l, delayTicks));

                var shutdown = Disposable.Create(() =>
                {
                    device.WriteRegister(OutputClock.CLOCK_GATE, 0u);
                });

                return new CompositeDisposable(
                    DeviceManager.RegisterDevice(deviceName, deviceInfo),
                    gate.SubscribeSafe(observer, value => device.WriteRegister(OutputClock.CLOCK_GATE, value ? 1u : 0u)),
                    shutdown
                );
            });
        }
    }

    static class OutputClock
    {
        public const int ID = 20;
        public const uint MinimumVersion = 1;

        public const uint NULL = 0; // No command
        public const uint CLOCK_GATE = 1;  // Output gate. Bit 0 = 0 is disabled, Bit 0 = 1 is enabled.
        public const uint HIGH_CYCLES = 2;  // Number of input clock cycles output clock should be high. Valid values are 1 or greater.
        public const uint LOW_CYCLES = 3; // Number of input clock cycles output clock should be low. Valid values are 1 or greater.
        public const uint DELAY_CYCLES = 4; // Delay, in input clock cycles, following reset before clock becomes active.
        public const uint GATE_RUN = 5; // LSB sets the gate using run status. Bit 0 = 0: Clock runs whenever CLOCK_GATE(0) is 1.  Bit 0 = 1: Clock runs only when acquisition is in RUNNING state.
        public const uint BASE_FREQ_HZ = 6; // Frequency of the input clock in Hz.

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(OutputClock))
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
    /// of ticks of the <see cref="ContextTask.AcquisitionClockHz">Acquisition Clock</see>.</param>
    /// <param name="HighTicks">Gets the exact clock high time per period as actualized by the clock
    /// synthesizer in units of ticks of the <see cref="ContextTask.AcquisitionClockHz">Acquisition
    /// Clock</see>.</param>
    /// <param name="LowTicks">Gets the exact clock low time per period as actualized by the clock synthesizer
    /// in units of ticks of the <see cref="ContextTask.AcquisitionClockHz">Acquisition
    /// Clock</see>.</param>
    /// <param name="DelayTicks">Gets the exact clock delay as actualized by the clock synthesizer in units of
    /// ticks of the <see cref="ContextTask.AcquisitionClockHz">Acquisition Clock</see>.</param>
    public readonly record struct OutputClockParameters(double Frequency,
        double DutyCycle, double Delay, uint PeriodTicks, uint HighTicks, uint LowTicks, uint DelayTicks);

    class OutputClockDeviceInfo : DeviceInfo
    {
        public OutputClockDeviceInfo(DeviceContext device, Type deviceType, OutputClockParameters parameters)
            : base(device, deviceType)
        {
            Parameters = parameters;
        }

        public OutputClockParameters Parameters { get; }
    }
}
