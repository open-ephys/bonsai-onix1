using Bonsai.IO;

namespace OpenEphys.Onix1.DataFrameWriter
{
    abstract class ArrowFileSink<T1, T2> : FileSink<T1, T2>, IArrowSinkOptions where T2 : ArrowWriter
    {
        public bool EnableCompression { get; set; } = false;
    }
}
