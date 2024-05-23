using System;
using System.Windows.Forms;

namespace OpenEphys.Onix.Design
{
    public partial class HeadstageRhs2116Dialog : Form
    {
        public Rhs2116ProbeGroup ChannelConfiguration;

        public HeadstageRhs2116Dialog(Rhs2116ProbeGroup channelConfiguration, Rhs2116StimulusSequence sequence)
        {
            InitializeComponent();

            ChannelConfiguration = channelConfiguration;

            var channelConfigurationDialog = new ChannelConfigurationDialog(ChannelConfiguration)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this,
            };

            tabPageChannelConfiguration.Controls.Add(channelConfigurationDialog);

            channelConfigurationDialog.Show();

            var stimulusSequenceDialog = new Rhs2116StimulusSequenceDialog(sequence, channelConfiguration)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this,
                Tag = nameof(Rhs2116StimulusSequenceDialog)
            };

            tabPageStimulusSequence.Controls.Add(stimulusSequenceDialog);

            stimulusSequenceDialog.Show();
        }

        private void OnClickOK(object sender, EventArgs e)
        {
            var stimSequenceDialog = (Rhs2116StimulusSequenceDialog)tabPageStimulusSequence.Controls[nameof(Rhs2116StimulusSequenceDialog)];

            if (Rhs2116StimulusSequenceDialog.CanCloseForm(stimSequenceDialog.Sequence, out DialogResult result))
            {
                DialogResult = result;
                Close();
            }
        }

        private void OnClickCancel(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
