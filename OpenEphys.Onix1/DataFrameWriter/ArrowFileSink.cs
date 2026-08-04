using Bonsai.IO;

namespace OpenEphys.Onix1.DataFrameWriter
{
    interface IArrowFileSink
    {
        bool EnableCompression { get; set; }
    }

    abstract class ArrowFileSink<TSource, TFrame> : FileSink<TSource, ArrowBatchWriter<TFrame>>, IArrowFileSink
        where TSource : TFrame
    {
        public bool EnableCompression { get; set; } = false;
    }
}
