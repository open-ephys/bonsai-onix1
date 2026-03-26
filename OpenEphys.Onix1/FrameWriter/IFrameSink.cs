namespace OpenEphys.Onix1.FrameWriter
{
    /// <summary>
    /// Interface for a frame sink, which is a component that receives frames of data and processes them in some way.
    /// </summary>
    public interface IFrameSink
    {
        /// <summary>
        /// Gets or sets a value indicating whether the data should be compressed before being written to the file.
        /// </summary>
        public bool CompressData { get; set; }
    }
}
