using System;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureHeadstageRhs2116"/>.
    /// </summary>
    /// <remarks>
    /// Within the GUI, there are two tabs, 
    /// specifically for <see cref="ConfigureRhs2116"/> and for <see cref="ConfigureRhs2116Trigger"/>. 
    /// </remarks>
    public partial class HeadstageRhs2116Dialog : Form
    {
        internal readonly Rhs2116StimulusSequenceDialog StimulusSequenceDialog;
        internal readonly GenericDeviceDialog Rhs2116Dialog;

        /// <summary>
        /// Initializes a new instance of a <see cref="HeadstageRhs2116Dialog"/>.
        /// </summary>
        /// <param name="rhs2116Trigger">Current configuration settings for <see cref="ConfigureRhs2116Trigger"/>.</param>
        /// <param name="rhs2116">Current configuration settings for a single <see cref="ConfigureRhs2116"/>.</param>
        public HeadstageRhs2116Dialog(ConfigureRhs2116Trigger rhs2116Trigger, ConfigureRhs2116Pair rhs2116)
        {
            InitializeComponent();

            StimulusSequenceDialog = new Rhs2116StimulusSequenceDialog(rhs2116Trigger);

            StimulusSequenceDialog.SetChildFormProperties(this).AddDialogToTab(tabPageStimulusSequence);
            this.AddMenuItemsFromDialogToFileOption(StimulusSequenceDialog);

            Rhs2116Dialog = new(rhs2116);

            Rhs2116Dialog.SetChildFormProperties(this).AddDialogToTab(tabPageRhs2116);
        }

        void OnClickOK(object sender, EventArgs e)
        {
            if (StimulusSequenceDialog.CanCloseForm(out DialogResult result))
            {
                DialogResult = result;
                Close();
            }
        }
    }
}
