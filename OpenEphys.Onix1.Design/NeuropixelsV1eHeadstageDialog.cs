using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureNeuropixelsV1eHeadstage"/>.
    /// </summary>
    /// <remarks>
    /// Within the GUI, there is a tab for both devices encapsulated by a <see cref="ConfigureNeuropixelsV1eHeadstage"/>,
    /// specifically a <see cref="ConfigureNeuropixelsV1e"/> and a <see cref="ConfigurePolledBno055"/>. 
    /// </remarks>
    public partial class NeuropixelsV1eHeadstageDialog : Form
    {
        /// <summary>
        /// Public method that provides access to the <see cref="NeuropixelsV1Dialog"/>.
        /// </summary>
        public readonly NeuropixelsV1Dialog DialogNeuropixelsV1e;

        /// <summary>
        /// Public method that provides access to the <see cref="PolledBno055Dialog"/>.
        /// </summary>
        public readonly PolledBno055Dialog DialogBno055;

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV1eHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureNeuropixelsV1e">Configuration settings for a <see cref="ConfigureNeuropixelsV1e"/>.</param>
        /// <param name="configureBno055">Configuration settings for a <see cref="ConfigurePolledBno055"/>.</param>
        public NeuropixelsV1eHeadstageDialog(ConfigureNeuropixelsV1e configureNeuropixelsV1e, ConfigurePolledBno055 configureBno055)
        {
            InitializeComponent();

            DialogNeuropixelsV1e = new(configureNeuropixelsV1e)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this
            };

            panelNeuropixelsV1e.Controls.Add(DialogNeuropixelsV1e);
            this.AddMenuItemsFromDialogToFileOption(DialogNeuropixelsV1e, "NeuropixelsV1e");
            DialogNeuropixelsV1e.Show();

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
            DialogNeuropixelsV1e.SaveVariables();

            DialogResult = DialogResult.OK;
        }
    }
}
