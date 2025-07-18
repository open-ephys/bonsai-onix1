using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureHeadstageNeuropixelsV2e"/>.
    /// </summary>
    public partial class NeuropixelsV2eHeadstageDialog : Form
    {
        /// <summary>
        /// A <see cref="NeuropixelsV2eDialog"/> that configures a <see cref="ConfigureNeuropixelsV2e"/>.
        /// </summary>
        public readonly NeuropixelsV2eDialog DialogNeuropixelsV2e;

        /// <summary>
        /// A <see cref="PolledBno055Dialog"/> that configures a <see cref="ConfigurePolledBno055"/>.
        /// </summary>
        public readonly PolledBno055Dialog DialogBno055;

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV2eHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureNeuropixelsV2e">Configuration settings for a <see cref="ConfigureNeuropixelsV2e"/>.</param>
        /// <param name="configureBno055">Configuration settings for a <see cref="ConfigurePolledBno055"/>.</param>
        public NeuropixelsV2eHeadstageDialog(IConfigureNeuropixelsV2 configureNeuropixelsV2e, ConfigurePolledBno055 configureBno055)
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

        private void Okay_Click(object sender, System.EventArgs e)
        {
            DialogNeuropixelsV2e.SaveVariables();

            DialogResult = DialogResult.OK;
        }

        private void NeuropixelsV2eHeadstageDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogNeuropixelsV2e.TryRemoveEmptyFiles();
        }
    }
}
