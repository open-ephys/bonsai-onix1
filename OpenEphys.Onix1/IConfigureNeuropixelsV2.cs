namespace OpenEphys.Onix1
{
    /// <summary>
    /// Public interface that defines common properties in NeuropixelsV2 devices.
    /// </summary>
    public interface IConfigureNeuropixelsV2
    {
        /// <summary>
        /// Returns a deep copy of the current <see cref="IConfigureNeuropixelsV2"/> instance.
        /// </summary>
        public IConfigureNeuropixelsV2 Clone();

        /// <summary>
        /// Gets or sets the device enable state.
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// Gets or sets the electrode configuration for Probe A.
        /// </summary>
        public NeuropixelsV2ProbeConfiguration ProbeConfigurationA { get; set; }

        /// <summary>
        /// Gets or sets the electrode configuration for Probe B.
        /// </summary>
        public NeuropixelsV2ProbeConfiguration ProbeConfigurationB { get; set; }
    }
}
