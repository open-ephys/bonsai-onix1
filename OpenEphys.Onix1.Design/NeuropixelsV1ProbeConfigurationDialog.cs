using System;
using System.ComponentModel;
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

        enum ChannelPresetNp1
        {
            BankA,
            BankB,
            BankC,
            SingleColumn,
            Tetrodes,
            None
        }

        enum ChannelPresetUhd
        {
            BankA,
            BankB,
            BankC,
            BankD,
            BankE,
            BankF,
            BankG,
            BankH,
            BankI,
            BankJ,
            BankK,
            BankL,
            BankM,
            BankN,
            BankO,
            BankP,
            None
        }

        /// <summary>
        /// Gets the <see cref="NeuropixelsV1ProbeConfiguration"/> object.
        /// </summary>
        public NeuropixelsV1ProbeConfiguration ProbeConfiguration
        {
            get => ChannelConfiguration.ProbeConfiguration;
        }

        /// <inheritdoc cref="ConfigureNeuropixelsV1e.InvertPolarity"/>
        public bool InvertPolarity { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV1Dialog"/>.
        /// </summary>
        /// <param name="probeConfiguration">A <see cref="NeuropixelsV1ProbeConfiguration"/> object holding the current configuration settings.</param>
        /// <param name="adcCalibrationFile">String defining the path to the ADC calibration file.</param>
        /// <param name="gainCalibrationFile">String defining the path to the gain calibration file.</param>
        /// <param name="invertPolarity">Boolean denoting whether or not to invert the polarity of neural data.</param>
        public NeuropixelsV1ProbeConfigurationDialog(NeuropixelsV1ProbeConfiguration probeConfiguration, string adcCalibrationFile, string gainCalibrationFile, bool invertPolarity)
        {
            InitializeComponent();
            Shown += FormShown;

            ChannelConfiguration = new(probeConfiguration)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this,
            };

            InvertPolarity = invertPolarity;

            panelProbe.Controls.Add(ChannelConfiguration);
            this.AddMenuItemsFromDialogToFileOption(ChannelConfiguration);

            ChannelConfiguration.OnZoom += UpdateTrackBarLocation;
            ChannelConfiguration.OnFileLoad += OnFileLoadEvent;

            comboBoxProbeType.DataSource = Enum.GetValues(typeof(NeuropixelsV1ProbeType));
            comboBoxProbeType.SelectedItem = ProbeConfiguration.ProbeType;
            comboBoxProbeType.SelectedIndexChanged += ProbeTypeIndexChanged;

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

            checkBoxInvertPolarity.Checked = InvertPolarity;
            checkBoxInvertPolarity.CheckedChanged += InvertPolarityIndexChanged;

            textBoxAdcCalibrationFile.Text = adcCalibrationFile;

            textBoxGainCalibrationFile.Text = gainCalibrationFile;

            comboBoxChannelPresets.DataSource = GetComboBoxChannelPresets(ProbeConfiguration.ProbeType);
            CheckForExistingChannelPreset(ProbeConfiguration.ProbeType);
            comboBoxChannelPresets.SelectedIndexChanged += ChannelPresetIndexChanged;

            ConfigureControls(ProbeConfiguration.ProbeType);

            CheckStatus();
        }

        void UpdateProbeConfiguration()
        {
            ChannelConfiguration.ProbeConfiguration = new((NeuropixelsV1eProbeGroup)ChannelConfiguration.ProbeGroup, ProbeConfiguration.ProbeType,
                ProbeConfiguration.SpikeAmplifierGain, ProbeConfiguration.LfpAmplifierGain, ProbeConfiguration.Reference, ProbeConfiguration.SpikeFilter);

            ChannelConfiguration.DrawProbeGroup();
            ChannelConfiguration.ResetZoom();
            ChannelConfiguration.RefreshZedGraph();

            comboBoxChannelPresets.SelectedIndexChanged -= ChannelPresetIndexChanged; // NB: Temporarily detach handler so the loaded electrode configuration is respected
            comboBoxChannelPresets.DataSource = GetComboBoxChannelPresets(ProbeConfiguration.ProbeType);
            comboBoxChannelPresets.SelectedIndexChanged += ChannelPresetIndexChanged;
        }

        private void ProbeTypeIndexChanged(object sender, EventArgs e)
        {
            var probeType = (NeuropixelsV1ProbeType)((ComboBox)sender).SelectedItem;

            if (probeType != ProbeConfiguration.ProbeType)
            {
                ProbeConfiguration.ProbeType = probeType;
                ChannelConfiguration.LoadDefaultProbeGroup();
                UpdateProbeConfiguration();
            }

            ConfigureControls(ProbeConfiguration.ProbeType);
        }

        void ConfigureControls(NeuropixelsV1ProbeType probeType)
        {
            if (probeType == NeuropixelsV1ProbeType.NP1)
            {
                buttonEnableContacts.Enabled = true;
                buttonClearSelections.Enabled = true;
                ChannelConfiguration.SetSelectElectrodesStatus(true);

            }
            else if (probeType == NeuropixelsV1ProbeType.UHD)
            {
                buttonEnableContacts.Enabled = false;
                buttonClearSelections.Enabled = false;
                ChannelConfiguration.SetSelectElectrodesStatus(false);
            }
            else
                throw new InvalidEnumArgumentException(nameof(NeuropixelsV1ProbeType));
        }

        static Array GetComboBoxChannelPresets(NeuropixelsV1ProbeType probeType)
        {
            return probeType switch
            {
                NeuropixelsV1ProbeType.NP1 => Enum.GetValues(typeof(ChannelPresetNp1)),
                NeuropixelsV1ProbeType.UHD => Enum.GetValues(typeof(ChannelPresetUhd)),
                _ => throw new InvalidEnumArgumentException(nameof(NeuropixelsV1ProbeType))
            };
        }

        private void InvertPolarityIndexChanged(object sender, EventArgs e)
        {
            InvertPolarity = ((CheckBox)sender).Checked;
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
            if (ProbeConfiguration.ProbeType == NeuropixelsV1ProbeType.NP1)
            {
                var channelPreset = (ChannelPresetNp1)((ComboBox)sender).SelectedItem;

                if (channelPreset != ChannelPresetNp1.None)
                {
                    SetChannelPreset(channelPreset);
                }
            }
            else if (ProbeConfiguration.ProbeType == NeuropixelsV1ProbeType.UHD)
            {
                var channelPreset = (ChannelPresetUhd)((ComboBox)sender).SelectedItem;

                if (channelPreset != ChannelPresetUhd.None)
                {
                    SetChannelPreset(channelPreset);
                }
            }

            ChannelConfiguration.HighlightEnabledContacts();
            ChannelConfiguration.HighlightSelectedContacts();
            ChannelConfiguration.UpdateContactLabels();
            ChannelConfiguration.RefreshZedGraph();
        }

        private void SpikeFilterIndexChanged(object sender, EventArgs e)
        {
            ProbeConfiguration.SpikeFilter = ((CheckBox)sender).Checked;
        }

        void SetChannelPreset(ChannelPresetUhd preset)
        {
            var electrodes = NeuropixelsV1eProbeGroup.ToElectrodes(ProbeConfiguration.ProbeGroup, ProbeConfiguration.ProbeType);

            switch (preset)
            {
                case ChannelPresetUhd.BankA:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.A).ToArray());
                    break;

                case ChannelPresetUhd.BankB:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.B).ToArray());
                    break;

                case ChannelPresetUhd.BankC:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.C).ToArray());
                    break;

                case ChannelPresetUhd.BankD:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.D).ToArray());
                    break;

                case ChannelPresetUhd.BankE:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.E).ToArray());
                    break;

                case ChannelPresetUhd.BankF:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.F).ToArray());
                    break;

                case ChannelPresetUhd.BankG:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.G).ToArray());
                    break;

                case ChannelPresetUhd.BankH:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.H).ToArray());
                    break;

                case ChannelPresetUhd.BankI:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.I).ToArray());
                    break;

                case ChannelPresetUhd.BankJ:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.J).ToArray());
                    break;

                case ChannelPresetUhd.BankK:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.K).ToArray());
                    break;

                case ChannelPresetUhd.BankL:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.L).ToArray());
                    break;

                case ChannelPresetUhd.BankM:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.M).ToArray());
                    break;

                case ChannelPresetUhd.BankN:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.N).ToArray());
                    break;

                case ChannelPresetUhd.BankO:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.O).ToArray());
                    break;

                case ChannelPresetUhd.BankP:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.P).ToArray());
                    break;
            }
        }

        void SetChannelPreset(ChannelPresetNp1 preset)
        {
            var electrodes = NeuropixelsV1eProbeGroup.ToElectrodes(ProbeConfiguration.ProbeGroup, ProbeConfiguration.ProbeType);

            switch (preset)
            {
                case ChannelPresetNp1.BankA:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.A).ToArray());
                    break;

                case ChannelPresetNp1.BankB:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.B).ToArray());
                    break;

                case ChannelPresetNp1.BankC:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.C ||
                                                                             (e.Bank == NeuropixelsV1Bank.B && e.Index >= 576)).ToArray());
                    break;

                case ChannelPresetNp1.SingleColumn:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Index % 2 == 0 && e.Bank == NeuropixelsV1Bank.A) ||
                                                                              (e.Index % 2 == 1 && e.Bank == NeuropixelsV1Bank.B)).ToArray());
                    break;

                case ChannelPresetNp1.Tetrodes:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Index % 8 < 4 && e.Bank == NeuropixelsV1Bank.A) ||
                                                                              (e.Index % 8 > 3 && e.Bank == NeuropixelsV1Bank.B)).ToArray());
                    break;
            }
        }

        void CheckForExistingChannelPreset(NeuropixelsV1ProbeType probeType)
        {
            if (probeType == NeuropixelsV1ProbeType.NP1)
                CheckForChannelPresetNp1();

            else if (probeType == NeuropixelsV1ProbeType.UHD)
                CheckForChannelPresetUhd();
        }

        void CheckForChannelPresetNp1()
        {
            var channelMap = ProbeConfiguration.ChannelMap;

            if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.A))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetNp1.BankA;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.B))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetNp1.BankB;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.C ||
                                        (e.Bank == NeuropixelsV1Bank.B && e.Index >= 576)))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetNp1.BankC;
            }
            else if (channelMap.All(e => (e.Index % 2 == 0 && e.Bank == NeuropixelsV1Bank.A) ||
                                         (e.Index % 2 == 1 && e.Bank == NeuropixelsV1Bank.B)))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetNp1.SingleColumn;
            }
            else if (channelMap.All(e => (e.Index % 8 < 4 && e.Bank == NeuropixelsV1Bank.A) ||
                                         (e.Index % 8 > 3 && e.Bank == NeuropixelsV1Bank.B)))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetNp1.Tetrodes;
            }
            else
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetNp1.None;
            }
        }

        void CheckForChannelPresetUhd()
        {
            var channelMap = ProbeConfiguration.ChannelMap;

            if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.A))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetUhd.BankA;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.B))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetUhd.BankB;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.C))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetUhd.BankC;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.D))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetUhd.BankD;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.E))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetUhd.BankE;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.F))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetUhd.BankF;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.G))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetUhd.BankG;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.H))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetUhd.BankH;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.I))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetUhd.BankI;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.J))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetUhd.BankJ;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.K))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetUhd.BankK;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.L))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetUhd.BankL;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.M))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetUhd.BankM;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.N))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetUhd.BankN;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.O))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetUhd.BankO;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV1Bank.P))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPresetUhd.BankP;
            }
            else
                comboBoxChannelPresets.SelectedItem = ChannelPresetUhd.None;
        }

        private void OnFileLoadEvent(object sender, EventArgs e)
        {
            ProbeConfiguration.ProbeType = NeuropixelsV1eProbeGroup.GetProbeTypeFromProbeName(ChannelConfiguration.ProbeGroup.Probes.First().Annotations.Name);
            comboBoxProbeType.SelectedItem = ProbeConfiguration.ProbeType;
            UpdateProbeConfiguration();
            ConfigureControls(ProbeConfiguration.ProbeType);
            CheckForExistingChannelPreset(ProbeConfiguration.ProbeType);
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
            var electrodes = NeuropixelsV1eProbeGroup.ToElectrodes(ProbeConfiguration.ProbeGroup, ProbeConfiguration.ProbeType);

            var selectedElectrodes = electrodes.Where((e, ind) => ChannelConfiguration.SelectedContacts[ind])
                                               .ToArray();

            ChannelConfiguration.EnableElectrodes(selectedElectrodes);

            CheckForExistingChannelPreset(ProbeConfiguration.ProbeType);
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
