namespace OpenEphys.Onix1
{
    /// <summary>
    /// Public interface that defines common properties in NeuropixelsV1 devices.
    /// </summary>
    public interface IConfigureNeuropixelsV1
    {
        /// <summary>
        /// Returns a deep copy of the current <see cref="IConfigureNeuropixelsV1"/> instance.
        /// </summary>
        public IConfigureNeuropixelsV1 Clone();

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// Gets or sets the probe configuration.
        /// </summary>
        public NeuropixelsV1ProbeConfiguration ProbeConfiguration { get; set; }
    }
}
