using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureNeuropixelsV2eBno055"/>.
    /// </summary>
    public partial class NeuropixelsV2eBno055Dialog : GenericDeviceDialog
    {
        /// <summary>
        /// Gets or sets the <see cref="ConfigureNeuropixelsV2eBno055"/> object attached to
        /// the property grid.
        /// </summary>
        public ConfigureNeuropixelsV2eBno055 ConfigureNode
        {
            get => (ConfigureNeuropixelsV2eBno055)propertyGrid.SelectedObject;
            set => propertyGrid.SelectedObject = value;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV2eBno055Dialog"/> with the given
        /// <see cref="ConfigureNeuropixelsV2eBno055"/> object.
        /// </summary>
        /// <param name="configureNode">A <see cref="ConfigureNeuropixelsV2eBno055"/> object that contains configuration settings</param>
        public NeuropixelsV2eBno055Dialog(ConfigureNeuropixelsV2eBno055 configureNode)
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
