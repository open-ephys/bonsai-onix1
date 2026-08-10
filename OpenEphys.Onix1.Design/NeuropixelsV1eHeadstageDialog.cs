namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// GUI for <see cref="ConfigureHeadstageNeuropixelsV1e"/>. Hosts one
    /// <see cref="NeuropixelsV1ImGuiDialog"/> and one <see cref="ImGuiPropertyPanel"/> for the Bno055,
    /// each in its own tab.
    /// </summary>
    internal class NeuropixelsV1eHeadstageDialog : ImGuiShellDialog
    {
        /// <summary>Gets the <see cref="NeuropixelsV1ImGuiDialog"/>.</summary>
        public NeuropixelsV1ImGuiDialog DialogNeuropixelsV1e { get; private set; }

        /// <summary>Gets the <see cref="ImGuiPropertyPanel"/> for the Bno055.</summary>
        public ImGuiPropertyPanel DialogBno055 { get; private set; }

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV1eHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureHeadstage">Configuration settings for a <see cref="ConfigureHeadstageNeuropixelsV1e"/>.</param>
        public NeuropixelsV1eHeadstageDialog(ConfigureHeadstageNeuropixelsV1e configureHeadstage)
            : base("Headstage NeuropixelsV1eConfiguration")
        {
            const string probeName = nameof(NeuropixelsV1);
            DialogNeuropixelsV1e = new NeuropixelsV1ImGuiDialog(configureHeadstage.NeuropixelsV1, probeName, Log);
            AddTab(probeName, DialogNeuropixelsV1e);

            DialogBno055 = new ImGuiPropertyPanel(configureHeadstage.Bno055, filterDeviceTableProperties: true);
            AddTab("Bno055", DialogBno055);
        }
    }
}
