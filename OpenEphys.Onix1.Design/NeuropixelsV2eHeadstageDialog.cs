using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureHeadstageNeuropixelsV2e"/>.
    /// </summary>
    public partial class NeuropixelsV2eHeadstageDialog : Form
    {
        internal NeuropixelsV2eDialog DialogNeuropixelsV2A { get; private set; }
        internal NeuropixelsV2eDialog DialogNeuropixelsV2B { get; private set; }

        internal readonly GenericDeviceDialog DialogBno055;

        bool HasChanges => DialogNeuropixelsV2A.HasChanges || DialogNeuropixelsV2B.HasChanges;

        NeuropixelsV2eHeadstageDialog(ConfigurePolledBno055 bno055)
        {
            InitializeComponent();
            FormClosing += DialogClosing;

            DialogBno055 = new(bno055, true);
            DialogBno055.SetChildFormProperties(this).AddDialogToPanel(panelBno055);
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV2eHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureHeadstage">Configuration settings for a <see cref="ConfigureHeadstageNeuropixelsV2e"/>.</param>
        public NeuropixelsV2eHeadstageDialog(ConfigureHeadstageNeuropixelsV2e configureHeadstage)
            : this(configureHeadstage.Bno055)
        {
            Text = "HeadstageNeuropixels2.0 Configuration";

            DialogNeuropixelsV2A = CreateNeuropixelsV2Dialog(this, configureHeadstage.NeuropixelsV2A, nameof(configureHeadstage.NeuropixelsV2A), panelNeuropixelsV2A, tabPageNeuropixelsV2A);

            DialogNeuropixelsV2B = CreateNeuropixelsV2Dialog(this, configureHeadstage.NeuropixelsV2B, nameof(configureHeadstage.NeuropixelsV2B), panelNeuropixelsV2B, tabPageNeuropixelsV2B);
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV2eHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureHeadstage">Configuration settings for a <see cref="ConfigureHeadstageNeuropixelsV2eBeta"/>.</param>
        public NeuropixelsV2eHeadstageDialog(ConfigureHeadstageNeuropixelsV2eBeta configureHeadstage)
            : this(configureHeadstage.Bno055)
        {
            Text = "HeadstageNeuropixels2.0-Beta Configuration";

            DialogNeuropixelsV2A = CreateNeuropixelsV2Dialog(this, configureHeadstage.NeuropixelsV2A, nameof(configureHeadstage.NeuropixelsV2A), panelNeuropixelsV2A, tabPageNeuropixelsV2A);

            DialogNeuropixelsV2B = CreateNeuropixelsV2Dialog(this, configureHeadstage.NeuropixelsV2B, nameof(configureHeadstage.NeuropixelsV2B), panelNeuropixelsV2B, tabPageNeuropixelsV2B);
        }

        static NeuropixelsV2eDialog CreateNeuropixelsV2Dialog(NeuropixelsV2eHeadstageDialog headstageDialog, IConfigureNeuropixelsV2 configureNeuropixelsV2, string probeName, Panel panel, TabPage tabPage)
        {
            var dialog = new NeuropixelsV2eDialog(configureNeuropixelsV2, probeName, true);

            dialog.SetChildFormProperties(headstageDialog).AddDialogToPanel(panel);

            dialog.OnStateChange += (sender, e) =>
            {
                if (dialog.HasChanges)
                {
                    if (!tabPage.Text.Contains("*"))
                    {
                        tabPage.Text += '*';
                    }
                }
                else
                {
                    tabPage.Text = tabPage.Text.TrimEnd('*');
                }
            };

            tabPage.Text = probeName;

            return dialog;
        }

        private void Okay_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        /// <inheritdoc/>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (tabControl1.SelectedTab == tabPageNeuropixelsV2A)
            {
                return DialogNeuropixelsV2A.ProcessMenuShortcut(keyData);
            }
            else if (tabControl1.SelectedTab == tabPageNeuropixelsV2B)
            {
                return DialogNeuropixelsV2B.ProcessMenuShortcut(keyData);
            }
            else if (tabControl1.SelectedTab == tabPageBno055)
            {
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        void DialogClosing(object sender, FormClosingEventArgs e)
        {
            if (HasChanges && this.HandleTopLevelDialogCancel(ref e, ChannelConfigurationDialog.ProbeConfigurationConfirmMessage))
            {
                return;
            }

            DialogNeuropixelsV2A.CloseWithResult(this);

            if (!DialogNeuropixelsV2A.IsDisposed)
            {
                e.Cancel = true;
                return;
            }

            DialogNeuropixelsV2B.CloseWithResult(this);

            if (!DialogNeuropixelsV2B.IsDisposed)
            {
                e.Cancel = true;

                // NB: Initialize the previously disposed dialog when the user cancels closing the dialog
                var dialog = DialogNeuropixelsV2A;
                panelNeuropixelsV2A.Controls.Remove(DialogNeuropixelsV2A);
                DialogNeuropixelsV2A = CreateNeuropixelsV2Dialog(this, DialogNeuropixelsV2A.ConfigureNeuropixelsV2, DialogNeuropixelsV2A.ProbeConfigurationDialog.ProbeName, panelNeuropixelsV2A, tabPageNeuropixelsV2A);
                DialogNeuropixelsV2A.ProbeConfigurationDialog.ChannelConfiguration.ProbeGroup = dialog.ProbeConfigurationDialog.ChannelConfiguration.ProbeGroup;
                DialogNeuropixelsV2A.ProbeConfigurationDialog.ChannelConfiguration.RedrawProbeGroup();
                DialogNeuropixelsV2A.ProbeConfigurationDialog.CheckForExistingChannelPreset();

                return;
            }
        }
    }
}
