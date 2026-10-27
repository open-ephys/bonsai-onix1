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
        const float ProbePaneWidth = 260f;
        const float CollapsedPaneWidth = 28f;

        readonly ImGuiProbeSelector selector = new(zoomWindowFraction: 1f) { ShowGrid = false };
        ImGuiProbeScopeBandColumns<TFrame> bands;
        EventLoopScheduler scheduler;
        string deviceName;
        bool probePaneCollapsed;
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
            columns = new[] { bands.BandColumn, bands.FiltersColumn }.Concat(StandardColumns.All)
                .Append(new ControlStripColumn("Rulers", DrawRulerControls)).ToArray();
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
            var clipping = frames
                .Select(frame => ClippedRows(band.Raw(frame), probe.AdcMaximum))
                .Where(channels => channels is not null)
                .Select(channels => (object)new ProbeScopeClipping(channels));

            var output = selection.AcCoupled && band.AcTransform is not null ? band.AcTransform(frames) : band.Transform(frames);
            if (selection.CommonMedianReference && probe.Reference is not null)
                output = probe.Reference(output);

            return output.Select(data => (object)new ProbeScopeSample(data, band.SampleRate)).Merge(clipping);
        }

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
            var probeWidth = probePaneCollapsed ? CollapsedPaneWidth : ProbePaneWidth;
            var waveWidth = maximized ? availX : Math.Max(200f, availX - probeWidth - ImGui.GetStyle().ItemSpacing.X);

            // NB: no vertical padding on either pane, so both start at their own top edge and the
            // two frames land on the same lines.
            var panePadding = ImGui.GetStyle().WindowPadding;
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(panePadding.X, 0));

            if (!maximized)
                DrawProbePane(probeWidth, availY);

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

                ImGui.SetCursorScreenPos(new Vector2(ImGui.GetCursorScreenPos().X, probeViewTop));
                selector.DrawZoomedView(selectionEnabled: false);
            }
            else if (!string.IsNullOrEmpty(model))
            {
                ImGuiWidgets.StackedLabel(model);
            }
            ImGui.EndChild();

            ImGui.SameLine();
        }

        // NB: a ruler armed while the probe is hidden would be dropped on the next frame, so the buttons wait for
        // the probe to be shown.
        void DrawRulerControls(float width)
        {
            ImGui.BeginDisabled(probePaneCollapsed);
            selector.DrawRulerControls();
            ImGui.EndDisabled();
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
