using System;
using System.Collections.Generic;
using Apache.Arrow;
using Bonsai.IO;

namespace OpenEphys.Onix1.DataFrameWriter
{
    class DataFrameArrowFileSink : FileSink<DataFrame, ArrowBatchWriter<DataFrame>>
    {
        readonly Schema schema;
        readonly Func<IList<DataFrame>, Schema, RecordBatch> createRecordBatch;
        readonly int bufferSize;
        readonly TimeSpan timeout;

        public DataFrameArrowFileSink(
            Schema schema,
            Func<IList<DataFrame>, Schema, RecordBatch> createRecordBatch,
            int bufferSize,
            TimeSpan timeout)
        {
            this.schema = schema;
            this.createRecordBatch = createRecordBatch;
            this.bufferSize = bufferSize;
            this.timeout = timeout;
        }

        protected override ArrowBatchWriter<DataFrame> CreateWriter(string filename, DataFrame frame)
        {
            return new ArrowBatchWriter<DataFrame>(filename, schema, bufferSize, timeout, createRecordBatch);
        }

        protected override void Write(ArrowBatchWriter<DataFrame> writer, DataFrame input)
        {
            writer.Write(input);
        }
    }
}
