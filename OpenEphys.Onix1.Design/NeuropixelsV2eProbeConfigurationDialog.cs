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
        const int BankDStartIndex = 896;

        readonly NeuropixelsV2eChannelConfigurationDialog ChannelConfiguration;

        internal event EventHandler InvertPolarityChanged;

        enum QuadShankChannelPreset
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

        enum SingleShankChannelPreset
        {
            BankA,
            BankB,
            BankC,
            BankD,
            None
        }

        /// <summary>
        /// Public <see cref="NeuropixelsV2ProbeConfiguration"/> object that is manipulated by
        /// <see cref="NeuropixelsV2eChannelConfigurationDialog"/>.
        /// </summary>
        public NeuropixelsV2ProbeConfiguration ProbeConfiguration
        {
            get => ChannelConfiguration.ProbeConfiguration;
        }

        readonly Dictionary<NeuropixelsV2ProbeType, NeuropixelsV2ProbeConfiguration> probeConfigurations;

        /// <inheritdoc cref="ConfigureNeuropixelsV2e.InvertPolarity"/>
        public bool InvertPolarity { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV2ProbeConfiguration"/>.
        /// </summary>
        /// <param name="configuration">A <see cref="NeuropixelsV2ProbeConfiguration"/> object holding the current configuration settings.</param>
        /// <param name="calibrationFile">String containing the path to the calibration file for this probe.</param>
        /// <param name="invertPolarity">Boolean denoting whether or not to invert the polarity of neural data.</param>
        /// <param name="isBeta">Boolean indicating if this is a beta probe or not.</param>
        public NeuropixelsV2eProbeConfigurationDialog(NeuropixelsV2ProbeConfiguration configuration, string calibrationFile, bool invertPolarity, bool isBeta)
        {
            InitializeComponent();
            Shown += FormShown;

            textBoxProbeCalibrationFile.Text = calibrationFile;

            probeConfigurations = new()
            {
                [NeuropixelsV2ProbeType.SingleShank] = new(configuration.Probe, NeuropixelsV2ProbeType.SingleShank, configuration.Reference),
                [NeuropixelsV2ProbeType.QuadShank] = new(configuration.Probe, NeuropixelsV2ProbeType.QuadShank, configuration.Reference)
            };

            probeConfigurations[configuration.ProbeType].SelectElectrodes(configuration.ChannelMap);

            ChannelConfiguration = new(probeConfigurations[configuration.ProbeType])
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

            comboBoxProbeType.DataSource = Enum.GetValues(typeof(NeuropixelsV2ProbeType));
            comboBoxProbeType.SelectedItem = ProbeConfiguration.ProbeType;

            if (isBeta)
                comboBoxProbeType.Enabled = false;
            else
                comboBoxProbeType.SelectedIndexChanged += SelectedProbeTypeChanged;

            comboBoxChannelPresets.DataSource = GetComboBoxChannelPresets(ProbeConfiguration.ProbeType);
            comboBoxChannelPresets.SelectedIndexChanged += SelectedChannelPresetChanged;

            checkBoxInvertPolarity.Checked = InvertPolarity;
            checkBoxInvertPolarity.CheckedChanged += InvertPolarityIndexChanged;

            CheckStatus();

            Text += ": " + ProbeConfiguration.Probe.ToString();

            UpdateProbeConfiguration();
        }

        static Array GetComboBoxChannelPresets(NeuropixelsV2ProbeType probeType)
        {
            return probeType switch
            {
                NeuropixelsV2ProbeType.SingleShank => Enum.GetValues(typeof(SingleShankChannelPreset)),
                NeuropixelsV2ProbeType.QuadShank => Enum.GetValues(typeof(QuadShankChannelPreset)),
                _ => throw new InvalidEnumArgumentException(nameof(NeuropixelsV2ProbeType))
            };
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
        }

        void UpdateProbeConfiguration()
        {
            var probeType = (NeuropixelsV2ProbeType)comboBoxProbeType.SelectedItem;

            ChannelConfiguration.ProbeConfiguration = probeConfigurations[probeType];
            ChannelConfiguration.ProbeGroup = ProbeConfiguration.ProbeGroup;

            ChannelConfiguration.DrawProbeGroup();
            ChannelConfiguration.ResetZoom();
            ChannelConfiguration.RefreshZedGraph();

            comboBoxChannelPresets.SelectedIndexChanged -= SelectedChannelPresetChanged; // NB: Temporarily detach handler so the loaded electrode configuration is respected
            comboBoxChannelPresets.DataSource = GetComboBoxChannelPresets(ProbeConfiguration.ProbeType);
            comboBoxChannelPresets.SelectedIndexChanged += SelectedChannelPresetChanged;

            comboBoxReference.SelectedIndexChanged -= SelectedReferenceChanged;
            comboBoxReference.DataSource = NeuropixelsV2ProbeConfiguration.FilterNeuropixelsV2ShankReference(ProbeConfiguration.ProbeType);
            comboBoxReference.SelectedItem = ProbeConfiguration.Reference;
            comboBoxReference.SelectedIndexChanged += SelectedReferenceChanged;

            CheckForExistingChannelPreset();
        }

        void SelectedProbeTypeChanged(object sender, EventArgs e)
        {
            UpdateProbeConfiguration();
        }

        private void SelectedReferenceChanged(object sender, EventArgs e)
        {
            ProbeConfiguration.Reference = (NeuropixelsV2ShankReference)((ComboBox)sender).SelectedItem;
        }

        private void SelectedChannelPresetChanged(object sender, EventArgs e)
        {
            switch (ProbeConfiguration.ProbeType)
            {
                case NeuropixelsV2ProbeType.SingleShank:
                    SetSingleShankChannelPreset((SingleShankChannelPreset)((ComboBox)sender).SelectedItem);
                    break;
                case NeuropixelsV2ProbeType.QuadShank:
                    SetQuadShankChannelPreset((QuadShankChannelPreset)((ComboBox)sender).SelectedItem);
                    break;
                default:
                    throw new NotSupportedException($"Unknown probe configuration found.");
            }

            ChannelConfiguration.HighlightEnabledContacts();
            ChannelConfiguration.HighlightSelectedContacts();
            ChannelConfiguration.UpdateContactLabels();
            ChannelConfiguration.RefreshZedGraph();
        }

        void SetSingleShankChannelPreset(SingleShankChannelPreset preset)
        {
            var electrodes = NeuropixelsV2eProbeGroup.ToElectrodes(ProbeConfiguration.ProbeGroup, NeuropixelsV2ProbeType.SingleShank);

            switch (preset)
            {
                case SingleShankChannelPreset.BankA:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2Bank.A).ToArray());
                    break;

                case SingleShankChannelPreset.BankB:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2Bank.B).ToArray());
                    break;

                case SingleShankChannelPreset.BankC:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2Bank.C).ToArray());
                    break;

                case SingleShankChannelPreset.BankD:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2Bank.D ||
                                                                             (e.Bank == NeuropixelsV2Bank.C && e.Index >= 896)).ToArray());
                    break;
            }

        }

        void SetQuadShankChannelPreset(QuadShankChannelPreset preset)
        {
            var electrodes = NeuropixelsV2eProbeGroup.ToElectrodes(ProbeConfiguration.ProbeGroup, NeuropixelsV2ProbeType.QuadShank);

            switch (preset)
            {
                case QuadShankChannelPreset.Shank0BankA:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2Bank.A &&
                                                                              e.Shank == 0).ToArray());
                    break;

                case QuadShankChannelPreset.Shank0BankB:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2Bank.B &&
                                                                              e.Shank == 0).ToArray());
                    break;

                case QuadShankChannelPreset.Shank0BankC:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2Bank.C &&
                                                                              e.Shank == 0).ToArray());
                    break;

                case QuadShankChannelPreset.Shank0BankD:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Bank == NeuropixelsV2Bank.D ||
                                                                              (e.Bank == NeuropixelsV2Bank.C && e.Index >= BankDStartIndex)) &&
                                                                               e.Shank == 0).ToArray());
                    break;

                case QuadShankChannelPreset.Shank1BankA:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2Bank.A &&
                                                                              e.Shank == 1).ToArray());
                    break;

                case QuadShankChannelPreset.Shank1BankB:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2Bank.B &&
                                                                      e.Shank == 1).ToArray());
                    break;

                case QuadShankChannelPreset.Shank1BankC:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2Bank.C &&
                                                                               e.Shank == 1).ToArray());
                    break;

                case QuadShankChannelPreset.Shank1BankD:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Bank == NeuropixelsV2Bank.D ||
                                                                              (e.Bank == NeuropixelsV2Bank.C && e.Index >= BankDStartIndex)) &&
                                                                               e.Shank == 1).ToArray());
                    break;

                case QuadShankChannelPreset.Shank2BankA:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2Bank.A &&
                                                                              e.Shank == 2).ToArray());
                    break;

                case QuadShankChannelPreset.Shank2BankB:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2Bank.B &&
                                                                              e.Shank == 2).ToArray());
                    break;

                case QuadShankChannelPreset.Shank2BankC:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2Bank.C &&
                                                                               e.Shank == 2).ToArray());
                    break;

                case QuadShankChannelPreset.Shank2BankD:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Bank == NeuropixelsV2Bank.D ||
                                                                              (e.Bank == NeuropixelsV2Bank.C && e.Index >= BankDStartIndex)) &&
                                                                               e.Shank == 2).ToArray());
                    break;

                case QuadShankChannelPreset.Shank3BankA:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2Bank.A &&
                                                                              e.Shank == 3).ToArray());
                    break;

                case QuadShankChannelPreset.Shank3BankB:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2Bank.B &&
                                                                              e.Shank == 3).ToArray());
                    break;

                case QuadShankChannelPreset.Shank3BankC:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2Bank.C &&
                                                                               e.Shank == 3).ToArray());
                    break;

                case QuadShankChannelPreset.Shank3BankD:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Bank == NeuropixelsV2Bank.D ||
                                                                              (e.Bank == NeuropixelsV2Bank.C && e.Index >= BankDStartIndex)) &&
                                                                               e.Shank == 3).ToArray());
                    break;

                case QuadShankChannelPreset.AllShanks0_95:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 0 && e.IntraShankElectrodeIndex <= 95) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 0 && e.IntraShankElectrodeIndex <= 95) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 0 && e.IntraShankElectrodeIndex <= 95) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 0 && e.IntraShankElectrodeIndex <= 95)).ToArray());
                    break;

                case QuadShankChannelPreset.AllShanks96_191:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 96 && e.IntraShankElectrodeIndex <= 191) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 96 && e.IntraShankElectrodeIndex <= 191) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 96 && e.IntraShankElectrodeIndex <= 191) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 96 && e.IntraShankElectrodeIndex <= 191)).ToArray());
                    break;

                case QuadShankChannelPreset.AllShanks192_287:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 192 && e.IntraShankElectrodeIndex <= 287) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 192 && e.IntraShankElectrodeIndex <= 287) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 192 && e.IntraShankElectrodeIndex <= 287) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 192 && e.IntraShankElectrodeIndex <= 287)).ToArray());
                    break;

                case QuadShankChannelPreset.AllShanks288_383:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 288 && e.IntraShankElectrodeIndex <= 383) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 288 && e.IntraShankElectrodeIndex <= 383) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 288 && e.IntraShankElectrodeIndex <= 383) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 288 && e.IntraShankElectrodeIndex <= 383)).ToArray());
                    break;

                case QuadShankChannelPreset.AllShanks384_479:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 384 && e.IntraShankElectrodeIndex <= 479) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 384 && e.IntraShankElectrodeIndex <= 479) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 384 && e.IntraShankElectrodeIndex <= 479) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 384 && e.IntraShankElectrodeIndex <= 479)).ToArray());
                    break;

                case QuadShankChannelPreset.AllShanks480_575:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 480 && e.IntraShankElectrodeIndex <= 575) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 480 && e.IntraShankElectrodeIndex <= 575) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 480 && e.IntraShankElectrodeIndex <= 575) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 480 && e.IntraShankElectrodeIndex <= 575)).ToArray());
                    break;

                case QuadShankChannelPreset.AllShanks576_671:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 576 && e.IntraShankElectrodeIndex <= 671) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 576 && e.IntraShankElectrodeIndex <= 671) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 576 && e.IntraShankElectrodeIndex <= 671) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 576 && e.IntraShankElectrodeIndex <= 671)).ToArray());
                    break;

                case QuadShankChannelPreset.AllShanks672_767:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 672 && e.IntraShankElectrodeIndex <= 767) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 672 && e.IntraShankElectrodeIndex <= 767) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 672 && e.IntraShankElectrodeIndex <= 767) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 672 && e.IntraShankElectrodeIndex <= 767)).ToArray());
                    break;

                case QuadShankChannelPreset.AllShanks768_863:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 768 && e.IntraShankElectrodeIndex <= 863) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 768 && e.IntraShankElectrodeIndex <= 863) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 768 && e.IntraShankElectrodeIndex <= 863) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 768 && e.IntraShankElectrodeIndex <= 863)).ToArray());
                    break;

                case QuadShankChannelPreset.AllShanks864_959:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 864 && e.IntraShankElectrodeIndex <= 959) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 864 && e.IntraShankElectrodeIndex <= 959) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 864 && e.IntraShankElectrodeIndex <= 959) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 864 && e.IntraShankElectrodeIndex <= 959)).ToArray());
                    break;

                case QuadShankChannelPreset.AllShanks960_1055:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 960 && e.IntraShankElectrodeIndex <= 1055) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 960 && e.IntraShankElectrodeIndex <= 1055) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 960 && e.IntraShankElectrodeIndex <= 1055) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 960 && e.IntraShankElectrodeIndex <= 1055)).ToArray());
                    break;

                case QuadShankChannelPreset.AllShanks1056_1151:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 1056 && e.IntraShankElectrodeIndex <= 1151) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 1056 && e.IntraShankElectrodeIndex <= 1151) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 1056 && e.IntraShankElectrodeIndex <= 1151) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 1056 && e.IntraShankElectrodeIndex <= 1151)).ToArray());
                    break;

                case QuadShankChannelPreset.AllShanks1152_1247:
                    ProbeConfiguration.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 1152 && e.IntraShankElectrodeIndex <= 1247) ||
                                                                              (e.Shank == 1 && e.IntraShankElectrodeIndex >= 1152 && e.IntraShankElectrodeIndex <= 1247) ||
                                                                              (e.Shank == 2 && e.IntraShankElectrodeIndex >= 1152 && e.IntraShankElectrodeIndex <= 1247) ||
                                                                              (e.Shank == 3 && e.IntraShankElectrodeIndex >= 1152 && e.IntraShankElectrodeIndex <= 1247)).ToArray());
                    break;
            }
        }

        void CheckForExistingChannelPreset()
        {
            switch (ProbeConfiguration.ProbeType)
            {
                case NeuropixelsV2ProbeType.SingleShank:
                    CheckSingleShankForChannelPreset(ProbeConfiguration.ChannelMap); break;
                case NeuropixelsV2ProbeType.QuadShank:
                    CheckQuadShankForChannelPreset(ProbeConfiguration.ChannelMap); break;
                default:
                    throw new NotSupportedException($"Unknown probe configuration found.");
            }
        }

        void CheckSingleShankForChannelPreset(NeuropixelsV2Electrode[] channelMap)
        {
            if (channelMap.All(e => e.Bank == NeuropixelsV2Bank.A))
            {
                comboBoxChannelPresets.SelectedItem = SingleShankChannelPreset.BankA;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2Bank.B))
            {
                comboBoxChannelPresets.SelectedItem = SingleShankChannelPreset.BankB;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2Bank.C))
            {
                comboBoxChannelPresets.SelectedItem = SingleShankChannelPreset.BankC;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2Bank.D ||
                                        (e.Bank == NeuropixelsV2Bank.C && e.Index >= BankDStartIndex)))
            {
                comboBoxChannelPresets.SelectedItem = SingleShankChannelPreset.BankD;
            }
            else
            {
                comboBoxChannelPresets.SelectedItem = SingleShankChannelPreset.None;
            }
        }

        void CheckQuadShankForChannelPreset(NeuropixelsV2Electrode[] channelMap)
        {
            if (channelMap.All(e => e.Bank == NeuropixelsV2Bank.A &&
                                    e.Shank == 0))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.Shank0BankA;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2Bank.B &&
                                         e.Shank == 0))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.Shank0BankB;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2Bank.C &&
                                         e.Shank == 0))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.Shank0BankC;
            }
            else if (channelMap.All(e => (e.Bank == NeuropixelsV2Bank.D
                                          || (e.Bank == NeuropixelsV2Bank.C
                                              && e.IntraShankElectrodeIndex >= BankDStartIndex))
                                          && e.Shank == 0))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.Shank0BankD;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2Bank.A &&
                                         e.Shank == 1))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.Shank1BankA;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2Bank.B &&
                                         e.Shank == 1))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.Shank1BankB;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2Bank.C &&
                                         e.Shank == 1))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.Shank1BankC;
            }
            else if (channelMap.All(e => (e.Bank == NeuropixelsV2Bank.D
                                          || (e.Bank == NeuropixelsV2Bank.C
                                              && e.IntraShankElectrodeIndex >= BankDStartIndex))
                                          && e.Shank == 1))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.Shank1BankD;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2Bank.A &&
                                         e.Shank == 2))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.Shank2BankA;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2Bank.B &&
                                         e.Shank == 2))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.Shank2BankB;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2Bank.C &&
                                         e.Shank == 2))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.Shank2BankC;
            }
            else if (channelMap.All(e => (e.Bank == NeuropixelsV2Bank.D
                                          || (e.Bank == NeuropixelsV2Bank.C
                                              && e.IntraShankElectrodeIndex >= BankDStartIndex))
                                          && e.Shank == 2))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.Shank2BankD;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2Bank.A &&
                                         e.Shank == 3))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.Shank3BankA;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2Bank.B &&
                                         e.Shank == 3))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.Shank3BankB;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2Bank.C &&
                                         e.Shank == 3))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.Shank3BankC;
            }
            else if (channelMap.All(e => (e.Bank == NeuropixelsV2Bank.D
                                          || (e.Bank == NeuropixelsV2Bank.C
                                              && e.IntraShankElectrodeIndex >= BankDStartIndex))
                                          && e.Shank == 3))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.Shank3BankD;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 0 && e.IntraShankElectrodeIndex <= 95) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 0 && e.IntraShankElectrodeIndex <= 95) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 0 && e.IntraShankElectrodeIndex <= 95) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 0 && e.IntraShankElectrodeIndex <= 95)))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.AllShanks0_95;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 192 && e.IntraShankElectrodeIndex <= 287) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 192 && e.IntraShankElectrodeIndex <= 287) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 192 && e.IntraShankElectrodeIndex <= 287) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 192 && e.IntraShankElectrodeIndex <= 287)))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.AllShanks192_287;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 288 && e.IntraShankElectrodeIndex <= 383) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 288 && e.IntraShankElectrodeIndex <= 383) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 288 && e.IntraShankElectrodeIndex <= 383) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 288 && e.IntraShankElectrodeIndex <= 383)))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.AllShanks288_383;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 394 && e.IntraShankElectrodeIndex <= 479) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 394 && e.IntraShankElectrodeIndex <= 479) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 394 && e.IntraShankElectrodeIndex <= 479) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 394 && e.IntraShankElectrodeIndex <= 479)))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.AllShanks384_479;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 480 && e.IntraShankElectrodeIndex <= 575) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 480 && e.IntraShankElectrodeIndex <= 575) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 480 && e.IntraShankElectrodeIndex <= 575) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 480 && e.IntraShankElectrodeIndex <= 575)))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.AllShanks480_575;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 576 && e.IntraShankElectrodeIndex <= 671) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 576 && e.IntraShankElectrodeIndex <= 671) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 576 && e.IntraShankElectrodeIndex <= 671) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 576 && e.IntraShankElectrodeIndex <= 671)))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.AllShanks576_671;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 672 && e.IntraShankElectrodeIndex <= 767) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 672 && e.IntraShankElectrodeIndex <= 767) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 672 && e.IntraShankElectrodeIndex <= 767) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 672 && e.IntraShankElectrodeIndex <= 767)))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.AllShanks672_767;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 768 && e.IntraShankElectrodeIndex <= 863) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 768 && e.IntraShankElectrodeIndex <= 863) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 768 && e.IntraShankElectrodeIndex <= 863) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 768 && e.IntraShankElectrodeIndex <= 863)))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.AllShanks768_863;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 864 && e.IntraShankElectrodeIndex <= 959) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 864 && e.IntraShankElectrodeIndex <= 959) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 864 && e.IntraShankElectrodeIndex <= 959) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 864 && e.IntraShankElectrodeIndex <= 959)))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.AllShanks864_959;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 960 && e.IntraShankElectrodeIndex <= 1055) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 960 && e.IntraShankElectrodeIndex <= 1055) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 960 && e.IntraShankElectrodeIndex <= 1055) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 960 && e.IntraShankElectrodeIndex <= 1055)))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.AllShanks960_1055;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 1056 && e.IntraShankElectrodeIndex <= 1151) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 1056 && e.IntraShankElectrodeIndex <= 1151) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 1056 && e.IntraShankElectrodeIndex <= 1151) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 1056 && e.IntraShankElectrodeIndex <= 1151)))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.AllShanks1056_1151;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.IntraShankElectrodeIndex >= 1152 && e.IntraShankElectrodeIndex <= 1247) ||
                                         (e.Shank == 1 && e.IntraShankElectrodeIndex >= 1152 && e.IntraShankElectrodeIndex <= 1247) ||
                                         (e.Shank == 2 && e.IntraShankElectrodeIndex >= 1152 && e.IntraShankElectrodeIndex <= 1247) ||
                                         (e.Shank == 3 && e.IntraShankElectrodeIndex >= 1152 && e.IntraShankElectrodeIndex <= 1247)))
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.AllShanks1152_1247;
            }
            else
            {
                comboBoxChannelPresets.SelectedItem = QuadShankChannelPreset.None;
            }
        }

        private void OnFileLoadEvent(object sender, EventArgs e)
        {
            NeuropixelsV2ProbeType probeType;

            try
            {
                probeType = NeuropixelsV2eProbeGroup.GetProbeTypeFromProbeName(ChannelConfiguration.ProbeGroup.Probes.First().Annotations.Name);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            probeConfigurations[probeType] = new((NeuropixelsV2eProbeGroup)ChannelConfiguration.ProbeGroup,
                probeConfigurations[probeType].Probe,
                probeConfigurations[probeType].ProbeType,
                probeConfigurations[probeType].Reference);

            comboBoxProbeType.SelectedItem = probeType;
            UpdateProbeConfiguration();
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
            var selected = NeuropixelsV2eProbeGroup
                    .ToElectrodes(ProbeConfiguration.ProbeGroup, ProbeConfiguration.ProbeType)
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
