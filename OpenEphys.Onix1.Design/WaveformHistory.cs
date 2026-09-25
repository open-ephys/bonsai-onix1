using OpenCV.Net;
using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Retains the most recent samples of a multi-channel signal in a fixed ring of columns, so that a
    /// window of them can be read back and decimated at any bin width.
    /// </summary>
    /// <remarks>
    /// Samples are addressed on an absolute timeline that starts at zero and never wraps, so a reader can
    /// hold a position across writes; <see cref="Oldest"/> and <see cref="Count"/> bound what is still
    /// held. The ring itself is never copied whole: a write or a read that straddles its end is two
    /// copies of adjoining column ranges.
    /// </remarks>
    internal sealed class WaveformHistory : IDisposable
    {
        readonly Mat buffer;
        long floor;

        /// <param name="rows">Number of channels.</param>
        /// <param name="capacity">Number of samples per channel to retain.</param>
        public WaveformHistory(int rows, int capacity)
        {
            if (rows < 1)
                throw new ArgumentOutOfRangeException(nameof(rows));

            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            Capacity = capacity;
            buffer = new Mat(rows, capacity, Depth.F32, 1);
            buffer.Set(Scalar.All(double.NaN));
        }

        /// <summary>
        /// Number of channels.
        /// </summary>
        public int Rows => buffer.Rows;

        /// <summary>
        /// Number of samples per channel retained.
        /// </summary>
        public int Capacity { get; }

        /// <summary>
        /// Total samples written; one past the newest sample on the absolute timeline.
        /// </summary>
        public long Count { get; private set; }

        /// <summary>
        /// Position of the oldest sample still held.
        /// </summary>
        public long Oldest => Math.Max(floor, Count - Capacity);

        /// <summary>
        /// Abandons everything held, without moving the timeline, so that samples written before a break
        /// in the recording are never read back as though they adjoined those written after it.
        /// </summary>
        public void Clear()
        {
            floor = Count;
        }

        /// <summary>
        /// Appends a block of samples, overwriting the oldest once the ring is full.
        /// </summary>
        /// <param name="block">
        /// Channel-by-sample matrix of the same depth and channel count as the ring.
        /// </param>
        public void Write(Mat block)
        {
            // NB: a block longer than the ring keeps only its tail, but the timeline still advances by
            // the whole block so that positions stay comparable across writes.
            var skipped = Math.Max(0, block.Cols - Capacity);
            var samples = block.Cols - skipped;
            var head = (int)((Count + skipped) % Capacity);
            var run = Math.Min(samples, Capacity - head);

            Copy(block, skipped, buffer, head, run);
            if (run < samples)
                Copy(block, skipped + run, buffer, 0, samples - run);

            Count += block.Cols;
        }

        /// <summary>
        /// Copies the samples at <paramref name="first"/> on the absolute timeline into
        /// <paramref name="destination"/>, which must be as wide as the window requested.
        /// </summary>
        /// <returns>
        /// False, having copied nothing, if any part of the window is no longer held.
        /// </returns>
        public bool CopyWindow(long first, Mat destination)
        {
            var samples = destination.Cols;
            if (first < Oldest || first + samples > Count)
                return false;

            var start = (int)(first % Capacity);
            var run = Math.Min(samples, Capacity - start);

            Copy(buffer, start, destination, 0, run);
            if (run < samples)
                Copy(buffer, 0, destination, run, samples - run);

            return true;
        }

        static void Copy(Mat source, int sourceColumn, Mat destination, int destinationColumn, int columns)
        {
            using var src = source.GetSubRect(new Rect(sourceColumn, 0, columns, source.Rows));
            using var dst = destination.GetSubRect(new Rect(destinationColumn, 0, columns, destination.Rows));
            CV.Copy(src, dst);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            buffer.Dispose();
        }
    }
}
