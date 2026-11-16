using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Apache.Arrow;

namespace OpenEphys.Onix1.DataFrameWriter
{
    /// <summary>
    /// Provides buffered writing of items to an Arrow file, batching items before writing.
    /// </summary>
    /// <typeparam name="T">The type of items to be written and batched.</typeparam>
    sealed class ArrowBatchWriter<T> : ArrowWriter
    {
        readonly Subject<T> subject = new();
        readonly IDisposable subscription;
        readonly RecordBatchBuilder<T> builder;

        bool isDisposed = false;

        /// <summary>
        /// Initializes a new instance of the ArrowBatchWriter class with the specified output file, schema, buffer
        /// size, and record batch builder.
        /// </summary>
        /// <param name="filename">The path to the output file.</param>
        /// <param name="schema">The schema describing the structure of the data.</param>
        /// <param name="bufferSize">The maximum number of items to buffer before writing a batch.</param>
        /// <param name="timeout">The maximum time to wait before writing a batch.</param>
        /// <param name="builder">The builder that creates each RecordBatch, which is disposed with the writer.</param>
        /// <param name="enableCompression">A boolean indicating whether to enable compression.</param>
        internal ArrowBatchWriter(string filename, Schema schema, int bufferSize, TimeSpan timeout, RecordBatchBuilder<T> builder, bool enableCompression)
            : base(filename, schema, enableCompression)
        {
            this.builder = builder;
            subscription = subject
                .Buffer(timeout, bufferSize)
                .Where(list => list.Count > 0)
                .Subscribe(items =>
                {
                    using var batch = builder.Build(items, schema);
                    WriteRecordBatch(batch);
                });
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
                    try
                    {
                        subject.OnCompleted();
                    }
                    finally
                    {
                        subscription.Dispose();
                        subject.Dispose();
                        builder.Dispose();
                    }
                }
            }

            base.Dispose(disposing);
        }
    }
}
