using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Hexa.NET.ImGui;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1.Design
{
    // Core of the NeuropixelsV2e ImGui configuration dialog: state, lifecycle, the
    // props-panel layout, and shared channel bookkeeping. The remaining concerns are
    // split across sibling partial files:
    //   .Channels.cs — probe type, channel config UI, contact info, enable/pin actions
    //   .Files.cs — gain cal + probe interface file I/O and close prompt
    //   .Survey.cs — electrode survey UI, log, and activity coloring
    internal partial class NeuropixelsV2eImGuiDialog : ImGuiProbeDialog
    {
        // Identity
        readonly string probeName;
        readonly bool isBeta;
        readonly bool isProbeA; // TODO: This is a hack. Its headstage level information that this probe level dialog should not care about

        // Config node
        readonly IConfigureNeuropixelsV2 configureNode;

        // Probe state
        NeuropixelsV2ProbeGroup probeGroup;
        IReadOnlyList<Contact> allContacts = Array.Empty<Contact>(); // NB: convenience reference to currently loaded probeGroup.Probe.Contacts

        // Channel / pin bookkeeping
        readonly MultiplexedContactState channelState = new();

        // Change tracking
        bool hasChanges;
        EventHandler onStateChange;

        // Survey
        readonly NeuropixelsV2eSurveyState survey = new();
        CancellationTokenSource cts;
        float spikeThreshold = -50f;
        bool useBandpassFilter = true;
        float timePerBank = 5f;
        SurveyActivityMetric selectedMetric = SurveyActivityMetric.SNR;
        float actMin, actMax = 1f;
        bool actRangeInitialized;
        readonly Dictionary<SurveyActivityMetric, (float min, float max)> actRanges = new();
        bool recordSurveyData = false;
        bool showActivityColors = false;
        bool bankSelectMode = false;
        HashSet<(int shank, NeuropixelsV2Bank bank)> surveyBanks  = new();
        HashSet<(int shank, NeuropixelsV2Bank bank)> pendingBanks = new();

        // Hardware (survey only) // TODO: lift to a headstage level dialog that houses this dialog
        string driver = "riffa";
        int hubIndex;
        PortName port = PortName.PortA;
        double? portVoltage;
        bool editingHardwareAddr;

        // Combo / preset state
        int presetIdx, refIdx, probeTypeIdx;
        Enum[] presets = Array.Empty<Enum>();
        string[] refNames = Array.Empty<string>();

        // Input buffers
        readonly byte[] driverBuf = new byte[64];
        readonly byte[] gainCalBuf = new byte[512];
        readonly byte[] probeFileBuf = new byte[512];

        // Visual constants
        const uint ColorContactEnabled = ImGuiPalette.GoldenPollen;
        const uint ColorContactPinned = ImGuiPalette.SteelBlue;
        const uint ColorContactDisabled = ImGuiPalette.Grey0x4D;

        static readonly Vector4 ColorTextWarning = ImGui.ColorConvertU32ToFloat4(ImGuiPalette.Marigold);
        static readonly Vector4 ColorTextSuccess = ImGui.ColorConvertU32ToFloat4(ImGuiPalette.MintGreen);

        /// <inheritdoc/>
        public override event EventHandler OnStateChange
        {
            add => onStateChange += value;
            remove => onStateChange -= value;
        }

        /// <summary>
        /// The configuration node updated by this dialog.
        /// </summary>
        internal IConfigureNeuropixelsV2 ConfigureNeuropixelsV2 => configureNode;

        /// <inheritdoc/>
        public override bool HasChanges
        {
            get => hasChanges;
            protected set
            {
                if (hasChanges == value)
                    return;
                hasChanges = value;
                onStateChange?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Opens the dialog for probe A on port A.
        /// </summary>
        public NeuropixelsV2eImGuiDialog(IConfigureNeuropixelsV2 configureNode, string probeName, bool isProbeA = true)
            : this(configureNode, probeName, PortName.PortA, isProbeA) { }

        /// <summary>
        /// Opens the dialog with an explicit port assignment.
        /// </summary>
        public NeuropixelsV2eImGuiDialog(IConfigureNeuropixelsV2 configureNode, string probeName, PortName port, bool isProbeA)
        {
            // NB: start the view at a 2 mm vertical box that extends 300 um below contact 0
            selector.DefaultZoomWindowMicrons = 2000f;
            selector.DefaultScrollYMicrons = 700f;

            this.configureNode = configureNode ?? throw new ArgumentNullException(nameof(configureNode));
            this.probeName = probeName;
            this.isProbeA = isProbeA;
            this.port = port;
            isBeta = configureNode is ConfigureNeuropixelsV2BetaPsbDecoder;

            Text = (isBeta ? "NeuropixelsV2eBeta" : "NeuropixelsV2e") + " Configuration";
            if (!string.IsNullOrEmpty(probeName)) Text += $" — {probeName}";

            SetupSelectorCallbacks();
            LoadOrCreateProbeGroup();
            RefreshProbeState();
        }

        #region Selector callback wiring

        static uint BankColor(NeuropixelsV2Bank bank) => bank switch
        {
            NeuropixelsV2Bank.A => ImGuiPalette.DustyGrape,
            NeuropixelsV2Bank.B => ImGuiPalette.WithAlpha(ImGuiPalette.VibrantCoral, 0x88),
            NeuropixelsV2Bank.C => ImGuiPalette.DustyGrape,
            NeuropixelsV2Bank.D => ImGuiPalette.WithAlpha(ImGuiPalette.VibrantCoral, 0x88),
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
                if (channelState.ElectrodeToChannel.TryGetValue(idx, out int ch) && channelState.PinnedChannels.Contains(ch)) return ColorContactPinned;
                return channelState.EnabledContactIndices.Contains(idx) ? ColorContactEnabled : ColorContactDisabled;
            };
            selector.IsBlocked = idx => channelState.BlockedContactIndices.Contains(idx);
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

        #region ImGuiProbeDialog overrides

        protected override bool SelectionEnabled => true;

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
            DrawProbeTypeSection();
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

        void HandleKeyboardShortcuts()
        {
            bool ctrl = ImGui.IsKeyDown(ImGuiKey.LeftCtrl)  || ImGui.IsKeyDown(ImGuiKey.RightCtrl);
            bool shift = ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift);

            if (ctrl && ImGui.IsKeyPressed(ImGuiKey.S, false))
            {
                bool hasFile = !string.IsNullOrEmpty(configureNode.ProbeConfiguration.ProbeInterfaceFileName);
                if (hasFile) SaveProbeGroup();
                else         SaveProbeGroupAs();
                return;
            }

            if (ImGui.GetIO().WantTextInput) return;
            if (ImGui.IsKeyPressed(ImGuiKey.E, false))           EnableSelectedContacts();
            if (ImGui.IsKeyPressed(ImGuiKey.P, false) && !shift) EnableAndPinSelectedContacts();
            if (ImGui.IsKeyPressed(ImGuiKey.P, false) &&  shift) UnpinSelectedContacts();
            if (ImGui.IsKeyPressed(ImGuiKey.A, false))           showActivityColors = !showActivityColors;
            if (ImGui.IsKeyPressed(ImGuiKey.B, false))
            {
                bankSelectMode = !bankSelectMode;
                selector.ClearSelection();
            }
        }

        protected override void OnClosing(FormClosingEventArgs e)
        {
            cts?.Cancel();
            if (hasChanges && DialogResult != DialogResult.Cancel)
                if (!PromptSaveOnClose())
                    e.Cancel = true;
        }

        #endregion

        #region Probe group init

        void LoadOrCreateProbeGroup()
        {
            var pc = configureNode.ProbeConfiguration;
            probeTypeIdx = Array.FindIndex(NeuropixelsV2ProbeTypes.All, e => e.MatchesProbeConfiguration(pc));
            if (probeTypeIdx < 0) probeTypeIdx = 0;

            var entry = NeuropixelsV2ProbeTypes.All[probeTypeIdx];

            if (!string.IsNullOrEmpty(pc.ProbeInterfaceFileName))
            {
                try
                {
                    probeGroup = entry.DeserializeGroup(File.ReadAllText(pc.ProbeInterfaceFileName));
                    Log($"Loaded probeinterface file {pc.ProbeInterfaceFileName}");
                    return;
                }
                catch (Exception ex)
                {
                    Log($"Error loading probeinterface file {pc.ProbeInterfaceFileName}: {ex.Message}", true);
                }
            }
            probeGroup = entry.CreateGroup();
        }

        void RefreshProbeState()
        {
            allContacts = probeGroup.Probe.Contacts;

            presets = probeGroup.GetChannelPresets().Cast<Enum>().ToArray();
            presetIdx = Array.FindIndex(presets, p => p.ToString() == "None");
            refNames = probeGroup.GetReferenceEnumValues().Cast<object>().Select(r => r.ToString()).ToArray();
            refIdx = Math.Max(0, Array.IndexOf(refNames, configureNode.ProbeConfiguration.Reference?.ToString() ?? ""));

            var contacts = allContacts
                .Select((c, i) => new ProbeContact(i, new Vector2((float)c.PosX, (float)c.PosY), ContactSizeUm(c)))
                .ToList();
            selector.Refresh(probeGroup, contacts);

            RebuildMaps();
            BuildPotentialChannelMap();
            ClearPins();
            RestorePinnedState();
            RestoreActivityData();
            InitSurveyBanks();
            WriteBackBufs();
        }

        void InitSurveyBanks()
        {
            if (surveyBanks.Count > 0) return;
            for (int i = 0; i < allContacts.Count; i++)
                surveyBanks.Add((probeGroup.GetShank(i), probeGroup.GetBank(i)));
        }

        #endregion

        #region Channel bookkeeping (wrappers over MultiplexedContactState)

        void RebuildMaps() => channelState.RebuildActiveMap(probeGroup.ChannelMap);

        void BuildPotentialChannelMap() => channelState.RebuildPotentialMap(allContacts.Count, probeGroup);

        void RecomputeBlockedIndices() => channelState.RecomputeBlocked(allContacts.Count);

        void ClearPins() => channelState.ClearPins();

        #endregion

        #region Misc helpers

        void DrawTitleBar()
        {
            ImGui.TextDisabled(probeName);
        }

        static void InfoRow(string label, string value)
        {
            ImGui.TextDisabled(label + ":");
            ImGui.SameLine();
            ImGui.TextUnformatted(value);
        }

        // Item ID + timestamp of the most recent click/drag/edit to finish. ImGui's own hover-delay
        // timer accumulates for as long as the mouse sits on an item, click or no click, so a
        // control that was already hovered-with-delay before being clicked stays "primed" through
        // the click and shows its tooltip instantly on release. Tracking this ourselves lets us
        // force the normal delay to re-elapse after an interaction, regardless of ImGui's own timer.
        static uint lastDeactivatedItemId;
        static double lastDeactivatedTime = -1.0;

        static void Tooltip(string text, string disabledHint = null)
        {
            if (ImGui.IsItemDeactivated())
            {
                lastDeactivatedItemId = ImGui.GetItemID();
                lastDeactivatedTime = ImGui.GetTime();
            }

            if (ImGui.IsItemActive())
                return; // don't show tooltips while the user is interacting (e.g. dragging a slider)

            if (ImGui.GetItemID() == lastDeactivatedItemId &&
                ImGui.GetTime() - lastDeactivatedTime < ImGui.GetStyle().HoverDelayNormal)
                return; // just interacted with this control; wait out the normal delay again

            if (!ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal | ImGuiHoveredFlags.AllowWhenDisabled))
                return; // only show when hovering for a delay and allow for disabled components as well
            bool disabled = ((ImGuiItemFlagsPrivate)ImGuiP.GetItemFlags() & ImGuiItemFlagsPrivate.Disabled) != 0;
            ImGui.SetTooltip(disabled && disabledHint != null ? $"{text}\n({disabledHint})" : text);
        }

        static Vector2 ContactSizeUm(Contact c)
        {
            var sp = c.ShapeParams;
            return c.Shape switch
            {
                ContactShape.Circle => new Vector2((float)(sp.Radius ?? 6.0) * 2f, (float)(sp.Radius ?? 6.0) * 2f),
                ContactShape.Rect   => new Vector2((float)(sp.Width ?? 12.0), (float)(sp.Height ?? sp.Width ?? 12.0)),
                _                   => new Vector2((float)(sp.Width ?? 12.0), (float)(sp.Width ?? 12.0))
            };
        }

        void WriteBackBufs()
        {
            WriteString(driverBuf, driver);
            WriteString(gainCalBuf, configureNode.ProbeConfiguration.GainCalibrationFileName ?? "");
            WriteString(probeFileBuf, configureNode.ProbeConfiguration.ProbeInterfaceFileName  ?? "");
        }

        static float ComputeMax(float?[] values)
        {
            float max = 1f;
            foreach (var v in values) if (v.HasValue && v.Value > max) max = v.Value;
            return max;
        }

        static void WriteString(byte[] buf, string value)
        {
            Array.Clear(buf, 0, buf.Length);
            if (!string.IsNullOrEmpty(value))
            {
                var bytes = Encoding.UTF8.GetBytes(value);
                Array.Copy(bytes, buf, Math.Min(bytes.Length, buf.Length - 1));
            }
        }

        static string ReadBuffer(byte[] buf)
        {
            int len = Array.IndexOf(buf, (byte)0);
            return Encoding.UTF8.GetString(buf, 0, len < 0 ? buf.Length : len);
        }

        #endregion
    }
}
