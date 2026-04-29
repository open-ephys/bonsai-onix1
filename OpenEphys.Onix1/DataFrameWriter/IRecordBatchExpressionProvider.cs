using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenEphys.Onix1.FrameWriter
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
            IEnumerable<MemberInfo> members);
    }
}
