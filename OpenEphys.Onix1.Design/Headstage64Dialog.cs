namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// GUI for <see cref="ConfigureHeadstage64"/>. Hosts two stimulus sequence dialogs
    /// (Electrical and Optical Stimulator) and three <see cref="GenericDeviceDialog"/> tabs
    /// (Rhd2164, Bno055, TS4231), each in its own tab.
    /// </summary>
    internal class Headstage64Dialog : HeadstageDialog
    {
        internal readonly GenericDeviceDialog Rhd2164Dialog;
        internal readonly GenericDeviceDialog Bno055Dialog;
        internal readonly GenericDeviceDialog TS4231V1Dialog;
        internal readonly Headstage64ElectricalStimulatorSequenceDialog ElectricalStimulatorSequenceDialog;
        internal readonly Headstage64OpticalStimulatorSequenceDialog OpticalStimulatorSequenceDialog;

        /// <summary>
        /// Initializes a new instance of a <see cref="Headstage64Dialog"/>.
        /// </summary>
        /// <param name="configureNode">Configure node for a Headstage 64.</param>
        public Headstage64Dialog(ConfigureHeadstage64 configureNode)
        {
            Text = "Headstage64 Configuration";

            ElectricalStimulatorSequenceDialog = new(configureNode.ElectricalStimulator, true);
            AddSequenceTab("Electrical Stimulator", ElectricalStimulatorSequenceDialog);

            OpticalStimulatorSequenceDialog = new(configureNode.OpticalStimulator, true);
            AddSequenceTab("Optical Stimulator", OpticalStimulatorSequenceDialog);

            Rhd2164Dialog = new(configureNode.Rhd2164, true);
            AddDeviceTab("Rhd2164", Rhd2164Dialog);

            Bno055Dialog = new(configureNode.Bno055, true);
            AddDeviceTab("Bno055", Bno055Dialog);

            TS4231V1Dialog = new(configureNode.TS4231, true);
            AddDeviceTab("TS4231", TS4231V1Dialog);
        }
    }
}
