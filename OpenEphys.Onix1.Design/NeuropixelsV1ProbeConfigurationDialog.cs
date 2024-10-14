using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="NeuropixelsV1ProbeConfiguration"/>.
    /// </summary>
    public partial class NeuropixelsV1ProbeConfigurationDialog : Form
    {
        readonly NeuropixelsV1ChannelConfigurationDialog ChannelConfiguration;

        private NeuropixelsV1Adc[] Adcs = null;

        private enum ChannelPreset
        {
            BankA,
            BankB,
            BankC,
            SingleColumn,
            Tetrodes,
            None
        }

        /// <summary>
        /// Public <see cref="IConfigureNeuropixelsV2"/> interface that is manipulated by
        /// <see cref="NeuropixelsV2eDialog"/>.
        /// </summary>
        /// <remarks>
        /// When a <see cref="IConfigureNeuropixelsV2"/> is passed to 
        /// <see cref="NeuropixelsV1Dialog"/>, it is copied and stored in this
        /// variable so that any modifications made to configuration settings can be easily reversed
        /// by not copying the new settings back to the original instance.
        /// </remarks>
        public NeuropixelsV1ProbeConfiguration ProbeConfiguration { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV1Dialog"/>.
        /// </summary>
        /// <param name="probeConfiguration">A <see cref="NeuropixelsV1ProbeConfiguration"/> object holding the current configuration settings.</param>
        /// <param name="adcCalibrationFile">String defining the path to the ADC calibration file.</param>
        /// <param name="gainCalibrationFile">String defining the path to the gain calibration file.</param>
        public NeuropixelsV1ProbeConfigurationDialog(NeuropixelsV1ProbeConfiguration probeConfiguration, string adcCalibrationFile, string gainCalibrationFile)
        {
            InitializeComponent();
            Shown += FormShown;

            ProbeConfiguration = new(probeConfiguration);

            ChannelConfiguration = new(ProbeConfiguration)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this,
            };

            panelProbe.Controls.Add(ChannelConfiguration);
            this.AddMenuItemsFromDialogToFileOption(ChannelConfiguration);

            ChannelConfiguration.OnZoom += UpdateTrackBarLocation;
            ChannelConfiguration.OnFileLoad += OnFileLoadEvent;

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

            textBoxAdcCalibrationFile.Text = adcCalibrationFile;

            textBoxGainCalibrationFile.Text = gainCalibrationFile;

            comboBoxChannelPresets.DataSource = Enum.GetValues(typeof(ChannelPreset));
            CheckForExistingChannelPreset();
            comboBoxChannelPresets.SelectedIndexChanged += ChannelPresetIndexChanged;

            CheckStatus();
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
            ChannelConfiguration.OnResizeZedGraph += ResizeTrackBar;
        }

        private void ResizeTrackBar(object sender, EventArgs e)
        {
            panelTrackBar.Height = ((ChannelConfigurationDialog)sender).zedGraphChannels.Size.Height;
            panelTrackBar.Location = new Point(panelProbe.Size.Width - panelTrackBar.Width, ChannelConfiguration.zedGraphChannels.Location.Y);
        }

        private void GainCalibrationFileTextChanged(object sender, EventArgs e)
        {
            CheckStatus();
        }

        private void AdcCalibrationFileTextChanged(object sender, EventArgs e)
        {
            CheckStatus();
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
            var probeConfiguration = ChannelConfiguration.ProbeConfiguration;
            var electrodes = NeuropixelsV1eProbeGroup.ToElectrodes(ChannelConfiguration.ProbeConfiguration.ChannelConfiguration);

            switch (preset)
            {
                case ChannelPreset.BankA:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.A).ToList());
                    break;

                case ChannelPreset.BankB:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.B).ToList());
                    break;

                case ChannelPreset.BankC:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.C ||
                                                                             (e.Bank == NeuropixelsV1Bank.B && e.Index >= 576)).ToList());
                    break;

                case ChannelPreset.SingleColumn:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Index % 2 == 0 && e.Bank == NeuropixelsV1Bank.A) ||
                                                                              (e.Index % 2 == 1 && e.Bank == NeuropixelsV1Bank.B)).ToList());
                    break;

                case ChannelPreset.Tetrodes:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Index % 8 < 4 && e.Bank == NeuropixelsV1Bank.A) ||
                                                                              (e.Index % 8 > 3 && e.Bank == NeuropixelsV1Bank.B)).ToList());
                    break;
            }

            ChannelConfiguration.HighlightEnabledContacts();
            ChannelConfiguration.HighlightSelectedContacts();
            ChannelConfiguration.UpdateContactLabels();
            ChannelConfiguration.RefreshZedGraph();
        }

        private void CheckForExistingChannelPreset()
        {
            var channelMap = ChannelConfiguration.ProbeConfiguration.ChannelMap;

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
            // NB: Ensure that the newly loaded ProbeConfiguration in the ChannelConfigurationDialog is reflected here.
            ProbeConfiguration = ChannelConfiguration.ProbeConfiguration;
            CheckForExistingChannelPreset();
        }

        private void CheckStatus()
        {
            const string NoFileSelected = "No file selected.";
            const string InvalidFile = "Invalid file.";

            NeuropixelsV1AdcCalibration? adcCalibration;

            string adcCalibrationFile = textBoxAdcCalibrationFile.Text;

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

            panelProbe.Visible = adcCalibration.HasValue && gainCorrection.HasValue;

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

        private void ResetZoom_Click(object sender, EventArgs e)
        {
            ResetZoom();
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
            var electrodes = NeuropixelsV1eProbeGroup.ToElectrodes(ChannelConfiguration.ProbeConfiguration.ChannelConfiguration);

            var selectedElectrodes = electrodes.Where((e, ind) => ChannelConfiguration.SelectedContacts[ind])
                                               .ToList();

            ChannelConfiguration.EnableElectrodes(selectedElectrodes);

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

        private void ResetZoom()
        {
            ChannelConfiguration.ResetZoom();
            ChannelConfiguration.RefreshZedGraph();
            ChannelConfiguration.DrawScale();
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
    }
}
