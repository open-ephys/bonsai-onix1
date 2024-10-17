namespace OpenEphys.Onix1
{
    /// <summary>
    /// ADC calibration values for a NeuropixelsV1e device.
    /// </summary>
    public class NeuropixelsV1Adc
    {
        /// <summary>
        /// Gets or sets a Neuropixels 1.0 CompP calibration setting
        /// </summary>
        public int CompP { get; set; } = 16;
        /// <summary>
        /// Gets or sets a Neuropixels 1.0 CompN calibration setting
        /// </summary>
        public int CompN { get; set; } = 16;
        /// <summary>
        /// Gets or sets a Neuropixels 1.0 Slope calibration setting
        /// </summary>
        public int Slope { get; set; } = 0;
        /// <summary>
        /// Gets or sets a Neuropixels 1.0 Coarse calibration setting
        /// </summary>
        public int Coarse { get; set; } = 0;
        /// <summary>
        /// Gets or sets a Neuropixels 1.0 Fine calibration setting
        /// </summary>
        public int Fine { get; set; } = 0;
        /// <summary>
        /// Gets or sets a Neuropixels 1.0 Cfix calibration setting
        /// </summary>
        public int Cfix { get; set; } = 0;
        /// <summary>
        /// Gets or sets a Neuropixels 1.0 Offset calibration setting
        /// </summary>
        public int Offset { get; set; } = 0;
        /// <summary>
        /// Gets or sets a Neuropixels 1.0 Threshold calibration setting
        /// </summary>
        public int Threshold { get; set; } = 512;
    }
}
