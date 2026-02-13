using System;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Tells the FrameWriter to ignore this property when writing frames to disk.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class FrameWriterIgnoreMembersAttribute : Attribute
    {
        /// <summary>
        /// Gets the <see cref="Onix1.MemberType"/> to ignore.
        /// </summary>
        public MemberType MemberType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameWriterIgnoreMembersAttribute"/> with
        /// <see cref="MemberType.All"/> as the <see cref="Onix1.MemberType"/>
        /// </summary>
        public FrameWriterIgnoreMembersAttribute()
            : this(MemberType.All)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameWriterIgnoreMembersAttribute"/> with the
        /// given <see cref="Onix1.MemberType"/>
        /// </summary>
        /// <param name="memberType">Selected <see cref="Onix1.MemberType"/> to ignore.</param>
        public FrameWriterIgnoreMembersAttribute(MemberType memberType)
        {
            MemberType = memberType;
        }
    }

    /// <summary>
    /// Specifies types of members.
    /// </summary>
    [Flags]
    public enum MemberType
    {
        /// <summary>
        /// Specifies no members.
        /// </summary>
        None = 0,
        /// <summary>
        /// Specifies all field members.
        /// </summary>
        Fields,
        /// <summary>
        /// Specifies all property members.
        /// </summary>
        Properties,
        /// <summary>
        /// Specifies all field and property members.
        /// </summary>
        All = Fields | Properties
    }
}
