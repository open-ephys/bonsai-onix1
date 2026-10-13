using OpenCV.Net;
using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Reduces a multi-channel signal to the envelope of a fixed-width ring of columns, each holding the min
    /// and max of <c>factor</c> input samples. Reduction state carries across calls, so an input block that ends
    /// part-way through a column resumes that column on the next call.
    /// </summary>
    /// <remarks>
    /// Ported from <c>Bonsai.Ephys.Design.Decimator</c>, which is internal to that assembly and so not
    /// reachable through a package reference, and which reduces with one operation where this reduces with
    /// both.
    /// </remarks>
    internal sealed class Decimator : IDisposable
    {
        int carry;
        int writeIndex;
        int inputIndex;
        readonly Mat carryMin;
        readonly Mat carryMax;

        public Decimator(int rows, int length, int factor)
        {
            if (length < 1)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (factor < 1)
                throw new ArgumentOutOfRangeException(nameof(factor));

            writeIndex = 0;
            DownsampleFactor = factor;
            carry = DownsampleFactor;
            carryMin = new Mat(rows, 1, Depth.F32, 1);
            carryMax = new Mat(rows, 1, Depth.F32, 1);
            Sweep = new Envelope(rows, length);
        }

        public Envelope Sweep { get; }

        /// <summary>
        /// A copy of <see cref="Sweep"/> taken each time <see cref="Cursor"/> wraps, when every column
        /// holds the same sweep, or null before the first.
        /// </summary>
        public Envelope LastSweep { get; private set; }

        public int Cursor => writeIndex;

        /// <summary>
        /// Samples already reduced into the column <see cref="Cursor"/> names, which is how far past a
        /// column boundary the reduction has got.
        /// </summary>
        /// <remarks>
        /// Columns are counted from wherever the reduction began, and a caller that has to line its own
        /// binning up with this one cannot work that out from the blocks it has handed over: they need not
        /// divide into columns, so the boundaries fall wherever they fall.
        /// </remarks>
        public int Filled => DownsampleFactor - carry;

        public int DownsampleFactor { get; }

        /// <summary>
        /// Reduces a block of samples into <see cref="Sweep"/>, continuing from the column the previous call
        /// left off at.
        /// </summary>
        /// <remarks>
        /// A single call of exactly <c>Sweep.Cols * DownsampleFactor</c> samples after a <see cref="Reset"/>
        /// therefore fills every column once and leaves <see cref="Cursor"/> back at zero, which is how a
        /// whole window is reduced without a second implementation of the binning.
        /// </remarks>
        /// <param name="input">Channel-by-sample matrix of 32-bit floats to reduce.</param>
        public void Process(Mat input)
        {
            while (inputIndex < input.Cols)
            {
                var inputSamples = Math.Min(input.Cols - inputIndex, carry);
                var inputRect = new Rect(inputIndex, 0, inputSamples, input.Rows);

                using var inputBuffer = input.GetSubRect(inputRect);
                using var min = Sweep.Min.GetCol(writeIndex);
                using var max = Sweep.Max.GetCol(writeIndex);
                if (carry < DownsampleFactor)
                {
                    CV.Reduce(inputBuffer, carryMin, 1, ReduceOperation.Min);
                    CV.Min(min, carryMin, min);
                    CV.Reduce(inputBuffer, carryMax, 1, ReduceOperation.Max);
                    CV.Max(max, carryMax, max);
                }
                else
                {
                    CV.Reduce(inputBuffer, min, 1, ReduceOperation.Min);
                    CV.Reduce(inputBuffer, max, 1, ReduceOperation.Max);
                }

                inputIndex += inputRect.Width;
                carry -= inputSamples;
                if (carry <= 0)
                {
                    writeIndex = (writeIndex + 1) % Sweep.Cols;
                    carry = DownsampleFactor;
                    if (writeIndex == 0)
                    {
                        LastSweep ??= new Envelope(Sweep.Rows, Sweep.Cols);
                        Sweep.CopyTo(LastSweep);
                    }
                }

                writeIndex = writeIndex % Sweep.Cols;
            }

            inputIndex -= input.Cols;
        }

        /// <summary>
        /// Returns this instance to its state at construction: <see cref="Cursor"/> back at zero and <see
        /// cref="Sweep"/> empty, so that unfilled columns plot as a gap rather than as stale data.
        /// </summary>
        public void Reset()
        {
            writeIndex = 0;
            inputIndex = 0;
            carry = DownsampleFactor;
            Sweep.Clear();
        }

        public void Dispose()
        {
            Sweep.Dispose();
            LastSweep?.Dispose();
            carryMin.Dispose();
            carryMax.Dispose();
        }
    }
}
