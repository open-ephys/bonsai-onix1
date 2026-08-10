namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// GUI for <see cref="ConfigureHeadstageNeuropixelsV1f"/>. Hosts two
    /// <see cref="NeuropixelsV1ImGuiDialog"/> instances (ProbeA and ProbeB) and one
    /// <see cref="ImGuiPropertyPanel"/> for the Bno055, each in its own tab.
    /// </summary>
    internal class NeuropixelsV1fHeadstageDialog : ImGuiShellDialog
    {
        /// <summary>Gets the <see cref="NeuropixelsV1ImGuiDialog"/> for ProbeA.</summary>
        internal NeuropixelsV1ImGuiDialog DialogNeuropixelsV1A { get; private set; }

        /// <summary>Gets the <see cref="NeuropixelsV1ImGuiDialog"/> for ProbeB.</summary>
        internal NeuropixelsV1ImGuiDialog DialogNeuropixelsV1B { get; private set; }

        /// <summary>Gets the <see cref="ImGuiPropertyPanel"/> for the Bno055.</summary>
        internal ImGuiPropertyPanel DialogBno055 { get; private set; }

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
            : base("Headstage NeuropixelsV1f Configuration")
        {
            const string nameA = nameof(ConfigureHeadstageNeuropixelsV1f.NeuropixelsV1A);
            const string nameB = nameof(ConfigureHeadstageNeuropixelsV1f.NeuropixelsV1B);

            DialogNeuropixelsV1A = new NeuropixelsV1ImGuiDialog(configureNeuropixelsV1A, nameA, Log);
            AddTab(nameA, DialogNeuropixelsV1A);

            DialogNeuropixelsV1B = new NeuropixelsV1ImGuiDialog(configureNeuropixelsV1B, nameB, Log);
            AddTab(nameB, DialogNeuropixelsV1B);

            DialogBno055 = new ImGuiPropertyPanel(configureBno055, filterDeviceTableProperties: true);
            AddTab("Bno055", DialogBno055);
        }
    }
}
