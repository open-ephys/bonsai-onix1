using System;
using System.IO;
using System.Threading;
using Apache.Arrow;
using Apache.Arrow.Ipc;

namespace OpenEphys.Onix1.FrameWriter
{
    /// <summary>
    /// Writes Apache Arrow record batches to a stream.
    /// </summary>
    public class ArrowWriter : IDisposable
    {
        readonly Stream stream = null;
        readonly ArrowFileWriter writer = null;

        int isDisposed = 0;

        /// <summary>
        /// Initializes a new instance of the ArrowWriter class using the specified stream.
        /// </summary>
        /// <param name="filename">The name of the file on which the elements should be written.</param>
        /// <param name="schema">A <see cref="Schema"/> that defines the current file.</param>
        public ArrowWriter(string filename, Schema schema)
        {
            stream = new FileStream(filename, FileMode.Create);
            writer = new(stream, schema);
            writer.WriteStart();
        }

        /// <summary>
        /// Writes the specified record batch to the underlying stream.
        /// </summary>
        /// <param name="batch">The record batch to write.</param>
        public void Write(RecordBatch batch)
        {
            writer.WriteRecordBatch(batch);
        }

        /// <summary>
        /// Disposes of the resources used by the instance, optionally releasing managed resources if disposing is true.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref isDisposed, 1, 0) == 0)
            {
                if (disposing)
                {
                    writer.WriteEnd();
                    writer.Dispose();
                    stream.Dispose();
                }
            }
        }

        /// <summary>
        /// Releases the resources used by the instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
