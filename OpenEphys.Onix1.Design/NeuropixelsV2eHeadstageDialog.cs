using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureHeadstageNeuropixelsV2e"/>.
    /// </summary>
    public partial class NeuropixelsV2eHeadstageDialog : Form
    {
        /// <summary>
        /// Gets the <see cref="NeuropixelsV2eDialog"/>.
        /// </summary>
        public readonly NeuropixelsV2eDialog DialogNeuropixelsV2e;

        /// <summary>
        /// Gets the <see cref="GenericDeviceDialog"/> for the Bno055.
        /// </summary>
        public readonly GenericDeviceDialog DialogBno055;

        bool HasChanges => DialogNeuropixelsV2e.HasChanges;

        /// <summary>
        /// Initializes a new instance of a <see cref="NeuropixelsV2eHeadstageDialog"/>.
        /// </summary>
        /// <param name="configureNeuropixelsV2e">Configuration settings for a <see cref="ConfigureNeuropixelsV2e"/>.</param>
        /// <param name="configureBno055">Configuration settings for a <see cref="ConfigurePolledBno055"/>.</param>
        public NeuropixelsV2eHeadstageDialog(IConfigureNeuropixelsV2 configureNeuropixelsV2e, ConfigurePolledBno055 configureBno055)
        {
            InitializeComponent();
            FormClosing += DialogClosing;

            DialogNeuropixelsV2e = new(configureNeuropixelsV2e);

            DialogNeuropixelsV2e.SetChildFormProperties(this).AddDialogToPanel(panelNeuropixelsV2e);

            DialogNeuropixelsV2e.OnStateChange += (sender, e) =>
            {
                if (HasChanges)
                {
                    if (!tabPageNeuropixelsV2e.Text.EndsWith("*"))
                    {
                        tabPageNeuropixelsV2e.Text += '*';
                    }
                }
                else
                {
                    tabPageNeuropixelsV2e.Text = tabPageNeuropixelsV2e.Text.TrimEnd('*');
                }
            };

            if (configureNeuropixelsV2e is ConfigureNeuropixelsV2eBeta)
            {
                Text = Text.Replace("NeuropixelsV2e ", "NeuropixelsV2eBeta ");
                tabPageNeuropixelsV2e.Text = "NeuropixelsV2eBeta";
            }

            DialogBno055 = new(new ConfigurePolledBno055(configureBno055));

            DialogBno055.SetChildFormProperties(this).AddDialogToPanel(panelBno055);
        }

        private void Okay_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        /// <inheritdoc/>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (tabControl1.SelectedTab == tabPageNeuropixelsV2e)
            {
                return DialogNeuropixelsV2e.ProcessMenuShortcut(keyData);
            }
            else if (tabControl1.SelectedTab == tabPageBno055)
            {
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        void DialogClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.Cancel)
                return;

            DialogNeuropixelsV2e.Close();
        }
    }
}
