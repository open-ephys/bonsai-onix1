using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Defines a ProbeInterface-compatible probe group for Neuropixels 2.0 and 2.0-beta probes.
    /// </summary>
    /// <remarks>
    /// Shank count, banks, and reference options are all derived from the loaded probe interface data
    /// (specifically each contact's <see cref="Contact.ShankId"/>) rather than from a hardcoded,
    /// per-topology subclass. A file with no shank IDs at all (e.g. a single-shank probe) is treated as
    /// having exactly one shank.
    /// </remarks>
    public class NeuropixelsV2ProbeGroup : SingleProbeGroup, IMultiplexedProbeGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2ProbeGroup"/> class using the
        /// default electrode geometry.
        /// </summary>
        public NeuropixelsV2ProbeGroup()
            : this(ProbeGroupResource.LoadDefault<NeuropixelsV2ProbeGroup>("NP2013.json"))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2ProbeGroup"/> class by copying an
        /// existing probe group.
        /// </summary>
        /// <param name="probeGroup">The probe group to copy.</param>
        public NeuropixelsV2ProbeGroup(NeuropixelsV2ProbeGroup probeGroup)
            : base(probeGroup)
        {
            InitializeShankLookup();
            Variant = NeuropixelsV2VariantRegistry.Resolve(Probe.Annotations.ModelName);

            if (base.ChannelMap is null)
                SelectBank(0, 0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2ProbeGroup"/> class from
        /// deserialized ProbeInterface data.
        /// </summary>
        /// <param name="specification">The ProbeInterface specification string.</param>
        /// <param name="version">The ProbeInterface version string.</param>
        /// <param name="probes">The array of probes deserialized from the ProbeInterface file.</param>
        [JsonConstructor]
        public NeuropixelsV2ProbeGroup(string specification, string version, Probe[] probes)
            : base(specification, version, probes)
        {
            InitializeShankLookup();

            if (NumberOfContacts != ShankCount * NeuropixelsV2.ElectrodesPerShank)
            {
                throw new ArgumentException(
                    $"Invalid number of contacts; expected a multiple of {NeuropixelsV2.ElectrodesPerShank} " +
                    $"matching the {ShankCount} shank(s) found in the probe interface data, but found {NumberOfContacts}.");
            }

            Variant = NeuropixelsV2VariantRegistry.Resolve(Probe.Annotations.ModelName);
            if (Variant.ShankCount != ShankCount)
            {
                throw new ArgumentException(
                    $"Probe model '{Probe.Annotations.ModelName}' expects {Variant.ShankCount} shank(s), " +
                    $"but the probe interface data has {ShankCount}.");
            }

            if (base.ChannelMap is null)
                SelectBank(0, 0);
        }

        /// <summary>
        /// Returns the channel map for this probe, mapping each acquisition channel index to its
        /// active contact index.
        /// </summary>
        /// <remarks>
        /// This property hides <see cref="ProbeGroup.ChannelMap"/>, which is nullable, to enforce the
        /// invariant that a Neuropixels 2.0 probe group always has a valid channel map after
        /// construction. If no channel map was present in the deserialized data, bank A of the first
        /// shank is applied automatically during construction.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the channel map is null.</exception>
        [JsonIgnore]
        public new IReadOnlyDictionary<int, int> ChannelMap =>
            base.ChannelMap ?? throw new InvalidOperationException("Neuropixels probes must have a valid channel map.");

        /// <summary>
        /// Gets the number of shanks on this probe, derived from the distinct <see cref="Contact.ShankId"/>
        /// values present in the loaded probe interface data (or 1 if none are present).
        /// </summary>
        [JsonIgnore]
        public int ShankCount { get; private set; }

        /// <summary>
        /// The resolved ASIC channel-wiring variant for this probe, keyed by the probe interface
        /// file's model name annotation.
        /// </summary>
        [JsonIgnore]
        internal NeuropixelsV2Variant Variant { get; private set; }

        int[] shankByContact;
        int[] intraShankIndexByContact;
        int[][] contactIndexByShankAndIntraIndex;

        void InitializeShankLookup()
        {
            var contacts = Probe.Contacts;
            int n = contacts.Count;
            var shankIds = new string[n];
            for (int i = 0; i < n; i++)
                shankIds[i] = contacts[i].ShankId;

            List<string> distinctShankIds;
            if (shankIds.All(id => id is null))
            {
                distinctShankIds = new List<string> { "0" };
            }
            else
            {
                var seen = shankIds.Where(id => id != null).Distinct().ToList();
                // NB: Prefer numeric order when every id parses as an integer (true for every known IMEC
                // file); fall back to first-appearance order for third-party files with non-numeric
                // shank ids.
                distinctShankIds = seen.All(id => int.TryParse(id, out _))
                    ? seen.OrderBy(int.Parse).ToList()
                    : seen;
            }

            var shankIndexById = new Dictionary<string, int>();
            for (int i = 0; i < distinctShankIds.Count; i++)
                shankIndexById[distinctShankIds[i]] = i;

            ShankCount = distinctShankIds.Count;

            shankByContact = new int[n];
            intraShankIndexByContact = new int[n];
            var runningCountPerShank = new int[ShankCount];
            for (int i = 0; i < n; i++)
            {
                int shank = shankIndexById[shankIds[i] ?? "0"];
                shankByContact[i] = shank;
                intraShankIndexByContact[i] = runningCountPerShank[shank]++;
            }

            contactIndexByShankAndIntraIndex = new int[ShankCount][];
            for (int s = 0; s < ShankCount; s++)
                contactIndexByShankAndIntraIndex[s] = new int[runningCountPerShank[s]];
            for (int i = 0; i < n; i++)
                contactIndexByShankAndIntraIndex[shankByContact[i]][intraShankIndexByContact[i]] = i;
        }

        /// <summary>
        /// Returns the zero-based shank index that contains the given contact index.
        /// </summary>
        /// <param name="contactIndex">The zero-based contact index across all electrodes on the
        /// probe.</param>
        /// <returns>The zero-based shank index.</returns>
        public int GetShank(int contactIndex) => shankByContact[contactIndex];

        /// <summary>
        /// Returns the electrode index within its shank for the given contact index.
        /// </summary>
        /// <param name="contactIndex">The zero-based contact index across all electrodes on the probe.</param>
        /// <returns>The zero-based electrode index within the shank.</returns>
        public int GetIntraShankElectrodeIndex(int contactIndex) => intraShankIndexByContact[contactIndex];

        int GetContactIndex(int shank, int intraShankElectrodeIndex) => contactIndexByShankAndIntraIndex[shank][intraShankElectrodeIndex];

        /// <summary>
        /// Returns the zero-based index of the bank that contains the given contact index.
        /// </summary>
        /// <param name="contactIndex">The zero-based contact index across all electrodes on the probe.</param>
        /// <returns>The zero-based bank index.</returns>
        public int GetBank(int contactIndex)
            => GetIntraShankElectrodeIndex(contactIndex) / NeuropixelsV2.ChannelCount;

        /// <summary>
        /// The number of banks on each shank of this probe.
        /// </summary>
        public int BankCount { get; } = (NeuropixelsV2.ElectrodesPerShank + NeuropixelsV2.ChannelCount - 1) / NeuropixelsV2.ChannelCount;

        /// <summary>
        /// Returns the acquisition channel number for the given contact index.
        /// </summary>
        /// <param name="contactIndex">The zero-based contact index across all electrodes on the probe.</param>
        /// <returns>The acquisition channel number (0 to <see cref="NeuropixelsV2.ChannelCount"/> - 1).</returns>
        public int GetChannel(int contactIndex)
        {
            int shank = GetShank(contactIndex);
            int intra = GetIntraShankElectrodeIndex(contactIndex);

            return Variant.GetChannel(shank, intra);
        }

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
        /// Returns every channel preset available for this probe, generated from its shank count.
        /// </summary>
        /// <returns>The list of channel presets, starting with <see cref="NeuropixelsV2ChannelPreset.None"/>.</returns>
        public IReadOnlyList<NeuropixelsV2ChannelPreset> GetChannelPresets()
        {
            var presets = new List<NeuropixelsV2ChannelPreset> { NeuropixelsV2ChannelPreset.None };

            var bankStarts = Enumerable.Range(0, BankCount)
                .Select(bankIndex => (
                    Name: $"Bank{Neuropixels.BankDisplayName(bankIndex)}",
                    Start: Neuropixels.BankWindowStart(bankIndex, NeuropixelsV2.ElectrodesPerShank, NeuropixelsV2.ChannelCount)))
                .ToArray();

            for (int s = 0; s < ShankCount; s++)
            {
                foreach (var (name, start) in bankStarts)
                {
                    var label = ShankCount == 1 ? name : $"Shank{s}{name}";
                    presets.Add(new NeuropixelsV2ChannelPreset(s, start, label));
                }
            }

            if (ShankCount > 1)
            {
                int windowWidth = NeuropixelsV2.ChannelCount / ShankCount;
                for (int start = 0; start + windowWidth <= NeuropixelsV2.ElectrodesPerShank; start += windowWidth)
                    presets.Add(new NeuropixelsV2ChannelPreset(null, start, $"AllShanks{start}_{start + windowWidth - 1}"));
            }

            return presets;
        }

        /// <summary>
        /// Configures the channel map to the specified preset.
        /// </summary>
        /// <param name="preset">The preset to apply. Passing <see cref="NeuropixelsV2ChannelPreset.None"/>
        /// leaves the existing channel map unchanged.</param>
        public void SelectPreset(NeuropixelsV2ChannelPreset preset)
        {
            if (preset == NeuropixelsV2ChannelPreset.None)
                return;

            if (preset.Shank is int shank)
            {
                EnableElectrodes(Enumerable.Range(preset.WindowStart, NeuropixelsV2.ChannelCount)
                    .Select(intra => GetContactIndex(shank, intra)));
            }
            else
            {
                int windowWidth = NeuropixelsV2.ChannelCount / ShankCount;
                var indices = new List<int>(NeuropixelsV2.ChannelCount);
                for (int s = 0; s < ShankCount; s++)
                    indices.AddRange(Enumerable.Range(preset.WindowStart, windowWidth).Select(intra => GetContactIndex(s, intra)));
                EnableElectrodes(indices);
            }
        }

        /// <summary>
        /// Configures the channel map for a specific shank and bank.
        /// </summary>
        /// <param name="shank">Zero-based shank index.</param>
        /// <param name="bankIndex">Zero-based index of the bank of electrodes to activate on the specified shank.</param>
        public void SelectBank(int shank, int bankIndex)
        {
            if (shank < 0 || shank >= ShankCount)
                throw new ArgumentOutOfRangeException(nameof(shank), $"Probe has {ShankCount} shank(s); got {shank}.");
            if (bankIndex < 0 || bankIndex >= BankCount)
                throw new ArgumentOutOfRangeException(nameof(bankIndex), $"Probe has {BankCount} bank(s); got {bankIndex}.");

            int start = Neuropixels.BankWindowStart(bankIndex, NeuropixelsV2.ElectrodesPerShank, NeuropixelsV2.ChannelCount);

            EnableElectrodes(Enumerable.Range(start, NeuropixelsV2.ChannelCount).Select(intra => GetContactIndex(shank, intra)));
        }

        // NB: Channel-number mapping: IMEC's ASIC mux-wiring formula. This is a hardware fact, not geometry,
        // so it cannot be derived from the probe interface file. only 1-shank and 4-shank layouts have a
        // known/verified formula (see GetChannel).

        const int SingleShankElectrodesPerBlock = 32;
        const int SingleShankElectrodesPerRow = 2;

        internal static int GetSingleShankChannelNumber(int intraShankElectrodeIndex)
        {
            var bank = intraShankElectrodeIndex / NeuropixelsV2.ChannelCount;
            int bankIndex = intraShankElectrodeIndex % NeuropixelsV2.ChannelCount;
            int block = bankIndex / SingleShankElectrodesPerBlock;
            int row = bankIndex % SingleShankElectrodesPerBlock / SingleShankElectrodesPerRow;
            int columnIndex = intraShankElectrodeIndex % 2;

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
                (0, 0) => row * SingleShankElectrodesPerRow + block * SingleShankElectrodesPerBlock,
                (0, 1) => row * SingleShankElectrodesPerRow + block * SingleShankElectrodesPerBlock + 1,
                (1, 0) => (row * 7 % HalfBlock) * SingleShankElectrodesPerRow + block * SingleShankElectrodesPerBlock,
                (1, 1) => ((row * 7 + 4) % HalfBlock) * SingleShankElectrodesPerRow + block * SingleShankElectrodesPerBlock + 1,
                (2, 0) => (row * 5 % HalfBlock) * SingleShankElectrodesPerRow + block * SingleShankElectrodesPerBlock,
                (2, 1) => ((row * 5 + 8) % HalfBlock) * SingleShankElectrodesPerRow + block * SingleShankElectrodesPerBlock + 1,
                (3, 0) => (row * 3 % HalfBlock) * SingleShankElectrodesPerRow + block * SingleShankElectrodesPerBlock,
                (3, 1) => ((row * 3 + 12) % HalfBlock) * SingleShankElectrodesPerRow + block * SingleShankElectrodesPerBlock + 1,
                _ => throw new NotImplementedException($"Invalid bank value: {bank}.")
            };
        }

        const int QuadShankElectrodesPerBlock = 48;

        internal static int GetQuadShankChannelNumber(int shank, int intraShankElectrodeIndex)
        {
            int block = (intraShankElectrodeIndex % NeuropixelsV2.ChannelCount) / QuadShankElectrodesPerBlock;
            int blockIndex = intraShankElectrodeIndex % QuadShankElectrodesPerBlock;

            return (shank, block) switch
            {
                (0, 0) => blockIndex + QuadShankElectrodesPerBlock * 0,
                (0, 1) => blockIndex + QuadShankElectrodesPerBlock * 2,
                (0, 2) => blockIndex + QuadShankElectrodesPerBlock * 4,
                (0, 3) => blockIndex + QuadShankElectrodesPerBlock * 6,
                (0, 4) => blockIndex + QuadShankElectrodesPerBlock * 5,
                (0, 5) => blockIndex + QuadShankElectrodesPerBlock * 7,
                (0, 6) => blockIndex + QuadShankElectrodesPerBlock * 1,
                (0, 7) => blockIndex + QuadShankElectrodesPerBlock * 3,
                (1, 0) => blockIndex + QuadShankElectrodesPerBlock * 1,
                (1, 1) => blockIndex + QuadShankElectrodesPerBlock * 3,
                (1, 2) => blockIndex + QuadShankElectrodesPerBlock * 5,
                (1, 3) => blockIndex + QuadShankElectrodesPerBlock * 7,
                (1, 4) => blockIndex + QuadShankElectrodesPerBlock * 4,
                (1, 5) => blockIndex + QuadShankElectrodesPerBlock * 6,
                (1, 6) => blockIndex + QuadShankElectrodesPerBlock * 0,
                (1, 7) => blockIndex + QuadShankElectrodesPerBlock * 2,
                (2, 0) => blockIndex + QuadShankElectrodesPerBlock * 4,
                (2, 1) => blockIndex + QuadShankElectrodesPerBlock * 6,
                (2, 2) => blockIndex + QuadShankElectrodesPerBlock * 0,
                (2, 3) => blockIndex + QuadShankElectrodesPerBlock * 2,
                (2, 4) => blockIndex + QuadShankElectrodesPerBlock * 1,
                (2, 5) => blockIndex + QuadShankElectrodesPerBlock * 3,
                (2, 6) => blockIndex + QuadShankElectrodesPerBlock * 5,
                (2, 7) => blockIndex + QuadShankElectrodesPerBlock * 7,
                (3, 0) => blockIndex + QuadShankElectrodesPerBlock * 5,
                (3, 1) => blockIndex + QuadShankElectrodesPerBlock * 7,
                (3, 2) => blockIndex + QuadShankElectrodesPerBlock * 1,
                (3, 3) => blockIndex + QuadShankElectrodesPerBlock * 3,
                (3, 4) => blockIndex + QuadShankElectrodesPerBlock * 0,
                (3, 5) => blockIndex + QuadShankElectrodesPerBlock * 2,
                (3, 6) => blockIndex + QuadShankElectrodesPerBlock * 4,
                (3, 7) => blockIndex + QuadShankElectrodesPerBlock * 6,
                _ => throw new ArgumentOutOfRangeException($"Invalid shank ({shank}) and/or block ({block}) value.")
            };
        }
    }

    /// <summary>
    /// Specifies a predefined electrode selection pattern for a Neuropixels 2.0 probe.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Presets are generated from the shank count of the loaded probe rather than hand-written per
    /// topology. A single-shank preset (<see cref="Shank"/> is non-null) selects all 384 channels in one
    /// bank on one shank. An all-shanks preset (<see cref="Shank"/> is null, only possible when there is
    /// more than one shank) distributes the 384 acquisition channels evenly across every shank, covering
    /// the same consecutive range of intra-shank electrode indices on each.
    /// </para>
    /// </remarks>
    public readonly record struct NeuropixelsV2ChannelPreset(int? Shank, int WindowStart, string DisplayName)
    {
        /// <summary>
        /// The "no preset" value; applying it leaves the existing channel map unchanged.
        /// </summary>
        public static readonly NeuropixelsV2ChannelPreset None = new(null, -1, "None");

        /// <inheritdoc/>
        public override string ToString() => DisplayName;
    }
}
