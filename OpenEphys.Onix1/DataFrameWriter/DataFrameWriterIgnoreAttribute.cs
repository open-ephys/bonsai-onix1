using System;

namespace OpenEphys.Onix1.DataFrameWriter
{
    /// <summary>
    /// Tells the FrameWriter to ignore this property when writing frames to disk.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class DataFrameWriterIgnoreAttribute : Attribute
    {
    }
}
