using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Simple dialog that attaches the <see cref="ConfigureBno055"/> to a property grid.
    /// </summary>
    public partial class Bno055Dialog : GenericDeviceDialog
    {
        /// <summary>
        /// Gets or sets the <see cref="ConfigureBno055"/>, allowing for changes made in the dialog to be reflected in the main editor.
        /// </summary>
        public ConfigureBno055 ConfigureNode
        {
            get => (ConfigureBno055)propertyGrid.SelectedObject;
            set => propertyGrid.SelectedObject = value;
        }

        /// <summary>
        /// Initializes a new dialog for the <see cref="ConfigureBno055"/> operator.
        /// </summary>
        /// <param name="configureNode"></param>
        public Bno055Dialog(ConfigureBno055 configureNode)
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
