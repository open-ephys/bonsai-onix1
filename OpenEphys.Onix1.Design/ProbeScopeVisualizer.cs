using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using Bonsai;
using Bonsai.Design;
using Hexa.NET.ImGui;
using OpenCV.Net;
using OpenEphys.EphysVisualization.Design;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Provides a type visualizer that shows a <see cref="ProbeScope{TFrame}"/>'s data as a probe schematic beside a
    /// live multi-channel waveform viewer.
    /// </summary>
    /// <typeparam name="TFrame">The data frame type the scope displays.</typeparam>
    public class ProbeScopeVisualizer<TFrame> : WaveformScopeVisualizerBase
    {
        const float DefaultProbePaneWidth = 260f;
        const float MinProbePaneWidth = 120f;
        const float MinPlotWidth = 200f;
        const float DefaultEventPaneWidth = 240f;
        const float MinEventPaneWidth = 120f;

        // The largest waveform amplitude kept, either side of zero, in microvolts.
        const double EventSpan = 1000;

        // NB: the detector's cost is mostly per block, not per sample, and a device can send blocks of a few samples,
        // so the shown signal is gathered into blocks this long, in seconds, before detection.
        const double DetectionBlockSeconds = 0.005;

        static readonly uint ColorContactEnabled = ImGuiPalette.AmberGold;
        static readonly uint ColorContactSelected = ImGuiPalette.White;
        static readonly uint ColorContactHovered = 0xFFC2EFFFu;

        static readonly uint ColorRateSelection = 0xFFFFFF00u;

        static readonly ImGuiKey[] GroupKeys = Enumerable.Range(1, 9).Select(n => ImGuiKey.Key0 + n).ToArray();

        readonly ImGuiProbeSelector selector = new(zoomWindowFraction: 1f) { ShowGrid = false };
        Dictionary<int, int> groupOfChannel = new();
        ChannelSettings[] groupedFrom;
        Func<int, int> groupOf;
        int lastGroupKey = -1;
        double lastGroupKeyTime;
        Dictionary<int, int> contactOfChannel = new();
        int[] channelOfContact = Array.Empty<int>();
        int selectedContact = -1;
        int hoveredContact = -1;
        int revealedChannel = -1;
        bool probeHovered;
        ImGuiProbeScopeBandColumns<TFrame> bands;
        EventLoopScheduler scheduler;
        string deviceName;
        float probePaneWidth = DefaultProbePaneWidth;
        float eventPaneWidth = DefaultEventPaneWidth;
        bool rateColored;
        float rateLow;
        float rateHigh;
        ProbeScopeSource<TFrame> source;

        // NB: made on the band chain's thread when detection first starts, and drawn on the window's, where its GL
        // objects are made.
        volatile ImGuiEventView events;
        BehaviorSubject<CrossingSettings> crossingSettings;

        sealed record CrossingSettings(bool Show, double Threshold, double Window, double PreTrigger)
        {
            public CrossingSettings(ProbeScope<TFrame> node)
                : this(node.ShowThresholdCrossings, node.CrossingThreshold, node.CrossingWindow,
                    node.CrossingPreTrigger)
            {
            }
        }

        ProbeScope<TFrame> Node => (ProbeScope<TFrame>)Scope;

        /// <inheritdoc/>
        protected override long HistoryBytes => Node.HistoryBytes;

        /// <inheritdoc/>
        protected override string Unit => Node.Unit;

        /// <inheritdoc/>
        protected override double MaxRange => Node.MaxRange;

        /// <inheritdoc/>
        protected override double MinRange => Node.MinRange;

        /// <inheritdoc/>
        protected override void DrawOffscreen() => events?.Flush();

        /// <inheritdoc/>
        protected override IEnumerable<ImGuiKey> Hotkeys =>
            selector.Hotkeys.Concat(new[] { ImGuiKey.P, ImGuiKey.K, ImGuiKey.G, ImGuiKey.U }).Concat(GroupKeys);

        /// <inheritdoc/>
        protected override string FaultHint => "Close this window, correct the ProbeScope's DeviceName, and open it again.";

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            base.Load(provider);
            var context = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            var workflowBuilder = (WorkflowBuilder)provider.GetService(typeof(WorkflowBuilder));
            var node = Node;

            try { deviceName = UpstreamDevice.Resolve(workflowBuilder?.Workflow, context.Source, node.DeviceName, node.DeviceNameOf); }
            catch (InvalidOperationException ex) { Fail($"ProbeScope: {ex.Message}"); }

            bands = new ImGuiProbeScopeBandColumns<TFrame>(node, () => Paused);
            var standard = StandardColumns;
            var display = standard.Display;
            columns = new[]
            {
                bands.BandColumn, bands.FiltersColumn, bands.OrderColumn, standard.Timebase, standard.ChannelHeight,
                standard.Range, standard.Palette, new ControlStripColumn("Threshold (uV)", DrawCrossings, 160),
                new ControlStripColumn(display.Label, width => DrawDisplay(display, width), display.Width),
            };
            scheduler = new EventLoopScheduler();
            crossingSettings = new(new CrossingSettings(node));

            rateHigh = (float)node.EventRateScale;
            selector.FillColor = ContactColor;
            selector.OutlineColor = ContactOutline;
            selector.ShowCoordinateReadout = false;
            selector.ShowScrollBars = true;
            selector.Blocked = contact => ChannelOf(contact) < 0;
            selector.SelectionChanged += SelectContacts;
            groupOf = GroupOf;
        }

        uint ContactColor(int contact) =>
            rateColored ? RateColor(ChannelOf(contact))
            : contact == selectedContact ? ColorContactSelected
            : contact == hoveredContact ? ColorContactHovered
            : Node.ShowGroups ? GroupColor(GroupOf(ChannelOf(contact)))
            : ColorContactEnabled;

        uint ContactOutline(int contact) =>
            !rateColored ? 0
            : contact == selectedContact ? ColorContactSelected
            : contact == hoveredContact ? ColorContactHovered
            : 0;

        uint RateColor(int channel)
        {
            var rate = (float)(events?.EventRate(channel) ?? 0);
            var t = rateHigh > rateLow ? Math.Max(0f, Math.Min(1f, (rate - rateLow) / (rateHigh - rateLow))) : 0f;
            return Plasma.DefaultMap[(int)(t * 255f)];
        }

        void DrawContactColoring()
        {
            var node = Node;
            var shown = rateColored ? 2 : node.ShowGroups ? 1 : 0;
            if (ImGui.RadioButton("Default##contactColor", ref shown, 0))
            {
                node.ColorContactsByEventRate = false;
                node.ShowGroups = false;
            }
            ImGuiWidgets.Tooltip("Color the contacts amber, and the traces in the palette's colors.");
            ImGui.SameLine();
            if (ImGui.RadioButton("Groups##contactColor", ref shown, 1))
            {
                node.ColorContactsByEventRate = false;
                node.ShowGroups = true;
            }
            ImGuiWidgets.Tooltip("Color each contact and its trace by the channel's group, and channels in no group " +
                "grey.\nKey: G");
            ImGui.SameLine();
            if (ImGui.RadioButton("Rate##contactColor", ref shown, 2))
            {
                node.ColorContactsByEventRate = true;
                node.ShowThresholdCrossings = true;
            }
            ImGuiWidgets.Tooltip("Color each contact by its channel's event rate, on the scale below, detecting " +
                "threshold crossings if they are not already.\nKey: U turns detection on and off");
        }

        void DrawContactInfo()
        {
            var contact = selector.HoveredContactIndex;
            if (contact < 0)
                contact = SoleSelectedContact();
            if (contact < 0)
                contact = selectedContact;

            var first = new List<string>();
            var second = new List<string>();
            var units = selector.CoordinateUnits;
            if (source?.ProbeGroup?.Probe?.Contacts is { } contacts && (uint)contact < (uint)contacts.Count)
            {
                var c = contacts[contact];
                var channel = ChannelOf(contact);
                first.Add(channel >= 0 ? $"Ch {channel}" : "Not recorded");
                first.Add($"Contact {(string.IsNullOrEmpty(c.ContactId) ? contact.ToString() : c.ContactId)}");
                if (!string.IsNullOrEmpty(c.ShankId))
                    first.Add($"Shank {c.ShankId}");
                second.Add($"({c.PosX:0.#}, {c.PosY:0.#}) {units}");
                if (channel >= 0 && GroupOf(channel) is var group and > 0)
                    second.Add($"Group {group}");
                if (channel >= 0 && Node.ShowThresholdCrossings && events is { } view)
                    second.Add($"{TwoFigures(view.EventRate(channel))} Hz");
            }
            else if (selector.PointerPosition is { } point)
                first.Add($"({point.X:0.#}, {point.Y:0.#}) {units}");

            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiPalette.Grey0x99);
            ImGui.Text(string.Join("   ", first));
            ImGui.Text(string.Join("   ", second));
            ImGui.PopStyleColor();
        }

        int SoleSelectedContact()
        {
            var sole = -1;
            for (int i = 0; i < selector.Contacts.Count; i++)
            {
                if (!selector.Contacts[i].Selected)
                    continue;
                if (sole >= 0)
                    return -1;
                sole = i;
            }
            return sole;
        }

        static string TwoFigures(double value)
        {
            if (value < 0.001)
                return "0";
            var decimals = Math.Max(0, 1 - (int)Math.Floor(Math.Log10(value)));
            return Math.Round(value, Math.Min(decimals, 4)).ToString(System.Globalization.CultureInfo.CurrentCulture);
        }

        void DrawColorKey()
        {
            var style = ImGui.GetStyle();
            var height = ImGui.GetFrameHeight() + 2 * style.ItemSpacing.Y + ImGui.GetTextLineHeight();
            ImGui.BeginChild("##colorKey", new Vector2(0, height), ImGuiChildFlags.None,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
            if (rateColored)
                DrawRateScale();
            else
                DrawGroupSwatches();
            ImGui.EndChild();
        }

        void DrawRateScale()
        {
            var node = Node;
            var style = ImGui.GetStyle();

            // NB: half an item spacing down, so the handles, which stand above the bar, are not clipped by the row.
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + style.ItemSpacing.Y / 2);
            var fieldWidth = ImGui.CalcTextSize("10000").X + 2 * style.FramePadding.X;
            var scale = (float)node.EventRateScale;
            ImGuiColormapRangeSlider.Draw("##rateScale", Plasma.DefaultMap, 0, scale, ref rateLow, ref rateHigh,
                ImGui.GetContentRegionAvail().X - fieldWidth - style.ItemInnerSpacing.X, valueFormat: "0.#",
                minHandleTooltip: "The event rate, in Hz, at or below which a contact is the scale's darkest.",
                maxHandleTooltip: "The event rate, in Hz, at or above which a contact is the scale's brightest.");

            ImGui.SameLine(0, style.ItemInnerSpacing.X);
            var top = node.EventRateScale;
            ImGui.SetNextItemWidth(fieldWidth);
            ImGui.InputDouble("##rateScaleTop", ref top, "%g");
            if (ImGui.IsItemDeactivatedAfterEdit() && top > 0)
            {
                if (rateHigh >= scale)
                    rateHigh = (float)top;
                node.EventRateScale = top;
            }
            ImGuiWidgets.Tooltip("The event rate, in Hz, at the right end of the scale, applied once you leave " +
                "the field.");
        }

        int GroupOf(int channel) => Groups().TryGetValue(channel, out var group) ? group : 0;

        Dictionary<int, int> Groups()
        {
            var channels = Node.Channels;
            if (!ReferenceEquals(groupedFrom, channels))
            {
                groupedFrom = channels;
                groupOfChannel = new();
                foreach (var entry in channels ?? Array.Empty<ChannelSettings>())
                {
                    if (entry.Group > 0)
                        groupOfChannel[entry.Index] = entry.Group;
                }
            }

            return groupOfChannel;
        }

        void AssignGroup(int[] channels, int group, bool replace) =>
            Node.Channels = ChannelSettings.WithGroup(Node.Channels, channels, group, replace);

        int[] GroupChannels(int group) => Groups().Where(entry => entry.Value == group).Select(entry => entry.Key).ToArray();

        void SelectGroup(int group)
        {
            var channels = GroupChannels(group);
            if (channels.Length > 0)
                selector.Select(channels.Select(ContactOf).Where(contact => contact >= 0));
        }

        void FitGroup(int group)
        {
            var channels = GroupChannels(group);
            if (channels.Length > 0)
                ZoomTo(channels);
        }

        void GroupSelection(int group, bool replace)
        {
            var channels = SelectedContactChannels();
            if (channels.Length == 0)
                return;
            AssignGroup(channels, group, replace);
            Node.ShowGroups = true;
        }

        void DrawGroupSwatches()
        {
            var spacing = ImGui.GetStyle().ItemInnerSpacing.X;
            var side = Math.Min(ImGui.GetFrameHeight(),
                MathF.Floor((ImGui.GetContentRegionAvail().X - spacing * (GroupKeys.Length - 1)) / GroupKeys.Length));
            var draw = ImGui.GetWindowDrawList();
            var io = ImGui.GetIO();

            for (int n = 1; n <= GroupKeys.Length; n++)
            {
                if (n > 1)
                    ImGui.SameLine(0, spacing);
                var min = ImGui.GetCursorScreenPos();
                ImGui.InvisibleButton($"##group{n}", new Vector2(side));
                var hovered = ImGui.IsItemHovered();
                var count = GroupChannels(n).Length;

                if (hovered && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && !io.KeyCtrl && !io.KeyShift)
                    FitGroup(n);
                else if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    if (io.KeyCtrl != io.KeyShift) GroupSelection(n, replace: io.KeyCtrl);
                    else if (!io.KeyCtrl) SelectGroup(n);
                }

                var color = GroupColor(n);
                draw.AddRectFilled(min, min + new Vector2(side), count > 0 ? color : ImGuiPalette.WithAlpha(color, 0x40),
                    ImGui.GetStyle().FrameRounding);
                if (hovered)
                    draw.AddRect(min, min + new Vector2(side), ImGuiPalette.White, ImGui.GetStyle().FrameRounding);
                var label = n.ToString();
                var text = ImGui.CalcTextSize(label);
                draw.AddText(min + (new Vector2(side) - text) / 2,
                    count == 0 ? ImGuiPalette.WithAlpha(ImGuiPalette.White, 0x80)
                    : Luminance(color) > 0.4f ? ImGuiPalette.Black : ImGuiPalette.White, label);

                ImGuiWidgets.Tooltip($"Group {n}: {(count > 0 ? $"{count} channels" : "empty")}.\nClick to select it on " +
                    "the probe, double-click to fit it to the plot, Shift+click to add the probe's selection to it, and " +
                    $"Ctrl+click to make the probe's selection the group.\nKeys: {n}, {n} twice, Shift+{n}, Ctrl+{n}");
            }
        }

        // NB: relative luminance of a packed 0xAABBGGRR color, without linearizing, which is close enough to choose
        // black or white text on it.
        static float Luminance(uint color) =>
            (0.2126f * (color & 0xFF) + 0.7152f * (color >> 8 & 0xFF) + 0.0722f * (color >> 16 & 0xFF)) / 255f;

        void DrawCrossings(float width)
        {
            var node = Node;
            var show = node.ShowThresholdCrossings;
            if (ImGui.Checkbox("##showCrossings", ref show))
                node.ShowThresholdCrossings = show;
            ImGuiWidgets.Tooltip("Detect threshold crossings on every channel, in the signal shown, and show an " +
                "expanded channel's waveforms beside it, each drawn over all those before it.\nKey: U");

            var style = ImGui.GetStyle();
            const string MoreLabel = "...";
            const string ClearLabel = "Clear";
            var moreWidth = ImGui.CalcTextSize(MoreLabel).X + 2 * style.FramePadding.X;
            var clearWidth = ImGui.CalcTextSize(ClearLabel).X + 2 * style.FramePadding.X;
            ImGui.SameLine(0, style.ItemInnerSpacing.X);
            var threshold = node.CrossingThreshold;
            ImGui.SetNextItemWidth(Math.Max(1,
                width - ImGui.GetFrameHeight() - moreWidth - clearWidth - 3 * style.ItemInnerSpacing.X));
            ImGui.InputDouble("##crossingThreshold", ref threshold, "%.0f");
            if (ImGui.IsItemDeactivatedAfterEdit())
                node.CrossingThreshold = threshold;
            ImGuiWidgets.Tooltip("The threshold whose crossing going down is detected on every channel. A new one " +
                "starts the events over, applied once you leave the field.");

            const string SettingsPopup = "##crossingSettings";
            ImGui.SameLine(0, style.ItemInnerSpacing.X);
            if (ImGui.Button(MoreLabel))
                ImGui.OpenPopup(SettingsPopup);
            ImGuiWidgets.Tooltip($"{node.CrossingWindow:g} ms window, {node.CrossingPreTrigger:g} ms pre-trigger, " +
                $"{node.CrossingRateTimeConstant:g} s rate time constant. Click to change them.");

            ImGui.SameLine(0, style.ItemInnerSpacing.X);
            ImGui.BeginDisabled(!node.ShowThresholdCrossings || events is null);
            if (ImGui.Button(ClearLabel))
                events?.Clear();
            ImGui.EndDisabled();
            ImGuiWidgets.Tooltip("Start every channel's events over: their waveforms, counts and rates.",
                "Detect threshold crossings first.\nKey: U");

            if (ImGui.BeginPopup(SettingsPopup))
            {
                const string WindowLabel = "Window (ms)";
                const string PreTriggerLabel = "Pre-trigger (ms)";
                const string RateLabel = "Rate time constant (s)";
                // NB: SameLine's offset counts from the popup's edge, not from inside its padding.
                var fieldX = style.WindowPadding.X + style.ItemSpacing.X + Math.Max(ImGui.CalcTextSize(RateLabel).X,
                    Math.Max(ImGui.CalcTextSize(WindowLabel).X, ImGui.CalcTextSize(PreTriggerLabel).X));
                var fieldWidth = ImGui.CalcTextSize("00.000").X + 2 * style.FramePadding.X;

                ImGui.AlignTextToFramePadding();
                ImGui.Text(WindowLabel);
                ImGui.SameLine(fieldX);
                var window = node.CrossingWindow;
                ImGui.SetNextItemWidth(fieldWidth);
                ImGui.InputDouble("##crossingWindow", ref window, "%g");
                if (ImGui.IsItemDeactivatedAfterEdit() && window > 0)
                {
                    node.CrossingWindow = window;
                    node.CrossingPreTrigger = Math.Min(node.CrossingPreTrigger, window);
                }
                ImGuiWidgets.Tooltip("The duration of the waveform kept at each crossing. A new one starts the " +
                    "events over, applied once you leave the field.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text(PreTriggerLabel);
                ImGui.SameLine(fieldX);
                var preTrigger = node.CrossingPreTrigger;
                ImGui.SetNextItemWidth(fieldWidth);
                ImGui.InputDouble("##crossingPreTrigger", ref preTrigger, "%g");
                if (ImGui.IsItemDeactivatedAfterEdit())
                    node.CrossingPreTrigger = Math.Max(0, Math.Min(preTrigger, node.CrossingWindow));
                ImGuiWidgets.Tooltip("The time in each crossing's waveform before its peak, or before the " +
                    "crossing itself where the peak comes first. A new one starts the events over, applied once you " +
                    "leave the field.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text(RateLabel);
                ImGui.SameLine(fieldX);
                var rateTimeConstant = node.CrossingRateTimeConstant;
                ImGui.SetNextItemWidth(fieldWidth);
                ImGui.InputDouble("##crossingRateTimeConstant", ref rateTimeConstant, "%g");
                if (ImGui.IsItemDeactivatedAfterEdit() && rateTimeConstant > 0)
                    node.CrossingRateTimeConstant = rateTimeConstant;
                ImGuiWidgets.Tooltip("The time over which each channel's event rate is averaged. A longer one " +
                    "steadies the rate, and a shorter one follows a change sooner. It applies without starting the " +
                    "events over, once you leave the field.");

                ImGui.EndPopup();
            }
        }

        void DrawEventPane(float width, float height)
        {
            ImGui.BeginChild("##eventPane", new Vector2(width, height),
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            var row = ExpandedRow;
            var channel = row?.Channel ?? -1;
            ImGui.AlignTextToFramePadding();
            ImGui.Text(channel >= 0 ? $"Ch {channel}" : string.Empty);
            const string ClearLabel = "Clear";
            var clearWidth = ImGui.CalcTextSize(ClearLabel).X + ImGui.GetStyle().FramePadding.X * 2;
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, ImGui.GetContentRegionAvail().X - clearWidth));
            if (ImGui.SmallButton(ClearLabel))
                events?.Clear();
            ImGuiWidgets.Tooltip("Start every channel's events over.");

            var view = events;
            if (view is null)
                ImGui.TextDisabled("Waiting for the probe.");
            else if (row is not { } expanded)
                view.Flush();
            else
            {
                ImGui.SetCursorScreenPos(new Vector2(ImGui.GetCursorScreenPos().X, expanded.Top));
                var size = new Vector2(ImGui.GetContentRegionAvail().X, expanded.Height);
                var (center, range) = Amplitude;
                var threshold = Node.CrossingThreshold;
                if (view.Draw(channel, size, ChannelColor(channel), center, range, threshold) is double dropped)
                    Node.CrossingThreshold = Math.Round(dropped);
            }

            ImGui.EndChild();
        }

        // NB: beside Clipping in the standard Display column, which leans on two things its API does not promise:
        // that Clipping is the last item it draws, and the shortened frame padding it draws its two lines of
        // checkboxes with and pops before returning, pushed again here so Groups sits level with Clipping.
        void DrawDisplay(ControlStripColumn display, float width)
        {
            display.Draw(width);
            ImGui.SameLine();

            var padding = ImGui.GetStyle().FramePadding;
            var line = (ImGui.GetTextLineHeight() + ImGui.GetFrameHeight()) / 2;
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,
                new Vector2(padding.X, Math.Max(0, (line - ImGui.GetFontSize()) / 2)));
            var node = Node;
            var show = node.ShowGroups;
            if (ImGui.Checkbox("Groups", ref show))
                node.ShowGroups = show;
            ImGuiWidgets.Tooltip("Draw each channel in its group's color, on the traces and the probe, and channels in " +
                "no group in grey.\nKey: G\n1 to 9 selects a group on the probe, and pressing it twice fits it to the " +
                "plot. Shift+1 to 9 adds the probe's selected contacts to a group, Ctrl+1 to 9 makes them the group, " +
                "and Delete over the probe takes them out of their groups.");
            ImGui.PopStyleVar();
        }

        // NB: the window's left padding, where the handle sits, lies partly outside the window's clip rect, so the
        // handle sets its own.
        void DrawProbeHandle(float height)
        {
            const float BarWidth = 4;
            var cursor = ImGui.GetCursorScreenPos();
            var style = ImGui.GetStyle();
            var min = new Vector2(ImGui.GetWindowPos().X, cursor.Y);
            var max = new Vector2(cursor.X, cursor.Y + height - style.ScrollbarSize - style.ItemSpacing.Y / 2);

            ImGui.PushClipRect(min, max, false);
            ImGui.SetCursorScreenPos(min);
            if (ImGui.InvisibleButton("##showProbe", max - min))
                Node.ShowProbe = true;
            var color = ImGui.GetColorU32(ImGui.IsItemHovered() || ImGui.IsItemActive()
                ? ImGuiCol.SeparatorHovered
                : ImGuiCol.ScrollbarGrab);
            ImGuiWidgets.Tooltip("Show the probe.\nKey: P");

            var draw = ImGui.GetWindowDrawList();
            var top = min.Y + ImGui.GetFrameHeight();
            var x = MathF.Floor((min.X + max.X - BarWidth) / 2);
            draw.AddRectFilled(new Vector2(x, top), new Vector2(x + BarWidth, max.Y), color);
            ImGui.PopClipRect();

            // NB: the chevron's left side on the bar's. ImGui centers its arrow in a box the font's size, with its
            // back 0.3 of that left of center, and it reaches past the padding into the empty corner above the
            // plot's labels.
            var arrow = ImGui.GetFontSize();
            ImGui.PushClipRect(min, new Vector2(x + arrow, top), false);
            ImGuiP.RenderArrow(draw, new Vector2(x - 0.2f * arrow, MathF.Floor((min.Y + top - arrow) / 2)),
                color, ImGuiDir.Right, 1);
            ImGui.PopClipRect();
            ImGui.SetCursorScreenPos(cursor);
        }

        void AnswerGroupKeys()
        {
            var io = ImGui.GetIO();
            if (io.WantTextInput)
                return;

            for (int n = 1; n <= GroupKeys.Length; n++)
            {
                if (!ImGui.IsKeyPressed(GroupKeys[n - 1], false))
                    continue;

                if (io.KeyCtrl != io.KeyShift)
                    GroupSelection(n, replace: io.KeyCtrl);
                else if (!io.KeyCtrl)
                {
                    var again = n == lastGroupKey && ImGui.GetTime() - lastGroupKeyTime < io.MouseDoubleClickTime;
                    SelectGroup(n);
                    if (again)
                        FitGroup(n);
                    lastGroupKey = n;
                    lastGroupKeyTime = ImGui.GetTime();
                }
            }
        }

        int ChannelOf(int contact) => (uint)contact < (uint)channelOfContact.Length ? channelOfContact[contact] : -1;

        int ContactOf(int channel) => contactOfChannel.TryGetValue(channel, out var contact) ? contact : -1;

        void MapContacts(SingleProbeGroup probe)
        {
            contactOfChannel = probe?.ChannelMap?.ToDictionary(entry => entry.Key, entry => entry.Value) ?? new();
            var contacts = contactOfChannel.Count > 0 ? contactOfChannel.Values.Max() + 1 : 0;
            channelOfContact = Enumerable.Repeat(-1, contacts).ToArray();
            foreach (var entry in contactOfChannel)
                channelOfContact[entry.Value] = entry.Key;
        }

        void SelectContacts(object sender, EventArgs e)
        {
            var channels = SelectedContactChannels();
            if (channels.Length == 1)
            {
                SelectedChannel = channels[0];
                Reveal(channels[0]);
            }
            else if (channels.Length == 0)
                SelectedChannel = -1;
        }

        int[] SelectedContactChannels() => Enumerable.Range(0, selector.Contacts.Count)
            .Where(contact => selector.Contacts[contact].Selected)
            .Select(ChannelOf)
            .Where(channel => channel >= 0)
            .ToArray();

        ControlStripColumn[] columns = Array.Empty<ControlStripColumn>();

        /// <inheritdoc/>
        /// <remarks>
        /// What the band chain produces is handed to the viewer as it arrives rather than in batches on a timer, as
        /// <see cref="BufferedVisualizer"/> delivers them, which would feed the frames unevenly.
        /// </remarks>
        public override IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            // NB: the node is taken once here rather than in the chain, which runs on its own thread and may do so
            // after the window has unloaded.
            var node = Node;
            return Observable.Create<object>(_ =>
                BindBands(node, source.Merge().Cast<TFrame>()).Subscribe(Forward, ex => Fail(ex.Message)));
        }

        IObservable<object> BindBands(ProbeScope<TFrame> node, IObservable<TFrame> frames)
        {
            if (Faulted)
                return Observable.Never<object>();

            // NB: off the acquisition thread the frames arrive on, so the band filters never hold it up, and onto
            // one ordered thread, since the filters carry state across blocks.
            return frames
                .ObserveOn(scheduler)
                // NB: GetDevice throws until the configuration operator has registered, which
                // nothing orders before the visualizer subscribes. A frame arriving proves it has.
                .Publish(shared => shared
                    .Take(1)
                    .Select(_ => ResolveSource(node))
                    .Where(probe => probe is not null)
                    .SelectMany(probe => Observable.Return<object>(probe)
                        .Concat(bands.BandSelected
                            .Select(band => BandOutput(probe, band, shared))
                            .Switch()
                        )
                    )
                );
        }

        // NB: in the device's channel order, into the event view, which queues them for the window's thread.
        IObservable<object> DetectCrossings(ProbeScopeSource<TFrame> probe, IObservable<Mat> signal, int sampleRate,
            CrossingSettings settings) => Observable.Defer(() =>
        {
            if (probe.ProbeGroup?.ChannelMap is not { Count: > 0 } map)
                return Observable.Empty<object>();

            var (length, delay) = SpikeDetection.WaveformSamples(sampleRate, settings.Window / 1000, settings.PreTrigger / 1000);
            var view = events;
            if (view is null)
                events = view = new ImGuiEventView(map.Keys.Max() + 1, length, delay, sampleRate, EventSpan)
                {
                    Unit = Unit,
                };
            else
                view.Reshape(length, delay, sampleRate);

            var samples = new float[length];
            var blockLength = Math.Max(length, (int)Math.Round(DetectionBlockSeconds * sampleRate));
            var blocks = new Bonsai.Dsp.Buffer { Count = blockLength }.Process(signal);
            return SpikeDetection.Detect(blocks, length, delay, new[] { settings.Threshold },
                    Bonsai.Dsp.SpikeWaveformRefinement.AlignPeaks)
                .Do(collection =>
                {
                    foreach (var crossing in collection)
                    {
                        Marshal.Copy(crossing.Waveform.Data, samples, 0, samples.Length);
                        view.Add(crossing.ChannelIndex, samples);
                    }
                })
                .IgnoreElements()
                .Select(_ => (object)null);
        });

        ProbeScopeSource<TFrame> ResolveSource(ProbeScope<TFrame> node)
        {
            try
            {
                DeviceInfo deviceInfo = null;
                using (DeviceManager.GetDevice(deviceName).Subscribe(info => deviceInfo = info)) { }
                return node.CreateSource(deviceInfo);
            }
            catch (Exception ex)
            {
                Fail(ex.Message);
                return null;
            }
        }

        IObservable<object> BandOutput(ProbeScopeSource<TFrame> probe, BandSelection selection, IObservable<TFrame> frames)
        {
            var band = probe.Bands[selection.Band];
            var order = selection.Order == ChannelOrder.Channel ? null
                : DepthOrder(probe.ProbeGroup, placed: selection.Order == ChannelOrder.Physical, selection.LanePerShank);
            var rowChannels = order?.Channels;
            var counts = frames.Select(band.Source);
            var clipping = counts
                .Select(data => ClippedRows(data, probe.AdcMaximum))
                .Where(channels => channels is not null)
                .Select(channels => (object)new ProbeScopeClipping(Reorder(channels, rowChannels)));

            var output = selection.AcCoupled && band.AcTransform is not null ? band.AcTransform(counts) : band.Transform(counts);
            if (selection.CommonMedianReference && probe.Reference is not null)
                output = probe.Reference(output);

            // NB: reordered last, since referencing groups the channels by their converter in the device's order.
            return output.Publish(signal => Observable.Return<object>(new ProbeScopeOrder(order))
                .Concat(signal
                    .Select(data => (object)new ProbeScopeSample(Reorder(data, rowChannels), band.SampleRate))
                    .Merge(clipping))
                .Merge(crossingSettings
                    .Select(settings => settings.Show
                        ? DetectCrossings(probe, signal, band.SampleRate, settings)
                        : Observable.Empty<object>())
                    .Switch()));
        }

        /// <summary>
        /// The channels of <paramref name="probe"/>'s channel map from the shallowest electrode to the deepest, and
        /// across the probe where electrodes share a depth, or each shank in a lane of its own if
        /// <paramref name="lanePerShank"/>, placed at their depths if <paramref name="placed"/>, or null where it maps
        /// no channels.
        /// </summary>
        static RowOrder DepthOrder(SingleProbeGroup probe, bool placed, bool lanePerShank)
        {
            var contacts = probe?.Probe?.Contacts;
            if (probe?.ChannelMap is not { Count: > 0 } map || contacts is null)
                return null;

            var shanks = ShanksOf(probe);
            var laned = lanePerShank && shanks.Count > 1;
            int ShankOf(KeyValuePair<int, int> entry) =>
                laned ? shanks.IndexOf(contacts[entry.Value].ShankId ?? string.Empty) : 0;

            var sorted = map
                .OrderBy(ShankOf)
                .ThenByDescending(entry => contacts[entry.Value].PosY)
                .ThenBy(entry => contacts[entry.Value].PosX)
                .ToArray();

            // NB: down the plot is down the probe, toward the tip, where PosY falls.
            var depths = sorted.Select(entry => (float)-contacts[entry.Value].PosY).ToArray();
            var lanes = laned ? sorted.Select(ShankOf).ToArray() : null;
            return new RowOrder(
                sorted.Select(entry => entry.Key).ToArray(),
                placed ? SplitShared(depths, lanes) : lanes is null ? null : LevelRows(depths, lanes),
                lanes);
        }

        /// <summary>
        /// The depths of rows, with rows sharing a depth in a lane spread across the pitch below it in their order.
        /// </summary>
        /// <remarks>
        /// The pitch, the smallest step between depths, and the most rows sharing one are the probe's, so every lane
        /// keeps one scale.
        /// </remarks>
        /// <param name="depths">Each row's depth, never less than the one before it in its lane.</param>
        /// <param name="lanes">Each row's lane, a lane's rows together, or null for one lane.</param>
        static float[] SplitShared(float[] depths, int[] lanes)
        {
            bool Shares(int i) => i > 0 && (lanes is null || lanes[i] == lanes[i - 1]) && depths[i] == depths[i - 1];
            bool SameLane(int i) => i > 0 && (lanes is null || lanes[i] == lanes[i - 1]);

            var pitch = float.PositiveInfinity;
            var shared = 1;
            var rank = new int[depths.Length];
            for (int i = 0; i < depths.Length; i++)
            {
                rank[i] = Shares(i) ? rank[i - 1] + 1 : 0;
                shared = Math.Max(shared, rank[i] + 1);
                if (SameLane(i) && depths[i] > depths[i - 1])
                    pitch = Math.Min(pitch, depths[i] - depths[i - 1]);
            }

            if (shared == 1)
                return depths;
            if (float.IsPositiveInfinity(pitch))
                pitch = 1;

            return depths.Select((depth, i) => depth + rank[i] * pitch / shared).ToArray();
        }

        static List<string> ShanksOf(SingleProbeGroup probe) =>
            probe?.Probe?.Contacts is { } contacts && probe.ChannelMap is { } map
                ? map.Select(entry => contacts[entry.Value].ShankId ?? string.Empty)
                    .Distinct()
                    .OrderBy(shank => shank, StringComparer.Ordinal)
                    .ToList()
                : new List<string>();

        /// <summary>
        /// Positions, in rows, that space the lane with the most rows evenly and put every other lane's rows level
        /// with the rows of that lane at the same depth.
        /// </summary>
        /// <param name="depths">Each row's depth, never less than the one before it in its lane.</param>
        /// <param name="lanes">Each row's lane, a lane's rows together.</param>
        static float[] LevelRows(float[] depths, int[] lanes)
        {
            var reference = lanes.GroupBy(lane => lane).OrderByDescending(lane => lane.Count()).First().Key;
            var levels = new List<(float Depth, int Row)>();
            var row = 0;
            for (int i = 0; i < depths.Length; i++)
            {
                if (lanes[i] != reference)
                    continue;
                if (levels.Count == 0 || depths[i] > levels[levels.Count - 1].Depth)
                    levels.Add((depths[i], row));
                row++;
            }

            var span = levels[levels.Count - 1].Depth - levels[0].Depth;
            var rowsPerDepth = span > 0 ? (levels[levels.Count - 1].Row - levels[0].Row) / span : 0;
            float RowAt(float depth)
            {
                if (depth <= levels[0].Depth)
                    return levels[0].Row - (levels[0].Depth - depth) * rowsPerDepth;
                if (depth >= levels[levels.Count - 1].Depth)
                    return levels[levels.Count - 1].Row + (depth - levels[levels.Count - 1].Depth) * rowsPerDepth;

                var next = levels.FindIndex(level => level.Depth >= depth);
                var (above, below) = (levels[next - 1], levels[next]);
                return above.Row + (depth - above.Depth) / (below.Depth - above.Depth) * (below.Row - above.Row);
            }

            var positions = new float[depths.Length];
            for (int i = 0; i < depths.Length; i++)
            {
                var position = MathF.Round(RowAt(depths[i]));
                if (i > 0 && lanes[i] == lanes[i - 1])
                    position = Math.Max(position, positions[i - 1] + 1);
                positions[i] = position;
            }

            return positions;
        }

        static unsafe Mat Reorder(Mat data, int[] rowChannels)
        {
            if (rowChannels is null || data.Rows != rowChannels.Length)
                return data;

            // NB: data is not disposed, since crossing detection shares it and holds it past this block.
            var reordered = new Mat(data.Size, data.Depth, data.Channels);

            // NB: by pointer, since GetRow allocates a native header per row on every block.
            var rowBytes = reordered.Step;
            for (int row = 0; row < rowChannels.Length; row++)
            {
                var from = (byte*)data.Data + (long)rowChannels[row] * data.Step;
                var to = (byte*)reordered.Data + (long)row * reordered.Step;
                Buffer.MemoryCopy(from, to, rowBytes, rowBytes);
            }

            return reordered;
        }

        static bool[] Reorder(bool[] channels, int[] rowChannels) =>
            rowChannels is null || channels.Length != rowChannels.Length
                ? channels
                : rowChannels.Select(channel => channels[channel]).ToArray();

        /// <summary>
        /// The rows of <paramref name="raw"/> holding a sample at either end of the ADC's range, or null where none
        /// does.
        /// </summary>
        /// <remarks>
        /// On the raw counts, before scaling and any filter, which smear a clipped stretch away from the limits and
        /// move its baseline. The counts are unsigned 16-bit, as every band's source is.
        /// </remarks>
        internal static unsafe bool[] ClippedRows(Mat raw, double maximum)
        {
            using var low = new Mat(raw.Rows, 1, Depth.U16, 1);
            using var high = new Mat(raw.Rows, 1, Depth.U16, 1);
            CV.Reduce(raw, low, 1, ReduceOperation.Min);
            CV.Reduce(raw, high, 1, ReduceOperation.Max);

            // NB: by pointer, since GetReal is a P/Invoke per value and was a seventh of the band thread.
            var lows = (byte*)low.Data;
            var highs = (byte*)high.Data;
            bool[] clipped = null;
            for (int row = 0; row < raw.Rows; row++)
            {
                if (*(ushort*)(lows + (long)row * low.Step) == 0 ||
                    *(ushort*)(highs + (long)row * high.Step) >= maximum)
                {
                    clipped ??= new bool[raw.Rows];
                    clipped[row] = true;
                }
            }

            return clipped;
        }

        // NB: the resolved source goes through the same queue as the samples so that it is applied on the UI
        // thread, in order, ahead of anything that depends on it.
        void Forward(object value)
        {
            switch (value)
            {
                case ProbeScopeSource<TFrame> probe:
                    Post(() =>
                    {
                        source = probe;
                        selector.Refresh(probe.ProbeGroup);
                        MapContacts(probe.ProbeGroup);
                        bands.Bands = probe.Bands;
                        bands.ReferenceOffered = probe.Reference is not null;
                        bands.Shanks = ShanksOf(probe.ProbeGroup).Count;
                    });
                    break;
                case ProbeScopeOrder order:
                    Post(() => Order = order.Order);
                    break;
                case ProbeScopeClipping clipping:
                    PostClipping(clipping.Channels);
                    break;
                case ProbeScopeSample sample:
                    PostData(sample.Data, sample.SampleRate);
                    break;
            }
        }

        /// <inheritdoc/>
        protected override void DrawFrame()
        {
            if (!Faulted)
                bands.RequestSelection();
            bands.AnswerKeys();

            var maximized = Maximized;
            if (!maximized && !ImGui.GetIO().WantTextInput && ImGui.IsKeyPressed(ImGuiKey.P, false))
                Node.ShowProbe = !Node.ShowProbe;
            if (!ImGui.GetIO().WantTextInput && ImGui.IsKeyPressed(ImGuiKey.G, false))
                Node.ShowGroups = !Node.ShowGroups;
            if (!ImGui.GetIO().WantTextInput && ImGui.IsKeyPressed(ImGuiKey.U, false))
                Node.ShowThresholdCrossings = !Node.ShowThresholdCrossings;
            AnswerGroupKeys();
            ChannelGroup = Node.ShowGroups ? groupOf : null;

            var crossingRequest = new CrossingSettings(Node);
            if (crossingRequest != crossingSettings.Value)
            {
                events?.Clear();
                crossingSettings.OnNext(crossingRequest);
            }
            if (events is { } eventView)
            {
                eventView.Paused = Paused;
                if (Node.CrossingRateTimeConstant > 0)
                    eventView.RateTimeConstant = Node.CrossingRateTimeConstant;
            }

            rateColored = Node.ColorContactsByEventRate && Node.ShowThresholdCrossings && events is not null;
            selector.SelectedOutlineColor = rateColored ? ColorRateSelection : ImGuiProbeSelector.SelectionBorderColor;

            var selected = SelectedChannel;
            selectedContact = ContactOf(selected);
            hoveredContact = ContactOf(HoveredChannel);
            if (selected != revealedChannel && selectedContact >= 0)
                selector.Reveal(selectedContact);
            revealedChannel = selected;

            var availX = ImGui.GetContentRegionAvail().X;
            var availY = ImGui.GetContentRegionAvail().Y - (maximized ? 0 : ControlStripHeight);
            var gap = ImGui.GetStyle().ItemSpacing.X;

            var panePadding = ImGui.GetStyle().WindowPadding;
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(panePadding.X, 0));

            var waveWidth = availX;
            probeHovered = false;
            var probePane = !maximized && Node.ShowProbe;
            if (!maximized && !probePane)
                DrawProbeHandle(availY);
            if (probePane)
            {
                probePaneWidth = Math.Max(MinProbePaneWidth, Math.Min(probePaneWidth, availX - gap - MinPlotWidth));
                waveWidth = Math.Max(MinPlotWidth, availX - probePaneWidth - gap);

                DrawProbePane(probePaneWidth, availY);
                ImGui.SameLine(0, 0);
                Splitter("##paneSplitter", new Vector2(gap, availY), ref probePaneWidth, DefaultProbePaneWidth, 1,
                    "Drag to resize the probe pane, or double-click to restore its width.");
                ImGui.SameLine(0, 0);
            }

            // NB: ExpandedRow is known from the frame before, so the pane follows an expand or collapse a frame late.
            var eventPane = Node.ShowThresholdCrossings && ExpandedRow is not null;
            if (eventPane)
            {
                eventPaneWidth = Math.Max(MinEventPaneWidth, Math.Min(eventPaneWidth, waveWidth - gap - MinPlotWidth));
                waveWidth -= eventPaneWidth + gap;
            }

            // NB: a probe not drawn this frame keeps the contact last hovered, which the pointer has since left.
            DrawPlot(new Vector2(waveWidth, availY), probePane ? ChannelOf(selector.HoveredContactIndex) : -1,
                probeHovered);
            if (eventPane)
            {
                ImGui.SameLine(0, 0);
                Splitter("##eventSplitter", new Vector2(gap, availY), ref eventPaneWidth, DefaultEventPaneWidth, -1,
                    "Drag to resize the event pane, or double-click to restore its width.");
                ImGui.SameLine(0, 0);
                DrawEventPane(eventPaneWidth, availY);
            }
            else
                events?.Flush();
            ImGui.PopStyleVar();

            if (!maximized)
                DrawControlStrip(columns);
        }

        void DrawProbePane(float probeWidth, float availY)
        {
            ImGui.BeginChild("##probePane", new Vector2(probeWidth, availY),
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
            probeHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows);

            // NB: a selected ruler takes Delete first, as the probe view answers it.
            if (probeHovered &&!selector.RulerSelected && !ImGui.GetIO().WantTextInput && !ImGui.GetIO().KeyShift &&
                ImGui.IsKeyPressed(ImGuiKey.Delete, false) && SelectedContactChannels() is { Length: > 0 } ungrouped)
                AssignGroup(ungrouped, 0, replace: false);

            var model = source?.ProbeGroup?.Probe?.Annotations?.ModelName;
            var header = ImGui.GetCursorScreenPos();
            var headerRight = header.X + ImGui.GetContentRegionAvail().X;
            if (!string.IsNullOrEmpty(model))
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text(model);
                ImGui.SameLine();
            }
            var style = ImGui.GetStyle();
            var inner = style.ItemInnerSpacing.X;
            var fitWidth = ImGui.CalcTextSize("Fit").X + 2 * style.FramePadding.X;
            var rulerWidth = ImGui.CalcTextSize("Ruler").X + 2 * style.FramePadding.X;
            var arrowX = headerRight - ImGui.GetFrameHeight();
            var small = new Vector2(style.FramePadding.X, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, small);
            ImGui.SetCursorScreenPos(new Vector2(arrowX - inner - rulerWidth - inner - fitWidth,
                header.Y + style.FramePadding.Y));
            var channels = SelectedContactChannels();
            ImGui.BeginDisabled(channels.Length == 0);
            var fitPressed = probeHovered &&!ImGui.GetIO().WantTextInput && ImGui.IsKeyPressed(ImGuiKey.F, false);
            if ((ImGui.Button("Fit") || fitPressed) && channels.Length > 0)
                ZoomTo(channels);
            ImGui.EndDisabled();
            ImGuiWidgets.Tooltip("Fit the selected contacts' channels to the plot.\nKey: F over the probe",
                "Select contacts on the probe first.");
            ImGui.SameLine(0, inner);
            ImGuiRulerControls.DrawToggle(selector, "Ruler");
            ImGui.PopStyleVar();

            ImGui.SetCursorScreenPos(new Vector2(arrowX, header.Y));
            if (ImGui.ArrowButton("##hideProbe", ImGuiDir.Left))
                Node.ShowProbe = false;
            ImGuiWidgets.Tooltip("Hide the probe to give the plot more room.\nKey: P");

            // NB: framed on the plot's frame's top and bottom lines, which the plot lays out a header row of one
            // frame height down and a scroll bar and half an item spacing up from the bottom.
            var paneTop = ImGui.GetWindowPos().Y;
            var min = new Vector2(ImGui.GetCursorScreenPos().X, paneTop + ImGui.GetFrameHeight());
            var max = new Vector2(min.X + ImGui.GetContentRegionAvail().X,
                paneTop + availY - style.ScrollbarSize - style.ItemSpacing.Y / 2);
            ImGui.SetCursorScreenPos(min);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, style.ItemSpacing);
            ImGui.BeginChild("##probeFrame", max - min, ImGuiChildFlags.AlwaysUseWindowPadding,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
            ImGui.PopStyleVar();

            DrawContactColoring();
            DrawColorKey();

            // NB: the probe in a child of its own, since the selector fills whatever room is left and the contact's
            // strip below needs its height kept.
            var infoHeight = 2 * ImGui.GetTextLineHeight() + 2 * style.ItemSpacing.Y;
            ImGui.BeginChild("##probeView", new Vector2(0, Math.Max(1, ImGui.GetContentRegionAvail().Y - infoHeight)),
                ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
            selector.DrawZoomedView(selectionEnabled: true);

            if (!selector.PlacingRuler && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) &&
                ChannelOf(selector.HoveredContactIndex) >= 0)
                ToggleExpanded();
            ImGui.EndChild();
            DrawContactInfo();
            ImGui.EndChild();

            // NB: filled rects on whole pixels, since AddRect anti-aliases a 1 px line across two.
            var (l, t, r, b) = (MathF.Floor(min.X), MathF.Floor(min.Y), MathF.Floor(max.X), MathF.Floor(max.Y));
            const float weight = 1;
            var color = ImGuiPalette.Grey0x88;
            var draw = ImGui.GetWindowDrawList();
            draw.AddRectFilled(new Vector2(l, t), new Vector2(r, t + weight), color);
            draw.AddRectFilled(new Vector2(l, b - weight), new Vector2(r, b), color);
            draw.AddRectFilled(new Vector2(l, t), new Vector2(l + weight, b), color);
            draw.AddRectFilled(new Vector2(r - weight, t), new Vector2(r, b), color);
            ImGui.EndChild();
        }

        // side is 1 for a pane left of the splitter, which a drag to the right widens, and -1 for one right of it.
        static void Splitter(string id, Vector2 size, ref float paneWidth, float defaultWidth, int side, string tooltip)
        {
            ImGui.InvisibleButton(id, size);
            var hovered = ImGui.IsItemHovered();
            var active = ImGui.IsItemActive();
            if (active)
                paneWidth += side * ImGui.GetIO().MouseDelta.X;
            if (hovered && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                paneWidth = defaultWidth;
            if (hovered || active)
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEw);
            ImGuiWidgets.Tooltip(tooltip);

            if (!hovered && !active)
                return;
            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            var x = (min.X + max.X) / 2;
            ImGui.GetWindowDrawList().AddLine(new Vector2(x, min.Y), new Vector2(x, max.Y),
                ImGui.GetColorU32(ImGuiCol.SeparatorHovered), 2f);
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            selector.SelectionChanged -= SelectContacts;
            crossingSettings?.Dispose();
            events = null;
            scheduler?.Dispose();
            scheduler = null;
            bands = null;
            source = null;
            base.Unload();
        }
    }
}
