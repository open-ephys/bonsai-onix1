using System;
using System.Collections.Generic;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Describes a Neuropixels 1.0 probe variant's electrode count and ASIC channel-wiring formula.
    /// </summary>
    /// <remarks>
    /// The channel-wiring formula is a hardware fact tied to the probe's part number, not something derivable
    /// from geometry alone. <see cref="ElectrodeCount"/> is checked against the contact count of the loaded
    /// probe interface data at construction time.
    /// </remarks>
    internal sealed class NeuropixelsV1Variant
    {
        /// <summary>
        /// The total number of electrodes on this variant.
        /// </summary>
        public int ElectrodeCount { get; }

        /// <summary>
        /// The number of 384-contact banks on this variant, computed as <c>ceil(ElectrodeCount /
        /// ChannelCount)</c>. The last bank's window is right-aligned against the end of the probe's
        /// electrodes (see <see cref="NeuropixelsV1ChannelToContactProbeGroup.SelectBank"/>) rather than
        /// being a full 384 contacts, exactly like the existing standard-NP1.0 <c>BankC</c> preset already
        /// does.
        /// </summary>
        public int BankCount { get; }

        /// <summary>
        /// Maps a contact index to its acquisition channel number.
        /// </summary>
        public Func<int, int> GetChannel { get; }

        /// <summary>
        /// True if this variant has a dedicated internal reference electrode once per bank (contact index
        /// bank*ChannelCount + 191, i.e. 191/575/959 for a standard 3-bank probe) that can never be wired to
        /// a recording channel. False for variants where every contact, including 191, is an ordinary
        /// recordable electrode.
        /// </summary>
        /// <remarks>
        /// True for standard NP1.0, NHP Active, and Opto. False for UHD Fixed and UHD Switchable, where every
        /// contact, including 191, is an ordinary recordable electrode. NB: getting this wrong in either
        /// direction is a real bug: true when it should be false silently blocks a legitimate electrode;
        /// false when it should be true was the original bug fixed in <see
        /// cref="NeuropixelsV1.IsInternalReferenceContact"/>.
        /// </remarks>
        public bool HasInternalReferenceElectrode { get; }

        /// <summary>
        /// True if this variant's site layout supports the <c>SingleColumn</c>/<c>Tetrodes</c> channel
        /// presets (see <see cref="NeuropixelsV1ChannelToContactProbeGroup.GetChannelPresets"/>).
        /// </summary>
        /// <remarks>
        /// A site-layout fact, independent of <see cref="BankCount"/>: a variant can have exactly 3 banks
        /// (matching the standard probe's bank count) without sharing its physical site pattern, so this is
        /// set explicitly per variant rather than derived from any other field.
        /// </remarks>
        public bool SupportsColumnAndTetrodePresets { get; }

        /// <summary>
        /// The electrode count used to size and lay out the shank configuration shift register (see <see
        /// cref="NeuropixelsV1.MakeShankRegisterLayout"/>). Defaults to <see cref="ElectrodeCount"/>.
        /// </summary>
        /// <remarks>
        /// Distinct from <see cref="ElectrodeCount"/> for UHD Switchable specifically: the shift register has
        /// one bit per acquisition channel (384), not one bit per physical electrode (6144). NB: which of a
        /// channel's 16 candidate electrodes is actually connected is decided by a separate group/bank
        /// multiplexer, not represented in this register at all.
        /// </remarks>
        public int ShankRegisterElectrodeCount { get; }

        /// <summary>
        /// Maps a contact index to the position used to place its bit within the shank configuration shift
        /// register (see <see cref="NeuropixelsV1.MakeShankRegisterLayout"/>). Defaults to the identity
        /// function.
        /// </summary>
        /// <remarks>
        /// For every family except UHD Switchable, a contact's raw index already equals <c>bank *
        /// ChannelCount + channel</c> by construction, so the identity default is correct. UHD Switchable's
        /// contact index has no such direct relationship to its channel (that's the whole
        /// 16-electrodes-per-channel multiplexing <see cref="GetChannel"/> encodes), and the register only
        /// has a bit per channel, so this is set to <see cref="GetChannel"/> for that variant specifically.
        /// </remarks>
        public Func<int, int> ShankRegisterPosition { get; }

        /// <summary>
        /// True if this variant has a column selection switch (<c>EN_A</c>/<c>EN_B</c>) with two dedicated
        /// bits at the end of the shank configuration shift register, which must be set high for the default
        /// all-columns-enabled configuration this codebase always requests (column selection itself, i.e.
        /// <c>selectColumnPattern</c>, is not exposed as a feature).
        /// </summary>
        public bool HasColumnSelectionSwitch { get; }

        /// <summary>
        /// True if individual contacts cannot be independently wired to channels on this variant, and
        /// electrode selection must instead go through per-channel-group bank selection (see <see
        /// cref="NeuropixelsV1ChannelGroupProbeGroup.SelectElectrodeGroup"/>).
        /// </summary>
        /// <remarks>
        /// True only for UHD Switchable (NP1110). Determines which concrete <see
        /// cref="NeuropixelsV1ProbeGroup"/> subtype a probe deserializes to (<see
        /// cref="NeuropixelsV1ProbeGroupConverter"/>): variants with this set become <see
        /// cref="NeuropixelsV1ChannelGroupProbeGroup"/>, which doesn't expose the ordinary per-contact API
        /// (<c>EnableElectrodes</c>, <c>SelectBank</c>, <c>SelectPreset</c>, <c>GetChannelPresets</c>) at
        /// all. NB: the atom of selection is a 16-contact channel-group tile, not a single contact, so those
        /// operations are a compile-time error for this variant rather than a runtime one.
        /// </remarks>
        public bool HasChannelGroupSelection { get; }

        public NeuropixelsV1Variant(
            int electrodeCount,
            Func<int, int> getChannel,
            bool hasInternalReferenceElectrode = true,
            bool supportsColumnAndTetrodePresets = false,
            int? shankRegisterElectrodeCount = null,
            Func<int, int> shankRegisterPosition = null,
            bool hasColumnSelectionSwitch = false,
            bool hasChannelGroupSelection = false)
        {
            ElectrodeCount = electrodeCount;
            BankCount = (electrodeCount + NeuropixelsV1.ChannelCount - 1) / NeuropixelsV1.ChannelCount;
            GetChannel = getChannel;
            HasInternalReferenceElectrode = hasInternalReferenceElectrode;
            SupportsColumnAndTetrodePresets = supportsColumnAndTetrodePresets;
            ShankRegisterElectrodeCount = shankRegisterElectrodeCount ?? electrodeCount;
            ShankRegisterPosition = shankRegisterPosition ?? (contactIndex => contactIndex);
            HasColumnSelectionSwitch = hasColumnSelectionSwitch;
            HasChannelGroupSelection = hasChannelGroupSelection;
        }

        /// <summary>
        /// Returns true if <paramref name="contactIndex"/> is this variant's internal reference electrode and
        /// must never be wired to a recording channel.
        /// </summary>
        /// <remarks>
        /// Gates <see cref="NeuropixelsV1.IsInternalReferenceContact"/>'s unconditional position check by
        /// <see cref="HasInternalReferenceElectrode"/>, since that position check alone is only meaningful
        /// for variants that actually have a reference electrode there.
        /// </remarks>
        public bool IsInternalReferenceContact(int contactIndex) =>
            HasInternalReferenceElectrode && NeuropixelsV1.IsInternalReferenceContact(contactIndex);
    }

    /// <summary>
    /// Resolves a Neuropixels 1.0 probe part number (or probe interface model name annotation) to its <see
    /// cref="NeuropixelsV1Variant"/>.
    /// </summary>
    internal static class NeuropixelsV1VariantRegistry
    {
        // Shared by every family that reuses the standard 32-ADC/12-channel readout ASIC's trivial modulo
        // channel wiring: Standard, NHP Active, and Opto.
        static int ModuloChannel(int contactIndex) => contactIndex % NeuropixelsV1.ChannelCount;

        // UHD Switchable (NP1110): 6144 electrodes, multiplexed 16-to-1 onto the standard 384 channels via a
        // fixed lookup table (Resources/NP1110ChannelOrder.bin). Mechanically verified: every value in the
        // table is in [0, 384), each of the 384 channels appears in it exactly 16 times, and every contiguous
        // 384-contact block (the same window SelectBank already uses generically) covers all 384 channels
        // exactly once with no collisions.
        //
        // The channel for contact i is channel_order[rowStart(i) + column_order[i % 8]], where rowStart(i) =
        // (i / 8) * 8. NB: electrodes are physically laid out 8 columns wide, and column_order reorders which
        // of the 8 physical columns' table entry backs each logical column position. As with UHD Fixed, no
        // electrode is an internal reference on this variant, so hasInternalReferenceElectrode is false here
        // too.
        const int Np1110NumColumns = 8;
        static readonly int[] Np1110ColumnOrder = { 0, 7, 1, 6, 2, 5, 3, 4 };
        static readonly Lazy<ushort[]> Np1110ChannelOrder =
            new(() => ProbeGroupResource.LoadUInt16Array("NP1110ChannelOrder.bin"));

        static int UhdSwitchableChannel(int contactIndex)
        {
            int rowStart = contactIndex / Np1110NumColumns * Np1110NumColumns;
            int physicalColumn = Np1110ColumnOrder[contactIndex % Np1110NumColumns];
            return Np1110ChannelOrder.Value[rowStart + physicalColumn];
        }

        /// <summary>
        /// The number of channel groups on the UHD Switchable probe (NP1110). See <see
        /// cref="NeuropixelsV1ChannelGroupProbeGroup.SelectElectrodeGroup"/>.
        /// </summary>
        internal const int Np1110ChannelGroupCount = 24;

        /// <summary>
        /// The number of candidate banks per channel group on the UHD Switchable probe (NP1110) -- the row
        /// width of <see cref="Np1110GroupBankRegisterIndex"/>, indexed <c>channelGroup *
        /// Np1110BanksPerChannelGroup + bank</c>.
        /// </summary>
        internal const int Np1110BanksPerChannelGroup = 16;

        /// <summary>
        /// Returns the fixed set of 16 channels belonging to <paramref name="channelGroup"/> (0-23) on the
        /// UHD Switchable probe (NP1110). This set is the same regardless of which bank is selected for the
        /// group.
        /// </summary>
        internal static IEnumerable<int> Np1110ChannelGroupChannels(int channelGroup)
        {
            for (int k = 0; k < 16; k++)
                yield return 32 * (channelGroup / 2) + channelGroup % 2 + 2 * k;
        }

        /// <summary>
        /// Returns the channel group (0-23) that <paramref name="channel"/> belongs to on the UHD Switchable
        /// probe (NP1110). The inverse of <see cref="Np1110ChannelGroupChannels"/>.
        /// </summary>
        internal static int Np1110ChannelGroupOf(int channel) => 2 * (channel / 32) + channel % 2;

        /// <summary>
        /// The physical columns (0-7, the across-shank axis) enabled under each <see
        /// cref="NeuropixelsV1ColumnPattern"/> on the UHD Switchable probe (NP1110). <see
        /// cref="NeuropixelsV1ColumnPattern.All"/> has no entry: every column is enabled, so consumers should
        /// treat an absent key as unrestricted rather than treating this as exhaustive.
        /// </summary>
        internal static readonly IReadOnlyDictionary<NeuropixelsV1ColumnPattern, IReadOnlyList<int>> Np1110ActiveColumnsByPattern =
            new Dictionary<NeuropixelsV1ColumnPattern, IReadOnlyList<int>>
            {
                [NeuropixelsV1ColumnPattern.Outer] = new[] { 0, 2, 5, 7 },
                [NeuropixelsV1ColumnPattern.Inner] = new[] { 1, 3, 4, 6 },
            };

        /// <summary>
        /// Maps a UHD Switchable (NP1110) <c>(channelGroup, bank)</c> pair (channel group 0-23, bank 0-15,
        /// row-major: index <c>channelGroup * 16 + bank</c>) to the value whose <see
        /// cref="NeuropixelsV1ShankRegisterLayout.GetElectrodeBitIndex"/> gives that pair's shank
        /// configuration shift-register bit.
        /// </summary>
        /// <remarks>
        /// Confirmed exhaustively: every one of these 384 values, run through <see
        /// cref="NeuropixelsV1ShankRegisterLayout.GetElectrodeBitIndex"/>, lands on a distinct bit position,
        /// forming a full bijection onto the register's 384 non-reference bits.
        /// </remarks>
        internal static readonly ushort[] Np1110GroupBankRegisterIndex =
        {
            0, 26, 48, 74, 96, 122, 144, 170, 192, 218, 240, 266, 288, 314, 336, 362,
            3, 25, 51, 73, 99, 121, 147, 169, 195, 217, 243, 265, 291, 313, 339, 361,
            2, 24, 50, 72, 98, 120, 146, 168, 194, 216, 242, 264, 290, 312, 338, 360,
            1, 27, 49, 75, 97, 123, 145, 171, 193, 219, 241, 267, 289, 315, 337, 363,
            4, 30, 52, 78, 100, 126, 148, 174, 196, 222, 244, 270, 292, 318, 340, 366,
            7, 29, 55, 77, 103, 125, 151, 173, 199, 221, 247, 269, 295, 317, 343, 365,
            6, 28, 54, 76, 102, 124, 150, 172, 198, 220, 246, 268, 294, 316, 342, 364,
            5, 31, 53, 79, 101, 127, 149, 175, 197, 223, 245, 271, 293, 319, 341, 367,
            8, 34, 56, 82, 104, 130, 152, 178, 200, 226, 248, 274, 296, 322, 344, 370,
            11, 33, 59, 81, 107, 129, 155, 177, 203, 225, 251, 273, 299, 321, 347, 369,
            10, 32, 58, 80, 106, 128, 154, 176, 202, 224, 250, 272, 298, 320, 346, 368,
            9, 35, 57, 83, 105, 131, 153, 179, 201, 227, 249, 275, 297, 323, 345, 371,
            12, 38, 60, 86, 108, 134, 156, 182, 204, 230, 252, 278, 300, 326, 348, 374,
            15, 37, 63, 85, 111, 133, 159, 181, 207, 229, 255, 277, 303, 325, 351, 373,
            14, 36, 62, 84, 110, 132, 158, 180, 206, 228, 254, 276, 302, 324, 350, 372,
            13, 39, 61, 87, 109, 135, 157, 183, 205, 231, 253, 279, 301, 327, 349, 375,
            16, 42, 64, 90, 112, 138, 160, 186, 208, 234, 256, 282, 304, 330, 352, 378,
            19, 41, 67, 89, 115, 137, 163, 185, 211, 233, 259, 281, 307, 329, 355, 377,
            18, 40, 66, 88, 114, 136, 162, 184, 210, 232, 258, 280, 306, 328, 354, 376,
            17, 43, 65, 91, 113, 139, 161, 187, 209, 235, 257, 283, 305, 331, 353, 379,
            20, 46, 68, 94, 116, 142, 164, 190, 212, 238, 260, 286, 308, 334, 356, 382,
            23, 45, 71, 93, 119, 141, 167, 189, 215, 237, 263, 285, 311, 333, 359, 381,
            22, 44, 70, 92, 118, 140, 166, 188, 214, 236, 262, 284, 310, 332, 358, 380,
            21, 47, 69, 95, 117, 143, 165, 191, 213, 239, 261, 287, 309, 335, 357, 383,
        };

        // Standard NP1.0's EEPROM reports old-style strings, while the bundled probe interface file's
        // model_name annotation is the modern "NP1000". Every other family reports the modern NPxxxx string
        // on both sides and needs no such bridging.
        static readonly IReadOnlyList<(string[] Aliases, NeuropixelsV1Variant Descriptor)> Entries =
            new (string[] Aliases, NeuropixelsV1Variant Descriptor)[]
        {
            // NP1001 ("Neuropixels 1.0 probe with cap") is identical to NP1000 in every field that matters
            // here.
            (new[] { "NP1000", "NP1001", "PRB_1_4_0480_1", "PRB_1_4_0480_1_C", "PRB_1_2_0480_2" },
                new NeuropixelsV1Variant(NeuropixelsV1.ElectrodeCount, ModuloChannel, supportsColumnAndTetrodePresets: true)),

            // Opto (NP1300, plus NP1400 "Opto-II pre-Alpha") is intentionally NOT registered yet. The
            // electrical channel mapping is confirmed identical to standard NP1.0's (960-electrode, mod-384
            // channel formula and per-bank internal reference pattern), documented here for whenever this is
            // picked up:
            //
            //   (new[] { "NP1300", "NP1400" }, new NeuropixelsV1Variant(960, ModuloChannel,
            //       supportsColumnAndTetrodePresets: true)),
            //
            // (Opto's geometry is confirmed identical to standard NP1.0's, so supportsColumnAndTetrodePresets
            // should be true once this is enabled.)
            //
            // NB: what's missing is hardware, not data. Opto requires a separate optical-signal muxing path,
            // unrelated to the electrical channel mapping above, that this codebase has not implemented.
            // Registering the part number here would let a user select "NP1300"/"NP1400" as if the probe were
            // fully supported, when only the electrical half is. Leave commented out until the optical muxing
            // hardware path exists.

            // NHP Active: same mod-384 channel formula as standard NP1.0, just longer shanks with more banks.
            // The different part numbers within each electrode-count group are staggered/linear/"Sapiens"
            // site-layout variants; geometry differs between them (carried by the loaded probe interface
            // file, not this registry), but the electrode count and channel formula are identical.
            //
            // NB: the last bank in the 4416-electrode group is a half-populated window that reuses contacts
            // from the previous bank, the same mechanism as standard NP1.0's BankC, which is exactly what
            // Neuropixels.BankWindowStart already implements generically.
            //
            // Register-width/bit-position scaling (see NeuropixelsV1.MakeShankRegisterLayout) is confirmed
            // for both the 2496- and 4416-electrode groups.
            (new[] { "NP1010", "NP1011", "NP1012", "NP1013", "NP1014", "NP1015", "NP1016", "NP1017" },
                new NeuropixelsV1Variant(960, ModuloChannel)),
            (new[] { "NP1020", "NP1021", "NP1022" },
                new NeuropixelsV1Variant(2496, ModuloChannel)),

            // NP1040/NP1041/NP1042/NP1050/NP1051: confirmed identical to NP1030-NP1033. Same ASIC and
            // 4416-electrode count, differing only in mechanical properties (electrode geometry/layout, shank
            // thickness, tube/cap packaging), not electrically. The channel formula and register-layout
            // scaling (see MakeShankRegisterLayout) are both confirmed for this electrode count/ASIC.
            (new[] { "NP1030", "NP1031", "NP1032", "NP1033", "NP1040", "NP1041", "NP1042", "NP1050", "NP1051" },
                new NeuropixelsV1Variant(4416, ModuloChannel)),

            // UHD Fixed: 384 electrodes, 1:1 identity channel wiring (no muxing, single bank). Unlike every
            // family above, no electrode (including 191) is an internal reference: every contact is an
            // ordinary recordable electrode, hence hasInternalReferenceElectrode: false. The five part
            // numbers differ only in column count/site spacing (pure geometry, carried by the loaded probe
            // interface file, not this registry); the wiring fact is identical.
            (new[] { "NP1100", "NP1120", "NP1121", "NP1122", "NP1123" },
                new NeuropixelsV1Variant(NeuropixelsV1.ChannelCount, contactIndex => contactIndex,
                    hasInternalReferenceElectrode: false)),

            // The shank configuration shift register (392 bits = 384 + 8) is sized the same as every other
            // family, but its 384 non-reference bits are NOT one per acquisition channel here, despite the
            // coincidental numeric match: they're one per (channel group, bank) tile (24 groups x 16 banks).
            // See Np1110GroupBankRegisterIndex's remarks. shankRegisterPosition (UhdSwitchableChannel) and
            // ShankRegisterElectrodeCount are effectively vestigial for this variant now. NB:
            // NeuropixelsV1.MakeShankBits branches on HasChannelGroupSelection before reaching the
            // ChannelMap/ShankRegisterPosition-driven path at all, using Np1110GroupBankRegisterIndex
            // directly instead. ShankRegisterElectrodeCount is still used, just for register sizing
            // (RegisterBits/H/Ext/Tip positions via MakeShankRegisterLayout), not per-contact bit placement.
            //
            // hasColumnSelectionSwitch: true. Column selection (Inner/Outer/All) is exposed as a feature, via
            // NeuropixelsV1ChannelGroupProbeGroup.ColumnPattern, see NeuropixelsV1.MakeShankBits.
            //
            // hasChannelGroupSelection: true. Individual contacts cannot be independently wired to channels
            // on this probe; the atom of selection is a channel group (see
            // Np1110ChannelGroupChannels/Np1110GroupBankRegisterIndex above). Also determines which concrete
            // NeuropixelsV1ProbeGroup subtype this variant deserializes to, see
            // NeuropixelsV1ProbeGroupConverter and
            // NeuropixelsV1ChannelGroupProbeGroup.SelectElectrodeGroup.
            (new[] { "NP1110" },
                new NeuropixelsV1Variant(6144, UhdSwitchableChannel, hasInternalReferenceElectrode: false,
                    shankRegisterElectrodeCount: NeuropixelsV1.ChannelCount,
                    shankRegisterPosition: UhdSwitchableChannel,
                    hasColumnSelectionSwitch: true,
                    hasChannelGroupSelection: true)),
        };

        /// <summary>
        /// Resolves <paramref name="partNumber"/> to a <see cref="NeuropixelsV1Variant"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if <paramref name="partNumber"/> is null or
        /// empty.</exception>
        /// <exception cref="NotSupportedException">Thrown if <paramref name="partNumber"/> is not a
        /// recognized Neuropixels 1.0 part number.</exception>
        internal static NeuropixelsV1Variant Resolve(string partNumber) =>
            NeuropixelsPartNumberRegistry.Resolve(partNumber, Entries, "Neuropixels 1.0");
    }
}
