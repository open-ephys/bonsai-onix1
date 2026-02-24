using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="NeuropixelsV2ProbeConfiguration"/>.
    /// </summary>
    public partial class NeuropixelsV2eProbeConfigurationDialog : Form
    {
        readonly NeuropixelsV2eChannelConfigurationDialog ChannelConfiguration;

        /// <summary>
        /// Public <see cref="NeuropixelsV2ProbeConfiguration"/> object that is manipulated by
        /// <see cref="NeuropixelsV2eChannelConfigurationDialog"/>.
        /// </summary>
        public NeuropixelsV2ProbeConfiguration ProbeConfiguration
        {
            get => ChannelConfiguration.ProbeConfiguration;
            internal set => ChannelConfiguration.ProbeConfiguration = value;
        }

        /// <inheritdoc cref="ConfigureNeuropixelsV2e.InvertPolarity"/>
        [Obsolete]
        public bool InvertPolarity
        {
            get => ProbeConfiguration.InvertPolarity;
            set => ProbeConfiguration.InvertPolarity = value;
        }

        INeuropixelsV2ProbeInfo ProbeInfo { get; set; }

        readonly Dictionary<ProbeType, NeuropixelsV2ProbeConfiguration> probeConfigurations;

        enum ProbeType
        {
            SingleShank = 0,
            QuadShank
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV2ProbeConfiguration"/>.
        /// </summary>
        /// <param name="configuration">A <see cref="NeuropixelsV2ProbeConfiguration"/> object holding the current configuration settings.</param>
        /// <param name="isBeta">Boolean denoting if this probe is a beta probe or not.</param>
        public NeuropixelsV2eProbeConfigurationDialog(NeuropixelsV2ProbeConfiguration configuration, bool isBeta)
        {
            InitializeComponent();
            Shown += FormShown;

            textBoxProbeCalibrationFile.Text = configuration.GainCalibrationFileName;
            textBoxProbeCalibrationFile.TextChanged += (sender, e) => ProbeConfiguration.GainCalibrationFileName = ((TextBox)sender).Text;

            probeConfigurations = new()
            {
                [ProbeType.SingleShank] = new NeuropixelsV2SingleShankProbeConfiguration(configuration.Probe,
                        NeuropixelsV2SingleShankReference.External,
                        configuration.InvertPolarity,
                        configuration.GainCalibrationFileName,
                        configuration.ProbeInterfaceFileName),
                [ProbeType.QuadShank] = new NeuropixelsV2QuadShankProbeConfiguration(configuration.Probe,
                        NeuropixelsV2QuadShankReference.External,
                        configuration.InvertPolarity,
                        configuration.GainCalibrationFileName,
                        configuration.ProbeInterfaceFileName)
            };

            var currentProbeType = GetCurrentProbeType(configuration);
            DesignHelper.CopyProperties(configuration, probeConfigurations[currentProbeType]);

            ChannelConfiguration = new(probeConfigurations[currentProbeType]);
            ChannelConfiguration.SetChildFormProperties(this).AddDialogToPanel(panelProbe);
            ChannelConfiguration.BringToFront();

            this.AddMenuItemsFromDialogToFileOption(ChannelConfiguration);

            ChannelConfiguration.OnZoom += UpdateTrackBarLocation;
            ChannelConfiguration.OnFileLoad += OnFileLoadEvent;

            propertyGrid.SelectedObject = configuration;
            bindingSource.DataSource = configuration;

            comboBoxProbeType.DataSource = Enum.GetValues(typeof(ProbeType));
            comboBoxProbeType.SelectedItem = currentProbeType;
            comboBoxProbeType.SelectedIndexChanged += ProbeTypeChanged;
            comboBoxProbeType.Enabled = !isBeta; // NB: Beta probes cannot be a single-shank probe

            ProbeInfo = ProbeDataFactory(ProbeConfiguration);

            textBoxProbeCalibrationFile.DataBindings.Add("Text",
                bindingSource,
                $"{nameof(configuration.GainCalibrationFileName)}",
                false,
                DataSourceUpdateMode.OnPropertyChanged);
            textBoxProbeCalibrationFile.TextChanged += (sender, e) => CheckStatus();

            comboBoxReference.DataSource = ProbeInfo.GetReferenceEnumValues();
            comboBoxReference.DataBindings.Add("SelectedItem",
                bindingSource,
                nameof(configuration.Reference),
                false,
                DataSourceUpdateMode.OnPropertyChanged);
            comboBoxReference.SelectedIndexChanged += (sender, e) =>
            {
                // NB: Needed to capture mouse scroll wheel updates
                var control = sender as Control;

                foreach (Binding binding in control.DataBindings)
                {
                    binding.WriteValue();
                }

                bindingSource.ResetCurrentItem();
            };

            comboBoxChannelPresets.DataSource = ProbeInfo.GetComboBoxChannelPresets();
            CheckForExistingChannelPreset();
            comboBoxChannelPresets.SelectedIndexChanged += (sender, e) =>
            {
                try
                {
                    Enum channelPreset = ((ComboBox)sender).SelectedItem as Enum ?? throw new InvalidEnumArgumentException("Invalid argument given for the channel preset.");
                    ProbeConfiguration.SelectElectrodes(ProbeInfo.GetChannelPreset(channelPreset));
                }
                catch (InvalidEnumArgumentException ex)
                {
                    MessageBox.Show(ex.Message, "Invalid Preset Chosen");
                    return;
                }

                UpdateProbeGroup();
            };

            checkBoxInvertPolarity.DataBindings.Add("Checked",
                bindingSource,
                $"{nameof(configuration.InvertPolarity)}",
                false,
                DataSourceUpdateMode.OnPropertyChanged);

            CheckStatus();

            Text += ": " + ProbeConfiguration.Probe.ToString();

            bindingSource.ListChanged += (sender, eventArgs) => propertyGrid.Refresh();

            tabControlProbe.SelectedIndexChanged += (sender, eventArgs) =>
            {
                if (tabControlProbe.SelectedTab == tabPageProperties)
                    propertyGrid.Refresh();

                else if (tabControlProbe.SelectedTab == tabPageConfiguration)
                    bindingSource.ResetCurrentItem();
            };
        }

        ProbeType GetCurrentProbeType(NeuropixelsV2ProbeConfiguration configuration)
        {
            if (configuration is NeuropixelsV2SingleShankProbeConfiguration)
                return ProbeType.SingleShank;
            else if (configuration is NeuropixelsV2QuadShankProbeConfiguration)
                return ProbeType.QuadShank;

            throw new InvalidEnumArgumentException($"Unknown {nameof(NeuropixelsV2ProbeConfiguration)} type: {configuration.GetType()}");
        }

        private void ProbeTypeChanged(object sender, EventArgs e)
        {
            UpdateProbeConfiguration();
        }

        static INeuropixelsV2ProbeInfo ProbeDataFactory(NeuropixelsV2ProbeConfiguration configuration)
        {
            if (configuration is NeuropixelsV2QuadShankProbeConfiguration quadShankConfiguration)
            {
                return new NeuropixelsV2QuadShankInfo(quadShankConfiguration);
            }
            else if (configuration is NeuropixelsV2SingleShankProbeConfiguration singleShankConfiguration)
            {
                return new NeuropixelsV2SingleShankInfo(singleShankConfiguration);
            }

            throw new NotImplementedException("Unknown configuration found.");
        }

        void UpdateProbeConfiguration()
        {
            var probeType = (ProbeType)comboBoxProbeType.SelectedItem;

            ProbeConfiguration = probeConfigurations[probeType];
            ChannelConfiguration.ResizeSelectedContacts();

            textBoxProbeCalibrationFile.Text = ProbeConfiguration.GainCalibrationFileName;

            ProbeInfo = ProbeDataFactory(ProbeConfiguration);

            ChannelConfiguration.DrawProbeGroup();
            ChannelConfiguration.ResetZoom();
            ChannelConfiguration.RefreshZedGraph();

            // NB: Temporarily detach handlers so the updated information is respected
            comboBoxChannelPresets.SelectedIndexChanged -= SelectedChannelPresetChanged;
            comboBoxChannelPresets.DataSource = ProbeInfo.GetComboBoxChannelPresets();
            comboBoxChannelPresets.SelectedIndexChanged += SelectedChannelPresetChanged;

            comboBoxReference.SelectedIndexChanged -= SelectedReferenceChanged;
            comboBoxReference.DataSource = ProbeInfo.GetReferenceEnumValues();
            comboBoxReference.SelectedItem = ProbeConfiguration.Reference;
            comboBoxReference.SelectedIndexChanged += SelectedReferenceChanged;

            checkBoxInvertPolarity.Checked = ProbeConfiguration.InvertPolarity;

            CheckForExistingChannelPreset();
        }

        private void FormShown(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                tableLayoutPanel1.Controls.Remove(flowLayoutPanel1);
                tableLayoutPanel1.RowCount = 1;

                menuStrip.Visible = false;
            }

            splitContainer1.SplitterDistance = splitContainer1.Size.Width - splitContainer1.Panel2MinSize;

            if (ChannelConfiguration.Visible)
                ChannelConfiguration.Show();
            ChannelConfiguration.ConnectResizeEventHandler();
            ChannelConfiguration.ResizeZedGraph();
        }

        private void SelectedReferenceChanged(object sender, EventArgs e)
        {
            ProbeConfiguration.Reference = (Enum)((ComboBox)sender).SelectedItem;
        }

        private void SelectedChannelPresetChanged(object sender, EventArgs e)
        {
            Enum channelPreset = ((ComboBox)sender).SelectedItem as Enum ?? throw new InvalidEnumArgumentException("Invalid argument given for the channel preset.");
            ProbeConfiguration.SelectElectrodes(ProbeInfo.GetChannelPreset(channelPreset));

            ChannelConfiguration.HighlightEnabledContacts();
            ChannelConfiguration.HighlightSelectedContacts();
            ChannelConfiguration.UpdateContactLabels();
            ChannelConfiguration.RefreshZedGraph();
        }

        void CheckForExistingChannelPreset()
        {
            comboBoxChannelPresets.SelectedItem = ProbeInfo.CheckForExistingChannelPreset(ProbeConfiguration.ChannelMap);
        }

        private void OnFileLoadEvent(object sender, EventArgs e)
        {
            var currentProbeType = GetCurrentProbeType(ProbeConfiguration);

            probeConfigurations[currentProbeType].ProbeGroup = (NeuropixelsV2eProbeGroup)ChannelConfiguration.ProbeGroup;

            UpdateProbeConfiguration();
        }

        private void CheckStatus()
        {
            const string NoFileSelected = "No file selected.";
            const string InvalidFile = "Invalid file.";

            NeuropixelsV2GainCorrection? gainCorrection;

            try
            {
                gainCorrection = NeuropixelsV2Helper.TryParseGainCalibrationFile(textBoxProbeCalibrationFile.Text);
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message, "I/O error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message, "Unauthorized access error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ChannelConfiguration.Visible = gainCorrection.HasValue;

            textBoxGainCorrection.Text = gainCorrection.HasValue
                                         ? gainCorrection.Value.GainCorrectionFactor.ToString()
                                         : "";

            toolStripGainCalSN.Text = gainCorrection.HasValue
                                     ? gainCorrection.Value.SerialNumber.ToString()
                                     : string.IsNullOrEmpty(textBoxProbeCalibrationFile.Text) ? NoFileSelected : InvalidFile;

            if (toolStripGainCalSN.Text == NoFileSelected)
                toolStripLabelGainCalibrationSN.Image = Properties.Resources.StatusWarningImage;
            else if (toolStripGainCalSN.Text == InvalidFile)
                toolStripLabelGainCalibrationSN.Image = Properties.Resources.StatusCriticalImage;
            else
                toolStripLabelGainCalibrationSN.Image = Properties.Resources.StatusReadyImage;
        }

        internal void ChooseCalibrationFile_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                CheckFileExists = true,
                Filter = "Gain calibration files (*_gainCalValues.csv)|*_gainCalValues.csv|All Files|*.*",
                FilterIndex = 0,
                InitialDirectory = File.Exists(textBoxProbeCalibrationFile.Text) ?
                                   Path.GetDirectoryName(textBoxProbeCalibrationFile.Text) :
                                   ""
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBoxProbeCalibrationFile.Text = ofd.FileName;
            }

            CheckStatus();
        }

        internal void ClearSelection_Click(object sender, EventArgs e)
        {
            DeselectContacts();
        }

        internal void EnableContacts_Click(object sender, EventArgs e)
        {
            EnableSelectedContacts();
            DeselectContacts();
        }

        private void EnableSelectedContacts()
        {
            var selected = ProbeConfiguration.ProbeGroup
                    .ToElectrodes()
                    .Where((e, ind) => ChannelConfiguration.SelectedContacts[ind])
                    .ToArray();

            ChannelConfiguration.EnableElectrodes(selected);

            CheckForExistingChannelPreset();
        }

        private void DeselectContacts()
        {
            ChannelConfiguration.SetAllSelections(false);
            ChannelConfiguration.HighlightEnabledContacts();
            ChannelConfiguration.HighlightSelectedContacts();
            ChannelConfiguration.UpdateContactLabels();
            ChannelConfiguration.RefreshZedGraph();
        }

        private void MoveToVerticalPosition(float relativePosition)
        {
            ChannelConfiguration.MoveToVerticalPosition(relativePosition);
            ChannelConfiguration.RefreshZedGraph();
        }

        private void TrackBarScroll(object sender, EventArgs e)
        {
            var trackBar = (TrackBar)sender;
            MoveToVerticalPosition((float)trackBar.Value / trackBar.Maximum);
        }

        private void UpdateTrackBarLocation(object sender, EventArgs e)
        {
            trackBarProbePosition.Value = (int)(ChannelConfiguration.GetRelativeVerticalPosition() * trackBarProbePosition.Maximum);
        }

        internal void UpdateProbeGroup()
        {
            ChannelConfiguration.UpdateProbeGroup();
        }

        internal void HidePropertiesTab()
        {
            tabControlProbe.TabPages.Remove(tabPageProperties);
        }
    }
}
