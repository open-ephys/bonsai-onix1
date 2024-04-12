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
    }
}
