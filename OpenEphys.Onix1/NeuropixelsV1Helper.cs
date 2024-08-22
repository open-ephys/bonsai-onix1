using System;
using System.IO;
using System.Linq;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Static helper class for NeuropixelsV1e methods.
    /// </summary>
    public static class NeuropixelsV1Helper
    {
        internal const int NumberOfGains = 8;

        /// <summary>
        /// Returns the ADC values and serial number from an ADC calibration file for a specific probe.
        /// </summary>
        /// <param name="file">Incoming <see cref="StreamReader"/> that is reading from the ADC calibration file.</param>
        /// <returns>Array of <see cref="NeuropixelsV1eAdc"/> values.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static NeuropixelsV1eAdcCalibration ParseAdcCalibrationFile(StreamReader file)
        {
            if (file == null || file.EndOfStream)
            {
                throw new ArgumentException("Incoming stream reader is not pointing to a valid ADC calibration file.");
            }

            var adcSerialNumber = ulong.Parse(file.ReadLine());

            NeuropixelsV1eAdc[] adcs = new NeuropixelsV1eAdc[NeuropixelsV1e.AdcCount];

            for (var i = 0; i < NeuropixelsV1e.AdcCount; i++)
            {
                var adcCal = file.ReadLine().Split(',').Skip(1);
                if (adcCal.Count() != NumberOfGains)
                {
                    throw new InvalidOperationException("Incorrectly formatted ADC calibration file.");
                }

                adcs[i] = new NeuropixelsV1eAdc
                {
                    CompP = int.Parse(adcCal.ElementAt(0)),
                    CompN = int.Parse(adcCal.ElementAt(1)),
                    Slope = int.Parse(adcCal.ElementAt(2)),
                    Coarse = int.Parse(adcCal.ElementAt(3)),
                    Fine = int.Parse(adcCal.ElementAt(4)),
                    Cfix = int.Parse(adcCal.ElementAt(5)),
                    Offset = int.Parse(adcCal.ElementAt(6)),
                    Threshold = int.Parse(adcCal.ElementAt(7))
                };
            }

            return new(adcSerialNumber, adcs);
        }

        /// <summary>
        /// Returns the <see cref="NeuropixelsV1eGainCorrection"/> values from a gain calibration file for a specific probe at the given gain for each stream.
        /// </summary>
        /// <param name="file">Incoming <see cref="StreamReader"/> that is reading from the gain calibration file.</param>
        /// <param name="apGain">Current AP gain.</param>
        /// <param name="lfpGain">Current LFP gain.</param>
        /// <returns><see cref="NeuropixelsV1eGainCorrection"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static NeuropixelsV1eGainCorrection ParseGainCalibrationFile(StreamReader file, NeuropixelsV1Gain apGain, NeuropixelsV1Gain lfpGain)
        {
            if (file == null || file.EndOfStream)
            {
                throw new ArgumentException("Incoming stream reader is not pointing to a valid gain calibration file.");
            }

            var serialNumber = ulong.Parse(file.ReadLine());

            var gainCorrections = file.ReadLine().Split(',').Skip(1);

            if (gainCorrections.Count() != 2 * NumberOfGains)
                throw new InvalidOperationException("Incorrectly formatted gain correction calibration file.");

            var ap = double.Parse(gainCorrections.ElementAt(Array.IndexOf(Enum.GetValues(typeof(NeuropixelsV1Gain)), apGain)));
            var lfp = double.Parse(gainCorrections.ElementAt(Array.IndexOf(Enum.GetValues(typeof(NeuropixelsV1Gain)), lfpGain) + 8));

            return new NeuropixelsV1eGainCorrection(serialNumber, ap, lfp);
        }
    }
}
