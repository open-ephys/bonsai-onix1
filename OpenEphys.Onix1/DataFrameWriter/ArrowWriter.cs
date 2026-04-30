using System;
using System.IO;
using Apache.Arrow;
using Apache.Arrow.Ipc;

namespace OpenEphys.Onix1.DataFrameWriter
{
    /// <summary>
    /// Writes Apache Arrow record batches to a stream.
    /// </summary>
    public class ArrowWriter : IDisposable
    {
        readonly Stream stream;
        readonly ArrowFileWriter writer;

        bool isDisposed = false;

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
        private protected void WriteRecordBatch(RecordBatch batch) => writer.WriteRecordBatch(batch);

        /// <summary>
        /// Disposes of the resources used by the instance, optionally releasing managed resources if disposing is true.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                isDisposed = true;

                if (disposing)
                {
                    try 
                    { 
                        writer.WriteEnd(); 
                    } 
                    finally 
                    {
                        writer.Dispose(); 
                        stream.Dispose(); 
                    }
                }
            }
        }

        /// <summary>
        /// Releases the resources used by the instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
