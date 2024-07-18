using System.Windows.Forms;

namespace OpenEphys.Onix.Design
{
    public partial class NeuropixelsV2eHeadstageDialog : Form
    {
        public readonly NeuropixelsV2eDialog ConfigureNeuropixelsV2e;
        public readonly NeuropixelsV2eBno055Dialog ConfigureBno055;

        public NeuropixelsV2eHeadstageDialog(ConfigureNeuropixelsV2e configureNeuropixelsV2e, ConfigureNeuropixelsV2eBno055 configureBno055)
        {
            InitializeComponent();

            ConfigureNeuropixelsV2e = new(configureNeuropixelsV2e)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this
            };

            panelNeuropixelsV2e.Controls.Add(ConfigureNeuropixelsV2e);
            this.AddMenuItemsFromDialogToFileOption(ConfigureNeuropixelsV2e, "NeuropixelsV2e");
            ConfigureNeuropixelsV2e.Show();

            ConfigureBno055 = new(configureBno055)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this
            };

            panelBno055.Controls.Add(ConfigureBno055);
            ConfigureBno055.Show();
            ConfigureBno055.Invalidate();
        }

        private void ButtonClick(object sender, System.EventArgs e)
        {
            if (sender is Button button && button != null)
            {
                if (button.Name == nameof(buttonOkay))
                {
                    ConfigureNeuropixelsV2e.UpdateProbeGroups();

                    DialogResult = DialogResult.OK;
                }
            }
        }
    }
}
