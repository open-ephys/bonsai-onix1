namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// GUI for <see cref="ConfigureHeadstageNeuropixelsV2e"/> and <see
    /// cref="ConfigureHeadstageNeuropixelsV2eBeta"/>. Hosts two <see cref="NeuropixelsV2eImGuiDialog"/>
    /// instances (ProbeA and ProbeB) and one <see cref="GenericDeviceDialog"/> for the Bno055, each in its
    /// own tab.
    /// </summary>
    internal class NeuropixelsV2eHeadstageDialog : HeadstageDialog
    {
        /// <summary>Gets the <see cref="NeuropixelsV2eImGuiDialog"/> for ProbeA.</summary>
        internal NeuropixelsV2eImGuiDialog DialogNeuropixelsV2A =>
            (NeuropixelsV2eImGuiDialog)GetProbeDialog(0);

        /// <summary>Gets the <see cref="NeuropixelsV2eImGuiDialog"/> for ProbeB.</summary>
        internal NeuropixelsV2eImGuiDialog DialogNeuropixelsV2B =>
            (NeuropixelsV2eImGuiDialog)GetProbeDialog(1);

        /// <summary>Gets the <see cref="GenericDeviceDialog"/> for the Bno055.</summary>
        internal GenericDeviceDialog DialogBno055 { get; private set; }

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV2eHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureHeadstage">Configuration settings for a <see cref="ConfigureHeadstageNeuropixelsV2e"/>.</param>
        public NeuropixelsV2eHeadstageDialog(ConfigureHeadstageNeuropixelsV2e configureHeadstage)
        {
            Text = "HeadstageNeuropixels2.0 Configuration";
            InitializeTabs(configureHeadstage.NeuropixelsV2A, configureHeadstage.NeuropixelsV2B, configureHeadstage.Bno055);
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV2eHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureHeadstage">Configuration settings for a <see cref="ConfigureHeadstageNeuropixelsV2eBeta"/>.</param>
        public NeuropixelsV2eHeadstageDialog(ConfigureHeadstageNeuropixelsV2eBeta configureHeadstage)
        {
            Text = "HeadstageNeuropixels2.0-Beta Configuration";
            InitializeTabs(configureHeadstage.NeuropixelsV2A, configureHeadstage.NeuropixelsV2B, configureHeadstage.Bno055);
        }

        void InitializeTabs(IConfigureNeuropixelsV2 neuropixelsV2A, IConfigureNeuropixelsV2 neuropixelsV2B, ConfigurePolledBno055 bno055)
        {
            const string nameA = nameof(ConfigureHeadstageNeuropixelsV2e.NeuropixelsV2A);
            const string nameB = nameof(ConfigureHeadstageNeuropixelsV2e.NeuropixelsV2B);

            AddProbeTab(nameA,
                new NeuropixelsV2eImGuiDialog(neuropixelsV2A, nameA, PortName.PortA, true), // TODO: get port from configureHeadstage somehow
                old => new NeuropixelsV2eImGuiDialog(old.ConfigureNeuropixelsV2, nameA, PortName.PortA, true));

            AddProbeTab(nameB, 
                new NeuropixelsV2eImGuiDialog(neuropixelsV2B, nameB, PortName.PortA, false),
                old => new NeuropixelsV2eImGuiDialog(old.ConfigureNeuropixelsV2, nameB, PortName.PortA, false));

            DialogBno055 = new GenericDeviceDialog(bno055, true);
            AddDeviceTab("Bno055", DialogBno055);
        }
    }
}
