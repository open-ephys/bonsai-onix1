using System.IO;
using System.Linq;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Static helper class for NeuropixelsV2 methods.
    /// </summary>
    public class NeuropixelsV2Helper
    {
        /// <summary>
        /// Tries to parse the gain calibration file.
        /// </summary>
        /// <remarks>
        /// Checks if the given filename points to a real file, and also checks for the following elements:
        /// <para>
        /// 1) Serial number as the first line.
        /// </para>
        /// <para>
        /// 2) All remaining lines (384 lines) are an integer and a double value, separated by a comma.
        /// The integer defines the channel number, and the double is the gain calibration value for that channel.
        /// </para>
        /// </remarks>
        /// <param name="gainCalibrationFileName">String containing the path to the gain calibration file.</param>
        /// <returns><see cref="NeuropixelsV2GainCorrection"/> object that contains the gain correction value. This object is null if the file was not successfully parsed.</returns>
        public static NeuropixelsV2GainCorrection? TryParseGainCalibrationFile(string gainCalibrationFileName)
        {
            if (!File.Exists(gainCalibrationFileName)) return null;

            var lines = File.ReadLines(gainCalibrationFileName);

            if (lines.Count() != NeuropixelsV2.ChannelCount + 1) return null;
            if (!ulong.TryParse(lines.ElementAt(0), out var serialNumber)) return null;

            if (!lines
                .Skip(1)
                .Select(l =>
                {
                    var s = l.Split(',')[0];
                    var ok = int.TryParse(s, out var channel);
                    return (Ok: ok, Channel: channel);
                })
                .Where(l => l.Ok)
                .Select(l => l.Channel)
                .SequenceEqual(Enumerable.Range(0, NeuropixelsV2.ChannelCount))) return null;

            var gains = lines
                        .Skip(1)
                        .Select(l =>
                        {
                            var ok = double.TryParse(l.Split(',')[1], out var gainCorrection);
                            return (Ok: ok, GainCorrection: gainCorrection);
                        })
                        .Where(l => l.Ok)
                        .Select(l => l.GainCorrection)
                        .Distinct();

            return gains.Count() == 1 ? new(serialNumber, gains.First()) : null;
        }
    }
}
