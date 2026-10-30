using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Forms;
using Hexa.NET.ImGui;
using Newtonsoft.Json;
using OpenEphys.ProbeInterface.NET;
using OpenEphys.EphysVisualization.Design;

namespace OpenEphys.Onix1.Design
{
    // Core of the NeuropixelsV2e ImGui configuration dialog: state, lifecycle, the
    // props-panel layout, and shared channel bookkeeping. The remaining concerns are
    // split across sibling partial files:
    //   .Channels.cs — probe type, channel config UI, contact info, enable/pin actions
    //   .Files.cs — gain cal + probe interface file I/O and close prompt
    //   .Survey.cs — electrode survey UI, log, and activity coloring
    internal partial class NeuropixelsV2eImGuiDialog : ImGuiNeuropixelsDialog<NeuropixelsV2ProbeGroup>
    {
        // Identity
        readonly bool isBeta;

        // Config node
        readonly IConfigureNeuropixelsV2 configureNode;

        // Probe state
        NeuropixelsV2ProbeGroup probeGroup;

        // Survey (execution/hardware config lives on the headstage-level control panel; this dialog only
        // owns its own results/status and the activity-view + bank-selection UI that reads/drives them)
        readonly NeuropixelsV2eSurveyState survey = new();

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
        readonly HashSet<(int shank, int bank)> surveyBanks  = new();
        readonly HashSet<(int shank, int bank)> pendingBanks = new();

        // Combo / preset state
        int presetIdx, refIdx;
        NeuropixelsV2ChannelPreset[] presets = Array.Empty<NeuropixelsV2ChannelPreset>();
        static readonly NeuropixelsV2ReferenceSource[] ReferenceValues = (NeuropixelsV2ReferenceSource[])Enum.GetValues(typeof(NeuropixelsV2ReferenceSource));
        static readonly string[] ReferenceNames = Enum.GetNames(typeof(NeuropixelsV2ReferenceSource));

        // Input buffers
        readonly byte[] gainCalBuf = new byte[512];

        // Visual constants
        static readonly Vector4 ColorTextWarning = ImGui.ColorConvertU32ToFloat4(ImGuiPalette.AmberGold);
        static readonly Vector4 ColorTextSuccess = ImGui.ColorConvertU32ToFloat4(ImGuiPalette.BrightFern);

        /// <summary>
        /// The configuration node updated by this dialog.
        /// </summary>
        internal IConfigureNeuropixelsV2 ConfigureNeuropixelsV2 => configureNode;

        /// <summary>
        /// This probe's own survey results/status. Written into by the headstage-level survey runner,
        /// read by this dialog's activity-view UI.
        /// </summary>
        internal NeuropixelsV2eSurveyState Survey => survey;

        /// <summary>
        /// This probe's own headstage-scan result/status. Written into by the headstage-level scan runner,
        /// read by this dialog's control panel and calibration-file UI.
        /// </summary>
        internal NeuropixelsV1ScanState Scan => scan;

        /// <summary>
        /// Which (shank, bank) pairs this probe's survey should sweep, as selected via the probe-view
        /// drag-select mechanism. Read by the headstage-level survey runner.
        /// </summary>
        internal HashSet<(int shank, int bank)> SurveyBanks => surveyBanks;

        #region Base class seams

        /// <summary>
        /// The current probe group. Read by the headstage-level survey runner to enumerate contacts/banks.
        /// </summary>
        internal override NeuropixelsV2ProbeGroup ProbeGroup => probeGroup;

        protected override IProbeInterfaceConfiguration ProbeConfigurationBase => configureNode.ProbeConfiguration;

        protected override string PrimaryCalibrationFileName => configureNode.ProbeConfiguration.GainCalibrationFileName;

        protected override bool InvertPolarity
        {
            get => configureNode.ProbeConfiguration.InvertPolarity;
            set => configureNode.ProbeConfiguration.InvertPolarity = value;
        }

        protected override void EnableElectrodes(IEnumerable<int> contactIndices) => probeGroup.EnableElectrodes(contactIndices);

        protected override void ReplaceProbeGroupFromJson(string json) =>
            probeGroup = JsonConvert.DeserializeObject<NeuropixelsV2ProbeGroup>(json)
                ?? throw new InvalidDataException("The probe interface data did not produce a valid probe group.");

        protected override void OnProbeGroupRefreshed()
        {
            presets = probeGroup.GetChannelPresets().ToArray();
            presetIdx = Array.FindIndex(presets, p => p == NeuropixelsV2ChannelPreset.None);

            refIdx = Math.Max(0, Array.IndexOf(ReferenceValues, configureNode.ProbeConfiguration.Reference));
            configureNode.ProbeConfiguration.TipReferenceShanks.RemoveAll(s => s < 0 || s >= probeGroup.ShankCount);

            RestoreActivityData();
            InitSurveyBanks();
            WriteBackBufs();
            CheckScannedPartNumberAgainstLoadedFile();
        }

        protected override void OnContactsEnabled()
        {
            presetIdx = Array.FindIndex(presets, p => p == NeuropixelsV2ChannelPreset.None);
        }

        protected override void HandleProbeSpecificShortcuts(bool shift)
        {
            if (!bankSelectMode && ImGui.IsKeyPressed(ImGuiKey.A, false)) showActivityColors = !showActivityColors;
            if (selector.Hovered && ImGui.IsKeyPressed(ImGuiKey.B, false)) SetBankSelectMode(!bankSelectMode);
        }

        void SetBankSelectMode(bool on)
        {
            if (on == bankSelectMode) return;
            bankSelectMode = on;
            selector.ClearSelection();
        }

        void DrawSelectionSection()
        {
            ImGui.Text("Selection");
            ImGui.Spacing();

            if (ImGui.RadioButton("Channels##seltarget", !bankSelectMode)) SetBankSelectMode(false);
            ImGuiWidgets.Tooltip("Select contacts to enable or pin them. Equivalent to pressing B over the probe view.");
            ImGui.SameLine();
            if (ImGui.RadioButton("Banks##seltarget", bankSelectMode)) SetBankSelectMode(true);
            ImGuiWidgets.Tooltip("Select whole banks (384 contacts each) instead of individual contacts, to choose " +
                "which banks the survey sweeps through. Run the survey itself from the headstage panel. Equivalent " +
                "to pressing B over the probe view.");
        }

        void DrawRulerSection()
        {
            ImGui.Text("Rulers");
            ImGui.Spacing();
            ImGuiRulerControls.DrawToggle(selector, "Add");
            ImGui.SameLine();
            ImGuiRulerControls.DrawClear(selector);
        }

        // NB: the selector's settings depend only on bank select and whether activity colors are shown, so they're
        // only updated when one of those changes.
        (bool, bool)? selectorView;

        void UpdateSelectorView()
        {
            bool actColorsActive = !bankSelectMode && showActivityColors && CurrentDisplayData() != null;
            var view = (bankSelectMode, actColorsActive);
            if (view == selectorView) return;
            selectorView = view;

            selector.FillColorOverridesBlocked = bankSelectMode || actColorsActive;
            selector.SelectionSkipsBlocked = !bankSelectMode;
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
        }

        #endregion

        /// <summary>
        /// Opens the dialog.
        /// </summary>
        public NeuropixelsV2eImGuiDialog(IConfigureNeuropixelsV2 configureNode, string probeName, ImGuiLogConsole log)
            : base(probeName, log)
        {
            this.configureNode = configureNode ?? throw new ArgumentNullException(nameof(configureNode));
            isBeta = configureNode is ConfigureNeuropixelsV2BetaPsbDecoder;

            survey.SurveyCompleted += () => HasChanges = true;
            scan.ScanCompleted += OnScanCompleted;

            SetupSelectorCallbacks();
            LoadOrCreateProbeGroup();
            RefreshProbeState();
        }

        #region Selector callback wiring

        static uint BankColor(int bankIndex) => bankIndex % 2 == 0 ? ImGuiPalette.AzureBlue : ImGuiPalette.CobaltBlue;

        void SetupSelectorCallbacks()
        {
            selector.FillColor = idx =>
            {
                if (bankSelectMode)
                {
                    var bank = probeGroup.GetBank(idx);
                    var key = (probeGroup.GetShank(idx), bank);
                    bool pending = pendingBanks.Contains(key);
                    bool inSurvey = surveyBanks.Contains(key);
                    if (pending) return selector.DragIntent == DragSelectIntent.Remove ? ColorContactDisabled : BankColor(bank);
                    return inSurvey ? BankColor(bank) : ColorContactDisabled;
                }
                var data = CurrentDisplayData();
                if (showActivityColors && data != null && idx < data.Length) return ActivityColor(data[idx]);
                return DefaultContactFillColor(idx);
            };
            selector.Blocked = DefaultIsBlocked;
            selector.SelectionChanged += (_, _) =>
            {
                if (!bankSelectMode) return;
                var pending = new HashSet<(int, int)>();
                for (int i = 0; i < selector.Contacts.Count && i < allContacts.Count; i++)
                    if (selector.Contacts[i].InDragBox)
                        pending.Add((probeGroup.GetShank(i), probeGroup.GetBank(i)));
                if (pending.Count == 0) return;
                if (selector.DragIntent == DragSelectIntent.Remove)
                    surveyBanks.ExceptWith(pending);
                else
                    surveyBanks.UnionWith(pending);
                selector.ClearSelection();
            };
        }

        #endregion

        #region ImGuiProbePanel overrides

        protected override void DrawPropsPanel()
        {
            UpdateSelectorView();

            if (bankSelectMode)
            {
                pendingBanks.Clear();
                for (int i = 0; i < selector.Contacts.Count && i < allContacts.Count; i++)
                    if (selector.Contacts[i].InDragBox)
                        pendingBanks.Add((probeGroup.GetShank(i), probeGroup.GetBank(i)));
            }

            ImGui.Spacing();
            DrawTitleBar();
            ImGui.Separator();
            DrawFileSection();
            ImGui.Separator();
            DrawSelectionSection();
            ImGui.Separator();
            DrawRulerSection();
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
                    ReplaceProbeGroupFromJson(File.ReadAllText(pc.ProbeInterfaceFileName));
                    Log($"Loaded probeinterface file {pc.ProbeInterfaceFileName}");
                    return;
                }
                catch (Exception ex)
                {
                    Log($"Error loading probeinterface file {pc.ProbeInterfaceFileName}: {ex.Message}", true);
                }
            }
            // No file configured (or it failed to load): start from the quad-shank probe, the more
            // prevalent NeuropixelsV2e configuration in the field, as a reasonable default to edit from.
            ReplaceProbeGroupFromJson(DesignResource.LoadDefaultJson("NP2013.json"));
        }

        void InitSurveyBanks()
        {
            if (surveyBanks.Count > 0) return;
            for (int i = 0; i < allContacts.Count; i++)
                surveyBanks.Add((probeGroup.GetShank(i), probeGroup.GetBank(i)));
        }

        #endregion

        #region Misc helpers

        void WriteBackBufs()
        {
            ImGuiStrings.WriteString(gainCalBuf, configureNode.ProbeConfiguration.GainCalibrationFileName ?? "");
            ImGuiStrings.WriteString(probeFileBuf, configureNode.ProbeConfiguration.ProbeInterfaceFileName  ?? "");
        }

        #endregion
    }
}
