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
    [XmlType(Namespace = Constants.XmlNamespace)]
    public abstract class NeuropixelsV2ProbeConfiguration
    {
        /// <summary>
        /// Gets or sets the <see cref="NeuropixelsV2Probe"/> for this probe.
        /// </summary>
        [Browsable(false)]
        public NeuropixelsV2Probe Probe { get; set; }

        /// <summary>
        /// Gets or sets the reference for all electrodes.
        /// </summary>
        [XmlIgnore]
        [Description("Defines the reference for the probe.")]
        public Enum Reference { get; set; }

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
        /// Gets the existing channel map listing all currently enabled electrodes.
        /// </summary>
        /// <remarks>
        /// The channel map will always be 384 channels, and will return the 384 enabled electrodes.
        /// </remarks>
        [XmlIgnore]
        public abstract NeuropixelsV2Electrode[] ChannelMap { get; }

        /// <summary>
        /// Update the <see cref="ChannelMap"/> with the selected electrodes.
        /// </summary>
        /// <param name="electrodes">List of selected electrodes that are being added to the <see cref="ChannelMap"/></param>
        public abstract void SelectElectrodes(NeuropixelsV2Electrode[] electrodes);

        /// <summary>
        /// Gets the <see cref="ProbeGroup"/> channel configuration for this probe.
        /// </summary>
        [XmlIgnore]
        [Category("Configuration")]
        [Description("Defines the shape of the probe, and which contacts are currently selected for streaming.")]
        public NeuropixelsV2eProbeGroup ProbeGroup { get; set; }

        /// <summary>
        /// Gets or sets a string defining the <see cref="ProbeGroup"/> in Base64.
        /// This variable is needed to properly save a workflow in Bonsai, but it is not
        /// directly accessible in the Bonsai editor.
        /// </summary>
        [Browsable(false)]
        [Externalizable(false)]
        [XmlElement(nameof(ProbeGroup))]
        public abstract string ProbeGroupString { get; set; }

        internal abstract BitArray[] CreateShankBits(Enum reference);

        internal abstract int GetReferenceBit(Enum reference);

        internal abstract bool IsGroundReference();
    }
}
