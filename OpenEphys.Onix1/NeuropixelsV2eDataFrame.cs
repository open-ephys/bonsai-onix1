using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Buffered data from a NeuropixelsV2 probe.
    /// </summary>
    public class NeuropixelsV2eDataFrame : BufferedDataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2eDataFrame"/> class.
        /// </summary>
        /// <param name="clock">An array of <see cref="DataFrame.Clock"/> values.</param>
        /// <param name="hubClock">An array of hub clock counter values.</param>
        /// <param name="amplifierData">An array of multi-channel amplifier data.</param>
        public NeuropixelsV2eDataFrame(ulong[] clock, ulong[] hubClock, Mat amplifierData)
            : base(clock, hubClock)
        {
            AmplifierData = amplifierData;
        }

        /// <summary>
        /// Gets the buffered electrophysiology data array.
        /// </summary>
        /// <remarks>
        /// Electrophysiology samples are organized in 384xN matrix with rows representing electrophysiology
        /// channel number and N columns representing samples acquired at 30 kHz. Each column is a 384-channel
        /// vector of ADC samples whose acquisition time is indicated by the corresponding elements in <see
        /// cref="DataFrame.Clock"/> and <see cref="DataFrame.HubClock"/>. Each ADC sample is a 12-bit, offset
        /// binary value represented as a <see cref="ushort"/>. The following equation can be used to convert
        /// a sample to microvolts:
        /// <code>
        /// Electrode Voltage (µV) = 3.05176 × (ADC Sample – 2048)
        /// </code>
        /// </remarks>
        public Mat AmplifierData { get; }

        internal static unsafe ushort GetProbeIndex(oni.Frame frame)
        {
            var data = (NeuropixelsV2Payload*)frame.Data.ToPointer();
            return data->ProbeIndex;
        }

        internal static unsafe void CopyAmplifierBuffer(ushort* amplifierData, ushort[,] amplifierBuffer, int index, double gainCorrection, bool invertPolarity, int[,] channelOrder)
        {
            if (invertPolarity)
            {
                const double NumberOfAdcBins = 4096;
                double inversionOffset = gainCorrection * NumberOfAdcBins;

                // Loop over 16 "frames" within each "super-frame"
                for (int i = 0; i < NeuropixelsV2e.FramesPerSuperFrame; i++)
                {
                    // The period of ADC data within data array is 36 words
                    var adcDataOffset = i * NeuropixelsV2e.FrameWords;

                    for (int k = 0; k < NeuropixelsV2e.AdcsPerProbe; k++)
                    {
                        amplifierBuffer[channelOrder[k, i], index] = (ushort)(inversionOffset - gainCorrection * amplifierData[AdcIndicies[k] + adcDataOffset]);
                    }
                }
            } 
            else
            {
                // Loop over 16 "frames" within each "super-frame"
                for (int i = 0; i < NeuropixelsV2e.FramesPerSuperFrame; i++)
                {
                    // The period of ADC data within data array is 36 words
                    var adcDataOffset = i * NeuropixelsV2e.FrameWords;

                    for (int k = 0; k < NeuropixelsV2e.AdcsPerProbe; k++)
                    {
                        amplifierBuffer[channelOrder[k, i], index] = (ushort)(gainCorrection * amplifierData[AdcIndicies[k] + adcDataOffset]);
                    }
                }
            }
        }

        // ADC & frame-index to channel mapping
        // First dimension: data index
        // Second dimension: frame index within super frame

        static readonly int[] AdcIndicies = {
            0, 1, 2,
            4, 5, 6,
            8, 9, 10,
            12, 13, 14,
            16, 17, 18,
            20, 21, 22,
            24, 25, 26,
            28, 29, 30
        };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct NeuropixelsV2Payload
    {
        public ulong HubClock;
        public ushort ProbeIndex;
        public ulong Reserved;
        public fixed ushort AmplifierData[NeuropixelsV2e.ChannelCount];
    }
}
