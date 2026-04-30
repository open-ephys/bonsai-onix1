namespace OpenEphys.Onix1
{
    /// <summary>
    /// Public interface that defines common properties in NeuropixelsV2 devices.
    /// </summary>
    public interface IConfigureNeuropixelsV2
    {
        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// Gets or sets the probe configuration.
        /// </summary>
        public NeuropixelsV2ProbeConfiguration ProbeConfiguration { get; set; }
    }
}
