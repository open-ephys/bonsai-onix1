using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureHeadstageNeuropixelsV1f"/>.
    /// </summary>
    /// <remarks>
    /// Within the GUI, there is a tab for all devices encapsulated by a <see cref="ConfigureHeadstageNeuropixelsV1f"/>,
    /// specifically two <see cref="ConfigureNeuropixelsV1f"/> devices and one <see cref="ConfigureBno055"/>. 
    /// </remarks>
    public partial class NeuropixelsV1fHeadstageDialog : Form
    {
        /// <summary>
        /// Gets the <see cref="NeuropixelsV1Dialog"/> for ProbeA.
        /// </summary>
        public readonly NeuropixelsV1Dialog DialogNeuropixelsV1A;

        /// <summary>
        /// Gets the <see cref="NeuropixelsV1Dialog"/> for ProbeB.
        /// </summary>
        public readonly NeuropixelsV1Dialog DialogNeuropixelsV1B;

        /// <summary>
        /// Gets the <see cref="GenericDeviceDialog"/> for the Bno055.
        /// </summary>
        public readonly GenericDeviceDialog DialogBno055;

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV1fHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureNeuropixelsV1A">Configuration settings for <see cref="ConfigureNeuropixelsV1f"/> A.</param>
        /// <param name="configureNeuropixelsV1B">Configuration settings for <see cref="ConfigureNeuropixelsV1f"/> B</param>
        /// <param name="configureBno055">Configuration settings for a <see cref="PolledBno055"/>.</param>
        public NeuropixelsV1fHeadstageDialog(ConfigureNeuropixelsV1f configureNeuropixelsV1A, ConfigureNeuropixelsV1f configureNeuropixelsV1B, ConfigureBno055 configureBno055)
        {
            InitializeComponent();

            DialogNeuropixelsV1A = new(configureNeuropixelsV1A)
            {
                Tag = configureNeuropixelsV1A.ProbeName
            };

            DialogNeuropixelsV1A.SetChildFormProperties(this).AddDialogToPanel(panelNeuropixelsV1A);

            this.AddMenuItemsFromDialogToFileOption(DialogNeuropixelsV1A, "NeuropixelsV1A");

            DialogNeuropixelsV1B = new(configureNeuropixelsV1B)
            {
                Tag = configureNeuropixelsV1B.ProbeName
            };

            DialogNeuropixelsV1B.SetChildFormProperties(this).AddDialogToPanel(panelNeuropixelsV1B);

            this.AddMenuItemsFromDialogToFileOption(DialogNeuropixelsV1B, "NeuropixelsV1B");

            DialogBno055 = new(configureBno055);

            DialogBno055.SetChildFormProperties(this).AddDialogToPanel(panelBno055);
        }

        private void Okay_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
