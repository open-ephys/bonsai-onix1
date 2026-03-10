using System;
using System.Collections;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using Bonsai;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Specifies the bank of electrodes within each shank.
    /// </summary>
    public enum NeuropixelsV2Bank
    {
        /// <summary>
        /// Specifies that Bank A is the current bank.
        /// </summary>
        /// <remarks>Bank A is defined as shank index 0 to 383 along each shank.</remarks>
        A,
        /// <summary>
        /// Specifies that Bank B is the current bank.
        /// </summary>
        /// <remarks>Bank B is defined as shank index 384 to 767 along each shank.</remarks>
        B,
        /// <summary>
        /// Specifies that Bank C is the current bank.
        /// </summary>
        /// <remarks>Bank C is defined as shank index 768 to 1151 along each shank.</remarks>
        C,
        /// <summary>
        /// Specifies that Bank D is the current bank.
        /// </summary>
        /// <remarks>
        /// Bank D is defined as shank index 1152 to 1279 along each shank. Note that Bank D is not a full contingent
        /// of 384 channels; to compensate for this, electrodes from Bank C (starting at shank index 896) are used to
        /// generate a full 384 channel map.
        /// </remarks>
        D,
    }

    /// <summary>
    /// Defines a configuration for Neuropixels 2.0 and 2.0-beta probes.
    /// </summary>
    [XmlInclude(typeof(NeuropixelsV2QuadShankProbeConfiguration))]
    [XmlInclude(typeof(NeuropixelsV2SingleShankProbeConfiguration))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public abstract class NeuropixelsV2ProbeConfiguration : IProbeInterfaceConfiguration
    {
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
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("Invert the polarity of the electrode voltages acquired by the probe.")]
        public bool InvertPolarity { get; set; } = true;

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
        [Category(DeviceFactory.ConfigurationCategory)]
        [FileNameFilter("Gain calibration files (*_gainCalValues.csv)|*_gainCalValues.csv")]
        [Description("Path to the gain calibration file for this probe.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string GainCalibrationFileName { get; set; }

        /// <summary>
        /// Gets or sets the reference for all electrodes.
        /// </summary>
        [XmlIgnore]
        [Description("Defines the reference for the probe.")]
        public abstract Enum Reference { get; set; }

        /// <summary>
        /// Gets or sets the serialized reference value.
        /// </summary>
        /// <remarks>
        /// Ensures that XML serialization can occur for the generic Enum type <see cref="Reference"/>.
        /// </remarks>
        [XmlElement(nameof(Reference))]
        [Browsable(false)]
        [Externalizable(false)]
        public abstract string ReferenceSerialized { get; set; }

        /// <summary>
        /// Gets or sets the file path where the ProbeInterface configuration will be saved.
        /// </summary>
        [Category(DeviceFactory.ConfigurationCategory)]
        [Description("File path to where the ProbeInterface file exists for this probe.")]
        [FileNameFilter(ProbeInterfaceHelper.ProbeInterfaceFileNameFilter)]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string ProbeInterfaceFileName { get; set; }
        
        const int ReferencePixelCount = 4;
        const int DummyRegisterCount = 4;

        private protected const int RegistersPerShank = NeuropixelsV2.ElectrodePerShank + ReferencePixelCount + DummyRegisterCount;

        private protected const int ShiftRegisterBitExternalElectrode0 = 1285;
        private protected const int ShiftRegisterBitExternalElectrode1 = 2;

        private protected const int ShiftRegisterBitTipElectrode0 = 644;
        private protected const int ShiftRegisterBitTipElectrode1 = 643;

        internal abstract BitArray[] CreateShankBits(Enum reference);

        internal abstract int GetReferenceBit(Enum reference);

        internal abstract bool IsGroundReference();

        internal abstract int GetChannelNumber(int index);

        internal abstract NeuropixelsV2ProbeConfiguration Clone();

        internal abstract Type GetProbeGroupType();
    }
}
