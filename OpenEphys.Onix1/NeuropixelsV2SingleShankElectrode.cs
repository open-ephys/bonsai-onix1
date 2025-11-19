using System;
using System.Drawing;
using System.Xml.Serialization;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Class defining a <see cref="NeuropixelsV2SingleShankElectrode"/>.
    /// </summary>
    public class NeuropixelsV2SingleShankElectrode : NeuropixelsV2Electrode
    {
        /// <summary>
        /// Gets the zero-indexed column index of this electrode.
        /// </summary>
        [XmlIgnore]
        public int ColumnIndex { get; set; }

        /// <summary>
        /// Class defining a Neuropixels 2.0 single-shank electrode.
        /// </summary>
        /// <param name="index">Global index of the electrode.</param>
        public NeuropixelsV2SingleShankElectrode(int index)
        {
            Index = index;
            Shank = 0;
            IntraShankElectrodeIndex = index;
            Bank = GetBank(index);
            Block = GetBlock(index);
            BlockIndex = GetBlockIndex(index);
            Position = GetPosition(index);
            ColumnIndex = GetColumnIndex(index);
            Channel = GetSingleShankChannelNumber(Bank, Block, GetRow(index), ColumnIndex);
        }

        static NeuropixelsV2Bank GetBank(int index) => (NeuropixelsV2Bank)(index / NeuropixelsV2.ChannelCount);

        const int ElectrodesPerBlock = 32;

        static int GetBankIndex(int index) => index % NeuropixelsV2.ChannelCount;

        static int GetBlock(int index) => GetBankIndex(index) / ElectrodesPerBlock;

        static int GetBlockIndex(int index) => index % ElectrodesPerBlock;

        const int ElectrodesPerRow = 2;

        static int GetRow(int index) => GetBankIndex(index) % ElectrodesPerBlock / ElectrodesPerRow;

        static PointF GetPosition(int index)
        {
            var position = NeuropixelsV2eProbeGroup.DefaultContactPosition(index);
            return new PointF(x: position[0], y: position[1]);
        }

        static int GetColumnIndex(int index)
        {
            return index % 2;
        }

        /// <summary>
        /// Gets the channel number of a given electrode.
        /// </summary>
        /// <param name="index">Integer defining the index of the electrode in the probe.</param>
        /// <returns>An integer between 0 and 383 defining the channel number.</returns>
        public static int GetChannelNumber(int index)
        {
            var bank = GetBank(index);
            var block = GetBlock(index);
            var row = GetRow(index);
            var columnIndex = GetColumnIndex(index);

            return GetSingleShankChannelNumber(bank, block, row, columnIndex);
        }

        static int GetSingleShankChannelNumber(NeuropixelsV2Bank bank, int block, int row, int columnIndex)
        {
            const int MaxBlockValue = 11;
            const int MaxRowValue = 15;

            if (block > MaxBlockValue || block < 0)
                throw new ArgumentOutOfRangeException($"Block value is out of range. Expected to be between 0 and {MaxBlockValue}, but value is {block}");

            if (row > MaxRowValue || row < 0)
                throw new ArgumentOutOfRangeException($"Row value is out of range. Expected to be between 0 and {MaxRowValue}, but value is {row}");

            if (columnIndex > 1 || columnIndex < 0)
                throw new ArgumentOutOfRangeException($"Column index value is out of range. Expected to be between 0 and 1, but value was {columnIndex}.");

            const int HalfBlock = 16;

            return (bank, columnIndex) switch
            {
                (NeuropixelsV2Bank.A, 0) => row * ElectrodesPerRow + block * ElectrodesPerBlock,
                (NeuropixelsV2Bank.A, 1) => row * ElectrodesPerRow + block * ElectrodesPerBlock + 1,
                (NeuropixelsV2Bank.B, 0) => (row * 7 % HalfBlock) * ElectrodesPerRow + block * ElectrodesPerBlock,
                (NeuropixelsV2Bank.B, 1) => ((row * 7 + 4) % HalfBlock) * ElectrodesPerRow + block * ElectrodesPerBlock + 1,
                (NeuropixelsV2Bank.C, 0) => (row * 5 % HalfBlock) * ElectrodesPerRow + block * ElectrodesPerBlock,
                (NeuropixelsV2Bank.C, 1) => ((row * 5 + 8) % HalfBlock) * ElectrodesPerRow + block * ElectrodesPerBlock + 1,
                (NeuropixelsV2Bank.D, 0) => (row * 3 % HalfBlock) * ElectrodesPerRow + block * ElectrodesPerBlock,
                (NeuropixelsV2Bank.D, 1) => ((row * 3 + 12) % HalfBlock) * ElectrodesPerRow + block * ElectrodesPerBlock + 1,
                _ => throw new NotImplementedException($"Invalid {nameof(NeuropixelsV2Bank)} value given.")
            };
        }
    }
}
