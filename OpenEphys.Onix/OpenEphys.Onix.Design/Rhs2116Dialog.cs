using System;
using System.Windows.Forms;

namespace OpenEphys.Onix.Design
{
    public partial class Rhs2116Dialog : Form
    {
        public ConfigureRhs2116 Rhs2116;

        public Rhs2116Dialog(ConfigureRhs2116 rhs2116)
        {
            InitializeComponent();
            Shown += FormShown;

            Rhs2116 = new ConfigureRhs2116(rhs2116);

            propertyGrid.SelectedObject = Rhs2116;
        }

        private void FormShown(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                splitContainer1.Panel2Collapsed = true;
                splitContainer1.Panel2.Hide();
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (TopLevel)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
