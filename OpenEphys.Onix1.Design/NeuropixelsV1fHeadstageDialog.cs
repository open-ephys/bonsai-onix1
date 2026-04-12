namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// GUI for <see cref="ConfigureHeadstageNeuropixelsV1f"/>. Hosts two
    /// <see cref="NeuropixelsV1Dialog"/> instances (ProbeA and ProbeB) and one
    /// <see cref="GenericDeviceDialog"/> for the Bno055, each in its own tab.
    /// </summary>
    public class NeuropixelsV1fHeadstageDialog : HeadstageDialog
    {
        /// <summary>Gets the <see cref="NeuropixelsV1Dialog"/> for ProbeA.</summary>
        internal NeuropixelsV1Dialog DialogNeuropixelsV1A =>
            (NeuropixelsV1Dialog)GetProbeDialog(0);

        /// <summary>Gets the <see cref="NeuropixelsV1Dialog"/> for ProbeB.</summary>
        internal NeuropixelsV1Dialog DialogNeuropixelsV1B =>
            (NeuropixelsV1Dialog)GetProbeDialog(1);

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

            AddProbeTab(nameA, new NeuropixelsV1Dialog(configureNeuropixelsV1A, nameA, true),
                old => RecreateDialog(old, nameA));

            AddProbeTab(nameB, new NeuropixelsV1Dialog(configureNeuropixelsV1B, nameB, true),
                old => RecreateDialog(old, nameB));

            DialogBno055 = new GenericDeviceDialog(configureBno055, true);
            AddDeviceTab("Bno055", DialogBno055);
        }

        static NeuropixelsV1Dialog RecreateDialog(NeuropixelsV1Dialog old, string probeName)
        {
            var newDialog = new NeuropixelsV1Dialog((ConfigureNeuropixelsV1f)old.ConfigureNode, probeName, true);
            newDialog.ProbeConfigurationDialog.ChannelConfiguration.ProbeGroup =
                old.ProbeConfigurationDialog.ChannelConfiguration.ProbeGroup;
            newDialog.ProbeConfigurationDialog.ChannelConfiguration.RedrawProbeGroup();
            newDialog.ProbeConfigurationDialog.CheckForExistingChannelPreset();
            return newDialog;
        }
    }
}
