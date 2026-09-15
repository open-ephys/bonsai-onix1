using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Provides a base class for expression builder that produces a <see
    /// cref="ProbeScopeVisualizer{TFrame}"/>.
    /// </summary>
    /// <typeparam name="TFrame">The data frame type the scope displays.</typeparam>
    public abstract class ProbeScopeBuilder<TFrame> : SingleArgumentExpressionBuilder
    {
        /// <inheritdoc/>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            var elementType = source.Type.GetGenericArguments()[0];
            if (elementType != typeof(TFrame))
            {
                throw new InvalidOperationException(
                    $"{GetType().Name} expects a sequence of {typeof(TFrame).Name} but was given {elementType.Name}.");
            }

            return Expression.Call(typeof(ProbeScopeBuilder<TFrame>), nameof(Process), null, source);
        }

        static IObservable<TFrame> Process(IObservable<TFrame> source) => source;
    }
}
