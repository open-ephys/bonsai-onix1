using System;
using System.Collections.Generic;
using Apache.Arrow;

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
            var createRecordBatch = RecordBatchExpressionFactory.CreateBuilder<Func<IList<DataFrame>, Schema, RecordBatch>>(
                new DataFrameExpressionProvider(), frameType, fieldGroups).Compile();
            var bufferSize = DataFrameWriterHelper.GetBufferSize(frameType);
            return new ArrowBatchWriter<DataFrame>(filename, schema, bufferSize, timeout, createRecordBatch, EnableCompression);
        }

        protected override void Write(ArrowBatchWriter<DataFrame> writer, TSource input)
        {
            writer.Write(input);
        }
    }
}
