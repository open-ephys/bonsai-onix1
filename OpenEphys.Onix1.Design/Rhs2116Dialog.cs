using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a dialog for a <see cref="ConfigureRhs2116"/>.
    /// </summary>
    public partial class Rhs2116Dialog : GenericDeviceDialog
    {
        internal ConfigureRhs2116Pair ConfigureNode
        {
            get => (ConfigureRhs2116Pair)propertyGrid.SelectedObject;
            set => propertyGrid.SelectedObject = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rhs2116Dialog"/>.
        /// </summary>
        /// <param name="configureRhs2116">Existing <see cref="ConfigureRhs2116"/> device configuration.</param>
        public Rhs2116Dialog(ConfigureRhs2116Pair configureRhs2116)
        {
            InitializeComponent();
            Shown += FormShown;

            ConfigureNode = configureRhs2116;
        }

        private void FormShown(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                tableLayoutPanel1.Controls.Remove(flowLayoutPanel1);
                tableLayoutPanel1.RowCount = 1;

                MaximumSize = new System.Drawing.Size(0, 0);
            }
        }
    }
}
