using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="NeuropixelsV1ProbeConfiguration"/>.
    /// </summary>
    public partial class NeuropixelsV1ProbeConfigurationDialog : Form
    {
        readonly NeuropixelsV1ChannelConfigurationDialog ChannelConfiguration;

        NeuropixelsV1Adc[] Adcs = null;

        enum ChannelPreset
        {
            BankA,
            BankB,
            BankC,
            SingleColumn,
            Tetrodes,
            None
        }

        NeuropixelsV1eProbeGroup ProbeGroup => ChannelConfiguration.ProbeGroup as NeuropixelsV1eProbeGroup ?? throw new InvalidCastException($"Expected the ProbeGroup to be of type '{nameof(NeuropixelsV1eProbeGroup)}', but it is '{ChannelConfiguration.ProbeGroup.GetType().Name}'.");

        /// <summary>
        /// Gets or sets the probe configuration.
        /// </summary>
        public NeuropixelsV1ProbeConfiguration ProbeConfiguration { get; set; }

        /// <inheritdoc cref="NeuropixelsV1ProbeConfiguration.InvertPolarity"/>
        [Obsolete]
        public bool InvertPolarity
        {
            get => ProbeConfiguration.InvertPolarity;
            set => ProbeConfiguration.InvertPolarity = value;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV1Dialog"/>.
        /// </summary>
        /// <param name="probeConfiguration">A <see cref="NeuropixelsV1ProbeConfiguration"/> object holding the current configuration settings.</param>
        public NeuropixelsV1ProbeConfigurationDialog(NeuropixelsV1ProbeConfiguration probeConfiguration)
        {
            InitializeComponent();
            Shown += FormShown;
            FormClosing += DialogClosing;

            ProbeConfiguration = probeConfiguration;

            ChannelConfiguration = new(probeConfiguration);
            ChannelConfiguration
                .SetChildFormProperties(this)
                .AddDialogToPanel(panelProbe);

            this.AddMenuItemsFromDialogToFileOption(ChannelConfiguration);

            ChannelConfiguration.OnZoom += UpdateTrackBarLocation;
            ChannelConfiguration.OnFileLoad += OnFileLoadEvent;
            ChannelConfiguration.BringToFront();

            comboBoxApGain.DataSource = Enum.GetValues(typeof(NeuropixelsV1Gain));
            comboBoxApGain.SelectedItem = ProbeConfiguration.SpikeAmplifierGain;
            comboBoxApGain.SelectedIndexChanged += SpikeAmplifierGainIndexChanged;

            comboBoxLfpGain.DataSource = Enum.GetValues(typeof(NeuropixelsV1Gain));
            comboBoxLfpGain.SelectedItem = ProbeConfiguration.LfpAmplifierGain;
            comboBoxLfpGain.SelectedIndexChanged += LfpAmplifierGainIndexChanged;

            comboBoxReference.DataSource = Enum.GetValues(typeof(NeuropixelsV1ReferenceSource));
            comboBoxReference.SelectedItem = ProbeConfiguration.Reference;
            comboBoxReference.SelectedIndexChanged += ReferenceIndexChanged;

            checkBoxSpikeFilter.Checked = ProbeConfiguration.SpikeFilter;
            checkBoxSpikeFilter.CheckedChanged += SpikeFilterIndexChanged;

            checkBoxInvertPolarity.Checked = ProbeConfiguration.InvertPolarity;
            checkBoxInvertPolarity.CheckedChanged += InvertPolarityIndexChanged;

            textBoxAdcCalibrationFile.Text = ProbeConfiguration.AdcCalibrationFileName;
            textBoxAdcCalibrationFile.TextChanged += (sender, e) =>
            {
                ProbeConfiguration.AdcCalibrationFileName = ((TextBox)sender).Text;
                CheckStatus();
            };

            textBoxGainCalibrationFile.Text = ProbeConfiguration.GainCalibrationFileName;
            textBoxGainCalibrationFile.TextChanged += (sender, e) =>
            {
                ProbeConfiguration.GainCalibrationFileName = ((TextBox)sender).Text;
                CheckStatus();
            };

            textBoxProbeInterfaceFileName.Text = ProbeConfiguration.ProbeInterfaceFileName;
            textBoxProbeInterfaceFileName.TextChanged += (sender, e) =>
            {
                ProbeConfiguration.ProbeInterfaceFileName = ((TextBox)sender).Text;
                CheckStatus();
            };

            comboBoxChannelPresets.DataSource = Enum.GetValues(typeof(ChannelPreset));
            CheckForExistingChannelPreset();
            comboBoxChannelPresets.SelectedIndexChanged += ChannelPresetIndexChanged;

            CheckStatus();
        }

        private void InvertPolarityIndexChanged(object sender, EventArgs e)
        {
            ProbeConfiguration.InvertPolarity = ((CheckBox)sender).Checked;
        }

        private void FormShown(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                tableLayoutPanel1.Controls.Remove(flowLayoutPanel1);
                tableLayoutPanel1.RowCount = 1;

                menuStrip.Visible = false;
            }

            if (ChannelConfiguration.Visible)
                ChannelConfiguration.Show();
            ChannelConfiguration.ConnectResizeEventHandler();
        }

        private void SpikeAmplifierGainIndexChanged(object sender, EventArgs e)
        {
            ProbeConfiguration.SpikeAmplifierGain = (NeuropixelsV1Gain)((ComboBox)sender).SelectedItem;
            CheckStatus();
        }

        private void LfpAmplifierGainIndexChanged(object sender, EventArgs e)
        {
            ProbeConfiguration.LfpAmplifierGain = (NeuropixelsV1Gain)((ComboBox)sender).SelectedItem;
            CheckStatus();
        }

        private void ReferenceIndexChanged(object sender, EventArgs e)
        {
            ProbeConfiguration.Reference = (NeuropixelsV1ReferenceSource)((ComboBox)sender).SelectedItem;
        }

        private void ChannelPresetIndexChanged(object sender, EventArgs e)
        {
            var channelPreset = (ChannelPreset)((ComboBox)sender).SelectedItem;

            if (channelPreset != ChannelPreset.None)
            {
                SetChannelPreset(channelPreset);
            }
        }

        private void SpikeFilterIndexChanged(object sender, EventArgs e)
        {
            ProbeConfiguration.SpikeFilter = ((CheckBox)sender).Checked;
        }

        private void SetChannelPreset(ChannelPreset preset)
        {
            var probeGroup = ProbeGroup;
            var electrodes = NeuropixelsV1eProbeGroup.ToElectrodes(probeGroup);

            switch (preset)
            {
                case ChannelPreset.BankA:
                    probeGroup.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.A).ToArray());
                    break;

                case ChannelPreset.BankB:
                    probeGroup.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.B).ToArray());
                    break;

                case ChannelPreset.BankC:
                    probeGroup.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.C ||
                                                                             (e.Bank == NeuropixelsV1Bank.B && e.Index >= 576)).ToArray());
                    break;

                case ChannelPreset.SingleColumn:
                    probeGroup.SelectElectrodes(electrodes.Where(e => (e.Index % 2 == 0 && e.Bank == NeuropixelsV1Bank.A) ||
                                                                              (e.Index % 2 == 1 && e.Bank == NeuropixelsV1Bank.B)).ToArray());
                    break;

                case ChannelPreset.Tetrodes:
                    probeGroup.SelectElectrodes(electrodes.Where(e => (e.Index % 8 < 4 && e.Bank == NeuropixelsV1Bank.A) ||
                                                                              (e.Index % 8 > 3 && e.Bank == NeuropixelsV1Bank.B)).ToArray());
                    break;
            }

            ChannelConfiguration.HighlightEnabledContacts();
            ChannelConfiguration.HighlightSelectedContacts();
            ChannelConfiguration.UpdateContactLabels();
            ChannelConfiguration.RefreshZedGraph();
        }

        private void CheckForExistingChannelPreset()
        {
            var channelMap = ProbeGroup.ChannelMap;

            if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.A))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.BankA;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.B))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.BankB;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.C ||
                                        (e.Bank == NeuropixelsV1Bank.B && e.Index >= 576)))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.BankC;
            }
            else if (channelMap.All(e => (e.Index % 2 == 0 && e.Bank == NeuropixelsV1Bank.A) ||
                                         (e.Index % 2 == 1 && e.Bank == NeuropixelsV1Bank.B)))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.SingleColumn;
            }
            else if (channelMap.All(e => (e.Index % 8 < 4 && e.Bank == NeuropixelsV1Bank.A) ||
                                         (e.Index % 8 > 3 && e.Bank == NeuropixelsV1Bank.B)))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.Tetrodes;
            }
            else
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.None;
            }
        }

        private void OnFileLoadEvent(object sender, EventArgs e)
        {
            CheckForExistingChannelPreset();
        }

        void CheckStatus()
        {
            const string NoFileSelected = "No file selected.";
            const string InvalidFile = "Invalid file.";
            const string SelectProbeInterfaceFile = "Please select a probe interface file to view electrode configuration.";
            const string SelectAdcCalibrationFile = "Please select an ADC calibration file to view electrode configuration.";
            const string SelectGainCalibrationFile = "Please select a gain calibration file to view electrode configuration.";

            labelDefaultText.Text = string.Empty;

            if (string.IsNullOrEmpty(textBoxProbeInterfaceFileName.Text))
            {
                labelDefaultText.Text += SelectProbeInterfaceFile + Environment.NewLine;
            }

            NeuropixelsV1AdcCalibration? adcCalibration;

            string adcCalibrationFile = textBoxAdcCalibrationFile.Text;

            if (string.IsNullOrEmpty(adcCalibrationFile))
            {
                labelDefaultText.Text += SelectAdcCalibrationFile + Environment.NewLine;
            }

            try
            {
                adcCalibration = NeuropixelsV1Helper.TryParseAdcCalibrationFile(adcCalibrationFile);
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

            Adcs = adcCalibration.HasValue
                   ? adcCalibration.Value.Adcs
                   : null;

            buttonViewAdcs.Enabled = adcCalibration.HasValue;
            toolStripAdcCalSN.Text = adcCalibration.HasValue
                                     ? adcCalibration.Value.SerialNumber.ToString()
                                     : string.IsNullOrEmpty(adcCalibrationFile)
                                       ? NoFileSelected
                                       : InvalidFile;

            NeuropixelsV1eGainCorrection? gainCorrection;

            string gainCalibrationFile = textBoxGainCalibrationFile.Text;

            if (string.IsNullOrEmpty(gainCalibrationFile))
            {
                labelDefaultText.Text += SelectGainCalibrationFile + Environment.NewLine;
            }

            try
            {
                gainCorrection = NeuropixelsV1Helper.TryParseGainCalibrationFile(gainCalibrationFile,
                                                                                 ProbeConfiguration.SpikeAmplifierGain,
                                                                                 ProbeConfiguration.LfpAmplifierGain,
                                                                                 960);
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

            toolStripGainCalSN.Text = gainCorrection.HasValue
                                      ? gainCorrection.Value.SerialNumber.ToString()
                                      : string.IsNullOrEmpty(gainCalibrationFile)
                                        ? NoFileSelected
                                        : InvalidFile;

            textBoxApCorrection.Text = gainCorrection.HasValue
                                       ? gainCorrection.Value.ApGainCorrectionFactor.ToString()
                                       : "";

            textBoxLfpCorrection.Text = gainCorrection.HasValue
                                        ? gainCorrection.Value.LfpGainCorrectionFactor.ToString()
                                        : "";

            ChannelConfiguration.Visible = adcCalibration.HasValue && gainCorrection.HasValue && !string.IsNullOrEmpty(textBoxProbeInterfaceFileName.Text);

            if (toolStripAdcCalSN.Text == NoFileSelected)
                toolStripLabelAdcCalibrationSN.Image = Properties.Resources.StatusWarningImage;
            else if (toolStripAdcCalSN.Text == InvalidFile)
                toolStripLabelAdcCalibrationSN.Image = Properties.Resources.StatusCriticalImage;
            else if (toolStripGainCalSN.Text != NoFileSelected && toolStripGainCalSN.Text != InvalidFile && toolStripAdcCalSN.Text != toolStripGainCalSN.Text)
                toolStripLabelAdcCalibrationSN.Image = Properties.Resources.StatusBlockedImage;
            else
                toolStripLabelAdcCalibrationSN.Image = Properties.Resources.StatusReadyImage;

            if (toolStripGainCalSN.Text == NoFileSelected)
                toolStripLabelGainCalibrationSn.Image = Properties.Resources.StatusWarningImage;
            else if (toolStripGainCalSN.Text == InvalidFile)
                toolStripLabelGainCalibrationSn.Image = Properties.Resources.StatusCriticalImage;
            else if (toolStripAdcCalSN.Text != NoFileSelected && toolStripAdcCalSN.Text != InvalidFile && toolStripAdcCalSN.Text != toolStripGainCalSN.Text)
                toolStripLabelGainCalibrationSn.Image = Properties.Resources.StatusBlockedImage;
            else
                toolStripLabelGainCalibrationSn.Image = Properties.Resources.StatusReadyImage;
        }

        private void ChooseGainCalibrationFile_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                CheckFileExists = true,
                Filter = "Gain calibration files (*_gainCalValues.csv)|*_gainCalValues.csv|All Files|*.*",
                FilterIndex = 0,
                InitialDirectory = File.Exists(textBoxGainCalibrationFile.Text) ?
                                   Path.GetDirectoryName(textBoxGainCalibrationFile.Text) :
                                   ""

            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBoxGainCalibrationFile.Text = ofd.FileName;
            }

            CheckStatus();
        }

        private void ChooseAdcCalibrationFile_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                CheckFileExists = true,
                Filter = "ADC calibration files (*_ADCCalibration.csv)|*_ADCCalibration.csv|All Files|*.*",
                FilterIndex = 0,
                InitialDirectory = File.Exists(textBoxAdcCalibrationFile.Text) ?
                                   Path.GetDirectoryName(textBoxAdcCalibrationFile.Text) :
                                   ""
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBoxAdcCalibrationFile.Text = ofd.FileName;
            }

            CheckStatus();
        }

        private void ClearSelection_Click(object sender, EventArgs e)
        {
            DeselectContacts();
        }

        private void EnableContacts_Click(object sender, EventArgs e)
        {
            EnableSelectedContacts();
            DeselectContacts();
        }

        private void ViewAdcs_Click(object sender, EventArgs e)
        {
            if (Adcs == null)
                return;

            System.Resources.ResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NeuropixelsV1Dialog));

            var adcForm = new Form()
            {
                Size = new Size(600, 1000),
                Text = "View ADC Correction Values",
                Icon = (Icon)resources.GetObject("$this.Icon"),
                StartPosition = FormStartPosition.CenterParent,
            };

            var dataGridView = new DataGridView
            {
                DataSource = Adcs,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToOrderColumns = false,
                ReadOnly = true,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Dock = DockStyle.Fill,
                Location = new Point(0, 0),
                Margin = new Padding(2),
                Name = "dataGridViewAdcs",
                RowHeadersWidth = 62,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            };
            dataGridView.RowTemplate.Height = 28;

            adcForm.Controls.Add(dataGridView);

            adcForm.ShowDialog();
        }

        private void EnableSelectedContacts()
        {
            var electrodes = NeuropixelsV1eProbeGroup.ToElectrodes(ProbeGroup);

            var selectedElectrodes = electrodes.Where((e, ind) => ChannelConfiguration.SelectedContacts[ind])
                                               .ToArray();

            ProbeGroup.SelectElectrodes(selectedElectrodes);

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
            MoveToVerticalPosition(((TrackBar)sender).Value / 100.0f);
        }

        private void UpdateTrackBarLocation(object sender, EventArgs e)
        {
            trackBarProbePosition.Value = (int)(ChannelConfiguration.GetRelativeVerticalPosition() * 100);
        }

        void ProbeInterfaceFileNameChanged(object sender, EventArgs e)
        {
            CheckStatus();
        }

        void ChooseProbeInterfaceFileName(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                CheckFileExists = false,
                Filter = ProbeInterfaceHelper.ProbeInterfaceFileNameFilter,
                FilterIndex = 0,
                InitialDirectory = File.Exists(textBoxProbeInterfaceFileName.Text) ?
                                   Path.GetDirectoryName(textBoxProbeInterfaceFileName.Text) :
                                   ""
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ConfirmFileSaving(ProbeGroup, ProbeConfiguration.ProbeInterfaceFileName);

                textBoxProbeInterfaceFileName.Text = ofd.FileName;
                if (File.Exists(ofd.FileName) && !ChannelConfiguration.TryUpdateProbeGroupFromFile(ofd.FileName))
                {
                    MessageBox.Show($"Unable to load file '{ofd.FileName}'.");
                }
            }

            CheckForExistingChannelPreset();
            CheckStatus();
        }

        static void ConfirmFileSaving(ProbeGroup probeGroup, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                var result = MessageBox.Show(
                    $"Would you like to save the current channel configuration to \"{fileName}\"?",
                    "Save File?",
                    MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    // NB: Catch all exceptions and show them as a MessageBox; uncaught exceptions will close the GUI without warning
                    try
                    {
                        ProbeInterfaceHelper.SaveExternalProbeInterfaceFile(probeGroup, fileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error Saving Probe Interface File");
                    }
                }
            }
        }

        void DialogClosing(object sender, FormClosingEventArgs e)
        {
            ConfirmFileSaving(ProbeGroup, ProbeConfiguration.ProbeInterfaceFileName);
        }
    }
}
