using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenEphys.Onix1
{
    static class Neuropixels
    {
        internal static int[,] OrderChannelsByDepth(Electrode[] channelMap, int[,] frameIndex)
        {
            int rows = frameIndex.GetLength(0);
            int cols = frameIndex.GetLength(1);

            var channelToPosition = new Dictionary<int, (int row, int col)>();
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    channelToPosition[frameIndex[row, col]] = (row, col);
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
                var (origRow, origCol) = channelToPosition[e.Channel];

                spatialRawToChannel[origRow, origCol] = index++;
            }

            return spatialRawToChannel;
        }
    }
}
