using Bonsai;
using System.ComponentModel;
using System.Drawing.Design;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A stimulus sequence for the Headstage64 Electrical Stimulator.
    /// </summary>
    public class Headstage64ElectricalStimulatorSequence
    {
        /// <summary>
        /// Gets or sets a delay from receiving a trigger to the start of stimulus sequence application in μsec.
        /// </summary>
        [Description("A delay from receiving a trigger to the start of stimulus sequence application (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public uint TriggerDelay { get; set; } = 0;

        private static double VerifyCurrentLimits(double value)
        {
            if (value > Headstage64ElectricalStimulator.AbsMaxMicroAmps)
                return Headstage64ElectricalStimulator.AbsMaxMicroAmps;
            else if (value < -Headstage64ElectricalStimulator.AbsMaxMicroAmps)
                return -Headstage64ElectricalStimulator.AbsMaxMicroAmps;
            else
                return value;
        }

        private double phaseOneCurrent = 0;

        /// <summary>
        /// Gets or sets the amplitude of the first phase of each pulse in μA.
        /// </summary>
        [Description("Amplitude of the first phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public double PhaseOneCurrent
        { 
            get => phaseOneCurrent;
            set => phaseOneCurrent = VerifyCurrentLimits(value);
        }

        private double interPhaseCurrent = 0;

        /// <summary>
        /// Gets or sets the amplitude of the interphase current of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the inter-phase current of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public double InterPhaseCurrent
        {
            get => interPhaseCurrent;
            set => interPhaseCurrent = VerifyCurrentLimits(value);
        }

        private double phaseTwoCurrent = 0;

        /// <summary>
        /// Gets or sets the amplitude of the second phase of each pulse in μA.
        /// </summary>
        [Description("The amplitude of the second phase of each pulse (uA).")]
        [Range(-Headstage64ElectricalStimulator.AbsMaxMicroAmps, Headstage64ElectricalStimulator.AbsMaxMicroAmps)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Precision(3, 1)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public double PhaseTwoCurrent
        {
            get => phaseTwoCurrent;
            set => phaseTwoCurrent = VerifyCurrentLimits(value);
        }

        /// <summary>
        /// Gets or sets the duration of the first phase of each pulse in μsec.
        /// </summary>
        [Description("The duration of the first phase of each pulse in μsec.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public uint PhaseOneDuration { get; set; } = 0;

        /// <summary>
        /// Gets or sets the duration of the interphase interval of each pulse in μsec.
        /// </summary>
        [Description("The duration of the interphase interval of each pulse (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public uint InterPhaseInterval { get; set; } = 0;

        /// <summary>
        /// Gets or sets the duration of the second phase of each pulse in μsec.
        /// </summary>
        [Description("The duration of the second phase of each pulse (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public uint PhaseTwoDuration { get; set; } = 0;

        /// <summary>
        /// Gets or sets the duration of the inter-pulse interval within a single burst in μsec.
        /// </summary>
        [Description("The duration of the inter-pulse interval within a single burst (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public uint InterPulseInterval { get; set; } = 0;

        /// <summary>
        /// Gets or sets the duration of the inter-burst interval within a stimulus train in μsec.
        /// </summary>
        [Description("The duration of the inter-burst interval within a stimulus train (uSec).")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public uint InterBurstInterval { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of pulses per burst.
        /// </summary>
        [Description("The number of pulses per burst.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public uint BurstPulseCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of bursts in a stimulus train.
        /// </summary>
        [Description("The number of bursts in each train.")]
        [Range(0, uint.MaxValue)]
        [Category(DeviceFactory.ConfigurationCategory)]
        public uint TrainBurstCount { get; set; } = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Headstage64ElectricalStimulatorSequence"/> class with default values.
        /// </summary>
        public Headstage64ElectricalStimulatorSequence()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Headstage64ElectricalStimulatorSequence"/> class with the given values.
        /// </summary>
        public Headstage64ElectricalStimulatorSequence(uint triggerDelay, double phaseOneCurrent, double interPhaseCurrent,
            double phaseTwoCurrent, uint phaseOneDuration, uint interPhaseInterval, uint phaseTwoDuration, uint interPulseInterval,
            uint interBurstInterval, uint burstPulseCount, uint trainBurstCount)
        {
            TriggerDelay = triggerDelay;
            PhaseOneCurrent = phaseOneCurrent;
            InterPhaseCurrent = interPhaseCurrent;
            PhaseTwoCurrent = phaseTwoCurrent;
            PhaseOneDuration = phaseOneDuration;
            InterPhaseInterval = interPhaseInterval;
            PhaseTwoDuration = phaseTwoDuration;
            InterPulseInterval = interPulseInterval;
            InterBurstInterval = interBurstInterval;
            BurstPulseCount = burstPulseCount;
            TrainBurstCount = trainBurstCount;
        }

        /// <summary>
        /// Copy constructor for the <see cref="Headstage64ElectricalStimulatorSequence"/> class.
        /// </summary>
        /// <param name="stimulatorSequence">Existing sequence to copy.</param>
        public Headstage64ElectricalStimulatorSequence(Headstage64ElectricalStimulatorSequence stimulatorSequence)
        {
            TriggerDelay = stimulatorSequence.TriggerDelay;
            PhaseOneCurrent = stimulatorSequence.PhaseOneCurrent;
            InterPhaseCurrent = stimulatorSequence.InterPhaseCurrent;
            PhaseTwoCurrent = stimulatorSequence.PhaseTwoCurrent;
            PhaseOneDuration = stimulatorSequence.PhaseOneDuration;
            InterPhaseInterval = stimulatorSequence.InterPhaseInterval;
            PhaseTwoDuration = stimulatorSequence.PhaseTwoDuration;
            InterPulseInterval = stimulatorSequence.InterPulseInterval;
            InterBurstInterval = stimulatorSequence.InterBurstInterval;
            BurstPulseCount = stimulatorSequence.BurstPulseCount;
            TrainBurstCount = stimulatorSequence.TrainBurstCount;
        }
    }
}
