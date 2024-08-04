using System;

namespace OpenEphys.Onix1.Design
{
    public partial class NeuropixelsV2eBno055Dialog : GenericDeviceDialog
    {
        public ConfigureNeuropixelsV2eBno055 ConfigureNode
        {
            get => (ConfigureNeuropixelsV2eBno055)propertyGrid.SelectedObject;
            set => propertyGrid.SelectedObject = value;
        }

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
