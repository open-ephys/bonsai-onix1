namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// GUI for <see cref="ConfigureHeadstageNeuropixelsV1e"/>. Hosts one
    /// <see cref="NeuropixelsV1ImGuiDialog"/> and one <see cref="GenericDeviceDialog"/> for the Bno055,
    /// each in its own tab.
    /// </summary>
    internal class NeuropixelsV1eHeadstageDialog : HeadstageDialog
    {
        /// <summary>Gets the <see cref="NeuropixelsV1ImGuiDialog"/>.</summary>
        public NeuropixelsV1ImGuiDialog DialogNeuropixelsV1e =>
            (NeuropixelsV1ImGuiDialog)GetProbeDialog(0);

        /// <summary>Gets the <see cref="GenericDeviceDialog"/> for the Bno055.</summary>
        public readonly GenericDeviceDialog DialogBno055;

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV1eHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureHeadstage">Configuration settings for a <see cref="ConfigureHeadstageNeuropixelsV1e"/>.</param>
        public NeuropixelsV1eHeadstageDialog(ConfigureHeadstageNeuropixelsV1e configureHeadstage)
        {
            Text = "HeadstageNeuropixels1.0e Configuration";

            const string probeName = nameof(NeuropixelsV1);
            var probeDialog = new NeuropixelsV1ImGuiDialog(configureHeadstage.NeuropixelsV1, probeName);
            AddProbeTab(probeName, probeDialog, old => new NeuropixelsV1ImGuiDialog(old.ConfigureNeuropixelsV1, probeName));

            DialogBno055 = new GenericDeviceDialog(configureHeadstage.Bno055, true);
            AddDeviceTab("Bno055", DialogBno055);
        }
    }
}
