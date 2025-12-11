using System;
using System.ComponentModel;
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
        /// <param name="device">Device that will be displayed in the property grid.</param>
        /// <param name="filterProperties">
        /// <see langword="true"/> if the properties should be filtered by <see cref="ShowInCustomDialogAttribute"/>,
        /// otherwise <see langword="false"/>. Default is <see langword="false"/>.
        /// </param>
        public GenericDeviceDialog(object device, bool filterProperties = false)
        {
            InitializeComponent();

            Shown += FormShown;

            if (filterProperties)
            {
                propertyGrid.BrowsableAttributes = new AttributeCollection(
                    new Attribute[]
                    {
                        new BrowsableAttribute(true),
                        new ShowInCustomDialogAttribute(true)
                    });
            }

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
