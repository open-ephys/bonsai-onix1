using System;
using System.IO;
using System.Linq;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Static helper class for NeuropixelsV2 methods.
    /// </summary>
    internal class NeuropixelsV2Helper
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
        /// <param name="gainCalibrationFile">String containing the path to the gain calibration file.</param>
        /// <returns><see cref="NeuropixelsV2GainCorrection"/> object that contains the gain correction value. This object is null if the file was not successfully parsed.</returns>
        internal static NeuropixelsV2GainCorrection? TryParseGainCalibrationFile(string gainCalibrationFile)
        {
            if (!File.Exists(gainCalibrationFile)) return null;

            var lines = File.ReadLines(gainCalibrationFile);

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

        /// <summary>
        /// Checks that a probe part number matches the model name annotation carried by the loaded
        /// probe interface data.
        /// </summary>
        /// <param name="partNumber">The probe part number to check.</param>
        /// <param name="probeGroup">The loaded probe group whose model name annotation should be
        /// checked against <paramref name="partNumber"/>.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="partNumber"/> is null or
        /// empty, or if it does not match the probe interface file's model name annotation.</exception>
        internal static void ValidateProbePartNumber(string partNumber, NeuropixelsV2ProbeGroup probeGroup)
        {
            if (string.IsNullOrEmpty(partNumber))
                throw new ArgumentException("A probe part number must be provided.", nameof(partNumber));

            var modelName = probeGroup.Probe.Annotations.ModelName;
            if (!string.Equals(partNumber, modelName, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Probe part number ({partNumber}) does not match the probe " +
                    $"interface file's model name ({modelName}).");
            }
        }
    }
}
