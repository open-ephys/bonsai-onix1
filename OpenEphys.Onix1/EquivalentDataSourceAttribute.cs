using System;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Specifies a device class as equivalent to another for data production.
    /// </summary>
    /// <remarks>
    /// This attribute can be used on a static class that specifies device constants (e.g. ID, register
    /// addresses, etc) to indicate that the device produces <see cref="oni.Frame">oni.Frames</see> equivalent to
    /// those of another device and, therefore, that both devices share a common (set of) data operator(s).
    /// A device produces a single kind of data, so it can be equivalent to at most one other device. To make a
    /// device usable by data operators whose device class is not accessible, use
    /// <see cref="EquivalentDataNodeAttribute"/> instead. This attribute is used by
    /// <see cref="DeviceNameConverter"/> to identify devices that can make use of a given data operator.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class EquivalentDataSourceAttribute : Attribute
    {
        internal Type BaseType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EquivalentDataSourceAttribute"/> class.
        /// </summary>
        /// <param name="baseType">Device type that produces equivalent <see cref="oni.Frame">oni.Frames</see>.</param>
        public EquivalentDataSourceAttribute(Type baseType)
        {
            BaseType = baseType;
        }
    }
}
