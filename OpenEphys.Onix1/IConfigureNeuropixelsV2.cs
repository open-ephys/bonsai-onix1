namespace OpenEphys.Onix1
{
    /// <summary>
    /// Public interface that defines common properties in NeuropixelsV2 devices.
    /// </summary>
    public interface IConfigureNeuropixelsV2
    {
        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// Gets or sets the electrode configuration for Probe A.
        /// </summary>
        public NeuropixelsV2QuadShankProbeConfiguration ProbeConfigurationA { get; set; }

        /// <summary>
        /// Gets or sets the path to the gain calibration file for Probe A.
        /// </summary>
        public string GainCalibrationFileA { get; set; }

        /// <summary>
        /// Gets or sets the electrode configuration for Probe B.
        /// </summary>
        public NeuropixelsV2QuadShankProbeConfiguration ProbeConfigurationB { get; set; }

        /// <summary>
        /// Gets or sets the path to the gain calibration file for Probe B.
        /// </summary>
        public string GainCalibrationFileB { get; set; }

        /// <summary>
        /// Gets or sets the boolean to determine if neural data is inverted
        /// </summary>
        public bool InvertPolarity { get; set; }
    }
}
