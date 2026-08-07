namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// GUI for <see cref="ConfigureHeadstageNeuropixelsV2e"/> and <see
    /// cref="ConfigureHeadstageNeuropixelsV2eBeta"/>. Hosts two <see cref="NeuropixelsV2eImGuiDialog"/>
    /// instances (ProbeA and ProbeB) and one <see cref="ImGuiPropertyPanel"/> for the Bno055, each in its
    /// own tab.
    /// </summary>
    internal class NeuropixelsV2eHeadstageDialog : ImGuiShellDialog
    {
        /// <summary>Gets the <see cref="NeuropixelsV2eImGuiDialog"/> for ProbeA.</summary>
        internal NeuropixelsV2eImGuiDialog DialogNeuropixelsV2A { get; private set; }

        /// <summary>Gets the <see cref="NeuropixelsV2eImGuiDialog"/> for ProbeB.</summary>
        internal NeuropixelsV2eImGuiDialog DialogNeuropixelsV2B { get; private set; }

        /// <summary>Gets the <see cref="ImGuiPropertyPanel"/> for the Bno055.</summary>
        internal ImGuiPropertyPanel DialogBno055 { get; private set; }

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV2eHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureHeadstage">Configuration settings for a <see cref="ConfigureHeadstageNeuropixelsV2e"/>.</param>
        public NeuropixelsV2eHeadstageDialog(ConfigureHeadstageNeuropixelsV2e configureHeadstage)
            : base("Headstage NeuropixelsV2e Configuration")
        {
            InitializeTabs(configureHeadstage.NeuropixelsV2A, configureHeadstage.NeuropixelsV2B, configureHeadstage.Bno055);
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV2eHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureHeadstage">Configuration settings for a <see cref="ConfigureHeadstageNeuropixelsV2eBeta"/>.</param>
        public NeuropixelsV2eHeadstageDialog(ConfigureHeadstageNeuropixelsV2eBeta configureHeadstage)
            : base("Headstage NeuropixelsV2e-Beta Configuration")
        {
            InitializeTabs(configureHeadstage.NeuropixelsV2A, configureHeadstage.NeuropixelsV2B, configureHeadstage.Bno055);
        }

        void InitializeTabs(IConfigureNeuropixelsV2 neuropixelsV2A, IConfigureNeuropixelsV2 neuropixelsV2B, ConfigurePolledBno055 bno055)
        {
            const string nameA = nameof(ConfigureHeadstageNeuropixelsV2e.NeuropixelsV2A);
            const string nameB = nameof(ConfigureHeadstageNeuropixelsV2e.NeuropixelsV2B);

            DialogNeuropixelsV2A = new NeuropixelsV2eImGuiDialog(neuropixelsV2A, nameA, PortName.PortA, true); // TODO: get port from configureHeadstage somehow
            AddTab(nameA, DialogNeuropixelsV2A);

            DialogNeuropixelsV2B = new NeuropixelsV2eImGuiDialog(neuropixelsV2B, nameB, PortName.PortA, false);
            AddTab(nameB, DialogNeuropixelsV2B);

            DialogBno055 = new ImGuiPropertyPanel(bno055, filterDeviceTableProperties: true);
            AddTab("Bno055", DialogBno055);
        }
    }
}
