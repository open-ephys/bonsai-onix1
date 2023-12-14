using OpenCV.Net;

namespace OpenEphys.Onix
{
    static class BufferHelper
    {
        public static Mat CopyBuffer<TBuffer>(
            TBuffer[] buffer,
            int sampleCount,
            int channelCount,
            Depth depth)
            where TBuffer : unmanaged
        {
            using var bufferHeader = Mat.CreateMatHeader(
                buffer,
                sampleCount,
                channelCount,
                depth,
                channels: 1);
            var amplifierData = new Mat(bufferHeader.Cols, bufferHeader.Rows, bufferHeader.Depth, 1);
            CV.Transpose(bufferHeader, amplifierData);
            return amplifierData;
        }
    }
}
