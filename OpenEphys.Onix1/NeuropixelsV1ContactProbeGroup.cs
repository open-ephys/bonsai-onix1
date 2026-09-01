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
    public sealed class NeuropixelsV1ContactProbeGroup : NeuropixelsV1ProbeGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV1ContactProbeGroup"/> class using the
        /// default electrode geometry.
        /// </summary>
        public NeuropixelsV1ContactProbeGroup()
            : this(ProbeGroupResource.LoadDefault<NeuropixelsV1ContactProbeGroup>("NP1000.json"))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV1ContactProbeGroup"/> class by
        /// copying an existing probe group.
        /// </summary>
        /// <param name="probeGroup">The probe group to copy.</param>
        public NeuropixelsV1ContactProbeGroup(NeuropixelsV1ContactProbeGroup probeGroup)
            : base(probeGroup)
        {
            if (!HasChannelMap)
                SelectBank(0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV1ContactProbeGroup"/> class from
        /// deserialized ProbeInterface data.
        /// </summary>
        /// <param name="specification">The ProbeInterface specification string.</param>
        /// <param name="version">The ProbeInterface version string.</param>
        /// <param name="probes">The array of probes deserialized from the ProbeInterface file.</param>
        [JsonConstructor]
        internal NeuropixelsV1ContactProbeGroup(string specification, string version, Probe[] probes)
            : base(specification, version, probes)
        {
            if (Variant.HasChannelGroupSelection)
                throw new ArgumentException(
                    $"{Probe.Annotations.ModelName} requires per-channel-group bank selection; use " +
                    $"{nameof(NeuropixelsNP1110ProbeGroup)} instead.");

            if (!HasChannelMap)
                SelectBank(0);
        }

        internal override NeuropixelsV1ProbeGroup Clone() => new NeuropixelsV1ContactProbeGroup(this);

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
        /// Returns every preset available for this probe, generated from its variant.
        /// </summary>
        /// <remarks>
        /// Always includes "Full" (every channel at the same relative bank). <c>SingleColumn</c>/
        /// <c>Tetrodes</c> are appended only when <see cref="NeuropixelsV1Variant.SupportsColumnAndTetrodePresets"/>
        /// is true for this probe's <see cref="NeuropixelsV1ProbeGroup.Variant"/>; <c>SingleColumn
        /// (Option 1)</c>/<c>(Option 2)</c> only when <see
        /// cref="NeuropixelsV1Variant.SupportsLeftRightColumnPresets"/> is. NB: these are site-layout
        /// patterns, not meaningful for every probe with a matching bank count.
        /// </remarks>
        /// <returns>The list of presets, starting with <see cref="NeuropixelsV1Preset.None"/>.</returns>
        public override IReadOnlyList<NeuropixelsV1Preset> GetPresets()
        {
            var presets = new List<NeuropixelsV1Preset>
            {
                NeuropixelsV1Preset.None,
                new("Full", Enumerable.Repeat(0, NeuropixelsV1.ChannelCount).ToArray(), null),
            };

            if (Variant.SupportsColumnAndTetrodePresets)
            {
                presets.Add(new NeuropixelsV1Preset("SingleColumn",
                    Enumerable.Range(0, NeuropixelsV1.ChannelCount).Select(c => c % 2).ToArray(), null, UnitsPerBank: 2));
                presets.Add(new NeuropixelsV1Preset("Tetrodes",
                    Enumerable.Range(0, NeuropixelsV1.ChannelCount).Select(c => c % 8 < 4 ? 0 : 1).ToArray(), null, UnitsPerBank: 2));
            }

            if (Variant.SupportsLeftRightColumnPresets)
            {
                presets.Add(new NeuropixelsV1Preset("SingleColumn (Option 1)",
                    Enumerable.Range(0, NeuropixelsV1.ChannelCount).Select(c => c % 2).ToArray(), null));
                presets.Add(new NeuropixelsV1Preset("SingleColumn (Option 2)",
                    Enumerable.Range(0, NeuropixelsV1.ChannelCount).Select(c => 1 - c % 2).ToArray(), null));
            }

            return presets;
        }

        /// <summary>
        /// Applies <paramref name="preset"/> at <paramref name="offset"/> banks from the tip, replacing
        /// the existing channel map.
        /// </summary>
        /// <remarks>
        /// "Full" applied at offset <c>N</c> reproduces <see cref="SelectBank"/> exactly.
        /// <para>
        /// A channel's own relative bank (<see cref="NeuropixelsV1Preset.RelativeBanks"/>) and the
        /// channel-half split forced by the wiring formula (see <see
        /// cref="NeuropixelsV1Preset.UnitsPerBank"/>) are combined into one virtual index -- relative
        /// bank as the high-order component, half as the low-order one -- so that every distinct
        /// (relative bank, half) pair gets its own position along a single offset axis. Advancing
        /// <paramref name="offset"/> then always means: whichever pair is currently least advanced
        /// leaps forward by a full cycle (covering every pair once), the same way every other bank
        /// leapfrogs in this codebase, just counted in half-bank units when <see
        /// cref="NeuropixelsV1Preset.UnitsPerBank"/> is 2. At offset 0 this reduces to plain <see
        /// cref="NeuropixelsV1Preset.RelativeBanks"/>, since nothing has advanced yet.
        /// </para>
        /// </remarks>
        /// <param name="preset">The preset to apply. Passing <see cref="NeuropixelsV1Preset.None"/>
        /// leaves the existing channel map unchanged.</param>
        /// <param name="offset">The tip offset, in <see cref="NeuropixelsV1Preset.UnitsPerBank"/>-sized
        /// steps, by which the whole preset pattern is shifted deeper.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="preset"/> is not
        /// <see cref="NeuropixelsV1Preset.None"/> and its widest relative bank exceeds this probe's
        /// bank range even at offset 0. Thrown before any state is changed.</exception>
        public override void SelectPreset(NeuropixelsV1Preset preset, int offset)
        {
            if (preset == NeuropixelsV1Preset.None)
                return;

            int maxOffset = MaxOffset(preset);
            if (maxOffset < 0)
                throw new ArgumentException(
                    $"Preset '{preset.DisplayName}' cannot be applied to this probe: its widest relative " +
                    $"bank exceeds the probe's {Variant.BankCount} bank(s), even at offset 0.", nameof(preset));

            int clampedOffset = Math.Max(0, Math.Min(offset, maxOffset));
            int unitsPerBank = preset.UnitsPerBank;
            int distinctBankValues = preset.RelativeBanks.Max() + 1;
            int totalVirtualUnits = unitsPerBank * distinctBankValues;
            int channelsPerUnit = NeuropixelsV1.ChannelCount / unitsPerBank;

            var contactForBank = new Dictionary<int, IReadOnlyDictionary<int, int>>();
            var contactsToEnable = new List<int>(NeuropixelsV1.ChannelCount);

            for (int channel = 0; channel < NeuropixelsV1.ChannelCount; channel++)
            {
                int unit = channel / channelsPerUnit;
                int nativeVirtual = unitsPerBank * preset.RelativeBanks[channel] + unit;
                int leaps = clampedOffset > nativeVirtual
                    ? (clampedOffset - nativeVirtual + totalVirtualUnits - 1) / totalVirtualUnits
                    : 0;
                int bank = (nativeVirtual + totalVirtualUnits * leaps) / unitsPerBank;

                if (!contactForBank.TryGetValue(bank, out var channelToContact))
                {
                    channelToContact = GetBankContactIndices(bank).ToDictionary(GetChannel, contact => contact);
                    contactForBank[bank] = channelToContact;
                }
                contactsToEnable.Add(channelToContact[channel]);
            }

            EnableElectrodes(contactsToEnable);
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
}
