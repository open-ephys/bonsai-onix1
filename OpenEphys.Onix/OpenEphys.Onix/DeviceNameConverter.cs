using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Bonsai.Expressions;
using Bonsai;

namespace OpenEphys.Onix
{
    /// <summary>
    /// Provides a type converter to convert a device name to and from other representations.
    /// It also provides a mechanism to find existing devices declared in the workflow.
    /// </summary>
    public class DeviceNameConverter : StringConverter
    {
        readonly Type targetType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceNameConverter"/> class
        /// for the specified type.
        /// </summary>
        /// <param name="deviceType">
        /// The type of devices supported by this converter.
        /// </param>
        protected DeviceNameConverter(Type deviceType)
        {
            targetType = deviceType;
        }

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

        /// <inheritdoc/>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Returns a collection of device names which are available in the call context
        /// of this type converter request.
        /// </summary>
        /// <returns>
        /// A <see cref="TypeConverter.StandardValuesCollection"/> containing the set of
        /// available devices. Only devices matching the specified type will be included.
        /// </returns>
        /// <inheritdoc/>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context != null)
            {
                var workflowBuilder = (WorkflowBuilder)context.GetService(typeof(WorkflowBuilder));
                var nodeBuilderGraph = (ExpressionBuilderGraph)context.GetService(typeof(ExpressionBuilderGraph));
                if (workflowBuilder != null && nodeBuilderGraph != null)
                {
                    var callContext = new Stack<ExpressionBuilderGraph>();
                    if (GetCallContext(workflowBuilder.Workflow, nodeBuilderGraph, callContext))
                    {
                        var names = (from level in callContext
                                     from element in SelectContextElements(level)
                                     let factory = ExpressionBuilder.GetWorkflowElement(element) as DeviceFactory
                                     where factory != null
                                     from device in factory.GetDevices()
                                     where device.Type == targetType
                                     select device.Name)
                                     .Distinct()
                                     .ToList();
                        return new StandardValuesCollection(names);
                    }
                }
            }

            return base.GetStandardValues(context);
        }
    }
}
