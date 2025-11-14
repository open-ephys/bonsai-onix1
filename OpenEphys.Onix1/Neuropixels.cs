using System.Collections.Generic;
using System.Linq;

namespace OpenEphys.Onix1
{
    static class Neuropixels
    {
        internal static int[,] OrderChannelsByDepth(Electrode[] channelMap, int[,] rawToChannel)
        {
            int adcIndices = rawToChannel.GetLength(0);
            int frameIndices = rawToChannel.GetLength(1);

            // NB: Create reverse lookup table where the channel number is used to find the ADC index / frame index
            var channelToPosition = new Dictionary<int, (int adcIndex, int frameIndex)>();
            for (int adc = 0; adc < adcIndices; adc++)
            {
                for (int frame = 0; frame < frameIndices; frame++)
                {
                    channelToPosition[rawToChannel[adc, frame]] = (adc, frame);
                }
            }

            var spatiallyOrdered = channelMap
                .OrderBy(x => x.Position.Y)
                .ThenBy(x => x.Position.X)
                .ToArray();

            // NB: Populate the array with the spatially ordered channel indices by grabbing the original ADC index /
            //     frame index for that electrode channel number, and writing the new channel number at that index.
            //     Example:
            //       rawToChannel        = [0, 2, 4; 1, 3, 5] // Channels are in one column, in order 0 -> 2 -> 4 -> 1 -> 3 -> 5
            //       spatialRawToChannel = [0, 1, 2; 3, 4, 5]
            //
            //       Now, channel 2 is at index 1 in the data frame, channel 4 is index 2, channel 1 is index 3, etc.
            var spatialRawToChannel = new int[adcIndices, frameIndices];
            int index = 0;

            foreach (var e in spatiallyOrdered)
            {
                var (origAdcIndex, origFrameIndex) = channelToPosition[e.Channel];

                spatialRawToChannel[origAdcIndex, origFrameIndex] = index++;
            }

            return spatialRawToChannel;
        }
    }
}
