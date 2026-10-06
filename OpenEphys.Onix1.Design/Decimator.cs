using OpenCV.Net;
using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Reduces a multi-channel signal to a fixed-width ring buffer by combining every <c>factor</c> input
    /// samples into one output sample with <see cref="ReduceOperation"/>. Reduction state carries across
    /// calls, so an input block that ends part-way through an output bin resumes that bin on the next call.
    /// </summary>
    /// <remarks>
    /// Ported from <c>Bonsai.Ephys.Design.Decimator</c>, which is internal to that assembly and so not
    /// reachable through a package reference.
    /// </remarks>
    internal sealed class Decimator : IDisposable
    {
        int carry;
        int writeIndex;
        int inputIndex;
        readonly Mat carryBuffer;
        readonly ReduceOperation reduceOp;

        public Decimator(int rows, int length, int factor, ReduceOperation reduceOperation)
        {
            if (length < 1)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (factor < 1)
                throw new ArgumentOutOfRangeException(nameof(factor));

            writeIndex = 0;
            DownsampleFactor = factor;
            carry = DownsampleFactor;
            carryBuffer = new Mat(rows, 1, Depth.F32, 1);
            Sweep = new Mat(rows, length, Depth.F32, 1);
            Sweep.Set(Scalar.All(double.NaN));
            reduceOp = reduceOperation;
        }

        public Mat Sweep { get; }

        /// <summary>
        /// A copy of <see cref="Sweep"/> taken each time <see cref="Cursor"/> wraps, when every column
        /// holds the same sweep, or null before the first.
        /// </summary>
        public Mat LastSweep { get; private set; }

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
                using var outputBuffer = Sweep.GetCol(writeIndex);
                if (carry < DownsampleFactor)
                {
                    CV.Reduce(inputBuffer, carryBuffer, 1, reduceOp);
                    switch (reduceOp)
                    {
                        case ReduceOperation.Sum:
                            CV.Add(outputBuffer, carryBuffer, outputBuffer);
                            break;
                        case ReduceOperation.Max:
                            CV.Max(outputBuffer, carryBuffer, outputBuffer);
                            break;
                        case ReduceOperation.Min:
                            CV.Min(outputBuffer, carryBuffer, outputBuffer);
                            break;
                    }
                }
                else CV.Reduce(inputBuffer, outputBuffer, 1, reduceOp);

                inputIndex += inputRect.Width;
                carry -= inputSamples;
                if (carry <= 0)
                {
                    writeIndex = (writeIndex + 1) % Sweep.Cols;
                    carry = DownsampleFactor;
                    if (writeIndex == 0)
                    {
                        LastSweep ??= new Mat(Sweep.Rows, Sweep.Cols, Depth.F32, 1);
                        CV.Copy(Sweep, LastSweep);
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
            Sweep.Set(Scalar.All(double.NaN));
        }

        public void Dispose()
        {
            Sweep.Dispose();
            LastSweep?.Dispose();
            carryBuffer.Dispose();
        }
    }
}
