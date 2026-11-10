using System.Collections.Generic;
using Bonsai.Dag;
using Bonsai.Expressions;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Represents Bonsai's rules for which of a workflow's nodes are in its scope, where a name such as a subject or
    /// a device is visible.
    /// </summary>
    /// <remarks>
    /// Scopes nest, and there is no single global one:
    /// <list type="bullet">
    /// <item><description>A name declared in the top-level workflow is visible everywhere below
    /// it.</description></item>
    /// <item><description>A nested workflow other than a Group or Include, such as the body of a SelectMany,
    /// starts a scope of its own. Its names are visible within it and the workflows nested in it, but not in its
    /// parent or siblings.</description></item>
    /// <item><description>A Group or Include starts none: its nodes belong to the scope of the workflow that
    /// holds it.</description></item>
    /// </list>
    /// So a name is resolved by looking in the scope it is used from, and then in each enclosing one.
    /// </remarks>
    static class WorkflowScope
    {
        /// <summary>
        /// Every node in the scope of <paramref name="workflow"/>, with the graph each one is in.
        /// </summary>
        /// <remarks>
        /// A Group or Include workflow is part of the workflow that holds it, so its nodes are in that scope too.
        /// Any other nested workflow has a scope of its own.
        /// </remarks>
        internal static IEnumerable<(ExpressionBuilderGraph Graph, Node<ExpressionBuilder, ExpressionBuilderArgument> Node)>
            Nodes(ExpressionBuilderGraph workflow)
        {
            foreach (var node in workflow)
            {
                yield return (workflow, node);
                if (ExpressionBuilder.Unwrap(node.Value) is IWorkflowExpressionBuilder builder && IsGroup(builder) &&
                    builder.Workflow is { } nested)
                {
                    foreach (var inner in Nodes(nested))
                        yield return inner;
                }
            }
        }

        /// <summary>
        /// Every node in <paramref name="workflow"/> and every workflow nested in it, whatever its scope, with the
        /// graph each one is in.
        /// </summary>
        internal static IEnumerable<(ExpressionBuilderGraph Graph, Node<ExpressionBuilder, ExpressionBuilderArgument> Node)>
            AllNodes(ExpressionBuilderGraph workflow)
        {
            foreach (var node in workflow)
            {
                yield return (workflow, node);
                if (ExpressionBuilder.Unwrap(node.Value) is WorkflowExpressionBuilder { Workflow: { } nested })
                {
                    foreach (var inner in AllNodes(nested))
                        yield return inner;
                }
            }
        }

        /// <summary>
        /// Whether <paramref name="builder"/> is part of the workflow that holds it rather than a scope of its own.
        /// </summary>
        internal static bool IsGroup(IWorkflowExpressionBuilder builder) =>
            builder is IncludeWorkflowBuilder || builder is GroupWorkflowBuilder;
    }
}
