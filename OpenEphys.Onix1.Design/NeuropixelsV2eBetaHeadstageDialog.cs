using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureNeuropixelsV2eHeadstage"/>.
    /// </summary>
    public partial class NeuropixelsV2eBetaHeadstageDialog : Form
    {
        /// <summary>
        /// A <see cref="NeuropixelsV2eBetaDialog"/> that configures a <see cref="ConfigureNeuropixelsV2eBeta"/>.
        /// </summary>
        public readonly NeuropixelsV2eBetaDialog DialogNeuropixelsV2e;

        /// <summary>
        /// A <see cref="NeuropixelsV2eBno055Dialog"/> that configures a <see cref="ConfigureNeuropixelsV2eBno055"/>.
        /// </summary>
        public readonly NeuropixelsV2eBno055Dialog DialogBno055;

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV2eBetaHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureNeuropixelsV2eBeta">Configuration settings for a <see cref="ConfigureNeuropixelsV2eBeta"/>.</param>
        /// <param name="configureBno055">Configuration settings for a <see cref="ConfigureNeuropixelsV2eBno055"/>.</param>
        public NeuropixelsV2eBetaHeadstageDialog(ConfigureNeuropixelsV2eBeta configureNeuropixelsV2eBeta, ConfigureNeuropixelsV2eBno055 configureBno055)
        {
            InitializeComponent();

            DialogNeuropixelsV2e = new(configureNeuropixelsV2eBeta)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this
            };

            panelNeuropixelsV2e.Controls.Add(DialogNeuropixelsV2e);
            this.AddMenuItemsFromDialogToFileOption(DialogNeuropixelsV2e, "NeuropixelsV2eBeta");
            DialogNeuropixelsV2e.Show();

            DialogBno055 = new(configureBno055)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this
            };

            panelBno055.Controls.Add(DialogBno055);
            DialogBno055.Show();
            DialogBno055.Invalidate();
        }

        private void ButtonClick(object sender, System.EventArgs e)
        {
            if (sender is Button button && button != null)
            {
                if (button.Name == nameof(buttonOkay))
                {
                    DialogNeuropixelsV2e.SaveVariables();

                    DialogResult = DialogResult.OK;
                }
            }
        }
    }
}
