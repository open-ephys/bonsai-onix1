namespace OpenEphys.Onix1
{
    /// <summary>
    /// A struct to hold the two gain correction values (AP and LFP) needed for each <see cref="ConfigureNeuropixelsV1e"/>.
    /// </summary>
    public struct NeuropixelsV1eGainCorrection
    {
        /// <summary>
        /// Gets or sets the AP gain correction.
        /// </summary>
        public double AP { get; set; }

        /// <summary>
        /// Gets or sets the LFP gain correction.
        /// </summary>
        public double LFP { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV1eGainCorrection"/> struct.
        /// </summary>
        /// <param name="ap">Double holding the AP gain correction as pulled from the gain calibration file.</param>
        /// <param name="lfp">Double holding the LFP gain correction as pulled from the gain calibration file.</param>
        public NeuropixelsV1eGainCorrection(double ap, double lfp)
        {
            AP = ap;
            LFP = lfp;
        }
    }
}
