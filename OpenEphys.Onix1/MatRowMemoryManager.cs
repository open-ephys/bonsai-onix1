using OpenCV.Net;
using System;
using System.Buffers;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Provides a MemoryManager implementation for accessing a specific row of a Mat object as a span or memory
    /// block.
    /// </summary>
    public sealed class MatRowMemoryManager : MemoryManager<byte>
    {
        readonly Mat mat;
        readonly int row;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatRowMemoryManager"/> class.
        /// </summary>
        /// <param name="mat">Existing <see cref="Mat"/> object.</param>
        /// <param name="row">Row of the <see cref="Mat"/> object to access.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public MatRowMemoryManager(Mat mat, int row)
        {
            if (row >= mat.Rows)
                throw new ArgumentOutOfRangeException(nameof(row));

            this.mat = mat;
            this.row = row;
        }

        /// <inheritdoc/>
        public unsafe override Span<byte> GetSpan()
        {
            return new((byte*)mat.Data.ToPointer() + row * mat.Step, mat.Step);
        }

        /// <inheritdoc/>
        public unsafe override MemoryHandle Pin(int elementIndex = 0)
        {
            if (elementIndex < 0 || elementIndex >= mat.Step)
                throw new ArgumentOutOfRangeException(nameof(elementIndex));

            // NB: Return a MemoryHandle to the location of the row of data
            byte* ptr = (byte*)mat.Data.ToPointer();
            ptr += row * mat.Step + elementIndex;
            return new MemoryHandle(ptr);
        }

        /// <inheritdoc/>
        public override void Unpin()
        {
            // NB: Do nothing, as we are not managing the Mat object memory in this class
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            // NB: Do nothing, as we are not managing the Mat object memory in this class
        }
    }
}
