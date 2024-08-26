using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Buffered data from a Nric1384 bioacqusition device.
    /// </summary>
    public class Nric1384DataFrame : BufferedDataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Nric1384DataFrame"/> class.
        /// </summary>
        /// <param name="clock">An array of <see cref="DataFrame.Clock"/> values.</param>
        /// <param name="hubClock">An array of hub clock counter values.</param>
        /// <param name="frameCount">An array of frame count values.</param>
        /// <param name="spikeData">An array of multi-channel spike data as a <see cref="Mat"/> object.</param>
        /// <param name="lfpData">An array of multi-channel LFP data as a <see cref="Mat"/> object.</param>
        public Nric1384DataFrame(ulong[] clock, ulong[] hubClock, int[] frameCount, Mat spikeData, Mat lfpData)
             : base(clock, hubClock)
        {
            FrameCount = frameCount;
            SpikeData = spikeData;
            LfpData = lfpData;
        }

        /// <summary>
        /// Gets the frame count value array.
        /// </summary>
        /// <remarks>
        /// A 20-bit counter on the chip that increments its value for every frame produced. The value ranges from 0 to
        /// 1048575 (2^20-1), and should always increment by 13 (one count is taken per super-frame and there are 13 frames 
        /// in a super frame) until it wraps around back to 0. This can be used to detect dropped frames.
        /// </remarks>
        public int[] FrameCount { get; }


        /// <summary>
        /// Gets the spike-band data as a <see cref="Mat"/> object.
        /// </summary>
        /// <remarks>
        /// Spike-band data has 384 rows (channels) with columns representing the samples acquired at 30 kHz. Each sample is a
        /// 10-bit, offset binary value encoded as a <see cref="ushort"/>.
        /// </remarks>
        public Mat SpikeData { get; }


        /// <summary>
        /// Gets the LFP band data as a <see cref="Mat"/> object.
        /// </summary>
        /// <remarks>
        /// LFP data has 32 rows (channels) with columns representing the samples acquired at 2.5 kHz. Each sample is a
        /// 10-bit, offset binary value encoded as a <see cref="ushort"/>.
        /// </remarks>
        public Mat LfpData { get; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct Nric1384Payload
    {
        public ulong HubClock;
        public fixed ushort LfpData[NeuropixelsV1e.AdcCount];
        public fixed ushort ApData[NeuropixelsV1e.ChannelCount];
        public int FrameCount;
    }
}
