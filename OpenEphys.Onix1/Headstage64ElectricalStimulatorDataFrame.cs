using System;
using System.Runtime.InteropServices;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A headstage-64 electrical stimulus report.
    /// </summary>
    /// <remarks>
    /// These frames provide synchronized information about the stimulus timing, trigger source, and stimulus
    /// parameters.
    /// </remarks>
    public class Headstage64ElectricalStimulatorDataFrame : DataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Headstage64ElectricalStimulatorDataFrame"/> class.
        /// </summary>
        /// <param name="frame">An ONI frame containing a electrical stimulus report.</param>
        public unsafe Headstage64ElectricalStimulatorDataFrame(oni.Frame frame)
            : base(frame.Clock)
        {
            var payload = (Headstage64ElectricalStimulatorPayload*)frame.Data.ToPointer();
            HubClock = payload->HubClock;
            Origin = (Headstage64StimulatorTriggerOrigin)(payload->DelayAndOrigin & 0x000F);
            Delay = (payload->DelayAndOrigin & 0xFFF0) >> 8;
            PhaseOneCurrent = Headstage64ElectricalStimulator.CodeToMicroamps(payload->PhaseOneCurrent);
            InterPhaseCurrent = Headstage64ElectricalStimulator.CodeToMicroamps(payload->RestCurrent);
            PhaseTwoCurrent = Headstage64ElectricalStimulator.CodeToMicroamps(payload->PhaseTwoCurrent);
            PhaseOneDuration = payload->PhaseOneDuration;
            InterPhaseInterval = payload->InterPhaseInterval;
            PhaseTwoDuration = payload->PhaseTwoDuration;
            InterPulseInterval = payload->InterPulseInterval;
            PulsesPerBurst = payload->PulsesPerBurst;
            InterBurstInterval = payload->InterBurstInterval;
            BurstsPerTrain = payload->BurstsPerTrain;
        }

        /// <summary>
        /// Gets the stimulus trigger origin.
        /// </summary>
        public Headstage64StimulatorTriggerOrigin Origin {get; }

        /// <summary>
        /// Gets the delay, in microseconds, from the time of trigger receipt (the <see
        /// cref="DataFrame.HubClock"/> value) to the physical application of the stimulus sequence.
        /// </summary>
        public uint Delay { get; }

        /// <summary>
        /// Gets the phase one current in microamps.
        /// </summary>
        public double PhaseOneCurrent { get; }

        /// <summary>
        /// Gets the inter-phase current in microamps.
        /// </summary>
        public double InterPhaseCurrent { get; }

        /// <summary>
        /// Gets the phase two current in microamps.
        /// </summary>
        public double PhaseTwoCurrent { get; }

        /// <summary>
        /// Gets the phase one duration in microseconds.
        /// </summary>
        public uint PhaseOneDuration { get; }

        /// <summary>
        /// Gets the inter-phase interval duration in microseconds.
        /// </summary>
        public uint InterPhaseInterval { get; }

        /// <summary>
        /// Gets the phase two duration in microseconds.
        /// </summary>
        public uint PhaseTwoDuration { get; }

        /// <summary>
        /// Gets the inter-pulse interval duration in microseconds.
        /// </summary>
        public uint InterPulseInterval { get; }

        /// <summary>
        /// Gets the number of pulses per burst.
        /// </summary>
        public uint PulsesPerBurst { get; }

        /// <summary>
        /// Gets the inter-burst interval duration in microseconds.
        /// </summary>
        public uint InterBurstInterval { get; }

        /// <summary>
        /// Gets the number of bursts per train.
        /// </summary>
        public uint BurstsPerTrain { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct Headstage64ElectricalStimulatorPayload
    {
        public ulong HubClock;
        public uint DelayAndOrigin;
        public uint PhaseOneCurrent;
        public uint RestCurrent;
        public uint PhaseTwoCurrent;
        public uint PhaseOneDuration;
        public uint InterPhaseInterval;
        public uint PhaseTwoDuration;
        public uint InterPulseInterval;
        public uint PulsesPerBurst;
        public uint InterBurstInterval;
        public uint BurstsPerTrain;
    }

    /// <summary>
    /// Specifies the origin of the trigger.
    /// </summary>
    [Flags]
    public enum Headstage64StimulatorTriggerOrigin : byte
    {
        /// <summary>
        /// Specifies the source of the trigger is unknown.
        /// </summary>
        Unknown = 0x0,

        /// <summary>
        /// Specifies the source of the trigger is a local Gpio pin.
        /// </summary>
        Gpio = 0x1,

        /// <summary>
        /// Specifies the source of the trigger is a register.
        /// </summary>
        Register = 0x2,
    }
}
