using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Newtonsoft.Json.Linq;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a headstage-64 dual-channel optical stimulator.
    /// </summary>
    /// <remarks>
    /// This configuration operator can be linked to a data IO operator, such as <see
    /// cref="Headstage64OpticalStimulatorTrigger"/>, using a shared
    /// <c>DeviceName</c>.
    /// </remarks>
    [Description("Configures a headstage-64 dual-channel optical stimulator.")]
    public class ConfigureHeadstage64OpticalStimulator : SingleDeviceFactory
    {
        readonly BehaviorSubject<Headstage64OpticalStimulatorSequence> stimulusSequence = new(new Headstage64OpticalStimulatorSequence());

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureHeadstage64OpticalStimulator"/> class.
        /// </summary>
        public ConfigureHeadstage64OpticalStimulator()
            : base(typeof(Headstage64OpticalStimulator))
        {
        }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, then the optical stimulator circuit will respect triggers. If set to false, triggers will be ignored.
        /// </remarks>
        [Description("Specifies whether the optical stimulator will respect triggers.")]
        [Category(ConfigurationCategory)]
        public bool Enable { get; set; } = false;

        /// <summary>
        /// Gets or sets the stimulus sequence.
        /// </summary>
        [Description("Stimulus sequence to be applied by the optical stimulator.")]
        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(GenericPropertyConverter))]
        public Headstage64OpticalStimulatorSequence StimulusSequence { get; set; } = new();

        /// <summary>
        /// Configure a headstage-64 dual-channel optical stimulator.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/>
        /// instance prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that holds configuration
        /// actions.</param>
        /// <returns>The original sequence modified by adding additional configuration actions required to
        /// configure a headstage-64 dual-channel optical stimulator.</returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice((context, observer) =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);
                device.WriteRegister(Headstage64OpticalStimulator.ENABLE, enable ? 0u : 1u);

                // NB: fit from Fig. 10 of CAT4016 datasheet
                // x = (y/a)^(1/b)
                // a = 3.833e+05
                // b = -0.9632
                static uint mAToPotSetting(double currentMa)
                {
                    double R = Math.Pow(currentMa / 3.833e+05, 1 / -0.9632);
                    double s = 256 * (R - Headstage64OpticalStimulator.MinRheostatResistanceOhms) / Headstage64OpticalStimulator.PotResistanceOhms;
                    return s > 255 ? 255 : s < 0 ? 0 : (uint)s;
                }

                uint currentSourceMask = 0;
                static uint percentToPulseMask(int channel, double percent, uint oldMask)
                {
                    uint mask = 0x00000000;
                    var p = 0.0;
                    while (p < percent)
                    {
                        mask = (mask << 1) | 1;
                        p += 12.5;
                    }

                    return channel == 0 ? (oldMask & 0x0000FF00) | mask : (mask << 8) | (oldMask & 0x000000FF);
                }

                static uint pulseDurationToRegister(double pulseDuration, double pulseHz)
                {
                    var pulsePeriod = 1000.0 / pulseHz;
                    return pulseDuration > pulsePeriod ? (uint)(1000 * pulsePeriod - 1) : (uint)(1000 * pulseDuration);
                }

                static uint pulseFrequencyToRegister(double pulseHz, double pulseDuration)
                {
                    var pulsePeriod = 1000.0 / pulseHz;
                    return pulsePeriod > pulseDuration ? (uint)(1000 * pulsePeriod) : (uint)(1000 * pulseDuration + 1);
                }

                return new CompositeDisposable(
                    stimulusSequence.SubscribeSafe(observer, value => {

                        device.WriteRegister(Headstage64OpticalStimulator.MAXCURRENT, mAToPotSetting(value.MaxCurrent));
                        currentSourceMask = percentToPulseMask(0, value.ChannelOneCurrent, currentSourceMask);
                        device.WriteRegister(Headstage64OpticalStimulator.PULSEMASK, currentSourceMask);
                        currentSourceMask = percentToPulseMask(1, value.ChannelTwoCurrent, currentSourceMask);
                        device.WriteRegister(Headstage64OpticalStimulator.PULSEMASK, currentSourceMask);
                        device.WriteRegister(Headstage64OpticalStimulator.PULSEDUR, pulseDurationToRegister(value.PulseDuration, value.PulsesPerSecond));
                        device.WriteRegister(Headstage64OpticalStimulator.PULSEPERIOD, pulseFrequencyToRegister(value.PulsesPerSecond, value.PulseDuration));
                        device.WriteRegister(Headstage64OpticalStimulator.BURSTCOUNT, value.PulsesPerBurst);
                        device.WriteRegister(Headstage64OpticalStimulator.IBI, (uint)(1000 * value.InterBurstInterval));
                        device.WriteRegister(Headstage64OpticalStimulator.TRAINCOUNT, value.BurstsPerTrain);
                        device.WriteRegister(Headstage64OpticalStimulator.TRAINDELAY, (uint)(1000 * value.Delay));
                    }),
                    DeviceManager.RegisterDevice(deviceName, device, DeviceType));
            });
        }
    }

    static class Headstage64OpticalStimulator
    {
        public const int ID = 5;

        // NB: can be read with MINRHEOR and POTRES, but will not change
        public const uint MinRheostatResistanceOhms = 590;
        public const uint PotResistanceOhms = 100_000;

        // managed registers
        public const uint NULLPARM = 0; // No command
        public const uint MAXCURRENT = 1; // Max LED/LD current, (0 to 255 = 800mA to 0 mA.See fig XX of CAT4016 datasheet)
        public const uint PULSEMASK = 2; // Bitmask determining which of the(up to 32) channels is affected by trigger
        public const uint PULSEDUR = 3; // Pulse duration, microseconds
        public const uint PULSEPERIOD = 4; // Inter-pulse interval, microseconds
        public const uint BURSTCOUNT = 5; // Number of pulses in burst
        public const uint IBI = 6; // Inter-burst interval, microseconds
        public const uint TRAINCOUNT = 7; // Number of bursts in train
        public const uint TRAINDELAY = 8; // Stimulus start delay, microseconds
        public const uint TRIGGER = 9; // Trigger stimulation (0 = off, 1 = deliver)
        public const uint ENABLE = 10; // 1: enables the stimulator, 0: stimulator ignores triggers (so that a common trigger can be used)
        public const uint RESTMASK = 11; // Bitmask determining the off state of the up to 32 current channels
        public const uint RESET = 12; // None If 1, Reset all parameters to default (not implemented)
        public const uint MINRHEOR = 13; // The series resistor between the potentiometer (rheostat) and RSET bin on the CAT4016
        public const uint POTRES = 14; // The resistance value of the potentiometer connected in rheostat config to RSET on CAT4016

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Headstage64OpticalStimulator))
            {
            }
        }
    }
}
