using System;
﻿using System.Collections.Generic;
using System.Linq;
using OpenCV.Net;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Provides shared signal-processing utilities used across Neuropixels probe versions.
    /// </summary>
    static class Neuropixels
    {
        internal static int[,] OrderChannelsByDepth(Electrode[] channelMap, int[,] rawToChannel)
        {
            int adcIndices = rawToChannel.GetLength(0);
            int frameIndices = rawToChannel.GetLength(1);

            // NB: Create reverse lookup table where the channel number is used to find the ADC index / frame index
            var channelToPosition = new Dictionary<int, (int adcIndex, int frameIndex)>();
            for (int adc = 0; adc < adcIndices; adc++)
            {
                for (int frame = 0; frame < frameIndices; frame++)
                {
                    channelToPosition[rawToChannel[adc, frame]] = (adc, frame);
                }
            }

            var spatiallyOrdered = channelMap
                .OrderBy(x => x.Position.Y)
                .ThenBy(x => x.Position.X)
                .ToArray();

            // NB: Populate the array with the spatially ordered channel indices by grabbing the original ADC index /
            //     frame index for that electrode channel number, and writing the new channel number at that index.
            //     Example:
            //       rawToChannel        = [0, 2, 4; 1, 3, 5] // Channels are in one column, in order 0 -> 2 -> 4 -> 1 -> 3 -> 5
            //       spatialRawToChannel = [0, 1, 2; 3, 4, 5]
            //
            //       Now, channel 2 is at index 1 in the data frame, channel 4 is index 2, channel 1 is index 3, etc.
            var spatialRawToChannel = new int[adcIndices, frameIndices];
            int index = 0;

            foreach (var e in spatiallyOrdered)
            {
                var (origAdcIndex, origFrameIndex) = channelToPosition[e.Channel];

                spatialRawToChannel[origAdcIndex, origFrameIndex] = index++;
            }

            return spatialRawToChannel;
        }

    /// <summary>
        /// Applies per-ADC group common median referencing (CMR) in-place to a <see cref="Depth.F32"/>
        /// matrix.
    /// </summary>
        /// <remarks>
        /// For each ADC group and each time sample, the group median is computed and subtracted from every
        /// channel in that group.
        /// </remarks>
        /// <param name="input">
        /// A <see cref="Depth.F32"/> matrix with one row per channel and one column per sample. Modified in
        /// place.
        /// </param>
        /// <param name="groups">
        /// The ADC channel groups. Each inner array lists the row indices belonging to one ADC.
        /// </param>
        /// <returns>
        /// The same <paramref name="input"/> matrix, modified in place, for convenient chaining.
        /// </returns>
        internal static unsafe Mat ApplyCmr(Mat input, int[][] groups)
        {
            int samples = input.Cols;
            int step = input.Step;
            byte* ptr = (byte*)input.Data.ToPointer();

            foreach (var group in groups)
            {
                var tmp = new float[group.Length];
                for (int t = 0; t < samples; t++)
    {
                    for (int g = 0; g < group.Length; g++)
                        tmp[g] = *(float*)(ptr + group[g] * step + t * sizeof(float));

                    Array.Sort(tmp);
                    int mid = group.Length / 2;
                    float median = (group.Length & 1) == 1
                        ? tmp[mid]
                        : 0.5f * (tmp[mid - 1] + tmp[mid]);

                    foreach (int ch in group)
                        *(float*)(ptr + ch * step + t * sizeof(float)) -= median;
                }
            }

            return input;
        }
    }
}
