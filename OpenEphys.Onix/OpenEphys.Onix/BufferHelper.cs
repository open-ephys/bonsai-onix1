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

        public static Mat CopyConvertBuffer<TBuffer>(
            TBuffer[] buffer,
            int sampleCount,
            int channelCount,
            Depth depth,
            double scale = 1,
            double shift = 0)
            where TBuffer : unmanaged
        {
            using var bufferHeader = Mat.CreateMatHeader(
                buffer,
                channelCount,
                sampleCount,
                depth,
                channels: 1);
            var data = new Mat(bufferHeader.Rows, bufferHeader.Cols, bufferHeader.Depth, 1);
            CV.ConvertScale(bufferHeader, data, scale, shift);
            return data;
        }

        public static Mat CopyTransposeBuffer<TBuffer>(
            TBuffer[] buffer,
            int sampleCount,
            int channelCount,
            Depth depth,
            Mat scale)
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
