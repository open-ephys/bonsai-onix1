using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace OpenEphys.Onix.Design
{
    public partial class NeuropixelsV2eDialog : Form
    {
        readonly NeuropixelsV2eChannelConfigurationDialog ChannelConfigurationA;
        readonly NeuropixelsV2eChannelConfigurationDialog ChannelConfigurationB;

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

        public ConfigureNeuropixelsV2e ConfigureNode { get; set; }

        public NeuropixelsV2eDialog(ConfigureNeuropixelsV2e configureNode)
        {
            InitializeComponent();
            Shown += FormShown;

            ConfigureNode = new(configureNode);

            textBoxProbeCalibrationFileA.Text = ConfigureNode.GainCalibrationFileA;
            textBoxProbeCalibrationFileB.Text = ConfigureNode.GainCalibrationFileB;

            ChannelConfigurationA = new(ConfigureNode.ProbeConfigurationA.ChannelConfiguration)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this,
                Tag = NeuropixelsV2Probe.ProbeA
            };

            panelProbeA.Controls.Add(ChannelConfigurationA);
            this.AddMenuItemsFromDialogToFileOption(ChannelConfigurationA, "Probe A");

            if (!File.Exists(textBoxProbeCalibrationFileA.Text))
            {
                panelProbeA.Visible = false;
            }

            ChannelConfigurationB = new(ConfigureNode.ProbeConfigurationB.ChannelConfiguration)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                Parent = this,
                Tag = NeuropixelsV2Probe.ProbeB
            };

            panelProbeB.Controls.Add(ChannelConfigurationB);
            this.AddMenuItemsFromDialogToFileOption(ChannelConfigurationB, "Probe B");

            if (!File.Exists(textBoxProbeCalibrationFileB.Text))
            {
                panelProbeB.Visible = false;
            }

            ChannelConfigurationA.OnZoom += UpdateTrackBarLocation;
            ChannelConfigurationA.OnFileLoad += UpdateChannelPresetIndex;

            comboBoxReferenceA.DataSource = Enum.GetValues(typeof(NeuropixelsV2QuadShankReference));
            comboBoxReferenceA.SelectedItem = ConfigureNode.ProbeConfigurationA.Reference;
            comboBoxReferenceA.SelectedIndexChanged += SelectedIndexChanged;

            ChannelConfigurationB.OnZoom += UpdateTrackBarLocation;
            ChannelConfigurationB.OnFileLoad += UpdateChannelPresetIndex;

            comboBoxReferenceB.DataSource = Enum.GetValues(typeof(NeuropixelsV2QuadShankReference));
            comboBoxReferenceB.SelectedItem = ConfigureNode.ProbeConfigurationB.Reference;
            comboBoxReferenceB.SelectedIndexChanged += SelectedIndexChanged;

            comboBoxChannelPresetsA.DataSource = Enum.GetValues(typeof(ChannelPreset));
            comboBoxChannelPresetsA.SelectedIndexChanged += SelectedIndexChanged;
            CheckForExistingChannelPreset(NeuropixelsV2Probe.ProbeA);

            comboBoxChannelPresetsB.DataSource = Enum.GetValues(typeof(ChannelPreset));
            comboBoxChannelPresetsB.SelectedIndexChanged += SelectedIndexChanged;
            CheckForExistingChannelPreset(NeuropixelsV2Probe.ProbeB);
        }

        private void FormShown(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                splitContainer1.Panel2Collapsed = true;
                splitContainer1.Panel2.Hide();

                menuStrip.Visible = false;
            }

            ChannelConfigurationA.Show();
            ChannelConfigurationB.Show();

            ChannelConfigurationA.ConnectResizeEventHandler();
            ChannelConfigurationB.ConnectResizeEventHandler();
        }

        private void FileTextChanged(object sender, EventArgs e)
        {
            if (sender is TextBox textBox && textBox != null)
            {
                if (textBox.Name == nameof(textBoxProbeCalibrationFileA))
                {
                    ConfigureNode.GainCalibrationFileA = textBox.Text;
                    ParseGainCalibrationFile("A");
                }
                else if (textBox.Name == nameof(textBoxProbeCalibrationFileB))
                {
                    ConfigureNode.GainCalibrationFileB = textBox.Text;
                    ParseGainCalibrationFile("B");
                }
            }
        }

        private void ParseGainCalibrationFile(string probe)
        {
            if (probe == "A")
            {
                ParseGainCalibrationFile(ConfigureNode.GainCalibrationFileA, probeSnA, gainA);
            }
            else if (probe == "B")
            {
                ParseGainCalibrationFile(ConfigureNode.GainCalibrationFileB, probeSnB, gainB);
            }
        }

        private void ParseGainCalibrationFile(string filename, ToolStripStatusLabel probeSN, ToolStripStatusLabel gainLabel)
        {
            if (filename != null && filename != "")
            {
                if (File.Exists(filename))
                {
                    using StreamReader gainCalibrationFile = new(filename);

                    probeSN.Text = ulong.Parse(gainCalibrationFile.ReadLine()).ToString();
                    gainLabel.Text = double.Parse(gainCalibrationFile.ReadLine()).ToString();
                }
                else
                {
                    probeSN.Text = "";
                    gainLabel.Text = "";
                }
            }

            else
            {
                probeSN.Text = "";
                gainLabel.Text = "";
            }
        }

        private void SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = sender as ComboBox;

            if (comboBox.Name == nameof(comboBoxReferenceA))
            {
                ConfigureNode.ProbeConfigurationA.Reference = (NeuropixelsV2QuadShankReference)comboBox.SelectedItem;
            }
            else if (comboBox.Name == nameof(comboBoxReferenceB))
            {
                ConfigureNode.ProbeConfigurationB.Reference = (NeuropixelsV2QuadShankReference)comboBox.SelectedItem;
            }
            else if (comboBox.Name == nameof(comboBoxChannelPresetsA))
            {
                if ((ChannelPreset)comboBox.SelectedItem != ChannelPreset.None)
                {
                    SetChannelPreset((ChannelPreset)comboBox.SelectedItem, NeuropixelsV2Probe.ProbeA);
                }
            }
            else if (comboBox.Name == nameof(comboBoxChannelPresetsB))
            {
                if ((ChannelPreset)comboBox.SelectedItem != ChannelPreset.None)
                {
                    SetChannelPreset((ChannelPreset)comboBox.SelectedItem, NeuropixelsV2Probe.ProbeB);
                }
            }
        }

        private void SetChannelPreset(ChannelPreset preset, NeuropixelsV2Probe probeSelected)
        {
            var channelConfiguration = probeSelected == NeuropixelsV2Probe.ProbeA ? ChannelConfigurationA : ChannelConfigurationB;

            var channelMap = channelConfiguration.ChannelMap;
            var electrodes = channelConfiguration.Electrodes;

            switch (preset)
            {
                case ChannelPreset.Shank0BankA:
                    channelMap.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.A &&
                                                                      e.Shank == 0).ToList());
                    break;

                case ChannelPreset.Shank0BankB:
                    channelMap.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.B &&
                                                                      e.Shank == 0).ToList());
                    break;

                case ChannelPreset.Shank0BankC:
                    channelMap.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.C &&
                                                                      e.Shank == 0).ToList());
                    break;

                case ChannelPreset.Shank0BankD:
                    channelMap.SelectElectrodes(electrodes.Where(e => (e.Bank == NeuropixelsV2QuadShankBank.D ||
                                                                      (e.Bank == NeuropixelsV2QuadShankBank.C && e.ElectrodeNumber >= 896)) &&
                                                                      e.Shank == 0).ToList());
                    break;

                case ChannelPreset.Shank1BankA:
                    channelMap.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.A &&
                                                                      e.Shank == 1).ToList());
                    break;

                case ChannelPreset.Shank1BankB:
                    channelMap.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.B &&
                                                                      e.Shank == 1).ToList());
                    break;

                case ChannelPreset.Shank1BankC:
                    channelMap.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.C &&
                                                                      e.Shank == 1).ToList());
                    break;

                case ChannelPreset.Shank1BankD:
                    channelMap.SelectElectrodes(electrodes.Where(e => (e.Bank == NeuropixelsV2QuadShankBank.D ||
                                                                      (e.Bank == NeuropixelsV2QuadShankBank.C && e.ElectrodeNumber >= 896)) &&
                                                                      e.Shank == 1).ToList());
                    break;

                case ChannelPreset.Shank2BankA:
                    channelMap.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.A &&
                                                                      e.Shank == 2).ToList());
                    break;

                case ChannelPreset.Shank2BankB:
                    channelMap.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.B &&
                                                                      e.Shank == 2).ToList());
                    break;

                case ChannelPreset.Shank2BankC:
                    channelMap.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.C &&
                                                                      e.Shank == 2).ToList());
                    break;

                case ChannelPreset.Shank2BankD:
                    channelMap.SelectElectrodes(electrodes.Where(e => (e.Bank == NeuropixelsV2QuadShankBank.D ||
                                                                      (e.Bank == NeuropixelsV2QuadShankBank.C && e.ElectrodeNumber >= 896)) &&
                                                                      e.Shank == 2).ToList());
                    break;

                case ChannelPreset.Shank3BankA:
                    channelMap.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.A &&
                                                                      e.Shank == 3).ToList());
                    break;

                case ChannelPreset.Shank3BankB:
                    channelMap.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.B &&
                                                                      e.Shank == 3).ToList());
                    break;

                case ChannelPreset.Shank3BankC:
                    channelMap.SelectElectrodes(electrodes.Where(e => e.Bank == NeuropixelsV2QuadShankBank.C &&
                                                                      e.Shank == 3).ToList());
                    break;

                case ChannelPreset.Shank3BankD:
                    channelMap.SelectElectrodes(electrodes.Where(e => (e.Bank == NeuropixelsV2QuadShankBank.D ||
                                                                      (e.Bank == NeuropixelsV2QuadShankBank.C && e.ElectrodeNumber >= 896)) &&
                                                                      e.Shank == 3).ToList());
                    break;

                case ChannelPreset.AllShanks0_95:
                    channelMap.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.ShankIndex >= 0 && e.ShankIndex <= 95) ||
                                                                      (e.Shank == 1 && e.ShankIndex >= 0 && e.ShankIndex <= 95) ||
                                                                      (e.Shank == 2 && e.ShankIndex >= 0 && e.ShankIndex <= 95) ||
                                                                      (e.Shank == 3 && e.ShankIndex >= 0 && e.ShankIndex <= 95)).ToList());
                    break;

                case ChannelPreset.AllShanks96_191:
                    channelMap.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.ShankIndex >= 96 && e.ShankIndex <= 191) ||
                                                                      (e.Shank == 1 && e.ShankIndex >= 96 && e.ShankIndex <= 191) ||
                                                                      (e.Shank == 2 && e.ShankIndex >= 96 && e.ShankIndex <= 191) ||
                                                                      (e.Shank == 3 && e.ShankIndex >= 96 && e.ShankIndex <= 191)).ToList());
                    break;

                case ChannelPreset.AllShanks192_287:
                    channelMap.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.ShankIndex >= 192 && e.ShankIndex <= 287) ||
                                                                      (e.Shank == 1 && e.ShankIndex >= 192 && e.ShankIndex <= 287) ||
                                                                      (e.Shank == 2 && e.ShankIndex >= 192 && e.ShankIndex <= 287) ||
                                                                      (e.Shank == 3 && e.ShankIndex >= 192 && e.ShankIndex <= 287)).ToList());
                    break;

                case ChannelPreset.AllShanks288_383:
                    channelMap.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.ShankIndex >= 288 && e.ShankIndex <= 383) ||
                                                                      (e.Shank == 1 && e.ShankIndex >= 288 && e.ShankIndex <= 383) ||
                                                                      (e.Shank == 2 && e.ShankIndex >= 288 && e.ShankIndex <= 383) ||
                                                                      (e.Shank == 3 && e.ShankIndex >= 288 && e.ShankIndex <= 383)).ToList());
                    break;

                case ChannelPreset.AllShanks384_479:
                    channelMap.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.ShankIndex >= 384 && e.ShankIndex <= 479) ||
                                                                      (e.Shank == 1 && e.ShankIndex >= 384 && e.ShankIndex <= 479) ||
                                                                      (e.Shank == 2 && e.ShankIndex >= 384 && e.ShankIndex <= 479) ||
                                                                      (e.Shank == 3 && e.ShankIndex >= 384 && e.ShankIndex <= 479)).ToList());
                    break;

                case ChannelPreset.AllShanks480_575:
                    channelMap.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.ShankIndex >= 480 && e.ShankIndex <= 575) ||
                                                                      (e.Shank == 1 && e.ShankIndex >= 480 && e.ShankIndex <= 575) ||
                                                                      (e.Shank == 2 && e.ShankIndex >= 480 && e.ShankIndex <= 575) ||
                                                                      (e.Shank == 3 && e.ShankIndex >= 480 && e.ShankIndex <= 575)).ToList());
                    break;

                case ChannelPreset.AllShanks576_671:
                    channelMap.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.ShankIndex >= 576 && e.ShankIndex <= 671) ||
                                                                      (e.Shank == 1 && e.ShankIndex >= 576 && e.ShankIndex <= 671) ||
                                                                      (e.Shank == 2 && e.ShankIndex >= 576 && e.ShankIndex <= 671) ||
                                                                      (e.Shank == 3 && e.ShankIndex >= 576 && e.ShankIndex <= 671)).ToList());
                    break;

                case ChannelPreset.AllShanks672_767:
                    channelMap.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.ShankIndex >= 672 && e.ShankIndex <= 767) ||
                                                                      (e.Shank == 1 && e.ShankIndex >= 672 && e.ShankIndex <= 767) ||
                                                                      (e.Shank == 2 && e.ShankIndex >= 672 && e.ShankIndex <= 767) ||
                                                                      (e.Shank == 3 && e.ShankIndex >= 672 && e.ShankIndex <= 767)).ToList());
                    break;

                case ChannelPreset.AllShanks768_863:
                    channelMap.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.ShankIndex >= 768 && e.ShankIndex <= 863) ||
                                                                      (e.Shank == 1 && e.ShankIndex >= 768 && e.ShankIndex <= 863) ||
                                                                      (e.Shank == 2 && e.ShankIndex >= 768 && e.ShankIndex <= 863) ||
                                                                      (e.Shank == 3 && e.ShankIndex >= 768 && e.ShankIndex <= 863)).ToList());
                    break;

                case ChannelPreset.AllShanks864_959:
                    channelMap.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.ShankIndex >= 864 && e.ShankIndex <= 959) ||
                                                                      (e.Shank == 1 && e.ShankIndex >= 864 && e.ShankIndex <= 959) ||
                                                                      (e.Shank == 2 && e.ShankIndex >= 864 && e.ShankIndex <= 959) ||
                                                                      (e.Shank == 3 && e.ShankIndex >= 864 && e.ShankIndex <= 959)).ToList());
                    break;

                case ChannelPreset.AllShanks960_1055:
                    channelMap.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.ShankIndex >= 960 && e.ShankIndex <= 1055) ||
                                                                      (e.Shank == 1 && e.ShankIndex >= 960 && e.ShankIndex <= 1055) ||
                                                                      (e.Shank == 2 && e.ShankIndex >= 960 && e.ShankIndex <= 1055) ||
                                                                      (e.Shank == 3 && e.ShankIndex >= 960 && e.ShankIndex <= 1055)).ToList());
                    break;

                case ChannelPreset.AllShanks1056_1151:
                    channelMap.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.ShankIndex >= 1056 && e.ShankIndex <= 1151) ||
                                                                      (e.Shank == 1 && e.ShankIndex >= 1056 && e.ShankIndex <= 1151) ||
                                                                      (e.Shank == 2 && e.ShankIndex >= 1056 && e.ShankIndex <= 1151) ||
                                                                      (e.Shank == 3 && e.ShankIndex >= 1056 && e.ShankIndex <= 1151)).ToList());
                    break;

                case ChannelPreset.AllShanks1152_1247:
                    channelMap.SelectElectrodes(electrodes.Where(e => (e.Shank == 0 && e.ShankIndex >= 1152 && e.ShankIndex <= 1247) ||
                                                                      (e.Shank == 1 && e.ShankIndex >= 1152 && e.ShankIndex <= 1247) ||
                                                                      (e.Shank == 2 && e.ShankIndex >= 1152 && e.ShankIndex <= 1247) ||
                                                                      (e.Shank == 3 && e.ShankIndex >= 1152 && e.ShankIndex <= 1247)).ToList());
                    break;
            }

            channelConfiguration.HighlightEnabledContacts();
            channelConfiguration.HighlightSelectedContacts();
            channelConfiguration.UpdateContactLabels();
            channelConfiguration.RefreshZedGraph();
        }

        private void CheckForExistingChannelPreset(NeuropixelsV2Probe probeSelected)
        {
            var channelConfiguration = probeSelected == NeuropixelsV2Probe.ProbeA ? ChannelConfigurationA : ChannelConfigurationB;
            var comboBox = probeSelected == NeuropixelsV2Probe.ProbeA ? comboBoxChannelPresetsA : comboBoxChannelPresetsB;

            var channelMap = channelConfiguration.ChannelMap;

            if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.A &&
                                    e.Shank == 0))
            {
                comboBox.SelectedItem = ChannelPreset.Shank0BankA;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.B &&
                                         e.Shank == 0))
            {
                comboBox.SelectedItem = ChannelPreset.Shank0BankB;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.C &&
                                         e.Shank == 0))
            {
                comboBox.SelectedItem = ChannelPreset.Shank0BankC;
            }
            else if (channelMap.All(e => (e.Bank == NeuropixelsV2QuadShankBank.D ||
                                         (e.Bank == NeuropixelsV2QuadShankBank.C && e.ElectrodeNumber >= 896)) &&
                                         e.Shank == 0))
            {
                comboBox.SelectedItem = ChannelPreset.Shank0BankD;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.A &&
                                         e.Shank == 1))
            {
                comboBox.SelectedItem = ChannelPreset.Shank1BankA;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.B &&
                                         e.Shank == 1))
            {
                comboBox.SelectedItem = ChannelPreset.Shank1BankB;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.C &&
                                         e.Shank == 1))
            {
                comboBox.SelectedItem = ChannelPreset.Shank1BankC;
            }
            else if (channelMap.All(e => (e.Bank == NeuropixelsV2QuadShankBank.D ||
                                         (e.Bank == NeuropixelsV2QuadShankBank.C && e.ElectrodeNumber >= 896)) &&
                                         e.Shank == 1))
            {
                comboBox.SelectedItem = ChannelPreset.Shank1BankD;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.A &&
                                         e.Shank == 2))
            {
                comboBox.SelectedItem = ChannelPreset.Shank2BankA;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.B &&
                                         e.Shank == 2))
            {
                comboBox.SelectedItem = ChannelPreset.Shank2BankB;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.C &&
                                         e.Shank == 2))
            {
                comboBox.SelectedItem = ChannelPreset.Shank2BankC;
            }
            else if (channelMap.All(e => (e.Bank == NeuropixelsV2QuadShankBank.D ||
                                         (e.Bank == NeuropixelsV2QuadShankBank.C && e.ElectrodeNumber >= 896)) &&
                                         e.Shank == 2))
            {
                comboBox.SelectedItem = ChannelPreset.Shank2BankD;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.A &&
                                         e.Shank == 3))
            {
                comboBox.SelectedItem = ChannelPreset.Shank3BankA;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.B &&
                                         e.Shank == 3))
            {
                comboBox.SelectedItem = ChannelPreset.Shank3BankB;
            }
            else if (channelMap.All(e => e.Bank == NeuropixelsV2QuadShankBank.C &&
                                         e.Shank == 3))
            {
                comboBox.SelectedItem = ChannelPreset.Shank3BankC;
            }
            else if (channelMap.All(e => (e.Bank == NeuropixelsV2QuadShankBank.D ||
                                         (e.Bank == NeuropixelsV2QuadShankBank.C && e.ElectrodeNumber >= 896)) &&
                                         e.Shank == 3))
            {
                comboBox.SelectedItem = ChannelPreset.Shank3BankD;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.ShankIndex >= 0 && e.ShankIndex <= 95) ||
                                         (e.Shank == 1 && e.ShankIndex >= 0 && e.ShankIndex <= 95) ||
                                         (e.Shank == 2 && e.ShankIndex >= 0 && e.ShankIndex <= 95) ||
                                         (e.Shank == 3 && e.ShankIndex >= 0 && e.ShankIndex <= 95)))
            {
                comboBox.SelectedItem = ChannelPreset.AllShanks0_95;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.ShankIndex >= 192 && e.ShankIndex <= 287) ||
                                         (e.Shank == 1 && e.ShankIndex >= 192 && e.ShankIndex <= 287) ||
                                         (e.Shank == 2 && e.ShankIndex >= 192 && e.ShankIndex <= 287) ||
                                         (e.Shank == 3 && e.ShankIndex >= 192 && e.ShankIndex <= 287)))
            {
                comboBox.SelectedItem = ChannelPreset.AllShanks192_287;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.ShankIndex >= 288 && e.ShankIndex <= 383) ||
                                         (e.Shank == 1 && e.ShankIndex >= 288 && e.ShankIndex <= 383) ||
                                         (e.Shank == 2 && e.ShankIndex >= 288 && e.ShankIndex <= 383) ||
                                         (e.Shank == 3 && e.ShankIndex >= 288 && e.ShankIndex <= 383)))
            {
                comboBox.SelectedItem = ChannelPreset.AllShanks288_383;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.ShankIndex >= 394 && e.ShankIndex <= 479) ||
                                         (e.Shank == 1 && e.ShankIndex >= 394 && e.ShankIndex <= 479) ||
                                         (e.Shank == 2 && e.ShankIndex >= 394 && e.ShankIndex <= 479) ||
                                         (e.Shank == 3 && e.ShankIndex >= 394 && e.ShankIndex <= 479)))
            {
                comboBox.SelectedItem = ChannelPreset.AllShanks384_479;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.ShankIndex >= 480 && e.ShankIndex <= 575) ||
                                         (e.Shank == 1 && e.ShankIndex >= 480 && e.ShankIndex <= 575) ||
                                         (e.Shank == 2 && e.ShankIndex >= 480 && e.ShankIndex <= 575) ||
                                         (e.Shank == 3 && e.ShankIndex >= 480 && e.ShankIndex <= 575)))
            {
                comboBox.SelectedItem = ChannelPreset.AllShanks480_575;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.ShankIndex >= 576 && e.ShankIndex <= 671) ||
                                         (e.Shank == 1 && e.ShankIndex >= 576 && e.ShankIndex <= 671) ||
                                         (e.Shank == 2 && e.ShankIndex >= 576 && e.ShankIndex <= 671) ||
                                         (e.Shank == 3 && e.ShankIndex >= 576 && e.ShankIndex <= 671)))
            {
                comboBox.SelectedItem = ChannelPreset.AllShanks576_671;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.ShankIndex >= 672 && e.ShankIndex <= 767) ||
                                         (e.Shank == 1 && e.ShankIndex >= 672 && e.ShankIndex <= 767) ||
                                         (e.Shank == 2 && e.ShankIndex >= 672 && e.ShankIndex <= 767) ||
                                         (e.Shank == 3 && e.ShankIndex >= 672 && e.ShankIndex <= 767)))
            {
                comboBox.SelectedItem = ChannelPreset.AllShanks672_767;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.ShankIndex >= 768 && e.ShankIndex <= 863) ||
                                         (e.Shank == 1 && e.ShankIndex >= 768 && e.ShankIndex <= 863) ||
                                         (e.Shank == 2 && e.ShankIndex >= 768 && e.ShankIndex <= 863) ||
                                         (e.Shank == 3 && e.ShankIndex >= 768 && e.ShankIndex <= 863)))
            {
                comboBox.SelectedItem = ChannelPreset.AllShanks768_863;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.ShankIndex >= 864 && e.ShankIndex <= 959) ||
                                         (e.Shank == 1 && e.ShankIndex >= 864 && e.ShankIndex <= 959) ||
                                         (e.Shank == 2 && e.ShankIndex >= 864 && e.ShankIndex <= 959) ||
                                         (e.Shank == 3 && e.ShankIndex >= 864 && e.ShankIndex <= 959)))
            {
                comboBox.SelectedItem = ChannelPreset.AllShanks864_959;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.ShankIndex >= 960 && e.ShankIndex <= 1055) ||
                                         (e.Shank == 1 && e.ShankIndex >= 960 && e.ShankIndex <= 1055) ||
                                         (e.Shank == 2 && e.ShankIndex >= 960 && e.ShankIndex <= 1055) ||
                                         (e.Shank == 3 && e.ShankIndex >= 960 && e.ShankIndex <= 1055)))
            {
                comboBox.SelectedItem = ChannelPreset.AllShanks960_1055;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.ShankIndex >= 1056 && e.ShankIndex <= 1151) ||
                                         (e.Shank == 1 && e.ShankIndex >= 1056 && e.ShankIndex <= 1151) ||
                                         (e.Shank == 2 && e.ShankIndex >= 1056 && e.ShankIndex <= 1151) ||
                                         (e.Shank == 3 && e.ShankIndex >= 1056 && e.ShankIndex <= 1151)))
            {
                comboBox.SelectedItem = ChannelPreset.AllShanks1056_1151;
            }
            else if (channelMap.All(e => (e.Shank == 0 && e.ShankIndex >= 1152 && e.ShankIndex <= 1247) ||
                                         (e.Shank == 1 && e.ShankIndex >= 1152 && e.ShankIndex <= 1247) ||
                                         (e.Shank == 2 && e.ShankIndex >= 1152 && e.ShankIndex <= 1247) ||
                                         (e.Shank == 3 && e.ShankIndex >= 1152 && e.ShankIndex <= 1247)))
            {
                comboBox.SelectedItem = ChannelPreset.AllShanks1152_1247;
            }
            else
            {
                comboBox.SelectedItem = ChannelPreset.None;
            }
        }

        private void UpdateChannelPresetIndex(object sender, EventArgs e)
        {
            if (sender is ChannelConfigurationDialog dialog)
            {
                if (dialog.Tag is NeuropixelsV2Probe probe)
                {
                    CheckForExistingChannelPreset(probe);
                }
            }
        }

        private void LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://open-ephys.github.io/onix-docs/Software%20Guide/Bonsai.ONIX/Nodes/NeuropixelsV2eDevice.html");
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to open documentation link.");
            }
        }

        internal void ButtonClick(object sender, EventArgs e)
        {
            const float zoomFactor = 8f;

            if (sender is Button button && button != null)
            {
                if (button.Name == nameof(buttonOkay))
                {
                    UpdateProbeGroups();

                    DialogResult = DialogResult.OK;
                }
                else if (button.Name == nameof(buttonCancel))
                {
                    DialogResult = DialogResult.Cancel;
                }
                else if (button.Name == nameof(buttonGainCalibrationFileA))
                {
                    var ofd = new OpenFileDialog()
                    {
                        CheckFileExists = true,
                        Filter = "Gain calibration files (*_gainCalValues.csv)|*_gainCalValues.csv|All Files|*.*",
                        FilterIndex = 0
                    };

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        textBoxProbeCalibrationFileA.Text = ofd.FileName;
                        panelProbeA.Visible = true;
                    }
                    else
                    {
                        panelProbeA.Visible = File.Exists(textBoxProbeCalibrationFileA.Text);
                    }
                }
                else if (button.Name == nameof(buttonGainCalibrationFileB))
                {
                    var ofd = new OpenFileDialog()
                    {
                        CheckFileExists = true,
                        Filter = "Gain calibration files (*_gainCalValues.csv)|*_gainCalValues.csv|All Files|*.*",
                        FilterIndex = 0
                    };

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        textBoxProbeCalibrationFileB.Text = ofd.FileName;
                        panelProbeB.Visible = true;
                    }
                    else
                    {
                        panelProbeB.Visible = File.Exists(textBoxProbeCalibrationFileB.Text);
                    }
                }
                else if (button.Name == nameof(buttonZoomIn))
                {
                    var probeSelected = tabControlProbe.SelectedTab == tabPageProbeA ? NeuropixelsV2Probe.ProbeA : NeuropixelsV2Probe.ProbeB;

                    ZoomIn(zoomFactor, probeSelected);
                }
                else if (button.Name == nameof(buttonZoomOut))
                {
                    var probeSelected = tabControlProbe.SelectedTab == tabPageProbeA ? NeuropixelsV2Probe.ProbeA : NeuropixelsV2Probe.ProbeB;

                    ZoomOut(1 / zoomFactor, probeSelected);
                }
                else if (button.Name == nameof(buttonResetZoom))
                {
                    var probeSelected = tabControlProbe.SelectedTab == tabPageProbeA ? NeuropixelsV2Probe.ProbeA : NeuropixelsV2Probe.ProbeB;

                    ResetZoom(probeSelected);
                }
                else if (button.Name == nameof(buttonClearSelections))
                {
                    var channelConfiguration = tabControlProbe.SelectedTab == tabPageProbeA ? ChannelConfigurationA : ChannelConfigurationB;

                    channelConfiguration.SetAllSelections(false);
                    channelConfiguration.HighlightEnabledContacts();
                    channelConfiguration.HighlightSelectedContacts();
                    channelConfiguration.UpdateContactLabels();
                    channelConfiguration.RefreshZedGraph();
                }
                else if (button.Name == nameof(buttonEnableContacts))
                {
                    var channelConfiguration = tabControlProbe.SelectedTab == tabPageProbeA ? ChannelConfigurationA : ChannelConfigurationB;

                    EnableSelectedContacts((NeuropixelsV2Probe)channelConfiguration.Tag);

                    channelConfiguration.SetAllSelections(false);
                    channelConfiguration.HighlightEnabledContacts();
                    channelConfiguration.HighlightSelectedContacts();
                    channelConfiguration.UpdateContactLabels();
                    channelConfiguration.RefreshZedGraph();
                }
                else if (button.Name == nameof(buttonClearCalibrationFileA))
                {
                    textBoxProbeCalibrationFileA.Text = "";
                    panelProbeA.Visible = false;
                }
                else if (button.Name == nameof(buttonClearCalibrationFileB))
                {
                    textBoxProbeCalibrationFileB.Text = "";
                    panelProbeB.Visible = false;
                }
            }
        }

        internal void UpdateProbeGroups()
        {
            NeuropixelsV2eProbeGroup.UpdateProbeGroup(ChannelConfigurationA.ChannelMap, ConfigureNode.ProbeConfigurationA.ChannelConfiguration);
            NeuropixelsV2eProbeGroup.UpdateProbeGroup(ChannelConfigurationB.ChannelMap, ConfigureNode.ProbeConfigurationB.ChannelConfiguration);
        }

        private void EnableSelectedContacts(NeuropixelsV2Probe probeSelected)
        {
            var channelConfiguration = probeSelected == NeuropixelsV2Probe.ProbeA ? ChannelConfigurationA : ChannelConfigurationB;

            if (channelConfiguration.SelectedContacts.Length != channelConfiguration.Electrodes.Count)
                throw new Exception("Invalid number of contacts versus electrodes found.");

            var selectedElectrodes = channelConfiguration.Electrodes
                                                          .Where((e, ind) => channelConfiguration.SelectedContacts[ind])
                                                          .ToList();

            channelConfiguration.EnableElectrodes(selectedElectrodes);

            CheckForExistingChannelPreset(probeSelected);
        }

        private void ZoomIn(double zoom, NeuropixelsV2Probe probeSelected)
        {
            if (zoom <= 1)
            {
                throw new ArgumentOutOfRangeException($"Argument {nameof(zoom)} must be greater than 1.0 to zoom in");
            }

            var channelConfiguration = probeSelected == NeuropixelsV2Probe.ProbeA ? ChannelConfigurationA : ChannelConfigurationB;

            channelConfiguration.ManualZoom(zoom);
            channelConfiguration.RefreshZedGraph();
        }

        private void ZoomOut(double zoom, NeuropixelsV2Probe probeSelected)
        {
            if (zoom >= 1)
            {
                throw new ArgumentOutOfRangeException($"Argument {nameof(zoom)} must be less than 1.0 to zoom out");
            }

            var channelConfiguration = probeSelected == NeuropixelsV2Probe.ProbeA ? ChannelConfigurationA : ChannelConfigurationB;

            channelConfiguration.ManualZoom(zoom);
            channelConfiguration.RefreshZedGraph();
        }

        private void ResetZoom(NeuropixelsV2Probe probeSelected)
        {
            var channelConfiguration = probeSelected == NeuropixelsV2Probe.ProbeA ? ChannelConfigurationA : ChannelConfigurationB;

            channelConfiguration.ResetZoom();
            channelConfiguration.RefreshZedGraph();
        }

        private void MoveToVerticalPosition(float relativePosition, NeuropixelsV2Probe probeSelected)
        {
            var channelConfiguration = probeSelected == NeuropixelsV2Probe.ProbeA ? ChannelConfigurationA : ChannelConfigurationB;

            channelConfiguration.MoveToVerticalPosition(relativePosition);
            channelConfiguration.RefreshZedGraph();
        }

        private void TrackBarScroll(object sender, EventArgs e)
        {
            if (sender is TrackBar trackBar && trackBar != null)
            {
                if (trackBar.Name == nameof(trackBarProbePosition))
                {
                    var probeSelected = tabControlProbe.SelectedTab == tabPageProbeA ? NeuropixelsV2Probe.ProbeA : NeuropixelsV2Probe.ProbeB;

                    MoveToVerticalPosition(trackBar.Value / 100.0f, probeSelected);
                }
            }
        }

        private void UpdateTrackBarLocation(object sender, EventArgs e)
        {
            var channelConfiguration = tabControlProbe.SelectedTab == tabPageProbeA ? ChannelConfigurationA : ChannelConfigurationB;

            trackBarProbePosition.Value = (int)(channelConfiguration.GetRelativeVerticalPosition() * 100);
        }
    }
}
