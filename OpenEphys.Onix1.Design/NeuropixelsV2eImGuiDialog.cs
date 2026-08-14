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

        // Survey (execution/hardware config lives on the headstage-level survey panel; this dialog only
        // owns its own results/status and the activity-view + bank-selection UI that reads/drives them)
        readonly NeuropixelsV2eSurveyState survey = new();
        SurveyActivityMetric selectedMetric = SurveyActivityMetric.SNR;
        float actMin, actMax = 1f;
        float actDomainMin, actDomainMax = 1f;
        bool actRangeInitialized;
        readonly Dictionary<SurveyActivityMetric, (float min, float max)> actRanges = new();
        bool showActivityColors = false;
        bool bankSelectMode = false;
        readonly HashSet<(int shank, NeuropixelsV2Bank bank)> surveyBanks  = new();
        readonly HashSet<(int shank, NeuropixelsV2Bank bank)> pendingBanks = new();

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
        /// Which (shank, bank) pairs this probe's survey should sweep, as selected via the probe-view
        /// drag-select mechanism. Read by the headstage-level survey runner.
        /// </summary>
        internal HashSet<(int shank, NeuropixelsV2Bank bank)> SurveyBanks => surveyBanks;

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

        protected override void ReplaceProbeGroupFromFile(string path) => LoadProbeGroupFromJson(File.ReadAllText(path));

        void LoadProbeGroupFromJson(string json) =>
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
        }

        protected override void OnContactsEnabled()
        {
            presetIdx = Array.FindIndex(presets, p => p == NeuropixelsV2ChannelPreset.None);
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
        /// Opens the dialog.
        /// </summary>
        public NeuropixelsV2eImGuiDialog(IConfigureNeuropixelsV2 configureNode, string probeName, ImGuiLogConsole log)
            : base(probeName, log)
        {
            // NB: start the view at a 2 mm vertical box that extends 300 um below contact 0
            selector.DefaultZoomWindowMicrons = 2000f;
            selector.DefaultScrollYMicrons = 700f;

            this.configureNode = configureNode ?? throw new ArgumentNullException(nameof(configureNode));
            isBeta = configureNode is ConfigureNeuropixelsV2BetaPsbDecoder;

            survey.SurveyCompleted += () => HasChanges = true;

            SetupSelectorCallbacks();
            LoadOrCreateProbeGroup();
            RefreshProbeState();
        }

        #region Selector callback wiring

        static uint BankColor(NeuropixelsV2Bank bank) => bank switch
        {
            NeuropixelsV2Bank.A => ImGuiPalette.AzureBlue,
            NeuropixelsV2Bank.B => ImGuiPalette.CobaltBlue,
            NeuropixelsV2Bank.C => ImGuiPalette.AzureBlue,
            NeuropixelsV2Bank.D => ImGuiPalette.CobaltBlue,
            _ => throw new ArgumentOutOfRangeException(nameof(bank), $"Invalid neuropixelsV2 bank: {bank}"),
        };

        void SetupSelectorCallbacks()
        {
            selector.GetFillColor = idx =>
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
            selector.IsBlocked = DefaultIsBlocked;
            selector.SelectionChanged += (_, _) =>
            {
                if (!bankSelectMode) return;
                var pending = new HashSet<(int, NeuropixelsV2Bank)>();
                for (int i = 0; i < selector.DragBoxContacts.Length && i < allContacts.Count; i++)
                    if (selector.DragBoxContacts[i])
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
                        pendingBanks.Add((probeGroup.GetShank(i), probeGroup.GetBank(i)));
            }

            ImGui.Spacing();
            DrawTitleBar();
            DrawQuickLoadSection();
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
                    LoadProbeGroupFromJson(File.ReadAllText(pc.ProbeInterfaceFileName));
                    Log($"Loaded probeinterface file {pc.ProbeInterfaceFileName}");
                    return;
                }
                catch (Exception ex)
                {
                    Log($"Error loading probeinterface file {pc.ProbeInterfaceFileName}: {ex.Message}", true);
                }
            }
            probeGroup = new NeuropixelsV2ProbeGroup();
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
            ImGuiControls.WriteString(gainCalBuf, configureNode.ProbeConfiguration.GainCalibrationFileName ?? "");
            ImGuiControls.WriteString(probeFileBuf, configureNode.ProbeConfiguration.ProbeInterfaceFileName  ?? "");
        }

        #endregion
    }
}
