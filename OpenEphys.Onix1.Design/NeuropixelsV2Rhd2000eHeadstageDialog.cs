namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// GUI for <see cref="ConfigureHeadstageNeuropixelsV2Rhd2000e"/>. Hosts one
    /// <see cref="NeuropixelsV2eDialog"/>, one <see cref="GenericDeviceDialog"/> for the
    /// Rhd2000, and one <see cref="GenericDeviceDialog"/> for the Bno055, each in its own tab.
    /// </summary>
    internal class NeuropixelsV2Rhd2000eHeadstageDialog : HeadstageDialog
    {
        /// <summary>Gets the <see cref="NeuropixelsV2eDialog"/>.</summary>
        internal NeuropixelsV2eDialog DialogNeuropixelsV2 =>
            (NeuropixelsV2eDialog)GetProbeDialog(0);

        /// <summary>Gets the <see cref="GenericDeviceDialog"/> for the Rhd2000.</summary>
        internal readonly GenericDeviceDialog DialogRhd2000;

        /// <summary>Gets the <see cref="GenericDeviceDialog"/> for the Bno055.</summary>
        internal readonly GenericDeviceDialog DialogBno055;

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV2Rhd2000eHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureHeadstage">Configuration settings for a <see cref="ConfigureHeadstageNeuropixelsV2Rhd2000e"/>.</param>
        public NeuropixelsV2Rhd2000eHeadstageDialog(ConfigureHeadstageNeuropixelsV2Rhd2000e configureHeadstage)
        {
            Text = "HeadstageNeuropixelsV2/Rhd2000e Configuration";

            const string probeName = nameof(ConfigureHeadstageNeuropixelsV2Rhd2000e.NeuropixelsV2);
            AddProbeTab(probeName, new NeuropixelsV2eDialog(configureHeadstage.NeuropixelsV2, probeName, true),
                old => RecreateDialog(old));

            DialogRhd2000 = new GenericDeviceDialog(configureHeadstage.Rhd2000, true);
            AddDeviceTab("Rhd2000", DialogRhd2000);

            DialogBno055 = new GenericDeviceDialog(configureHeadstage.Bno055, true);
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
