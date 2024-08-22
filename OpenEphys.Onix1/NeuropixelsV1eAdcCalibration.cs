namespace OpenEphys.Onix1
{
    /// <summary>
    /// A struct to hold an array of <see cref="NeuropixelsV1eAdcCalibration"/> values and the serial number needed for each <see cref="ConfigureNeuropixelsV1e"/>.
    /// </summary>
    public readonly struct NeuropixelsV1eAdcCalibration
    {
        /// <summary>
        /// Gets an array of <see cref="NeuropixelsV1eAdc"/> values.
        /// </summary>
        public NeuropixelsV1eAdc[] Adcs { get; }

        /// <summary>
        /// Gets the serial number found in the ADC calibration file.
        /// </summary>
        public ulong SN { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV1eAdcCalibration"/> struct.
        /// </summary>
        /// <param name="sn">Unsigned long value holding the serial number of the probe from the calibration file.</param>
        /// <param name="adcs">Array of <see cref="NeuropixelsV1eAdc"/> values from the calibration file.</param>
        public NeuropixelsV1eAdcCalibration(ulong sn, NeuropixelsV1eAdc[] adcs)
        {
            SN = sn;
            Adcs = adcs;
        }
    }
}
