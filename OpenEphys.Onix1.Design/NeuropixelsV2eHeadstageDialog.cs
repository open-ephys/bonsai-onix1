namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// GUI for <see cref="ConfigureHeadstageNeuropixelsV2e"/> and
    /// <see cref="ConfigureHeadstageNeuropixelsV2eBeta"/>. Hosts two
    /// <see cref="NeuropixelsV2eDialog"/> instances (ProbeA and ProbeB) and one
    /// <see cref="GenericDeviceDialog"/> for the Bno055, each in its own tab.
    /// </summary>
    public class NeuropixelsV2eHeadstageDialog : HeadstageDialog
    {
        /// <summary>Gets the <see cref="NeuropixelsV2eDialog"/> for ProbeA.</summary>
        internal NeuropixelsV2eDialog DialogNeuropixelsV2A =>
            (NeuropixelsV2eDialog)GetProbeDialog(0);

        /// <summary>Gets the <see cref="NeuropixelsV2eDialog"/> for ProbeB.</summary>
        internal NeuropixelsV2eDialog DialogNeuropixelsV2B =>
            (NeuropixelsV2eDialog)GetProbeDialog(1);

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

            AddProbeTab(nameA, new NeuropixelsV2eDialog(neuropixelsV2A, nameA, true),
                old => RecreateDialog(old));

            AddProbeTab(nameB, new NeuropixelsV2eDialog(neuropixelsV2B, nameB, true),
                old => RecreateDialog(old));

            DialogBno055 = new GenericDeviceDialog(bno055, true);
            AddDeviceTab("Bno055", DialogBno055);
        }

        static NeuropixelsV2eDialog RecreateDialog(NeuropixelsV2eDialog old)
        {
            var newDialog = new NeuropixelsV2eDialog(
                old.ConfigureNeuropixelsV2, old.ProbeConfigurationDialog.ProbeName, true);
            newDialog.ProbeConfigurationDialog.ChannelConfiguration.ProbeGroup =
                old.ProbeConfigurationDialog.ChannelConfiguration.ProbeGroup;
            newDialog.ProbeConfigurationDialog.ChannelConfiguration.RedrawProbeGroup();
            newDialog.ProbeConfigurationDialog.CheckForExistingChannelPreset();
            return newDialog;
        }
    }
}
