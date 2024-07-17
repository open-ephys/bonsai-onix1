using System.Drawing;
using System.Xml.Serialization;

namespace OpenEphys.Onix
{
    public abstract class Electrode
    {
        /// <summary>
        /// Index of the electrode within the context of the probe
        /// </summary>
        [XmlIgnore]
        public int ElectrodeNumber { get; internal set; }
        /// <summary>
        /// The shank this electrode belongs to
        /// </summary>
        [XmlIgnore]
        public int Shank { get; internal set; }
        /// <summary>
        /// Index of the electrode within this shank
        /// </summary>
        [XmlIgnore]
        public int ShankIndex { get; internal set; }
        /// <summary>
        /// The bank, or logical block of channels, this electrode belongs to
        /// </summary>
        [XmlIgnore]
        public int Channel { get; internal set; }
        /// <summary>
        /// Location of the electrode in two-dimensional space
        /// </summary>
        [XmlIgnore]
        public PointF Position { get; internal set; }

        public Electrode()
        {
        }
    }
}
