using Apache.Arrow;
using Apache.Arrow.Compression;
using Apache.Arrow.Ipc;
using System;
using System.IO;

namespace OpenEphys.Onix1.FrameWriter
{
    /// <summary>
    /// Writes Apache Arrow record batches to a stream.
    /// </summary>
    public class ArrowWriter : IDisposable
    {
        readonly Stream stream = null;
        readonly ArrowFileWriter writer = null;

        private protected bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the ArrowWriter class using the specified stream.
        /// </summary>
        /// <param name="filename">The name of the file on which the elements should be written.</param>
        /// <param name="schema">A <see cref="Schema"/> that defines the current file.</param>
        /// <param name="compressData">Flag that specifies whether to compress the data using Zstd compression.</param>
        public ArrowWriter(string filename, Schema schema, bool compressData)
        {
            var options = compressData
                ? new IpcOptions()
                  {
                      CompressionCodec = CompressionCodecType.Zstd,
                      CompressionCodecFactory = new CompressionCodecFactory(),
                      CompressionLevel = 1
                  }
                : null;
            stream = new FileStream(filename, FileMode.Create);
            writer = new(stream, schema, false, options);
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
        public virtual void Dispose()
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
