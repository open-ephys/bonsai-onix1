using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureNeuropixelsV1e"/>.
    /// </summary>
    public partial class NeuropixelsV1eDialog : Form
    {
        readonly NeuropixelsV1eChannelConfigurationDialog ChannelConfiguration;

        private enum ChannelPreset
        {
            BankA,
            BankB,
            BankC,
            SingleColumn,
            Tetrodes,
            None
        }

        NeuropixelsV1eAdc[] Adcs = null;

        double ApGainCorrection = default;
        double LfpGainCorrection = default;

        /// <summary>
        /// Public <see cref="ConfigureNeuropixelsV1e"/> instance that is manipulated by
        /// <see cref="NeuropixelsV1eDialog"/>.
        /// </summary>
        /// <remarks>
        /// When a <see cref="ConfigureNeuropixelsV1e"/> is passed to 
        /// <see cref="NeuropixelsV1eDialog"/>, it is copied and stored in this
        /// variable so that any modifications made to configuration settings can be easily reversed
        /// by not copying the new settings back to the original instance.
        /// </remarks>
        public ConfigureNeuropixelsV1e ConfigureNode { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV1eDialog"/>.
        /// </summary>
        /// <param name="configureNode">A <see cref="ConfigureNeuropixelsV1e"/> object holding the current configuration settings.</param>
        public NeuropixelsV1eDialog(ConfigureNeuropixelsV1e configureNode)
        {
            InitializeComponent();
            Shown += FormShown;

            ConfigureNode = new(configureNode);

            ChannelConfiguration = new(ConfigureNode.ProbeConfiguration)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this,
            };

            panelProbe.Controls.Add(ChannelConfiguration);
            this.AddMenuItemsFromDialogToFileOption(ChannelConfiguration);

            ChannelConfiguration.OnZoom += UpdateTrackBarLocation;
            ChannelConfiguration.OnFileLoad += UpdateChannelPresetIndex;

            comboBoxApGain.DataSource = Enum.GetValues(typeof(NeuropixelsV1Gain));
            comboBoxApGain.SelectedItem = ConfigureNode.ProbeConfiguration.SpikeAmplifierGain;
            comboBoxApGain.SelectedIndexChanged += SelectedIndexChanged;

            comboBoxLfpGain.DataSource = Enum.GetValues(typeof(NeuropixelsV1Gain));
            comboBoxLfpGain.SelectedItem = ConfigureNode.ProbeConfiguration.LfpAmplifierGain;
            comboBoxLfpGain.SelectedIndexChanged += SelectedIndexChanged;

            comboBoxReference.DataSource = Enum.GetValues(typeof(NeuropixelsV1ReferenceSource));
            comboBoxReference.SelectedItem = ConfigureNode.ProbeConfiguration.Reference;
            comboBoxReference.SelectedIndexChanged += SelectedIndexChanged;

            checkBoxSpikeFilter.Checked = ConfigureNode.ProbeConfiguration.SpikeFilter;
            checkBoxSpikeFilter.CheckedChanged += SelectedIndexChanged;

            textBoxAdcCalibrationFile.Text = ConfigureNode.AdcCalibrationFile;

            textBoxGainCalibrationFile.Text = ConfigureNode.GainCalibrationFile;

            comboBoxChannelPresets.DataSource = Enum.GetValues(typeof(ChannelPreset));
            CheckForExistingChannelPreset();
            comboBoxChannelPresets.SelectedIndexChanged += SelectedIndexChanged;

            CheckStatus();
        }

        private void FormShown(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                splitContainer1.Panel2Collapsed = true;
                splitContainer1.Panel2.Hide();

                menuStrip.Visible = false;
            }

            ChannelConfiguration.Show();
            ChannelConfiguration.ConnectResizeEventHandler();
            ChannelConfiguration.OnResizeZedGraph += ResizeTrackBar;
        }

        private void ResizeTrackBar(object sender, EventArgs e)
        {
            if (sender is ChannelConfigurationDialog dialog)
            {
                panelTrackBar.Height = dialog.zedGraphChannels.Size.Height;
                panelTrackBar.Location = new Point(panelProbe.Size.Width - panelTrackBar.Width, ChannelConfiguration.zedGraphChannels.Location.Y);
            }
        }

        private void FileTextChanged(object sender, EventArgs e)
        {
            if (sender is TextBox textBox && textBox != null)
            {
                if (textBox.Name == nameof(textBoxGainCalibrationFile))
                {
                    ConfigureNode.GainCalibrationFile = textBox.Text;
                    ParseGainCalibrationFile();
                }
                else if (textBox.Name == nameof(textBoxAdcCalibrationFile))
                {
                    ConfigureNode.AdcCalibrationFile = textBox.Text;
                    ParseAdcCalibrationFile();
                }
            }

            CheckStatus();
        }

        private void ParseAdcCalibrationFile()
        {
            if (ConfigureNode.AdcCalibrationFile != null && ConfigureNode.AdcCalibrationFile != "")
            {
                if (File.Exists(ConfigureNode.AdcCalibrationFile))
                {
                    StreamReader adcFile = new(ConfigureNode.AdcCalibrationFile);

                    adcCalibrationSN.Text = ulong.Parse(adcFile.ReadLine()).ToString();

                    Adcs = NeuropixelsV1Helper.ParseAdcCalibrationFile(adcFile);

                    dataGridViewAdcs.DataSource = Adcs;

                    adcFile.Close();
                }
            }
        }

        private void ParseGainCalibrationFile()
        {
            if (ConfigureNode.GainCalibrationFile != null && ConfigureNode.GainCalibrationFile != "")
            {
                if (File.Exists(ConfigureNode.GainCalibrationFile))
                {
                    StreamReader gainCalibrationFile = new(ConfigureNode.GainCalibrationFile);

                    gainCalibrationSN.Text = ulong.Parse(gainCalibrationFile.ReadLine()).ToString();

                    var gainCorrection = NeuropixelsV1Helper.ParseGainCalibrationFile(gainCalibrationFile, ConfigureNode.ProbeConfiguration.SpikeAmplifierGain, ConfigureNode.ProbeConfiguration.LfpAmplifierGain);

                    ApGainCorrection = gainCorrection.AP;
                    LfpGainCorrection = gainCorrection.LFP;

                    gainCalibrationFile.Close();
                }
            }
        }

        private void SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox != null)
            {
                if (comboBox.Name == nameof(comboBoxApGain))
                {
                    ConfigureNode.ProbeConfiguration.SpikeAmplifierGain = (NeuropixelsV1Gain)comboBox.SelectedItem;
                    ParseGainCalibrationFile();

                    if (ApGainCorrection != default && LfpGainCorrection != default)
                    {
                        ShowCorrectionValues();
                    }
                }
                else if (comboBox.Name == nameof(comboBoxLfpGain))
                {
                    ConfigureNode.ProbeConfiguration.LfpAmplifierGain = (NeuropixelsV1Gain)comboBox.SelectedItem;
                    ParseGainCalibrationFile();

                    if (ApGainCorrection != default && LfpGainCorrection != default)
                    {
                        ShowCorrectionValues();
                    }
                }
                else if (comboBox.Name == nameof(comboBoxReference))
                {
                    ConfigureNode.ProbeConfiguration.Reference = (NeuropixelsV1ReferenceSource)comboBox.SelectedItem;
                }
                else if (comboBox.Name == nameof(comboBoxChannelPresets))
                {
                    if ((ChannelPreset)comboBox.SelectedItem != ChannelPreset.None)
                    {
                        SetChannelPreset((ChannelPreset)comboBox.SelectedItem);
                    }
                }
            }
            else if (sender is CheckBox checkBox && checkBox != null)
            {
                if (checkBox.Name == nameof(checkBoxSpikeFilter))
                {
                    ConfigureNode.ProbeConfiguration.SpikeFilter = checkBox.Checked;
                }
            }
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

        private void UpdateChannelPresetIndex(object sender, EventArgs e)
        {
            CheckForExistingChannelPreset();
        }

        private void CheckStatus()
        {
            if (gainCalibrationSN.Text == "" || adcCalibrationSN.Text == "" || gainCalibrationSN.Text != adcCalibrationSN.Text)
            {
                toolStripStatus.Image = Properties.Resources.StatusWarningImage;
                toolStripStatus.Text = "Serial number mismatch";
            }
            else if (!ChannelConfiguration.ChannelConfiguration.ValidateDeviceChannelIndices())
            {
                toolStripStatus.Image = Properties.Resources.StatusBlockedImage;
                toolStripStatus.Text = "Invalid channels selected";
            }
            else
            {
                toolStripStatus.Image = Properties.Resources.StatusReadyImage;
                toolStripStatus.Text = "";
            }

            if (ApGainCorrection != default && LfpGainCorrection != default)
            {
                ShowCorrectionValues();
            }
            else
            {
                textBoxApGainCorrection.Text = "";
                textBoxLfpGainCorrection.Text = "";
            }

            if (ApGainCorrection != default && LfpGainCorrection != default && Adcs != null)
            {
                panelProbe.Visible = true;
            }
            else
            {
                panelProbe.Visible = false;
            }
        }

        private void ShowCorrectionValues()
        {
            textBoxApGainCorrection.Text = ApGainCorrection.ToString();
            textBoxLfpGainCorrection.Text = LfpGainCorrection.ToString();
        }

        private void ButtonClick(object sender, EventArgs e)
        {
            if (sender is Button button && button != null)
            {
                if (button.Name == nameof(buttonOkay))
                {
                    DialogResult = DialogResult.OK;
                }
                else if (button.Name == nameof(buttonChooseGainCalibrationFile))
                {
                    var ofd = new OpenFileDialog()
                    {
                        CheckFileExists = true,
                        Filter = "Gain calibration files (*_gainCalValues.csv)|*_gainCalValues.csv|All Files|*.*",
                        FilterIndex = 0
                    };

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        textBoxGainCalibrationFile.Text = ofd.FileName;
                    }
                }
                else if (button.Name == nameof(buttonClearGainCalibrationFile))
                {
                    textBoxGainCalibrationFile.Text = "";
                    ApGainCorrection = default;
                    LfpGainCorrection = default;

                    panelProbe.Visible = false;

                    CheckStatus();
                }
                else if (button.Name == nameof(buttonChooseAdcCalibrationFile))
                {
                    var ofd = new OpenFileDialog()
                    {
                        CheckFileExists = true,
                        Filter = "ADC calibration files (*_ADCCalibration.csv)|*_ADCCalibration.csv|All Files|*.*",
                        FilterIndex = 0
                    };

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        textBoxAdcCalibrationFile.Text = ofd.FileName;
                    }
                }
                else if (button.Name == nameof(buttonClearAdcCalibrationFile))
                {
                    textBoxAdcCalibrationFile.Text = "";
                    Adcs = null;
                    dataGridViewAdcs.DataSource = null;

                    CheckStatus();
                }
                else if (button.Name == nameof(buttonResetZoom))
                {
                    ResetZoom();
                }
                else if (button.Name == nameof(buttonClearSelections))
                {
                    ChannelConfiguration.SetAllSelections(false);
                    ChannelConfiguration.HighlightEnabledContacts();
                    ChannelConfiguration.HighlightSelectedContacts();
                    ChannelConfiguration.UpdateContactLabels();
                    ChannelConfiguration.RefreshZedGraph();
                }
                else if (button.Name == nameof(buttonEnableContacts))
                {
                    EnableSelectedContacts();
                    ChannelConfiguration.SetAllSelections(false);
                    ChannelConfiguration.HighlightEnabledContacts();
                    ChannelConfiguration.HighlightSelectedContacts();
                    ChannelConfiguration.UpdateContactLabels();
                    ChannelConfiguration.RefreshZedGraph();
                }
            }
        }

        private void EnableSelectedContacts()
        {
            var electrodes = NeuropixelsV1eProbeGroup.ToElectrodes(ChannelConfiguration.ProbeConfiguration.ChannelConfiguration);

            var selectedElectrodes = electrodes.Where((e, ind) => ChannelConfiguration.SelectedContacts[ind])
                                               .ToList();

            ChannelConfiguration.EnableElectrodes(selectedElectrodes);

            CheckForExistingChannelPreset();
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
            if (sender is TrackBar trackBar && trackBar != null)
            {
                if (trackBar.Name == nameof(trackBarProbePosition))
                {
                    MoveToVerticalPosition(trackBar.Value / 100.0f);
                }
            }
        }

        private void UpdateTrackBarLocation(object sender, EventArgs e)
        {
            trackBarProbePosition.Value = (int)(ChannelConfiguration.GetRelativeVerticalPosition() * 100);
        }
    }
}
