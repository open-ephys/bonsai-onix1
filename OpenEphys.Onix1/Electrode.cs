using System.Drawing;
using System.Xml.Serialization;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Abstract base class for describing a single electode.
    /// </summary>
    public abstract class Electrode
    {
        /// <summary>
        /// Gets the index of the electrode (the electrode "number") within
        /// the context of the entire probe.
        /// </summary>
        [XmlIgnore]
        public int Index { get; internal set; }

        /// <summary>
        /// Gets the shank this electrode belongs to.
        /// </summary>
        [XmlIgnore]
        public int Shank { get; internal set; }

        /// <summary>
        /// Gets the index of the electrode within the context of <see cref="Shank"/>.
        /// </summary>
        [XmlIgnore]
        public int IntraShankIndex { get; internal set; }

        /// <summary>
        /// Gets the electrical channel that this electode is mapped to.
        /// </summary>
        [XmlIgnore]
        public int Channel { get; internal set; }

        /// <summary>
        /// Gets the location of the electrode in two-dimensional space in arbitrary units.
        /// </summary>
        [XmlIgnore]
        public PointF Position { get; internal set; }
    }
}
