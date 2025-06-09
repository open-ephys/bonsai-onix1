using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a channel configuration GUI for a <see cref="Headstage64Dialog"/>.
    /// </summary>
    public partial class Headstage64Dialog : Form
    {
        internal readonly Rhd2164Dialog Rhd2164Dialog;
        internal readonly Bno055Dialog Bno055Dialog;
        internal readonly TS4231V1Dialog TS4231V1Dialog;
        internal readonly Headstage64ElectricalStimulatorSequenceDialog ElectricalStimulatorSequenceDialog;
        internal readonly Headstage64OpticalStimulatorSequenceDialog OpticalStimulatorSequenceDialog;

        /// <summary>
        /// Initializes a new instance of a <see cref="Headstage64Dialog"/>.
        /// </summary>
        /// <param name="configureNode">Configure node for a Headstage 64.</param>
        public Headstage64Dialog(ConfigureHeadstage64 configureNode)
        {
            InitializeComponent();

            Rhd2164Dialog = new(configureNode.Rhd2164);
            Rhd2164Dialog.SetChildFormProperties(this).AddDialogToTab(tabPageRhd2164);

            Bno055Dialog = new(configureNode.Bno055);
            Bno055Dialog.SetChildFormProperties(this).AddDialogToTab(tabPageBno055);

            TS4231V1Dialog = new(configureNode.TS4231);
            TS4231V1Dialog.SetChildFormProperties(this).AddDialogToTab(tabPageTS4231);

            ElectricalStimulatorSequenceDialog = new(configureNode.ElectricalStimulator);
            ElectricalStimulatorSequenceDialog.SetChildFormProperties(this).AddDialogToTab(tabPageElectricalStimulator);

            OpticalStimulatorSequenceDialog = new(configureNode.OpticalStimulator);
            OpticalStimulatorSequenceDialog.SetChildFormProperties(this).AddDialogToTab(tabPageOpticalStimulator);

            this.AddMenuItemsFromDialogToFileOption(ElectricalStimulatorSequenceDialog)
                .AddMenuItemsFromDialogToFileOption(OpticalStimulatorSequenceDialog);
        }
    }
}
