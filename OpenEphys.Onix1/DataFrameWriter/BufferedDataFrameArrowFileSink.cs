using System;
using System.Collections.Generic;
using Apache.Arrow;
using Bonsai.IO;

namespace OpenEphys.Onix1.DataFrameWriter
{
    class BufferedDataFrameArrowFileSink : FileSink<BufferedDataFrame, ArrowBatchWriter<BufferedDataFrame>>
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
            var schema = DataFrameWriterHelper.GenerateSchema(members, dataFrame);
            var createRecordBatch = RecordBatchExpressionFactory.CreateBuilder<Func<IList<BufferedDataFrame>, Schema, RecordBatch>>(
                new BufferedDataFrameExpressionProvider(), frameType, members).Compile();
            var bufferSize = (int)Math.Ceiling((double)DataFrameWriterHelper.GetBufferSize(frameType) / dataFrame.Clock.Length);
            return new ArrowBatchWriter<BufferedDataFrame>(filename, schema, bufferSize, timeout, createRecordBatch);
        }

        protected override void Write(ArrowBatchWriter<BufferedDataFrame> writer, BufferedDataFrame input)
        {
            writer.Write(input);
        }
    }
}
