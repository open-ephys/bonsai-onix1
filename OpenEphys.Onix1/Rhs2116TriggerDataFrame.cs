using System;
using System.Runtime.InteropServices;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A trigger event received by an Rhs2116 trigger device.
    /// </summary>
    /// <remarks>
    /// These events provide synchronized recordings of stimulus trigger times. They provide information about
    /// the source of the trigger and if it was applied or ignored and for what reason.
    /// </remarks>
    public class Rhs2116TriggerDataFrame : DataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rhs2116TriggerDataFrame"/> class.
        /// </summary>
        /// <param name="frame">An ONI data frame containing Rhs2116 trigger data.</param>
        public unsafe Rhs2116TriggerDataFrame(oni.Frame frame)
            : base(frame.Clock)
        {
            var payload = (Rhs2116TriggerPayload*)frame.Data.ToPointer();
            HubClock = payload->HubClock;
            Delay = 1e6 * ((payload->TriggerCode & 0b1111_1111_1111_1111_1111_0000_0000_0000) >> 12) / Rhs2116.SampleFrequencyHz;
            Status = (Rhs2116TriggerResult)(payload->TriggerCode & 0b11);
            Origin = (Rhs2116TriggerOrigin)((payload->TriggerCode & 0b11100) >> 2) ;
        }

        /// <summary>
        /// Gets the delay, in microseconds, from this trigger to the physical application of the stimulus
        /// sequence.
        /// </summary>
        /// <remarks>
        /// This value is determined by the delay sequence provided as input to <see
        /// cref="Rhs2116StimulusTrigger"/>.
        /// </remarks>
        public double Delay { get; }

        /// <summary>
        /// Gets the status of the trigger.
        /// </summary>
        public Rhs2116TriggerResult Status { get; }

        /// <summary>
        /// Gets the origin of the trigger.
        /// </summary>
        public Rhs2116TriggerOrigin Origin {get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct Rhs2116TriggerPayload
    {
        public ulong HubClock;
        public uint TriggerCode;
        public short Padding;
    }

    /// <summary>
    /// Specifies the status of the trigger.
    /// </summary>
    [Flags]
    public enum Rhs2116TriggerResult : byte
    {
        /// <summary>
        /// Specifies that the trigger was applied successfully.
        /// </summary>
        Delivered = 0x0,

        /// <summary>
        /// Specifies that the trigger was ignored because the stimulator is disarmed.
        /// </summary>
        Disarmed = 0x1,

        /// <summary>
        /// Specifies that the trigger was ignored because the stimulus sequencer is in the middle of applying a stimulus sequence.
        /// </summary>
        SequencerBusy = 0x02,
    }

    /// <summary>
    /// Specifies the origin of the trigger.
    /// </summary>
    [Flags]
    public enum Rhs2116TriggerOrigin : byte
    {
        /// <summary>
        /// Specifies the source of the trigger is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Specifies the source of the trigger is a local Gpio pin.
        /// </summary>
        Gpio = 0x1,

        /// <summary>
        /// Specifies the source of the trigger is a a register.
        /// </summary>
        Register = 0x2,

        /// <summary>
        /// Specifies the source of the trigger is an external synchronization pin.
        /// </summary>
        External = 0x4
    }
}
