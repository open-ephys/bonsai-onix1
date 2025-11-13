using System.Runtime.InteropServices;
namespace OpenEphys.Onix1
{
    class NeuropixelsV1fDataFrame
    {
        internal static unsafe void CopyAmplifierBuffer(ushort* amplifierData, int[] frameCountBuffer, ushort[,] spikeBuffer, ushort[,] lfpBuffer, int index, int[,] channelOrder)
        {
            var frameCountStartIndex = index * NeuropixelsV1.FramesPerSuperFrame;
            frameCountBuffer[frameCountStartIndex] = (amplifierData[FrameCounterMsbIndex] << 10) | (amplifierData[FrameCounterLsbIndex] << 0);

            // Single LFP frame
            // The period of ADC data within data array is 36 words
            var lfpBufferIndex = index / NeuropixelsV1.FramesPerRoundRobin;
            var lfpFrameIndex = index % NeuropixelsV1.FramesPerRoundRobin;

            for (int k = 0; k < NeuropixelsV1.AdcCount; k++)
            {
                // TODO: Why would I not do this bit shift ont the FPGA??
                lfpBuffer[channelOrder[k, lfpFrameIndex], lfpBufferIndex] = (ushort)(amplifierData[AdcToFrameIndex[k]] >> 5); // Q11.5 -> Q11.0
            }

            // Loop over 12 AP frames within each "super-frame"
            for (int i = 0; i < NeuropixelsV1.FramesPerRoundRobin; i++)
            {
                // The period of ADC data within data array is 36 words
                var adcDataOffset = (i + 1) * NeuropixelsV1f.WordsPerFrame;

                for (int k = 0; k < NeuropixelsV1.AdcCount; k++)
                {
                    spikeBuffer[channelOrder[k, i], index] = (ushort)(amplifierData[AdcToFrameIndex[k] + adcDataOffset] >> 5); // Q11.5 -> Q11.0
                }

                frameCountBuffer[frameCountStartIndex + i + 1] =
                    (amplifierData[adcDataOffset + FrameCounterMsbIndex] << 10) | (amplifierData[adcDataOffset + FrameCounterLsbIndex] << 0);
            }
        }

        const int FrameCounterMsbIndex = 28;
        const int FrameCounterLsbIndex = 35;

        // ADC to frame index
        // Input: ADC index
        // Output: index of ADC's data within a frame
        static readonly int[] AdcToFrameIndex = {1, 8, 15, 22, 29,
                                                 2, 9, 16, 23, 30,
                                                 3, 10, 17, 24, 31,
                                                 4, 11, 18, 25, 32,
                                                 5, 12, 19, 26, 33,
                                                 6, 13, 20, 27, 34,
                                                 7, 14};
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct NeuropixelsV1fPayload
    {
        public fixed ushort AmplifierData[NeuropixelsV1f.WordsPerFrame * NeuropixelsV1.FramesPerSuperFrame];
        public ulong HubClock;
    }
}
