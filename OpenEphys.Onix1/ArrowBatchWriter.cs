using System;
using System.Collections.Generic;
using Apache.Arrow;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Provides buffered writing of items to an Arrow file, batching items before writing.
    /// </summary>
    /// <typeparam name="T">The type of items to be written and batched.</typeparam>
    public class ArrowBatchWriter<T> : ArrowWriter
    {
        readonly int bufferSize;
        readonly List<T> buffer;
        readonly Func<IList<T>, Schema, RecordBatch> createRecordBatch;
        readonly Schema schema;

        /// <summary>
        /// Initializes a new instance of the ArrowBatchWriter class with the specified output file, schema, buffer
        /// size, and record batch creation delegate.
        /// </summary>
        /// <param name="filename">The path to the output file.</param>
        /// <param name="schema">The schema describing the structure of the data.</param>
        /// <param name="bufferSize">The maximum number of items to buffer before writing a batch.</param>
        /// <param name="createRecordBatch">A delegate to create a RecordBatch from a list of items and a schema.</param>
        public ArrowBatchWriter(string filename, Schema schema, int bufferSize, Func<IList<T>, Schema, RecordBatch> createRecordBatch)
            : base(filename, schema)
        {
            this.schema = schema;
            this.bufferSize = bufferSize;
            buffer = new(bufferSize);
            this.createRecordBatch = createRecordBatch;
        }

        /// <summary>
        /// Adds an item to the buffer and flushes the buffer when it reaches its maximum size.
        /// </summary>
        /// <param name="item">The item to add to the buffer.</param>
        public void Write(T item)
        {
            buffer.Add(item);

            if (buffer.Count >= bufferSize)
            {
                Flush();
            }
        }

        /// <summary>
        /// Writes any buffered records as a batch and clears the buffer.
        /// </summary>
        public void Flush()
        {
            if (buffer.Count > 0)
            {
                var recordBatch = createRecordBatch(buffer, schema);
                base.Write(recordBatch);
                buffer.Clear();
            }
        }

        /// <summary>
        /// Releases resources used by the object and flushes any buffered data.
        /// </summary>
        public override void Dispose()
        {
            if (!disposed)
            {
                Flush();
                base.Dispose();
            }
        }
    }
}
