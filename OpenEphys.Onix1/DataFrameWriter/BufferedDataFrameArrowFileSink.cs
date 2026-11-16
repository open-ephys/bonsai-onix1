using System;

namespace OpenEphys.Onix1.DataFrameWriter
{
    class BufferedDataFrameArrowFileSink<TSource> : ArrowFileSink<TSource, BufferedDataFrame>
        where TSource : BufferedDataFrame
    {
        readonly TimeSpan timeout;

        public BufferedDataFrameArrowFileSink(TimeSpan timeout)
        {
            this.timeout = timeout;
        }

        protected override ArrowBatchWriter<BufferedDataFrame> CreateWriter(string filename, TSource dataFrame)
        {
            var frameType = dataFrame.GetType();
            var members = DataFrameWriterHelper.GetDataMembers(frameType);
            var fieldGroups = DataFrameWriterHelper.BuildFieldMappings(members, dataFrame);
            var schema = DataFrameWriterHelper.BuildSchema(fieldGroups);
            var builder = RecordBatchExpressionFactory.CreateBuilder<BufferedDataFrame>(
                new BufferedDataFrameExpressionProvider(), frameType, fieldGroups);
            var bufferSize = (int)Math.Ceiling((double)DataFrameWriterHelper.GetBufferSize(frameType) / dataFrame.Clock.Length);
            return new ArrowBatchWriter<BufferedDataFrame>(filename, schema, bufferSize, timeout, builder, EnableCompression);
        }

        protected override void Write(ArrowBatchWriter<BufferedDataFrame> writer, TSource input)
        {
            writer.Write(input);
        }
    }
}
