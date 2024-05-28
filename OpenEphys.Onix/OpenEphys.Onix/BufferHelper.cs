using OpenCV.Net;

namespace OpenEphys.Onix
{
    static class BufferHelper
    {
        public static Mat CopyTranspose<TBuffer>(
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
            var data = new Mat(bufferHeader.Cols, bufferHeader.Rows, depth, 1);
            CV.Transpose(bufferHeader, data);
            return data;
        }

        public static Mat CopyTranspose<TBuffer>(
            TBuffer[] buffer,
            int sampleCount,
            int channelCount,
            Depth depth,
            Mat scale,
            Mat transposeBuffer)
            where TBuffer : unmanaged
        {
            if (scale == null)
            {
                return CopyTranspose(buffer, sampleCount, channelCount, depth);
            }

            using var bufferHeader = Mat.CreateMatHeader(
                buffer,
                sampleCount,
                channelCount,
                depth,
                channels: 1);
            var data = new Mat(bufferHeader.Cols, bufferHeader.Rows, scale.Depth, 1);
            CV.Transpose(bufferHeader, transposeBuffer);
            CV.Mul(transposeBuffer, scale, data);
            return data;
        }
    }
}
