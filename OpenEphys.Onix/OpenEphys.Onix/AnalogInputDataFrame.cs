using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    /// <summary>
    /// Buffered analog data produced by the ONIX breakout board.
    /// </summary>
    public class AnalogInputDataFrame
    {
        /// <summary>
        /// Initializes a new instance of the AnalogInputDataFrame class.
        /// </summary>
        /// <param name="clock">A buffered array of <see cref="ManagedFrame{T}.FrameClock"/> values.</param>
        /// <param name="hubSyncCounter"> A buffered array of hub clock counter values.</param>
        /// <param name="analogData">A buffered array of multi-channel analog data.</param>
        public AnalogInputDataFrame(ulong[] clock, ulong[] hubSyncCounter, Mat analogData)
        {
            Clock = clock;
            HubClock = hubSyncCounter;
            AnalogData = analogData;
        }

        /// <inheritdoc cref="ManagedFrame{T}.FrameClock"/>
        public ulong[] Clock { get; }

        /// <summary>
        /// Possibly asynchronous local clock counter.
        /// </summary>
        public ulong[] HubClock { get; }

        /// <summary>
        /// Get the buffered analog data array.
        /// </summary>
        public Mat AnalogData { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct AnalogInputPayload
    {
        public ulong HubClock;
        public fixed short AnalogData[AnalogIO.ChannelCount];
    }
}
