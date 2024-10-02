using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureNeuropixelsV1fHeadstage"/>.
    /// </summary>
    /// <remarks>
    /// Within the GUI, there is a tab for all devices encapsulated by a <see cref="ConfigureNeuropixelsV1fHeadstage"/>,
    /// specifically two <see cref="ConfigureNeuropixelsV1f"/> devices and one <see cref="ConfigureBno055"/>. 
    /// </remarks>
    public partial class NeuropixelsV1fHeadstageDialog : Form
    {
        /// <summary>
        /// Public method that provides access to <see cref="NeuropixelsV1Dialog"/> A.
        /// </summary>
        public readonly NeuropixelsV1Dialog DialogNeuropixelsV1A;

        /// <summary>
        /// Public method that provides access to <see cref="NeuropixelsV1Dialog"/> B.
        /// </summary>
        public readonly NeuropixelsV1Dialog DialogNeuropixelsV1B;

        /// <summary>
        /// Public method that provides access to the <see cref="Bno055Dialog"/>.
        /// </summary>
        public readonly Bno055Dialog DialogBno055;

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
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this,
                Tag = configureNeuropixelsV1A.ProbeName
            };

            panelNeuropixelsV1A.Controls.Add(DialogNeuropixelsV1A);
            this.AddMenuItemsFromDialogToFileOption(DialogNeuropixelsV1A, "NeuropixelsV1A");
            DialogNeuropixelsV1A.Show();

            DialogNeuropixelsV1B = new(configureNeuropixelsV1B)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this,
                Tag = configureNeuropixelsV1B.ProbeName
            };

            panelNeuropixelsV1B.Controls.Add(DialogNeuropixelsV1B);
            this.AddMenuItemsFromDialogToFileOption(DialogNeuropixelsV1B, "NeuropixelsV1B");
            DialogNeuropixelsV1B.Show();

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
            DialogNeuropixelsV1A.SaveVariables();
            DialogNeuropixelsV1B.SaveVariables();

            DialogResult = DialogResult.OK;
        }
    }
}
