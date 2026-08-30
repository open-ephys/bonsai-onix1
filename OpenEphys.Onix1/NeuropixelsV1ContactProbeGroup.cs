using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A <see cref="NeuropixelsV1ProbeGroup"/> whose contacts can be channel mapped individually.
    /// </summary>
    public sealed class NeuropixelsV1ChannelToContactProbeGroup : NeuropixelsV1ProbeGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV1ChannelToContactProbeGroup"/> class using the
        /// default electrode geometry.
        /// </summary>
        public NeuropixelsV1ChannelToContactProbeGroup()
            : this(ProbeGroupResource.LoadDefault<NeuropixelsV1ChannelToContactProbeGroup>("NP1000.json"))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV1ChannelToContactProbeGroup"/> class by
        /// copying an existing probe group.
        /// </summary>
        /// <param name="probeGroup">The probe group to copy.</param>
        public NeuropixelsV1ChannelToContactProbeGroup(NeuropixelsV1ChannelToContactProbeGroup probeGroup)
            : base(probeGroup)
        {
            if (!HasChannelMap)
                SelectBank(0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV1ChannelToContactProbeGroup"/> class from
        /// deserialized ProbeInterface data.
        /// </summary>
        /// <param name="specification">The ProbeInterface specification string.</param>
        /// <param name="version">The ProbeInterface version string.</param>
        /// <param name="probes">The array of probes deserialized from the ProbeInterface file.</param>
        [JsonConstructor]
        public NeuropixelsV1ChannelToContactProbeGroup(string specification, string version, Probe[] probes)
            : base(specification, version, probes)
        {
            if (Variant.HasChannelGroupSelection)
                throw new ArgumentException(
                    $"{Probe.Annotations.ModelName} requires per-channel-group bank selection; use " +
                    $"{nameof(NeuropixelsV1ChannelGroupProbeGroup)} instead.");

            if (!HasChannelMap)
                SelectBank(0);
        }

        internal override NeuropixelsV1ProbeGroup Clone() => new NeuropixelsV1ChannelToContactProbeGroup(this);

        internal override void SetShankConfigurationBits(NeuropixelsV1ShankRegisterLayout layout, BitArray shankBits)
        {
            foreach (var kvp in ChannelMap)
            {
                int contactIdx = kvp.Value;
                if (Variant.IsInternalReferenceContact(contactIdx)) continue;

                shankBits[layout.GetElectrodeBitIndex(Variant.ShankRegisterPosition(contactIdx))] = true;
            }
        }

        /// <summary>
        /// Wires each contact in <paramref name="contactIndices"/> to its corresponding acquisition channel.
        /// </summary>
        /// <remarks>
        /// On variants with a dedicated internal reference electrode (see <see
        /// cref="NeuropixelsV1Variant.HasInternalReferenceElectrode"/>), those contacts (see <see
        /// cref="NeuropixelsV1.IsInternalReferenceContact"/>) are silently skipped: they are never wired to a
        /// recording channel on any bank.
        /// </remarks>
        /// <param name="contactIndices">The contact indices to enable.</param>
        public void EnableElectrodes(IEnumerable<int> contactIndices)
        {
            foreach (int contactIdx in contactIndices)
            {
                if (Variant.IsInternalReferenceContact(contactIdx)) continue;
                ChannelWiring.WireChannel(this, 0, contactIdx, GetChannel(contactIdx));
            }
        }

        /// <summary>
        /// Returns every channel preset available for this probe, generated from its variant.
        /// </summary>
        /// <remarks>
        /// Always includes one preset per bank. <c>SingleColumn</c>/<c>Tetrodes</c> are appended only when
        /// <see cref="NeuropixelsV1Variant.SupportsColumnAndTetrodePresets"/> is true for this probe's <see
        /// cref="NeuropixelsV1ProbeGroup.Variant"/>. NB: they are a site-layout pattern, not meaningful for
        /// every probe with a matching bank count.
        /// </remarks>
        /// <returns>The list of channel presets, starting with <see
        /// cref="NeuropixelsV1ChannelPreset.None"/>.</returns>
        public IReadOnlyList<NeuropixelsV1ChannelPreset> GetChannelPresets()
        {
            var presets = new List<NeuropixelsV1ChannelPreset> { NeuropixelsV1ChannelPreset.None };

            presets.AddRange(Enumerable.Range(0, BankCount)
                .Select(bankIndex => new NeuropixelsV1ChannelPreset(
                    $"Bank{Neuropixels.BankDisplayName(bankIndex)}",
                    pg => pg.GetBankContactIndices(bankIndex))));

            if (Variant.SupportsColumnAndTetrodePresets)
            {
                presets.Add(new NeuropixelsV1ChannelPreset("SingleColumn", pg => Enumerable.Range(0, pg.NumberOfContacts)
                    .Where(i => (i % 2 == 0 && GetBank(i) == 0) || (i % 2 == 1 && GetBank(i) == 1))));
                presets.Add(new NeuropixelsV1ChannelPreset("Tetrodes", pg => Enumerable.Range(0, pg.NumberOfContacts)
                    .Where(i => (i % 8 < 4 && GetBank(i) == 0) || (i % 8 > 3 && GetBank(i) == 1))));
            }

            return presets;
        }

        /// <summary>
        /// Configures the channel map to the specified preset.
        /// </summary>
        /// <param name="preset">The preset to apply. Passing <see cref="NeuropixelsV1ChannelPreset.None"/>
        /// leaves the existing channel map unchanged.</param>
        public void SelectPreset(NeuropixelsV1ChannelPreset preset)
        {
            if (preset == NeuropixelsV1ChannelPreset.None)
                return;

            EnableElectrodes(preset.ContactSelector(this));
        }

        /// <summary>
        /// Configures the channel map to a specific bank, addressed by index.
        /// </summary>
        /// <param name="bankIndex">The zero-based bank index.</param>
        public override void SelectBank(int bankIndex) => EnableElectrodes(GetBankContactIndices(bankIndex));

        /// <summary>
        /// Returns the contact indices belonging to the given bank.
        /// </summary>
        /// <remarks>
        /// Each bank is a <see cref="NeuropixelsV1.ChannelCount"/>-wide window; the last bank's window is
        /// right-aligned against the end of the probe's electrodes rather than starting a full <see
        /// cref="NeuropixelsV1.ChannelCount"/> past the previous one.
        /// </remarks>
        /// <param name="bankIndex">The zero-based bank index.</param>
        IEnumerable<int> GetBankContactIndices(int bankIndex)
        {
            if (bankIndex < 0 || bankIndex >= Variant.BankCount)
                throw new ArgumentOutOfRangeException(nameof(bankIndex), $"Probe has {Variant.BankCount} bank(s); got {bankIndex}.");

            return Enumerable.Range(
                Neuropixels.BankWindowStart(bankIndex, Variant.ElectrodeCount, NeuropixelsV1.ChannelCount),
                NeuropixelsV1.ChannelCount);
        }
    }

    /// <summary>
    /// Specifies a predefined electrode selection pattern for a Neuropixels 1.0 probe.
    /// </summary>
    public readonly record struct NeuropixelsV1ChannelPreset(string DisplayName, Func<NeuropixelsV1ChannelToContactProbeGroup, IEnumerable<int>> ContactSelector)
    {
        /// <summary>
        /// The "no preset" value; applying it leaves the existing channel map unchanged.
        /// </summary>
        public static readonly NeuropixelsV1ChannelPreset None = new("None", _ => Enumerable.Empty<int>());

        /// <inheritdoc/>
        public override string ToString() => DisplayName;
    }
}
