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
        public NeuropixelsV2ProbeConfiguration ProbeConfigurationA { get; set; }

        /// <summary>
        /// Gets or sets the path to the gain calibration file for Probe A.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each probe is linked to a gain calibration file that contains gain adjustments determined by IMEC during
        /// factory testing. Electrode voltages are scaled using these values to ensure they can be accurately compared
        /// across probes. Therefore, using the correct gain calibration file is mandatory to create standardized recordings.
        /// </para>
        /// <para>
        /// Calibration files are probe-specific and not interchangeable across probes. Calibration files must contain the
        /// serial number of the corresponding probe on their first line of text. If you have lost track of a calibration
        /// file for your probe, email IMEC at neuropixels.info@imec.be with the probe serial number to retrieve a new copy.
        /// </para>
        /// </remarks>
        public string GainCalibrationFileA { get; set; }

        /// <summary>
        /// Gets or sets the electrode configuration for Probe B.
        /// </summary>
        public NeuropixelsV2ProbeConfiguration ProbeConfigurationB { get; set; }

        /// <summary>
        /// Gets or sets the path to the gain calibration file for Probe B.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each probe is linked to a gain calibration file that contains gain adjustments determined by IMEC during
        /// factory testing. Electrode voltages are scaled using these values to ensure they can be accurately compared
        /// across probes. Therefore, using the correct gain calibration file is mandatory to create standardized recordings.
        /// </para>
        /// <para>
        /// Calibration files are probe-specific and not interchangeable across probes. Calibration files must contain the
        /// serial number of the corresponding probe on their first line of text. If you have lost track of a calibration
        /// file for your probe, email IMEC at neuropixels.info@imec.be with the probe serial number to retrieve a new copy.
        /// </para>
        /// </remarks>
        public string GainCalibrationFileB { get; set; }

        /// <summary>
        /// Gets or sets a value determining if the polarity of the electrode voltages acquired by the probe
        /// should be inverted.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Neuropixels contain inverting amplifiers. This means that neural data that is captured by the probe
        /// will be inverted compared to the physical signal that occurs at the electrode: e.g., extracellular
        /// action potentials will tend to have positive deflections instead of negative. Setting this
        /// property to true will apply a gain of -1 to undo this effect.
        /// </para>
        /// </remarks>
        public bool InvertPolarity { get; set; }
    }
}
