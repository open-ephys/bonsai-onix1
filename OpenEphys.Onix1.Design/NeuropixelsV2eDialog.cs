using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                new(ConfigureNode.ProbeConfigurationA, ConfigureNode.GainCalibrationFileA, ConfigureNode.InvertPolarity)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill,
                    Parent = this,
                    Tag = NeuropixelsV2Probe.ProbeA
                },
                new(ConfigureNode.ProbeConfigurationB, ConfigureNode.GainCalibrationFileB, ConfigureNode.InvertPolarity)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill,
                    Parent = this,
                    Tag = NeuropixelsV2Probe.ProbeB
                }
            };
        }

        private void InvertPolarityChanged(object sender, EventArgs e)
        {
            NeuropixelsV2eProbeConfigurationDialog sendingDialog = (NeuropixelsV2eProbeConfigurationDialog)sender;
            foreach (var channelConfiguration in ProbeConfigurations)
            {
                if (channelConfiguration.Tag != sendingDialog.Tag)
                {
                    channelConfiguration.SetInvertPolarity(sendingDialog.InvertPolarity);
                }
            }

            ConfigureNode.InvertPolarity = sendingDialog.InvertPolarity;
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

            int index = 0;

            foreach (var channelConfiguration in ProbeConfigurations)
            {
                channelConfiguration.InvertPolarityChanged += InvertPolarityChanged;
                channelConfiguration.ValueChanged += RefreshPropertyGrid;

                string probeName = GetProbeName((NeuropixelsV2Probe)channelConfiguration.Tag);

                tabControlProbe.TabPages.Insert(index++, probeName, probeName);
                tabControlProbe.TabPages[probeName].Controls.Add(channelConfiguration);
                this.AddMenuItemsFromDialogToFileOption(channelConfiguration, probeName);

                channelConfiguration.Show();
            }

            tabControlProbe.SelectedIndex = 0;
        }

        void RefreshPropertyGrid(object s, EventArgs e)
        {
            var dialog = (NeuropixelsV2eProbeConfigurationDialog)s;
            if ((NeuropixelsV2Probe)dialog.Tag == NeuropixelsV2Probe.ProbeA)
            {
                ConfigureNode.GainCalibrationFileA = dialog.textBoxProbeCalibrationFile.Text;
            }
            else if ((NeuropixelsV2Probe)dialog.Tag == NeuropixelsV2Probe.ProbeB)
            {
                ConfigureNode.GainCalibrationFileB = dialog.textBoxProbeCalibrationFile.Text;
            }

            propertyGrid.Refresh();
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

            ConfigureNode.InvertPolarity = ProbeConfigurations[GetProbeIndex(NeuropixelsV2Probe.ProbeA)].InvertPolarity;
        }

        void PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            var propertyGrid = (PropertyGrid)s;
            var configureNode = (IConfigureNeuropixelsV2)propertyGrid.SelectedObject;

            foreach (var configuration in ProbeConfigurations)
            {
                if ((NeuropixelsV2Probe)configuration.Tag == NeuropixelsV2Probe.ProbeA)
                {
                    configuration.UpdateControls(configureNode.ProbeConfigurationA, configureNode.GainCalibrationFileA, configureNode.InvertPolarity);
                }
                else if ((NeuropixelsV2Probe)configuration.Tag == NeuropixelsV2Probe.ProbeB)
                {
                    configuration.UpdateControls(configureNode.ProbeConfigurationB, configureNode.GainCalibrationFileB, configureNode.InvertPolarity);
                }
                else
                    throw new InvalidEnumArgumentException();
            }
        }
    }
}
