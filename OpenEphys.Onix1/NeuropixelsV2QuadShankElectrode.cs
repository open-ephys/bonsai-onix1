using System;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Class defining a <see cref="NeuropixelsV2QuadShankElectrode"/>.
    /// </summary>
    [Obsolete($"Use {nameof(NeuropixelsV2Electrode)} instead. This class is obsolete and will be removed.")]
    public class NeuropixelsV2QuadShankElectrode : NeuropixelsV2Electrode
    {
        /// <inheritdoc cref="NeuropixelsV2Electrode"/>
        [Obsolete($"Use {nameof(NeuropixelsV2Electrode)} instead. This class is obsolete and will be removed.")]
        public NeuropixelsV2QuadShankElectrode(int index) : base(index, NeuropixelsV2ProbeType.QuadShank)
        {
        }
    }
}
