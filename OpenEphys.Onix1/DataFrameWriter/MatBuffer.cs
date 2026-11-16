using System;
using OpenCV.Net;

namespace OpenEphys.Onix1.DataFrameWriter
{
    /// <summary>
    /// Holds the native memory that the arrays of a record batch are built over, and reuses it from one batch to the
    /// next for as long as its shape does not change.
    /// </summary>
    /// <remarks>
    /// The memory can be reused and disposed deterministically because all of its users are known: the record batch
    /// builder that writes into it and the record batch built over it, which is written and disposed before the next
    /// batch is built. The <see cref="Mat"/> is disposed when it is replaced by one of a different shape, and when the
    /// buffer is disposed.
    /// </remarks>
    sealed class MatBuffer : IDisposable
    {
        Mat mat;

        internal Mat Mat => mat;

        internal Mat Get(int rows, int cols, Depth depth, int channels)
        {
            if (mat != null)
            {
                if (mat.Rows == rows && mat.Cols == cols && mat.Depth == depth && mat.Channels == channels)
                    return mat;

                mat.Dispose();
                mat = null;
            }

            return mat = new Mat(rows, cols, depth, channels);
        }

        public void Dispose()
        {
            mat?.Dispose();
            mat = null;
        }
    }
}
