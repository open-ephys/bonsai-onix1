namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// GUI for <see cref="ConfigureHeadstageNeuropixelsV1f"/>. Hosts two
    /// <see cref="NeuropixelsV1ImGuiDialog"/> instances (ProbeA and ProbeB) and one
    /// <see cref="GenericDeviceDialog"/> for the Bno055, each in its own tab.
    /// </summary>
    internal class NeuropixelsV1fHeadstageDialog : HeadstageDialog
    {
        /// <summary>Gets the <see cref="NeuropixelsV1ImGuiDialog"/> for ProbeA.</summary>
        internal NeuropixelsV1ImGuiDialog DialogNeuropixelsV1A =>
            (NeuropixelsV1ImGuiDialog)GetProbeDialog(0);

        /// <summary>Gets the <see cref="NeuropixelsV1ImGuiDialog"/> for ProbeB.</summary>
        internal NeuropixelsV1ImGuiDialog DialogNeuropixelsV1B =>
            (NeuropixelsV1ImGuiDialog)GetProbeDialog(1);

        /// <summary>Gets the <see cref="GenericDeviceDialog"/> for the Bno055.</summary>
        internal readonly GenericDeviceDialog DialogBno055;

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV1fHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureNeuropixelsV1A">Configuration settings for <see cref="ConfigureNeuropixelsV1f"/> A.</param>
        /// <param name="configureNeuropixelsV1B">Configuration settings for <see cref="ConfigureNeuropixelsV1f"/> B.</param>
        /// <param name="configureBno055">Configuration settings for the Bno055.</param>
        public NeuropixelsV1fHeadstageDialog(
            ConfigureNeuropixelsV1f configureNeuropixelsV1A,
            ConfigureNeuropixelsV1f configureNeuropixelsV1B,
            ConfigureBno055 configureBno055)
        {
            Text = "HeadstageNeuropixels1.0f Configuration";

            const string nameA = nameof(ConfigureHeadstageNeuropixelsV1f.NeuropixelsV1A);
            const string nameB = nameof(ConfigureHeadstageNeuropixelsV1f.NeuropixelsV1B);

            AddProbeTab(nameA, new NeuropixelsV1ImGuiDialog(configureNeuropixelsV1A, nameA),
                old => new NeuropixelsV1ImGuiDialog(old.ConfigureNeuropixelsV1, nameA));

            AddProbeTab(nameB, new NeuropixelsV1ImGuiDialog(configureNeuropixelsV1B, nameB),
                old => new NeuropixelsV1ImGuiDialog(old.ConfigureNeuropixelsV1, nameB));

            DialogBno055 = new GenericDeviceDialog(configureBno055, true);
            AddDeviceTab("Bno055", DialogBno055);
        }
    }
}
