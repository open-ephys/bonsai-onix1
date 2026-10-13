using System;
using OpenCV.Net;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// The minimum and maximum of a multi-channel signal over each of a run of columns, one row per channel.
    /// </summary>
    /// <remarks>
    /// A column with nothing reduced into it holds NaN in both, which draws as a gap.
    /// </remarks>
    internal sealed class Envelope : IDisposable
    {
        public Envelope(int rows, int columns)
            : this(new Mat(rows, columns, Depth.F32, 1), new Mat(rows, columns, Depth.F32, 1))
        {
            Clear();
        }

        Envelope(Mat min, Mat max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Per-column minima, one row per channel.
        /// </summary>
        public Mat Min { get; }

        /// <summary>
        /// Per-column maxima, one row per channel.
        /// </summary>
        public Mat Max { get; }

        /// <summary>
        /// Number of channels.
        /// </summary>
        public int Rows => Min.Rows;

        /// <summary>
        /// Number of columns.
        /// </summary>
        public int Cols => Min.Cols;

        /// <summary>
        /// Empties every column.
        /// </summary>
        public void Clear()
        {
            Min.Set(Scalar.All(double.NaN));
            Max.Set(Scalar.All(double.NaN));
        }

        /// <summary>
        /// Copies this envelope into <paramref name="destination"/>, which has the same shape.
        /// </summary>
        public void CopyTo(Envelope destination)
        {
            CV.Copy(Min, destination.Min);
            CV.Copy(Max, destination.Max);
        }

        public Envelope Clone() => new(Min.Clone(), Max.Clone());

        public void Dispose()
        {
            Min.Dispose();
            Max.Dispose();
        }
    }
}
