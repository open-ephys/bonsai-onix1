using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Apache.Arrow;

namespace OpenEphys.Onix1.DataFrameWriter
{
    /// <summary>
    /// Provides buffered writing of items to an Arrow file, batching items before writing.
    /// </summary>
    /// <typeparam name="T">The type of items to be written and batched.</typeparam>
    public sealed class ArrowBatchWriter<T> : ArrowWriter
    {
        readonly Subject<T> subject = new();
        readonly IDisposable subscription;

        bool isDisposed = false;

        /// <summary>
        /// Initializes a new instance of the ArrowBatchWriter class with the specified output file, schema, buffer
        /// size, and record batch creation delegate.
        /// </summary>
        /// <param name="filename">The path to the output file.</param>
        /// <param name="schema">The schema describing the structure of the data.</param>
        /// <param name="bufferSize">The maximum number of items to buffer before writing a batch.</param>
        /// <param name="timeout">The maximum time to wait before writing a batch.</param>
        /// <param name="createRecordBatch">A delegate to create a RecordBatch from a list of items and a schema.</param>
        public ArrowBatchWriter(string filename, Schema schema, int bufferSize, TimeSpan timeout, Func<IList<T>, Schema, RecordBatch> createRecordBatch)
            : base(filename, schema)
        {
            subscription = subject
                .Buffer(timeout, bufferSize)
                .Where(list => list.Count > 0)
                .Subscribe(items => WriteRecordBatch(createRecordBatch(items, schema))); 
        }

        /// <summary>
        /// Adds an item to the buffer and flushes the buffer when it reaches its maximum size.
        /// </summary>
        /// <param name="item">The item to add to the buffer.</param>
        public void Write(T item) => subject.OnNext(item);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                isDisposed = true;

                if (disposing)
                {
                    subject.OnCompleted();
                    subscription.Dispose();
                    subject.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
