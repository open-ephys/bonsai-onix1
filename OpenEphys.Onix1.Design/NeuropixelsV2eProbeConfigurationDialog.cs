using System;
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
        }

        /// <inheritdoc cref="ConfigureNeuropixelsV2e.InvertPolarity"/>
        [Obsolete]
        public bool InvertPolarity
        {
            get => ProbeConfiguration.InvertPolarity;
            set => ProbeConfiguration.InvertPolarity = value;
        }

        INeuropixelsV2ProbeInfo ProbeData { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV2ProbeConfiguration"/>.
        /// </summary>
        /// <param name="configuration">A <see cref="NeuropixelsV2ProbeConfiguration"/> object holding the current configuration settings.</param>
        public NeuropixelsV2eProbeConfigurationDialog(NeuropixelsV2ProbeConfiguration configuration)
        {
            InitializeComponent();
            Shown += FormShown;

            ChannelConfiguration = new(configuration);
            ChannelConfiguration.SetChildFormProperties(this).AddDialogToPanel(panelProbe);

            this.AddMenuItemsFromDialogToFileOption(ChannelConfiguration);

            ChannelConfiguration.OnZoom += UpdateTrackBarLocation;
            ChannelConfiguration.OnFileLoad += OnFileLoadEvent;

            propertyGrid.SelectedObject = configuration;
            bindingSource.DataSource = configuration;

            ProbeData = ProbeDataFactory(configuration);

            textBoxProbeCalibrationFile.DataBindings.Add("Text",
                bindingSource,
                $"{nameof(configuration.GainCalibrationFileName)}",
                false,
                DataSourceUpdateMode.OnPropertyChanged);
            textBoxProbeCalibrationFile.TextChanged += (sender, e) => CheckStatus();

            comboBoxReference.DataSource = ProbeData.GetReferenceEnumValues();
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

            comboBoxChannelPresets.DataSource = ProbeData.GetComboBoxChannelPresets();
            CheckForExistingChannelPreset();
            comboBoxChannelPresets.SelectedIndexChanged += (sender, e) =>
            {
                try
                {
                    Enum channelPreset = ((ComboBox)sender).SelectedItem as Enum ?? throw new InvalidEnumArgumentException("Invalid argument given for the channel preset.");
                    ProbeConfiguration.SelectElectrodes(ProbeData.GetChannelPreset(channelPreset));
                }
                catch (InvalidEnumArgumentException ex)
                {
                    MessageBox.Show(ex.Message, "Invalid Preset Chosen");
                    return;
                }

                ChannelConfiguration.HighlightEnabledContacts();
                ChannelConfiguration.HighlightSelectedContacts();
                ChannelConfiguration.UpdateContactLabels();
                ChannelConfiguration.RefreshZedGraph();
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

        static INeuropixelsV2ProbeInfo ProbeDataFactory(NeuropixelsV2ProbeConfiguration configuration)
        {
            if (configuration is NeuropixelsV2QuadShankProbeConfiguration quadShankConfiguration)
            {
                return new NeuropixelsV2QuadShankInfo(quadShankConfiguration);
            }

            throw new NotImplementedException("Unknown configuration found.");
        }

        private void FormShown(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                tableLayoutPanel1.Controls.Remove(flowLayoutPanel1);
                tableLayoutPanel1.RowCount = 1;

                menuStrip.Visible = false;
            }

            ChannelConfiguration.Show();
            ChannelConfiguration.ConnectResizeEventHandler();
            ChannelConfiguration.ResizeZedGraph();
        }

        void CheckForExistingChannelPreset()
        {
            comboBoxChannelPresets.SelectedItem = ProbeData.CheckForExistingChannelPreset(ProbeConfiguration.ChannelMap);
        }

        private void OnFileLoadEvent(object sender, EventArgs e)
        {
            CheckForExistingChannelPreset();
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

            panelProbe.Visible = gainCorrection.HasValue;

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
    }
}
