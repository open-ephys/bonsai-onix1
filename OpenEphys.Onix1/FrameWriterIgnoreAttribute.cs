using System;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Tells the FrameWriter to ignore this property when writing frames to disk.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class FrameWriterIgnoreAttribute : Attribute
    {
    }
}
