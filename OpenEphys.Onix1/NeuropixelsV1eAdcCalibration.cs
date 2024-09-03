namespace OpenEphys.Onix1
{
    /// <summary>
    /// A struct to hold an array of <see cref="NeuropixelsV1eAdcCalibration"/> values and the serial number needed for each <see cref="ConfigureNeuropixelsV1e"/>.
    /// </summary>
    /// <param name="SerialNumber">The serial number from a calibration file.</param>
    ///  <param name="Adcs">The ADC calibration values from a calibration file.</param>
    public readonly record struct NeuropixelsV1eAdcCalibration(ulong SerialNumber, NeuropixelsV1eAdc[] Adcs);
}
