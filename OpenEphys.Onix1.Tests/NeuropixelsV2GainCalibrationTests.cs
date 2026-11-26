using System;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenEphys.Onix1.Tests
{
    public sealed class NeuropixelsV2GainCalibrationTests : IDisposable
    {
        const ulong SerialNumber = 22420002374;

        // NB: Same layout as an IMEC Neuropixels 2.0 gain calibration file: the serial number, then one
        // "channel,correction" row per channel, all with the same correction
        const string Correction = "0.956554";
        const double ExpectedCorrection = 0.956554;

        readonly string directory = Path.Combine(Path.GetTempPath(), $"{nameof(NeuropixelsV2GainCalibrationTests)}-{Guid.NewGuid():N}");

        public NeuropixelsV2GainCalibrationTests() => Directory.CreateDirectory(directory);

        public void Dispose() => Directory.Delete(directory, recursive: true);

        string WriteFile(string[] rows)
        {
            var path = Path.Combine(directory, $"{Guid.NewGuid():N}_gainCalValues.csv");
            File.WriteAllLines(path, new[] { SerialNumber.ToString() }.Concat(rows));
            return path;
        }

        static string[] Rows(int count) => Enumerable.Range(0, count).Select(i => $"{i},{Correction}").ToArray();

        static NeuropixelsV2GainCorrection? Parse(string path) => NeuropixelsV2Helper.TryParseGainCalibrationFile(path);

        static void AssertParsed(NeuropixelsV2GainCorrection? correction)
        {
            Assert.True(correction.HasValue);
            Assert.Equal(SerialNumber, correction.Value.SerialNumber);
            Assert.Equal(ExpectedCorrection, correction.Value.GainCorrectionFactor);
        }

        // The format the parser accepts
        [Fact]
        public void OneRowPerChannel_IsAccepted()
        {
            AssertParsed(Parse(WriteFile(Rows(NeuropixelsV2.ChannelCount))));
        }

        // Calibration files always use '.' as the decimal separator, whatever the computer's regional settings.
        // Parsing them with the current culture read 0.956554 as 956554 (es-ES, de-DE) or rejected them (fr-FR)
        [Theory]
        [MemberData(nameof(CultureScope.Names), MemberType = typeof(CultureScope))]
        public void Parse_AnyCulture_ReadsCorrection(string culture)
        {
            var path = WriteFile(Rows(NeuropixelsV2.ChannelCount));
            using var scope = new CultureScope(culture);
            AssertParsed(Parse(path));
        }

        // Windows users can change the separators of their culture in the regional settings
        [Fact]
        public void Parse_CustomDecimalSeparator_ReadsCorrection()
        {
            var path = WriteFile(Rows(NeuropixelsV2.ChannelCount));
            using var scope = new CultureScope(CultureScope.CustomSeparators());
            AssertParsed(Parse(path));
        }

        [Theory]
        [InlineData(NeuropixelsV2.ChannelCount - 1)]
        [InlineData(NeuropixelsV2.ChannelCount + 1)]
        public void WrongRowCount_IsRejected(int rowCount)
        {
            Assert.Null(Parse(WriteFile(Rows(rowCount))));
        }

        [Fact]
        public void RepeatedChannelNumber_IsRejected()
        {
            var rows = Rows(NeuropixelsV2.ChannelCount);
            rows[10] = $"9,{Correction}";
            Assert.Null(Parse(WriteFile(rows)));
        }

        [Fact]
        public void NonNumericSerialNumber_IsRejected()
        {
            var path = WriteFile(Rows(NeuropixelsV2.ChannelCount));
            File.WriteAllLines(path, new[] { "serial" }.Concat(File.ReadLines(path).Skip(1)).ToArray());
            Assert.Null(Parse(path));
        }

        // A probe has a single gain correction: a file with two is not a valid calibration file
        [Fact]
        public void DifferingCorrection_IsRejected()
        {
            var rows = Rows(NeuropixelsV2.ChannelCount);
            rows[5] = "5,1.000000";
            Assert.Null(Parse(WriteFile(rows)));
        }

        // A row whose correction cannot be read used to be skipped silently, so a damaged file was accepted
        [Fact]
        public void UnparsableCorrection_IsRejected()
        {
            var rows = Rows(NeuropixelsV2.ChannelCount);
            rows[5] = "5,x";
            Assert.Null(Parse(WriteFile(rows)));
        }

        // A row without a correction used to throw IndexOutOfRangeException instead of rejecting the file
        [Fact]
        public void MissingCorrection_IsRejected()
        {
            var rows = Rows(NeuropixelsV2.ChannelCount);
            rows[5] = "5";
            Assert.Null(Parse(WriteFile(rows)));
        }

        [Fact]
        public void MissingFile_IsRejected()
        {
            Assert.Null(Parse(Path.Combine(directory, "missing.csv")));
        }
    }
}
