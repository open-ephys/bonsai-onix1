using System;

namespace OpenEphys.Onix1.DataFrameWriter
{
    class DataFrameArrowFileSink<TSource> : ArrowFileSink<TSource, DataFrame>
        where TSource : DataFrame
    {
        readonly TimeSpan timeout;

        public DataFrameArrowFileSink(TimeSpan timeout)
        {
            this.timeout = timeout;
        }

        protected override ArrowBatchWriter<DataFrame> CreateWriter(string filename, TSource frame)
        {
            var frameType = frame.GetType();
            var members = DataFrameWriterHelper.GetDataMembers(frameType);
            var fieldGroups = DataFrameWriterHelper.BuildFieldMappings(members, frame);
            var schema = DataFrameWriterHelper.BuildSchema(fieldGroups);
            var builder = RecordBatchExpressionFactory.CreateBuilder<DataFrame>(
                new DataFrameExpressionProvider(), frameType, fieldGroups);
            var bufferSize = DataFrameWriterHelper.GetBufferSize(frameType);
            return new ArrowBatchWriter<DataFrame>(filename, schema, bufferSize, timeout, builder, EnableCompression);
        }

        protected override void Write(ArrowBatchWriter<DataFrame> writer, TSource input)
        {
            writer.Write(input);
        }
    }
}
