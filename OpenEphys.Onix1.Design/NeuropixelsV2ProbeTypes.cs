using System;
using Newtonsoft.Json;

namespace OpenEphys.Onix1.Design
{
    internal readonly record struct NeuropixelsV2ProbeTypeEntry(
        string DisplayName,
        Func<NeuropixelsV2ProbeGroup> CreateGroup,
        Func<string, NeuropixelsV2ProbeGroup> DeserializeGroup,
        Func<NeuropixelsV2ProbeConfiguration> CreateConfiguration,
        Predicate<NeuropixelsV2ProbeConfiguration> MatchesProbeConfiguration,
        Predicate<NeuropixelsV2ProbeGroup> MatchesProbeGroup
    );

    internal static class NeuropixelsV2ProbeTypes
    {
        internal static readonly NeuropixelsV2ProbeTypeEntry[] All =
        {
            new("Single Shank",
                () => new NeuropixelsV2SingleShankProbeGroup(),
                json => JsonConvert.DeserializeObject<NeuropixelsV2SingleShankProbeGroup>(json)
                        ?? new NeuropixelsV2SingleShankProbeGroup(),
                () => new NeuropixelsV2SingleShankProbeConfiguration(),
                pc => pc is NeuropixelsV2SingleShankProbeConfiguration,
                pg => pg is NeuropixelsV2SingleShankProbeGroup),
            new("Quad Shank",
                () => new NeuropixelsV2QuadShankProbeGroup(),
                json => JsonConvert.DeserializeObject<NeuropixelsV2QuadShankProbeGroup>(json)
                        ?? new NeuropixelsV2QuadShankProbeGroup(),
                () => new NeuropixelsV2QuadShankProbeConfiguration(),
                pc => pc is NeuropixelsV2QuadShankProbeConfiguration,
                pg => pg is NeuropixelsV2QuadShankProbeGroup),
        };

        internal static readonly string[] DisplayNames = Array.ConvertAll(All, e => e.DisplayName);
    }
}
