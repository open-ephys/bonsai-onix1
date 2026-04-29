using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureHeadstageNeuropixelsV1e"/>.
    /// </summary>
    /// <remarks>
    /// Within the GUI, there is a tab for both devices encapsulated by a <see cref="ConfigureHeadstageNeuropixelsV1e"/>,
    /// specifically a <see cref="ConfigureNeuropixelsV1PsbDecoder"/> and a <see cref="ConfigurePolledBno055"/>. 
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
        /// <param name="configureHeadstage">Configuration settings for a <see cref="ConfigureHeadstageNeuropixelsV1e"/>.</param>
        public NeuropixelsV1eHeadstageDialog(ConfigureHeadstageNeuropixelsV1e configureHeadstage)
        {
            InitializeComponent();

            DialogNeuropixelsV1e = new(configureHeadstage.NeuropixelsV1, nameof(NeuropixelsV1), true);
            DialogNeuropixelsV1e.SetChildFormProperties(this).AddDialogToPanel(panelNeuropixelsV1e);

            DialogNeuropixelsV1e.OnStateChange += (sender, e) =>
            {
                if (DialogNeuropixelsV1e.HasChanges)
                {
                    tabPageNeuropixelsV1e.Text += '*';
                }
                else
                {
                    tabPageNeuropixelsV1e.Text = tabPageNeuropixelsV1e.Text.TrimEnd('*');
                }
            };

            DialogBno055 = new(configureHeadstage.Bno055, true);

            DialogBno055.SetChildFormProperties(this).AddDialogToPanel(panelBno055);

            FormClosing += DialogClosing;
        }

        private void Okay_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <inheritdoc/>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (tabControl1.SelectedTab == tabPageNeuropixelsV1e)
            {
                return DialogNeuropixelsV1e.ProcessMenuShortcut(keyData);
            }
            else if (tabControl1.SelectedTab == tabPageBno055)
            {
                return true;
            }

            return false;
        }

        void DialogClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogNeuropixelsV1e.HasChanges && this.HandleTopLevelDialogCancel(ref e, ChannelConfigurationDialog.ProbeConfigurationConfirmMessage))
            {
                return;
            }

            DialogNeuropixelsV1e.CloseWithResult(this);

            if (!DialogNeuropixelsV1e.IsDisposed)
            {
                e.Cancel = true;
                return;
            }
        }
    }
}
