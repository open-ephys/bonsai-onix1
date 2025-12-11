using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureHeadstageNeuropixelsV2e"/>.
    /// </summary>
    public partial class NeuropixelsV2eHeadstageDialog : Form
    {
        /// <summary>
        /// Gets the <see cref="NeuropixelsV2eDialog"/>.
        /// </summary>
        public readonly NeuropixelsV2eDialog DialogNeuropixelsV2e;

        /// <summary>
        /// Gets the <see cref="GenericDeviceDialog"/> for the Bno055.
        /// </summary>
        public readonly GenericDeviceDialog DialogBno055;

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV2eHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureNeuropixelsV2e">Configuration settings for a <see cref="ConfigureNeuropixelsV2e"/>.</param>
        /// <param name="configureBno055">Configuration settings for a <see cref="ConfigurePolledBno055"/>.</param>
        public NeuropixelsV2eHeadstageDialog(IConfigureNeuropixelsV2 configureNeuropixelsV2e, ConfigurePolledBno055 configureBno055)
        {
            InitializeComponent();

            DialogNeuropixelsV2e = new(configureNeuropixelsV2e, true);

            DialogNeuropixelsV2e.SetChildFormProperties(this).AddDialogToPanel(panelNeuropixelsV2e);

            if (configureNeuropixelsV2e is ConfigureNeuropixelsV2e)
            {
                this.AddMenuItemsFromDialogToFileOption(DialogNeuropixelsV2e, "NeuropixelsV2e");
            }
            else if (configureNeuropixelsV2e is ConfigureNeuropixelsV2eBeta)
            {
                this.AddMenuItemsFromDialogToFileOption(DialogNeuropixelsV2e, "NeuropixelsV2eBeta");
                Text = Text.Replace("NeuropixelsV2e ", "NeuropixelsV2eBeta ");
                tabPageNeuropixelsV2e.Text = "NeuropixelsV2eBeta";
            }

            DialogBno055 = new(configureBno055, true);

            DialogBno055.SetChildFormProperties(this).AddDialogToPanel(panelBno055);
        }

        private void Okay_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
