using System;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenEphys.Onix1.Tests
{
    public sealed class NeuropixelsV1AdcCalibrationTests : IDisposable
    {
        const ulong SerialNumber = 21144102401;

        // NB: Same layout as an IMEC Neuropixels 1.0 ADC calibration file: the serial number, then one row per ADC
        // with the ADC number followed by CompP, CompN, Slope, Coarse, Fine, Cfix, Offset and Threshold. The values
        // are arbitrary but distinct per ADC, and include a negative one to cover the sign
        static string Row(int adc) => $"{adc},{adc + 1},{adc + 2},{adc % 4},{adc % 3},{adc + 10},{adc % 5},{-adc},{500 + adc}";

        readonly string directory = Path.Combine(Path.GetTempPath(), $"{nameof(NeuropixelsV1AdcCalibrationTests)}-{Guid.NewGuid():N}");

        public NeuropixelsV1AdcCalibrationTests() => Directory.CreateDirectory(directory);

        public void Dispose() => Directory.Delete(directory, recursive: true);

        string WriteFile(string[] rows)
        {
            var path = Path.Combine(directory, $"{Guid.NewGuid():N}_ADCCalibration.csv");
            File.WriteAllLines(path, new[] { SerialNumber.ToString() }.Concat(rows));
            return path;
        }

        static string[] Rows(int count) => Enumerable.Range(0, count).Select(Row).ToArray();

        static NeuropixelsV1AdcCalibration? Parse(string path) => NeuropixelsV1Helper.TryParseAdcCalibrationFile(path);

        static void AssertParsed(NeuropixelsV1AdcCalibration? calibration)
        {
            Assert.True(calibration.HasValue);
            Assert.Equal(SerialNumber, calibration.Value.SerialNumber);
            Assert.Equal(NeuropixelsV1.AdcCount, calibration.Value.Adcs.Length);
            for (int adc = 0; adc < NeuropixelsV1.AdcCount; adc++)
            {
                var a = calibration.Value.Adcs[adc];
                Assert.Equal(Row(adc), $"{adc},{a.CompP},{a.CompN},{a.Slope},{a.Coarse},{a.Fine},{a.Cfix},{a.Offset},{a.Threshold}");
            }
        }

        // The format the parser accepts, each value going to its ADC setting
        [Fact]
        public void OneRowPerAdc_IsAccepted()
        {
            AssertParsed(Parse(WriteFile(Rows(NeuropixelsV1.AdcCount))));
        }

        // Calibration files do not depend on the computer's regional settings
        [Theory]
        [MemberData(nameof(CultureScope.Names), MemberType = typeof(CultureScope))]
        public void Parse_AnyCulture_ReadsValues(string culture)
        {
            var path = WriteFile(Rows(NeuropixelsV1.AdcCount));
            using var scope = new CultureScope(culture);
            AssertParsed(Parse(path));
        }

        [Theory]
        [InlineData(NeuropixelsV1.AdcCount - 1)]
        [InlineData(NeuropixelsV1.AdcCount + 1)]
        public void WrongRowCount_IsRejected(int rowCount)
        {
            Assert.Null(Parse(WriteFile(Rows(rowCount))));
        }

        [Fact]
        public void RepeatedAdcNumber_IsRejected()
        {
            var rows = Rows(NeuropixelsV1.AdcCount);
            rows[3] = "2" + rows[3].Substring(1);
            Assert.Null(Parse(WriteFile(rows)));
        }

        [Fact]
        public void MissingValue_IsRejected()
        {
            var rows = Rows(NeuropixelsV1.AdcCount);
            rows[3] = rows[3].Substring(0, rows[3].LastIndexOf(','));
            Assert.Null(Parse(WriteFile(rows)));
        }

        [Fact]
        public void NonIntegerValue_IsRejected()
        {
            var rows = Rows(NeuropixelsV1.AdcCount);
            rows[3] = rows[3] + ".5";
            Assert.Null(Parse(WriteFile(rows)));
        }

        [Fact]
        public void MissingFile_IsRejected()
        {
            Assert.Null(Parse(Path.Combine(directory, "missing.csv")));
        }
    }
}
