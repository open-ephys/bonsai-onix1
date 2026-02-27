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
        /// <param name="probeName">The name of the probe.</param>
        public NeuropixelsV1Dialog(IConfigureNeuropixelsV1 configureNode, string probeName)
        {
            InitializeComponent();
            Shown += FormShown;
            FormClosing += DialogClosing;

            ProbeConfigurationDialog = new(configureNode.ProbeConfiguration, probeName);
            ProbeConfigurationDialog
                .SetChildFormProperties(this)
                .AddDialogToPanel(panelProbe);
        }

        private void FormShown(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                tableLayoutPanel1.Controls.Remove(flowLayoutPanel1);
                tableLayoutPanel1.RowCount = 1;
            }

            ProbeConfigurationDialog.Show();
        }

        internal bool ProcessMenuShortcut(Keys keyData)
        {
            return ProbeConfigurationDialog.ProcessMenuShortcut(keyData);
        }

        internal void Okay_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        void DialogClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.Cancel)
                return;

            ProbeConfigurationDialog.Close();
        }
    }
}
