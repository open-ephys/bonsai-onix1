using Bonsai.IO;

namespace OpenEphys.Onix1.DataFrameWriter
{
    abstract class ArrowFileSink<TSource> : FileSink<TSource, ArrowBatchWriter<TSource>>
    {
        public bool EnableCompression { get; set; } = false;
    }
}
