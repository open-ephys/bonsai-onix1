using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Defines a constrained probeInterface compatible probe group for Neuropixels 1.0 probes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Contact selection and channel mapping differs by variant in a way that can't be expressed as a single,
    /// uniform API (see <see cref="NeuropixelsV1Variant.HasChannelGroupSelection"/>). Most variants select
    /// per contact (<see cref="NeuropixelsV1ChannelToContactProbeGroup"/>), while UHD Switchable selects per
    /// channel group (<see cref="NeuropixelsV1ChannelGroupProbeGroup"/>). This base holds what's common to
    /// both: geometry, the resolved <see cref="Variant"/>, and the channel map. 
    /// </para>
    /// <para>
    /// Deserializing through this base type, rather than a concrete subtype directly, requires resolving to
    /// the correct concrete type first, see <see cref="NeuropixelsV1ProbeGroupConverter"/>, registered
    /// explicitly by callers that need it (deliberately not applied here via a <c>[JsonConverter]</c>
    /// attribute; see that type's remarks).
    /// </para>
    /// </remarks>
    public abstract class NeuropixelsV1ProbeGroup : SingleProbeGroup, IMultiplexedProbeGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV1ProbeGroup"/> class by copying
        /// an existing probe group.
        /// </summary>
        /// <param name="probeGroup">The probe group to copy.</param>
        protected NeuropixelsV1ProbeGroup(NeuropixelsV1ProbeGroup probeGroup)
            : base(probeGroup)
        {
            Variant = NeuropixelsV1VariantRegistry.Resolve(Probe.Annotations.ModelName);

            if (NumberOfContacts != Variant.ElectrodeCount)
                throw new ArgumentException($"Invalid number of contacts; expected {Variant.ElectrodeCount}, but found {NumberOfContacts}.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV1ProbeGroup"/> class from
        /// deserialized ProbeInterface data.
        /// </summary>
        /// <param name="specification">The ProbeInterface specification string.</param>
        /// <param name="version">The ProbeInterface version string.</param>
        /// <param name="probes">The array of probes deserialized from the ProbeInterface file.</param>
        protected NeuropixelsV1ProbeGroup(string specification, string version, Probe[] probes)
            : base(specification, version, probes)
        {
            Variant = NeuropixelsV1VariantRegistry.Resolve(Probe.Annotations.ModelName);

            if (NumberOfContacts != Variant.ElectrodeCount)
                throw new ArgumentException($"Invalid number of contacts; expected {Variant.ElectrodeCount}, but found {NumberOfContacts}.");
        }

        /// <summary>
        /// Returns a copy of this probe group with the same runtime type.
        /// </summary>
        internal abstract NeuropixelsV1ProbeGroup Clone();

        /// <summary>
        /// Sets every variant-specific bit of the shank configuration shift register into <paramref
        /// name="shankBits"/>: the electrode/channel/group-select bits, and (for variants with <see
        /// cref="NeuropixelsV1Variant.HasColumnSelectionSwitch"/>) <c>EN_A</c>/<c>EN_B</c> at the last two
        /// bit positions. Called once by <see cref="NeuropixelsV1.MakeShankBits"/>, which separately fills in
        /// the universal reference bits (<c>Ext1</c>/<c>Ext2</c>/<c>Tip1</c>/<c>Tip2</c>) that don't depend
        /// on which selection model this probe uses.
        /// </summary>
        /// <param name="layout">The register's bit layout for this probe's electrode count.</param>
        /// <param name="shankBits">The register bits, sized to <paramref name="layout"/>. Modified in
        /// place.</param>
        internal abstract void SetShankConfigurationBits(NeuropixelsV1ShankRegisterLayout layout, BitArray shankBits);

        /// <summary>
        /// Selects bank <paramref name="bank"/> for the whole probe, replacing the existing channel map.
        /// </summary>
        /// <param name="bank">The zero-based bank index.</param>
        public abstract void SelectBank(int bank);

        /// <summary>
        /// Returns the channel map for this probe, mapping each acquisition channel index to its active
        /// contact index.
        /// </summary>
        /// <remarks>
        /// This property hides <see cref="ProbeGroup.ChannelMap"/>, which is nullable, to enforce the
        /// invariant that a Neuropixels 1.0 probe group always has a valid channel map after construction. If
        /// no channel map was present in the deserialized data, a default configuration is applied
        /// automatically during construction.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the channel map is null.</exception>
        [JsonIgnore]
        public new IReadOnlyDictionary<int, int> ChannelMap =>
            base.ChannelMap ?? throw new InvalidOperationException($"Neuropixels probes must have a valid channel map.");

        /// <summary>
        /// True if a channel map is already present (e.g. from deserialized ProbeInterface data or a copied
        /// source).
        /// </summary>
        /// <remarks>
        /// Subclass constructors use this to decide whether a default configuration needs applying, instead
        /// of checking <see cref="ChannelMap"/> directly. NB: from a subclass, <c>base.ChannelMap</c>
        /// resolves to <em>this</em> class's throwing override (<c>base</c> only ever skips one level), not
        /// the nullable <c>SingleProbeGroup.ChannelMap</c> two levels up, so a null-check written as
        /// <c>base.ChannelMap is null</c> in a subclass constructor would throw instead of returning false,
        /// exactly backwards from what's intended.
        /// </remarks>
        protected bool HasChannelMap => base.ChannelMap != null;

        /// <summary>
        /// The resolved ASIC channel-wiring variant for this probe, keyed by the probe interface
        /// file's model name annotation.
        /// </summary>
        [JsonIgnore]
        internal NeuropixelsV1Variant Variant { get; private set; }

        /// <summary>
        /// Returns the acquisition channel number for the given contact index.
        /// </summary>
        /// <param name="contactIndex">The zero-based contact index across all electrodes on the
        /// probe.</param>
        /// <returns>The acquisition channel number (0 to <see cref="NeuropixelsV1.ChannelCount"/> -
        /// 1).</returns>
        public int GetChannel(int contactIndex) => Variant.GetChannel(contactIndex);

        /// <summary>
        /// Returns the zero-based index of the bank that contains the given contact index.
        /// </summary>
        /// <param name="contactIndex">The zero-based contact index across all electrodes on the
        /// probe.</param>
        /// <returns>The zero-based bank index.</returns>
        public static int GetBank(int contactIndex) => contactIndex / NeuropixelsV1.ChannelCount;

        /// <summary>
        /// The number of banks on this probe.
        /// </summary>
        public int BankCount => Variant.BankCount;
    }
}
