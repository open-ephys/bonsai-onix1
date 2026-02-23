namespace OpenEphys.Onix1
{
    /// <summary>
    /// Defines the configuration for a probe that implements the ProbeInterface specification.
    /// </summary>
    public interface IProbeInterfaceConfiguration
    {
        /// <summary>
        /// Gets or sets the ProbeInterface file name.
        /// </summary>
        string ProbeInterfaceFileName { get; set; }
    }
}
