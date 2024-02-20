using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Bonsai;

namespace OpenEphys.Onix
{
    public class ConfigureHeadstage64ElectricalStimulator : SingleDeviceFactory
    {

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
        readonly BehaviorSubject<uint> trainDelay = new(0);
        readonly BehaviorSubject<bool> powerEnable = new(false);

        const double DacBitDepth = 16;
        const double AbsMaxMicroAmps = 2500;

        public ConfigureHeadstage64ElectricalStimulator()
            : base(typeof(Headstage64ElectricalStimulator))
        {
        }

        [Category(ConfigurationCategory)]
        [Description("Specifies whether the electrical stimulation subcircuit will respect triggers.")]
        public bool Enable { get; set; } = true;

        [Category(AcquisitionCategory)]
        [Description("Phase 1 pulse current (uA).")]
        [Range(-AbsMaxMicroAmps, AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        public double PhaseOneCurrent
        {
            get => phaseOneCurrent.Value;
            set => phaseOneCurrent.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("Interphase rest current (uA).")]
        [Range(-AbsMaxMicroAmps, AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        public double InterPhaseCurrent
        {
            get => interPhaseCurrent.Value;
            set => interPhaseCurrent.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("Phase 2 pulse current (uA).")]
        [Range(-AbsMaxMicroAmps, AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        public double PhaseTwoCurrent
        {
            get => phaseTwoCurrent.Value;
            set => phaseTwoCurrent.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("Pulse train start delay (uSec).")]
        [Range(0, uint.MaxValue)]
        public uint TrainDelay
        {
            get => trainDelay.Value;
            set => trainDelay.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("Phase 1 pulse duration (uSec).")]
        [Range(0, uint.MaxValue)]
        public uint PhaseOneDuration
        {
            get => phaseOneDuration.Value;
            set => phaseOneDuration.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("Inter-phase interval (uSec).")]
        [Range(0, uint.MaxValue)]
        public uint InterPhaseInterval
        {
            get => interPhaseInterval.Value;
            set => interPhaseInterval.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("Phase 2 pulse duration (uSec).")]
        [Range(0, uint.MaxValue)]
        public uint PhaseTwoDuration
        {
            get => phaseTwoDuration.Value;
            set => phaseTwoDuration.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("Inter-pulse interval (uSec).")]
        [Range(0, uint.MaxValue)]
        public uint InterPulseInterval
        {
            get => interPulseInterval.Value;
            set => interPulseInterval.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("Inter-burst interval (uSec).")]
        [Range(0, uint.MaxValue)]
        public uint InterBurstInterval
        {
            get => interBurstInterval.Value;
            set => interBurstInterval.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("Number of pulses in each burst.")]
        [Range(0, uint.MaxValue)]
        public uint BurstPulseCount
        {
            get => burstPulseCount.Value;
            set => burstPulseCount.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("Number of bursts in each train.")]
        [Range(0, uint.MaxValue)]
        public uint TrainBurstCount
        {
            get => trainBurstCount.Value;
            set => trainBurstCount.OnNext(value);
        }

        [Category(AcquisitionCategory)]
        [Description("Stimulator power on/off.")]
        [Range(0, uint.MaxValue)]
        public bool PowerEnable
        {
            get => powerEnable.Value;
            set => powerEnable.OnNext(value);
        }

        public override IObservable<ContextTask> Process(IObservable<ContextTask> source)
        {
            var deviceName = DeviceName;
            var deviceAddress = DeviceAddress;
            return source.ConfigureDevice(context =>
            {
                var device = context.GetDeviceContext(deviceAddress, Headstage64ElectricalStimulator.ID);
                device.WriteRegister(Headstage64ElectricalStimulator.ENABLE, Enable ? 1u : 0u);

                static uint uAToCode(double currentuA)
                {
                    var k = 1 / (2 * AbsMaxMicroAmps / (Math.Pow(2, DacBitDepth) - 1)); // static
                    return (uint)(k * (currentuA + AbsMaxMicroAmps));
                }

                return new CompositeDisposable(
                    DeviceManager.RegisterDevice(deviceName, device, DeviceType),
                    phaseOneCurrent.Subscribe(newValue => device.WriteRegister(Headstage64ElectricalStimulator.CURRENT1, uAToCode(newValue))),
                    interPhaseCurrent.Subscribe(newValue => device.WriteRegister(Headstage64ElectricalStimulator.RESTCURR, uAToCode(newValue))),
                    phaseTwoCurrent.Subscribe(newValue => device.WriteRegister(Headstage64ElectricalStimulator.CURRENT2, uAToCode(newValue))),
                    trainDelay.Subscribe(newValue => device.WriteRegister(Headstage64ElectricalStimulator.TRAINDELAY, newValue)),
                    phaseOneDuration.Subscribe(newValue => device.WriteRegister(Headstage64ElectricalStimulator.PULSEDUR1, newValue)),
                    interPhaseInterval.Subscribe(newValue => device.WriteRegister(Headstage64ElectricalStimulator.INTERPHASEINTERVAL, newValue)),
                    phaseTwoDuration.Subscribe(newValue => device.WriteRegister(Headstage64ElectricalStimulator.PULSEDUR2, newValue)),
                    interPulseInterval.Subscribe(newValue => device.WriteRegister(Headstage64ElectricalStimulator.INTERPULSEINTERVAL, newValue)),
                    interBurstInterval.Subscribe(newValue => device.WriteRegister(Headstage64ElectricalStimulator.INTERBURSTINTERVAL, newValue)),
                    burstPulseCount.Subscribe(newValue => device.WriteRegister(Headstage64ElectricalStimulator.BURSTCOUNT, newValue)),
                    trainBurstCount.Subscribe(newValue => device.WriteRegister(Headstage64ElectricalStimulator.TRAINCOUNT, newValue)),
                    powerEnable.Subscribe(newValue => device.WriteRegister(Headstage64ElectricalStimulator.POWERON, newValue ? 1u: 0u))
                );

            });
        }
    }

    static class Headstage64ElectricalStimulator
    {
        public const int ID = 4;

        // managed registers
        public const uint NULLPARM = 0; // No command
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
        public const uint ENABLE = 14; // If 0 then stimulation triggers will be ignored, otherwise they will be applied 
        public const uint RESTCURR = 15; // Resting current between pulse phases
        public const uint RESET = 16; // Reset all parameters to default
        public const uint REZ = 17; // Internal DAC resolution in bits

        internal class NameConverter : DeviceNameConverter
        {
            public NameConverter()
                : base(typeof(Headstage64ElectricalStimulator))
            {
            }
        }
    }
}
