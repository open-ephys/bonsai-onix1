using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="IConfigureNeuropixelsV1"/>.
    /// </summary>
    public partial class NeuropixelsV1Dialog : Form
    {
        internal event EventHandler OnStateChange;

        internal readonly NeuropixelsV1ProbeConfigurationDialog ProbeConfigurationDialog;

        internal bool HasChanges
        {
            get => ProbeConfigurationDialog.HasChanges;
            set => ProbeConfigurationDialog.HasChanges = value;
        }

        IConfigureNeuropixelsV1 configureNeuropixelsV1;

        internal IConfigureNeuropixelsV1 ConfigureNode
        {
            get => configureNeuropixelsV1;
            private set
            {
                configureNeuropixelsV1 = value;
                ProbeConfigurationDialog.propertyGrid.SelectedObject = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV1Dialog"/>.
        /// </summary>
        /// <param name="configureNode">A <see cref="IConfigureNeuropixelsV1"/> object holding the current configuration settings.</param>
        /// <param name="probeName">The name of the probe.</param>
        /// <param name="filterProperties">
        /// <see langword="true"/> if the properties should be filtered by <see cref="DeviceTablePropertyAttribute"/>,
        /// otherwise <see langword="false"/>. Default is <see langword="false"/>.
        /// </param>
        public NeuropixelsV1Dialog(IConfigureNeuropixelsV1 configureNode, string probeName, bool filterProperties = false)
        {
            InitializeComponent();
            Shown += FormShown;
            FormClosing += DialogClosing;

            ProbeConfigurationDialog = new(configureNode.ProbeConfiguration, probeName);
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
            
            if (filterProperties)
            {
                ProbeConfigurationDialog.propertyGrid.BrowsableAttributes = new AttributeCollection(
                    new Attribute[]
                    {
                        new BrowsableAttribute(true),
                        new DeviceTablePropertyAttribute (false)
                    });
            }

            ConfigureNode = configureNode;
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
