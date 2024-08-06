using System.Runtime.InteropServices;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Buffered data from a NeuropixelsV1e device.
    /// </summary>
    public class NeuropixelsV1eDataFrame : BufferedDataFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV1eDataFrame"/> class.
        /// </summary>
        /// <param name="clock">An array of <see cref="DataFrame.Clock"/> values.</param>
        /// <param name="hubClock">An array of hub clock counter values.</param>
        /// <param name="frameCount">An array of frame count values.</param>
        /// <param name="spikeData">An array of multi-channel spike data as a <see cref="Mat"/> object.</param>
        /// <param name="lfpData">An array of multi-channel LFP data as a <see cref="Mat"/> object.</param>
        public NeuropixelsV1eDataFrame(ulong[] clock, ulong[] hubClock, int[] frameCount, Mat spikeData, Mat lfpData)
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
        /// Frame count is a 20-bit counter on the probe that increments its value for every frame produced.
        /// The value ranges from 0 to 1048575 (2^20-1), and should always increment by 1 until it wraps around back to 0.
        /// This can be used to detect dropped frames.
        /// </remarks>
        public int[] FrameCount { get; }

        /// <summary>
        /// Gets the spike-band data as a <see cref="Mat"/> object.
        /// </summary>
        /// <remarks>
        /// Spike-band data has 384 rows (channels) with columns representing the samples acquired at 30 kHz. Each sample is a
        /// 10-bit offset binary encoded as an unsigned short value.
        /// </remarks>
        public Mat SpikeData { get; }

        /// <summary>
        /// Gets the LFP band data as a <see cref="Mat"/> object.
        /// </summary>
        /// <remarks>
        /// LFP data has 32 rows (channels) with columns representing the samples acquired at 2.5 kHz. Each sample is a
        /// 10-bit offset binary encoded as an unsigned short value.
        /// </remarks>
        public Mat LfpData { get; }

        internal static unsafe void CopyAmplifierBuffer(ushort* amplifierData, int[] frameCountBuffer, ushort[,] spikeBuffer, ushort[,] lfpBuffer, int index, double apGainCorrection, double lfpGainCorrection, ushort[] thresholds, ushort[] offsets)
        {

            var frameCountStartIndex = index * NeuropixelsV1e.FramesPerSuperFrame;
            frameCountBuffer[frameCountStartIndex] =   (amplifierData[31] << 10) | (amplifierData[39] << 0);

            // Single LFP frame
            // The period of ADC data within data array is 36 words
            var lfpBufferIndex = index / 12;
            var lfpFrameIndex = index % 12;

            for (int k = 0; k < NeuropixelsV1e.AdcCount; k++)
            {
                var a = amplifierData[adcToFrameIndex[k]];
                lfpBuffer[RawToChannel[k, lfpFrameIndex], lfpBufferIndex] = (ushort)(lfpGainCorrection * (a > thresholds[k] ? a - offsets[k] : a));
            }


            // Loop over 12 AP frames within each "super-frame"
            for (int i = 0; i < NeuropixelsV1e.FramesPerRoundRobin; i++)
            {
                // The period of ADC data within data array is 36 words
                var adcDataOffset = (i + 1) * NeuropixelsV1e.FrameWords;

                for (int k = 0; k < NeuropixelsV1e.AdcCount; k++)
                {
                    var a = amplifierData[adcToFrameIndex[k] + adcDataOffset];
                    spikeBuffer[RawToChannel[k, i], index] = (ushort)(apGainCorrection * (a > thresholds[k] ? a - offsets[k] : a));
                }

                frameCountBuffer[frameCountStartIndex + i + 1] = (amplifierData[adcDataOffset + 31] << 10) | (amplifierData[adcDataOffset + 39] << 0);
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

        // ADC to channel
        // First dimension: ADC index
        // Second dimension: frame index within super frame
        // Output: channel number
        static readonly int[,] RawToChannel = {
            {0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22 },
            {1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23 },
            {24, 26, 28, 30, 32, 34, 36, 38, 40, 42, 44, 46 },
            {25, 27, 29, 31, 33, 35, 37, 39, 41, 43, 45, 47 },
            {48, 50, 52, 54, 56, 58, 60, 62, 64, 66, 68, 70 },
            {49, 51, 53, 55, 57, 59, 61, 63, 65, 67, 69, 71 },
            {72, 74, 76, 78, 80, 82, 84, 86, 88, 90, 92, 94 },
            {73, 75, 77, 79, 81, 83, 85, 87, 89, 91, 93, 95 },
            {96, 98, 100, 102, 104, 106, 108, 110, 112, 114, 116, 118 },
            {97, 99, 101, 103, 105, 107, 109, 111, 113, 115, 117, 119 },
            {120, 122, 124, 126, 128, 130, 132, 134, 136, 138, 140, 142 },
            {121, 123, 125, 127, 129, 131, 133, 135, 137, 139, 141, 143 },
            {144, 146, 148, 150, 152, 154, 156, 158, 160, 162, 164, 166 },
            {145, 147, 149, 151, 153, 155, 157, 159, 161, 163, 165, 167 },
            {168, 170, 172, 174, 176, 178, 180, 182, 184, 186, 188, 190 },
            {169, 171, 173, 175, 177, 179, 181, 183, 185, 187, 189, 191 },
            {192, 194, 196, 198, 200, 202, 204, 206, 208, 210, 212, 214 },
            {193, 195, 197, 199, 201, 203, 205, 207, 209, 211, 213, 215 },
            {216, 218, 220, 222, 224, 226, 228, 230, 232, 234, 236, 238 },
            {217, 219, 221, 223, 225, 227, 229, 231, 233, 235, 237, 239 },
            {240, 242, 244, 246, 248, 250, 252, 254, 256, 258, 260, 262 },
            {241, 243, 245, 247, 249, 251, 253, 255, 257, 259, 261, 263 },
            {264, 266, 268, 270, 272, 274, 276, 278, 280, 282, 284, 286 },
            {265, 267, 269, 271, 273, 275, 277, 279, 281, 283, 285, 287 },
            {288, 290, 292, 294, 296, 298, 300, 302, 304, 306, 308, 310 },
            {289, 291, 293, 295, 297, 299, 301, 303, 305, 307, 309, 311 },
            {312, 314, 316, 318, 320, 322, 324, 326, 328, 330, 332, 334 },
            {313, 315, 317, 319, 321, 323, 325, 327, 329, 331, 333, 335 },
            {336, 338, 340, 342, 344, 346, 348, 350, 352, 354, 356, 358 },
            {337, 339, 341, 343, 345, 347, 349, 351, 353, 355, 357, 359 },
            {360, 362, 364, 366, 368, 370, 372, 374, 376, 378, 380, 382 },
            {361, 363, 365, 367, 369, 371, 373, 375, 377, 379, 381, 383 } };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct NeuropixelsV1ePayload
    {
        public ulong HubClock;
        public ushort ProbeIndex;
        public fixed ushort AmplifierData[NeuropixelsV1e.FrameWords * NeuropixelsV1e.FramesPerSuperFrame];
    }
}
