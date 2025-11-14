using System;
using System.Xml.Serialization;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Class defining a <see cref="NeuropixelsV2Electrode"/>.
    /// </summary>
    [XmlInclude(typeof(NeuropixelsV2QuadShankElectrode))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public abstract class NeuropixelsV2Electrode : Electrode
    {
        /// <summary>
        /// Gets the bank, or logical block of channels, this electrode belongs to.
        /// </summary>
        [XmlIgnore]
        public NeuropixelsV2Bank Bank { get; init; }

        /// <summary>
        /// Gets the block this electrode belongs to.
        /// </summary>
        [XmlIgnore]
        public int Block { get; init; }

        /// <summary>
        /// Gets the index within the block this electrode belongs to.
        /// </summary>
        [XmlIgnore]
        public int BlockIndex { get; init; }

        internal abstract Func<int, int> GetChannelNumberFunc();
    }
}
