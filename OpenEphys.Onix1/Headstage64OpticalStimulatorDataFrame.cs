using System.Runtime.InteropServices;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A headstage-64 optical stimulus report.
    /// </summary>
    /// <remarks>
    /// These frames provide synchronized information about the stimulus timing, trigger source, and stimulus
    /// parameters.
    /// </remarks>
    public class Headstage64OpticalStimulatorDataFrame : DataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Headstage64OpticalStimulatorDataFrame"/> class.
        /// </summary>
        /// <param name="frame">An ONI frame containing a headstage-64 optical stimulus report.</param>
        public unsafe Headstage64OpticalStimulatorDataFrame(oni.Frame frame)
            : base(frame.Clock)
        {
            var payload = (Headstage64OpticalStimulatorPayload*)frame.Data.ToPointer();
            HubClock = payload->HubClock;
            Origin = (Headstage64StimulatorTriggerOrigin)(payload->DelayAndOrigin & 0x000F);
            Delay = (payload->DelayAndOrigin & 0xFFF0) >> 8;
            ChannelOneCurrent = CodeToMilliamps(payload->MaxCurrent, (byte)(payload->PulseMask & 0x00FF));
            ChannelTwoCurrent = CodeToMilliamps(payload->MaxCurrent, (byte)((payload->PulseMask & 0xFF00) >> 8));
            PulseDuration = payload->PulseDuration / 1e3;
            PulsePeriod = payload->PulsePeriod / 1e3;
            PulsesPerBurst = payload->PulsesPerBurst;
            InterBurstInterval = payload->InterBurstInterval / 1e3;
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
        /// Gets the channel one pulse current in milliamps.
        /// </summary>
        public double ChannelOneCurrent { get; }

        /// <summary>
        /// Gets the channel two pulse current in milliamps.
        /// </summary>
        public double ChannelTwoCurrent { get; }

        /// <summary>
        /// Gets the pulse duration in millseconds.
        /// </summary>
        public double PulseDuration { get; }

        /// <summary>
        /// Gets the pulse period in milliseconds.
        /// </summary>
        public double PulsePeriod { get; }

        /// <summary>
        /// Gets the number of pulses per burst.
        /// </summary>
        public uint PulsesPerBurst { get; }

        /// <summary>
        /// Gets the inter-burst interval duration in milliseconds.
        /// </summary>
        public double InterBurstInterval { get; }

        /// <summary>
        /// Gets the number of bursts per train.
        /// </summary>
        public uint BurstsPerTrain { get; }

        static double CodeToMilliamps(uint potSetting, byte mask)
        {
            return Headstage64OpticalStimulator.PotSettingToMilliamps(potSetting) * 0.125 * BitsSetTable[mask];
        }

        // TODO: The use of hardware BitOperations.PopCount() hardware intrinsic is only available starting in
        // .NET Core 3.0. If we upgrade we should use that instead
        static readonly byte[] BitsSetTable = new byte[256]
        {
            0,1,1,2,1,2,2,3,1,2,2,3,2,3,3,4,1,2,2,3,2,3,3,4,2,3,3,4,3,4,4,5,
            1,2,2,3,2,3,3,4,2,3,3,4,3,4,4,5,2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,
            1,2,2,3,2,3,3,4,2,3,3,4,3,4,4,5,2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,
            2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,3,4,4,5,4,5,5,6,4,5,5,6,5,6,6,7,
            1,2,2,3,2,3,3,4,2,3,3,4,3,4,4,5,2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,
            2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,3,4,4,5,4,5,5,6,4,5,5,6,5,6,6,7,
            2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,3,4,4,5,4,5,5,6,4,5,5,6,5,6,6,7,
            3,4,4,5,4,5,5,6,4,5,5,6,5,6,6,7,4,5,5,6,5,6,6,7,5,6,6,7,6,7,7,8
        };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct Headstage64OpticalStimulatorPayload
    {
        public ulong HubClock;
        public uint DelayAndOrigin;
        public uint MaxCurrent;
        public uint PulseMask;
        public uint PulseDuration;
        public uint PulsePeriod;
        public uint PulsesPerBurst;
        public uint InterBurstInterval;
        public uint BurstsPerTrain;
    }
}
