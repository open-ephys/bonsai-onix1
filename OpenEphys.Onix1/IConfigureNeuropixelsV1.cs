using System.ComponentModel;

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
        /// Gets or sets the probe configuration.
        /// </summary>
        /// <remarks>
        /// Configuration is accomplished using a GUI to aid in channel selection and relevant configuration properties.
        /// To open a probe configuration GUI, select the ellipses next the ProbeConfiguration variable
        /// in the property pane, or double-click the configuration node to configure the probe and the Bno055 simultaneously.
        /// </remarks>
        public NeuropixelsV1ProbeConfiguration ProbeConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the path to the gain calibration file.
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
        public string GainCalibrationFile { get; set; }

        /// <summary>
        /// Gets or sets the path to the ADC calibration file.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each probe must be provided with an ADC calibration file that contains probe-specific hardware settings that is
        /// created by IMEC during factory calibration. These files are used to set internal bias currents, correct for ADC
        /// nonlinearities, correct ADC-zero crossing non-monotonicities, etc. Using the correct calibration file is mandatory
        /// for the probe to operate correctly. 
        /// </para>
        /// <para>
        /// Calibration files are probe-specific and not interchangeable across probes. Calibration files must contain the 
        /// serial number of the corresponding probe on their first line of text. If you have lost track of a calibration 
        /// file for your probe, email IMEC at neuropixels.info@imec.be with the probe serial number to retrieve a new copy.
        /// </para>
        /// </remarks>
        public string AdcCalibrationFile { get; set; }

        /// <summary>
        /// Gets or sets a value determining if the polarity of the electrode voltages acquired by the probe
        /// should be inverted.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Neuropixels contain inverting amplifiers.This means that neural data that is captured by the probe
        /// will be inverted compared to the physical signal that occurs at the electrode: e.g. extracellular
        /// action potentials will tend to have positive deflections instead of negative. Enabling this
        /// setting this property to true will apply a gain of -1 to undo this effect.
        /// </para>
        /// </remarks>
        public bool InvertPolarity { get; set; }
    }
}
