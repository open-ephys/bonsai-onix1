using System;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Specifies a data node that can use the data produced by a device class.
    /// </summary>
    /// <remarks>
    /// This attribute can be used on a static class that specifies device constants (e.g. ID, register
    /// addresses, etc) to indicate that the device produces <see cref="oni.Frame">oni.Frames</see> in the format
    /// read by the specified data node and, therefore, that it can be used by all the data operators of the
    /// device type that node works with. It is an alternative to <see cref="EquivalentDataSourceAttribute"/> for
    /// when the device class of those operators is not accessible, such as from other libraries. This attribute
    /// is used by <see cref="DeviceNameConverter"/> to identify devices that can make use of a given data
    /// operator.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class EquivalentDataNodeAttribute : Attribute
    {
        internal Type NodeType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EquivalentDataNodeAttribute"/> class.
        /// </summary>
        /// <param name="nodeType">
        /// Type of a data node that can read the <see cref="oni.Frame">oni.Frames</see> produced by the device.
        /// </param>
        public EquivalentDataNodeAttribute(Type nodeType)
        {
            NodeType = nodeType;
        }
    }
}
