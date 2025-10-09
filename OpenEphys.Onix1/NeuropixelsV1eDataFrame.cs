using System.Runtime.InteropServices;

namespace OpenEphys.Onix1
{
    class NeuropixelsV1eDataFrame
    {
        internal static unsafe void CopyAmplifierBuffer(ushort* amplifierData, int[] frameCountBuffer, ushort[,] spikeBuffer, ushort[,] lfpBuffer, int index, double apGainCorrection, double lfpGainCorrection, ushort[] thresholds, ushort[] offsets, bool invertPolarity, int[,] channelOrder)
        {
            var frameCountStartIndex = index * NeuropixelsV1.FramesPerSuperFrame;
            frameCountBuffer[frameCountStartIndex] = (amplifierData[31] << 10) | (amplifierData[39] << 0);

            // Single LFP frame
            // The period of ADC data within data array is 36 words
            var lfpBufferIndex = index / NeuropixelsV1.FramesPerRoundRobin;
            var lfpFrameIndex = index % NeuropixelsV1.FramesPerRoundRobin;

            if (invertPolarity)
            {
                const double NumberOfAdcBins = 1024;
                double lfpInversionOffset = lfpGainCorrection * NumberOfAdcBins;
                double apInversionOffset = apGainCorrection * NumberOfAdcBins;

                for (int k = 0; k < NeuropixelsV1.AdcCount; k++)
                {
                    var a = amplifierData[adcToFrameIndex[k]];
                    lfpBuffer[channelOrder[k, lfpFrameIndex], lfpBufferIndex] = (ushort)(lfpInversionOffset - lfpGainCorrection * (a > thresholds[k] ? a - offsets[k] : a));
                }

                // Loop over 12 AP frames within each "super-frame"
                for (int i = 0; i < NeuropixelsV1.FramesPerRoundRobin; i++)
                {
                    // The period of ADC data within data array is 36 words
                    var adcDataOffset = (i + 1) * NeuropixelsV1.FrameWords;

                    for (int k = 0; k < NeuropixelsV1.AdcCount; k++)
                    {
                        var a = amplifierData[adcToFrameIndex[k] + adcDataOffset];
                        spikeBuffer[channelOrder[k, i], index] = (ushort)(apInversionOffset - apGainCorrection * (a > thresholds[k] ? a - offsets[k] : a));
                    }

                    frameCountBuffer[frameCountStartIndex + i + 1] = (amplifierData[adcDataOffset + 31] << 10) | (amplifierData[adcDataOffset + 39] << 0);
                }
            }
            else
            {
                for (int k = 0; k < NeuropixelsV1.AdcCount; k++)
                {
                    var a = amplifierData[adcToFrameIndex[k]];
                    lfpBuffer[channelOrder[k, lfpFrameIndex], lfpBufferIndex] = (ushort)(lfpGainCorrection * (a > thresholds[k] ? a - offsets[k] : a));
                }

                // Loop over 12 AP frames within each "super-frame"
                for (int i = 0; i < NeuropixelsV1.FramesPerRoundRobin; i++)
                {
                    // The period of ADC data within data array is 36 words
                    var adcDataOffset = (i + 1) * NeuropixelsV1.FrameWords;

                    for (int k = 0; k < NeuropixelsV1.AdcCount; k++)
                    {
                        var a = amplifierData[adcToFrameIndex[k] + adcDataOffset];
                        spikeBuffer[channelOrder[k, i], index] = (ushort)(apGainCorrection * (a > thresholds[k] ? a - offsets[k] : a));
                    }

                    frameCountBuffer[frameCountStartIndex + i + 1] = (amplifierData[adcDataOffset + 31] << 10) | (amplifierData[adcDataOffset + 39] << 0);
                }
            }
        }

        // ADC to frame index
        // Input: ADC index
        // Output: index of ADC's data within a frame
        static readonly int[] adcToFrameIndex = {1, 9 , 17, 25, 33,
                                                 2, 10, 18, 26, 34,
                                                 3, 11, 19, 27, 35,
                                                 4, 12, 20, 28, 36,
                                                 5, 13, 21, 29, 37,
                                                 6, 14, 22, 30, 38,
                                                 7, 15 };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct NeuropixelsV1ePayload
    {
        public ulong HubClock;
        public ushort ProbeIndex;
        public fixed ushort AmplifierData[NeuropixelsV1.FrameWords * NeuropixelsV1.FramesPerSuperFrame];
    }
}
