using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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

        internal event EventHandler OnStateChange;
        internal event EventHandler OnProbeConfigurationChange;
        internal event EventHandler OnPropertyValueChanged;

        internal bool HasChanges
        {
            get => ChannelConfiguration.HasChanges;
            private set => ChannelConfiguration.HasChanges = value;
        }

        internal NeuropixelsV2ProbeConfiguration ProbeConfiguration
        {
            get => ChannelConfiguration.ProbeConfiguration as NeuropixelsV2ProbeConfiguration;
            set => ChannelConfiguration.ProbeConfiguration = value;
        }

        NeuropixelsV2eProbeGroup ProbeGroup
        {
            get => ChannelConfiguration.ProbeGroup as NeuropixelsV2eProbeGroup;
            set => ChannelConfiguration.ProbeGroup = value;
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
        /// <param name="probeName">The name of the probe.</param>
        public NeuropixelsV2eProbeConfigurationDialog(NeuropixelsV2ProbeConfiguration configuration, bool isBeta, string probeName)
        {
            InitializeComponent();
            Shown += FormShown;
            FormClosing += DialogClosing;

            Text += ": " + probeName;

            probeConfigurations = new()
            {
                [ProbeType.SingleShank] = new NeuropixelsV2SingleShankProbeConfiguration(),
                [ProbeType.QuadShank] = new NeuropixelsV2QuadShankProbeConfiguration()
            };

            var currentProbeType = GetCurrentProbeType(configuration);
            probeConfigurations[currentProbeType] = configuration;

            ChannelConfiguration = new(configuration, probeName, GetProbeGroupType(currentProbeType));
            ChannelConfiguration
                .SetChildFormProperties(this)
                .AddDialogToPanel(panelProbe);

            ChannelConfiguration.OnZoom += UpdateTrackBarLocation;
            ChannelConfiguration.OnFileLoad += OnFileLoadEvent;
            ChannelConfiguration.OnFileImport += (sender, e) => CheckForExistingChannelPreset();
            ChannelConfiguration.OnProbeConfigurationChanged += (sender, e) =>
            {
                ProbeConfigurationChanged();
                CheckStatus();
            };
            ChannelConfiguration.OnStateChange += (sender, e) =>
            {
                if (HasChanges)
                {
                    Text += '*';
                }
                else
                {
                    Text = Text.TrimEnd('*');
                }

                OnStateChange?.Invoke(this, EventArgs.Empty);
            };
            ChannelConfiguration.BringToFront();

            propertyGrid.SelectedObject = configuration;
            propertyGrid.PropertyValueChanged += (sender, e) => CheckStatus();
            bindingSource.DataSource = configuration;

            comboBoxProbeType.DataSource = Enum.GetValues(typeof(ProbeType));
            comboBoxProbeType.SelectedItem = currentProbeType;
            comboBoxProbeType.SelectedIndexChanged += ProbeTypeChanged;
            comboBoxProbeType.Enabled = !isBeta; // NB: Beta probes cannot be a single-shank probe

            ProbeInfo = ProbeDataFactory(ProbeGroup);

            textBoxProbeCalibrationFile.DataBindings.Add("Text",
                bindingSource,
                $"{nameof(configuration.GainCalibrationFileName)}",
                false,
                DataSourceUpdateMode.OnPropertyChanged);
            textBoxProbeCalibrationFile.TextChanged += (sender, e) =>
            {
                ProbeConfiguration.GainCalibrationFileName = ((TextBox)sender).Text;
                PropertyValueChanged();
                CheckStatus();
            };

            comboBoxReference.DataSource = ProbeInfo.GetReferenceEnumValues();
            comboBoxReference.DataBindings.Add("SelectedItem",
                bindingSource,
                nameof(configuration.Reference),
                false,
                DataSourceUpdateMode.OnPropertyChanged);
            comboBoxReference.SelectedIndexChanged += SelectedReferenceChanged;

            comboBoxChannelPresets.DataSource = ProbeInfo.GetComboBoxChannelPresets();
            CheckForExistingChannelPreset();
            comboBoxChannelPresets.SelectedIndexChanged += SelectedChannelPresetChanged;

            checkBoxInvertPolarity.DataBindings.Add("Checked",
                bindingSource,
                $"{nameof(configuration.InvertPolarity)}",
                false,
                DataSourceUpdateMode.OnPropertyChanged);
            checkBoxInvertPolarity.CheckedChanged += (sender, e) => PropertyValueChanged();

            bindingSource.ListChanged += (sender, eventArgs) => PropertyValueChanged();

            tabControlProbe.SelectedIndexChanged += (sender, eventArgs) =>
            {
                if (tabControlProbe.SelectedTab == tabPageProperties)
                    PropertyValueChanged();

                else if (tabControlProbe.SelectedTab == tabPageConfiguration)
                    bindingSource.ResetCurrentItem();
            };

            CheckStatus();
        }

        ProbeType GetCurrentProbeType(NeuropixelsV2ProbeConfiguration configuration)
        {
            if (configuration is NeuropixelsV2SingleShankProbeConfiguration)
                return ProbeType.SingleShank;
            else if (configuration is NeuropixelsV2QuadShankProbeConfiguration)
                return ProbeType.QuadShank;

            throw new InvalidEnumArgumentException($"Unknown {nameof(NeuropixelsV2ProbeConfiguration)} type: {configuration.GetType()}");
        }

        void PropertyValueChanged()
        {
            propertyGrid.Refresh();
            OnPropertyValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ProbeTypeChanged(object sender, EventArgs e)
        {
            UpdateProbeConfiguration();
        }

        static INeuropixelsV2ProbeInfo ProbeDataFactory(NeuropixelsV2eProbeGroup probeGroup)
        {
            if (probeGroup is NeuropixelsV2eQuadShankProbeGroup quadShankProbeGroup)
            {
                return new NeuropixelsV2QuadShankInfo(quadShankProbeGroup);
            }
            else if (probeGroup is NeuropixelsV2eSingleShankProbeGroup singleShankProbeGroup)
            {
                return new NeuropixelsV2SingleShankInfo(singleShankProbeGroup);
            }

            throw new NotImplementedException("Unknown configuration found.");
        }

        static Type GetProbeGroupType(ProbeType probeType)
        {
            return probeType switch
            {
                ProbeType.SingleShank => typeof(NeuropixelsV2eSingleShankProbeGroup),
                ProbeType.QuadShank => typeof(NeuropixelsV2eQuadShankProbeGroup),
                _ => throw new InvalidEnumArgumentException(nameof(ProbeType))
            };
        }

        void ProbeConfigurationChanged()
        {
            OnProbeConfigurationChange?.Invoke(this, EventArgs.Empty);
        }

        void UpdateProbeConfiguration()
        {
            SuspendLayout();

            var probeType = (ProbeType)comboBoxProbeType.SelectedItem;

            if (!ChannelConfiguration.UpdateProbeConfiguration(probeConfigurations[probeType], GetProbeGroupType(probeType)))
            {
                // NB: If the update fails, revert to the previous probe type selection to avoid inconsistencies in the GUI
                comboBoxProbeType.SelectedIndexChanged -= ProbeTypeChanged;
                comboBoxProbeType.SelectedItem = GetCurrentProbeType(ProbeConfiguration);
                comboBoxProbeType.SelectedIndexChanged += ProbeTypeChanged;
                return;
            }

            ProbeInfo = ProbeDataFactory(ProbeGroup);

            // NB: Temporarily detach handlers so the updated information is respected
            comboBoxChannelPresets.SelectedIndexChanged -= SelectedChannelPresetChanged;
            comboBoxChannelPresets.DataSource = ProbeInfo.GetComboBoxChannelPresets();
            CheckForExistingChannelPreset();
            comboBoxChannelPresets.SelectedIndexChanged += SelectedChannelPresetChanged;

            comboBoxReference.SelectedIndexChanged -= SelectedReferenceChanged;
            comboBoxReference.DataSource = ProbeInfo.GetReferenceEnumValues();
            comboBoxReference.SelectedItem = ProbeConfiguration.Reference;
            comboBoxReference.SelectedIndexChanged += SelectedReferenceChanged;

            bindingSource.DataSource = ProbeConfiguration;
            propertyGrid.SelectedObject = ProbeConfiguration;

            ProbeConfigurationChanged();
            CheckStatus();

            ResumeLayout();
            Refresh();
        }

        internal Color PanelBorderColor { get; set; } = Color.Black;

        private void FormShown(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                tableLayoutPanel1.Controls.Remove(flowLayoutPanel1);
                tableLayoutPanel1.RowCount = 1;

                ChannelConfiguration.panel.BorderStyle = BorderStyle.None;
                ChannelConfiguration.panel.Paint += (sender, e) =>
                {
                    var panel = sender as Panel;
                    var graphics = e.Graphics;
                    ControlPaint.DrawBorder(graphics, panel.ClientRectangle, PanelBorderColor, ButtonBorderStyle.Solid);
                };
            }

            splitContainer1.SplitterDistance = splitContainer1.Size.Width - splitContainer1.Panel2MinSize;

            if (ChannelConfiguration.Visible)
                ChannelConfiguration.Show();
            ChannelConfiguration.ConnectResizeEventHandler();
            ChannelConfiguration.ResizeZedGraph();
        }

        private void SelectedReferenceChanged(object sender, EventArgs e)
        {
            var comboBox = sender as ComboBox;
            ProbeConfiguration.Reference = (Enum)comboBox.SelectedItem;

            foreach (Binding binding in comboBox.DataBindings)
            {
                binding.WriteValue();
            }

            bindingSource.ResetCurrentItem();
            PropertyValueChanged();
        }

        void SelectedChannelPresetChanged(object sender, EventArgs e)
        {
            try
            {
                Enum channelPreset = ((ComboBox)sender).SelectedItem as Enum ?? throw new InvalidEnumArgumentException("Invalid argument given for the channel preset.");
                SetChannelPreset(channelPreset);
            }
            catch (InvalidEnumArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Invalid Preset Chosen");
                return;
            }

            UpdateProbeGroup();
        }

        void SetChannelPreset(Enum channelPreset)
        {
            ProbeGroup.SelectElectrodes(ProbeInfo.GetChannelPreset(channelPreset));

            ChannelConfiguration.HighlightEnabledContacts();
            ChannelConfiguration.HighlightSelectedContacts();
            ChannelConfiguration.UpdateContactLabels();
            ChannelConfiguration.RefreshZedGraph();
            HasChanges = true;
        }

        void CheckForExistingChannelPreset()
        {
            comboBoxChannelPresets.SelectedItem = ProbeInfo.CheckForExistingChannelPreset(ProbeGroup.ChannelMap);
        }

        private void OnFileLoadEvent(object sender, EventArgs e)
        {
            UpdateProbeConfiguration();
        }

        internal void CheckStatus()
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

            toolStripFileName.Text = string.IsNullOrEmpty(ProbeConfiguration.ProbeInterfaceFileName)
                                         ? "?"
                                         : Path.GetFileName(ProbeConfiguration.ProbeInterfaceFileName);

            toolStripFileName.ToolTipText = ProbeConfiguration.ProbeInterfaceFileName;
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
            var selected = ProbeGroup
                    .ToElectrodes()
                    .Where((e, ind) => ChannelConfiguration.SelectedContacts[ind])
                    .ToArray();

            if (selected.Length == 0)
                return;

            ProbeGroup.SelectElectrodes(selected);
            HasChanges = true;

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

        internal bool ProcessMenuShortcut(Keys keyData)
        {
            return ChannelConfiguration.Visible && ChannelConfiguration.ProcessMenuShortcut(keyData);
        }

        void DialogClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.Cancel || (string.IsNullOrEmpty(ProbeConfiguration.ProbeInterfaceFileName) && string.IsNullOrEmpty(ProbeConfiguration.GainCalibrationFileName)))
                return;

            ChannelConfiguration.Close();

            if (!ChannelConfiguration.IsDisposed)
            {
                e.Cancel = true;
                return;
            }
        }
    }
}
