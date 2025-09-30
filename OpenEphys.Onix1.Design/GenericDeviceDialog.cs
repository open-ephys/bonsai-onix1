using System;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Abstract form that implements a very basic GUI consisting of a single property grid and
    /// two buttons (OK / Cancel).
    /// </summary>
    public partial class GenericDeviceDialog : Form
    {
        internal object Device
        {
            get => propertyGrid.SelectedObject;
            set => propertyGrid.SelectedObject = value;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="GenericDeviceDialog"/>.
        /// </summary>
        public GenericDeviceDialog(object device)
        {
            InitializeComponent();

            Shown += FormShown;
            Device = device;
        }

        void FormShown(object sender, EventArgs e)
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
