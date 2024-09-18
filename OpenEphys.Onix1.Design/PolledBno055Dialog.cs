using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigurePolledBno055"/>.
    /// </summary>
    public partial class PolledBno055Dialog : GenericDeviceDialog
    {
        /// <summary>
        /// Gets or sets the <see cref="ConfigurePolledBno055"/> object attached to
        /// the property grid.
        /// </summary>
        public ConfigurePolledBno055 ConfigureNode
        {
            get => (ConfigurePolledBno055)propertyGrid.SelectedObject;
            set => propertyGrid.SelectedObject = value;
        }

        /// <summary>
        /// Initializes a new <see cref="PolledBno055Dialog"/> instance with the given
        /// <see cref="ConfigurePolledBno055"/> object.
        /// </summary>
        /// <param name="configureNode">A <see cref="ConfigurePolledBno055"/> object that contains configuration settings.</param>
        public PolledBno055Dialog(ConfigurePolledBno055 configureNode)
        {
            InitializeComponent();
            Shown += FormShown;

            ConfigureNode = new(configureNode);
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
