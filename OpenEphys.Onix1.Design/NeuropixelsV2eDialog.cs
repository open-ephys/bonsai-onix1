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
        readonly IReadOnlyList<NeuropixelsV2eProbeConfigurationDialog> ProbeConfigurations;

        /// <summary>
        /// Public <see cref="IConfigureNeuropixelsV2"/> interface that is manipulated by
        /// <see cref="NeuropixelsV2eDialog"/>.
        /// </summary>
        public IConfigureNeuropixelsV2 ConfigureNode { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV2eDialog"/>.
        /// </summary>
        /// <param name="configureNode">A <see cref="ConfigureNeuropixelsV2e"/> object holding the current configuration settings.</param>
        public NeuropixelsV2eDialog(IConfigureNeuropixelsV2 configureNode)
        {
            InitializeComponent();
            Shown += FormShown;

            if (configureNode is ConfigureNeuropixelsV2eBeta configureV2eBeta)
            {
                ConfigureNode = new ConfigureNeuropixelsV2eBeta(configureV2eBeta);
                Text = Text.Replace("NeuropixelsV2e ", "NeuropixelsV2eBeta ");
            }
            else if (configureNode is ConfigureNeuropixelsV2e configureV2e)
            {
                ConfigureNode = new ConfigureNeuropixelsV2e(configureV2e);
            }

            ProbeConfigurations = new List<NeuropixelsV2eProbeConfigurationDialog>
            {
                new(ConfigureNode.ProbeConfigurationA, ConfigureNode.GainCalibrationFileA)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill,
                    Parent = this,
                    Tag = NeuropixelsV2Probe.ProbeA
                },
                new(ConfigureNode.ProbeConfigurationB, ConfigureNode.GainCalibrationFileB)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill,
                    Parent = this,
                    Tag = NeuropixelsV2Probe.ProbeB
                }
            };

            foreach (var channelConfiguration in ProbeConfigurations)
            {
                string probeName = GetProbeName((NeuropixelsV2Probe)channelConfiguration.Tag);

                tabControlProbe.TabPages.Add(probeName, probeName);
                tabControlProbe.TabPages[probeName].Controls.Add(channelConfiguration);
                this.AddMenuItemsFromDialogToFileOption(channelConfiguration, probeName);
            }
        }

        private string GetProbeName(NeuropixelsV2Probe probe)
        {
            return probe switch
            {
                NeuropixelsV2Probe.ProbeA => "Probe A",
                NeuropixelsV2Probe.ProbeB => "Probe B",
                _ => "Invalid probe was specified."
            };
        }

        private int GetProbeIndex(NeuropixelsV2Probe probe)
        {
            return probe == NeuropixelsV2Probe.ProbeA ? 0 : 1;
        }

        private void FormShown(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                tableLayoutPanel1.Controls.Remove(flowLayoutPanel1);
                tableLayoutPanel1.RowCount = 1;

                menuStrip.Visible = false;
            }

            foreach (var channelConfiguration in ProbeConfigurations)
            {
                channelConfiguration.Show();
            }
        }

        internal void Okay_Click(object sender, EventArgs e)
        {
            SaveVariables();

            DialogResult = DialogResult.OK;
        }

        internal void SaveVariables()
        {
            ConfigureNode.ProbeConfigurationA = ProbeConfigurations[GetProbeIndex(NeuropixelsV2Probe.ProbeA)].ProbeConfiguration;
            ConfigureNode.ProbeConfigurationB = ProbeConfigurations[GetProbeIndex(NeuropixelsV2Probe.ProbeB)].ProbeConfiguration;

            ConfigureNode.GainCalibrationFileA = ProbeConfigurations[GetProbeIndex(NeuropixelsV2Probe.ProbeA)].textBoxProbeCalibrationFile.Text;
            ConfigureNode.GainCalibrationFileB = ProbeConfigurations[GetProbeIndex(NeuropixelsV2Probe.ProbeB)].textBoxProbeCalibrationFile.Text;
        }
    }
}
