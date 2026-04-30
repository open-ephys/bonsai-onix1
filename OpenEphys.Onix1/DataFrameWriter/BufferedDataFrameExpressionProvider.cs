using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Apache.Arrow;
using Apache.Arrow.Types;
using OpenCV.Net;

namespace OpenEphys.Onix1.DataFrameWriter
{
    class BufferedDataFrameExpressionProvider : IRecordBatchExpressionProvider
    {
        public ParameterExpression InputParameter { get; }

        public BufferedDataFrameExpressionProvider()
        {
            InputParameter = Expression.Parameter(typeof(IList<BufferedDataFrame>), "frame");
        }

        public Expression GetLengthExpression()
        {
            // NB: We assume that all frames in the batch have the same number of rows, so we can take the length of the first frame's clock array and multiply by the number of frames.
            var firstFrame = Expression.Property(
                InputParameter,
                typeof(IList<BufferedDataFrame>).GetProperty("Item"),
                Expression.Constant(0));
            var clockArray = Expression.Property(firstFrame, nameof(BufferedDataFrame.Clock));
            var clockArrayLength = Expression.ArrayLength(clockArray);
            var numberOfFrames = Expression.Property(
                    Expression.Convert(InputParameter, typeof(ICollection<BufferedDataFrame>)),
                    nameof(ICollection<BufferedDataFrame>.Count));
            return Expression.Multiply(clockArrayLength, numberOfFrames);
        }

        public List<Expression> GetArrayPopulationExpressions(
            ParameterExpression arrowArrays,
            ParameterExpression arrowArrayIndex,
            ParameterExpression batchRows,
            Type frameType,
            IEnumerable<MemberInfo> members)
        {
            var frameParameter = Expression.Parameter(typeof(BufferedDataFrame), "df");
            List<Expression> expressions = new();

            var stack = new Stack<MemberNode>(members.Select(m => new MemberNode { Member = m }));

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                var memberType = DataFrameWriterHelper.GetMemberType(current.Member);

                if (memberType.IsArray)
                {
                    var memberAccessor = DataFrameWriterHelper.CreateMemberAccess(
                        Expression.Convert(frameParameter, frameType),
                        current);

                    var convertFrameMemberMethod = typeof(BufferedDataFrameExpressionProvider)
                                .GetMethod(nameof(ConvertFrameMemberToArrowArray), BindingFlags.Static | BindingFlags.NonPublic)
                                .MakeGenericMethod(memberType.GetElementType());

                    expressions.Add(ConvertFrameMemberExpressionBuilder(
                        memberType, frameParameter, arrowArrays, arrowArrayIndex,
                        InputParameter, memberAccessor,
                        convertFrameMemberMethod));
                }
                else if (memberType == typeof(Mat))
                {
                    var combineMatMethod = typeof(BufferedDataFrameExpressionProvider)
                        .GetMethod(nameof(CombineMatObjects), BindingFlags.Static | BindingFlags.NonPublic);

                    var combinedMat = Expression.Variable(typeof(Mat), "combinedMat");
                    var combineMatCall = Expression.Call(
                        combineMatMethod,
                        InputParameter,
                        Expression.Lambda<Func<BufferedDataFrame, Mat>>(
                            DataFrameWriterHelper.CreateMemberAccess(
                                Expression.Convert(frameParameter, frameType),
                                current),
                            frameParameter));

                    var convertMatRowMethod = typeof(BufferedDataFrameExpressionProvider)
                        .GetMethod(nameof(ConvertMatRowToArrowArray), BindingFlags.Static | BindingFlags.NonPublic);

                    var matElementType = Expression.Variable(typeof(IArrowType), "matElementType");
                    var rowIndex = Expression.Variable(typeof(int), "rowIndex");
                    var breakLabel = Expression.Label("break");

                    var loopBody = Expression.Block(
                        Expression.Assign(
                            Expression.ArrayAccess(arrowArrays, arrowArrayIndex),
                            Expression.Call(
                                convertMatRowMethod,
                                combinedMat,
                                rowIndex,
                                matElementType,
                                batchRows)),
                        Expression.PostIncrementAssign(arrowArrayIndex));

                    var forLoop = Expression.Block(
                        new[] { matElementType, rowIndex },
                        Expression.Assign(rowIndex, Expression.Constant(0)),
                        Expression.Assign(
                            matElementType,
                            Expression.Call(
                                typeof(DataFrameWriterHelper).GetMethod(
                                    nameof(DataFrameWriterHelper.GetArrowType),
                                    BindingFlags.Static | BindingFlags.NonPublic,
                                    null,
                                    new[] { typeof(Depth) },
                                    null),
                                Expression.Property(combinedMat, nameof(Mat.Depth)))),
                        Expression.Loop(
                            Expression.IfThenElse(
                                Expression.LessThan(
                                    rowIndex,
                                    Expression.Property(combinedMat, nameof(Mat.Rows))),
                                Expression.Block(
                                    loopBody,
                                    Expression.PostIncrementAssign(rowIndex)),
                                Expression.Break(breakLabel)),
                            breakLabel));

                    expressions.Add(Expression.Block(
                        new[] { combinedMat },
                        Expression.Assign(combinedMat, combineMatCall),
                        forLoop
                    ));
                }
                else
                {
                    throw new NotSupportedException(
                        $"The member type '{memberType}' is not supported for generating RecordBatch builders.");
                }
            }

            return expressions;
        }

        static int GetTotalSamples(IList<BufferedDataFrame> frames)
        {
            if (frames.Count == 0)
                return 0;
            int samplesPerFrame = frames[0].Clock.Length;
            return frames.Count * samplesPerFrame;
        }

        static IArrowArray ConvertFrameMemberToArrowArray<TMember>(IList<BufferedDataFrame> frames, Func<BufferedDataFrame, TMember[]> getter, IArrowType arrowType) where TMember : unmanaged
        {
            int length = GetTotalSamples(frames);

            if (length == 0)
                return ArrowArrayFactory.BuildArray(new ArrayData(arrowType, 0, 0, 0, new[] { ArrowBuffer.Empty, ArrowBuffer.Empty }, null, null));

            int samplesPerFrame = frames[0].Clock.Length;
            var array = new TMember[length];

            for (int i = 0; i < frames.Count; i++)
            {
                var member = getter(frames[i]);

                for (int j = 0; j < samplesPerFrame; j++)
                {
                    array[i * samplesPerFrame + j] = member[j];
                }
            }

            return DataFrameWriterHelper.ConvertArrayToArrowArray(array, arrowType, length);
        }

        static Expression ConvertFrameMemberExpressionBuilder(
            Type memberType,
            ParameterExpression frameParameter,
            ParameterExpression arrowArrays,
            ParameterExpression arrowArrayIndex,
            ParameterExpression frames,
            Expression memberAccessor,
            MethodInfo convertFrameMemberMethod)
        {
            var arrayArrowType = Expression.Constant(DataFrameWriterHelper.GetArrowType(memberType.GetElementType()));
            var getter = Expression.Lambda(
                            typeof(Func<,>).MakeGenericType(typeof(BufferedDataFrame), memberType),
                            memberAccessor,
                            frameParameter
                        );

            var block = Expression.Block(
                Expression.Assign(
                    Expression.ArrayAccess(arrowArrays, arrowArrayIndex),
                    Expression.Call(
                        convertFrameMemberMethod,
                        frames,
                        getter,
                        arrayArrowType
                    )
                ),
                Expression.PostIncrementAssign(arrowArrayIndex)
            );

            return block;
        }

        static ArrowBuffer CreateStrideValidityBitmap(int validCount, int totalCount, int stride)
        {
            int byteCount = (totalCount + 7) / 8;
            byte[] bitmap = new byte[byteCount];

            for (int i = 0; i < validCount; i++)
            {
                int bitIndex = i * stride;
                if (bitIndex >= totalCount) break;

                bitmap[bitIndex >> 3] |= (byte)(1 << (bitIndex & 7));
            }

            return new ArrowBuffer(bitmap);
        }

        static unsafe void StrideCopy<T>(void* src, void* dest, int elementCount, int stride) where T : unmanaged
        {
            T* srcPtr = (T*)src;
            T* destPtr = (T*)dest;

            for (int i = 0; i < elementCount; i++)
            {
                *destPtr = *srcPtr++;
                destPtr += stride;
            }
        }

        static unsafe void StrideCopyFromDepth(Depth depth, void* src, void* dest, int elementCount, int stride)
        {
            switch (depth)
            {
                case Depth.U8: StrideCopy<byte>(src, dest, elementCount, stride); break;
                case Depth.S8: StrideCopy<sbyte>(src, dest, elementCount, stride); break;
                case Depth.U16: StrideCopy<ushort>(src, dest, elementCount, stride); break;
                case Depth.S16: StrideCopy<short>(src, dest, elementCount, stride); break;
                case Depth.S32: StrideCopy<int>(src, dest, elementCount, stride); break;
                case Depth.F32: StrideCopy<float>(src, dest, elementCount, stride); break;
                case Depth.F64: StrideCopy<double>(src, dest, elementCount, stride); break;
                default: throw new NotSupportedException($"Cannot StrideCopy for depth '{depth}'.");
            }
        }

        static Mat CombineMatObjects(IList<BufferedDataFrame> frames, Func<BufferedDataFrame, Mat> getter)
        {
            Mat first = getter(frames[0]);
            int length = first.Cols * frames.Count;

            Mat dest = new(first.Rows, length, first.Depth, first.Channels);

            int samplesPerFrame = first.Cols;
            int rows = first.Rows;
            int offset = 0;

            for (int i = 0; i < frames.Count; i++)
            {
                Mat src = getter(frames[i]);

                using var destRect = dest.GetSubRect(new Rect(offset, 0, samplesPerFrame, rows));
                CV.Copy(src, destRect);
                offset += samplesPerFrame;
            }

            return dest;
        }

        static unsafe IArrowArray ConvertMatRowToArrowArray(Mat mat, int rowIndex, IArrowType elementType, int batchRows)
        {
            int length = mat.Cols;

            if (batchRows < length)
                throw new InvalidOperationException($"The number of batch rows ({batchRows}) is smaller than the number of samples in the Mat ({length}).");

            if (batchRows % length != 0)
                throw new InvalidOperationException($"The number of batch rows ({batchRows}) must be a multiple of the number of samples in the Mat ({length}).");

            int stride = batchRows / length;

            var rowManager = new MatRowMemoryManager(mat, rowIndex);
            int nullCount = batchRows - length;
            ArrowBuffer arrowBuffer;
            ArrowBuffer nullBitmap;

            if (nullCount == 0)
            {
                arrowBuffer = new ArrowBuffer(rowManager.Memory);
                nullBitmap = ArrowBuffer.Empty;
            }
            else
            {
                byte[] buffer = new byte[batchRows * mat.ElementSize];

                fixed (byte* bufferPtr = buffer)
                {
                    using var handle = rowManager.Memory.Pin();
                    StrideCopyFromDepth(mat.Depth, handle.Pointer, bufferPtr, length, stride);
                }

                arrowBuffer = new ArrowBuffer(buffer);
                nullBitmap = CreateStrideValidityBitmap(length, batchRows, stride);
            }

            var arrayData = new ArrayData(
                elementType,
                batchRows,
                nullCount,
                0,
                new[] { nullBitmap, arrowBuffer },
                null,
                null
            );

            return ArrowArrayFactory.BuildArray(arrayData);
        }
    }
}
