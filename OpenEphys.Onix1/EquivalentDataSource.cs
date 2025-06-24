using System;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Specifies a device class as equivalent to another for data production
    /// </summary>
    /// <remarks>
    /// This attribute can be used on a static class that specifies device constants (e.g. ID, register
    /// addresses, etc) to indicate that another device produces equivalent <see
    /// cref="oni.Frame">oni.Frames</see> and, therefore, that both devices a common (set of) production
    /// operator(s). This attribute is used by <see cref="DeviceNameConverter"/> to identify devices that can
    /// make use of a given data production operator.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class EquivalentDataSource : Attribute
    {
        internal Type BaseType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EquivalentDataSource"/> class.
        /// </summary>
        /// <param name="baseType">Device type that produces equivalent <see cref="oni.Frame">oni.Frames</see></param>
        public EquivalentDataSource(Type baseType)
        {
            BaseType = baseType;
        }
    }
}
