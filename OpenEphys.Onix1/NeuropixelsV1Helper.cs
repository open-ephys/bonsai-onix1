using System;
using System.IO;
using System.Linq;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Static helper class for NeuropixelsV1.
    /// </summary>
    static class NeuropixelsV1Helper
    {
        internal const int NumberOfGainFactors = 8;
        internal const int NumberOfAdcParameters = 8;

        /// <summary>
        /// Tries to parse the ADC calibration file.
        /// </summary>
        /// <remarks>
        /// Checks if the given filename points to a real file, and checks each individual element:
        /// <para>
        /// 1) Serial number as the first line.
        /// </para>
        /// <para>
        /// 2) All remaining lines (32 lines) contain the ADC correction values for each ADC. First is the ADC number,
        /// followed by the <see cref="NeuropixelsV1Adc"/> values. All elements are separated by commas, and each element is a valid 
        /// integer value.
        /// </para>
        /// </remarks>
        /// <param name="adcCalibrationFile">String defining the path to the ADC calibration file.</param>
        /// <returns><see cref="NeuropixelsV1AdcCalibration"/> object that contains the ADC calibration values. This object is null if the file was not successfully parsed.</returns>
        internal static NeuropixelsV1AdcCalibration? TryParseAdcCalibrationFile(string adcCalibrationFile)
        {
            if (!File.Exists(adcCalibrationFile)) return null;

            var lines = File.ReadLines(adcCalibrationFile);

            if (lines.Count() != NeuropixelsV1.AdcCount + 1) return null;
            if (!ulong.TryParse(lines.ElementAt(0), out var serialNumber)) return null;

            if (!lines
                .Skip(1)
                .Select(l =>
                {
                    var ok = int.TryParse(l.Split(',')[0], out int adcNumber);
                    return (Ok: ok, AdcNumber: adcNumber);
                })
                .Where(l => l.Ok)
                .Select(l => l.AdcNumber)
                .SequenceEqual(Enumerable.Range(0, NeuropixelsV1.AdcCount))) return null;

            var adcs = lines
                       .Skip(1)
                       .Select(l =>
                       {
                           var calibrationValues = l
                                                   .Split(',')
                                                   .Skip(1)
                                                   .Select(p =>
                                                   {
                                                       var ok = int.TryParse(p, out int param);
                                                       return (Ok: ok, Param: param);
                                                   })
                                                   .Where(l => l.Ok)
                                                   .Select(l => l.Param);

                           return calibrationValues.Count() != NumberOfAdcParameters
                           ? null
                           : new NeuropixelsV1Adc {
                               CompP = calibrationValues.ElementAt(0),
                               CompN = calibrationValues.ElementAt(1),
                               Slope = calibrationValues.ElementAt(2),
                               Coarse = calibrationValues.ElementAt(3),
                               Fine = calibrationValues.ElementAt(4),
                               Cfix = calibrationValues.ElementAt(5),
                               Offset = calibrationValues.ElementAt(6),
                               Threshold = calibrationValues.ElementAt(7)
                           };
                       });

            return adcs.Any(adc => adc == null) ? null : new(serialNumber, adcs.ToArray());
        }

        /// <summary>
        /// Tries to parse the gain calibration file.
        /// </summary>
        /// <remarks>
        /// Checks if the given filename points to a real file, and checks each individual element:
        /// <para>
        /// 1) Serial number as the first line.
        /// </para>
        /// <para>
        /// 2) All remaining lines (384 lines) contain the gain correction values for each channel. First is the channel number,
        /// followed by the <see cref="NeuropixelsV1Gain"/> values. All elements are separated by commas, and each element is a valid 
        /// double value. After the channel number, the first 8 values are AP gain corrections, related to the spike-band, and the last 8 values
        /// are LFP gain corrections, for the LFP band.
        /// </para>
        /// </remarks>
        /// <param name="gainCalibrationFile">String defining the path to the gain calibration file.</param>
        /// <param name="apGain">Current <see cref="NeuropixelsV1Gain"/> for the AP data.</param>
        /// <param name="lfpGain">Current <see cref="NeuropixelsV1Gain"/> for the LFP data.</param>
        /// <param name="electrodeCount">Number of electrodes expected in the calibration file.</param>
        /// <returns><see cref="NeuropixelsV1eGainCorrection"/> object that contains the AP and LFP gain correction values. This object is null if the file was not successfully parsed.</returns>
        internal static NeuropixelsV1eGainCorrection? TryParseGainCalibrationFile(string gainCalibrationFile, NeuropixelsV1Gain apGain, NeuropixelsV1Gain lfpGain, int electrodeCount)
        {
            if (!File.Exists(gainCalibrationFile)) return null;

            var lines = File.ReadLines(gainCalibrationFile);

            if (lines.Count() != electrodeCount + 1) return null;
            if (!ulong.TryParse(lines.ElementAt(0), out var serialNumber)) return null;

            if (!lines
                .Skip(1)
                .Select(l =>
                {
                    var ok = int.TryParse(l.Split(',')[0], out var channel);
                    return (Ok: ok, Channel: channel);
                })
                .Where(l => l.Ok)
                .Select(l => l.Channel)
                .SequenceEqual(Enumerable.Range(0, electrodeCount))) return null;

            var apIndex = Array.IndexOf(Enum.GetValues(typeof(NeuropixelsV1Gain)), apGain);
            var apGainCorrections = lines
                                    .Skip(1)
                                    .Select<string, double?>(l =>
                                    {
                                        var corrections = l
                                                          .Split(',')
                                                          .Skip(1)
                                                          .Select(s =>
                                                          {
                                                              var ok = double.TryParse(s, out var correction);
                                                              return (Ok: ok, Correction: correction);
                                                          })
                                                          .Where(l => l.Ok)
                                                          .Select(l => l.Correction);
                                    
                                        return corrections.Count() == NumberOfGainFactors * 2 ? corrections.ElementAt(apIndex) : null;
                                    })
                                    .Distinct();

            if (apGainCorrections.Count() == 2) apGainCorrections = apGainCorrections.Where(val => val != 1.0); // NB: Reference contacts are always 1.0. Filter this out.

            var lfpIndex = Array.IndexOf(Enum.GetValues(typeof(NeuropixelsV1Gain)), lfpGain) + NumberOfGainFactors;
            var lfpGainCorrections = lines
                                     .Skip(1)
                                     .Select<string, double?>(l =>
                                     {
                                         var corrections = l
                                                          .Split(',')
                                                          .Skip(1)
                                                          .Select(s =>
                                                          {
                                                              var ok = double.TryParse(s, out var correction);
                                                              return (Ok: ok, Correction: correction);
                                                          })
                                                          .Where(l => l.Ok)
                                                          .Select(l => l.Correction);

                                         return corrections.Count() == NumberOfGainFactors * 2 ? corrections.ElementAt(lfpIndex) : null;
                                     })
                                     .Distinct();

            if (lfpGainCorrections.Count() == 2) lfpGainCorrections = lfpGainCorrections.Where(val => val != 1.0); // NB: Reference contacts are always 1.0. Filter this out.

            return (apGainCorrections.Count() != 1 || lfpGainCorrections.Count() != 1)
                   ? null
                   : new(serialNumber, apGainCorrections.First().Value, lfpGainCorrections.First().Value);
        }

        /// <summary>
        /// Checks that a probe part number resolves to the same variant as the model name annotation carried
        /// by the loaded probe interface data.
        /// </summary>
        /// <remarks>
        /// Both strings are resolved through <see cref="NeuropixelsV1VariantRegistry"/> rather than compared
        /// directly, since some part since standard NP1.0 probes can report old-style EEPROM string (e.g.
        /// <c>PRB_1_4_0480_1</c>) that differs from the probe interface file's modern <c>NP1000</c> model
        /// name annotation.
        /// </remarks>
        /// <param name="partNumber">The probe part number to check.</param>
        /// <param name="probeGroup">The loaded probe group whose model name annotation should be checked
        /// against <paramref name="partNumber"/>.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="partNumber"/> is null or empty, or
        /// if it resolves to a different variant than the probe interface file's model name
        /// annotation.</exception>
        /// <exception cref="NotSupportedException">Thrown if <paramref name="partNumber"/> is not a
        /// recognized Neuropixels 1.0 part number.</exception>
        internal static void ValidateProbePartNumber(string partNumber, NeuropixelsV1ProbeGroup probeGroup)
        {
            var eepromVariant = NeuropixelsV1VariantRegistry.Resolve(partNumber);
            var fileVariant = NeuropixelsV1VariantRegistry.Resolve(probeGroup.Probe.Annotations.ModelName);

            if (!ReferenceEquals(eepromVariant, fileVariant))
            {
                throw new ArgumentException($"Probe part number ({partNumber}) does not match the probe " +
                    $"interface file's model name ({probeGroup.Probe.Annotations.ModelName}).");
            }
        }
    }
}
