namespace OpenEphys.Onix1
{
    /// <summary>
    /// Identifies a probe group with more physical contacts than acquisition channels, where
    /// enabling a contact wires it to a specific channel.
    /// </summary>
    public interface IMultiplexedProbeGroup
    {
        /// <summary>
        /// Returns the acquisition channel the given contact would occupy if enabled.
        /// </summary>
        /// <param name="contactIndex">The zero-based contact index across all electrodes on the probe.</param>
        int GetChannel(int contactIndex);
    }
}
