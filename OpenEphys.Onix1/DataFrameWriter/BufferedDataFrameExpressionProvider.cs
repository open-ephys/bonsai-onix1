using System;
using System.Collections.Generic;
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
            InputParameter = Expression.Parameter(typeof(IList<BufferedDataFrame>), "bufferedDataFrames");
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
            IEnumerable<MemberFieldGroup> fieldGroups)
        {
            var frameParameter = Expression.Parameter(typeof(BufferedDataFrame), "bufferedDataFrame");
            var expressions = new List<Expression>();

            foreach (var group in fieldGroups)
            {
                var memberType = DataFrameWriterHelper.GetMemberType(group.Member.Member);

                switch (memberType)
                {
                    case { IsArray: true }:
                        expressions.Add(BuildArrayExpression(group, frameType, frameParameter, arrowArrays, arrowArrayIndex));
                        break;

                    case var t when t == typeof(Mat):
                        if (group.Fields[0].DataType is RunEndEncodedType)
                            expressions.Add(BuildRunEndEncodedMatExpression(group, frameType, frameParameter, arrowArrays, arrowArrayIndex));
                        else
                            expressions.Add(BuildMatExpression(group, frameType, frameParameter, arrowArrays, arrowArrayIndex, batchRows));
                        break;

                    default:
                        throw new NotSupportedException(
                        $"The member type '{memberType}' is not supported for generating RecordBatch builders.");
                }
            }

            return expressions;
        }

        Expression BuildArrayExpression(
            MemberFieldGroup group,
            Type frameType,
            ParameterExpression frameParameter,
            ParameterExpression arrowArrays,
            ParameterExpression arrowArrayIndex)
        {
            var memberType = DataFrameWriterHelper.GetMemberType(group.Member.Member);
            var memberAccessor = DataFrameWriterHelper.CreateMemberAccess(
                Expression.Convert(frameParameter, frameType), group.Member);

            var convertFrameMemberMethod = typeof(BufferedDataFrameExpressionProvider)
                .GetMethod(nameof(ConvertFrameMemberToArrowArray), BindingFlags.Static | BindingFlags.NonPublic)
                .MakeGenericMethod(memberType.GetElementType());

            return ConvertFrameMemberExpressionBuilder(
                memberType, frameParameter, arrowArrays, arrowArrayIndex,
                InputParameter, memberAccessor, convertFrameMemberMethod);
        }

        Expression BuildMatExpression(
            MemberFieldGroup group,
            Type frameType,
            ParameterExpression frameParameter,
            ParameterExpression arrowArrays,
            ParameterExpression arrowArrayIndex,
            ParameterExpression batchRows)
        {
            var combineMatMethod = typeof(BufferedDataFrameExpressionProvider)
                .GetMethod(nameof(CombineMatObjects), BindingFlags.Static | BindingFlags.NonPublic);
            var convertMatRowMethod = typeof(BufferedDataFrameExpressionProvider)
                .GetMethod(nameof(ConvertMatRowToArrowArray), BindingFlags.Static | BindingFlags.NonPublic);
            var getArrowTypeMethod = typeof(DataFrameWriterHelper).GetMethod(
                nameof(DataFrameWriterHelper.GetArrowType),
                BindingFlags.Static | BindingFlags.NonPublic,
                null, new[] { typeof(Depth) }, null);

            var combinedMat = Expression.Variable(typeof(Mat), "combinedMat");
            var matElementType = Expression.Variable(typeof(IArrowType), "matElementType");
            var rowIndex = Expression.Variable(typeof(int), "rowIndex");
            var breakLabel = Expression.Label("break");

            var combineMatCall = Expression.Call(
                combineMatMethod,
                InputParameter,
                Expression.Lambda<Func<BufferedDataFrame, Mat>>(
                    DataFrameWriterHelper.CreateMemberAccess(
                        Expression.Convert(frameParameter, frameType), group.Member),
                    frameParameter));

            var loopBody = Expression.Block(
                Expression.Assign(
                    Expression.ArrayAccess(arrowArrays, arrowArrayIndex),
                    Expression.Call(convertMatRowMethod, combinedMat, rowIndex, matElementType, batchRows)),
                Expression.PostIncrementAssign(arrowArrayIndex));

            var forLoop = Expression.Block(
                new[] { matElementType, rowIndex },
                Expression.Assign(rowIndex, Expression.Constant(0)),
                Expression.Assign(
                    matElementType,
                    Expression.Call(getArrowTypeMethod, Expression.Property(combinedMat, nameof(Mat.Depth)))),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(rowIndex, Expression.Property(combinedMat, nameof(Mat.Rows))),
                        Expression.Block(loopBody, Expression.PostIncrementAssign(rowIndex)),
                        Expression.Break(breakLabel)),
                    breakLabel));

            return Expression.Block(
                new[] { combinedMat },
                Expression.Assign(combinedMat, combineMatCall),
                forLoop);
        }

        Expression BuildRunEndEncodedMatExpression(
            MemberFieldGroup group,
            Type frameType,
            ParameterExpression frameParameter,
            ParameterExpression arrowArrays,
            ParameterExpression arrowArrayIndex)
        {
            var combineMatMethod = typeof(BufferedDataFrameExpressionProvider)
                .GetMethod(nameof(CombineMatObjects), BindingFlags.Static | BindingFlags.NonPublic);
            var convertMatRowMethod = typeof(BufferedDataFrameExpressionProvider)
                .GetMethod(nameof(ConvertMatRowToRunEndEncodedArrowArray), BindingFlags.Static | BindingFlags.NonPublic);
            var getArrowTypeMethod = typeof(DataFrameWriterHelper).GetMethod(
                nameof(DataFrameWriterHelper.GetArrowType),
                BindingFlags.Static | BindingFlags.NonPublic,
                null, new[] { typeof(Depth) }, null);

            var attr = group.Member.Member.GetCustomAttribute<SubsampledAttribute>();
            var stride = Expression.Constant(attr.Divisor);

            var combinedMat = Expression.Variable(typeof(Mat), "combinedMat");
            var matElementType = Expression.Variable(typeof(IArrowType), "matElementType");
            var rowIndex = Expression.Variable(typeof(int), "rowIndex");
            var runEndsArray = Expression.Variable(typeof(IArrowArray), "runEndArray");
            var breakLabel = Expression.Label("break");

            var combineMatCall = Expression.Call(
                combineMatMethod,
                InputParameter,
                Expression.Lambda<Func<BufferedDataFrame, Mat>>(
                    DataFrameWriterHelper.CreateMemberAccess(
                        Expression.Convert(frameParameter, frameType), group.Member),
                    frameParameter));

            var runEndsArrayCall = Expression.Call(
                typeof(BufferedDataFrameExpressionProvider)
                    .GetMethod(nameof(CreateRunEnds), BindingFlags.Static | BindingFlags.NonPublic),
                Expression.Property(combinedMat, nameof(Mat.Cols)),
                stride);

            var loopBody = Expression.Block(
                Expression.Assign(
                    Expression.ArrayAccess(arrowArrays, arrowArrayIndex),
                    Expression.Call(convertMatRowMethod, combinedMat, rowIndex, matElementType, runEndsArray)),
                Expression.PostIncrementAssign(arrowArrayIndex));

            var forLoop = Expression.Block(
                new[] { matElementType, rowIndex, runEndsArray },
                Expression.Assign(runEndsArray, runEndsArrayCall),
                Expression.Assign(rowIndex, Expression.Constant(0)),
                Expression.Assign(
                    matElementType,
                    Expression.Call(getArrowTypeMethod, Expression.Property(combinedMat, nameof(Mat.Depth)))),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(rowIndex, Expression.Property(combinedMat, nameof(Mat.Rows))),
                        Expression.Block(loopBody, Expression.PostIncrementAssign(rowIndex)),
                        Expression.Break(breakLabel)),
                    breakLabel));

            return Expression.Block(
                new[] { combinedMat },
                Expression.Assign(combinedMat, combineMatCall),
                forLoop);
        }

        static int GetTotalSamples(IList<BufferedDataFrame> frames)
        {
            if (frames.Count == 0)
                return 0;
            int samplesPerFrame = frames[0].Clock.Length;
            return frames.Count * samplesPerFrame;
        }

        static IArrowArray ConvertFrameMemberToArrowArray<TMember>(
            IList<BufferedDataFrame> frames, 
            Func<BufferedDataFrame, TMember[]> getter, 
            IArrowType arrowType) where TMember : unmanaged
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

            return Expression.Block(
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
        }

        static IArrowArray CreateRunEnds(int dataLength, int runLength)
        {
            var runEndsBuilder = new Int32Array.Builder().Reserve(dataLength);

            for (int i = 0; i < dataLength; i++)
            {
                runEndsBuilder.Append((i + 1) * runLength);
            }

            return runEndsBuilder.Build();
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

        static IArrowArray ConvertMatRowToArrowArray(Mat mat, int rowIndex, IArrowType elementType, int batchRows)
        {
            if (batchRows != mat.Cols)
                throw new InvalidOperationException($"The number of batch rows ({batchRows}) is not equal to the number of samples in the Mat ({mat.Cols}).");

            var rowManager = new MatRowMemoryManager(mat, rowIndex);

            var arrayData = new ArrayData(
                elementType,
                batchRows,
                0,
                0,
                new[] { ArrowBuffer.Empty, new ArrowBuffer(rowManager.Memory) },
                null,
                null
            );

            return ArrowArrayFactory.BuildArray(arrayData);
        }

        static IArrowArray ConvertMatRowToRunEndEncodedArrowArray(Mat mat, int rowIndex, IArrowType elementType, IArrowArray runEnds)
        {
            int length = mat.Cols;

            if (runEnds.Length != length)
                throw new InvalidOperationException($"The number of run ends ({runEnds.Length}) is smaller than the number of samples in the Mat ({length}).");

            var rowManager = new MatRowMemoryManager(mat, rowIndex);
            var values = new ArrayData(
                elementType,
                length,
                0,
                0,
                new[] { ArrowBuffer.Empty, new ArrowBuffer(rowManager.Memory) },
                null,
                null
            );

            return new RunEndEncodedArray(runEnds, ArrowArrayFactory.BuildArray(values));
        }
    }
}
