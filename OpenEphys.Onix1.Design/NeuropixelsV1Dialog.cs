using System;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureNeuropixelsV1e"/>.
    /// </summary>
    public partial class NeuropixelsV1Dialog : Form
    {
        readonly NeuropixelsV1ProbeConfigurationDialog ProbeConfigurationDialog;

        /// <summary>
        /// Public <see cref="IConfigureNeuropixelsV1"/> interface that is manipulated by
        /// <see cref="NeuropixelsV1Dialog"/>.
        /// </summary>
        public IConfigureNeuropixelsV1 ConfigureNode { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV1Dialog"/>.
        /// </summary>
        /// <param name="configureNode">A <see cref="ConfigureNeuropixelsV1e"/> object holding the current configuration settings.</param>
        public NeuropixelsV1Dialog(IConfigureNeuropixelsV1 configureNode)
        {
            InitializeComponent();
            Shown += FormShown;

            if (configureNode is ConfigureNeuropixelsV1e configureV1e)
            {
                ConfigureNode = new ConfigureNeuropixelsV1e(configureV1e);
            }
            else if (configureNode is ConfigureNeuropixelsV1f configureV1f)
            {
                ConfigureNode = new ConfigureNeuropixelsV1f(configureV1f);
            }

            ProbeConfigurationDialog = new(ConfigureNode.ProbeConfiguration, ConfigureNode.AdcCalibrationFile, ConfigureNode.GainCalibrationFile)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this
            };

            panelProbe.Controls.Add(ProbeConfigurationDialog);

            this.AddMenuItemsFromDialogToFileOption(ProbeConfigurationDialog);
        }

        private void FormShown(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                tableLayoutPanel1.Controls.Remove(flowLayoutPanel1);
                tableLayoutPanel1.RowCount = 1;

                menuStrip.Visible = false;
            }

            ProbeConfigurationDialog.Show();
        }

        internal void Okay_Click(object sender, EventArgs e)
        {
            SaveVariables();

            DialogResult = DialogResult.OK;
        }

        internal void SaveVariables()
        {
            ConfigureNode.ProbeConfiguration = ProbeConfigurationDialog.ProbeConfiguration;

            ConfigureNode.GainCalibrationFile = ProbeConfigurationDialog.textBoxGainCalibrationFile.Text;
            ConfigureNode.AdcCalibrationFile = ProbeConfigurationDialog.textBoxAdcCalibrationFile.Text;
        }
    }
}
