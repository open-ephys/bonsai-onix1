using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Defines a constrained probeInterface compatible probe group for Neuropixels 1.0 probes.
    /// </summary>
    public class NeuropixelsV1ProbeGroup : SingleProbeGroup, IMultiplexedProbeGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV1ProbeGroup"/> class using the
        /// default electrode geometry.
        /// </summary>
        public NeuropixelsV1ProbeGroup()
            : this(ProbeGroupResource.LoadDefault<NeuropixelsV1ProbeGroup>("NP1000.json"))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV1ProbeGroup"/> class by copying
        /// an existing probe group.
        /// </summary>
        /// <param name="probeGroup">The probe group to copy.</param>
        public NeuropixelsV1ProbeGroup(NeuropixelsV1ProbeGroup probeGroup)
            : base(probeGroup)
        {
            if (NumberOfContacts != NeuropixelsV1.ElectrodeCount)
                throw new ArgumentException($"Invalid number of contacts; expected {NeuropixelsV1.ElectrodeCount}, but found {NumberOfContacts}.");

            if (base.ChannelMap is null)
                SelectPreset(NeuropixelsV1ChannelPreset.BankA);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV1ProbeGroup"/> class from
        /// deserialized ProbeInterface data.
        /// </summary>
        /// <param name="specification">The ProbeInterface specification string.</param>
        /// <param name="version">The ProbeInterface version string.</param>
        /// <param name="probes">The array of probes deserialized from the ProbeInterface file.</param>
        [JsonConstructor]
        public NeuropixelsV1ProbeGroup(string specification, string version, Probe[] probes)
            : base(specification, version, probes)
        {
            if (NumberOfContacts != NeuropixelsV1.ElectrodeCount)
                throw new ArgumentException($"Invalid number of contacts; expected {NeuropixelsV1.ElectrodeCount}, but found {NumberOfContacts}.");

            if (base.ChannelMap is null)
                SelectPreset(NeuropixelsV1ChannelPreset.BankA);
        }

        /// <summary>
        /// Returns the channel map for this probe, mapping each acquisition channel index to its
        /// active contact index.
        /// </summary>
        /// <remarks>
        /// This property hides <see cref="ProbeGroup.ChannelMap"/>, which is nullable, to enforce the
        /// invariant that a Neuropixels 1.0 probe group always has a valid channel map after
        /// construction. If no channel map was present in the deserialized data,
        /// <see cref="NeuropixelsV1ChannelPreset.BankA"/> is applied automatically during
        /// construction.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the channel map is null.</exception>
        [JsonIgnore]
        public new IReadOnlyDictionary<int, int> ChannelMap =>
            base.ChannelMap ?? throw new InvalidOperationException($"Neuropixels probes must have a valid channel map.");

        /// <summary>
        /// Returns the acquisition channel number for the given contact index.
        /// </summary>
        /// <param name="contactIndex">The zero-based contact index across all electrodes on the probe.</param>
        /// <returns>The acquisition channel number (0 to <see cref="NeuropixelsV1.ChannelCount"/> - 1).</returns>
        public int GetChannel(int contactIndex) =>
            contactIndex % NeuropixelsV1.ChannelCount;

        /// <summary>
        /// Returns the bank that contains the given contact index.
        /// </summary>
        /// <param name="contactIndex">The zero-based contact index across all electrodes on the probe.</param>
        /// <returns>The <see cref="NeuropixelsV1Bank"/> containing the specified contact.</returns>
        public static NeuropixelsV1Bank GetBank(int contactIndex) =>
            (NeuropixelsV1Bank)(contactIndex / NeuropixelsV1.ChannelCount);

        /// <summary>
        /// Wires each contact in <paramref name="contactIndices"/> to its corresponding acquisition channel.
        /// </summary>
        /// <remarks>
        /// Internal reference contacts (see <see cref="NeuropixelsV1.IsInternalReferenceContact"/>)
        /// are silently skipped: they are never wired to a recording channel on any bank.
        /// </remarks>
        /// <param name="contactIndices">The contact indices to enable.</param>
        public void EnableElectrodes(IEnumerable<int> contactIndices)
        {
            foreach (int contactIdx in contactIndices)
            {
                if (NeuropixelsV1.IsInternalReferenceContact(contactIdx)) continue;
                ChannelWiring.WireChannel(this, 0, contactIdx, GetChannel(contactIdx));
            }
        }

        /// <summary>
        /// Configures the channel map to one of the predefined electrode selection patterns.
        /// </summary>
        /// <param name="preset">The channel preset to apply.</param>
        public void SelectPreset(NeuropixelsV1ChannelPreset preset)
        {
            int n = NumberOfContacts;
            IEnumerable<int> contactIndices = preset switch
            {
                NeuropixelsV1ChannelPreset.BankA => Enumerable.Range(0, NeuropixelsV1.ChannelCount),
                NeuropixelsV1ChannelPreset.BankB => Enumerable.Range(NeuropixelsV1.ChannelCount, NeuropixelsV1.ChannelCount),
                NeuropixelsV1ChannelPreset.BankC => Enumerable.Range(0, n)
                    .Where(i => GetBank(i) == NeuropixelsV1Bank.C ||
                                (GetBank(i) == NeuropixelsV1Bank.B && i >= 576)),
                NeuropixelsV1ChannelPreset.SingleColumn => Enumerable.Range(0, n)
                    .Where(i => (i % 2 == 0 && GetBank(i) == NeuropixelsV1Bank.A) ||
                                (i % 2 == 1 && GetBank(i) == NeuropixelsV1Bank.B)),
                NeuropixelsV1ChannelPreset.Tetrodes => Enumerable.Range(0, n)
                    .Where(i => (i % 8 < 4 && GetBank(i) == NeuropixelsV1Bank.A) ||
                                (i % 8 > 3 && GetBank(i) == NeuropixelsV1Bank.B)),
                _ => throw new ArgumentOutOfRangeException(nameof(preset))
            };
            EnableElectrodes(contactIndices);
        }
    }

    /// <summary>
    /// Specifies a predefined electrode selection pattern for a Neuropixels 1.0 probe.
    /// </summary>
    public enum NeuropixelsV1ChannelPreset
    {
        /// <summary>
        /// Selects the 384 electrodes in Bank A (electrode indices 0–383).
        /// </summary>
        BankA,
        /// <summary>
        /// Selects the 384 electrodes in Bank B (electrode indices 384–767).
        /// </summary>
        BankB,
        /// <summary>
        /// Selects 384 electrodes spanning the upper half of Bank B and all of Bank C
        /// (electrode indices 576–959).
        /// </summary>
        BankC,
        /// <summary>
        /// Selects 384 electrodes in a single-column pattern, alternating between Bank A (even electrodes)
        /// and Bank B (odd electrodes).
        /// </summary>
        SingleColumn,
        /// <summary>
        /// Selects 384 electrodes in a tetrode pattern, grouping sets of four adjacent contacts from
        /// Bank A and Bank B.
        /// </summary>
        Tetrodes
    }
}
