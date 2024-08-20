namespace OpenEphys.Onix1
{
    /// <summary>
    /// A struct to hold the two gain correction values (AP and LFP) and the serial number needed for each <see cref="ConfigureNeuropixelsV1e"/>.
    /// </summary>
    public readonly struct NeuropixelsV1eGainCorrection
    {
        /// <summary>
        /// Gets the serial number found in the gain calibration file.
        /// </summary>
        public ulong SN { get; }

        /// <summary>
        /// Gets the AP gain correction found in the gain calibration file for the appropriate AP gain.
        /// </summary>
        public double AP { get; }

        /// <summary>
        /// Gets the LFP gain correction found in the gain calibration file for the appropriate LFP gain.
        /// </summary>
        public double LFP { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV1eGainCorrection"/> struct.
        /// </summary>
        /// <param name="sn">Unsigned long value holding the serial number of the probe from the calibration file.</param>
        /// <param name="ap">Double holding the AP gain correction as pulled from the gain calibration file.</param>
        /// <param name="lfp">Double holding the LFP gain correction as pulled from the gain calibration file.</param>
        public NeuropixelsV1eGainCorrection(ulong sn, double ap, double lfp)
        {
            SN = sn;
            AP = ap;
            LFP = lfp;
        }
    }
}
