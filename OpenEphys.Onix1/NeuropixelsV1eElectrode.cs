using System.Drawing;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Class defining a <see cref="NeuropixelsV1eElectrode"/>.
    /// </summary>
    public class NeuropixelsV1eElectrode : Electrode
    {
        /// <summary>
        /// The bank, or logical block of channels, this electrode belongs to
        /// </summary>
        public NeuropixelsV1Bank Bank { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV1eElectrode"/> using the given index.
        /// </summary>
        /// <param name="index">Integer defining the zero-indexed position of the electrode in the probe.</param>
        public NeuropixelsV1eElectrode(int index)
        {
            Index = index;
            Shank = 0;
            IntraShankElectrodeIndex = index;
            Bank = (NeuropixelsV1Bank)(index / NeuropixelsV1e.ChannelCount);
            Channel = GetChannelNumber(index);
            var position = NeuropixelsV1eProbeGroup.DefaultContactPosition(index);
            Position = new PointF(position[0], position[1]);
        }

        /// <summary>
        /// Static method returning the channel number of a given electrode.
        /// </summary>
        /// <param name="electrodeIndex">Integer defining the index of the electrode in the probe.</param>
        /// <returns>An integer between 0 and 383 defining the channel number.</returns>
        public static int GetChannelNumber(int electrodeIndex)
        {
            return electrodeIndex % NeuropixelsV1e.ChannelCount;
        }
    }
}
