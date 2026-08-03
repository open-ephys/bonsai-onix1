using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Defines a constrained probeInterface compatible probe group for Neuropixels 2.0 single-shank probes.
    /// </summary>
    public class NeuropixelsV2SingleShankProbeGroup : NeuropixelsV2ProbeGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2SingleShankProbeGroup"/> class using
        /// the default electrode geometry.
        /// </summary>
        public NeuropixelsV2SingleShankProbeGroup()
            : this(ProbeGroupResource.LoadDefault<NeuropixelsV2SingleShankProbeGroup>("NP2003.json"))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2SingleShankProbeGroup"/> class by
        /// copying an existing probe group.
        /// </summary>
        /// <param name="probeGroup">The probe group to copy.</param>
        public NeuropixelsV2SingleShankProbeGroup(NeuropixelsV2SingleShankProbeGroup probeGroup)
            : base(probeGroup)
        {
            if (base.ChannelMap is null)
                SelectPreset(NeuropixelsV2SingleShankChannelPreset.BankA);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2SingleShankProbeGroup"/> class from
        /// deserialized ProbeInterface data.
        /// </summary>
        /// <param name="specification">The ProbeInterface specification string.</param>
        /// <param name="version">The ProbeInterface version string.</param>
        /// <param name="probes">The array of probes deserialized from the ProbeInterface file.</param>
        [JsonConstructor]
        public NeuropixelsV2SingleShankProbeGroup(string specification, string version, Probe[] probes)
            : base(specification, version, probes)
        {
            if (NumberOfContacts != NeuropixelsV2.ElectrodesPerShank)
                throw new ArgumentException($"Invalid number of contacts; expected {NeuropixelsV2.ElectrodesPerShank}, but found {NumberOfContacts}.");
            if (base.ChannelMap is null)
                SelectPreset(NeuropixelsV2SingleShankChannelPreset.BankA);
        }

        /// <summary>
        /// Returns the channel map for this probe, mapping each acquisition channel index to its
        /// active contact index.
        /// </summary>
        /// <remarks>
        /// This property hides <see cref="ProbeGroup.ChannelMap"/>, which is nullable, to enforce the
        /// invariant that a Neuropixels 2.0 single-shank probe group always has a valid channel
        /// map after construction. If no channel map was present in the deserialized data,
        /// <see cref="NeuropixelsV2SingleShankChannelPreset.BankA"/> is applied automatically
        /// during construction.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the channel map is null.</exception>
        [JsonIgnore]
        public new IReadOnlyDictionary<int, int> ChannelMap =>
            base.ChannelMap ?? throw new InvalidOperationException("Neuropixels probes must have a valid channel map.");

        /// <inheritdoc/>
        public override int GetChannel(int contactIndex)
        {
            var bank = (NeuropixelsV2Bank)(contactIndex / NeuropixelsV2.ChannelCount);
            int bankIndex = contactIndex % NeuropixelsV2.ChannelCount;
            int block = bankIndex / ElectrodesPerBlock;
            int row = bankIndex % ElectrodesPerBlock / ElectrodesPerRow;
            int columnIndex = contactIndex % 2;
            return GetSingleShankChannelNumber(bank, block, row, columnIndex);
        }

        /// <inheritdoc/>
        public override Array GetReferenceEnumValues()
            => Enum.GetValues(typeof(NeuropixelsV2SingleShankReference));

        /// <inheritdoc/>
        public override Array GetChannelPresets()
            => Enum.GetValues(typeof(NeuropixelsV2SingleShankChannelPreset));

        /// <inheritdoc/>
        public override void SelectPreset(Enum preset)
        {
            var p = (NeuropixelsV2SingleShankChannelPreset)preset;
            if (p != NeuropixelsV2SingleShankChannelPreset.None) SelectPreset(p);
        }

        /// <inheritdoc/>
        public override void SelectBank(int shank, NeuropixelsV2Bank bank)
        {
            if (shank != 0)
                throw new ArgumentOutOfRangeException(nameof(shank), $"Single-shank probe only has shank 0; got {shank}.");
            var preset = bank switch
            {
                NeuropixelsV2Bank.A => NeuropixelsV2SingleShankChannelPreset.BankA,
                NeuropixelsV2Bank.B => NeuropixelsV2SingleShankChannelPreset.BankB,
                NeuropixelsV2Bank.C => NeuropixelsV2SingleShankChannelPreset.BankC,
                NeuropixelsV2Bank.D => NeuropixelsV2SingleShankChannelPreset.BankD,
                _ => throw new ArgumentOutOfRangeException(nameof(bank))
            };
            SelectPreset(preset);
        }

        /// <summary>
        /// Configures the channel map to the specified single-shank channel preset.
        /// </summary>
        /// <param name="preset">The preset to apply.</param>
        public void SelectPreset(NeuropixelsV2SingleShankChannelPreset preset)
        {
            int presetInt = (int)preset;
            int start = presetInt < 3
                ? presetInt * NeuropixelsV2.ChannelCount
                : NeuropixelsV2.ElectrodesPerShank - NeuropixelsV2.ChannelCount;
            EnableElectrodes(Enumerable.Range(start, NeuropixelsV2.ChannelCount));
        }

        const int ElectrodesPerBlock = 32;
        const int ElectrodesPerRow = 2;

        static int GetSingleShankChannelNumber(NeuropixelsV2Bank bank, int block, int row, int columnIndex)
        {
            const int MaxBlockValue = 11;
            const int MaxRowValue = 15;

            if (block > MaxBlockValue || block < 0)
                throw new ArgumentOutOfRangeException($"Block value out of range [0, {MaxBlockValue}]: {block}");
            if (row > MaxRowValue || row < 0)
                throw new ArgumentOutOfRangeException($"Row value out of range [0, {MaxRowValue}]: {row}");
            if (columnIndex > 1 || columnIndex < 0)
                throw new ArgumentOutOfRangeException($"Column index out of range [0, 1]: {columnIndex}");

            const int HalfBlock = 16;

            return (bank, columnIndex) switch
            {
                (NeuropixelsV2Bank.A, 0) => row * ElectrodesPerRow + block * ElectrodesPerBlock,
                (NeuropixelsV2Bank.A, 1) => row * ElectrodesPerRow + block * ElectrodesPerBlock + 1,
                (NeuropixelsV2Bank.B, 0) => (row * 7 % HalfBlock) * ElectrodesPerRow + block * ElectrodesPerBlock,
                (NeuropixelsV2Bank.B, 1) => ((row * 7 + 4) % HalfBlock) * ElectrodesPerRow + block * ElectrodesPerBlock + 1,
                (NeuropixelsV2Bank.C, 0) => (row * 5 % HalfBlock) * ElectrodesPerRow + block * ElectrodesPerBlock,
                (NeuropixelsV2Bank.C, 1) => ((row * 5 + 8) % HalfBlock) * ElectrodesPerRow + block * ElectrodesPerBlock + 1,
                (NeuropixelsV2Bank.D, 0) => (row * 3 % HalfBlock) * ElectrodesPerRow + block * ElectrodesPerBlock,
                (NeuropixelsV2Bank.D, 1) => ((row * 3 + 12) % HalfBlock) * ElectrodesPerRow + block * ElectrodesPerBlock + 1,
                _ => throw new NotImplementedException($"Invalid {nameof(NeuropixelsV2Bank)} value.")
            };
        }
    }

    /// <summary>
    /// Specifies a predefined electrode selection pattern for a Neuropixels 2.0 single-shank probe.
    /// </summary>
    public enum NeuropixelsV2SingleShankChannelPreset
    {
        /// <summary
        /// >Selects all 384 channels in bank A (electrode indices 0–383).
        /// </summary>
        BankA,
        /// <summary>
        /// Selects all 384 channels in bank B (electrode indices 384–767).
        /// </summary>
        BankB,
        /// <summary>
        /// Selects all 384 channels in bank C (electrode indices 768–1151).
        /// </summary>
        BankC,
        /// <summary>
        /// Selects the 384 deepest electrodes (indices 896–1279), spanning the upper portion of bank C and all of bank D.
        /// </summary>
        BankD,
        /// <summary>
        /// No preset applied; leaves the existing channel map unchanged.
        /// </summary>
        None
    }
}
