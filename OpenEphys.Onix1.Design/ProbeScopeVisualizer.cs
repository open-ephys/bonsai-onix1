using System;
using System.ComponentModel;
using System.Collections.Generic;
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
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// One selectable band of a probe: a short display name, a description of its passband for the dropdown,
    /// the rate at which that band produces samples, and the passband of its AC coupled variant, or null where
    /// it has none, as a band already high-passed well above the coupling's cutoff does not. How the band is
    /// actually computed is left to concrete implementation of <see cref="ProbeScopeVisualizer{TFrame}.ProcessBand"/>.
    /// </summary>
    internal sealed record ProbeScopeBand(string Name, string Description, int SampleRate, string AcDescription);

    /// <summary>
    /// Everything a <see cref="ProbeScopeVisualizer{TFrame}"/> needs from a resolved device: the probe
    /// geometry and the bands available for display.
    /// </summary>
    internal sealed record ProbeScopeSource(
        SingleProbeGroup ProbeGroup,
        IReadOnlyList<ProbeScopeBand> Bands);

    /// <summary>
    /// One block of band output on its way to the display, with the rate of the band that produced it.
    /// </summary>
    internal sealed record ProbeScopeSample(Mat Data, int SampleRate);

    /// <summary>
    /// The channels one frame found at either end of the ADC's range.
    /// </summary>
    internal sealed record ProbeScopeClipping(bool[] Channels);

    /// <summary>
    /// Provides the pass-through node a <see cref="ProbeScopeVisualizer{TFrame}"/> opens on, and the
    /// settings that belong to the workflow rather than to one viewing session.
    /// </summary>
    /// <typeparam name="TFrame">The data frame type the scope displays.</typeparam>
    public abstract class ProbeScope<TFrame> : Sink<TFrame>
    {
        /// <summary>
        /// Gets or sets the number of seconds of data retained behind the display.
        /// </summary>
        /// <remarks>
        /// While the scope is paused, the view can be moved back through this much of what has already
        /// been shown and redrawn at any timebase. What a second costs, and how many of them a probe is
        /// willing to spend memory on, is answered by <see cref="HistoryBytes"/>.
        /// </remarks>
        [Description("The number of seconds of data retained behind the display, which a paused view can " +
            "be moved back through. Longer histories cost proportionally more memory.")]
        public double HistorySeconds { get; set; } = 10;

        /// <summary>
        /// Gets or sets the name of the device whose data is displayed.
        /// </summary>
        /// <remarks>
        /// Left empty, the upstream data operator's device is used if it can be unambiguously resolved. It needs
        /// setting only when it cannot, as when the data arrives through a subject, or through a combinator
        /// such as Zip that hides its source.
        /// </remarks>
        public abstract string DeviceName { get; set; }

        /// <summary>
        /// What <see cref="HistorySeconds"/> costs on this probe, which only a concrete scope can say,
        /// knowing its own channel count and fastest band, and how much memory is reasonable to spend.
        /// </summary>
        internal abstract long HistoryBytes { get; }

        /// <inheritdoc/>
        public override IObservable<TFrame> Process(IObservable<TFrame> source) => source;
    }

    /// <summary>
    /// Provides a type visualizer that shows a probe schematic beside a live multi-channel waveform viewer
    /// for a streaming probe. Subclasses supply how to read the device name off the upstream data operator
    /// and how to turn its <see cref="DeviceInfo"/> into geometry and bands.
    /// </summary>
    /// <remarks>
    /// The visualizer opens on a pass-through sink node downstream of the data operator. The device is
    /// resolved once the first frame arrives, since data flowing proves the configuration operator has
    /// registered it.
    /// </remarks>
    /// <typeparam name="TFrame">The data frame type the scope displays.</typeparam>
    public abstract class ProbeScopeVisualizer<TFrame> : BufferedVisualizer
    {
        const float ProbePaneWidth = 260f;
        const float CollapsedPaneWidth = 28f;

        readonly ImGuiProbeSelector selector = new();
        ImGuiLfpViewerPanel waveform;
        ImGuiProbeScopeControlStrip strip;

        ImPlotGLControl canvas;
        DisplayPacer pacer;
        EventLoopScheduler scheduler;

        string deviceName;

        // NB: shown in the pane rather than thrown. The scope is a view on data it does not change, so a
        // mistake in it must not stop the workflow, and closing the window, correcting the name and
        // reopening it should work while the workflow runs. Set on the scheduler thread as well as this
        // one, and read once a frame.
        volatile string fault;
        bool probePaneCollapsed;
        ProbeScopeSource source;
        float stripHeight;

        /// <summary>
        /// Returns the device name declared on <paramref name="upstreamOperator"/>, or null if
        /// it is not a data operator this scope can display.
        /// </summary>
        private protected abstract string DeviceNameOf(object upstreamOperator);

        /// <summary>
        /// Builds the geometry and bands for a resolved device. Throw to report a device this
        /// scope cannot display.
        /// </summary>
        private protected abstract ProbeScopeSource CreateSource(DeviceInfo info);

        /// <summary>
        /// Builds the complete signal displayed for <paramref name="selection"/>: everything done to the
        /// device's data to produce what is drawn, in the unit named by <see cref="Unit"/>.
        /// </summary>
        /// <param name="selection">The band, by index into the list returned by <see cref="CreateSource"/>,
        /// and the options asked for.</param>
        /// <param name="frames">The device's frame sequence.</param>
        private protected abstract IObservable<Mat> ProcessBand(BandSelection selection, IObservable<TFrame> frames);

        /// <summary>
        /// Unit the bands produce.
        /// </summary>
        private protected abstract string Unit { get; }

        /// <summary>
        /// The channels of <paramref name="frame"/> holding a sample at either end of the ADC's range in the stream
        /// <paramref name="selection"/>'s band comes from, or null where none does.
        /// </summary>
        private protected abstract bool[] Clipped(TFrame frame, BandSelection selection);

        /// <summary>
        /// The rows of <paramref name="raw"/> holding a sample at either end of the ADC's range, or null where none
        /// does.
        /// </summary>
        /// <remarks>
        /// On the raw counts, before scaling and any filter, which smear a clipped stretch away from the limits and
        /// move its baseline. Null when nothing clipped, so a clean recording sends the display nothing.
        /// </remarks>
        private protected static bool[] ClippedRows(Mat raw, double maximum)
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

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            var context = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            var workflowBuilder = (WorkflowBuilder)provider.GetService(typeof(WorkflowBuilder));
            var node = (ProbeScope<TFrame>)ExpressionBuilder.GetWorkflowElement(context.Source);
            fault = null;

            try { deviceName = UpstreamDevice.Resolve(workflowBuilder?.Workflow, context.Source, node.DeviceName, DeviceNameOf); }
            catch (InvalidOperationException ex) { fault = $"ProbeScope: {ex.Message}"; }

            // NB: no plot can be wider than the widest monitor unless its window spans two, so that is as
            // much horizontal detail as is ever worth holding.
            waveform = new(node.HistoryBytes, Screen.AllScreens.Max(screen => screen.Bounds.Width)) { Unit = Unit };
            selector.ShowGrid = false;
            selector.DefaultZoomWindowFraction = 1f;

            strip = new ImGuiProbeScopeControlStrip();

            scheduler = new EventLoopScheduler();

            canvas = new ImPlotGLControl
            {
                Size = new System.Drawing.Size(1100, 620),
                Dock = DockStyle.Fill
            };
            canvas.HoverKeys.UnionWith(waveform.Hotkeys);

            // NB: the probe selector's, for its ruler.
            canvas.HoverKeys.Add(Keys.R);
            canvas.HoverKeys.Add(Keys.Delete);
            canvas.Render += RenderFrame;

            // NB: paced from the handle's lifetime rather than Load's, since a handle can be recreated.
            canvas.HandleCreated += (_, _) => pacer = new DisplayPacer(canvas.Handle);
            canvas.HandleDestroyed += (_, _) => StopPacer();
            canvas.Paint += (_, _) => pacer?.Painted();

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            visualizerService?.AddControl(canvas);
        }

        /// <inheritdoc/>
        public override IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            return base.Visualize(source.Select(BindBands), provider);
        }

        IObservable<object> BindBands(IObservable<object> frames)
        {
            if (fault is not null)
                return Observable.Never<object>();

            // NB: BufferedVisualizer forwards to Show under the lock its producer takes to append,
            // so without this hop the acquisition thread waits on the UI thread once a tick. It must
            // be one ordered thread: the band filters carry state across blocks.
            return frames
                .Cast<TFrame>()
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

        ProbeScopeSource ResolveSource()
        {
            try
            {
                DeviceInfo deviceInfo = null;
                using (DeviceManager.GetDevice(deviceName).Subscribe(info => deviceInfo = info)) { }
                return CreateSource(deviceInfo);
            }
            catch (Exception ex)
            {
                fault = ex.Message;
                return null;
            }
        }

        IObservable<object> BandOutput(ProbeScopeSource probe, BandSelection selection, IObservable<TFrame> frames)
        {
            var sampleRate = probe.Bands[selection.Band].SampleRate;
            var clipping = frames
                .Select(frame => Clipped(frame, selection))
                .Where(channels => channels is not null)
                .Select(channels => (object)new ProbeScopeClipping(channels));

            return ProcessBand(selection, frames).Select(data => (object)new ProbeScopeSample(data, sampleRate)).Merge(clipping);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The resolved source arrives through the same sequence as the samples so that it is
        /// applied on the UI thread, in order, ahead of anything that depends on it.
        /// </remarks>
        public override void Show(object value)
        {
            if (value is ProbeScopeSource probe)
            {
                source = probe;
                selector.Refresh(probe.ProbeGroup);
                strip.Bands = probe.Bands;
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
            var availY = ImGui.GetContentRegionAvail().Y - stripHeight;
            var probeWidth = probePaneCollapsed ? CollapsedPaneWidth : ProbePaneWidth;
            var waveWidth = Math.Max(200f, availX - probeWidth - ImGui.GetStyle().ItemSpacing.X);
            var probeOrigin = ImGui.GetCursorScreenPos();

            // NB: no vertical padding on either pane, so both start at their own top edge and the
            // two frames land on the same lines.
            var panePadding = ImGui.GetStyle().WindowPadding;
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(panePadding.X, 0));

            ImGui.BeginChild("##probePane", new Vector2(probeWidth, availY),
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            // NB: a button's height, which is the row the plot's time labels take as well, so the
            // probe view and the plot begin on the same line
            var probeViewTop = ImGui.GetCursorScreenPos().Y + ImGui.GetFrameHeight();
            if (ImGui.Button(probePaneCollapsed ? "»" : "«"))
                probePaneCollapsed = !probePaneCollapsed;

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

            ImGui.BeginChild("##wavePane", new Vector2(waveWidth, availY));
            var message = fault;
            if (message is null)
            {
                waveform.RefreshRate = pacer?.RefreshRate ?? waveform.RefreshRate;
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

            var stripTop = ImGui.GetCursorScreenPos().Y;
            strip.Draw(waveform);
            stripHeight = ImGui.GetCursorScreenPos().Y - stripTop;

            // Scroll and zoom are handled from the root window, bounded to the probe pane so the
            // wheel still scrolls the channel list on the waveform side.
            if (!probePaneCollapsed)
                selector.HandleScrollInput(probeOrigin.Y + availY, probeOrigin.X + probeWidth);

            ImGui.End();
        }

        void StopPacer()
        {
            pacer?.Dispose();
            pacer = null;
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            StopPacer();
            scheduler?.Dispose();
            waveform?.Dispose();
            canvas?.Dispose();

            strip = null;
            scheduler = null;
            waveform = null;
            canvas = null;
            source = null;
        }
    }
}
