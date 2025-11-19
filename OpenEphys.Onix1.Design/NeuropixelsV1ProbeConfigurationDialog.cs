using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
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
        /// Public <see cref="NeuropixelsV1ProbeConfiguration"/> interface that is manipulated by
        /// <see cref="NeuropixelsV1ProbeConfigurationDialog"/>.
        /// </summary>
        /// <remarks>
        /// When a <see cref="NeuropixelsV1ProbeConfiguration"/> is passed to 
        /// <see cref="NeuropixelsV1ProbeConfigurationDialog"/>, it is copied and stored in this
        /// variable so that any modifications made to configuration settings can be easily reversed
        /// by not copying the new settings back to the original instance.
        /// </remarks>
        public NeuropixelsV1ProbeConfiguration ProbeConfiguration
        {
            get => ((IConfigureNeuropixelsV1)propertyGrid.SelectedObject).ProbeConfiguration;
            set => ((IConfigureNeuropixelsV1)propertyGrid.SelectedObject).ProbeConfiguration = value;
        }

        [Obsolete]
        IConfigureNeuropixelsV1 ConfigureNode
        {
            get => (IConfigureNeuropixelsV1)propertyGrid.SelectedObject;
            set => propertyGrid.SelectedObject = value;
        }

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
        /// <param name="configureNode">Existing configuration node.</param>
        public NeuropixelsV1ProbeConfigurationDialog(IConfigureNeuropixelsV1 configureNode)
        {
            InitializeComponent();
            Shown += FormShown;

            ChannelConfiguration = new(configureNode.ProbeConfiguration);
            ChannelConfiguration.SetChildFormProperties(this).AddDialogToPanel(panelProbe);
            this.AddMenuItemsFromDialogToFileOption(ChannelConfiguration);

            panelProbe.Controls.Add(ChannelConfiguration);

            ChannelConfiguration.OnZoom += UpdateTrackBarLocation;
            ChannelConfiguration.OnFileLoad += OnFileLoadEvent;

            propertyGrid.SelectedObject = configureNode;
            bindingSource.DataSource = configureNode;

            // NB: Needed to capture mouse scroll wheel updates
            void ForceBindingUpdate(object sender, EventArgs e)
            {
                var control = sender as Control;

                foreach (Binding binding in control.DataBindings)
                {
                    binding.WriteValue();
                }

                bindingSource.ResetCurrentItem();
            }

            comboBoxApGain.DataSource = Enum.GetValues(typeof(NeuropixelsV1Gain));
            comboBoxApGain.DataBindings.Add("SelectedItem",
                bindingSource,
                $"{nameof(configureNode.ProbeConfiguration)}.{nameof(configureNode.ProbeConfiguration.SpikeAmplifierGain)}",
                false,
                DataSourceUpdateMode.OnPropertyChanged);
            comboBoxApGain.SelectedIndexChanged += (sender, e) =>
            {
                ForceBindingUpdate(sender, e);
                CheckStatus();
            };

            comboBoxLfpGain.DataSource = Enum.GetValues(typeof(NeuropixelsV1Gain));
            comboBoxLfpGain.DataBindings.Add("SelectedItem",
                bindingSource,
                $"{nameof(configureNode.ProbeConfiguration)}.{nameof(configureNode.ProbeConfiguration.LfpAmplifierGain)}",
                false,
                DataSourceUpdateMode.OnPropertyChanged);
            comboBoxLfpGain.SelectedIndexChanged += (sender, e) =>
            {
                ForceBindingUpdate(sender, e);
                CheckStatus();
            };

            comboBoxReference.DataSource = Enum.GetValues(typeof(NeuropixelsV1ReferenceSource));
            comboBoxReference.DataBindings.Add("SelectedItem",
                bindingSource,
                $"{nameof(configureNode.ProbeConfiguration)}.{nameof(configureNode.ProbeConfiguration.Reference)}",
                false,
                DataSourceUpdateMode.OnPropertyChanged);
            comboBoxReference.SelectedIndexChanged += ForceBindingUpdate;

            checkBoxSpikeFilter.DataBindings.Add("Checked",
                bindingSource,
                $"{nameof(configureNode.ProbeConfiguration)}.{nameof(configureNode.ProbeConfiguration.SpikeFilter)}",
                false,
                DataSourceUpdateMode.OnPropertyChanged);

            checkBoxInvertPolarity.DataBindings.Add("Checked",
                bindingSource,
                $"{nameof(configureNode.ProbeConfiguration)}.{nameof(configureNode.ProbeConfiguration.InvertPolarity)}",
                false,
                DataSourceUpdateMode.OnPropertyChanged);

            textBoxAdcCalibrationFile.DataBindings.Add("Text",
                bindingSource,
                $"{nameof(configureNode.ProbeConfiguration)}.{nameof(configureNode.ProbeConfiguration.AdcCalibrationFileName)}",
                false,
                DataSourceUpdateMode.OnPropertyChanged);
            textBoxAdcCalibrationFile.TextChanged += (sender, e) => CheckStatus();

            textBoxGainCalibrationFile.DataBindings.Add("Text",
                bindingSource,
                $"{nameof(configureNode.ProbeConfiguration)}.{nameof(configureNode.ProbeConfiguration.GainCalibrationFileName)}",
                false,
                DataSourceUpdateMode.OnPropertyChanged);
            textBoxGainCalibrationFile.TextChanged += (sender, e) => CheckStatus();

            comboBoxChannelPresets.DataSource = Enum.GetValues(typeof(ChannelPreset));
            CheckForExistingChannelPreset();
            comboBoxChannelPresets.SelectedIndexChanged += (sender, e) =>
            {
                var channelPreset = ((ComboBox)sender).SelectedItem is ChannelPreset preset
                    ? preset
                    : throw new InvalidEnumArgumentException($"Invalid channel preset value found.");

                if (channelPreset != ChannelPreset.None)
                {
                    SetChannelPreset(channelPreset);
                }
            };

            CheckStatus();

            bindingSource.ListChanged += (sender, eventArgs) => propertyGrid.Refresh();

            tabControl1.SelectedIndexChanged += (sender, eventArgs) =>
            {
                if (tabControl1.SelectedTab == tabPageProperties)
                    propertyGrid.Refresh();

                else if (tabControl1.SelectedTab == tabPageConfiguration)
                    bindingSource.ResetCurrentItem();
            };
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
        }

        private void SetChannelPreset(ChannelPreset preset)
        {
            var electrodes = NeuropixelsV1eProbeGroup.ToElectrodes(ProbeConfiguration.ProbeGroup);

            switch (preset)
            {
                case ChannelPreset.BankA:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.A).ToArray());
                    break;

                case ChannelPreset.BankB:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.B).ToArray());
                    break;

                case ChannelPreset.BankC:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV1Bank.C ||
                                                                             (e.Bank == NeuropixelsV1Bank.B && e.Index >= 576)).ToArray());
                    break;

                case ChannelPreset.SingleColumn:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Index % 2 == 0 && e.Bank == NeuropixelsV1Bank.A) ||
                                                                              (e.Index % 2 == 1 && e.Bank == NeuropixelsV1Bank.B)).ToArray());
                    break;

                case ChannelPreset.Tetrodes:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Index % 8 < 4 && e.Bank == NeuropixelsV1Bank.A) ||
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
            var channelMap = ProbeConfiguration.ChannelMap;

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
            // NB: Ensure that the newly loaded ProbeGroup in the ChannelConfigurationDialog is reflected here.
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

            propertyGrid.Refresh();
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

            ResourceManager resources = new ComponentResourceManager(typeof(NeuropixelsV1Dialog));

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
            var electrodes = NeuropixelsV1eProbeGroup.ToElectrodes(ChannelConfiguration.ProbeConfiguration.ProbeGroup);

            var selectedElectrodes = electrodes.Where((e, ind) => ChannelConfiguration.SelectedContacts[ind])
                                               .ToArray();

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

        void TextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            CheckStatus();
        }
    }
}
