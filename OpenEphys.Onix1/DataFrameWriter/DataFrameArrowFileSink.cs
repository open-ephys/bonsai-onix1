using System;
using System.Collections.Generic;
using Apache.Arrow;
using Bonsai.IO;

namespace OpenEphys.Onix1.DataFrameWriter
{
    class DataFrameArrowFileSink : FileSink<DataFrame, ArrowBatchWriter<DataFrame>>
    {
        readonly TimeSpan timeout;

        public DataFrameArrowFileSink(TimeSpan timeout)
        {
            this.timeout = timeout;
        }

        protected override ArrowBatchWriter<DataFrame> CreateWriter(string filename, DataFrame frame)
        {
            var frameType = frame.GetType();
            var members = DataFrameWriterHelper.GetDataMembers(frameType);
            var schema = DataFrameWriterHelper.GenerateSchema(members, frame);
            var createRecordBatch = RecordBatchExpressionFactory.CreateBuilder<Func<IList<DataFrame>, Schema, RecordBatch>>( new DataFrameExpressionProvider(), frameType, members).Compile();
            var bufferSize = DataFrameWriterHelper.GetBufferSize(frameType);
            return new ArrowBatchWriter<DataFrame>(filename, schema, bufferSize, timeout, createRecordBatch);
        }

        protected override void Write(ArrowBatchWriter<DataFrame> writer, DataFrame input)
        {
            writer.Write(input);
        }
    }
}
