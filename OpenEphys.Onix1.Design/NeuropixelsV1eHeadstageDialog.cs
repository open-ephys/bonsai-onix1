using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureHeadstageNeuropixelsV1e"/>.
    /// </summary>
    /// <remarks>
    /// Within the GUI, there is a tab for both devices encapsulated by a <see cref="ConfigureHeadstageNeuropixelsV1e"/>,
    /// specifically a <see cref="ConfigureNeuropixelsV1e"/> and a <see cref="ConfigurePolledBno055"/>. 
    /// </remarks>
    public partial class NeuropixelsV1eHeadstageDialog : Form
    {
        /// <summary>
        /// Gets the <see cref="NeuropixelsV1Dialog"/>.
        /// </summary>
        public readonly NeuropixelsV1Dialog DialogNeuropixelsV1e;

        /// <summary>
        /// Gets the <see cref="GenericDeviceDialog"/> for the Bno055.
        /// </summary>
        public readonly GenericDeviceDialog DialogBno055;

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV1eHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureNeuropixelsV1e">Configuration settings for a <see cref="ConfigureNeuropixelsV1e"/>.</param>
        /// <param name="configureBno055">Configuration settings for a <see cref="ConfigurePolledBno055"/>.</param>
        public NeuropixelsV1eHeadstageDialog(ConfigureNeuropixelsV1e configureNeuropixelsV1e, ConfigurePolledBno055 configureBno055)
        {
            InitializeComponent();

            DialogNeuropixelsV1e = new(configureNeuropixelsV1e);

            DialogNeuropixelsV1e.SetChildFormProperties(this).AddDialogToPanel(panelNeuropixelsV1e);

            this.AddMenuItemsFromDialogToFileOption(DialogNeuropixelsV1e, "NeuropixelsV1e");

            DialogBno055 = new(new ConfigurePolledBno055(configureBno055));

            DialogBno055.SetChildFormProperties(this).AddDialogToPanel(panelBno055);

            FormClosing += DialogClosing;
        }

        private void Okay_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        void DialogClosing(object sender, FormClosingEventArgs e)
        {
            DialogNeuropixelsV1e.Close();
        }
    }
}
