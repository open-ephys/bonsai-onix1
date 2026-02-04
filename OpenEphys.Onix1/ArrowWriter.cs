using Apache.Arrow;
using Apache.Arrow.Ipc;
using System;
using System.IO;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Writes Apache Arrow record batches to a stream.
    /// </summary>
    public class ArrowWriter : IDisposable
    {
        readonly Stream stream = null;
        ArrowFileWriter writer = null;

        bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the ArrowWriter class using the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write Arrow data to.</param>
        public ArrowWriter(Stream stream)
        {
            this.stream = stream;
        }

        /// <summary>
        /// Writes the specified record batch to the underlying stream.
        /// </summary>
        /// <param name="batch">The record batch to write.</param>
        public void Write(RecordBatch batch)
        {
            if (writer == null)
            {
                writer = new(stream, batch.Schema);
                writer.WriteStart();
            }

            writer.WriteRecordBatch(batch);
            batch.Dispose();
        }

        /// <summary>
        /// Releases the resources used by the instance.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                if (writer != null)
                {
                    writer.WriteEnd();
                    writer.Dispose();
                }

                stream?.Dispose();

                disposed = true;
            }
        }
    }
}
