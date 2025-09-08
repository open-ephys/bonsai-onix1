using System;
using System.Drawing;
using System.Xml.Serialization;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Class defining a <see cref="NeuropixelsV2Electrode"/>.
    /// </summary>
    public class NeuropixelsV2Electrode : Electrode
    {
        /// <summary>
        /// Gets the bank, or logical block of channels, this electrode belongs to.
        /// </summary>
        [XmlIgnore]
        public NeuropixelsV2Bank Bank { get; private set; }

        /// <summary>
        /// Gets the block this electrode belongs to.
        /// </summary>
        [XmlIgnore]
        public int Block { get; private set; }

        /// <summary>
        /// Gets the index within the block this electrode belongs to.
        /// </summary>
        [XmlIgnore]
        public int BlockIndex { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NeuropixelsV2Electrode"/> class.
        /// </summary>
        /// <param name="index">Integer defining the index of the contact.</param>
        /// <param name="probeType">Probe type that this electrode is a part of.</param>
        public NeuropixelsV2Electrode(int index, NeuropixelsV2ProbeType probeType)
        {
            Index = index;
            Shank = GetShank(index);
            IntraShankElectrodeIndex = GetIntraShankIndex(index);
            Bank = GetBank(index);
            Block = GetBlock(index, probeType);
            BlockIndex = GetBlockIndex(index, probeType);
            Position = GetPosition(index);

            if (probeType == NeuropixelsV2ProbeType.SingleShank)
            {
                Channel = GetSingleShankChannelNumber(Bank, Block, GetRow(index), index % 2 == 0);
            }
            else if (probeType == NeuropixelsV2ProbeType.QuadShank)
            {
                Channel = GetQuadShankChannelNumber(Shank, Block, BlockIndex);
            }
            else
                throw new InvalidOperationException("Unknown probe type given.");
        }

        private PointF GetPosition(int electrodeNumber)
        {
            var position = NeuropixelsV2eProbeGroup.DefaultContactPosition(electrodeNumber);
            return new PointF(x: position[0], y: position[1]);
        }

        static NeuropixelsV2Bank GetBank(int index) => (NeuropixelsV2Bank)(GetIntraShankIndex(index) / NeuropixelsV2.ChannelCount);

        internal static int GetShank(int index) => index / NeuropixelsV2.ElectrodePerShank;

        internal static int GetIntraShankIndex(int index) => index % NeuropixelsV2.ElectrodePerShank;

        static int GetBlock(int index, NeuropixelsV2ProbeType probeType)
        {
            if (probeType == NeuropixelsV2ProbeType.SingleShank)
                return (index % NeuropixelsV2.ChannelCount) / NeuropixelsV2.ElectrodePerBlockSingleShank;

            else if (probeType == NeuropixelsV2ProbeType.QuadShank)
                return (GetIntraShankIndex(index) % NeuropixelsV2.ChannelCount) / NeuropixelsV2.ElectrodePerBlockQuadShank;

            else
                throw new InvalidOperationException("Invalid probe type given.");
        }

        const int ElectrodesPerRow = 2;

        static int GetRow(int index) => (index % NeuropixelsV2.ElectrodePerBlockSingleShank) / ElectrodesPerRow;

        static int GetBlockIndex(int index, NeuropixelsV2ProbeType probeType)
        {
            if (probeType == NeuropixelsV2ProbeType.SingleShank)
                return GetIntraShankIndex(index) % NeuropixelsV2.ElectrodePerBlockSingleShank;

            else if (probeType == NeuropixelsV2ProbeType.QuadShank)
                return GetIntraShankIndex(index) % NeuropixelsV2.ElectrodePerBlockQuadShank;

            else
                throw new InvalidOperationException("Invalid probe type given.");
        }

        /// <summary>
        /// Static method returning the channel number of a given electrode.
        /// </summary>
        /// <param name="electrodeIndex">Integer defining the index of the electrode in the probe.</param>
        /// <param name="probeType">Probe type that this electrode is a part of.</param>
        /// <returns>An integer between 0 and 383 defining the channel number.</returns>
        public static int GetChannelNumber(int electrodeIndex, NeuropixelsV2ProbeType probeType)
        {
            if (probeType == NeuropixelsV2ProbeType.SingleShank)
            {
                var bank = GetBank(electrodeIndex);
                var block = GetBlock(electrodeIndex, probeType);
                var row = GetRow(electrodeIndex);
                bool isEven = electrodeIndex % 2 == 0;

                return GetSingleShankChannelNumber(bank, block, row, isEven);
            }
            else if (probeType == NeuropixelsV2ProbeType.QuadShank)
            {
                var shank = GetShank(electrodeIndex);
                var block = GetBlock(electrodeIndex, probeType);
                var blockIndex = GetBlockIndex(electrodeIndex, probeType);

                return GetQuadShankChannelNumber(shank, block, blockIndex);
            }
            else
                throw new InvalidOperationException("Unknown probe type given.");
        }

        internal static int GetSingleShankChannelNumber(NeuropixelsV2Bank bank, int block, int row, bool even)
        {
            const int MaxBlockValue = 11;
            const int MaxRowValue = 15;

            if (block > MaxBlockValue || block < 0)
                throw new ArgumentOutOfRangeException($"Block value is out of range. Expected to be between 0 and {MaxBlockValue}, but value is {block}");

            if (row > MaxRowValue || row < 0)
                throw new ArgumentOutOfRangeException($"Row value is out of range. Expected to be between 0 and {MaxRowValue}, but value is {row}");

            int offset = even ? 0 : 1; // NB: Left electrodes (even numbers) have no offset, while right electrodes (odd numbers) have +1

            const int HalfBlock = 16;

            return bank switch
            {
                NeuropixelsV2Bank.A => row * ElectrodesPerRow + block * NeuropixelsV2.ElectrodePerBlockSingleShank + offset,
                NeuropixelsV2Bank.B => (row * 7 % HalfBlock) * ElectrodesPerRow + block * NeuropixelsV2.ElectrodePerBlockSingleShank + offset,
                NeuropixelsV2Bank.C => (row * 5 % HalfBlock) * ElectrodesPerRow + block * NeuropixelsV2.ElectrodePerBlockSingleShank + offset,
                NeuropixelsV2Bank.D => (row * 3 % HalfBlock) * ElectrodesPerRow + block * NeuropixelsV2.ElectrodePerBlockSingleShank + offset,
                _ => throw new NotImplementedException($"Invalid {nameof(NeuropixelsV2Bank)} value given.")
            };
        }

        internal static int GetQuadShankChannelNumber(int shank, int block, int blockIndex) => (shank, block) switch
        {
            (0, 0) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 0,
            (0, 1) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 2,
            (0, 2) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 4,
            (0, 3) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 6,
            (0, 4) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 5,
            (0, 5) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 7,
            (0, 6) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 1,
            (0, 7) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 3,

            (1, 0) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 1,
            (1, 1) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 3,
            (1, 2) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 5,
            (1, 3) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 7,
            (1, 4) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 4,
            (1, 5) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 6,
            (1, 6) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 0,
            (1, 7) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 2,

            (2, 0) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 4,
            (2, 1) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 6,
            (2, 2) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 0,
            (2, 3) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 2,
            (2, 4) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 1,
            (2, 5) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 3,
            (2, 6) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 5,
            (2, 7) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 7,

            (3, 0) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 5,
            (3, 1) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 7,
            (3, 2) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 1,
            (3, 3) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 3,
            (3, 4) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 0,
            (3, 5) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 2,
            (3, 6) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 4,
            (3, 7) => blockIndex + NeuropixelsV2.ElectrodePerBlockQuadShank * 6,

            _ => throw new ArgumentOutOfRangeException($"Invalid shank and/or electrode value: {(shank, block)}"),
        };
    }
}
