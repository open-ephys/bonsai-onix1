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
        internal NeuropixelsV1Dialog DialogNeuropixelsV1A { get; private set; }

        /// <summary>
        /// Gets the <see cref="NeuropixelsV1Dialog"/> for ProbeB.
        /// </summary>
        internal NeuropixelsV1Dialog DialogNeuropixelsV1B { get; private set; }

        /// <summary>
        /// Gets the <see cref="GenericDeviceDialog"/> for the Bno055.
        /// </summary>
        internal GenericDeviceDialog DialogBno055 { get; private set; }

        bool HasChanges => DialogNeuropixelsV1A.HasChanges || DialogNeuropixelsV1B.HasChanges;

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV1fHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureNeuropixelsV1A">Configuration settings for <see cref="ConfigureNeuropixelsV1f"/> A.</param>
        /// <param name="configureNeuropixelsV1B">Configuration settings for <see cref="ConfigureNeuropixelsV1f"/> B</param>
        /// <param name="configureBno055">Configuration settings for a <see cref="PolledBno055"/>.</param>
        public NeuropixelsV1fHeadstageDialog(ConfigureNeuropixelsV1f configureNeuropixelsV1A, ConfigureNeuropixelsV1f configureNeuropixelsV1B, ConfigureBno055 configureBno055)
        {
            InitializeComponent();

            DialogNeuropixelsV1A = CreateNeuropixelsV1Dialog(this, configureNeuropixelsV1A, nameof(ConfigureHeadstageNeuropixelsV1f.NeuropixelsV1A), panelNeuropixelsV1A, tabPageNeuropixelsV1A);

            DialogNeuropixelsV1B = CreateNeuropixelsV1Dialog(this, configureNeuropixelsV1B, nameof(ConfigureHeadstageNeuropixelsV1f.NeuropixelsV1B), panelNeuropixelsV1B, tabPageNeuropixelsV1B);

            DialogBno055 = new(configureBno055, true);

            DialogBno055.SetChildFormProperties(this).AddDialogToPanel(panelBno055);

            FormClosing += DialogClosing;
        }

        static NeuropixelsV1Dialog CreateNeuropixelsV1Dialog(NeuropixelsV1fHeadstageDialog headstageDialog, ConfigureNeuropixelsV1f configureNeuropixelsV1, string probeName, Panel panel, TabPage tabPage)
        {
            var dialog = new NeuropixelsV1Dialog(configureNeuropixelsV1, probeName, true)
            {
                Tag = configureNeuropixelsV1.ProbeName
            };

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

            return dialog;
        }

        private void Okay_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        /// <inheritdoc/>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (tabControl1.SelectedTab == tabPageNeuropixelsV1A)
            {
                return DialogNeuropixelsV1A.ProcessMenuShortcut(keyData);
            }
            else if (tabControl1.SelectedTab == tabPageNeuropixelsV1B)
            {
                return DialogNeuropixelsV1B.ProcessMenuShortcut(keyData);
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

            DialogNeuropixelsV1A.CloseWithResult(this);

            if (!DialogNeuropixelsV1A.IsDisposed)
            {
                e.Cancel = true;
                return;
            }

            DialogNeuropixelsV1B.CloseWithResult(this);

            if (!DialogNeuropixelsV1B.IsDisposed)
            {
                e.Cancel = true;

                // NB: Initialize the previously disposed dialog when the user cancels closing the dialog
                var dialog = DialogNeuropixelsV1A;
                panelNeuropixelsV1A.Controls.Remove(DialogNeuropixelsV1A);
                DialogNeuropixelsV1A = CreateNeuropixelsV1Dialog(this, DialogNeuropixelsV1A.ConfigureNode as ConfigureNeuropixelsV1f, nameof(ConfigureHeadstageNeuropixelsV1f.NeuropixelsV1A), panelNeuropixelsV1A, tabPageNeuropixelsV1A);
                DialogNeuropixelsV1A.ProbeConfigurationDialog.ChannelConfiguration.ProbeGroup = dialog.ProbeConfigurationDialog.ChannelConfiguration.ProbeGroup;
                DialogNeuropixelsV1A.ProbeConfigurationDialog.ChannelConfiguration.RedrawProbeGroup();
                DialogNeuropixelsV1A.ProbeConfigurationDialog.CheckForExistingChannelPreset();

                return;
            }
        }
    }
}
