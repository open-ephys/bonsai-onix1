namespace OpenEphys.Onix1
{
    /// <summary>
    /// Public interface that defines common properties in NeuropixelsV1 devices.
    /// </summary>
    public interface IConfigureNeuropixelsV1
    {
        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// Gets or sets the electrode configuration.
        /// </summary>
        public NeuropixelsV1ProbeConfiguration ProbeConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the path to the gain calibration file.
        /// </summary>
        public string GainCalibrationFile { get; set; }

        /// <summary>
        /// Gets or sets the path to the gain calibration file.
        /// </summary>
        public string AdcCalibrationFile { get; set; }
    }
}
