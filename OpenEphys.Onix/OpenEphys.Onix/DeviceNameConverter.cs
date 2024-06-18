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
                    var devices = DeviceHelper.FindDevices(workflowBuilder, nodeBuilderGraph, targetType);
                    var deviceNames = devices.Select(device => device.DeviceName).Distinct().ToList();
                    return new StandardValuesCollection(deviceNames);
                }
            }

            return base.GetStandardValues(context);
        }
    }
}
