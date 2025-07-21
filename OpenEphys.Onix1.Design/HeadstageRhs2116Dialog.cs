using System;
using System.Windows.Forms;
using OpenEphys.ProbeInterface.NET;

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
        internal readonly Rhs2116Dialog Rhs2116Dialog;

        /// <summary>
        /// Initializes a new instance of a <see cref="HeadstageRhs2116Dialog"/>.
        /// </summary>
        /// <param name="probeGroup">Current channel configuration settings for a <see cref="Rhs2116ProbeGroup"/>.</param>
        /// <param name="sequence">Current stimulus sequence for a <see cref="Rhs2116StimulusSequencePair"/>.</param>
        /// <param name="rhs2116">Current configuration settings for a single <see cref="ConfigureRhs2116"/>.</param>
        [Obsolete("This constructor is now deprecated, as Probe Groups are now held in externalized files.")]
        public HeadstageRhs2116Dialog(Rhs2116ProbeGroup probeGroup, Rhs2116StimulusSequencePair sequence,
            ConfigureRhs2116Pair rhs2116) : this("", sequence, rhs2116)
        {
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="HeadstageRhs2116Dialog"/>.
        /// </summary>
        /// <param name="probeInterfaceFile">Filepath to the location where the Probe Interface file is saved.</param>
        /// <param name="sequence">Stimulus sequence object containing all stimulation parameters.</param>
        /// <param name="rhs2116">Current configuration settings for the Rhs2116 device.</param>
        public HeadstageRhs2116Dialog(string probeInterfaceFile, Rhs2116StimulusSequencePair sequence,
            ConfigureRhs2116Pair rhs2116)
        {
            InitializeComponent();

            StimulusSequenceDialog = new Rhs2116StimulusSequenceDialog(sequence, probeInterfaceFile)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this,
            };

            tabPageStimulusSequence.Controls.Add(StimulusSequenceDialog);
            this.AddMenuItemsFromDialogToFileOption(StimulusSequenceDialog);

            StimulusSequenceDialog.Show();

            Rhs2116Dialog = new Rhs2116Dialog(rhs2116)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this,
            };

            tabPageRhs2116A.Controls.Add(Rhs2116Dialog);
            Rhs2116Dialog.Show();
        }

        private void OnClickOK(object sender, EventArgs e)
        {
            if (Rhs2116StimulusSequenceDialog.CanCloseForm(StimulusSequenceDialog.Sequence, out DialogResult result))
            {
                DialogResult = result;
                Close();
            }
        }

        private void HeadstageRhs2116Dialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            ChannelConfigurationDialog.TryRemoveEmptyFile(StimulusSequenceDialog.ChannelDialog.ProbeInterfaceFile);
        }
    }
}
