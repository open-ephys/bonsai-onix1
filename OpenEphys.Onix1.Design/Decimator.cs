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
        readonly Mat conversionBuffer;
        readonly ReduceOperation reduceOp;

        public Decimator(Mat input, int length, int factor, ReduceOperation reduceOperation)
        {
            if (length < 1)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (factor < 1)
                throw new ArgumentOutOfRangeException(nameof(factor));

            writeIndex = 0;
            DownsampleFactor = factor;
            InputDepth = input.Depth;
            carry = DownsampleFactor;
            carryBuffer = new Mat(input.Rows, 1, Depth.F32, input.Channels);
            Buffer = new Mat(input.Rows, length, Depth.F32, input.Channels);
            Buffer.Set(Scalar.All(double.NaN));
            reduceOp = reduceOperation;
            if (InputDepth != Depth.F32)
                conversionBuffer = new Mat(input.Size, Depth.F32, input.Channels);
        }

        public Mat Buffer { get; }

        public int Cursor => writeIndex;

        public int DownsampleFactor { get; }

        public Depth InputDepth { get; }

        /// <summary>
        /// Reduces a block of samples into <see cref="Buffer"/>, continuing from the column the previous call
        /// left off at.
        /// </summary>
        /// <remarks>
        /// A single call of exactly <c>Buffer.Cols * DownsampleFactor</c> samples after a <see cref="Reset"/>
        /// therefore fills every column once and leaves <see cref="Cursor"/> back at zero, which is how a
        /// whole window is reduced without a second implementation of the binning.
        /// </remarks>
        /// <param name="input">Channel-by-sample matrix to reduce.</param>
        public void Process(Mat input)
        {
            if (conversionBuffer is not null)
            {
                CV.ConvertScale(input, conversionBuffer);
                input = conversionBuffer;
            }

            while (inputIndex < input.Cols)
            {
                var inputSamples = Math.Min(input.Cols - inputIndex, carry);
                var inputRect = new Rect(inputIndex, 0, inputSamples, input.Rows);

                using var inputBuffer = input.GetSubRect(inputRect);
                using var outputBuffer = Buffer.GetCol(writeIndex);
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
                    writeIndex = (writeIndex + 1) % Buffer.Cols;
                    carry = DownsampleFactor;
                }

                writeIndex = writeIndex % Buffer.Cols;
            }

            inputIndex -= input.Cols;
        }

        /// <summary>
        /// Returns this instance to its state at construction: <see cref="Cursor"/> back at zero and <see
        /// cref="Buffer"/> empty, so that unfilled columns plot as a gap rather than as stale data.
        /// </summary>
        public void Reset()
        {
            writeIndex = 0;
            inputIndex = 0;
            carry = DownsampleFactor;
            Buffer.Set(Scalar.All(double.NaN));
        }

        public void Dispose()
        {
            Buffer.Dispose();
            carryBuffer.Dispose();
            conversionBuffer?.Dispose();
        }
    }
}
