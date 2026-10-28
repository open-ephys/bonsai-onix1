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
    public class ProbeScopeVisualizer<TFrame> : WaveformScopeVisualizer
    {
        const float DefaultProbePaneWidth = 260f;
        const float MinProbePaneWidth = 120f;
        const float MinPlotWidth = 200f;
        const float CollapsedPaneWidth = 28f;

        readonly ImGuiProbeSelector selector = new(zoomWindowFraction: 1f) { ShowGrid = false };
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
        protected override IEnumerable<ImGuiKey> Hotkeys => selector.Hotkeys;

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
            columns = new[] { bands.BandColumn, bands.FiltersColumn, bands.OrderColumn }.Concat(StandardColumns.All).ToArray();
            scheduler = new EventLoopScheduler();
        }

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
            var rowChannels = selection.Order == ChannelOrder.Depth ? DepthOrder(probe.ProbeGroup) : null;
            var clipping = frames
                .Select(frame => ClippedRows(band.Raw(frame), probe.AdcMaximum))
                .Where(channels => channels is not null)
                .Select(channels => (object)new ProbeScopeClipping(Reorder(channels, rowChannels)));

            var output = selection.AcCoupled && band.AcTransform is not null ? band.AcTransform(frames) : band.Transform(frames);
            if (selection.CommonMedianReference && probe.Reference is not null)
                output = probe.Reference(output);

            // NB: reordered last, since referencing groups the channels by their converter in the device's order.
            return Observable.Return<object>(new ProbeScopeOrder(rowChannels)).Concat(output
                .Select(data => (object)new ProbeScopeSample(Reorder(data, rowChannels), band.SampleRate))
                .Merge(clipping));
        }

        /// <summary>
        /// The channels of <paramref name="probe"/>'s channel map from the shallowest electrode to the deepest, and
        /// across the probe where electrodes share a depth, or null where it maps no channels.
        /// </summary>
        static int[] DepthOrder(SingleProbeGroup probe)
        {
            var contacts = probe?.Probe?.Contacts;
            if (probe?.ChannelMap is not { Count: > 0 } map || contacts is null)
                return null;

            return map
                .OrderByDescending(entry => contacts[entry.Value].PosY)
                .ThenBy(entry => contacts[entry.Value].PosX)
                .Select(entry => entry.Key)
                .ToArray();
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
                        bands.Bands = probe.Bands;
                        bands.ReferenceOffered = probe.Reference is not null;
                    });
                    break;
                case ProbeScopeOrder order:
                    Post(() => RowChannels = order.RowChannels);
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

            var availX = ImGui.GetContentRegionAvail().X;
            var maximized = Maximized;
            var availY = ImGui.GetContentRegionAvail().Y - (maximized ? 0 : ControlStripHeight);
            var gap = ImGui.GetStyle().ItemSpacing.X;

            // NB: no vertical padding on either pane, so both start at their own top edge and the
            // two frames land on the same lines.
            var panePadding = ImGui.GetStyle().WindowPadding;
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(panePadding.X, 0));

            var waveWidth = availX;
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

            DrawPlot(new Vector2(waveWidth, availY));
            ImGui.PopStyleVar();

            if (!maximized)
                DrawControlStrip(columns);
        }

        void DrawProbePane(float probeWidth, float availY)
        {
            ImGui.BeginChild("##probePane", new Vector2(probeWidth, availY),
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            // NB: a button's height, which is the row the plot's time labels take as well, so the
            // probe view and the plot begin on the same line
            var probeViewTop = ImGui.GetCursorScreenPos().Y + ImGui.GetFrameHeight();
            if (ImGui.Button(probePaneCollapsed ? "»" : "«"))
                probePaneCollapsed = !probePaneCollapsed;
            ImGuiWidgets.Tooltip(probePaneCollapsed ? "Show the probe." : "Hide the probe, to give the plot more room.");

            var model = source?.ProbeGroup?.Probe?.Annotations?.ModelName;
            if (!probePaneCollapsed)
            {
                if (!string.IsNullOrEmpty(model))
                {
                    ImGui.SameLine();
                    ImGui.Text(model);
                }

                const string RulerLabel = "Ruler";
                var rulerWidth = ImGui.CalcTextSize(RulerLabel).X + ImGui.GetStyle().FramePadding.X * 2;
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, ImGui.GetContentRegionAvail().X - rulerWidth));
                ImGuiRulerControls.DrawToggle(selector, RulerLabel);

                ImGui.SetCursorScreenPos(new Vector2(ImGui.GetCursorScreenPos().X, probeViewTop));
                selector.DrawZoomedView(selectionEnabled: false);
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
            scheduler?.Dispose();
            scheduler = null;
            bands = null;
            source = null;
            base.Unload();
        }
    }
}
