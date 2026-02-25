using System;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Specifies whether a property is an ONI device table property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public sealed class DeviceTablePropertyAttribute : Attribute
    {
        /// <summary>
        /// Specifies that the default value for this attribute is that the property
        /// is not an ONI device table property.
        /// </summary>
        public static readonly DeviceTablePropertyAttribute Default = new(false);

        /// <summary>
        /// Gets a value indicating whether a property is an ONI device table property.
        /// </summary>
        public bool DeviceTableProperty { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceTablePropertyAttribute"/> class.
        /// </summary>
        /// <param name="deviceTableProperty">
        /// <see langword="true"/> if the property is an ONI device table property;
        /// otherwise, <see langword="false"/>. The default is <see langword="true"/>.
        /// </param>
        public DeviceTablePropertyAttribute(bool deviceTableProperty)
        {
            DeviceTableProperty = deviceTableProperty;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="obj"/> is an instance of <see cref="DeviceTablePropertyAttribute"/>
        /// and the state equals the state of this instance; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            return obj is DeviceTablePropertyAttribute other && other.DeviceTableProperty == DeviceTableProperty;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return DeviceTableProperty.GetHashCode();
        }
    }
}
