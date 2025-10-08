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

        internal Rhs2116ProbeGroup ProbeGroup;

        /// <summary>
        /// Initializes a new instance of a <see cref="HeadstageRhs2116Dialog"/>.
        /// </summary>
        /// <param name="probeGroup">Current channel configuration settings for a <see cref="Rhs2116ProbeGroup"/>.</param>
        /// <param name="sequence">Current stimulus sequence for a <see cref="Rhs2116StimulusSequencePair"/>.</param>
        /// <param name="rhs2116">Current configuration settings for a single <see cref="ConfigureRhs2116"/>.</param>
        public HeadstageRhs2116Dialog(Rhs2116ProbeGroup probeGroup, Rhs2116StimulusSequencePair sequence,
            ConfigureRhs2116Pair rhs2116)
        {
            InitializeComponent();

            ProbeGroup = new Rhs2116ProbeGroup(probeGroup);

            StimulusSequenceDialog = new Rhs2116StimulusSequenceDialog(sequence, ProbeGroup);

            StimulusSequenceDialog.SetChildFormProperties(this).AddDialogToTab(tabPageStimulusSequence);
            this.AddMenuItemsFromDialogToFileOption(StimulusSequenceDialog);

            Rhs2116Dialog = new(new ConfigureRhs2116Pair(rhs2116));

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
