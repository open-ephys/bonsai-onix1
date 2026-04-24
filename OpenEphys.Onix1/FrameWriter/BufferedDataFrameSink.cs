using System;
using System.Collections.Generic;
using Apache.Arrow;
using Bonsai.IO;

namespace OpenEphys.Onix1.FrameWriter
{
    class BufferedDataFrameSink : FileSink<BufferedDataFrame, ArrowBatchWriter<BufferedDataFrame>>
    {
        readonly Schema schema;
        readonly Func<IList<BufferedDataFrame>, Schema, RecordBatch> createRecordBatch;
        readonly int bufferSize;

        public BufferedDataFrameSink(
            Schema schema,
            Func<IList<BufferedDataFrame>, Schema, RecordBatch> createRecordBatch,
            int bufferSize)
        {
            this.schema = schema;
            this.createRecordBatch = createRecordBatch;
            this.bufferSize = bufferSize;
        }

        protected override ArrowBatchWriter<BufferedDataFrame> CreateWriter(string filename, BufferedDataFrame dataFrame)
        {
            return new ArrowBatchWriter<BufferedDataFrame>(filename, schema, bufferSize, createRecordBatch);
        }

        protected override void Write(ArrowBatchWriter<BufferedDataFrame> writer, BufferedDataFrame input)
        {
            writer.Write(input);
        }
    }
}
