using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
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
    /// <remarks>
    /// The device is resolved once the first frame arrives, since data flowing proves the configuration operator has
    /// registered it.
    /// </remarks>
    /// <typeparam name="TFrame">The data frame type the scope displays.</typeparam>
    public class ProbeScopeVisualizer<TFrame> : WaveformScopeVisualizerBase
    {
        const float DefaultProbePaneWidth = 260f;
        const float MinProbePaneWidth = 120f;
        const float MinPlotWidth = 200f;
        const float CollapsedPaneWidth = 28f;

        // NB: the colors of the configuration dialogs' enabled contacts, with the plot's selection white as its
        // selected label's box is, and hover amber three quarters of the way to white.
        static readonly uint ColorContactEnabled = ImGuiPalette.AmberGold;
        static readonly uint ColorContactSelected = ImGuiPalette.White;
        static readonly uint ColorContactHovered = 0xFFC2EFFFu;

        // NB: group n's key is GroupKeys[n - 1].
        static readonly ImGuiKey[] GroupKeys = Enumerable.Range(1, 9).Select(n => ImGuiKey.Key0 + n).ToArray();

        // NB: no legend, since the probe was configured in the configuration dialog, which explains its colors, and
        // the group swatches name the groups'.
        readonly ImGuiProbeSelector selector = new(zoomWindowFraction: 1f) { ShowGrid = false };
        Dictionary<int, int> groupOfChannel = new();
        ChannelSettings[] groupedFrom;
        Func<int, int> traceGroup;
        int lastGroupKey = -1;
        double lastGroupKeyTime;
        Dictionary<int, int> contactOfChannel = new();
        int[] channelOfContact = Array.Empty<int>();
        int selectedContact = -1;
        int hoveredContact = -1;
        int revealedChannel = -1;
        ImGuiProbeScopeBandColumns<TFrame> bands;
        EventLoopScheduler scheduler;
        string deviceName;
        bool probePaneCollapsed;
        float probePaneWidth = DefaultProbePaneWidth;
        ProbeScopeSource<TFrame> source;

        ProbeScope<TFrame> Node => (ProbeScope<TFrame>)Scope;

        /// <inheritdoc/>
        protected override long HistoryBytes => Node.HistoryBytes;

        /// <inheritdoc/>
        protected override string Unit => Node.Unit;

        /// <inheritdoc/>
        protected override IEnumerable<ImGuiKey> Hotkeys =>
            selector.Hotkeys.Concat(new[] { ImGuiKey.P, ImGuiKey.K, ImGuiKey.G }).Concat(GroupKeys);

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
            columns = new[]
            {
                bands.BandColumn, bands.FiltersColumn, bands.OrderColumn, standard.Timebase, standard.ChannelHeight,
                standard.Range, standard.Palette, new ControlStripColumn("Groups", DrawGroups), standard.Display,
            };
            scheduler = new EventLoopScheduler();

            selector.FillColor = ContactColor;
            selector.Blocked = contact => ChannelOf(contact) < 0;
            selector.SelectionChanged += SelectContacts;
            traceGroup = TraceGroup;
        }

        // NB: while groups are shown, a contact is the color of its channel's trace.
        uint ContactColor(int contact) =>
            contact == selectedContact ? ColorContactSelected
            : contact == hoveredContact ? ColorContactHovered
            : Node.ShowGroups ? GroupColor(TraceGroup(ChannelOf(contact)))
            : ColorContactEnabled;

        // NB: groups are numbered from 1 on the probe, as their keys are, and from 0 in the palette, where a negative
        // group is none.
        int TraceGroup(int channel) => GroupOf(channel) - 1;

        int GroupOf(int channel) => Groups().TryGetValue(channel, out var group) ? group : 0;

        // NB: the node's groups by channel, worked out again only when its channel settings are replaced, since they
        // are read for every row and contact every frame.
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

        // NB: the channels given join group, leaving whatever group they were in, and with replace its other members
        // leave it, or with group 0 they leave their groups. A new array rather than the entries changed in place,
        // since the plot works its masks out again only when the array is replaced. An entry left with nothing to
        // say is dropped.
        void AssignGroup(int[] channels, int group, bool replace)
        {
            var assigned = new HashSet<int>(channels);
            var entries = (Node.Channels ?? Array.Empty<ChannelSettings>())
                .Select(entry => new ChannelSettings
                {
                    Index = entry.Index,
                    Hidden = entry.Hidden,
                    Group = assigned.Remove(entry.Index) ? group : replace && entry.Group == group ? 0 : entry.Group,
                })
                .Concat(assigned.Select(channel => new ChannelSettings { Index = channel, Group = group }))
                .Where(entry => entry.Hidden || entry.Group > 0)
                .OrderBy(entry => entry.Index)
                .ToArray();
            Node.Channels = entries.Length > 0 ? entries : null;
        }

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

        // NB: the probe's selection becomes the group with replace, or joins it without.
        void GroupSelection(int group, bool replace)
        {
            var channels = SelectedContactChannels();
            if (channels.Length == 0)
                return;
            AssignGroup(channels, group, replace);
            Node.ShowGroups = true;
        }

        // NB: the buttons the group keys press, each in its group's color, faint while the group is empty.
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

                var color = GroupColor(n - 1);
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

        void DrawGroups(float width)
        {
            var node = Node;
            var show = node.ShowGroups;
            if (ImGui.Checkbox("Show", ref show))
                node.ShowGroups = show;
            ImGuiWidgets.Tooltip("Draw each channel in its group's color, on the traces and the probe, and channels in " +
                "no group in grey.\nKey: G\n1 to 9 selects a group on the probe, and pressing it twice fits it to the " +
                "plot. Shift+1 to 9 adds the probe's selected contacts to a group, Ctrl+1 to 9 makes them the group, " +
                "and Delete over the probe takes them out of their groups.");
        }

        // NB: anywhere in the window, since no other keys are digits, though they act on the probe's selection. A
        // group's number pressed twice in quick succession fits it, as F would after the first.
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

        // NB: one contact selects its channel and brings it into view. Several are only fitted when asked, so the
        // selection stays what Fit acts on rather than a zoom applied at once.
        void SelectContacts(object sender, EventArgs e)
        {
            var channels = SelectedContactChannels();
            if (channels.Length == 1)
            {
                SelectedChannel = channels[0];
                Reveal(channels[0]);
            }
        }

        // NB: a contact no channel records has nothing to show.
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
            var clipping = frames
                .Select(frame => ClippedRows(band.Raw(frame), probe.AdcMaximum))
                .Where(channels => channels is not null)
                .Select(channels => (object)new ProbeScopeClipping(Reorder(channels, rowChannels)));

            var output = selection.AcCoupled && band.AcTransform is not null ? band.AcTransform(frames) : band.Transform(frames);
            if (selection.CommonMedianReference && probe.Reference is not null)
                output = probe.Reference(output);

            // NB: reordered last, since referencing groups the channels by their converter in the device's order.
            return Observable.Return<object>(new ProbeScopeOrder(order)).Concat(output
                .Select(data => (object)new ProbeScopeSample(Reorder(data, rowChannels), band.SampleRate))
                .Merge(clipping));
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
        /// So that each has a row of its own where many electrodes share a depth, as on a high density probe, while
        /// depth stays proportional down the plot. The pitch, the smallest step between depths, and the most rows
        /// sharing one are the probe's, so every lane keeps one scale.
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
        /// <remarks>
        /// A depth between two of that lane's depths takes a position between theirs, and one beyond its ends a
        /// position at its average spacing. Positions are whole rows, so that the rows stay a row apart, and a row
        /// that would land on or above the one before it in its lane goes on the next row down, as electrodes
        /// sharing a depth do in that lane.
        /// </remarks>
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

        // NB: a block whose row count the channel map does not match is shown in the device's order, under the
        // labels of the order asked for.
        static unsafe Mat Reorder(Mat data, int[] rowChannels)
        {
            if (rowChannels is null || data.Rows != rowChannels.Length)
                return data;

            var reordered = new Mat(data.Size, data.Depth, data.Channels);
            using (data)
            {
                // NB: by pointer, since GetRow allocates a native header per row on every block.
                var rowBytes = reordered.Step;
                for (int row = 0; row < rowChannels.Length; row++)
                {
                    var from = (byte*)data.Data + (long)rowChannels[row] * data.Step;
                    var to = (byte*)reordered.Data + (long)row * reordered.Step;
                    Buffer.MemoryCopy(from, to, rowBytes, rowBytes);
                }
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
        /// move its baseline. Null when nothing clipped, so a clean recording sends the display nothing.
        /// </remarks>
        static bool[] ClippedRows(Mat raw, double maximum)
        {
            using var low = new Mat(raw.Rows, 1, raw.Depth, 1);
            using var high = new Mat(raw.Rows, 1, raw.Depth, 1);
            CV.Reduce(raw, low, 1, ReduceOperation.Min);
            CV.Reduce(raw, high, 1, ReduceOperation.Max);

            bool[] clipped = null;
            for (int row = 0; row < raw.Rows; row++)
            {
                if (low.GetReal(row, 0) <= 0 || high.GetReal(row, 0) >= maximum)
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
        /// <remarks>
        /// The probe pane sits left of the plot, and both are left out with the strip while maximized.
        /// </remarks>
        protected override void DrawFrame()
        {
            if (!Faulted)
                bands.RequestSelection();
            bands.AnswerKeys();

            // NB: not while maximized, where the pane is not drawn and its state would change out of sight.
            var maximized = Maximized;
            if (!maximized && !ImGui.GetIO().WantTextInput && ImGui.IsKeyPressed(ImGuiKey.P, false))
                probePaneCollapsed = !probePaneCollapsed;
            if (!ImGui.GetIO().WantTextInput && ImGui.IsKeyPressed(ImGuiKey.G, false))
                Node.ShowGroups = !Node.ShowGroups;
            AnswerGroupKeys();
            ChannelGroup = Node.ShowGroups ? traceGroup : null;

            // NB: only a new selection scrolls the probe, so a pan away from it holds until the next one.
            var selected = SelectedChannel;
            selectedContact = ContactOf(selected);
            hoveredContact = ContactOf(HoveredChannel);
            if (selected != revealedChannel && selectedContact >= 0)
                selector.Reveal(selectedContact);
            revealedChannel = selected;

            var availX = ImGui.GetContentRegionAvail().X;
            var availY = ImGui.GetContentRegionAvail().Y - (maximized ? 0 : ControlStripHeight);
            var gap = ImGui.GetStyle().ItemSpacing.X;

            // NB: no vertical padding on either pane, so both start at their own top edge and the
            // two frames land on the same lines.
            var panePadding = ImGui.GetStyle().WindowPadding;
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(panePadding.X, 0));

            var waveWidth = availX;
            KeysElsewhere = false;
            if (!maximized)
            {
                // NB: clamped every frame rather than only while dragged, so shrinking the window can't leave the
                // plot narrower than its minimum.
                probePaneWidth = Math.Max(MinProbePaneWidth, Math.Min(probePaneWidth, availX - gap - MinPlotWidth));
                var probeWidth = probePaneCollapsed ? CollapsedPaneWidth : probePaneWidth;
                waveWidth = Math.Max(MinPlotWidth, availX - probeWidth - gap);

                DrawProbePane(probeWidth, availY);
                DrawPaneSplitter(gap, availY);
            }

            // NB: a probe not drawn this frame keeps the contact last hovered, which the pointer has since left.
            HoveredChannel = !maximized && !probePaneCollapsed ? ChannelOf(selector.HoveredContactIndex) : -1;
            DrawPlot(new Vector2(waveWidth, availY));
            ImGui.PopStyleVar();

            if (!maximized)
                DrawControlStrip(columns);
        }

        void DrawProbePane(float probeWidth, float availY)
        {
            ImGui.BeginChild("##probePane", new Vector2(probeWidth, availY),
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
            KeysElsewhere = !probePaneCollapsed && ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows);

            // NB: a selected ruler takes Delete first, as the probe view answers it.
            if (KeysElsewhere && !selector.RulerSelected && !ImGui.GetIO().WantTextInput && !ImGui.GetIO().KeyShift &&
                ImGui.IsKeyPressed(ImGuiKey.Delete, false) && SelectedContactChannels() is { Length: > 0 } ungrouped)
                AssignGroup(ungrouped, 0, replace: false);

            if (ImGui.Button(probePaneCollapsed ? "»" : "«"))
                probePaneCollapsed = !probePaneCollapsed;
            ImGuiWidgets.Tooltip((probePaneCollapsed ? "Show the probe." : "Hide the probe, to give the plot more room.") +
                "\nKey: P");

            var model = source?.ProbeGroup?.Probe?.Annotations?.ModelName;
            if (!probePaneCollapsed)
            {
                if (!string.IsNullOrEmpty(model))
                {
                    ImGui.SameLine();
                    ImGui.Text(model);
                }

                // NB: set apart from the title above, as controls rather than part of the heading.
                ImGui.Dummy(new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));

                var channels = SelectedContactChannels();
                ImGui.BeginDisabled(channels.Length == 0);
                var fitPressed = !ImGui.GetIO().WantTextInput && ImGui.IsKeyPressed(ImGuiKey.F, false)
                    && ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows);
                if ((ImGui.Button("Fit") || fitPressed) && channels.Length > 0)
                    ZoomTo(channels);
                ImGui.EndDisabled();
                ImGuiWidgets.Tooltip("Fit the selected contacts' channels to the plot.\nKey: F over the probe",
                    "Select contacts on the probe first.");
                ImGui.SameLine();
                ImGuiRulerControls.DrawToggle(selector, "Ruler");

                DrawGroupSwatches();
                selector.DrawZoomedView(selectionEnabled: true);
            }
            else if (!string.IsNullOrEmpty(model))
            {
                ImGuiWidgets.StackedLabel(model);
            }
            ImGui.EndChild();
        }

        void DrawPaneSplitter(float width, float height)
        {
            ImGui.SameLine(0, 0);
            if (probePaneCollapsed)
            {
                ImGui.Dummy(new Vector2(width, height));
                ImGui.SameLine(0, 0);
                return;
            }

            ImGui.InvisibleButton("##paneSplitter", new Vector2(width, height));
            var hovered = ImGui.IsItemHovered();
            var active = ImGui.IsItemActive();
            if (active)
                probePaneWidth += ImGui.GetIO().MouseDelta.X;
            if (hovered && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                probePaneWidth = DefaultProbePaneWidth;
            if (hovered || active)
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEw);
            ImGuiWidgets.Tooltip("Drag to resize the probe pane, or double-click to restore its width.");

            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            var x = (min.X + max.X) / 2;
            var color = ImGui.GetColorU32(active ? ImGuiCol.SeparatorActive : hovered ? ImGuiCol.SeparatorHovered : ImGuiCol.Separator);
            ImGui.GetWindowDrawList().AddLine(new Vector2(x, min.Y), new Vector2(x, max.Y), color, active || hovered ? 2f : 1f);

            ImGui.SameLine(0, 0);
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            selector.SelectionChanged -= SelectContacts;
            scheduler?.Dispose();
            scheduler = null;
            bands = null;
            source = null;
            base.Unload();
        }
    }
}
