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
        readonly ArrowFileWriter writer = null;

        bool disposed = false;

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
