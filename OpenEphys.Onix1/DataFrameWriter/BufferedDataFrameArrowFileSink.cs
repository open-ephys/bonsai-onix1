using System;
using System.Collections.Generic;
using Apache.Arrow;
using Bonsai.IO;

namespace OpenEphys.Onix1.FrameWriter
{
    class BufferedDataFrameArrowFileSink : FileSink<BufferedDataFrame, ArrowBatchWriter<BufferedDataFrame>>
    {
        readonly Schema schema;
        readonly Func<IList<BufferedDataFrame>, Schema, RecordBatch> createRecordBatch;
        readonly int bufferSize;
        readonly TimeSpan timeout;

        public BufferedDataFrameArrowFileSink(
            Schema schema,
            Func<IList<BufferedDataFrame>, Schema, RecordBatch> createRecordBatch,
            int bufferSize,
            TimeSpan timeout)
        {
            this.schema = schema;
            this.createRecordBatch = createRecordBatch;
            this.bufferSize = bufferSize;
            this.timeout = timeout;
        }

        protected override ArrowBatchWriter<BufferedDataFrame> CreateWriter(string filename, BufferedDataFrame dataFrame)
        {
            return new ArrowBatchWriter<BufferedDataFrame>(filename, schema, bufferSize, timeout, createRecordBatch);
        }

        protected override void Write(ArrowBatchWriter<BufferedDataFrame> writer, BufferedDataFrame input)
        {
            writer.Write(input);
        }
    }
}
