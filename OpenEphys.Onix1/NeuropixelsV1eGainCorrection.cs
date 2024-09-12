namespace OpenEphys.Onix1
{
    /// <summary>
    /// A struct to hold the two gain correction values (AP and LFP) and the serial number needed for each <see cref="ConfigureNeuropixelsV1e"/>.
    /// </summary>
    /// <param name="SerialNumber">The serial number from a calibration file.</param>
    /// <param name="ApGainCorrectionFactor">The gain correction for the spike-band from a gain calibration file.</param>
    /// <param name="LfpGainCorrectionFactor">The gain correction for the LFP-band from a gain calibration file.</param>
    public readonly record struct NeuropixelsV1eGainCorrection(ulong SerialNumber, double ApGainCorrectionFactor, double LfpGainCorrectionFactor);
}
