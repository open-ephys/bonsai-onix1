using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace OpenEphys.Onix1.DataFrameWriter
{
    interface IRecordBatchExpressionProvider
    {
        ParameterExpression InputParameter { get; }

        Expression GetLengthExpression();

        List<Expression> GetArrayPopulationExpressions(
            ParameterExpression arrowArrays,
            ParameterExpression arrowArrayIndex,
            ParameterExpression batchRows,
            Type frameType,
            IEnumerable<MemberFieldGroup> fieldGroups);
    }
}
