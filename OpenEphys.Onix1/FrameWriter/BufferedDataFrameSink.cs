using System;
using Apache.Arrow;
using Bonsai.IO;

namespace OpenEphys.Onix1.FrameWriter
{
    class BufferedDataFrameSink : FileSink<RecordBatch, ArrowWriter>
    {
        protected override ArrowWriter CreateWriter(string filename, RecordBatch batch)
        {
            return new ArrowWriter(filename, batch.Schema);
        }

        protected override void Write(ArrowWriter writer, RecordBatch input)
        {
            writer.Write(input);
        }

        public IObservable<BufferedDataFrame> Process(IObservable<BufferedDataFrame> source, Func<BufferedDataFrame, RecordBatch> selector)
        {
            return base.Process(source, selector);
        }
    }
}
