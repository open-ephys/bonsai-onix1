using System;
using System.Collections.Generic;
using Apache.Arrow;
using Bonsai.IO;

namespace OpenEphys.Onix1.FrameWriter
{
    class DataFrameSink : FileSink<DataFrame, ArrowBatchWriter<DataFrame>>
    {
        readonly Schema schema;
        readonly Func<IList<DataFrame>, Schema, RecordBatch> createRecordBatch;
        readonly int bufferSize;

        public DataFrameSink(
            Schema schema,
            Func<IList<DataFrame>, Schema, RecordBatch> createRecordBatch,
            int bufferSize)
        {
            this.schema = schema;
            this.createRecordBatch = createRecordBatch;
            this.bufferSize = bufferSize;
        }

        protected override ArrowBatchWriter<DataFrame> CreateWriter(string filename, DataFrame frame)
        {
            return new ArrowBatchWriter<DataFrame>(filename, schema, bufferSize, createRecordBatch);
        }

        protected override void Write(ArrowBatchWriter<DataFrame> writer, DataFrame input)
        {
            writer.Write(input);
        }
    }
}
