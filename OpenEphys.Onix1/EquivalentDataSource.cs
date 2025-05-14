using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Specifies a device class as equivalent to another for data production
    /// </summary>
    /// <remarks>
    /// This attribute must be used on the static classes that specify
    /// the device IDs and name converters to mark it as equivalent to
    /// another. 
    /// 
    /// This will be used only for data production and name converters
    /// 
    /// The method <see cref="ContextHelper.CheckDeviceType(Type, Type)"/>
    /// can be used to check for type or equivalent type matching
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class EquivalentDataSource : Attribute
    {
        /// <summary>
        /// The original type the tagged class is equivalent to
        /// </summary>
        public Type BaseType { get; }

        public EquivalentDataSource(Type baseType)
        {
            BaseType = baseType;
        }
    }
}
