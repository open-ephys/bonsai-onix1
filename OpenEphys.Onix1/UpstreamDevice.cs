using System;
using System.Linq;
using Bonsai;
using Bonsai.Dag;
using Bonsai.Expressions;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Finds which device's data reaches a node in a workflow, by following the workflow upstream from it.
    /// </summary>
    static class UpstreamDevice
    {
        /// <summary>
        /// The device whose data reaches <paramref name="self"/>: <paramref name="declared"/> if it is set, checked
        /// against the data operator upstream, or else that operator's device.
        /// </summary>
        /// <remarks>
        /// The data is followed upstream only through nodes with exactly one input, such as a Condition or a Gate,
        /// which cannot bring in another device's data, and from a SubscribeSubject to whatever feeds its subject.
        /// It stops at a branch such as Zip, where nothing upstream says which device the data is from.
        /// </remarks>
        /// <param name="workflow">The whole workflow, in which <paramref name="self"/> is searched for.</param>
        /// <param name="self">The node whose data is traced.</param>
        /// <param name="declared">The device name set on the node, or empty to take the upstream one.</param>
        /// <param name="deviceNameOf">
        /// The device name declared on a workflow element, or null if it is not a data operator the caller can
        /// use.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// No device can be found, or the declared one is not the one the data comes from.
        /// </exception>
        internal static string Resolve(
            ExpressionBuilderGraph workflow, ExpressionBuilder self, string declared,
            Func<object, string> deviceNameOf)
        {
            var (graph, node) = workflow is null
                ? default
                : WorkflowScope.AllNodes(workflow).FirstOrDefault(x => x.Node.Value == self);
            if (node is null)
                throw new InvalidOperationException("The node could not locate itself in the workflow.");

            string attached = null;
            while (attached is null)
            {
                var upstream = graph.Predecessors(node).ToList();
                if (upstream.Count == 1)
                {
                    node = upstream[0];
                    attached = deviceNameOf(ExpressionBuilder.GetWorkflowElement(node.Value));
                }
                else if (upstream.Count != 0 ||
                    ExpressionBuilder.Unwrap(node.Value) is not SubscribeSubject subscribe ||
                    !TryFindSubjectSource(workflow, graph, subscribe.Name, out graph, out node))
                {
                    break;
                }
            }

            if (string.IsNullOrEmpty(declared))
            {
                return string.IsNullOrEmpty(attached)
                    ? throw new InvalidOperationException(
                        "No data operator the node can use was found upstream of it. Set its DeviceName.")
                    : attached;
            }

            if (!string.IsNullOrEmpty(attached) && attached != declared)
            {
                throw new InvalidOperationException(
                    $"The node's DeviceName is {declared}, but its data comes from {attached}.");
            }

            return declared;
        }

        /// <summary>
        /// The node that feeds the subject <paramref name="name"/> as seen from <paramref name="scope"/>: the
        /// subject itself when it has an input, or else the one MulticastSubject that writes to it.
        /// </summary>
        /// <remarks>
        /// Looked up as Bonsai resolves a subject, in the innermost workflow that declares the name and then
        /// in each enclosing one.
        /// </remarks>
        static bool TryFindSubjectSource(
            ExpressionBuilderGraph workflow,
            ExpressionBuilderGraph scope,
            string name,
            out ExpressionBuilderGraph graph,
            out Node<ExpressionBuilder, ExpressionBuilderArgument> node)
        {
            for (; scope is not null; scope = WorkflowScope.AllNodes(workflow).FirstOrDefault(x =>
                ExpressionBuilder.Unwrap(x.Node.Value) is WorkflowExpressionBuilder w && w.Workflow == scope).Graph)
            {
                // NB: in the scope's Groups too, which Bonsai counts as part of it.
                var subject = WorkflowScope.Nodes(scope).FirstOrDefault(x =>
                    ExpressionBuilder.Unwrap(x.Node.Value) is SubjectExpressionBuilder s && s.Name == name);
                if (subject.Node is null)
                    continue;

                if (subject.Graph.Predecessors(subject.Node).Any())
                {
                    (graph, node) = subject;
                    return true;
                }

                var writers = WorkflowScope.AllNodes(scope)
                    .Where(x => ExpressionBuilder.Unwrap(x.Node.Value) is MulticastSubject m && m.Name == name)
                    .ToList();
                (graph, node) = writers.Count == 1 ? writers[0] : default;
                return writers.Count == 1;
            }

            graph = null;
            node = null;
            return false;
        }
    }
}
