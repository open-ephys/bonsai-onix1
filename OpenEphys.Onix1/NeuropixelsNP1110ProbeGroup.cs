using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// A <see cref="NeuropixelsV1ProbeGroup"/> whose channels are mapped to contact groups.
    /// </summary>
    /// <remarks>
    /// Individual contacts cannot be independently wired to channels on this probe; the atom of selection and
    /// mapping is a multi-contact, multi-channel tile.
    /// </remarks>
    public sealed class NeuropixelsNP1110ProbeGroup : NeuropixelsV1ProbeGroup
    {
        /// <summary>
        /// Per-channel-group selected banks (0, 1, or 2 of 16), indexed by channel group (0-23).
        /// </summary>
        /// <remarks>
        /// This is the true contact-to-channel source of truth. ChannelMap is a derived view. This field must
        /// be reconstructed on every load from ChannelMap directly when deserializing/copying an existing
        /// configuration (RestoreChannelGroupBanksFromChannelMap/the copy constructor), or defaulted to bank
        /// 0 for every group when there's nothing to restore.
        /// </remarks>
        readonly List<int>[] channelGroupBanks =
            Enumerable.Range(0, NeuropixelsV1VariantRegistry.Np1110ChannelGroupCount).Select(_ => new List<int>()).ToArray();

        NeuropixelsV1ColumnPattern columnPattern = NeuropixelsV1ColumnPattern.All;

        // ColumnPattern is round-tripped through a probe-level annotation. It is written whenever the
        // property changes (see the ColumnPattern setter), read back in the JSON constructor below.
        internal const string ColumnPatternAnnotationKey = "column_pattern";

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsNP1110ProbeGroup"/> class by
        /// copying an existing probe group.
        /// </summary>
        /// <param name="probeGroup">The probe group to copy.</param>
        public NeuropixelsNP1110ProbeGroup(NeuropixelsNP1110ProbeGroup probeGroup)
            : base(probeGroup)
        {
            columnPattern = probeGroup.columnPattern;
            for (int channelGroup = 0; channelGroup < channelGroupBanks.Length; channelGroup++)
                channelGroupBanks[channelGroup] = new List<int>(probeGroup.channelGroupBanks[channelGroup]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsNP1110ProbeGroup"/> class from
        /// deserialized ProbeInterface data.
        /// </summary>
        /// <param name="specification">The ProbeInterface specification string.</param>
        /// <param name="version">The ProbeInterface version string.</param>
        /// <param name="probes">The array of probes deserialized from the ProbeInterface file.</param>
        [JsonConstructor]
        internal NeuropixelsNP1110ProbeGroup(string specification, string version, Probe[] probes)
            : base(specification, version, probes)
        {
            if (!Variant.HasChannelGroupSelection)
                throw new ArgumentException(
                    $"{Probe.Annotations.ModelName} does not support per-channel-group bank selection; use " +
                    $"{nameof(NeuropixelsV1ContactProbeGroup)} instead.");

            var savedPattern = Probe.Annotations.GetAnnotation<string>(ColumnPatternAnnotationKey);
            columnPattern = !string.IsNullOrEmpty(savedPattern) &&
                Enum.TryParse(savedPattern, out NeuropixelsV1ColumnPattern parsed)
                    ? parsed
                    : NeuropixelsV1ColumnPattern.All;

            if (HasChannelMap)
            {
                RestoreChannelGroupBanksFromChannelMap();
            }
            else
            {
                for (int channelGroup = 0; channelGroup < NeuropixelsV1VariantRegistry.Np1110ChannelGroupCount; channelGroup++)
                    SelectElectrodeGroup(channelGroup, 0);
            }
        }

        internal override NeuropixelsV1ProbeGroup Clone() => new NeuropixelsNP1110ProbeGroup(this);

        /// <remarks>
        /// The register's 384 non-reference bits are one per (channel group, bank) tile here, not
        /// one per channel. NB: <see cref="NeuropixelsV1ProbeGroup.ChannelMap"/>/
        /// <see cref="NeuropixelsV1Variant.ShankRegisterPosition"/> (the path
        /// <see cref="NeuropixelsV1ContactProbeGroup"/> uses) don't apply here. See
        /// <see cref="NeuropixelsV1VariantRegistry.Np1110GroupBankRegisterIndex"/>.
        /// <c>EN_A</c>/<c>EN_B</c> reflect <see cref="ColumnPattern"/>: Inner = (1,0),
        /// Outer = (0,1), All = (1,1).
        /// </remarks>
        internal override void SetShankConfigurationBits(NeuropixelsV1ShankRegisterLayout layout, BitArray shankBits)
        {
            for (int channelGroup = 0; channelGroup < NeuropixelsV1VariantRegistry.Np1110ChannelGroupCount; channelGroup++)
            {
                foreach (var bank in GetSelectedBanks(channelGroup))
                {
                    int lookupIndex = NeuropixelsV1VariantRegistry.Np1110GroupBankRegisterIndex[
                        channelGroup * NeuropixelsV1VariantRegistry.Np1110BanksPerChannelGroup + bank];
                    shankBits[layout.GetElectrodeBitIndex(lookupIndex)] = true;
                }
            }

            var (enableA, enableB) = columnPattern switch
            {
                NeuropixelsV1ColumnPattern.Inner => (true, false),
                NeuropixelsV1ColumnPattern.Outer => (false, true),
                _ => (true, true), // All
            };
            shankBits[layout.RegisterBits - 2] = enableA;
            shankBits[layout.RegisterBits - 1] = enableB;
        }

        /// <summary>
        /// Reconstructs <see cref="channelGroupBanks"/> from an already-deserialized
        /// <see cref="NeuropixelsV1ProbeGroup.ChannelMap"/>, the only state a ProbeInterface JSON file
        /// actually carries. For each active channel, its channel group and the bank of its wired
        /// contact are both derivable (<see cref="NeuropixelsV1VariantRegistry.Np1110ChannelGroupOf"/>,
        /// <see cref="NeuropixelsV1ProbeGroup.GetBank"/>), so this recovers the full per-group bank
        /// selection exactly, including two-bank (zig-zag) groups, with no ambiguity.
        /// </summary>
        void RestoreChannelGroupBanksFromChannelMap()
        {
            foreach (var kvp in ChannelMap)
            {
                int channel = kvp.Key;
                int contact = kvp.Value;
                int channelGroup = NeuropixelsV1VariantRegistry.Np1110ChannelGroupOf(channel);
                int bank = GetBank(contact);
                if (!channelGroupBanks[channelGroup].Contains(bank))
                    channelGroupBanks[channelGroup].Add(bank);
            }
        }

        /// <summary>
        /// Gets or sets which physical columns are enabled across the whole probe.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown by the setter if some channel group currently has two banks selected whose surviving
        /// (column-masked) channels would collide under the new value. Deselect one of that group's banks
        /// first.
        /// </exception>
        public NeuropixelsV1ColumnPattern ColumnPattern
        {
            get => columnPattern;
            set
            {
                if (value == columnPattern)
                    return;

                var previous = columnPattern;
                columnPattern = value;

                for (int channelGroup = 0; channelGroup < channelGroupBanks.Length; channelGroup++)
                {
                    var banks = channelGroupBanks[channelGroup];
                    if (banks.Count == 2 && SurvivingChannelsCollide(channelGroup, banks[0], banks[1]))
                    {
                        columnPattern = previous;
                        throw new InvalidOperationException(
                            $"Cannot change {nameof(ColumnPattern)} to {value}: channel group {channelGroup} has " +
                            $"banks {banks[0]} and {banks[1]} selected, which would drive overlapping channels " +
                            $"under {value}. Deselect one of them first.");
                    }
                }

                Probe.Annotations.SetAnnotation(ColumnPatternAnnotationKey, value.ToString());
                RebuildChannelMap();
            }
        }

        /// <summary>
        /// Returns the channel group (0-23) that <paramref name="contactIndex"/> belongs to.
        /// </summary>
        /// <remarks>
        /// A contact's channel group is entirely determined by its channel (see
        /// <see cref="NeuropixelsV1ProbeGroup.GetChannel"/>), not its position within a bank: every one
        /// of a group's 16 candidate contacts, across all 16 banks, shares the same channel group.
        /// </remarks>
        /// <param name="contactIndex">The zero-based contact index across all electrodes on the probe.</param>
        /// <returns>The zero-based channel group index (0-23).</returns>
        public int GetChannelGroup(int contactIndex) =>
            NeuropixelsV1VariantRegistry.Np1110ChannelGroupOf(GetChannel(contactIndex));

        /// <summary>
        /// Selects which bank(s) feed the given channel group. This is the only supported way to select
        /// electrodes on this probe: individual contacts cannot be independently wired to channels,
        /// since the atom of selection is a whole channel-group tile.
        /// </summary>
        /// <remarks>
        /// Passing no banks deselects the group. Passing two banks is only valid when
        /// <see cref="ColumnPattern"/> is <see cref="NeuropixelsV1ColumnPattern.Inner"/> or
        /// <see cref="NeuropixelsV1ColumnPattern.Outer"/>, and only if the two banks' surviving
        /// (column-masked) channels don't overlap.
        /// NB: under <see cref="NeuropixelsV1ColumnPattern.All"/>, any two distinct banks for the
        /// same group always drive the identical, unmasked 16-channel set and would collide, so at
        /// most one bank is ever valid there.
        /// </remarks>
        /// <param name="channelGroup">The zero-based channel group index (0-23).</param>
        /// <param name="banks">Zero, one, or two zero-based bank indices (0-15) to select for this group.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="channelGroup"/> or any entry of <paramref name="banks"/> is out of range.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="banks"/> has more than two entries, contains a duplicate, or its two
        /// banks would drive overlapping channels under the current <see cref="ColumnPattern"/>.
        /// </exception>
        public void SelectElectrodeGroup(int channelGroup, params int[] banks)
        {
            if (channelGroup < 0 || channelGroup >= NeuropixelsV1VariantRegistry.Np1110ChannelGroupCount)
                throw new ArgumentOutOfRangeException(nameof(channelGroup),
                    $"Probe has {NeuropixelsV1VariantRegistry.Np1110ChannelGroupCount} channel groups; got {channelGroup}.");

            banks ??= Array.Empty<int>();

            if (banks.Length > 2)
                throw new ArgumentException("At most two banks may be selected for a single channel group.", nameof(banks));

            foreach (var bank in banks)
                if (bank < 0 || bank >= Variant.BankCount)
                    throw new ArgumentOutOfRangeException(nameof(banks), $"Probe has {Variant.BankCount} bank(s); got {bank}.");

            if (banks.Length == 2)
            {
                if (banks[0] == banks[1])
                    throw new ArgumentException("The same bank cannot be selected twice for one channel group.", nameof(banks));

                if (SurvivingChannelsCollide(channelGroup, banks[0], banks[1]))
                    throw new ArgumentException(
                        $"Banks {banks[0]} and {banks[1]} cannot both be selected for channel group {channelGroup}: " +
                        $"under the current {nameof(ColumnPattern)} ({columnPattern}), they would drive overlapping channels.",
                        nameof(banks));
            }

            channelGroupBanks[channelGroup] = banks.ToList();
            RebuildChannelMap();
        }

        /// <summary>
        /// Selects bank <paramref name="bank"/> for every channel group on the probe at once, replacing
        /// each group's existing selection, and resets <see cref="ColumnPattern"/> to
        /// <see cref="NeuropixelsV1ColumnPattern.All"/>.
        /// </summary>
        /// <param name="bank">The zero-based bank index.</param>
        public override void SelectBank(int bank)
        {
            for (int channelGroup = 0; channelGroup < NeuropixelsV1VariantRegistry.Np1110ChannelGroupCount; channelGroup++)
                SelectElectrodeGroup(channelGroup, bank);

            ColumnPattern = NeuropixelsV1ColumnPattern.All;
        }

        /// <summary>
        /// Returns every preset available for this probe: "Full" plus the named linear patterns
        /// registered for this variant.
        /// </summary>
        /// <returns>The list of presets, starting with <see cref="NeuropixelsV1Preset.None"/>.</returns>
        public override IReadOnlyList<NeuropixelsV1Preset> GetPresets()
        {
            var presets = new List<NeuropixelsV1Preset> { NeuropixelsV1Preset.None };
            presets.AddRange(NeuropixelsV1VariantRegistry.Np1110Presets);
            return presets;
        }

        /// <summary>
        /// Applies <paramref name="preset"/> at <paramref name="offset"/> banks from the tip, replacing
        /// the existing channel map and setting <see cref="ColumnPattern"/> to
        /// <see cref="NeuropixelsV1Preset.RequiredColumnPattern"/>.
        /// </summary>
        /// <remarks>
        /// <paramref name="offset"/> is clamped to <see cref="NeuropixelsV1ProbeGroup.MaxOffset"/> rather
        /// than rejected. Under a <see cref="NeuropixelsV1Preset.RequiredColumnPattern"/> other than
        /// <see cref="NeuropixelsV1ColumnPattern.All"/>, each channel group additionally gets its
        /// preset entry's +4 zig-zag complement.
        /// <para>
        /// A channel group's selected bank(s) cannot simply be <paramref name="offset"/> plus its
        /// <see cref="NeuropixelsV1Preset.RelativeBanks"/> entry: which physical column a channel
        /// group's contacts land on depends on the bank's value modulo the preset's number of distinct
        /// relative-bank values (<c>D</c>, e.g. 4 for the two Inner/Outer linear presets, 2 for the two
        /// All-pattern linear presets), so a group can only move in leaps of <c>D</c> banks without
        /// drifting onto the wrong column. At offset 0 every one of the <c>D</c> possible bank values
        /// (0..D-1, plus each one's own +4 partner where applicable) is already claimed by some group, so
        /// shifting the whole pattern one bank deeper means advancing whichever group is currently
        /// shallowest by a full leap of <c>D</c>, vacating its old (now-shallowest) bank and extending the
        /// window by one new (deepest) bank -- the same net effect as a plain +1 shift of the whole
        /// window, applied per group in units its column can tolerate.
        /// </para>
        /// </remarks>
        /// <param name="preset">The preset to apply. Passing <see cref="NeuropixelsV1Preset.None"/>
        /// leaves the existing channel map unchanged.</param>
        /// <param name="offset">The tip offset, in banks, by which the whole preset pattern is shifted
        /// deeper.</param>
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
            bool needsPartner = preset.RequiredColumnPattern.HasValue &&
                preset.RequiredColumnPattern != NeuropixelsV1ColumnPattern.All;
            int distinctBankValues = preset.RelativeBanks.Max() + 1;

            for (int channelGroup = 0; channelGroup < NeuropixelsV1VariantRegistry.Np1110ChannelGroupCount; channelGroup++)
                SelectElectrodeGroup(channelGroup);

            ColumnPattern = preset.RequiredColumnPattern ?? NeuropixelsV1ColumnPattern.All;

            for (int channelGroup = 0; channelGroup < preset.RelativeBanks.Count; channelGroup++)
            {
                int nativeBank = preset.RelativeBanks[channelGroup];
                int leaps = clampedOffset > nativeBank
                    ? (clampedOffset - nativeBank + distinctBankValues - 1) / distinctBankValues
                    : 0;
                int bank = nativeBank + distinctBankValues * leaps;
                if (needsPartner)
                    SelectElectrodeGroup(channelGroup, bank, bank + 4);
                else
                    SelectElectrodeGroup(channelGroup, bank);
            }
        }

        /// <summary>
        /// Returns the bank(s) currently selected for <paramref name="channelGroup"/> (0, 1, or 2 of 16).
        /// Used by <see cref="NeuropixelsV1.MakeShankBits"/> to generate the shank configuration register.
        /// </summary>
        internal IReadOnlyList<int> GetSelectedBanks(int channelGroup) => channelGroupBanks[channelGroup];

        /// <summary>
        /// Returns true if the channels that survive column masking (see <see cref="ColumnPattern"/>) for
        /// <paramref name="bank1"/> and <paramref name="bank2"/> of <paramref name="channelGroup"/>
        /// overlap. Under <see cref="NeuropixelsV1ColumnPattern.All"/> no column is masked, so any two
        /// distinct banks of the same group always collide (both drive that group's full, identical
        /// 16-channel set).
        /// NB: this single check is what enforces "at most one bank per group under All" without
        /// needing a separate rule.
        /// </summary>
        bool SurvivingChannelsCollide(int channelGroup, int bank1, int bank2)
        {
            var survivors1 = new HashSet<int>(GetSurvivingChannels(channelGroup, bank1));
            return survivors1.Overlaps(GetSurvivingChannels(channelGroup, bank2));
        }

        /// <summary>
        /// Returns the channels that <paramref name="channelGroup"/>/<paramref name="bank"/> actually
        /// drives once column masking is applied: the subset of the tile's 16 contacts sitting on a
        /// currently-enabled physical column (see <see cref="ColumnPattern"/>), mapped to their channel.
        /// </summary>
        /// <remarks>
        /// Internal rather than private so the Design layer can pin exactly the channels a specific bank
        /// currently produces (see <c>NeuropixelsV1ImGuiDialog.GetChannelsToPin</c>), not a channel
        /// group's full 16-channel set.
        /// NB: pinning the full set would incorrectly block the complementary row-crossed bank a
        /// second bank selection needs under <see cref="NeuropixelsV1ColumnPattern.Inner"/>/
        /// <see cref="NeuropixelsV1ColumnPattern.Outer"/>, since that bank's surviving channels are
        /// exactly the ones the first bank does *not* currently produce.
        /// </remarks>
        internal IEnumerable<int> GetSurvivingChannels(int channelGroup, int bank) =>
            GetChannelGroupBankContacts(channelGroup, bank).Where(IsColumnEnabled).Select(GetChannel);

        /// <summary>
        /// Returns the (up to 16) contact indices belonging to the given channel group/bank tile, i.e.
        /// the contacts within <paramref name="bank"/>'s contiguous window whose channel (via the
        /// existing <see cref="NeuropixelsV1ProbeGroup.GetChannel"/>) belongs to
        /// <paramref name="channelGroup"/>'s fixed 16-channel set.
        /// </summary>
        IEnumerable<int> GetChannelGroupBankContacts(int channelGroup, int bank)
        {
            var channels = new HashSet<int>(NeuropixelsV1VariantRegistry.Np1110ChannelGroupChannels(channelGroup));
            int start = bank * NeuropixelsV1.ChannelCount;

            for (int contactIdx = start; contactIdx < start + NeuropixelsV1.ChannelCount; contactIdx++)
                if (channels.Contains(GetChannel(contactIdx)))
                    yield return contactIdx;
        }

        /// <summary>
        /// Returns true if <paramref name="contactIndex"/>'s physical column is enabled under the current
        /// <see cref="ColumnPattern"/>. "Column" is the across-shank axis (contact x-position / 6 um
        /// pitch, giving a 0-7 physical column index), not to be confused with a channel group.
        /// </summary>
        /// <remarks>
        /// Internal rather than private so the Design layer can drive contact-blocking for disabled
        /// columns (see <c>NeuropixelsV1ImGuiDialog</c>) without duplicating the active-column sets.
        /// NB: both this method and that UI read the same <see cref="NeuropixelsV1VariantRegistry.Np1110ActiveColumnsByPattern"/>.
        /// </remarks>
        internal bool IsColumnEnabled(int contactIndex)
        {
            if (!NeuropixelsV1VariantRegistry.Np1110ActiveColumnsByPattern.TryGetValue(columnPattern, out var activeColumns))
                return true; // All: unrestricted.

            int column = (int)Math.Round(Probe.Contacts[contactIndex].PosX / 6.0);
            return activeColumns.Contains(column);
        }

        /// <summary>
        /// Rebuilds the channel map from scratch based on the current <see cref="channelGroupBanks"/>
        /// selections and <see cref="ColumnPattern"/>. This entirely replaces per-contact enable as the
        /// way <see cref="NeuropixelsV1ProbeGroup.ChannelMap"/> changes.
        /// </summary>
        void RebuildChannelMap()
        {
            var assignments = new Dictionary<int, int>();

            for (int channelGroup = 0; channelGroup < channelGroupBanks.Length; channelGroup++)
                foreach (var bank in channelGroupBanks[channelGroup])
                    foreach (var contactIdx in GetChannelGroupBankContacts(channelGroup, bank).Where(IsColumnEnabled))
                        assignments[GetChannel(contactIdx)] = contactIdx;

            ChannelWiring.UnwireChannels(this, 0);
            ChannelWiring.WireChannels(this, 0, assignments);
        }
    }

    /// <summary>
    /// Specifies which physical columns are enabled on a <see cref="NeuropixelsNP1110ProbeGroup"/>
    /// (currently just UHD Switchable, NP1110). See <see cref="NeuropixelsNP1110ProbeGroup.ColumnPattern"/>.
    /// </summary>
    public enum NeuropixelsV1ColumnPattern
    {
        /// <summary>
        /// Every column is enabled. The default.
        /// </summary>
        All,

        /// <summary>
        /// Only the inner columns (physical column indices 1, 3, 4, 6) are enabled.
        /// </summary>
        Inner,

        /// <summary>
        /// Only the outer columns (physical column indices 0, 2, 5, 7) are enabled.
        /// </summary>
        Outer
    }
}
