using System;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureNeuropixelsV1e"/>.
    /// </summary>
    public partial class NeuropixelsV1Dialog : Form
    {
        internal readonly NeuropixelsV1ProbeConfigurationDialog ProbeConfigurationDialog;

        /// <summary>
        /// Public <see cref="IConfigureNeuropixelsV1"/> interface that is manipulated by
        /// <see cref="NeuropixelsV1Dialog"/>.
        /// </summary>
        [Obsolete]
        public IConfigureNeuropixelsV1 ConfigureNode { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV1Dialog"/>.
        /// </summary>
        /// <param name="configureNode">A <see cref="ConfigureNeuropixelsV1e"/> object holding the current configuration settings.</param>
        public NeuropixelsV1Dialog(IConfigureNeuropixelsV1 configureNode)
        {
            InitializeComponent();
            Shown += FormShown;

            ProbeConfigurationDialog = new(configureNode.ProbeConfiguration);
            ProbeConfigurationDialog
                .SetChildFormProperties(this)
                .AddDialogToPanel(panelProbe);

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
            DialogResult = DialogResult.OK;
        }
    }
}
