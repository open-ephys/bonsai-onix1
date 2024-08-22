using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureNeuropixelsV1eBno055"/>.
    /// </summary>
    public partial class NeuropixelsV1eBno055Dialog : GenericDeviceDialog
    {
        /// <summary>
        /// Gets or sets the <see cref="ConfigureNeuropixelsV1eBno055"/> object attached to
        /// the property grid.
        /// </summary>
        public ConfigureNeuropixelsV1eBno055 ConfigureNode
        {
            get => (ConfigureNeuropixelsV1eBno055)propertyGrid.SelectedObject;
            set => propertyGrid.SelectedObject = value;
        }

        /// <summary>
        /// Initializes a new <see cref="NeuropixelsV1eBno055Dialog"/> instance with the given
        /// <see cref="ConfigureNeuropixelsV1eBno055"/> object.
        /// </summary>
        /// <param name="configureNode">A <see cref="ConfigureNeuropixelsV1eBno055"/> object that contains configuration settings.</param>
        public NeuropixelsV1eBno055Dialog(ConfigureNeuropixelsV1eBno055 configureNode)
        {
            InitializeComponent();
            Shown += FormShown;

            ConfigureNode = new(configureNode);
        }

        private void FormShown(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                splitContainer1.Panel2Collapsed = true;
                splitContainer1.Panel2.Hide();
            }
        }
    }
}
