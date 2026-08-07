namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// GUI for <see cref="ConfigureHeadstageNeuropixelsV2Rhd2000e"/>. Hosts one
    /// <see cref="NeuropixelsV2eImGuiDialog"/>, one <see cref="ImGuiPropertyPanel"/> for the
    /// Rhd2000, and one <see cref="ImGuiPropertyPanel"/> for the Bno055, each in its own tab.
    /// </summary>
    internal class NeuropixelsV2Rhd2000eHeadstageDialog : ImGuiShellDialog
    {
        /// <summary>Gets the <see cref="NeuropixelsV2eImGuiDialog"/>.</summary>
        internal NeuropixelsV2eImGuiDialog DialogNeuropixelsV2 { get; private set; }

        /// <summary>Gets the <see cref="ImGuiPropertyPanel"/> for the Rhd2000.</summary>
        internal ImGuiPropertyPanel DialogRhd2000 { get; private set; }

        /// <summary>Gets the <see cref="ImGuiPropertyPanel"/> for the Bno055.</summary>
        internal ImGuiPropertyPanel DialogBno055 { get; private set; }

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV2Rhd2000eHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureHeadstage">Configuration settings for a <see cref="ConfigureHeadstageNeuropixelsV2Rhd2000e"/>.</param>
        public NeuropixelsV2Rhd2000eHeadstageDialog(ConfigureHeadstageNeuropixelsV2Rhd2000e configureHeadstage)
            : base("Headstage NeuropixelsV2/Rhd2000e Configuration")
        {
            const string probeName = nameof(ConfigureHeadstageNeuropixelsV2Rhd2000e.NeuropixelsV2);
            DialogNeuropixelsV2 = new NeuropixelsV2eImGuiDialog(configureHeadstage.NeuropixelsV2, probeName, PortName.PortA, true); // TODO: Port selection
            AddTab(probeName, DialogNeuropixelsV2);

            DialogRhd2000 = new ImGuiPropertyPanel(configureHeadstage.Rhd2000, filterDeviceTableProperties: true);
            AddTab("Rhd2000", DialogRhd2000);

            DialogBno055 = new ImGuiPropertyPanel(configureHeadstage.Bno055, filterDeviceTableProperties: true);
            AddTab("Bno055", DialogBno055);
        }
    }
}
