using System;
using System.Drawing;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Class defining a <see cref="NeuropixelsV2QuadShankElectrode"/>.
    /// </summary>
    public class NeuropixelsV2QuadShankElectrode : NeuropixelsV2Electrode
    {
        /// <summary>
        /// Class defining a Neuropixels 2.0 quad-shank electrode.
        /// </summary>
        /// <param name="index">Global index of the electrode.</param>
        public NeuropixelsV2QuadShankElectrode(int index)
        {
            Index = index;
            Shank = GetShank(index);
            IntraShankElectrodeIndex = GetIntraShankIndex(index);
            Bank = GetBank(index);
            Block = GetBlock(index);
            BlockIndex = GetBlockIndex(index);
            Position = GetPosition(index);
            Channel = GetQuadShankChannelNumber(Shank, Block, BlockIndex);
        }

        NeuropixelsV2Bank GetBank(int index) => (NeuropixelsV2Bank)(GetIntraShankIndex(index) / NeuropixelsV2.ChannelCount);

        static int GetShank(int index) => index / NeuropixelsV2.ElectrodePerShank;

        static int GetIntraShankIndex(int index) => index % NeuropixelsV2.ElectrodePerShank;

        const int ElectrodePerBlock = 48;

        static int GetBlock(int index) => (GetIntraShankIndex(index) % NeuropixelsV2.ChannelCount) / ElectrodePerBlock;

        static int GetBlockIndex(int index) => GetIntraShankIndex(index) % ElectrodePerBlock;

        static PointF GetPosition(int index)
        {
            var position = NeuropixelsV2eQuadShankProbeGroup.DefaultContactPosition(index);
            return new PointF(x: position[0], y: position[1]);
        }

        /// <summary>
        /// Gets the channel number of a given electrode.
        /// </summary>
        /// <param name="index">Integer defining the index of the electrode in the probe.</param>
        /// <returns>An integer between 0 and 383 defining the channel number.</returns>
        public static int GetChannelNumber(int index)
        {
            var shank = GetShank(index);
            var block = GetBlock(index);
            var blockIndex = GetBlockIndex(index);

            return GetQuadShankChannelNumber(shank, block, blockIndex);
        }

        static int GetQuadShankChannelNumber(int shank, int block, int blockIndex) => (shank, block) switch
        {
            (0, 0) => blockIndex + ElectrodePerBlock * 0,
            (0, 1) => blockIndex + ElectrodePerBlock * 2,
            (0, 2) => blockIndex + ElectrodePerBlock * 4,
            (0, 3) => blockIndex + ElectrodePerBlock * 6,
            (0, 4) => blockIndex + ElectrodePerBlock * 5,
            (0, 5) => blockIndex + ElectrodePerBlock * 7,
            (0, 6) => blockIndex + ElectrodePerBlock * 1,
            (0, 7) => blockIndex + ElectrodePerBlock * 3,

            (1, 0) => blockIndex + ElectrodePerBlock * 1,
            (1, 1) => blockIndex + ElectrodePerBlock * 3,
            (1, 2) => blockIndex + ElectrodePerBlock * 5,
            (1, 3) => blockIndex + ElectrodePerBlock * 7,
            (1, 4) => blockIndex + ElectrodePerBlock * 4,
            (1, 5) => blockIndex + ElectrodePerBlock * 6,
            (1, 6) => blockIndex + ElectrodePerBlock * 0,
            (1, 7) => blockIndex + ElectrodePerBlock * 2,

            (2, 0) => blockIndex + ElectrodePerBlock * 4,
            (2, 1) => blockIndex + ElectrodePerBlock * 6,
            (2, 2) => blockIndex + ElectrodePerBlock * 0,
            (2, 3) => blockIndex + ElectrodePerBlock * 2,
            (2, 4) => blockIndex + ElectrodePerBlock * 1,
            (2, 5) => blockIndex + ElectrodePerBlock * 3,
            (2, 6) => blockIndex + ElectrodePerBlock * 5,
            (2, 7) => blockIndex + ElectrodePerBlock * 7,

            (3, 0) => blockIndex + ElectrodePerBlock * 5,
            (3, 1) => blockIndex + ElectrodePerBlock * 7,
            (3, 2) => blockIndex + ElectrodePerBlock * 1,
            (3, 3) => blockIndex + ElectrodePerBlock * 3,
            (3, 4) => blockIndex + ElectrodePerBlock * 0,
            (3, 5) => blockIndex + ElectrodePerBlock * 2,
            (3, 6) => blockIndex + ElectrodePerBlock * 4,
            (3, 7) => blockIndex + ElectrodePerBlock * 6,

            _ => throw new ArgumentOutOfRangeException($"Invalid shank and/or electrode value: {(shank, block)}"),
        };
    }
}
