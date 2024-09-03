namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A struct to hold the gain correction value and the serial number from a Neuropixels 2.0 gain
    /// calibration file.
    /// </summary>
    /// <param name="SerialNumber">The serial number from a calibration file.</param>
    ///  <param name="GainCorrectionFactor">The gain correction from a gain calibration file.</param>
    public readonly record struct NeuropixelsV2GainCorrection(ulong SerialNumber, double GainCorrectionFactor);
}
