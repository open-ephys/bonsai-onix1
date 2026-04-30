namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// GUI for <see cref="ConfigureHeadstageNeuropixelsV1e"/>. Hosts one
    /// <see cref="NeuropixelsV1Dialog"/> and one <see cref="GenericDeviceDialog"/> for the Bno055,
    /// each in its own tab.
    /// </summary>
    internal class NeuropixelsV1eHeadstageDialog : HeadstageDialog
    {
        /// <summary>Gets the <see cref="NeuropixelsV1Dialog"/>.</summary>
        public NeuropixelsV1Dialog DialogNeuropixelsV1e =>
            (NeuropixelsV1Dialog)GetProbeDialog(0);

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
            var probeDialog = new NeuropixelsV1Dialog(configureHeadstage.NeuropixelsV1, probeName, true);
            AddProbeTab(probeName, probeDialog, old => RecreateDialog(old, probeName));

            DialogBno055 = new GenericDeviceDialog(configureHeadstage.Bno055, true);
            AddDeviceTab("Bno055", DialogBno055);
        }

        static NeuropixelsV1Dialog RecreateDialog(NeuropixelsV1Dialog old, string probeName)
        {
            var newDialog = new NeuropixelsV1Dialog((IConfigureNeuropixelsV1)old.ConfigureNode, probeName, true);
            newDialog.ProbeConfigurationDialog.ChannelConfiguration.ProbeGroup =
                old.ProbeConfigurationDialog.ChannelConfiguration.ProbeGroup;
            newDialog.ProbeConfigurationDialog.ChannelConfiguration.RedrawProbeGroup();
            newDialog.ProbeConfigurationDialog.CheckForExistingChannelPreset();
            return newDialog;
        }
    }
}
