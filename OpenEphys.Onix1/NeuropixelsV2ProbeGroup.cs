using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Defines a constrained probeInterface compatible probe group for Neuropixels 2.0 and 2.0-beta probes.
    /// </summary>
    [XmlInclude(typeof(NeuropixelsV2QuadShankProbeGroup))]
    [XmlInclude(typeof(NeuropixelsV2SingleShankProbeGroup))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public abstract class NeuropixelsV2ProbeGroup : SingleProbeGroup, IMultiplexedProbeGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2ProbeGroup"/> class from deserialized
        /// ProbeInterface data.
        /// </summary>
        /// <param name="specification">The ProbeInterface specification string.</param>
        /// <param name="version">The ProbeInterface version string.</param>
        /// <param name="probes">The probes deserialized from the ProbeInterface file.</param>
        public NeuropixelsV2ProbeGroup(string specification, string version, IEnumerable<Probe> probes)
            : base(specification, version, probes) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2ProbeGroup"/> class by copying an
        /// existing probe group.
        /// </summary>
        /// <param name="probeGroup">The probe group to copy.</param>
        protected NeuropixelsV2ProbeGroup(NeuropixelsV2ProbeGroup probeGroup)
            : base(probeGroup) { }

        /// <summary>
        /// Returns the zero-based shank index that contains the given contact index.
        /// </summary>
        /// <param name="contactIndex">The zero-based contact index across all electrodes on the
        /// probe.</param>
        /// <returns>The zero-based shank index.</returns>
        public int GetShank(int contactIndex) =>
            contactIndex / NeuropixelsV2.ElectrodesPerShank;

        /// <summary>
        /// Returns the electrode index within its shank for the given contact index.
        /// </summary>
        /// <param name="contactIndex">The zero-based contact index across all electrodes on the probe.</param>
        /// <returns>The zero-based electrode index within the shank.</returns>
        public int GetIntraShankElectrodeIndex(int contactIndex) =>
            contactIndex % NeuropixelsV2.ElectrodesPerShank;

        /// <summary>
        /// Returns the bank that contains the given contact index.
        /// </summary>
        /// <param name="contactIndex">The zero-based contact index across all electrodes on the probe.</param>
        /// <returns>The <see cref="NeuropixelsV2Bank"/> containing the specified contact.</returns>
        public NeuropixelsV2Bank GetBank(int contactIndex)
            => (NeuropixelsV2Bank)((contactIndex % NeuropixelsV2.ElectrodesPerShank) / NeuropixelsV2.ChannelCount);

        /// <summary>
        /// Returns the acquisition channel number for the given contact index.
        /// </summary>
        /// <param name="contactIndex">The zero-based contact index across all electrodes on the probe.</param>
        /// <returns>The acquisition channel number (0 to <see cref="NeuropixelsV2.ChannelCount"/> - 1).</returns>
        public abstract int GetChannel(int contactIndex);

        /// <summary>
        /// Wires each contact in <paramref name="contactIndices"/> to its corresponding acquisition channel.
        /// </summary>
        /// <param name="contactIndices">The contact indices to enable.</param>
        public void EnableElectrodes(IEnumerable<int> contactIndices)
        {
            foreach (int contactIdx in contactIndices)
                ChannelWiring.WireChannel(this, 0, contactIdx, GetChannel(contactIdx));
        }

        /// <summary>
        /// Returns an array of all valid reference source values for this probe type.
        /// </summary>
        /// <returns>An <see cref="Array"/> of the probe-specific reference enum values.</returns>
        public abstract Array GetReferenceEnumValues();

        /// <summary>
        /// Returns an array of all available channel preset values for this probe type.
        /// </summary>
        /// <returns>An <see cref="Array"/> of the probe-specific channel preset enum values.</returns>
        public abstract Array GetChannelPresets();

        /// <summary>
        /// Configures the channel map to the specified preset.
        /// </summary>
        /// <param name="preset">The preset enum value to apply. Must be a value from the probe-specific
        /// channel preset enum returned by <see cref="GetChannelPresets"/>.</param>
        public abstract void SelectPreset(Enum preset);

        /// <summary>
        /// Configures the channel map for a specific shank and bank by selecting the corresponding preset.
        /// </summary>
        /// <param name="shank">Zero-based shank index.</param>
        /// <param name="bank">Bank of electrodes to activate on the specified shank.</param>
        public abstract void SelectBank(int shank, NeuropixelsV2Bank bank);

    }
}
