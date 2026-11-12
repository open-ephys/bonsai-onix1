using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenCV.Net;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Provides shared signal-processing utilities used across Neuropixels probe versions.
    /// </summary>
    static class Neuropixels
    {
        /// <summary>
        /// Applies per-ADC group common median referencing (CMR) in-place to a <see cref="Depth.F32"/>
        /// matrix.
        /// </summary>
        /// <remarks>
        /// For each ADC group and each time sample, the group median is computed and subtracted from every
        /// channel in that group. The medians come from a sorting network applied to several samples at once.
        /// <para>
        /// This depends on JIT optimization. In an optimized build, each vector operation compiles to a single SIMD
        /// instruction, and this is about 7 times faster than sorting each sample with <see cref="Array.Sort(Array)"/>.
        /// A Debug build disables JIT optimization for the whole assembly, so each vector operation becomes a method
        /// call that copies its arguments, and this becomes about 3 times slower than sorting each sample. That is
        /// slow enough for a live display to fall behind, so measure the speed of this method only in a Release
        /// build.
        /// </para>
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
            int width = Vector<float>.Count;
            var half = new Vector<float>(0.5f);

            int largest = 0;
            foreach (var group in groups)
                largest = Math.Max(largest, group.Length);
            var vectors = stackalloc Vector<float>[largest];
            var values = stackalloc float[largest];

            foreach (var group in groups)
            {
                int count = group.Length;
                if (count == 0)
                    continue;

                var network = SortingNetwork(count);
                int mid = count / 2;
                bool odd = (count & 1) == 1;

                // NB: a row holds one channel's samples in time order, so a vector read from it holds one channel's
                // next few samples, and sorting the group's vectors finds the median of each of those samples at once.
                int t = 0;
                for (; t + width <= samples; t += width)
                {
                    for (int g = 0; g < count; g++)
                        vectors[g] = *(Vector<float>*)(ptr + group[g] * step + t * sizeof(float));

                    foreach (var (low, high) in network)
                    {
                        var smaller = Vector.Min(vectors[low], vectors[high]);
                        vectors[high] = Vector.Max(vectors[low], vectors[high]);
                        vectors[low] = smaller;
                    }

                    var median = odd ? vectors[mid] : half * (vectors[mid - 1] + vectors[mid]);
                    for (int g = 0; g < count; g++)
                        *(Vector<float>*)(ptr + group[g] * step + t * sizeof(float)) -= median;
                }

                for (; t < samples; t++)
                {
                    for (int g = 0; g < count; g++)
                        values[g] = *(float*)(ptr + group[g] * step + t * sizeof(float));

                    foreach (var (low, high) in network)
                    {
                        var smaller = Math.Min(values[low], values[high]);
                        values[high] = Math.Max(values[low], values[high]);
                        values[low] = smaller;
                    }

                    float median = odd ? values[mid] : 0.5f * (values[mid - 1] + values[mid]);
                    for (int g = 0; g < count; g++)
                        *(float*)(ptr + group[g] * step + t * sizeof(float)) -= median;
                }
            }

            return input;
        }

        static readonly ConcurrentDictionary<int, (int Low, int High)[]> SortingNetworks = new();

        /// <summary>
        /// Returns the compare-exchange pairs that sort <paramref name="count"/> values into ascending order, in the
        /// order they are applied.
        /// </summary>
        /// <remarks>
        /// Batcher's odd-even merge sort, from K. E. Batcher, "Sorting networks and their applications", Proceedings
        /// of the AFIPS Spring Joint Computer Conference, 1968, pp. 307-314. It is defined for a power of two. The
        /// pairs that would reach past the last value are left out, which sorts the same as padding the values with
        /// ones larger than any of them.
        /// </remarks>
        /// <param name="count">The number of values to sort.</param>
        internal static (int Low, int High)[] SortingNetwork(int count) =>
            SortingNetworks.GetOrAdd(count, n =>
            {
                var pairs = new List<(int, int)>();
                for (int p = 1; p < n; p *= 2)
                    for (int k = p; k >= 1; k /= 2)
                        for (int j = k % p; j <= n - 1 - k; j += 2 * k)
                            for (int i = 0; i <= Math.Min(k - 1, n - j - k - 1); i++)
                                if ((i + j) / (p * 2) == (i + j + k) / (p * 2))
                                    pairs.Add((i + j, i + j + k));
                return pairs.ToArray();
            });

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

        /// <summary>
        /// Creates the exception thrown when an enabled probe's flex EEPROM cannot be read.
        /// </summary>
        /// <param name="deviceName">The name of the probe's device.</param>
        internal static InvalidOperationException ProbeNotFoundException(string deviceName) =>
            new($"Could not communicate with probe \"{deviceName}\". Check that the flex cable is " +
                "properly seated. If this probe is not in use, set its Enable property to false.");

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
