using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Apache.Arrow;
using Apache.Arrow.Types;

namespace OpenEphys.Onix1.FrameWriter
{
    class DataFrameExpressionProvider : IRecordBatchExpressionProvider
    {
        public ParameterExpression InputParameter { get; }

        public DataFrameExpressionProvider()
        {
            InputParameter = Expression.Parameter(typeof(IList<DataFrame>), "frames");
        }

        public Expression GetLengthExpression()
        {
            return Expression.Property(
                    Expression.Convert(InputParameter, typeof(ICollection<DataFrame>)),
                    nameof(ICollection<DataFrame>.Count));
        }

        public List<Expression> GetArrayPopulationExpressions(
            ParameterExpression arrowArrays,
            ParameterExpression arrowArrayIndex,
            ParameterExpression batchRows,
            Type frameType,
            IEnumerable<MemberInfo> members)
        {
            var frameParameter = Expression.Parameter(typeof(DataFrame), "df");
            List<Expression> expressions = new();

            var stack = new Stack<MemberNode>(members.Select(m => new MemberNode { Member = m }));

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                var memberType = DataFrameWriterHelper.GetMemberType(current.Member);

                if (memberType.IsPrimitive)
                {
                    var memberAccessor = DataFrameWriterHelper.CreateMemberAccess(
                        Expression.Convert(frameParameter, frameType),
                        current);

                    expressions.Add(ConvertFrameMemberExpressionBuilder(
                        memberType, frameParameter, arrowArrays, arrowArrayIndex,
                        InputParameter, batchRows, memberAccessor));
                }
                else if (memberType.IsEnum)
                {
                    var memberAccessor = Expression.Convert(
                        DataFrameWriterHelper.CreateMemberAccess(
                            Expression.Convert(frameParameter, frameType),
                            current),
                        Enum.GetUnderlyingType(memberType));

                    expressions.Add(ConvertFrameMemberExpressionBuilder(
                        Enum.GetUnderlyingType(memberType), frameParameter,
                        arrowArrays, arrowArrayIndex, InputParameter,
                        batchRows, memberAccessor));
                }
                else if (memberType.IsValueType)
                {
                    var structMembers = DataFrameWriterHelper.GetDataMembers(memberType);

                    foreach (var structMember in structMembers.Reverse())
                    {
                        if (DataFrameWriterHelper.IsMemberIgnored(current.Member, structMember))
                            continue;

                        stack.Push(new MemberNode
                        {
                            Member = structMember,
                            Parent = current
                        });
                    }
                }
                else
                {
                    throw new NotSupportedException(
                        $"The member type '{memberType}' is not supported for generating RecordBatch builders.");
                }
            }

            return expressions;
        }

        static IArrowArray ConvertFrameMemberToArrowArray<TMember>(IList<DataFrame> frames, Func<DataFrame, TMember> getter, IArrowType arrowType, int length) where TMember : unmanaged
        {
            var array = new TMember[length];

            for (int i = 0; i < length; i++)
            {
                array[i] = getter(frames[i]);
            }

            return DataFrameWriterHelper.ConvertArrayToArrowArray(array, arrowType, length);
        }

        static Expression ConvertFrameMemberExpressionBuilder(
            Type memberType,
            ParameterExpression frameParameter,
            ParameterExpression arrowArrays,
            ParameterExpression arrowArrayIndex,
            ParameterExpression frames,
            ParameterExpression count,
            Expression memberAccessor)
        {
            var convertFrameMemberMethod = typeof(DataFrameExpressionProvider)
                        .GetMethod(nameof(ConvertFrameMemberToArrowArray), BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(memberType);

            var arrayArrowType = Expression.Constant(DataFrameWriterHelper.GetArrowType(memberType));
            var getter = Expression.Lambda(
                            typeof(Func<,>).MakeGenericType(typeof(DataFrame), memberType),
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
                        arrayArrowType,
                        count
                    )
                ),
                Expression.PostIncrementAssign(arrowArrayIndex)
            );

            return block;
        }
    }
}
