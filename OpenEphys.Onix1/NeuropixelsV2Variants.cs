using System;
using System.Collections.Generic;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Describes a Neuropixels 2.0 probe variant's ASIC channel-wiring formula.
    /// </summary>
    /// <remarks>
    /// The channel-wiring formula is a hardware fact tied to the probe's part number, not something derivable
    /// from geometry alone. <see cref="ShankCount"/> is the shank count this variant's formula is known to be
    /// valid for; it is checked against the shank count derived from the loaded probe interface data at
    /// construction time.
    /// </remarks>
    internal sealed class NeuropixelsV2Variant
    {
        /// <summary>
        /// The number of shanks this variant's channel-wiring formula is valid for.
        /// </summary>
        public int ShankCount { get; }

        /// <summary>
        /// Maps a (shank, intra-shank electrode index) pair to its acquisition channel number.
        /// </summary>
        public Func<int, int, int> GetChannel { get; }

        public NeuropixelsV2Variant(int shankCount, Func<int, int, int> getChannel)
        {
            ShankCount = shankCount;
            GetChannel = getChannel;
        }
    }

    /// <summary>
    /// Resolves a Neuropixels 2.0 probe part number (or probe interface model name annotation) to its <see
    /// cref="NeuropixelsV2Variant"/>.
    /// </summary>
    internal static class NeuropixelsV2VariantRegistry
    {
        static readonly IReadOnlyList<(string[] Aliases, NeuropixelsV2Variant Descriptor)> Entries =
            new (string[] Aliases, NeuropixelsV2Variant Descriptor)[]
        {
            // NP2000/NP2004/PRB2_1_2_0640_0: confirmed alias, not a guess. All of these (plus NP2003) share
            // the same single-shank wiring formula. These are the earlier 14-bit-ADC "Phase 1" ASIC revision
            // (vs. NP2003's 12-bit "Phase 2B"), an electrical/calibration difference only, with zero effect
            // on the wiring formula.
            //
            // NP2005/NP2006: best-guess alias, not independently confirmed. Aliased here on the basis of a
            // single confirmed fact: both are single-shank NP2.0 probes with 1280 electrodes, i.e. the same
            // shank count and electrode count as NP2003, whose wiring formula is otherwise fully verified.
            // Reviewable, intentional decision, not an implicit fallback.
            (new[] { "NP2003", "NP2000", "NP2004", "PRB2_1_2_0640_0", "NP2005", "NP2006" }, new NeuropixelsV2Variant(1,
                (shank, intra) => NeuropixelsV2ProbeGroup.GetSingleShankChannelNumber(intra))),

            // NP2010/PRB2_4_2_0640_0: same confirmed-alias reasoning as NP2000/NP2004 above, for the
            // multi-shank formula. NP2014 is a quad-shank NP2.0 with a metal mounting cap, a mechanical
            // variant of NP2013, not an electrical one.
            (new[] { "NP2013", "NP2010", "NP2014", "PRB2_4_2_0640_0" }, new NeuropixelsV2Variant(4,
                (shank, intra) => NeuropixelsV2ProbeGroup.GetQuadShankChannelNumber(shank, intra))),

            // NP2300 ("Opto-II Alpha"): intentionally NOT registered. Same rationale as NP1300/NP1400 in
            // NeuropixelsV1Variants.cs: requires optical-signal muxing hardware this codebase does not
            // implement. NB: unlike NP1300/Opto, this part number's electrical wiring formula is not
            // independently confirmed either; electrodes_per_shank=1280 and num_shanks=1 match NP2003,
            // suggesting (not confirming) it shares NP2003's single-shank formula. Leave unregistered until
            // both the optical hardware path exists and this formula is independently confirmed.
        };

        /// <summary>
        /// Resolves <paramref name="partNumber"/> to a <see cref="NeuropixelsV2Variant"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if <paramref name="partNumber"/> is null or
        /// empty.</exception>
        /// <exception cref="NotSupportedException">Thrown if <paramref name="partNumber"/> is not a
        /// recognized Neuropixels 2.0 part number.</exception>
        internal static NeuropixelsV2Variant Resolve(string partNumber) =>
            NeuropixelsPartNumberRegistry.Resolve(partNumber, Entries, "Neuropixels 2.0");
    }
}
