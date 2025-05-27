﻿using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="NeuropixelsV2QuadShankProbeConfiguration"/>.
    /// </summary>
    public partial class NeuropixelsV2eProbeConfigurationDialog : Form
    {
        readonly NeuropixelsV2eChannelConfigurationDialog ChannelConfiguration;

        internal event EventHandler InvertPolarityChanged;

        private enum ChannelPreset
        {
            Shank0BankA,
            Shank0BankB,
            Shank0BankC,
            Shank0BankD,
            Shank1BankA,
            Shank1BankB,
            Shank1BankC,
            Shank1BankD,
            Shank2BankA,
            Shank2BankB,
            Shank2BankC,
            Shank2BankD,
            Shank3BankA,
            Shank3BankB,
            Shank3BankC,
            Shank3BankD,
            AllShanks0_95,
            AllShanks96_191,
            AllShanks192_287,
            AllShanks288_383,
            AllShanks384_479,
            AllShanks480_575,
            AllShanks576_671,
            AllShanks672_767,
            AllShanks768_863,
            AllShanks864_959,
            AllShanks960_1055,
            AllShanks1056_1151,
            AllShanks1152_1247,
            None
        }

        /// <summary>
        /// Public <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> object that is manipulated by
        /// <see cref="NeuropixelsV2eProbeConfigurationDialog"/>.
        /// </summary>
        public NeuropixelsV2QuadShankProbeConfiguration ProbeConfiguration { get; set; }

        /// <inheritdoc cref="ConfigureNeuropixelsV2e.InvertPolarity"/>
        public bool InvertPolarity { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV2eProbeConfigurationDialog"/>.
        /// </summary>
        /// <param name="configuration">A <see cref="NeuropixelsV2QuadShankProbeConfiguration"/> object holding the current configuration settings.</param>
        /// <param name="calibrationFile">String containing the path to the calibration file for this probe.</param>
        /// <param name="invertPolarity">Boolean denoting whether or not to invert the polarity of neural data.</param>
        public NeuropixelsV2eProbeConfigurationDialog(NeuropixelsV2QuadShankProbeConfiguration configuration, string calibrationFile, bool invertPolarity)
        {
            InitializeComponent();
            Shown += FormShown;

            ProbeConfiguration = new(configuration);

            textBoxProbeCalibrationFile.Text = calibrationFile;

            ChannelConfiguration = new(ProbeConfiguration)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this
            };

            InvertPolarity = invertPolarity;

            panelProbe.Controls.Add(ChannelConfiguration);
            this.AddMenuItemsFromDialogToFileOption(ChannelConfiguration);

            ChannelConfiguration.OnZoom += UpdateTrackBarLocation;
            ChannelConfiguration.OnFileLoad += OnFileLoadEvent;

            comboBoxReference.DataSource = Enum.GetValues(typeof(NeuropixelsV2QuadShankReference));
            comboBoxReference.SelectedItem = ProbeConfiguration.Reference;
            comboBoxReference.SelectedIndexChanged += SelectedReferenceChanged;

            comboBoxChannelPresets.DataSource = Enum.GetValues(typeof(ChannelPreset));
            comboBoxChannelPresets.SelectedIndexChanged += SelectedChannelPresetChanged;

            checkBoxInvertPolarity.Checked = InvertPolarity;
            checkBoxInvertPolarity.CheckedChanged += InvertPolarityIndexChanged;

            CheckForExistingChannelPreset();

            CheckStatus();

            Text += ": " + ProbeConfiguration.Probe.ToString();
        }

        private void InvertPolarityIndexChanged(object sender, EventArgs e)
        {
            InvertPolarity = ((CheckBox)sender).Checked;
            OnInvertPolarityChangedHandler();
        }

        /// <summary>
        /// Set the <see cref="checkBoxInvertPolarity"/> value to the given boolean.
        /// </summary>
        /// <param name="invertPolarity">Boolean denoting whether or not to invert the neural data polarity.</param>
        public void SetInvertPolarity(bool invertPolarity)
        {
            checkBoxInvertPolarity.Checked = invertPolarity;
        }

        private void OnInvertPolarityChangedHandler()
        {
            InvertPolarityChanged?.Invoke(this, EventArgs.Empty);
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
            if (sender is ChannelConfigurationDialog dialog)
            {
                panelTrackBar.Height = dialog.zedGraphChannels.Size.Height;
                panelTrackBar.Location = new Point(panelProbe.Size.Width - panelTrackBar.Width, ChannelConfiguration.zedGraphChannels.Location.Y);
            }
        }

        private void SelectedReferenceChanged(object sender, EventArgs e)
        {
            ProbeConfiguration.Reference = (NeuropixelsV2QuadShankReference)((ComboBox)sender).SelectedItem;
        }

        private void SelectedChannelPresetChanged(object sender, EventArgs e)
        {
            var channelPreset = (ChannelPreset)((ComboBox)sender).SelectedItem;

            if (channelPreset != ChannelPreset.None)
            {
                SetChannelPreset(channelPreset);
            }
        }

        private void SetChannelPreset(ChannelPreset preset)
        {
            var probeConfiguration = ChannelConfiguration.ProbeConfiguration;
            var electrodes = NeuropixelsV2eProbeGroup.ToElectrodes(ChannelConfiguration.ProbeConfiguration.ChannelConfiguration);

            switch (preset)
            {
                case ChannelPreset.Shank0BankA:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.A &&
                                                                              e.Shank == 0).ToList());
                    break;

                case ChannelPreset.Shank0BankB:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.B &&
                                                                              e.Shank == 0).ToList());
                    break;

                case ChannelPreset.Shank0BankC:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.C &&
                                                                              e.Shank == 0).ToList());
                    break;

                case ChannelPreset.Shank0BankD:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Bank == NeuropixelsV2QuadShankBank.D ||
                                                                              (e.Bank == NeuropixelsV2QuadShankBank.C && e.Index >= 896)) &&
                                                                               e.Shank == 0).ToList());
                    break;

                case ChannelPreset.Shank1BankA:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.A &&
                                                                              e.Shank == 1).ToList());
                    break;

                case ChannelPreset.Shank1BankB:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.B &&
                                                                      e.Shank == 1).ToList());
                    break;

                case ChannelPreset.Shank1BankC:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.C &&
                                                                              e.Shank == 1).ToList());
                    break;

                case ChannelPreset.Shank1BankD:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Bank == NeuropixelsV2QuadShankBank.D ||
                                                                              (e.Bank == NeuropixelsV2QuadShankBank.C && e.Index >= 896)) &&
                                                                               e.Shank == 1).ToList());
                    break;

                case ChannelPreset.Shank2BankA:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.A &&
                                                                              e.Shank == 2).ToList());
                    break;

                case ChannelPreset.Shank2BankB:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.B &&
                                                                              e.Shank == 2).ToList());
                    break;

                case ChannelPreset.Shank2BankC:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.C &&
                                                                              e.Shank == 2).ToList());
                    break;

                case ChannelPreset.Shank2BankD:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Bank == NeuropixelsV2QuadShankBank.D ||
                                                                              (e.Bank == NeuropixelsV2QuadShankBank.C && e.Index >= 896)) &&
                                                                               e.Shank == 2).ToList());
                    break;

                case ChannelPreset.Shank3BankA:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.A &&
                                                                              e.Shank == 3).ToList());
                    break;

                case ChannelPreset.Shank3BankB:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.B &&
                                                                              e.Shank == 3).ToList());
                    break;

                case ChannelPreset.Shank3BankC:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.C &&
                                                                              e.Shank == 3).ToList());
                    break;

                case ChannelPreset.Shank3BankD:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Bank == NeuropixelsV2QuadShankBank.D ||
                                                                              (e.Bank == NeuropixelsV2QuadShankBank.C && e.Index >= 896)) &&
                                                                               e.Shank == 3).ToList());
                    break;

                case ChannelPreset.AllShanks0_95:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 0 && e.IntraShankElectrodeIndex <= 95) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 0 && e.IntraShankElectrodeIndex <= 95) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 0 && e.IntraShankElectrodeIndex <= 95) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 0 && e.IntraShankElectrodeIndex <= 95)).ToList());
                    break;

                case ChannelPreset.AllShanks96_191:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 96 && e.IntraShankElectrodeIndex <= 191) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 96 && e.IntraShankElectrodeIndex <= 191) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 96 && e.IntraShankElectrodeIndex <= 191) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 96 && e.IntraShankElectrodeIndex <= 191)).ToList());
                    break;

                case ChannelPreset.AllShanks192_287:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 192 && e.IntraShankElectrodeIndex <= 287) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 192 && e.IntraShankElectrodeIndex <= 287) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 192 && e.IntraShankElectrodeIndex <= 287) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 192 && e.IntraShankElectrodeIndex <= 287)).ToList());
                    break;

                case ChannelPreset.AllShanks288_383:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 288 && e.IntraShankElectrodeIndex <= 383) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 288 && e.IntraShankElectrodeIndex <= 383) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 288 && e.IntraShankElectrodeIndex <= 383) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 288 && e.IntraShankElectrodeIndex <= 383)).ToList());
                    break;

                case ChannelPreset.AllShanks384_479:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 384 && e.IntraShankElectrodeIndex <= 479) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 384 && e.IntraShankElectrodeIndex <= 479) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 384 && e.IntraShankElectrodeIndex <= 479) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 384 && e.IntraShankElectrodeIndex <= 479)).ToList());
                    break;

                case ChannelPreset.AllShanks480_575:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 480 && e.IntraShankElectrodeIndex <= 575) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 480 && e.IntraShankElectrodeIndex <= 575) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 480 && e.IntraShankElectrodeIndex <= 575) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 480 && e.IntraShankElectrodeIndex <= 575)).ToList());
                    break;

                case ChannelPreset.AllShanks576_671:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 576 && e.IntraShankElectrodeIndex <= 671) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 576 && e.IntraShankElectrodeIndex <= 671) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 576 && e.IntraShankElectrodeIndex <= 671) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 576 && e.IntraShankElectrodeIndex <= 671)).ToList());
                    break;

                case ChannelPreset.AllShanks672_767:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 672 && e.IntraShankElectrodeIndex <= 767) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 672 && e.IntraShankElectrodeIndex <= 767) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 672 && e.IntraShankElectrodeIndex <= 767) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 672 && e.IntraShankElectrodeIndex <= 767)).ToList());
                    break;

                case ChannelPreset.AllShanks768_863:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 768 && e.IntraShankElectrodeIndex <= 863) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 768 && e.IntraShankElectrodeIndex <= 863) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 768 && e.IntraShankElectrodeIndex <= 863) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 768 && e.IntraShankElectrodeIndex <= 863)).ToList());
                    break;

                case ChannelPreset.AllShanks864_959:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 864 && e.IntraShankElectrodeIndex <= 959) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 864 && e.IntraShankElectrodeIndex <= 959) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 864 && e.IntraShankElectrodeIndex <= 959) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 864 && e.IntraShankElectrodeIndex <= 959)).ToList());
                    break;

                case ChannelPreset.AllShanks960_1055:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 960 && e.IntraShankElectrodeIndex <= 1055) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 960 && e.IntraShankElectrodeIndex <= 1055) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 960 && e.IntraShankElectrodeIndex <= 1055) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 960 && e.IntraShankElectrodeIndex <= 1055)).ToList());
                    break;

                case ChannelPreset.AllShanks1056_1151:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 1056 && e.IntraShankElectrodeIndex <= 1151) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 1056 && e.IntraShankElectrodeIndex <= 1151) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 1056 && e.IntraShankElectrodeIndex <= 1151) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 1056 && e.IntraShankElectrodeIndex <= 1151)).ToList());
                    break;

                case ChannelPreset.AllShanks1152_1247:
                    probeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 1152 && e.IntraShankElectrodeIndex <= 1247) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 1152 && e.IntraShankElectrodeIndex <= 1247) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 1152 && e.IntraShankElectrodeIndex <= 1247) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 1152 && e.IntraShankElectrodeIndex <= 1247)).ToList());
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

            if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.A &&
                                    e.Shank == 0))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.Shank0BankA;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.B &&
                                         e.Shank == 0))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.Shank0BankB;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.C &&
                                         e.Shank == 0))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.Shank0BankC;
            }
            else if (channelMap.All(e => (e.Bank == NeuropixelsV2QuadShankBank.D ||
                                         (e.Bank == NeuropixelsV2QuadShankBank.C && e.Index >= 896)) &&
                                         e.Shank == 0))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.Shank0BankD;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.A &&
                                         e.Shank == 1))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.Shank1BankA;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.B &&
                                         e.Shank == 1))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.Shank1BankB;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.C &&
                                         e.Shank == 1))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.Shank1BankC;
            }
            else if (channelMap.All(e => (e.Bank == NeuropixelsV2QuadShankBank.D ||
                                         (e.Bank == NeuropixelsV2QuadShankBank.C && e.Index >= 896)) &&
                                         e.Shank == 1))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.Shank1BankD;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.A &&
                                         e.Shank == 2))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.Shank2BankA;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.B &&
                                         e.Shank == 2))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.Shank2BankB;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.C &&
                                         e.Shank == 2))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.Shank2BankC;
            }
            else if (channelMap.All(e => (e.Bank == NeuropixelsV2QuadShankBank.D ||
                                         (e.Bank == NeuropixelsV2QuadShankBank.C && e.Index >= 896)) &&
                                         e.Shank == 2))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.Shank2BankD;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.A &&
                                         e.Shank == 3))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.Shank3BankA;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.B &&
                                         e.Shank == 3))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.Shank3BankB;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.C &&
                                         e.Shank == 3))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.Shank3BankC;
            }
            else if (channelMap.All(e => (e.Bank == NeuropixelsV2QuadShankBank.D ||
                                         (e.Bank == NeuropixelsV2QuadShankBank.C && e.Index >= 896)) &&
                                         e.Shank == 3))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.Shank3BankD;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 0 && e.IntraShankElectrodeIndex <= 95) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 0 && e.IntraShankElectrodeIndex <= 95) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 0 && e.IntraShankElectrodeIndex <= 95) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 0 && e.IntraShankElectrodeIndex <= 95)))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.AllShanks0_95;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 192 && e.IntraShankElectrodeIndex <= 287) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 192 && e.IntraShankElectrodeIndex <= 287) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 192 && e.IntraShankElectrodeIndex <= 287) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 192 && e.IntraShankElectrodeIndex <= 287)))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.AllShanks192_287;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 288 && e.IntraShankElectrodeIndex <= 383) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 288 && e.IntraShankElectrodeIndex <= 383) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 288 && e.IntraShankElectrodeIndex <= 383) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 288 && e.IntraShankElectrodeIndex <= 383)))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.AllShanks288_383;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 394 && e.IntraShankElectrodeIndex <= 479) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 394 && e.IntraShankElectrodeIndex <= 479) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 394 && e.IntraShankElectrodeIndex <= 479) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 394 && e.IntraShankElectrodeIndex <= 479)))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.AllShanks384_479;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 480 && e.IntraShankElectrodeIndex <= 575) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 480 && e.IntraShankElectrodeIndex <= 575) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 480 && e.IntraShankElectrodeIndex <= 575) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 480 && e.IntraShankElectrodeIndex <= 575)))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.AllShanks480_575;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 576 && e.IntraShankElectrodeIndex <= 671) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 576 && e.IntraShankElectrodeIndex <= 671) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 576 && e.IntraShankElectrodeIndex <= 671) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 576 && e.IntraShankElectrodeIndex <= 671)))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.AllShanks576_671;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 672 && e.IntraShankElectrodeIndex <= 767) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 672 && e.IntraShankElectrodeIndex <= 767) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 672 && e.IntraShankElectrodeIndex <= 767) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 672 && e.IntraShankElectrodeIndex <= 767)))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.AllShanks672_767;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 768 && e.IntraShankElectrodeIndex <= 863) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 768 && e.IntraShankElectrodeIndex <= 863) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 768 && e.IntraShankElectrodeIndex <= 863) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 768 && e.IntraShankElectrodeIndex <= 863)))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.AllShanks768_863;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 864 && e.IntraShankElectrodeIndex <= 959) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 864 && e.IntraShankElectrodeIndex <= 959) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 864 && e.IntraShankElectrodeIndex <= 959) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 864 && e.IntraShankElectrodeIndex <= 959)))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.AllShanks864_959;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 960 && e.IntraShankElectrodeIndex <= 1055) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 960 && e.IntraShankElectrodeIndex <= 1055) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 960 && e.IntraShankElectrodeIndex <= 1055) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 960 && e.IntraShankElectrodeIndex <= 1055)))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.AllShanks960_1055;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 1056 && e.IntraShankElectrodeIndex <= 1151) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 1056 && e.IntraShankElectrodeIndex <= 1151) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 1056 && e.IntraShankElectrodeIndex <= 1151) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 1056 && e.IntraShankElectrodeIndex <= 1151)))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.AllShanks1056_1151;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 1152 && e.IntraShankElectrodeIndex <= 1247) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 1152 && e.IntraShankElectrodeIndex <= 1247) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 1152 && e.IntraShankElectrodeIndex <= 1247) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 1152 && e.IntraShankElectrodeIndex <= 1247)))
            {
                comboBoxChannelPresets.SelectedItem = ChannelPreset.AllShanks1152_1247;
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

        private void FileTextChanged(object sender, EventArgs e)
        {
            CheckStatus();
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

        internal void ResetZoom_Click(object sender, EventArgs e)
        {
            ResetZoom();
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
            var electrodes = NeuropixelsV2eProbeGroup.ToElectrodes(ChannelConfiguration.ProbeConfiguration.ChannelConfiguration);

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
            ChannelConfiguration.DrawScale();
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
