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

        // Survey (execution/hardware config lives on the headstage-level control panel; this dialog only
        // owns its own results/status and the activity-view + bank-selection UI that reads/drives them)
        readonly NeuropixelsV1SurveyState survey = new();

        // Scan (execution/hardware config lives on the headstage-level control panel; this dialog only
        // owns its own result/status)
        readonly NeuropixelsV1ScanState scan = new();
        SurveyActivityMetric selectedMetric = SurveyActivityMetric.SNR;
        float actMin, actMax = 1f;
        float actDomainMin, actDomainMax = 1f;
        bool actRangeInitialized;
        readonly Dictionary<SurveyActivityMetric, (float min, float max)> actRanges = new();
        bool showActivityColors = false;
        bool bankSelectMode = false;
        readonly HashSet<int> surveyBanks = new();
        readonly HashSet<int> pendingBanks = new();

        int spikeGainIdx, lfpGainIdx, refIdx, presetIdx, columnPatternIdx, presetOffset;
        NeuropixelsV1Preset[] presets = Array.Empty<NeuropixelsV1Preset>();

        static readonly string[] GainNames = Enum.GetNames(typeof(NeuropixelsV1Gain));
        static readonly NeuropixelsV1Gain[] GainValues = (NeuropixelsV1Gain[])Enum.GetValues(typeof(NeuropixelsV1Gain));
        static readonly string[] ReferenceNames = Enum.GetNames(typeof(NeuropixelsV1ReferenceSource));
        static readonly NeuropixelsV1ReferenceSource[] ReferenceValues = (NeuropixelsV1ReferenceSource[])Enum.GetValues(typeof(NeuropixelsV1ReferenceSource));
        static readonly NeuropixelsV1ColumnPattern[] ColumnPatternValues = (NeuropixelsV1ColumnPattern[])Enum.GetValues(typeof(NeuropixelsV1ColumnPattern));
        static readonly string[] ColumnPatternNames = ColumnPatternValues.Select(ColumnPatternDisplayName).ToArray();

        // "All", or "Inner (1, 3, 4, 6)"/"Outer (0, 2, 5, 7)". Reads the same active-column sets
        // NeuropixelsV1ProbeGroup.IsColumnEnabled uses, so the label can't drift from the actual behavior.
        static string ColumnPatternDisplayName(NeuropixelsV1ColumnPattern pattern) =>
            NeuropixelsV1VariantRegistry.Np1110ActiveColumnsByPattern.TryGetValue(pattern, out var columns)
                ? $"{pattern} ({string.Join(", ", columns)})"
                : pattern.ToString(); // All

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
        /// This probe's own headstage-scan result/status. Written into by the headstage-level scan runner,
        /// read by this dialog's control panel and calibration-file UI.
        /// </summary>
        internal NeuropixelsV1ScanState Scan => scan;

        /// <summary>
        /// Which banks this probe's survey should sweep, as selected via the probe-view drag-select
        /// mechanism. Read by the headstage-level survey runner.
        /// </summary>
        internal HashSet<int> SurveyBanks => surveyBanks;

        #region Base class seams

        internal override NeuropixelsV1ProbeGroup ProbeGroup => probeGroup;

        protected override IProbeInterfaceConfiguration ProbeConfigurationBase => configureNode.ProbeConfiguration;

        protected override string PrimaryCalibrationFileName => configureNode.ProbeConfiguration.GainCalibrationFileName;

        protected override bool InvertPolarity
        {
            get => configureNode.ProbeConfiguration.InvertPolarity;
            set => configureNode.ProbeConfiguration.InvertPolarity = value;
        }

        // Enabling a contact enables the whole channel group it belongs to: every distinct (channel
        // group, bank) pair implied by the given contacts is selected via SelectElectrodeGroup, which
        // replaces that group's prior bank selection with just the bank(s) touched by this call. 
        // A single contact selects one bank, a selection spanning two complementary (normal + row-
        // crossed) contacts of the same group selects both banks (zig-zag) in one action.
        protected override void EnableElectrodes(IEnumerable<int> contactIndices)
        {
            if (probeGroup is NeuropixelsV1ContactProbeGroup contactProbe)
            {
                contactProbe.EnableElectrodes(contactIndices);
                return;
            }

            var channelGroupProbe = (NeuropixelsNP1110ProbeGroup)probeGroup;

            var banksByGroup = new Dictionary<int, HashSet<int>>();
            foreach (var contactIdx in contactIndices)
            {
                int group = channelGroupProbe.GetChannelGroup(contactIdx);
                int bank = NeuropixelsV1ProbeGroup.GetBank(contactIdx);
                if (!banksByGroup.TryGetValue(group, out var banks))
                    banksByGroup[group] = banks = new HashSet<int>();
                banks.Add(bank);
            }

            foreach (var entry in banksByGroup)
            {
                int group = entry.Key;
                var banks = entry.Value;

                // A pinned bank is immutable: it's never silently dropped by a new enable action, only by
                // explicitly unpinning it first. Any *unpinned* existing bank for this group remains fair
                // game to be replaced by whatever this gesture selects (confirmed "replace with this gesture"
                // semantics). Without this, selecting the complementary row-crossed bank for a zig-zag pair
                // would evict the already-pinned bank instead of joining it.
                foreach (var existingBank in channelGroupProbe.GetSelectedBanks(group))
                {
                    if (channelGroupProbe.GetSurvivingChannels(group, existingBank).Any(ch => channelState.PinnedChannels.Contains(ch)))
                        banks.Add(existingBank);
                }

                try
                {
                    channelGroupProbe.SelectElectrodeGroup(group, banks.ToArray());
                }
                catch (ArgumentException ex)
                {
                    Log($"Could not enable channel group {group}: {ex.Message}", true);
                }
            }
        }

        // A pinned contact protects whatever its own bank currently produces for its channel group, not the
        // group's full 16-channel set. Under Inner/Outer, a single bank only surviving-drives 8 of a group's
        // 16 channels; the other 8 are exactly what the complementary row-crossed bank supplies for a second
        // bank selection (zig-zag). Pinning all 16 would make that second bank's candidate contacts look like
        // they collide with a pin nothing is actively producing, blocking the very thing Inner/Outer 2-bank
        // selection exists for. 
        protected override IEnumerable<int> GetChannelsToPin(int contactIndex, int channel) =>
            probeGroup is NeuropixelsNP1110ProbeGroup channelGroupProbe
                ? channelGroupProbe.GetSurvivingChannels(channelGroupProbe.GetChannelGroup(contactIndex), NeuropixelsV1ProbeGroup.GetBank(contactIndex))
                : base.GetChannelsToPin(contactIndex, channel);

        protected override void ReplaceProbeGroupFromFile(string path) =>
            probeGroup = JsonConvert.DeserializeObject<NeuropixelsV1ProbeGroup>(File.ReadAllText(path), new NeuropixelsV1ProbeGroupConverter()) ?? new NeuropixelsV1ContactProbeGroup();

        protected override void OnProbeGroupRefreshed()
        {
            presets = probeGroup.GetPresets().ToArray();
            // Which preset (if any) currently produced this channel map can't be recovered from the
            // loaded/restored data, so this always starts at None rather than guessing.
            presetIdx = Array.FindIndex(presets, p => p == NeuropixelsV1Preset.None);
            presetOffset = 0;

            columnPatternIdx = probeGroup is NeuropixelsNP1110ProbeGroup np1110Probe
                ? Array.IndexOf(ColumnPatternValues, np1110Probe.ColumnPattern)
                : 0;

            spikeGainIdx = Array.IndexOf(GainValues, configureNode.ProbeConfiguration.SpikeAmplifierGain);
            lfpGainIdx  = Array.IndexOf(GainValues, configureNode.ProbeConfiguration.LfpAmplifierGain);
            refIdx = Array.IndexOf(ReferenceValues, configureNode.ProbeConfiguration.Reference);

            ImGuiControls.WriteString(gainCalBuf, configureNode.ProbeConfiguration.GainCalibrationFileName ?? "");
            ImGuiControls.WriteString(adcCalBuf, configureNode.ProbeConfiguration.AdcCalibrationFileName ?? "");
            ImGuiControls.WriteString(probeFileBuf, configureNode.ProbeConfiguration.ProbeInterfaceFileName ?? "");

            RestoreActivityData();
            InitSurveyBanks();
            CheckScannedPartNumberAgainstLoadedFile();
        }

        protected override void OnContactsEnabled()
        {
            presetIdx = Array.FindIndex(presets, p => p == NeuropixelsV1Preset.None);
            presetOffset = 0;
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
            scan.ScanCompleted += OnScanCompleted;

            SetupSelectorCallbacks();
            LoadOrCreateProbeGroup();
            RefreshProbeState();
        }

        static uint BankColor(int bankIndex) => bankIndex % 2 == 0 ? ImGuiPalette.AzureBlue : ImGuiPalette.CobaltBlue;

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

            // On variants with per-channel-group bank selection, a contact on a column disabled by the
            // current ColumnPattern is blocked too. It can't be enabled or pinned, in addition to the
            // ordinary pin-collision blocking every variant already gets from DefaultIsBlocked.
            selector.IsBlocked = idx => DefaultIsBlocked(idx) ||
                (probeGroup is NeuropixelsNP1110ProbeGroup channelGroupProbe && !channelGroupProbe.IsColumnEnabled(idx));
            selector.SelectionChanged += (_, _) =>
            {
                if (!bankSelectMode) return;
                var pending = new HashSet<int>();
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
                    probeGroup = JsonConvert.DeserializeObject<NeuropixelsV1ProbeGroup>(File.ReadAllText(pc.ProbeInterfaceFileName), new NeuropixelsV1ProbeGroupConverter())
                                 ?? new NeuropixelsV1ContactProbeGroup();
                    Log($"Loaded probeinterface file {pc.ProbeInterfaceFileName}");
                    return;
                }
                catch (Exception ex)
                {
                    Log($"Error loading probeinterface file {pc.ProbeInterfaceFileName}: {ex.Message}", true);
                }
            }
            probeGroup = new NeuropixelsV1ContactProbeGroup();
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
                "_gainCalValues.csv",
                "Gain calibration files (*_gainCalValues.csv)|*_gainCalValues.csv|All files (*.*)|*.*",
                "Select Gain Calibration File",
                "Path to the gain calibration file for this probe. Required to acquire data from the probe.");

            ImGui.Spacing();
            ImGui.Text("ADC Calibration File");
            ImGui.Spacing();
            DrawCalFileRow(adcCalBuf, "##adccal",
                () => configureNode.ProbeConfiguration.AdcCalibrationFileName,
                v => configureNode.ProbeConfiguration.AdcCalibrationFileName = v,
                "_ADCCalibration.csv",
                "ADC calibration files (*_ADCCalibration.csv)|*_ADCCalibration.csv|All files (*.*)|*.*",
                "Select ADC Calibration File",
                "Path to the ADC calibration file for this probe. Required to acquire data from the probe.");
        }

        void DrawCalFileRow(byte[] buf, string id, Func<string> getPath, Action<string> setPath,
            string fileSuffix, string filter, string dialogTitle, string tooltip)
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
                if (scan.SerialNumber.HasValue)
                {
                    var pattern = $"{scan.SerialNumber}*{fileSuffix}";
                    ofd.Filter = $"This probe's files ({pattern})|{pattern}|{filter}";
                    ofd.FilterIndex = 1;
                }
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

            var presetNames = presets.Select(p => p.ToString()).ToArray();
            ImGui.SetNextItemWidth(ComboboxStartWidthPx);
            if (ImGui.Combo("Preset##preset", ref presetIdx, presetNames, presetNames.Length))
                ApplyPreset(presets[presetIdx]);
            ImGuiControls.Tooltip("Apply a ready-made channel selection in a single step. Overrides current enabled/pinned contacts.");

            if (presets[presetIdx] != NeuropixelsV1Preset.None)
            {
                var preset = presets[presetIdx];
                int maxOffset = probeGroup.MaxOffset(preset);
                ImGui.SetNextItemWidth(ComboboxStartWidthPx);
                if (ImGui.SliderInt("Offset##presetoffset", ref presetOffset, 0, maxOffset))
                    ApplyPreset(preset);
                ImGuiControls.Tooltip("Tip offset, in banks, added to the preset. Clamped so no atom's bank exceeds the probe's bank range.");
            }

            if (probeGroup is NeuropixelsNP1110ProbeGroup np1110Probe)
            {
                ImGui.SetNextItemWidth(ComboboxStartWidthPx);
                if (ImGui.Combo("Column Mode##columnmode", ref columnPatternIdx, ColumnPatternNames, ColumnPatternNames.Length))
                    ApplyColumnPattern(ColumnPatternValues[columnPatternIdx]);
                ImGuiControls.Tooltip("Which physical columns are enabled. Contacts on disabled columns are blocked: they cannot be enabled or pinned.");
            }
        }

        void ApplyPreset(NeuropixelsV1Preset preset)
        {
            if (preset == NeuropixelsV1Preset.None) return;

            try
            {
                probeGroup.SelectPreset(preset, presetOffset);
            }
            catch (ArgumentException ex)
            {
                Log($"Could not apply preset {preset.DisplayName}: {ex.Message}", true);
                presetIdx = Array.FindIndex(presets, p => p == NeuropixelsV1Preset.None);
                return;
            }

            if (probeGroup is NeuropixelsNP1110ProbeGroup np1110Probe)
                columnPatternIdx = Array.IndexOf(ColumnPatternValues, np1110Probe.ColumnPattern);

            RebuildMaps();
            ClearPins();
            RecomputeBlockedIndices();
            selector.ClearSelection();
            HasChanges = true;
        }

        void ApplyColumnPattern(NeuropixelsV1ColumnPattern pattern)
        {
            var channelGroupProbe = (NeuropixelsNP1110ProbeGroup)probeGroup;
            var previous = channelGroupProbe.ColumnPattern;
            if (pattern == previous) return;

            try
            {
                channelGroupProbe.ColumnPattern = pattern;
            }
            catch (InvalidOperationException ex)
            {
                Log($"Could not switch column mode to {pattern}: {ex.Message}", true);
                columnPatternIdx = Array.IndexOf(ColumnPatternValues, previous);
                return;
            }

            presetIdx = Array.FindIndex(presets, p => p == NeuropixelsV1Preset.None);
            presetOffset = 0;
            RebuildMaps();
            RecomputeBlockedIndices();
            HasChanges = true;
        }

        #endregion

        protected override void DrawProbeSpecificContactInfo(IReadOnlyList<int> sel)
        {
            var banks = sel.Select(NeuropixelsV1ProbeGroup.GetBank).Distinct().Select(Neuropixels.BankDisplayName);
            ImGuiControls.InfoRow("Bank(s)", banks.Any() ? string.Join(",", banks) : "-");

            if (probeGroup is NeuropixelsNP1110ProbeGroup channelGroupProbe)
            {
                var groups = sel.Select(channelGroupProbe.GetChannelGroup).Distinct().OrderBy(g => g);
                ImGuiControls.InfoRow("Channel Group(s)", groups.Any() ? string.Join(",", groups) : "-");
            }
        }
    }
}
