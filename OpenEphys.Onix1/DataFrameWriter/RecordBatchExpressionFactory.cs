using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Apache.Arrow;

namespace OpenEphys.Onix1.DataFrameWriter
{
    static class RecordBatchExpressionFactory
    {
        static readonly ConstructorInfo recordBatchConstructor = typeof(RecordBatch)
            .GetConstructor(
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new Type[] { typeof(Schema), typeof(IArrowArray[]), typeof(int) },
                null);

        static Expression InitializeArrowArrayFromSchema(ParameterExpression schema, Expression arrowArrays)
        {
            var fieldsList = Expression.Property(schema, nameof(Schema.FieldsList));
            var fieldsListAsCollection = Expression.Convert(
                fieldsList,
                typeof(IReadOnlyCollection<Field>)
            );

            return Expression.Assign(
                    arrowArrays,
                    Expression.NewArrayBounds(
                        typeof(IArrowArray),
                        Expression.Property(fieldsListAsCollection, nameof(Schema.FieldsList.Count))
                    )
                );
        }

        internal static Expression<TDelegate> CreateBuilder<TDelegate>(
            IRecordBatchExpressionProvider provider,
            Type frameType,
            IEnumerable<MemberInfo> members)
        {
            var expressions = new List<Expression>();
            var parameters = new List<ParameterExpression>();

            var arrowArrays = Expression.Variable(typeof(IArrowArray[]), "arrowArrays");
            var arrowArrayIndex = Expression.Variable(typeof(int), "arrowArrayIndex");
            var schemaParameter = Expression.Parameter(typeof(Schema), "schema");
            var batchRowsExpression = provider.GetLengthExpression();
            var batchRowsVariable = Expression.Variable(typeof(int), "batchRows");

            parameters.Add(batchRowsVariable);
            expressions.Add(Expression.Assign(
                batchRowsVariable,
                batchRowsExpression
            ));

            parameters.Add(arrowArrays);
            expressions.Add(InitializeArrowArrayFromSchema(schemaParameter, arrowArrays));

            parameters.Add(arrowArrayIndex);
            expressions.Add(Expression.Assign(arrowArrayIndex, Expression.Constant(0)));

            expressions.AddRange(
                provider.GetArrayPopulationExpressions(
                    arrowArrays, arrowArrayIndex, batchRowsVariable, frameType, members));

            var recordBatch = Expression.Variable(typeof(RecordBatch), "recordBatch");
            parameters.Add(recordBatch);
            expressions.Add(Expression.Assign(
                recordBatch,
                Expression.New(recordBatchConstructor, schemaParameter, arrowArrays, batchRowsVariable)));

            var createRecordBatch = Expression.Block(parameters, expressions);

            return Expression.Lambda<TDelegate>(
                createRecordBatch,
                provider.InputParameter,
                schemaParameter
            );
        }
    }
}
