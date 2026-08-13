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
    /// calibration file inputs, probe-interface file I/O, and channel enable/pin/preset.
    /// </summary>
    internal partial class NeuropixelsV1ImGuiDialog : ImGuiNeuropixelsDialog<NeuropixelsV1ProbeGroup>
    {
        readonly IConfigureNeuropixelsV1 configureNode;
        NeuropixelsV1ProbeGroup probeGroup;

        // Survey (execution/hardware config lives on the headstage-level survey panel; this dialog only
        // owns its own results/status and the activity-view + bank-selection UI that reads/drives them)
        readonly NeuropixelsV1SurveyState survey = new();
        SurveyActivityMetric selectedMetric = SurveyActivityMetric.SNR;
        float actMin, actMax = 1f;
        float actDomainMin, actDomainMax = 1f;
        bool actRangeInitialized;
        readonly Dictionary<SurveyActivityMetric, (float min, float max)> actRanges = new();
        bool showActivityColors = false;
        bool bankSelectMode = false;
        readonly HashSet<NeuropixelsV1Bank> surveyBanks = new();
        readonly HashSet<NeuropixelsV1Bank> pendingBanks = new();

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

        /// <summary>
        /// This probe's own survey results/status. Written into by the headstage-level survey runner,
        /// read by this dialog's activity-view UI.
        /// </summary>
        internal NeuropixelsV1SurveyState Survey => survey;

        /// <summary>
        /// Which banks this probe's survey should sweep, as selected via the probe-view drag-select
        /// mechanism. Read by the headstage-level survey runner.
        /// </summary>
        internal HashSet<NeuropixelsV1Bank> SurveyBanks => surveyBanks;

        #region Base class seams

        internal override NeuropixelsV1ProbeGroup ProbeGroup => probeGroup;

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

            RestoreActivityData();
            InitSurveyBanks();
        }

        protected override void HandleProbeSpecificShortcuts(bool shift)
        {
            if (ImGui.IsKeyPressed(ImGuiKey.A, false)) showActivityColors = !showActivityColors;
            if (ImGui.IsKeyPressed(ImGuiKey.B, false))
            {
                bankSelectMode = !bankSelectMode;
                selector.ClearSelection();
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV1ImGuiDialog"/>.
        /// </summary>
        public NeuropixelsV1ImGuiDialog(IConfigureNeuropixelsV1 configureNode, string probeName, ImGuiLogConsole log)
            : base(probeName, log)
        {
            selector.DefaultZoomWindowMicrons = 2000f;
            selector.DefaultScrollYMicrons = 700f;

            this.configureNode = configureNode ?? throw new ArgumentNullException(nameof(configureNode));

            survey.SurveyCompleted += () => HasChanges = true;

            SetupSelectorCallbacks();
            LoadOrCreateProbeGroup();
            RefreshProbeState();
        }

        static uint BankColor(NeuropixelsV1Bank bank) => bank switch
        {
            NeuropixelsV1Bank.A => ImGuiPalette.AzureBlue,
            NeuropixelsV1Bank.B => ImGuiPalette.CobaltBlue,
            NeuropixelsV1Bank.C => ImGuiPalette.AzureBlue,
            _ => throw new ArgumentOutOfRangeException(nameof(bank), $"Invalid NeuropixelsV1 bank: {bank}"),
        };

        void SetupSelectorCallbacks()
        {
            selector.GetFillColor = idx =>
            {
                if (bankSelectMode)
                {
                    var bank = NeuropixelsV1ProbeGroup.GetBank(idx);
                    bool pending = pendingBanks.Contains(bank);
                    bool inSurvey = surveyBanks.Contains(bank);
                    if (pending) return selector.DragIntent == DragSelectIntent.Remove ? ColorContactDisabled : BankColor(bank);
                    return inSurvey ? BankColor(bank) : ColorContactDisabled;
                }
                var data = CurrentDisplayData();
                if (showActivityColors && data != null && idx < data.Length) return ActivityColor(data[idx]);
                return DefaultContactFillColor(idx);
            };
            selector.IsBlocked = DefaultIsBlocked;
            selector.SelectionChanged += (_, _) =>
            {
                if (!bankSelectMode) return;
                var pending = new HashSet<NeuropixelsV1Bank>();
                for (int i = 0; i < selector.DragBoxContacts.Length && i < allContacts.Count; i++)
                    if (selector.DragBoxContacts[i])
                        pending.Add(NeuropixelsV1ProbeGroup.GetBank(i));
                if (pending.Count == 0) return;
                if (selector.DragIntent == DragSelectIntent.Remove)
                    surveyBanks.ExceptWith(pending);
                else
                    surveyBanks.UnionWith(pending);
                selector.ClearSelection();
            };
        }

        #region ImGuiProbePanel overrides

        protected override void DrawPropsPanel()
        {
            selector.FillColorOverridesBlocked = bankSelectMode || (showActivityColors && CurrentDisplayData() != null);
            selector.SelectionSkipsBlocked = !bankSelectMode;
            selector.ModeLabel = bankSelectMode ? "MODE: BANK SELECT (B to exit)" : "MODE: CHANNEL SELECT";
            bool actColorsActive = !bankSelectMode && showActivityColors && CurrentDisplayData() != null;
            selector.Legend = bankSelectMode
                ? new LegendEntry[]
                  {
                      new(ColorContactDisabled, "Excluded from survey"),
                  }
                : actColorsActive
                    ? new LegendEntry[]
                      {
                          new(ImGuiProbeSelector.SelectionBorderColor, "Editable selection", OutlineOnly: true),
                      }
                    : new LegendEntry[]
                      {
                          new(ColorContactPinned, "Enabled & Pinned"),
                          new(ColorContactEnabled, "Enabled"),
                          new(ColorContactDisabled, "Disabled"),
                          new(ImGuiProbeSelector.BlockedFillColor, "Unavailable", DottedOutline: true),
                          new(ImGuiProbeSelector.SelectionBorderColor, "Editable selection", OutlineOnly: true),
                      };

            if (bankSelectMode)
            {
                pendingBanks.Clear();
                for (int i = 0; i < selector.DragBoxContacts.Length && i < allContacts.Count; i++)
                    if (selector.DragBoxContacts[i])
                        pendingBanks.Add(NeuropixelsV1ProbeGroup.GetBank(i));
            }

            ImGui.Spacing();
            DrawTitleBar();
            ImGui.Separator();
            DrawFileSection();
            ImGui.Separator();
            DrawChannelSection();
            ImGui.Separator();
            DrawSurveySection();
            ImGui.Separator();
            DrawContactInfo();
            HandleKeyboardShortcuts();
        }

        public override bool CanClose(DialogResult pendingResult)
        {
            if (HasChanges)
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

        void InitSurveyBanks()
        {
            if (surveyBanks.Count > 0) return;
            for (int i = 0; i < allContacts.Count; i++)
                surveyBanks.Add(NeuropixelsV1ProbeGroup.GetBank(i));
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
