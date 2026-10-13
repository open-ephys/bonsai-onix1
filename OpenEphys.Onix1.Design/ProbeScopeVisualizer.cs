using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Forms;
using Bonsai;
using Bonsai.Design;
using Bonsai.Expressions;
using Hexa.NET.ImGui;
using OpenCV.Net;

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
    public class ProbeScopeVisualizer<TFrame> : DialogTypeVisualizer
    {
        const float ProbePaneWidth = 260f;
        const int DrainMilliseconds = 15;
        const float CollapsedPaneWidth = 28f;

        ProbeScope<TFrame> node;
        readonly ConcurrentQueue<object> pending = new();
        Timer drainTimer;
        readonly ImGuiProbeSelector selector = new();
        ImGuiLfpViewerPanel waveform;
        ImGuiProbeScopeControlStrip<TFrame> strip;

        ImGuiHoverGLControl canvas;
        DisplayPacer pacer;
        EventLoopScheduler scheduler;

        string deviceName;

        // NB: shown in the pane rather than thrown. The scope is a view on data it does not change, so a
        // mistake in it must not stop the workflow, and closing the window, correcting the name and
        // reopening it should work while the workflow runs. Set on the scheduler thread as well as this
        // one, and read once a frame.
        volatile string fault;
        bool probePaneCollapsed;
        ProbeScopeSource<TFrame> source;
        float stripHeight;

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            var context = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            var workflowBuilder = (WorkflowBuilder)provider.GetService(typeof(WorkflowBuilder));
            node = (ProbeScope<TFrame>)ExpressionBuilder.GetWorkflowElement(context.Source);
            fault = null;

            try { deviceName = UpstreamDevice.Resolve(workflowBuilder?.Workflow, context.Source, node.DeviceName, node.DeviceNameOf); }
            catch (InvalidOperationException ex) { fault = $"ProbeScope: {ex.Message}"; }

            // NB: no plot can be wider than the widest monitor unless its window spans two, so that is as
            // much horizontal detail as is ever worth holding.
            waveform = new(node, node.HistoryBytes, Screen.AllScreens.Max(screen => screen.Bounds.Width)) { Unit = node.Unit };
            selector.ShowGrid = false;
            selector.DefaultZoomWindowFraction = 1f;

            strip = new ImGuiProbeScopeControlStrip<TFrame>(node);

            scheduler = new EventLoopScheduler();

            canvas = new ImGuiHoverGLControl(waveform.Hotkeys.Concat(selector.Hotkeys))
            {
                Size = new System.Drawing.Size(1100, 620),
                Dock = DockStyle.Fill
            };
            canvas.Render += RenderFrame;

            // NB: paced from the handle's lifetime rather than Load's, since a handle can be recreated.
            canvas.HandleCreated += (_, _) => pacer = new DisplayPacer(canvas.Handle);
            canvas.HandleDestroyed += (_, _) => StopPacer();
            canvas.Paint += (_, _) => pacer?.Painted();

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            visualizerService?.AddControl(canvas);

            // NB: each frame applies what has arrived, and so does a timer, which runs while the window is
            // minimized and paints nothing, so that what arrives meanwhile is applied rather than held.
            drainTimer = new Timer { Interval = DrainMilliseconds };
            drainTimer.Tick += (_, _) => Drain();
            drainTimer.Start();
        }

        /// <inheritdoc/>
        /// <remarks>
        /// What the band chain produces is queued and applied at the start of each frame, rather than handed to
        /// <see cref="Show"/> one value at a time as the base class does, which at a probe's rate would keep the
        /// window from painting, or in batches on a timer, as <see cref="BufferedVisualizer"/> delivers them, which
        /// would feed the frames unevenly.
        /// </remarks>
        public override IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider) =>
            Observable.Create<object>(_ =>
                BindBands(source.Merge().Cast<TFrame>()).Subscribe(pending.Enqueue, ex => fault = ex.Message));

        IObservable<object> BindBands(IObservable<TFrame> frames)
        {
            if (fault is not null)
                return Observable.Never<object>();

            // NB: off the acquisition thread the frames arrive on, so the band filters never hold it up, and onto
            // one ordered thread, since the filters carry state across blocks.
            return frames
                .ObserveOn(scheduler)
                // NB: GetDevice throws until the configuration operator has registered, which
                // nothing orders before the visualizer subscribes. A frame arriving proves it has.
                .Publish(shared => shared
                    .Take(1)
                    .Select(_ => ResolveSource())
                    .Where(probe => probe is not null)
                    .SelectMany(probe => Observable.Return<object>(probe)
                        .Concat(strip.BandSelected
                            .Select(band => BandOutput(probe, band, shared))
                            .Switch()
                        )
                    )
                );
        }

        ProbeScopeSource<TFrame> ResolveSource()
        {
            try
            {
                DeviceInfo deviceInfo = null;
                using (DeviceManager.GetDevice(deviceName).Subscribe(info => deviceInfo = info)) { }
                return node.CreateSource(deviceInfo);
            }
            catch (Exception ex)
            {
                fault = ex.Message;
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

        void Drain()
        {
            while (pending.TryDequeue(out var value))
                Apply(value);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Nothing is shown through here, since <see cref="Visualize"/> applies what the scope receives itself.
        /// </remarks>
        public override void Show(object value)
        {
        }

        // NB: the resolved source arrives through the same sequence as the samples so that it is applied on the UI
        // thread, in order, ahead of anything that depends on it.
        void Apply(object value)
        {
            if (value is ProbeScopeSource<TFrame> probe)
            {
                source = probe;
                selector.Refresh(probe.ProbeGroup);
                strip.Bands = probe.Bands;
                strip.ReferenceOffered = probe.Reference is not null;
                return;
            }

            if (value is ProbeScopeClipping clipping)
            {
                waveform.MarkClipping(clipping.Channels);
                return;
            }

            var sample = (ProbeScopeSample)value;
            waveform.Update(sample.Data, sample.SampleRate);
        }

        void RenderFrame(object sender, EventArgs e)
        {
            Drain();
            var io = ImGui.GetIO();
            io.DisplaySize = new Vector2(canvas.Width, canvas.Height);

            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(io.DisplaySize);
            ImGui.Begin("Probe Scope##probescope",
                ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            var availX = ImGui.GetContentRegionAvail().X;

            // NB: measured from the last frame rather than derived from the widgets, which are the
            // strip's business and change as columns are added.
            if (stripHeight <= 0)
                stripHeight = ImGui.GetTextLineHeight() + ImGui.GetFrameHeight() * 2;

            // NB: maximized, the plot takes the window, and the probe pane and the strip are not drawn.
            var maximized = waveform.Maximized;
            var availY = ImGui.GetContentRegionAvail().Y - (maximized ? 0 : stripHeight);
            var probeWidth = probePaneCollapsed ? CollapsedPaneWidth : ProbePaneWidth;
            var waveWidth = maximized ? availX : Math.Max(200f, availX - probeWidth - ImGui.GetStyle().ItemSpacing.X);
            var probeOrigin = ImGui.GetCursorScreenPos();

            // NB: no vertical padding on either pane, so both start at their own top edge and the
            // two frames land on the same lines.
            var panePadding = ImGui.GetStyle().WindowPadding;
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(panePadding.X, 0));

            if (!maximized)
                DrawProbePane(probeWidth, availY);

            ImGui.BeginChild("##wavePane", new Vector2(waveWidth, availY));
            var message = fault;
            if (message is null)
            {
                waveform.RefreshRate = pacer?.RefreshRate ?? waveform.RefreshRate;
                strip.RequestSelection();
                waveform.Draw();
            }
            else
            {
                // NB: TextUnformatted rather than TextWrapped, which reads its string as a printf format,
                // and the message is exception text that may contain '%'.
                ImGui.PushTextWrapPos();
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiPalette.VibrantCoral);
                ImGui.TextUnformatted(message);
                ImGui.PopStyleColor();
                ImGui.TextUnformatted("Close this window, correct the ProbeScope's DeviceName, and open it again.");
                ImGui.PopTextWrapPos();
            }
            ImGui.EndChild();

            ImGui.PopStyleVar();

            if (!maximized)
            {
                var stripTop = ImGui.GetCursorScreenPos().Y;
                strip.Draw(waveform);
                stripHeight = ImGui.GetCursorScreenPos().Y - stripTop;

                // Scroll and zoom are handled from the root window, bounded to the probe pane so the
                // wheel still scrolls the channel list on the waveform side.
                if (!probePaneCollapsed)
                    selector.HandleScrollInput(probeOrigin.Y + availY, probeOrigin.X + probeWidth);
            }

            ImGui.End();
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
            ImGuiControls.Tooltip(probePaneCollapsed ? "Show the probe." : "Hide the probe, to give the plot more room.");

            var model = source?.ProbeGroup?.Probe?.Annotations?.ModelName;
            if (!probePaneCollapsed)
            {
                if (!string.IsNullOrEmpty(model))
                {
                    ImGui.SameLine();
                    ImGui.Text(model);
                }

                ImGui.SetCursorScreenPos(new Vector2(ImGui.GetCursorScreenPos().X, probeViewTop));
                var probeHeight = ImGui.GetContentRegionAvail().Y;
                selector.UpdateLayout(probeHeight, ImGui.GetContentRegionAvail().X);
                selector.DrawZoomedView(probeHeight, selectionEnabled: false);

            }
            else if (!string.IsNullOrEmpty(model))
            {
                ImGuiControls.StackedLabel(model);
            }
            ImGui.EndChild();

            ImGui.SameLine();
        }

        void StopPacer()
        {
            pacer?.Dispose();
            pacer = null;
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            drainTimer?.Dispose();
            while (pending.TryDequeue(out _)) { }
            StopPacer();
            scheduler?.Dispose();
            waveform?.Dispose();
            canvas?.Dispose();

            strip = null;
            scheduler = null;
            waveform = null;
            canvas = null;
            source = null;
            drainTimer = null;
            node = null;
        }
    }
}
