using OpenCV.Net;

namespace OpenEphys.Onix
{
    static class BufferHelper
    {
        public static Mat CopyBuffer<TBuffer>(
            TBuffer[] buffer,
            int sampleCount,
            int channelCount,
            Depth depth,
            Mat scale = null)
            where TBuffer : unmanaged
        {
            using var bufferHeader = Mat.CreateMatHeader(
                buffer,
                sampleCount,
                channelCount,
                depth,
                channels: 1);
            var data = new Mat(bufferHeader.Cols, bufferHeader.Rows, bufferHeader.Depth, 1);
            CV.Transpose(bufferHeader, data);
            if (scale != null)
            {
                CV.Mul(data, scale, data);
            }
            return data;
        }
    }
}
