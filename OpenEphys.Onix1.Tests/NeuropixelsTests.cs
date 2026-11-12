using System;
using OpenCV.Net;
using Xunit;

namespace OpenEphys.Onix1.Tests
{
    public class NeuropixelsTests
    {
        // NB: a comparator network sorts every input if it sorts every input of zeros and ones, so this covers
        // all inputs of each size.
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(11)]
        [InlineData(12)]
        [InlineData(13)]
        [InlineData(16)]
        public void SortingNetwork_SortsEveryInputOfZerosAndOnes(int count)
        {
            var network = Neuropixels.SortingNetwork(count);
            var values = new int[count];
            for (int bits = 0; bits < 1 << count; bits++)
            {
                for (int i = 0; i < count; i++)
                    values[i] = (bits >> i) & 1;

                foreach (var (low, high) in network)
                {
                    if (values[low] > values[high])
                        (values[low], values[high]) = (values[high], values[low]);
                }

                for (int i = 1; i < count; i++)
                    Assert.True(values[i - 1] <= values[i], $"{count} values {Convert.ToString(bits, 2)} are not sorted.");
            }
        }

        public static TheoryData<string> Probes => new() { "NeuropixelsV1", "NeuropixelsV2", "NeuropixelsV2Beta" };

        [Theory]
        [MemberData(nameof(Probes))]
        public unsafe void ApplyCmrF32_SubtractsEachGroupsMedian(string probe)
        {
            var groups = probe switch
            {
                "NeuropixelsV1" => NeuropixelsV1.AdcChannelGroups(),
                "NeuropixelsV2" => NeuropixelsV2.AdcChannelGroups(),
                _ => NeuropixelsV2Beta.AdcChannelGroups(),
            };
            const int Rows = 384;

            // NB: not a multiple of the vector width, so some samples are left over after the whole vectors.
            const int Samples = 37;

            var random = new Random(1);
            var values = new float[Rows, Samples];
            for (int row = 0; row < Rows; row++)
                for (int t = 0; t < Samples; t++)
                    values[row, t] = (float)Math.Round(random.NextDouble() * 400 - 200, 1);

            var expected = (float[,])values.Clone();
            foreach (var group in groups)
            {
                var sorted = new float[group.Length];
                for (int t = 0; t < Samples; t++)
                {
                    for (int g = 0; g < group.Length; g++)
                        sorted[g] = values[group[g], t];
                    Array.Sort(sorted);

                    int mid = sorted.Length / 2;
                    var median = sorted.Length % 2 == 1 ? sorted[mid] : 0.5f * (sorted[mid - 1] + sorted[mid]);
                    foreach (var row in group)
                        expected[row, t] -= median;
                }
            }

            using var input = new Mat(Rows, Samples, Depth.F32, 1);
            for (int row = 0; row < Rows; row++)
                for (int t = 0; t < Samples; t++)
                    ((float*)((byte*)input.Data + row * input.Step))[t] = values[row, t];

            var output = Neuropixels.ApplyCmrF32(input, groups);

            Assert.Same(input, output);
            for (int row = 0; row < Rows; row++)
                for (int t = 0; t < Samples; t++)
                    Assert.Equal(expected[row, t], ((float*)((byte*)output.Data + row * output.Step))[t]);
        }
    }
}
