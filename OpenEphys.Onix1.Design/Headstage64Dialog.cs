using System;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a channel configuration GUI for a <see cref="Headstage64Dialog"/>.
    /// </summary>
    public partial class Headstage64Dialog : Form
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
            InitializeComponent();

            Rhd2164Dialog = new(new ConfigureRhd2164(configureNode.Rhd2164));
            Rhd2164Dialog.SetChildFormProperties(this).AddDialogToTab(tabPageRhd2164);

            Bno055Dialog = new(new ConfigureBno055(configureNode.Bno055));
            Bno055Dialog.SetChildFormProperties(this).AddDialogToTab(tabPageBno055);

            TS4231V1Dialog = new(new ConfigureTS4231V1(configureNode.TS4231));
            TS4231V1Dialog.SetChildFormProperties(this).AddDialogToTab(tabPageTS4231);

            ElectricalStimulatorSequenceDialog = new(configureNode.ElectricalStimulator);
            ElectricalStimulatorSequenceDialog.SetChildFormProperties(this).AddDialogToTab(tabPageElectricalStimulator);

            OpticalStimulatorSequenceDialog = new(configureNode.OpticalStimulator);
            OpticalStimulatorSequenceDialog.SetChildFormProperties(this).AddDialogToTab(tabPageOpticalStimulator);

            menuStrip1.Visible = false;
        }

        void OnClickOk(object sender, EventArgs e)
        {
            if (ElectricalStimulatorSequenceDialog.CanCloseForm(out DialogResult electricalResult, "Electrical Stimulator")
                && OpticalStimulatorSequenceDialog.CanCloseForm(out DialogResult opticalResult, "Optical Stimulator"))
            {
                if (electricalResult == DialogResult.OK || opticalResult == DialogResult.OK)
                    DialogResult = DialogResult.OK;
                else
                    DialogResult = DialogResult.Cancel;

                Close();
            }
        }
    }
}
