using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Reorders a raw-to-channel index mapping so that channels are numbered by increasing depth
        /// (primary) and lateral position (secondary).
        /// </summary>
        /// <param name="probe">
        /// A probeinterface probe group containing the channel map and spatial contact informatiion.
        /// </param>
        /// <param name="rawToChannel">
        /// A 2-D array [ADC index, frame index] mapping raw data positions to channel numbers, as produced by
        /// the probe decoder.
        /// </param>
        /// <returns>
        /// A new 2-D array with the same dimensions as <paramref name="rawToChannel"/> in which the values
        /// are reassigned so that channel 0 corresponds to the contact nearest the probe tip and channel
        /// indices increase toward the surface.
        /// </returns>
        internal static int[,] OrderChannelsByDepth(
            SingleProbeGroup probe,
            int[,] rawToChannel)
        {
            int adcIndices = rawToChannel.GetLength(0);
            int frameIndices = rawToChannel.GetLength(1);

            // NB: Create reverse lookup table where the channel number is used to find the ADC index / frame index
            var channelToDataIndex = new Dictionary<int, (int adcIndex, int frameIndex)>();
            for (int adc = 0; adc < adcIndices; adc++)
            {
                for (int frame = 0; frame < frameIndices; frame++)
                {
                    channelToDataIndex[rawToChannel[adc, frame]] = (adc, frame);
                }
            }

            var spatiallyOrderedChannels = probe.ChannelMap
                .Select(x => new KeyValuePair<int, Contact>(x.Key, probe.Probe.Contacts[x.Value]))
                .OrderBy(x => x.Value.PosY)
                .ThenBy(x => x.Value.PosX)
                .Select(x => x.Key);


            // NB: Populate the array with the spatially ordered channel indices by grabbing the original ADC index /
            //     frame index for that electrode channel number, and writing the new channel number at that index.
            //     Example:
            //       rawToChannel        = [0, 2, 4; 1, 3, 5] // Channels are in one column, in order 0 -> 2 -> 4 -> 1 -> 3 -> 5
            //       spatialRawToChannel = [0, 1, 2; 3, 4, 5]
            //
            //       Now, channel 2 is at index 1 in the data frame, channel 4 is index 2, channel 1 is index 3, etc.
            var spatialRawToChannel = new int[adcIndices, frameIndices];
            int index = 0;

            foreach (var c in spatiallyOrderedChannels)
            {
                var (origAdcIndex, origFrameIndex) = channelToDataIndex[c];
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
        /// <exception cref="ArgumentException">
        /// <paramref name="input"/> is not <see cref="Depth.F32"/>.
        /// </exception>
        internal static unsafe Mat ApplyCmrF32(Mat input, int[][] groups)
        {
            if (input.Depth != Depth.F32)
                throw new ArgumentException($"Expected {nameof(Depth.F32)} matrix but got {input.Depth}.", nameof(input));

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

        /// <summary>
        /// Returns the starting contact index of the <paramref name="bankWidth"/>-wide window for bank
        /// <paramref name="bankIndex"/>, right-aligned against <paramref name="totalElectrodes"/> for the
        /// last bank rather than starting a full <paramref name="bankWidth"/> past the previous one.
        /// </summary>
        /// <remarks>
        /// Shared by both Neuropixels 1.0 (<see cref="NeuropixelsV1ProbeGroup"/>) and 2.0
        /// (<see cref="NeuropixelsV2ProbeGroup"/>): both ASICs read out a fixed-width channel window per
        /// bank, and the last bank on any probe whose electrode count isn't an exact multiple of that
        /// width reuses electrodes from the previous bank to fill out a full window, rather than being
        /// partially populated.
        /// </remarks>
        /// <param name="bankIndex">The zero-based bank index.</param>
        /// <param name="totalElectrodes">The total number of electrodes on the probe (or per shank).</param>
        /// <param name="bankWidth">The number of contacts in a full bank window.</param>
        internal static int BankWindowStart(int bankIndex, int totalElectrodes, int bankWidth) =>
            Math.Min(bankIndex * bankWidth, totalElectrodes - bankWidth);

        static readonly char[] SkipLetters = { 'I', 'O', 'Q', 'S', 'X', 'Z' };

        /// <summary>
        /// Returns the display letter for a zero-based bank index (0 → "A", 1 → "B", ...).
        /// </summary>
        /// <remarks>
        /// Skips I, O, Q, S, X, Z (standard JEDEC/IPC convention). Handles double-letter rows (AA, AB, ...)
        /// once you exceed 20 rows, same way spreadsheet columns roll over
        /// </remarks>
        /// <param name="bankIndex">The zero-based bank index.</param>
        internal static string BankDisplayName(int bankIndex) // 0-based
        {
            var valid = new List<char>();
            for (char c = 'A'; c <= 'Z'; c++)
                if (!SkipLetters.Contains(c)) valid.Add(c);

            int n = valid.Count; // 20
            int idx = bankIndex;
            string label = "";

            do
            {
                label = valid[idx % n] + label;
                idx = idx / n - 1;
            } while (idx >= 0);

            return label;
        }
    }
}
