using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureHeadstageNeuropixelsV2e"/>.
    /// </summary>
    public partial class NeuropixelsV2Rhd2000eHeadstageDialog : Form
    {
        internal NeuropixelsV2eDialog DialogNeuropixelsV2 { get; private set; }

        internal readonly GenericDeviceDialog DialogRhd2000;

        internal readonly GenericDeviceDialog DialogBno055;

        bool HasChanges => DialogNeuropixelsV2.HasChanges;

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV2eHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureHeadstage">Configuration settings for a <see cref="ConfigureHeadstageNeuropixelsV2e"/>.</param>
        public NeuropixelsV2Rhd2000eHeadstageDialog(ConfigureHeadstageNeuropixelsV2Rhd2000e configureHeadstage)
        {

            Text = "HeadstageNeuropixelsV2/Rhd2000e Configuration";

            InitializeComponent();
            FormClosing += DialogClosing;

            DialogRhd2000 = new(configureHeadstage.Rhd2000, true);
            DialogRhd2000.SetChildFormProperties(this).AddDialogToPanel(panelRhd2000);

            DialogBno055 = new(configureHeadstage.Bno055, true);
            DialogBno055.SetChildFormProperties(this).AddDialogToPanel(panelBno055);

            DialogNeuropixelsV2 = CreateNeuropixelsV2Dialog(this, configureHeadstage.NeuropixelsV2, nameof(configureHeadstage.NeuropixelsV2), panelNeuropixelsV2A, tabPageNeuropixelsV2);
        }


        static NeuropixelsV2eDialog CreateNeuropixelsV2Dialog(NeuropixelsV2Rhd2000eHeadstageDialog headstageDialog, IConfigureNeuropixelsV2 configureNeuropixelsV2, string probeName, Panel panel, TabPage tabPage)
        {
            var dialog = new NeuropixelsV2eDialog(configureNeuropixelsV2, probeName, true);

            dialog.SetChildFormProperties(headstageDialog).AddDialogToPanel(panel);

            dialog.OnStateChange += (sender, e) =>
            {
                if (dialog.HasChanges)
                {
                    if (!tabPage.Text.EndsWith("*"))
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
            if (tabControl1.SelectedTab == tabPageNeuropixelsV2)
            {
                return DialogNeuropixelsV2.ProcessMenuShortcut(keyData);
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

            DialogNeuropixelsV2.CloseWithResult(this);

            if (!DialogNeuropixelsV2.IsDisposed)
            {
                e.Cancel = true;

                // NB: Initialize the previously disposed dialog when the user cancels closing the dialog
                var dialog = DialogNeuropixelsV2;
                panelNeuropixelsV2A.Controls.Remove(DialogNeuropixelsV2);
                DialogNeuropixelsV2 = CreateNeuropixelsV2Dialog(this, DialogNeuropixelsV2.ConfigureNeuropixelsV2, DialogNeuropixelsV2.ProbeConfigurationDialog.ProbeName, panelNeuropixelsV2A, tabPageNeuropixelsV2);
                DialogNeuropixelsV2.ProbeConfigurationDialog.ChannelConfiguration.ProbeGroup = dialog.ProbeConfigurationDialog.ChannelConfiguration.ProbeGroup;
                DialogNeuropixelsV2.ProbeConfigurationDialog.ChannelConfiguration.RedrawProbeGroup();
                DialogNeuropixelsV2.ProbeConfigurationDialog.CheckForExistingChannelPreset();

                return;
            }
        }
    }
}
