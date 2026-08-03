using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Defines a ProbeInterface-compatible probe group for Neuropixels 2.0 quad-shank probes.
    /// </summary>
    public class NeuropixelsV2QuadShankProbeGroup : NeuropixelsV2ProbeGroup
    {
        const int ElectrodesPerBlock = 48;
        const int ShankCount = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeGroup"/> class using
        /// the default electrode geometry.
        /// </summary>
        public NeuropixelsV2QuadShankProbeGroup()
            : this(ProbeGroupResource.LoadDefault<NeuropixelsV2QuadShankProbeGroup>("NP2013.json"))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeGroup"/> class by
        /// copying an existing probe group.
        /// </summary>
        /// <param name="probeGroup">The probe group to copy.</param>
        public NeuropixelsV2QuadShankProbeGroup(NeuropixelsV2QuadShankProbeGroup probeGroup)
            : base(probeGroup)
        {
            if (base.ChannelMap is null)
                SelectPreset(NeuropixelsV2QuadShankChannelPreset.Shank0BankA);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeGroup"/> class from
        /// deserialized ProbeInterface data.
        /// </summary>
        /// <param name="specification">The ProbeInterface specification string.</param>
        /// <param name="version">The ProbeInterface version string.</param>
        /// <param name="probes">The array of probes deserialized from the ProbeInterface file.</param>
        [JsonConstructor]
        public NeuropixelsV2QuadShankProbeGroup(string specification, string version, Probe[] probes)
            : base(specification, version, probes)
        {
            if (NumberOfContacts != NeuropixelsV2.ElectrodesPerShank * ShankCount)
                throw new ArgumentException($"Invalid number of contacts; expected {NeuropixelsV2.ElectrodesPerShank * 4}, but found {NumberOfContacts}.");

            if (base.ChannelMap is null)
                SelectPreset(NeuropixelsV2QuadShankChannelPreset.Shank0BankA);
        }

        /// <summary>
        /// Returns the channel map for this probe, mapping each acquisition channel index to its
        /// active contact index.
        /// </summary>
        /// <remarks>
        /// This property hides <see cref="ProbeGroup.ChannelMap"/>, which is nullable, to enforce the
        /// invariant that a Neuropixels 2.0 quad-shank probe group always has a valid channel map
        /// after construction. If no channel map was present in the deserialized data,
        /// <see cref="NeuropixelsV2QuadShankChannelPreset.Shank0BankA"/> is applied automatically
        /// during construction.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the channel map is null.</exception>
        [JsonIgnore]
        public new IReadOnlyDictionary<int, int> ChannelMap =>
            base.ChannelMap ?? throw new InvalidOperationException("Neuropixels probes must have a valid channel map.");

        /// <inheritdoc/>
        public override int GetChannel(int contactIndex)
        {
            int intra = contactIndex % NeuropixelsV2.ElectrodesPerShank;
            int shank = contactIndex / NeuropixelsV2.ElectrodesPerShank;
            int block = (intra % NeuropixelsV2.ChannelCount) / ElectrodesPerBlock;
            int blockIndex = intra % ElectrodesPerBlock;
            return GetQuadShankChannelNumber(shank, block, blockIndex);
        }

        /// <inheritdoc/>
        public override Array GetReferenceEnumValues() 
            => Enum.GetValues(typeof(NeuropixelsV2QuadShankReference));

        /// <inheritdoc/>
        public override Array GetChannelPresets()       
            => Enum.GetValues(typeof(NeuropixelsV2QuadShankChannelPreset));

        /// <inheritdoc/>
        public override void SelectPreset(Enum preset)
        {
            var p = (NeuropixelsV2QuadShankChannelPreset)preset;
            if (p != NeuropixelsV2QuadShankChannelPreset.None) SelectPreset(p);
        }

        /// <inheritdoc/>
        public override void SelectBank(int shank, NeuropixelsV2Bank bank)
        {
            var preset = (shank, bank) switch
            {
                (0, NeuropixelsV2Bank.A) => NeuropixelsV2QuadShankChannelPreset.Shank0BankA,
                (0, NeuropixelsV2Bank.B) => NeuropixelsV2QuadShankChannelPreset.Shank0BankB,
                (0, NeuropixelsV2Bank.C) => NeuropixelsV2QuadShankChannelPreset.Shank0BankC,
                (0, NeuropixelsV2Bank.D) => NeuropixelsV2QuadShankChannelPreset.Shank0BankD,
                (1, NeuropixelsV2Bank.A) => NeuropixelsV2QuadShankChannelPreset.Shank1BankA,
                (1, NeuropixelsV2Bank.B) => NeuropixelsV2QuadShankChannelPreset.Shank1BankB,
                (1, NeuropixelsV2Bank.C) => NeuropixelsV2QuadShankChannelPreset.Shank1BankC,
                (1, NeuropixelsV2Bank.D) => NeuropixelsV2QuadShankChannelPreset.Shank1BankD,
                (2, NeuropixelsV2Bank.A) => NeuropixelsV2QuadShankChannelPreset.Shank2BankA,
                (2, NeuropixelsV2Bank.B) => NeuropixelsV2QuadShankChannelPreset.Shank2BankB,
                (2, NeuropixelsV2Bank.C) => NeuropixelsV2QuadShankChannelPreset.Shank2BankC,
                (2, NeuropixelsV2Bank.D) => NeuropixelsV2QuadShankChannelPreset.Shank2BankD,
                (3, NeuropixelsV2Bank.A) => NeuropixelsV2QuadShankChannelPreset.Shank3BankA,
                (3, NeuropixelsV2Bank.B) => NeuropixelsV2QuadShankChannelPreset.Shank3BankB,
                (3, NeuropixelsV2Bank.C) => NeuropixelsV2QuadShankChannelPreset.Shank3BankC,
                (3, NeuropixelsV2Bank.D) => NeuropixelsV2QuadShankChannelPreset.Shank3BankD,
                _ => throw new ArgumentOutOfRangeException(
                    $"No preset for shank {shank}, bank {bank} on a quad-shank probe.")
            };
            SelectPreset(preset);
        }

        /// <summary>
        /// Configures the channel map to the specified quad-shank channel preset.
        /// </summary>
        /// <param name="preset">The preset to apply.</param>
        public void SelectPreset(NeuropixelsV2QuadShankChannelPreset preset)
        {
            if (preset == NeuropixelsV2QuadShankChannelPreset.None) return;
            EnableElectrodes(preset switch
            {
                NeuropixelsV2QuadShankChannelPreset.Shank0BankA => SingleShankRange(0, NeuropixelsV2Bank.A),
                NeuropixelsV2QuadShankChannelPreset.Shank0BankB => SingleShankRange(0, NeuropixelsV2Bank.B),
                NeuropixelsV2QuadShankChannelPreset.Shank0BankC => SingleShankRange(0, NeuropixelsV2Bank.C),
                NeuropixelsV2QuadShankChannelPreset.Shank0BankD => SingleShankRange(0, NeuropixelsV2Bank.D),
                NeuropixelsV2QuadShankChannelPreset.Shank1BankA => SingleShankRange(1, NeuropixelsV2Bank.A),
                NeuropixelsV2QuadShankChannelPreset.Shank1BankB => SingleShankRange(1, NeuropixelsV2Bank.B),
                NeuropixelsV2QuadShankChannelPreset.Shank1BankC => SingleShankRange(1, NeuropixelsV2Bank.C),
                NeuropixelsV2QuadShankChannelPreset.Shank1BankD => SingleShankRange(1, NeuropixelsV2Bank.D),
                NeuropixelsV2QuadShankChannelPreset.Shank2BankA => SingleShankRange(2, NeuropixelsV2Bank.A),
                NeuropixelsV2QuadShankChannelPreset.Shank2BankB => SingleShankRange(2, NeuropixelsV2Bank.B),
                NeuropixelsV2QuadShankChannelPreset.Shank2BankC => SingleShankRange(2, NeuropixelsV2Bank.C),
                NeuropixelsV2QuadShankChannelPreset.Shank2BankD => SingleShankRange(2, NeuropixelsV2Bank.D),
                NeuropixelsV2QuadShankChannelPreset.Shank3BankA => SingleShankRange(3, NeuropixelsV2Bank.A),
                NeuropixelsV2QuadShankChannelPreset.Shank3BankB => SingleShankRange(3, NeuropixelsV2Bank.B),
                NeuropixelsV2QuadShankChannelPreset.Shank3BankC => SingleShankRange(3, NeuropixelsV2Bank.C),
                NeuropixelsV2QuadShankChannelPreset.Shank3BankD => SingleShankRange(3, NeuropixelsV2Bank.D),
                NeuropixelsV2QuadShankChannelPreset.AllShanks0_95 => AllShanksRange(0),
                NeuropixelsV2QuadShankChannelPreset.AllShanks96_191 => AllShanksRange(96),
                NeuropixelsV2QuadShankChannelPreset.AllShanks192_287 => AllShanksRange(192),
                NeuropixelsV2QuadShankChannelPreset.AllShanks288_383 => AllShanksRange(288),
                NeuropixelsV2QuadShankChannelPreset.AllShanks384_479 => AllShanksRange(384),
                NeuropixelsV2QuadShankChannelPreset.AllShanks480_575 => AllShanksRange(480),
                NeuropixelsV2QuadShankChannelPreset.AllShanks576_671 => AllShanksRange(576),
                NeuropixelsV2QuadShankChannelPreset.AllShanks672_767 => AllShanksRange(672),
                NeuropixelsV2QuadShankChannelPreset.AllShanks768_863 => AllShanksRange(768),
                NeuropixelsV2QuadShankChannelPreset.AllShanks864_959 => AllShanksRange(864),
                NeuropixelsV2QuadShankChannelPreset.AllShanks960_1055 => AllShanksRange(960),
                NeuropixelsV2QuadShankChannelPreset.AllShanks1056_1151 => AllShanksRange(1056),
                NeuropixelsV2QuadShankChannelPreset.AllShanks1152_1247 => AllShanksRange(1152),
                _ => throw new ArgumentOutOfRangeException(nameof(preset), preset, null)
            });
        }

        static IEnumerable<int> SingleShankRange(int shank, NeuropixelsV2Bank bank)
        {
            int intraStart = bank switch
            {
                NeuropixelsV2Bank.A => 0,
                NeuropixelsV2Bank.B => NeuropixelsV2.ChannelCount,
                NeuropixelsV2Bank.C => 2 * NeuropixelsV2.ChannelCount,
                NeuropixelsV2Bank.D => NeuropixelsV2.ElectrodesPerShank - NeuropixelsV2.ChannelCount,
                _ => throw new ArgumentOutOfRangeException(nameof(bank))
            };
            return Enumerable.Range(shank * NeuropixelsV2.ElectrodesPerShank + intraStart, NeuropixelsV2.ChannelCount);
        }

        static IEnumerable<int> AllShanksRange(int intraShankStart)
        {
            int contactsPerShank = NeuropixelsV2.ChannelCount / ShankCount;
            var indices = new List<int>(NeuropixelsV2.ChannelCount);
            for (int s = 0; s < ShankCount; s++)
                indices.AddRange(Enumerable.Range(s * NeuropixelsV2.ElectrodesPerShank + intraShankStart, contactsPerShank));
            return indices;
        }

        static int GetQuadShankChannelNumber(int shank, int block, int blockIndex) => (shank, block) switch
        {
            (0, 0) => blockIndex + ElectrodesPerBlock * 0,
            (0, 1) => blockIndex + ElectrodesPerBlock * 2,
            (0, 2) => blockIndex + ElectrodesPerBlock * 4,
            (0, 3) => blockIndex + ElectrodesPerBlock * 6,
            (0, 4) => blockIndex + ElectrodesPerBlock * 5,
            (0, 5) => blockIndex + ElectrodesPerBlock * 7,
            (0, 6) => blockIndex + ElectrodesPerBlock * 1,
            (0, 7) => blockIndex + ElectrodesPerBlock * 3,
            (1, 0) => blockIndex + ElectrodesPerBlock * 1,
            (1, 1) => blockIndex + ElectrodesPerBlock * 3,
            (1, 2) => blockIndex + ElectrodesPerBlock * 5,
            (1, 3) => blockIndex + ElectrodesPerBlock * 7,
            (1, 4) => blockIndex + ElectrodesPerBlock * 4,
            (1, 5) => blockIndex + ElectrodesPerBlock * 6,
            (1, 6) => blockIndex + ElectrodesPerBlock * 0,
            (1, 7) => blockIndex + ElectrodesPerBlock * 2,
            (2, 0) => blockIndex + ElectrodesPerBlock * 4,
            (2, 1) => blockIndex + ElectrodesPerBlock * 6,
            (2, 2) => blockIndex + ElectrodesPerBlock * 0,
            (2, 3) => blockIndex + ElectrodesPerBlock * 2,
            (2, 4) => blockIndex + ElectrodesPerBlock * 1,
            (2, 5) => blockIndex + ElectrodesPerBlock * 3,
            (2, 6) => blockIndex + ElectrodesPerBlock * 5,
            (2, 7) => blockIndex + ElectrodesPerBlock * 7,
            (3, 0) => blockIndex + ElectrodesPerBlock * 5,
            (3, 1) => blockIndex + ElectrodesPerBlock * 7,
            (3, 2) => blockIndex + ElectrodesPerBlock * 1,
            (3, 3) => blockIndex + ElectrodesPerBlock * 3,
            (3, 4) => blockIndex + ElectrodesPerBlock * 0,
            (3, 5) => blockIndex + ElectrodesPerBlock * 2,
            (3, 6) => blockIndex + ElectrodesPerBlock * 4,
            (3, 7) => blockIndex + ElectrodesPerBlock * 6,
            _ => throw new ArgumentOutOfRangeException($"Invalid shank ({shank}) and/or block ({block}) value.")
        };
    }

    /// <summary>
    /// Specifies a predefined electrode selection pattern for a Neuropixels 2.0 quad-shank probe.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Single-shank presets (e.g. <see cref="Shank0BankA"/>) connect all 384 channels to a single bank
    /// on a single shank.
    /// </para>
    /// <para>
    /// All-shanks presets (e.g. <see cref="AllShanks0_95"/>) distribute the 384 acquisition channels
    /// evenly across all four shanks, with 96 contacts per shank, covering a consecutive range of
    /// intra-shank electrode indices indicated by the preset name.
    /// </para>
    /// </remarks>
    public enum NeuropixelsV2QuadShankChannelPreset
    {
        /// <summary>
        /// Selects all 384 channels on shank 0, bank A (intra-shank indices 0–383).
        /// </summary>
        Shank0BankA,
        /// <summary>
        /// Selects all 384 channels on shank 0, bank B (intra-shank indices 384–767).
        /// </summary>
        Shank0BankB,
        /// <summary>
        /// Selects all 384 channels on shank 0, bank C (intra-shank indices 768–1151).
        /// </summary>
        Shank0BankC,
        /// <summary>
        /// Selects the 384 deepest electrodes on shank 0 (intra-shank indices 896–1279), spanning the upper
        /// portion of bank C and all of bank D.
        /// </summary>
        Shank0BankD,
        /// <summary>
        /// Selects all 384 channels on shank 1, bank A (intra-shank indices 0–383).
        /// </summary>
        Shank1BankA,
        /// <summary>
        /// Selects all 384 channels on shank 1, bank B (intra-shank indices 384–767).
        /// </summary>
        Shank1BankB,
        /// <summary>
        /// Selects all 384 channels on shank 1, bank C (intra-shank indices 768–1151).
        /// </summary>
        Shank1BankC,
        /// <summary>
        /// Selects the 384 deepest electrodes on shank 1 (intra-shank indices 896–1279), spanning the upper
        /// portion of bank C and all of bank D.
        /// </summary>
        Shank1BankD,
        /// <summary>
        /// Selects all 384 channels on shank 2, bank A (intra-shank indices 0–383).
        /// </summary>
        Shank2BankA,
        /// <summary>
        /// Selects all 384 channels on shank 2, bank B (intra-shank indices 384–767).
        /// </summary>
        Shank2BankB,
        /// <summary>
        /// Selects all 384 channels on shank 2, bank C (intra-shank indices 768–1151).
        /// </summary>
        Shank2BankC,
        /// <summary>
        /// Selects the 384 deepest electrodes on shank 2 (intra-shank indices 896–1279), spanning the upper
        /// portion of bank C and all of bank D.
        /// </summary>
        Shank2BankD,
        /// <summary>
        /// Selects all 384 channels on shank 3, bank A (intra-shank indices 0–383).
        /// </summary>
        Shank3BankA,
        /// <summary>
        /// Selects all 384 channels on shank 3, bank B (intra-shank indices 384–767).
        /// </summary>
        Shank3BankB,
        /// <summary>
        /// Selects all 384 channels on shank 3, bank C (intra-shank indices 768–1151).
        /// </summary>
        Shank3BankC,
        /// <summary>
        /// Selects the 384 deepest electrodes on shank 3 (intra-shank indices 896–1279), spanning the upper
        /// portion of bank C and all of bank D.
        /// </summary>
        Shank3BankD,
        /// <summary>
        /// Selects 96 channels on each shank covering intra-shank indices 0–95.
        /// </summary>
        AllShanks0_95,
        /// <summary>
        /// Selects 96 channels on each shank covering intra-shank indices 96–191.
        /// </summary>
        AllShanks96_191,
        /// <summary>
        /// Selects 96 channels on each shank covering intra-shank indices 192–287.
        /// </summary>
        AllShanks192_287,
        /// <summary>
        /// Selects 96 channels on each shank covering intra-shank indices 288–383.
        /// </summary>
        AllShanks288_383,
        /// <summary>
        /// Selects 96 channels on each shank covering intra-shank indices 384–479.
        /// </summary>
        AllShanks384_479,
        /// <summary>
        /// Selects 96 channels on each shank covering intra-shank indices 480–575.
        /// </summary>
        AllShanks480_575,
        /// <summary>
        /// Selects 96 channels on each shank covering intra-shank indices 576–671.
        /// </summary>
        AllShanks576_671,
        /// <summary>
        /// Selects 96 channels on each shank covering intra-shank indices 672–767.
        /// </summary>
        AllShanks672_767,
        /// <summary>
        /// Selects 96 channels on each shank covering intra-shank indices 768–863.
        /// </summary>
        AllShanks768_863,
        /// <summary>
        /// Selects 96 channels on each shank covering intra-shank indices 864–959.
        /// </summary>
        AllShanks864_959,
        /// <summary>
        /// Selects 96 channels on each shank covering intra-shank indices 960–1055.
        /// </summary>
        AllShanks960_1055,
        /// <summary>
        /// Selects 96 channels on each shank covering intra-shank indices 1056–1151.
        /// </summary>
        AllShanks1056_1151,
        /// <summary>
        /// Selects 96 channels on each shank covering intra-shank indices 1152–1247.
        /// </summary>
        AllShanks1152_1247,
        /// <summary>
        /// No preset applied; leaves the existing channel map unchanged.
        /// </summary>
        None
    }
}
