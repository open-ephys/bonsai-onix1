using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureNeuropixelsV2e"/>.
    /// </summary>
    public partial class NeuropixelsV2eDialog : Form
    {
        internal readonly Dictionary<NeuropixelsV2Probe, NeuropixelsV2eProbeConfigurationDialog> ProbeConfigurations;

        internal NeuropixelsV2ProbeConfiguration ProbeConfigurationA
        {
            get
            {
                return ProbeConfigurations.TryGetValue(NeuropixelsV2Probe.ProbeA, out var probeConfigurationDialog)
                    ? probeConfigurationDialog.ProbeConfiguration
                    : throw new NullReferenceException("Unable to find the probe configuration dialog for Probe A.");
            }
        }

        internal NeuropixelsV2ProbeConfiguration ProbeConfigurationB
        {
            get
            {
                return ProbeConfigurations.TryGetValue(NeuropixelsV2Probe.ProbeB, out var probeConfigurationDialog)
                    ? probeConfigurationDialog.ProbeConfiguration
                    : throw new NullReferenceException("Unable to find the probe configuration dialog for Probe B.");
            }
        }

        /// <summary>
        /// Public <see cref="IConfigureNeuropixelsV2"/> interface that is manipulated by
        /// <see cref="NeuropixelsV2eDialog"/>.
        /// </summary>
        public IConfigureNeuropixelsV2 ConfigureNode
        {
            get => (IConfigureNeuropixelsV2)propertyGrid.SelectedObject;
            set => propertyGrid.SelectedObject = value;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV2eDialog"/>.
        /// </summary>
        /// <param name="configureNode">A <see cref="ConfigureNeuropixelsV2e"/> object holding the current configuration settings.</param>
        public NeuropixelsV2eDialog(IConfigureNeuropixelsV2 configureNode)
        {
            InitializeComponent();
            Shown += FormShown;

            propertyGrid.SelectedObject = configureNode;

            if (configureNode is ConfigureNeuropixelsV2eBeta)
            {
                Text = Text.Replace("NeuropixelsV2e ", "NeuropixelsV2eBeta ");
            }

            ProbeConfigurations = new()
            {
                { NeuropixelsV2Probe.ProbeA, new(configureNode.ProbeConfigurationA) },
                { NeuropixelsV2Probe.ProbeB, new(configureNode.ProbeConfigurationB) }
            };

            foreach (var channelConfiguration in ProbeConfigurations)
            {
                channelConfiguration.Value.SetChildFormProperties(this);
                this.AddMenuItemsFromDialogToFileOption(channelConfiguration.Value, channelConfiguration.Key.ToString());
            }
        }

        private void FormShown(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                tableLayoutPanel1.Controls.Remove(flowLayoutPanel1);
                tableLayoutPanel1.RowCount = 1;

                menuStrip.Visible = false;
            }

            int index = 0;

            foreach (var channelConfiguration in ProbeConfigurations)
            {
                string probeName = channelConfiguration.Key.ToString();

                tabControlProbe.TabPages.Insert(index++, probeName, probeName);
                tabControlProbe.TabPages[probeName].Controls.Add(channelConfiguration.Value);

                channelConfiguration.Value.Show();
            }

            tabControlProbe.SelectedIndex = 0;
        }

        internal void Okay_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
