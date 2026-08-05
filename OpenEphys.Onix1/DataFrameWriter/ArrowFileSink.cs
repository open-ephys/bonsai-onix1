using Bonsai.IO;

namespace OpenEphys.Onix1.DataFrameWriter
{
    abstract class ArrowFileSink<TSource, TFrame> : FileSink<TSource, ArrowBatchWriter<TFrame>>
        where TSource : TFrame
    {
        public bool EnableCompression { get; set; } = false;
    }
}
