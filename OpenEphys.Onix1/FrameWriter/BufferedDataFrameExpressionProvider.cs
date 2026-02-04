using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Apache.Arrow;
using Apache.Arrow.Types;
using OpenCV.Net;

namespace OpenEphys.Onix1.FrameWriter
{
    class BufferedDataFrameExpressionProvider : IRecordBatchExpressionProvider
    {
        public ParameterExpression InputParameter { get; }

        public BufferedDataFrameExpressionProvider()
        {
            InputParameter = Expression.Parameter(typeof(BufferedDataFrame), "frame");
        }

        public Expression GetLengthExpression()
        {
            return Expression.ArrayLength(
                    Expression.Property(
                        InputParameter,
                        nameof(BufferedDataFrame.Clock)));
        }

        public List<Expression> GetArrayPopulationExpressions(
            ParameterExpression arrowArrays,
            ParameterExpression arrowArrayIndex,
            Expression batchRows,
            Type frameType,
            IEnumerable<MemberInfo> members)
        {
            List<Expression> expressions = new();

            var stack = new Stack<MemberNode>(members.Select(m => new MemberNode { Member = m }));

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                var memberType = FrameWriterHelper.GetMemberType(current.Member);

                if (memberType.IsArray)
                {
                    var convertMethod = typeof(FrameWriterHelper)
                        .GetMethod(nameof(FrameWriterHelper.ConvertArrayToArrowArray), BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(memberType.GetElementType());

                    var arrayProperty = Expression.Property(
                        Expression.Convert(InputParameter, frameType),
                        current.GetFullName());
                    var arrayArrowType = Expression.Constant(FrameWriterHelper.GetArrowType(memberType.GetElementType()));

                    var block = Expression.Block(
                        Expression.Assign(
                            Expression.ArrayAccess(arrowArrays, arrowArrayIndex),
                            Expression.Call(
                                convertMethod,
                                arrayProperty,
                                arrayArrowType,
                                batchRows)),
                        Expression.PostIncrementAssign(arrowArrayIndex));

                    expressions.Add(block);
                }
                else if (memberType == typeof(Mat))
                {
                    var convertMatRowMethod = typeof(BufferedDataFrameExpressionProvider)
                        .GetMethod(nameof(ConvertMatRowToArrowArray), BindingFlags.Static | BindingFlags.NonPublic);

                    var matProperty = Expression.Property(
                        Expression.Convert(InputParameter, frameType), current.GetFullName());
                    var matElementType = Expression.Variable(typeof(IArrowType), "matElementType");
                    var rowIndex = Expression.Variable(typeof(int), "rowIndex");
                    var breakLabel = Expression.Label("break");

                    var loopBody = Expression.Block(
                        Expression.Assign(
                            Expression.ArrayAccess(arrowArrays, arrowArrayIndex),
                            Expression.Call(
                                convertMatRowMethod,
                                matProperty,
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
                                typeof(FrameWriterHelper).GetMethod(
                                    nameof(FrameWriterHelper.GetArrowType),
                                    BindingFlags.Static | BindingFlags.NonPublic,
                                    null,
                                    new[] { typeof(Depth) },
                                    null),
                                Expression.Property(matProperty, nameof(Mat.Depth)))),
                        Expression.Loop(
                            Expression.IfThenElse(
                                Expression.LessThan(
                                    rowIndex,
                                    Expression.Property(matProperty, nameof(Mat.Rows))),
                                Expression.Block(
                                    loopBody,
                                    Expression.PostIncrementAssign(rowIndex)),
                                Expression.Break(breakLabel)),
                            breakLabel));

                    expressions.Add(forLoop);
                }
                else
                {
                    throw new NotSupportedException(
                        $"The member type '{memberType}' is not supported for generating RecordBatch builders.");
                }
            }

            return expressions;
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
                    StrideCopy<ushort>(rowManager.Memory.Pin().Pointer, bufferPtr, length, stride);
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
