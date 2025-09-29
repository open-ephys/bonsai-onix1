using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenEphys.Onix1
{
    class ChannelHelper
    {
        internal static int[,] OrderChannelsByDepth(Electrode[] channelMap, int[,] originalOrder)
        {
            int rows = originalOrder.GetLength(0);
            int cols = originalOrder.GetLength(1);

            var channelToPosition = new Dictionary<int, (int row, int col)>();
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    channelToPosition[originalOrder[row, col]] = (row, col);
                }
            }

            var spatiallyOrdered = channelMap
                .OrderBy(x => x.Position.Y)
                .ThenBy(x => x.Position.X)
                .ToArray();

            var spatialRawToChannel = new int[rows, cols];
            int index = 0;

            foreach (var e in spatiallyOrdered)
            {
                var (newRow, newCol) = channelToPosition[index++];
                var (origRow, origCol) = channelToPosition[e.Channel];

                spatialRawToChannel[origRow, origCol] = originalOrder[newRow, newCol];
            }

            if (spatialRawToChannel.Cast<int>().Distinct().Count() != originalOrder.Length)
            {
                throw new InvalidOperationException($"An error occurred reordering the channels by depth. Expected {originalOrder.Length} channels," +
                    $" but only found {spatialRawToChannel.Cast<int>().Distinct().Count()} unique channels.");
            }

            return spatialRawToChannel;
        }
    }
}
