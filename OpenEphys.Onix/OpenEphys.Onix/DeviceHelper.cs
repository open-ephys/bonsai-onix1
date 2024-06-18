using System;
using System.Collections.Generic;
using System.Linq;
using Bonsai;
using Bonsai.Expressions;

namespace OpenEphys.Onix
{
    internal static class DeviceHelper
    {
        static bool IsGroup(IWorkflowExpressionBuilder builder)
        {
            return builder is IncludeWorkflowBuilder || builder is GroupWorkflowBuilder;
        }

        static IEnumerable<ExpressionBuilder> SelectContextElements(ExpressionBuilderGraph source)
        {
            foreach (var node in source)
            {
                var element = ExpressionBuilder.Unwrap(node.Value);
                yield return element;

                var workflowBuilder = element as IWorkflowExpressionBuilder;
                if (IsGroup(workflowBuilder))
                {
                    var workflow = workflowBuilder.Workflow;
                    if (workflow == null) continue;
                    foreach (var groupElement in SelectContextElements(workflow))
                    {
                        yield return groupElement;
                    }
                }
            }
        }

        static bool GetCallContext(ExpressionBuilderGraph source, ExpressionBuilderGraph target, Stack<ExpressionBuilderGraph> context)
        {
            context.Push(source);
            if (source == target)
            {
                return true;
            }

            foreach (var element in SelectContextElements(source))
            {
                var groupBuilder = element as IWorkflowExpressionBuilder;
                if (IsGroup(groupBuilder) && groupBuilder.Workflow == target)
                {
                    return true;
                }

                if (element is WorkflowExpressionBuilder workflowBuilder)
                {
                    if (GetCallContext(workflowBuilder.Workflow, target, context))
                    {
                        return true;
                    }
                }
            }

            context.Pop();
            return false;
        }

        public static IEnumerable<IDeviceConfiguration> FindDevices(WorkflowBuilder workflowBuilder, ExpressionBuilderGraph contextBuilderGraph, Type deviceType)
        {
            var callContext = new Stack<ExpressionBuilderGraph>();
            if (GetCallContext(workflowBuilder.Workflow, contextBuilderGraph, callContext))
            {
                return from level in callContext
                       from element in SelectContextElements(level)
                       let factory = ExpressionBuilder.GetWorkflowElement(element) as DeviceFactory
                       where factory != null
                       from device in factory.GetDevices()
                       where device.DeviceType == deviceType
                       select device;
            }

            return Enumerable.Empty<IDeviceConfiguration>();
        }
    }
}
