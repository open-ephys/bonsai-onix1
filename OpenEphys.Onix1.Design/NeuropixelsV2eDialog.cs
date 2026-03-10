using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureNeuropixelsV2e"/>.
    /// </summary>
    public partial class NeuropixelsV2eDialog : Form
    {
        internal event EventHandler OnStateChange;

        internal readonly Dictionary<NeuropixelsV2Probe, NeuropixelsV2eProbeConfigurationDialog> ProbeConfigurationDialogs;

        internal bool HasChanges => ProbeConfigurationDialogs.Values.Any(dialog => dialog.HasChanges);

        internal NeuropixelsV2ProbeConfiguration ProbeConfigurationA
        {
            get
            {
                return ProbeConfigurationDialogs.TryGetValue(NeuropixelsV2Probe.ProbeA, out var probeConfigurationDialog)
                    ? probeConfigurationDialog.ProbeConfiguration
                    : throw new NullReferenceException("Unable to find the probe configuration for Probe A.");
            }
        }

        internal NeuropixelsV2ProbeConfiguration ProbeConfigurationB
        {
            get
            {
                return ProbeConfigurationDialogs.TryGetValue(NeuropixelsV2Probe.ProbeB, out var probeConfigurationDialog)
                    ? probeConfigurationDialog.ProbeConfiguration
                    : throw new NullReferenceException("Unable to find the probe configuration for Probe B.");
            }
        }

        /// <summary>
        /// Public <see cref="IConfigureNeuropixelsV2"/> interface that is manipulated by
        /// <see cref="NeuropixelsV2eDialog"/>.
        /// </summary>
        [Obsolete]
        public IConfigureNeuropixelsV2 ConfigureNode { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV2eDialog"/>.
        /// </summary>
        /// <param name="configureNode">A <see cref="ConfigureNeuropixelsV2e"/> object holding the current configuration settings.</param>
        public NeuropixelsV2eDialog(IConfigureNeuropixelsV2 configureNode)
        {
            InitializeComponent();
            Shown += FormShown;
            FormClosing += DialogClosing;

            bool isBeta = false;

            if (configureNode is ConfigureNeuropixelsV2eBeta configureV2eBeta)
            {
                Text = Text.Replace("NeuropixelsV2e ", "NeuropixelsV2eBeta ");
                isBeta = true;
            }

            ProbeConfigurationDialogs = new()
            {
                { NeuropixelsV2Probe.ProbeA, new(configureNode.ProbeConfigurationA, isBeta, nameof(NeuropixelsV2Probe.ProbeA)) },
                { NeuropixelsV2Probe.ProbeB, new(configureNode.ProbeConfigurationB, isBeta, nameof(NeuropixelsV2Probe.ProbeB)) }
            };

            foreach (var probeConfigurationDialog in ProbeConfigurationDialogs)
            {
                string probeName = probeConfigurationDialog.Key.ToString();

                tabControlProbe.TabPages.Add(probeName, probeName);
                probeConfigurationDialog.Value.SetChildFormProperties(this).AddDialogToTab(tabControlProbe.TabPages[probeName]);
                probeConfigurationDialog.Value.OnStateChange += (sender, args) =>
                {
                    var dialog = sender as NeuropixelsV2eProbeConfigurationDialog;

                    if (dialog.HasChanges)
                    {
                        tabControlProbe.TabPages[probeName].Text += '*';
                    }
                    else
                    {
                        tabControlProbe.TabPages[probeName].Text = tabControlProbe.TabPages[probeName].Text.TrimEnd('*');
                    }

                    OnStateChange?.Invoke(this, EventArgs.Empty);
                };
            }
        }

        private void FormShown(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                tableLayoutPanel1.Controls.Remove(flowLayoutPanel1);
                tableLayoutPanel1.RowCount = 1;
            }

            foreach (var channelConfiguration in ProbeConfigurationDialogs)
            {
                channelConfiguration.Value.Show();
            }
        }

        internal bool ProcessMenuShortcut(Keys keyData)
        {
            foreach (var probeConfigurationDialogPair in ProbeConfigurationDialogs)
            {
                if (tabControlProbe.SelectedTab.Name == probeConfigurationDialogPair.Key.ToString() && probeConfigurationDialogPair.Value.ProcessMenuShortcut(keyData))
                {
                    return true;
                }
            }   

            return false;
        }

        internal void Okay_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        void DialogClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.Cancel)
                return;

            bool cancel = false;

            foreach (var dialog in ProbeConfigurationDialogs.Values)
            {
                dialog.Close();

                if (!dialog.IsDisposed)
                {
                    cancel = true;
                    break;
                }
            }

            e.Cancel = cancel;
        }
    }
}
