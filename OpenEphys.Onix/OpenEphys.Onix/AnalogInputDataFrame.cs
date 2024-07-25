using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix
{
    /// <summary>
    /// Buffered analog data produced by the ONIX breakout board.
    /// </summary>
    public class AnalogInputDataFrame : BufferedDataFrame
    {
        /// <summary>
        /// Initializes a new instance of the AnalogInputDataFrame class.
        /// </summary>
        /// <param name="clock">A buffered array of <see cref="ManagedFrame{T}.FrameClock"/> values.</param>
        /// <param name="hubSyncCounter"> A buffered array of hub clock counter values.</param>
        /// <param name="analogData">A buffered array of multi-channel analog data.</param>
        public AnalogInputDataFrame(ulong[] clock, ulong[] hubClock, Mat analogData)
            : base(clock, hubClock)
        {
            AnalogData = analogData;
        }

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
