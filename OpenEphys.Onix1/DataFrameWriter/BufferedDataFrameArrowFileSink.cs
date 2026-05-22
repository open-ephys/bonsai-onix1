using System;
using System.Collections.Generic;
using Apache.Arrow;

namespace OpenEphys.Onix1.DataFrameWriter
{
    class BufferedDataFrameArrowFileSink : ArrowFileSink<BufferedDataFrame>
    {
        readonly TimeSpan timeout;

        public BufferedDataFrameArrowFileSink(TimeSpan timeout)
        {
            this.timeout = timeout;
        }

        protected override ArrowBatchWriter<BufferedDataFrame> CreateWriter(string filename, BufferedDataFrame dataFrame)
        {
            var frameType = dataFrame.GetType();
            var members = DataFrameWriterHelper.GetDataMembers(frameType);
            var fieldGroups = DataFrameWriterHelper.BuildFieldMappings(members, dataFrame);
            var schema = DataFrameWriterHelper.BuildSchema(fieldGroups);
            var createRecordBatch = RecordBatchExpressionFactory.CreateBuilder<Func<IList<BufferedDataFrame>, Schema, RecordBatch>>(
                new BufferedDataFrameExpressionProvider(), frameType, fieldGroups).Compile();
            var bufferSize = (int)Math.Ceiling((double)DataFrameWriterHelper.GetBufferSize(frameType) / dataFrame.Clock.Length);
            return new ArrowBatchWriter<BufferedDataFrame>(filename, schema, bufferSize, timeout, createRecordBatch, EnableCompression);
        }

        protected override void Write(ArrowBatchWriter<BufferedDataFrame> writer, BufferedDataFrame input)
        {
            writer.Write(input);
        }
    }
}
