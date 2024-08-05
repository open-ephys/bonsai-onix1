using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureNeuropixelsV2eHeadstage"/>.
    /// </summary>
    public partial class NeuropixelsV2eHeadstageDialog : Form
    {
        /// <summary>
        /// A <see cref="NeuropixelsV2eDialog"/> that configures a <see cref="ConfigureNeuropixelsV2e"/>.
        /// </summary>
        public readonly NeuropixelsV2eDialog DialogNeuropixelsV2e;

        /// <summary>
        /// A <see cref="NeuropixelsV2eBno055Dialog"/> that configures a <see cref="ConfigureNeuropixelsV2eBno055"/>.
        /// </summary>
        public readonly NeuropixelsV2eBno055Dialog ConfigureBno055;

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV2eHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureNeuropixelsV2e">Configuration settings for a <see cref="ConfigureNeuropixelsV2e"/>.</param>
        /// <param name="configureBno055">Configuration settings for a <see cref="ConfigureNeuropixelsV2eBno055"/>.</param>
        public NeuropixelsV2eHeadstageDialog(ConfigureNeuropixelsV2e configureNeuropixelsV2e, ConfigureNeuropixelsV2eBno055 configureBno055)
        {
            InitializeComponent();

            DialogNeuropixelsV2e = new(configureNeuropixelsV2e)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this
            };

            panelNeuropixelsV2e.Controls.Add(DialogNeuropixelsV2e);
            this.AddMenuItemsFromDialogToFileOption(DialogNeuropixelsV2e, "NeuropixelsV2e");
            DialogNeuropixelsV2e.Show();

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
                    DialogNeuropixelsV2e.UpdateProbeGroups();

                    DialogResult = DialogResult.OK;
                }
            }
        }
    }
}
