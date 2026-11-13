using System;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenEphys.Onix1.Tests
{
    public sealed class NeuropixelsV1GainCalibrationTests : IDisposable
    {
        const ulong SerialNumber = 21144102401;

        // NB: A row from an IMEC UHD gain calibration file: 8 AP corrections then 8 LFP corrections, in
        // NeuropixelsV1Gain order
        const string Corrections = "1.060024, 1.060024, 1.066860, 1.036412, 1.052347, 1.052347, 1.052347, 1.052347, " +
                                   "0.990994, 0.990994, 0.997718, 0.969976, 0.991767, 0.991767, 0.991767, 0.991767";
        const string ReferenceCorrections = "1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1";
        const NeuropixelsV1Gain ApGain = NeuropixelsV1Gain.Gain1000;
        const NeuropixelsV1Gain LfpGain = NeuropixelsV1Gain.Gain50;
        const double ExpectedApCorrection = 1.052347;
        const double ExpectedLfpCorrection = 0.990994;

        readonly string directory = Path.Combine(Path.GetTempPath(), $"{nameof(NeuropixelsV1GainCalibrationTests)}-{Guid.NewGuid():N}");

        public NeuropixelsV1GainCalibrationTests() => Directory.CreateDirectory(directory);

        public void Dispose() => Directory.Delete(directory, recursive: true);

        string WriteFile(string[] rows)
        {
            var path = Path.Combine(directory, $"{Guid.NewGuid():N}_gainCalValues.csv");
            File.WriteAllLines(path, new[] { SerialNumber.ToString() }.Concat(rows));
            return path;
        }

        static string[] Rows(int count) => Rows(Enumerable.Range(0, count));

        static string[] Rows(System.Collections.Generic.IEnumerable<int> numbers) =>
            numbers.Select(i => $"{i}, {Corrections}").ToArray();

        static NeuropixelsV1eGainCorrection? Parse(string path, int electrodeCount) =>
            NeuropixelsV1Helper.TryParseGainCalibrationFile(path, ApGain, LfpGain, electrodeCount);

        static void AssertParsed(NeuropixelsV1eGainCorrection? correction)
        {
            Assert.True(correction.HasValue);
            Assert.Equal(SerialNumber, correction.Value.SerialNumber);
            Assert.Equal(ExpectedApCorrection, correction.Value.ApGainCorrectionFactor);
            Assert.Equal(ExpectedLfpCorrection, correction.Value.LfpGainCorrectionFactor);
        }

        // NB: One part number per distinct electrode count, so the electrode count is the one the register
        // contexts pass for that variant
        public static TheoryData<string> PartNumbers => new() { "NP1000", "NP1020", "NP1030", "NP1100", "NP1110" };

        [Theory]
        [MemberData(nameof(PartNumbers))]
        public void OneRowPerChannel_IsAccepted(string partNumber)
        {
            var electrodeCount = NeuropixelsV1VariantRegistry.Resolve(partNumber).ElectrodeCount;
            AssertParsed(Parse(WriteFile(Rows(NeuropixelsV1.ChannelCount)), electrodeCount));
        }

        [Theory]
        [MemberData(nameof(PartNumbers))]
        public void OneRowPerElectrode_IsAccepted(string partNumber)
        {
            var electrodeCount = NeuropixelsV1VariantRegistry.Resolve(partNumber).ElectrodeCount;
            AssertParsed(Parse(WriteFile(Rows(electrodeCount)), electrodeCount));
        }

        [Fact]
        public void OneRowPerChannel_Nric1384_IsAccepted()
        {
            AssertParsed(Parse(WriteFile(Rows(Nric1384.ChannelCount)), Nric1384.ElectrodeCount));
        }

        [Fact]
        public void ReferenceRows_AreIgnored()
        {
            var rows = Enumerable.Range(0, NeuropixelsV1.ElectrodeCount)
                .Select(i => $"{i}, {(NeuropixelsV1.IsInternalReferenceContact(i) ? ReferenceCorrections : Corrections)}")
                .ToArray();

            AssertParsed(Parse(WriteFile(rows), NeuropixelsV1.ElectrodeCount));
        }

        [Theory]
        [InlineData(NeuropixelsV1.ChannelCount - 1)]
        [InlineData(NeuropixelsV1.ChannelCount + 1)]
        [InlineData(NeuropixelsV1.ElectrodeCount - 1)]
        [InlineData(NeuropixelsV1.ElectrodeCount + 1)]
        [InlineData(2496)]
        public void WrongRowCount_IsRejected(int rowCount)
        {
            Assert.Null(Parse(WriteFile(Rows(rowCount)), NeuropixelsV1.ElectrodeCount));
        }

        [Fact]
        public void OneRowPerElectrode_ForAnotherVariant_IsRejected()
        {
            var electrodeCount = NeuropixelsV1VariantRegistry.Resolve("NP1100").ElectrodeCount;
            Assert.Null(Parse(WriteFile(Rows(NeuropixelsV1.ElectrodeCount)), electrodeCount));
        }

        [Fact]
        public void RowsNumberedFromOne_AreRejected()
        {
            Assert.Null(Parse(WriteFile(Rows(Enumerable.Range(1, NeuropixelsV1.ChannelCount))), NeuropixelsV1.ElectrodeCount));
        }

        [Fact]
        public void RepeatedRowNumber_IsRejected()
        {
            var numbers = Enumerable.Range(0, NeuropixelsV1.ChannelCount).Select(i => i == 10 ? 9 : i);
            Assert.Null(Parse(WriteFile(Rows(numbers)), NeuropixelsV1.ElectrodeCount));
        }

        [Fact]
        public void PerChannelNumberingInPerElectrodeFile_IsRejected()
        {
            var numbers = Enumerable.Range(0, NeuropixelsV1.ElectrodeCount).Select(i => i % NeuropixelsV1.ChannelCount);
            Assert.Null(Parse(WriteFile(Rows(numbers)), NeuropixelsV1.ElectrodeCount));
        }

        [Fact]
        public void NonIntegerRowNumber_IsRejected()
        {
            var rows = Rows(NeuropixelsV1.ChannelCount);
            rows[7] = $"x, {Corrections}";
            Assert.Null(Parse(WriteFile(rows), NeuropixelsV1.ElectrodeCount));
        }

        [Fact]
        public void DifferingCorrection_IsRejected()
        {
            var rows = Rows(NeuropixelsV1.ChannelCount);
            rows[5] = $"5, {Corrections.Replace("1.052347", "1.100000")}";
            Assert.Null(Parse(WriteFile(rows), NeuropixelsV1.ElectrodeCount));
        }

        [Fact]
        public void MissingFile_IsRejected()
        {
            Assert.Null(Parse(Path.Combine(directory, "missing.csv"), NeuropixelsV1.ElectrodeCount));
        }
    }
}
