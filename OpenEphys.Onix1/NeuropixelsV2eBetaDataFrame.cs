using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Buffered data from a NeuropixelsV2-Beta probe.
    /// </summary>
    public class NeuropixelsV2eBetaDataFrame : BufferedDataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2eDataFrame"/> class.
        /// </summary>
        /// <param name="clock">An array of <see cref="DataFrame.Clock"/> values.</param>
        /// <param name="hubClock">An array of hub clock counter values.</param>
        /// <param name="amplifierData">An array of multi-channel amplifier data.</param>
        /// <param name="frameCount">An array of frame count values.</param>
        public NeuropixelsV2eBetaDataFrame(ulong[] clock, ulong[] hubClock, Mat amplifierData, int[] frameCount)
            : base(clock, hubClock)
        {
            AmplifierData = amplifierData;
            FrameCount = frameCount;
        }

        /// <summary>
        /// Gets the buffered electrophysiology data array.
        /// </summary>
        /// <remarks>
        /// Electrophysiology samples are organized in 384xN matrix with rows representing electrophysiology
        /// channel number and N columns representing samples acquired at 30 kHz. Each column is a 384-channel
        /// vector of ADC samples whose acquisition time is indicated by the corresponding elements in <see
        /// cref="DataFrame.Clock"/> and <see cref="DataFrame.HubClock"/>. Each ADC sample is a 14-bit, offset
        /// binary value represented as a <see cref="ushort"/>. The following equation can be used to convert
        /// a sample to microvolts:
        /// <code>
        /// Electrode Voltage (µV) = 0.76294 × (ADC Sample – 8192)
        /// </code>
        /// </remarks>
        public Mat AmplifierData { get; }

        /// <summary>
        /// Gets the frame count value array.
        /// </summary>
        /// <remarks>
        /// A 20-bit counter on the probe that increments its value for every "frame" produced by the probe.
        /// Sixteen frames are produced for each 384-channel column of samples in  <see cref="AmplifierData"/>.
        /// The value ranges from 0 to 1048575 (2^20-1), and should always increment by 1 until it wraps
        /// around back to 0. This can be used to detect dropped frames.
        /// </remarks>
        public int[] FrameCount { get; }

        internal static unsafe ushort GetProbeIndex(oni.Frame frame)
        {
            var data = (NeuropixelsV2BetaPayload*)frame.Data.ToPointer();
            return data->ProbeIndex;
        }

        internal static unsafe void CopyAmplifierBuffer(ushort* superFrame, ushort[,] amplifierBuffer, int[] frameCounter, int index, double gainCorrection, bool invertPolarity, int[,] channelOrder)
        {
            if (invertPolarity)
            {
                const double NumberOfAdcBins = 16384;
                double inversionOffset = gainCorrection * NumberOfAdcBins;

                // Loop over 16 "frames" within each "super frame"
                for (var i = 0; i < NeuropixelsV2eBeta.FramesPerSuperFrame; i++)
                {
                    var frameOffset = i * NeuropixelsV2eBeta.FrameWords;
                    var frameCounterIndex = index * NeuropixelsV2eBeta.FramesPerSuperFrame + i;
                    frameCounter[frameCounterIndex] = (superFrame[frameOffset] << 14) | (superFrame[frameOffset + 1] << 0);

                    // The period of data within super frame is 28 words (24 ADCs, 2 Syncs, 2 counters)
                    var adcDataOffset = 2 + frameOffset;

                    // Loop over ADC samples within each "frame" and map to channel position
                    for (var k = 0; k < NeuropixelsV2eBeta.ADCsPerProbe; k++)
                    {
                        amplifierBuffer[channelOrder[k, i], index] = (ushort)(inversionOffset - gainCorrection * superFrame[adcDataOffset + k]);
                    }
                }
            }
            else
            {
                // Loop over 16 "frames" within each "super frame"
                for (var i = 0; i < NeuropixelsV2eBeta.FramesPerSuperFrame; i++)
                {
                    var frameOffset = i * NeuropixelsV2eBeta.FrameWords;
                    var frameCounterIndex = index * NeuropixelsV2eBeta.FramesPerSuperFrame + i;
                    frameCounter[frameCounterIndex] = (superFrame[frameOffset] << 14) | (superFrame[frameOffset + 1] << 0);

                    // The period of data within super frame is 28 words (24 ADCs, 2 Syncs, 2 counters)
                    var adcDataOffset = 2 + frameOffset;

                    // Loop over ADC samples within each "frame" and map to channel position
                    for (var k = 0; k < NeuropixelsV2eBeta.ADCsPerProbe; k++)
                    {
                        amplifierBuffer[channelOrder[k, i], index] = (ushort)(gainCorrection * superFrame[adcDataOffset + k]);
                    }
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct NeuropixelsV2BetaPayload
    {
        public ulong HubClock;
        public ushort ProbeIndex;
        public uint Reserved;
        public fixed ushort SuperFrame[NeuropixelsV2eBeta.FramesPerSuperFrame * NeuropixelsV2eBeta.FrameWords];
    }
}
