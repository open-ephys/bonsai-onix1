using System;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Defines a configuration for quad-shank, Neuropixels 2.0 and 2.0-beta probes.
    /// </summary>
    [Obsolete($"Use {nameof(NeuropixelsV2ProbeConfiguration)} instead. This class will be removed.")]
    public class NeuropixelsV2QuadShankProbeConfiguration : NeuropixelsV2ProbeConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> class.
        /// </summary>
        [Obsolete($"Use {nameof(NeuropixelsV2ProbeConfiguration)} instead. This class will be removed.")]
        public NeuropixelsV2QuadShankProbeConfiguration(NeuropixelsV2Probe probe, NeuropixelsV2ShankReference reference)
            : base(probe, NeuropixelsV2ProbeType.QuadShank, reference)
        {
        }
    }
}
