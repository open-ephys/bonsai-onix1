using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Configures a headstage-64 onboard electrical stimulator.
    /// </summary>
    /// <remarks>
    /// This configuration operator can be linked to a data IO operator, such as <see
    /// cref="Headstage64ElectricalStimulatorTrigger"/>, using a shared
    /// <c>DeviceName</c>.
    /// </remarks>
    [Description("Configures a headstage-64 onboard electrical stimulator.")]
    [Editor("OpenEphys.Onix1.Design.Headstage64ElectricalStimulatorComponentEditor, OpenEphys.Onix1.Design", typeof(ComponentEditor))]
    public class ConfigureHeadstage64ElectricalStimulator : SingleDeviceFactory
    {
        readonly BehaviorSubject<bool> stimEnable = new(false);
        readonly BehaviorSubject<double> phaseOneCurrent = new(0);
        readonly BehaviorSubject<double> interPhaseCurrent = new(0);
        readonly BehaviorSubject<double> phaseTwoCurrent = new(0);
        readonly BehaviorSubject<uint> phaseOneDuration = new(0);
        readonly BehaviorSubject<uint> interPhaseInterval = new(0);
        readonly BehaviorSubject<uint> phaseTwoDuration = new(0);
        readonly BehaviorSubject<uint> interPulseInterval = new(0);
        readonly BehaviorSubject<uint> burstPulseCount = new(0);
        readonly BehaviorSubject<uint> interBurstInterval = new(0);
        readonly BehaviorSubject<uint> trainBurstCount = new(0);
        readonly BehaviorSubject<uint> triggerDelay = new(0);
        readonly BehaviorSubject<bool> powerEnable = new(false);

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureHeadstage64ElectricalStimulator"/> class.
        /// </summary>
        public ConfigureHeadstage64ElectricalStimulator()
            : base(typeof(Headstage64ElectricalStimulator))
        {
        }

        /// <summary>
        /// Copy constructor for the <see cref="ConfigureHeadstage64ElectricalStimulator"/> class.
        /// </summary>
        /// <param name="electricalStimulator">Existing <see cref="ConfigureHeadstage64ElectricalStimulator"/> object.</param>
        public ConfigureHeadstage64ElectricalStimulator(ConfigureHeadstage64ElectricalStimulator electricalStimulator) : this()
        {
            DeviceName = electricalStimulator.DeviceName;
            DeviceAddress = electricalStimulator.DeviceAddress;
            Enable = electricalStimulator.Enable;
            StimEnable = electricalStimulator.StimEnable;
            PowerEnable = electricalStimulator.PowerEnable;
            TriggerDelay = electricalStimulator.TriggerDelay;
            PhaseOneCurrent = electricalStimulator.PhaseOneCurrent;
            InterPhaseCurrent = electricalStimulator.InterPhaseCurrent;
            PhaseTwoCurrent = electricalStimulator.PhaseTwoCurrent;
            PhaseOneDuration = electricalStimulator.PhaseOneDuration;
            InterPhaseInterval = electricalStimulator.InterPhaseInterval;
            PhaseTwoDuration = electricalStimulator.PhaseTwoDuration;
            InterPulseInterval = electricalStimulator.InterPulseInterval;
            InterBurstInterval = electricalStimulator.InterBurstInterval;
            BurstPulseCount = electricalStimulator.BurstPulseCount;
            TrainBurstCount = electricalStimulator.TrainBurstCount;
        }

        /// <summary>
        /// Gets or sets the data enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, <see cref="Headstage64ElectricalStimulatorData"/> will produce data. If set to
        /// false, <see cref="Headstage64ElectricalStimulatorData"/> will not produce data.
        /// </remarks>
        [Category(ConfigurationCategory)]
        [Description("Specifies whether the headstage-64 electrical stimulator will produce stimulus reports.")]
        public bool Enable { get; set; }

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        /// <remarks>
        /// If set to true, then the electrical stimulator circuit will respect triggers. If set to false, triggers will be ignored.
        /// </remarks>
        [Description("Specifies whether the electrical stimulator will respect triggers.")]
        [Category(AcquisitionCategory)]
        public bool StimEnable { get; set; } = true;

        /// <summary>
        /// Gets or sets the electrical stimulator's power state.
        /// </summary>
        /// <remarks>
        /// If set to true, then the electrical stimulator's ±15V power supplies will be turned on. If set to false,
        /// they will be turned off. It may be desirable to power down the electrical stimulator's power supplies outside
        /// of stimulation windows to reduce power consumption and electrical noise. This property must be set to true
        /// in order for electrical stimuli to be delivered properly. It takes ~10 milliseconds for these supplies to stabilize.
        /// </remarks>
        [Description("Stimulator power on/off.")]
        [Category(AcquisitionCategory)]
        public bool PowerEnable
        {
            get => powerEnable.Value;
            set => powerEnable.OnNext(value);
        }

        /// <summary>
        /// Gets or sets a delay from receiving a trigger to the start of stimulus sequence application in μsec.
        /// </summary>
        [Description("A delay from receiving a trigger to the start of stimulus sequence application (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(AcquisitionCategory)]
        public uint TriggerDelay
        {
            get => triggerDelay.Value;
            set => triggerDelay.OnNext(value);
        }

        static double ClampCurrent(double value)
        {
            if (value > Headstage64ElectricalStimulator.AbsMaxMicroAmps)
                return Headstage64ElectricalStimulator.AbsMaxMicroAmps;
            else if (value < -Headstage64ElectricalStimulator.AbsMaxMicroAmps)
                return -Headstage64ElectricalStimulator.AbsMaxMicroAmps;
            else
                return value;
        }

        /// <summary>
        /// Gets or sets the amplitude of the first phase of each pulse in μA.
        /// </summary>
        [Description("Amplitude of the first phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(AcquisitionCategory)]
        public double PhaseOneCurrent
        {
            get => phaseOneCurrent.Value;
            set => phaseOneCurrent.OnNext(ClampCurrent(value));
        }

        /// <summary>
        /// Gets or sets the amplitude of the interphase current of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the inter-phase current of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(AcquisitionCategory)]
        public double InterPhaseCurrent
        {
            get => interPhaseCurrent.Value;
            set => interPhaseCurrent.OnNext(ClampCurrent(value));
        }

        /// <summary>
        /// Gets or sets the amplitude of the second phase of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the second phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(AcquisitionCategory)]
        public double PhaseTwoCurrent
        {
            get => phaseTwoCurrent.Value;
            set => phaseTwoCurrent.OnNext(ClampCurrent(value));
        }

        /// <summary>
        /// Gets or sets the duration of the first phase of each pulse in μsec.
        /// </summary>
        [Description("The duration of the first phase of each pulse in μsec.")]
        [Range(0, uint.MaxValue)]
        [Category(AcquisitionCategory)]
        public uint PhaseOneDuration
        {
            get => phaseOneDuration.Value;
            set => phaseOneDuration.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the duration of the interphase interval of each pulse in μsec.
        /// </summary>
        [Description("The duration of the interphase interval of each pulse (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(AcquisitionCategory)]
        public uint InterPhaseInterval
        {
            get => interPhaseInterval.Value;
            set => interPhaseInterval.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the duration of the second phase of each pulse in μsec.
        /// </summary>
        [Description("The duration of the second phase of each pulse (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(AcquisitionCategory)]
        public uint PhaseTwoDuration
        {
            get => phaseTwoDuration.Value;
            set => phaseTwoDuration.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the duration of the inter-pulse interval within a single burst in μsec.
        /// </summary>
        [Description("The duration of the inter-pulse interval within a single burst (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(AcquisitionCategory)]
        public uint InterPulseInterval
        {
            get => interPulseInterval.Value;
            set => interPulseInterval.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the duration of the inter-burst interval within a stimulus train in μsec.
        /// </summary>
        [Description("The duration of the inter-burst interval within a stimulus train (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(AcquisitionCategory)]
        public uint InterBurstInterval
        {
            get => interBurstInterval.Value;
            set => interBurstInterval.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the number of pulses per burst.
        /// </summary>
        [Description("The number of pulses per burst.")]
        [Range(0, uint.MaxValue)]
        [Category(AcquisitionCategory)]
        public uint BurstPulseCount
        {
            get => burstPulseCount.Value;
            set => burstPulseCount.OnNext(value);
        }

        /// <summary>
        /// Gets or sets the number of bursts in a stimulus train.
        /// </summary>
        [Description("The number of bursts in each train.")]
        [Range(0, uint.MaxValue)]
        [Category(AcquisitionCategory)]
        public uint TrainBurstCount
        {
            get => trainBurstCount.Value;
            set => trainBurstCount.OnNext(value);
        }

        /// <summary>
        /// Configure a headstage-64 onboard electrical stimulator.
        /// </summary>
        /// <remarks>
        /// This will schedule configuration actions to be applied by a <see cref="StartAcquisition"/>
        /// instance prior to data acquisition.
        /// </remarks>
        /// <param name="source">A sequence of <see cref="ContextTask"/> instances that holds configuration
        /// actions.</param>
        /// <returns>The original sequence modified by adding additional configuration actions required to
        /// configure a headstage-64 onboard electrical stimulator.</returns>
        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var enable = Enable;
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;

            return source.ConfigureDevice((context, observer) =>
            {
                var device = context.GetDeviceContext(deviceAddress, DeviceType);
                device.WriteRegister(Headstage64ElectricalStimulator.ENABLE, enable ? 1u : 0u);

                return new CompositeDisposable(
                    stimEnable.SubscribeSafe(observer, value =>
                        device.WriteRegister(Headstage64ElectricalStimulator.STIMENABLE, value ? 1u : 0u)),
                    phaseOneCurrent.SubscribeSafe(observer, value =>
                        device.WriteRegister(Headstage64ElectricalStimulator.CURRENT1, Headstage64ElectricalStimulator.MicroampsToCode(value))),
                    interPhaseCurrent.SubscribeSafe(observer, value =>
                        device.WriteRegister(Headstage64ElectricalStimulator.RESTCURR, Headstage64ElectricalStimulator.MicroampsToCode(value))),
                    phaseTwoCurrent.SubscribeSafe(observer, value =>
                        device.WriteRegister(Headstage64ElectricalStimulator.CURRENT2, Headstage64ElectricalStimulator.MicroampsToCode(value))),
                    triggerDelay.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.TRAINDELAY, value)),
                    phaseOneDuration.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.PULSEDUR1, value)),
                    interPhaseInterval.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.INTERPHASEINTERVAL, value)),
                    phaseTwoDuration.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.PULSEDUR2, value)),
                    interPulseInterval.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.INTERPULSEINTERVAL, value)),
                    interBurstInterval.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.INTERBURSTINTERVAL, value)),
                    burstPulseCount.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.BURSTCOUNT, value)),
                    trainBurstCount.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.TRAINCOUNT, value)),
                    powerEnable.SubscribeSafe(observer, value => device.WriteRegister(Headstage64ElectricalStimulator.POWERON, value ? 1u : 0u)),
                    DeviceManager.RegisterDevice(deviceName, device, DeviceType));
            });
        }
    }

    static class Headstage64ElectricalStimulator
    {
        public const int ID = 4;
        public const uint MinimumVersion = 3;

        // NB: could be read from REZ but these are constant
        public const double DacBitDepth = 16;
        public const double AbsMaxMicroAmps = 2500;

        // managed registers
        public const uint ENABLE = 0; // Enable stimulus report stream
        public const uint BIPHASIC = 1; // Biphasic pulse (0 = monophasic, 1 = biphasic; NB: currently ignored)
        public const uint CURRENT1 = 2; // Phase 1 current
        public const uint CURRENT2 = 3; // Phase 2 current
        public const uint PULSEDUR1 = 4; // Phase 1 duration, 1 microsecond steps
        public const uint INTERPHASEINTERVAL = 5; // Inter-phase interval, 10 microsecond steps
        public const uint PULSEDUR2 = 6; // Phase 2 duration, 1 microsecond steps
        public const uint INTERPULSEINTERVAL = 7; // Inter-pulse interval, 10 microsecond steps
        public const uint BURSTCOUNT = 8; // Burst duration, number of pulses in burst
        public const uint INTERBURSTINTERVAL = 9; // Inter-burst interval, microseconds
        public const uint TRAINCOUNT = 10; // Pulse train duration, number of bursts in train
        public const uint TRAINDELAY = 11; // Pulse train delay, microseconds
        public const uint TRIGGER = 12; // Trigger stimulation (1 = deliver)
        public const uint POWERON = 13; // Control estim sub-circuit power (0 = off, 1 = on)
        public const uint STIMENABLE = 14; // If 0 then stimulation triggers will be ignored, otherwise they will be applied 
        public const uint RESTCURR = 15; // Resting current between pulse phases
        public const uint RESET = 16; // Reset all parameters to default
        public const uint REZ = 17; // Internal DAC resolution in bits

        internal static uint MicroampsToCode(double currentuA)
        {
            var k = 1 / (2 * AbsMaxMicroAmps / (Math.Pow(2, DacBitDepth) - 1)); // NB: constexpr, if we get support for it.
            return (uint)(k * (currentuA + AbsMaxMicroAmps));
        }

        internal static double CodeToMicroamps(uint code)
        {
            var k = 2 * AbsMaxMicroAmps / (Math.Pow(2, DacBitDepth) - 1); // NB: constexpr, if we get support for it.
            return k * code - AbsMaxMicroAmps;
        }

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Headstage64ElectricalStimulator))
            {
            }
        }
    }
}
