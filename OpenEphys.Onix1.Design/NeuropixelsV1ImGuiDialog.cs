using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Hexa.NET.ImGui;
using Newtonsoft.Json;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// ImGui-based configuration dialog for a NeuropixelsV1 probe: gain/reference/filter controls,
    /// calibration file inputs, probe-interface file I/O, and the channel enable/pin/preset workflow
    /// shared with NeuropixelsV2e via <see cref="ImGuiNeuropixelsDialog"/>.
    /// </summary>
    internal sealed class NeuropixelsV1ImGuiDialog : ImGuiNeuropixelsDialog
    {
        readonly IConfigureNeuropixelsV1 configureNode;
        NeuropixelsV1ProbeGroup probeGroup;

        int spikeGainIdx, lfpGainIdx, refIdx, presetIdx;

        static readonly string[] GainNames = Enum.GetNames(typeof(NeuropixelsV1Gain));
        static readonly NeuropixelsV1Gain[] GainValues = (NeuropixelsV1Gain[])Enum.GetValues(typeof(NeuropixelsV1Gain));
        static readonly string[] ReferenceNames = Enum.GetNames(typeof(NeuropixelsV1ReferenceSource));
        static readonly NeuropixelsV1ReferenceSource[] ReferenceValues = (NeuropixelsV1ReferenceSource[])Enum.GetValues(typeof(NeuropixelsV1ReferenceSource));
        static readonly string[] PresetNames = Enum.GetNames(typeof(NeuropixelsV1ChannelPreset));
        static readonly NeuropixelsV1ChannelPreset[] PresetValues = (NeuropixelsV1ChannelPreset[])Enum.GetValues(typeof(NeuropixelsV1ChannelPreset));

        readonly byte[] gainCalBuf = new byte[512];
        readonly byte[] adcCalBuf = new byte[512];

        /// <summary>
        /// The configuration node updated by this dialog.
        /// </summary>
        internal IConfigureNeuropixelsV1 ConfigureNeuropixelsV1 => configureNode;

        #region Base class seams

        protected override SingleProbeGroup ProbeInterfaceGroup => probeGroup;

        protected override IMultiplexedProbeGroup MultiplexedProbeGroup => probeGroup;

        protected override IProbeInterfaceConfiguration ProbeConfigurationBase => configureNode.ProbeConfiguration;

        protected override string PrimaryCalibrationFileName => configureNode.ProbeConfiguration.GainCalibrationFileName;

        protected override bool InvertPolarity
        {
            get => configureNode.ProbeConfiguration.InvertPolarity;
            set => configureNode.ProbeConfiguration.InvertPolarity = value;
        }

        protected override void EnableElectrodes(IEnumerable<int> contactIndices) => probeGroup.EnableElectrodes(contactIndices);

        protected override void ReplaceProbeGroupFromFile(string path) =>
            probeGroup = JsonConvert.DeserializeObject<NeuropixelsV1ProbeGroup>(File.ReadAllText(path)) ?? new NeuropixelsV1ProbeGroup();

        protected override void OnProbeGroupRefreshed()
        {
            spikeGainIdx = Array.IndexOf(GainValues, configureNode.ProbeConfiguration.SpikeAmplifierGain);
            lfpGainIdx   = Array.IndexOf(GainValues, configureNode.ProbeConfiguration.LfpAmplifierGain);
            refIdx       = Array.IndexOf(ReferenceValues, configureNode.ProbeConfiguration.Reference);

            ImGuiControls.WriteString(gainCalBuf, configureNode.ProbeConfiguration.GainCalibrationFileName ?? "");
            ImGuiControls.WriteString(adcCalBuf, configureNode.ProbeConfiguration.AdcCalibrationFileName ?? "");
            ImGuiControls.WriteString(probeFileBuf, configureNode.ProbeConfiguration.ProbeInterfaceFileName ?? "");
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV1ImGuiDialog"/>.
        /// </summary>
        public NeuropixelsV1ImGuiDialog(IConfigureNeuropixelsV1 configureNode, string probeName)
            : base(probeName)
        {
            selector.DefaultZoomWindowMicrons = 2000f;
            selector.DefaultScrollYMicrons = 700f;

            this.configureNode = configureNode ?? throw new ArgumentNullException(nameof(configureNode));

            SetupSelectorCallbacks();
            LoadOrCreateProbeGroup();
            RefreshProbeState();
        }

        void SetupSelectorCallbacks()
        {
            selector.GetFillColor = DefaultContactFillColor;
            selector.IsBlocked = DefaultIsBlocked;
        }

        #region ImGuiProbePanel overrides

        protected override void DrawPropsPanel()
        {
            selector.FillColorOverridesBlocked = false;
            selector.SelectionSkipsBlocked = true;
            selector.ModeLabel = "MODE: CHANNEL SELECT";
            selector.Legend = new LegendEntry[]
            {
                new(ColorContactPinned, "Enabled & Pinned"),
                new(ColorContactEnabled, "Enabled"),
                new(ColorContactDisabled, "Disabled"),
                new(ImGuiProbeSelector.BlockedFillColor, "Unavailable", DottedOutline: true),
                new(ImGuiProbeSelector.SelectionBorderColor, "Editable selection", OutlineOnly: true),
            };

            ImGui.Spacing();
            DrawTitleBar();
            ImGui.Separator();
            DrawFileSection();
            ImGui.Separator();
            DrawChannelSection();
            ImGui.Separator();
            DrawContactInfo();
            HandleKeyboardShortcuts();
        }

        public override bool CanClose(DialogResult pendingResult)
        {
            if (hasChanges && pendingResult != DialogResult.Cancel)
                return PromptSaveOnClose();
            return true;
        }

        #endregion

        #region Probe group init

        void LoadOrCreateProbeGroup()
        {
            var pc = configureNode.ProbeConfiguration;
            if (!string.IsNullOrEmpty(pc.ProbeInterfaceFileName))
            {
                try
                {
                    probeGroup = JsonConvert.DeserializeObject<NeuropixelsV1ProbeGroup>(File.ReadAllText(pc.ProbeInterfaceFileName))
                                 ?? new NeuropixelsV1ProbeGroup();
                    Log($"Loaded probeinterface file {pc.ProbeInterfaceFileName}");
                    return;
                }
                catch (Exception ex)
                {
                    Log($"Error loading probeinterface file {pc.ProbeInterfaceFileName}: {ex.Message}", true);
                }
            }
            probeGroup = new NeuropixelsV1ProbeGroup();
        }

        #endregion

        #region Calibration files

        protected override void DrawCalibrationFileSection()
        {
            ImGui.Text("Gain Calibration File");
            ImGui.Spacing();
            DrawCalFileRow(gainCalBuf, "##gaincal",
                () => configureNode.ProbeConfiguration.GainCalibrationFileName,
                v => configureNode.ProbeConfiguration.GainCalibrationFileName = v,
                "Gain calibration files (*_gainCalValues.csv)|*_gainCalValues.csv|All files (*.*)|*.*",
                "Select Gain Calibration File",
                "Path to the gain calibration file for this probe. Required to acquire data from the probe.");

            ImGui.Spacing();
            ImGui.Text("ADC Calibration File");
            ImGui.Spacing();
            DrawCalFileRow(adcCalBuf, "##adccal",
                () => configureNode.ProbeConfiguration.AdcCalibrationFileName,
                v => configureNode.ProbeConfiguration.AdcCalibrationFileName = v,
                "ADC calibration files (*_ADCCalibration.csv)|*_ADCCalibration.csv|All files (*.*)|*.*",
                "Select ADC Calibration File",
                "Path to the ADC calibration file for this probe. Required to acquire data from the probe.");
        }

        void DrawCalFileRow(byte[] buf, string id, Func<string> getPath, Action<string> setPath,
            string filter, string dialogTitle, string tooltip)
        {
            float fileTargetW = ComputeFileRowInputWidth();
            string current = getPath() ?? "";
            ImGuiControls.WriteString(buf, current);
            ImGui.SetNextItemWidth(fileTargetW);
            unsafe
            {
                fixed (byte* p = buf)
                    if (ImGui.InputText(id, p, (nuint)buf.Length))
                        setPath(ImGuiControls.ReadBuffer(buf));
            }
            if (!string.IsNullOrEmpty(getPath()))
                ImGuiControls.Tooltip(getPath());

            ImGui.SameLine();
            if (ImGui.Button("Open..." + id))
            {
                using var ofd = new OpenFileDialog { Title = dialogTitle, Filter = filter };
                if (!string.IsNullOrEmpty(current) && File.Exists(current))
                    ofd.InitialDirectory = Path.GetDirectoryName(current);
                if (ofd.ShowDialog() == DialogResult.OK)
                    setPath(ofd.FileName);
            }
            ImGuiControls.Tooltip(tooltip);
        }

        #endregion

        #region Channel controls

        protected override void DrawProbeSpecificChannelControls()
        {
            ImGui.SetNextItemWidth(ComboboxStartWidthPx);
            if (ImGui.Combo("Spike Gain##spikegain", ref spikeGainIdx, GainNames, GainNames.Length))
                configureNode.ProbeConfiguration.SpikeAmplifierGain = GainValues[spikeGainIdx];
            ImGuiControls.Tooltip("Amplifier gain applied to the spike-band (300 Hz-10 kHz, or DC-10 kHz if the spike filter is off).");

            ImGui.SetNextItemWidth(ComboboxStartWidthPx);
            if (ImGui.Combo("LFP Gain##lfpgain", ref lfpGainIdx, GainNames, GainNames.Length))
                configureNode.ProbeConfiguration.LfpAmplifierGain = GainValues[lfpGainIdx];
            ImGuiControls.Tooltip("Amplifier gain applied to the LFP band (0.5-500 Hz).");

            ImGui.SetNextItemWidth(ComboboxStartWidthPx);
            if (ImGui.Combo("Reference##ref", ref refIdx, ReferenceNames, ReferenceNames.Length))
                configureNode.ProbeConfiguration.Reference = ReferenceValues[refIdx];
            ImGuiControls.Tooltip("Choose the voltage reference for every recording channel: External or the probe Tip electrode.");

            bool spikeFilter = configureNode.ProbeConfiguration.SpikeFilter;
            if (ImGui.Checkbox("300 Hz spike-band high-pass filter##filter", ref spikeFilter))
                configureNode.ProbeConfiguration.SpikeFilter = spikeFilter;
            ImGuiControls.Tooltip("Activate a 300 Hz high-pass filter on the spike-band data stream.");

            ImGui.SetNextItemWidth(ComboboxStartWidthPx);
            if (ImGui.Combo("Preset##preset", ref presetIdx, PresetNames, PresetNames.Length))
                ApplyPreset(PresetValues[presetIdx]);
            ImGuiControls.Tooltip("Apply a ready-made channel selection in a single step. Overrides current enabled/pinned contacts.");
        }

        void ApplyPreset(NeuropixelsV1ChannelPreset preset)
        {
            probeGroup.SelectPreset(preset);
            RebuildMaps();
            ClearPins();
            selector.ClearSelection();
            HasChanges = true;
        }

        #endregion

        protected override void DrawProbeSpecificContactInfo(IReadOnlyList<int> sel)
        {
            var banks = sel.Select(NeuropixelsV1ProbeGroup.GetBank).Distinct();
            ImGuiControls.InfoRow("Bank(s)", banks.Any() ? string.Join(",", banks) : "-");
        }
    }
}
