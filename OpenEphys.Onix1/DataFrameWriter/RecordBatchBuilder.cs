using System;
using System.Collections.Generic;
using Apache.Arrow;

namespace OpenEphys.Onix1.DataFrameWriter
{
    /// <summary>
    /// Builds record batches from lists of items, and owns the resources, such as native buffers, that the batches are
    /// built over.
    /// </summary>
    /// <remarks>
    /// Each batch must be written and disposed before the next one is built, since the next batch may reuse the
    /// resources of the previous one. The builder must therefore not be shared between writers, and the resources
    /// are disposed with the builder.
    /// </remarks>
    /// <typeparam name="T">The type of the items in a batch.</typeparam>
    sealed class RecordBatchBuilder<T> : IDisposable
    {
        readonly Func<IList<T>, Schema, RecordBatch> build;
        readonly IDisposable[] resources;

        internal RecordBatchBuilder(Func<IList<T>, Schema, RecordBatch> build, params IDisposable[] resources)
        {
            this.build = build ?? throw new ArgumentNullException(nameof(build));
            this.resources = resources;
        }

        internal IReadOnlyList<IDisposable> Resources => resources;

        internal RecordBatch Build(IList<T> items, Schema schema) => build(items, schema);

        public void Dispose()
        {
            foreach (var resource in resources)
                resource.Dispose();
        }
    }
}
