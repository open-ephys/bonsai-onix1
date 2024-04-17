using System;
using System.Windows.Forms;

namespace OpenEphys.Onix.Design
{
    public partial class Rhs2116StimulusSequenceDialog : Form
    {
        public Rhs2116StimulusSequence Sequence;

        public Rhs2116StimulusSequenceDialog(Rhs2116StimulusSequence sequence)
        {
            InitializeComponent();

            Sequence = ObjectExtensions.Copy(sequence);

            propertyGrid1.SelectedObject = Sequence;
        }

        private void linkLabel_Documentation_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://open-ephys.github.io/onix-docs/Software%20Guide/Bonsai.ONIX/Nodes/RHS2116TriggerDevice.html");
            }
            catch 
            {
                MessageBox.Show("Unable to open documentation link.");
            }
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            //TODO: Clean everything up here, don't commit anything to the device, close the window
            Close();
        }
    }
}
