using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureNeuropixelsV2PsbDecoder"/>.
    /// </summary>
    public partial class NeuropixelsV2eDialog : Form
    {
        readonly bool IsBeta = false;

        internal event EventHandler OnStateChange;

        internal readonly NeuropixelsV2eProbeConfigurationDialog ProbeConfigurationDialog;

        internal bool HasChanges
        {
            get => ProbeConfigurationDialog.HasChanges;
            set => ProbeConfigurationDialog.HasChanges = value;
        }

        IConfigureNeuropixelsV2 configureNeuropixelsV2;

        internal IConfigureNeuropixelsV2 ConfigureNeuropixelsV2
        {
            get => configureNeuropixelsV2;
            set
            {
                ProbeConfigurationDialog.propertyGrid.SelectedObject = value;
                configureNeuropixelsV2 = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV2eDialog"/>.
        /// </summary>
        /// <param name="configureNode">A <see cref="ConfigureNeuropixelsV2PsbDecoder"/> object holding the current configuration settings.</param>
        /// <param name="probeName">The name of the probe.</param>
        /// <param name="filterProperties">
        /// <see langword="true"/> if the properties should be filtered by <see cref="DeviceTablePropertyAttribute"/>,
        /// otherwise <see langword="false"/>. Default is <see langword="false"/>.
        /// </param>
        public NeuropixelsV2eDialog(IConfigureNeuropixelsV2 configureNode, string probeName, bool filterProperties = false)
        {
            InitializeComponent();
            Shown += FormShown;
            FormClosing += DialogClosing;

            if (configureNode is ConfigureNeuropixelsV2BetaPsbDecoder)
            {
                Text = Text.Replace("NeuropixelsV2e ", "NeuropixelsV2eBeta ");
                IsBeta = true;
            }

            ProbeConfigurationDialog = new(configureNode.ProbeConfiguration, IsBeta, probeName);
            ProbeConfigurationDialog.SetChildFormProperties(this).AddDialogToPanel(panelProbe);
            ProbeConfigurationDialog.OnStateChange += (sender, e) =>
            {
                if (HasChanges)
                {
                    Text += '*';
                }
                else
                {
                    Text = Text.TrimEnd('*');
                }

                OnStateChange?.Invoke(this, EventArgs.Empty);
            };
            ProbeConfigurationDialog.OnProbeConfigurationChange += (sender, e) =>
            {
                configureNeuropixelsV2.ProbeConfiguration = ProbeConfigurationDialog.ProbeConfiguration;
                ProbeConfigurationDialog.propertyGrid.SelectedObject = configureNeuropixelsV2;
            };

            if (filterProperties)
            {
                ProbeConfigurationDialog.propertyGrid.BrowsableAttributes = new AttributeCollection(
                    new Attribute[]
                    {
                        new BrowsableAttribute(true),
                        new DeviceTablePropertyAttribute (false)
                    });
            }

            ConfigureNeuropixelsV2 = configureNode;
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
            if (HasChanges && this.HandleTopLevelDialogCancel(ref e, ChannelConfigurationDialog.ProbeConfigurationConfirmMessage))
            {
                return;
            }

            ProbeConfigurationDialog.CloseWithResult(this);

            if (!ProbeConfigurationDialog.IsDisposed)
            {
                e.Cancel = true;
                return;
            }
        }
    }
}
