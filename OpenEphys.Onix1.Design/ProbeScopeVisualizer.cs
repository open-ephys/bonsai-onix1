using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Forms;
using Bonsai;
using Bonsai.Dag;
using Bonsai.Design;
using Bonsai.Expressions;
using Hexa.NET.ImGui;
using OpenCV.Net;
using OpenEphys.ProbeInterface.NET;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// One selectable band of a probe: a short display name, a description of its passband for the
    /// dropdown, and the rate at which that band produces samples. How the band is actually computed is
    /// <see cref="ProbeScopeVisualizer{TFrame}.ProcessBand"/>, which only a concrete scope can answer.
    /// </summary>
    internal sealed record ProbeScopeBand(string Name, string Description, int SampleRate);

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
    /// The visualizer opens on a pass-through sink node that must sit directly downstream of the data
    /// operator. The device is resolved once the first frame arrives, since data
    /// flowing proves the configuration operator has registered it.
    /// </remarks>
    /// <typeparam name="TFrame">The data frame type the scope displays.</typeparam>
    public abstract class ProbeScopeVisualizer<TFrame> : BufferedVisualizer
    {
        const float ProbePaneWidth = 260f;
        const float CollapsedPaneWidth = 28f;

        readonly ImGuiProbeSelector selector = new();

        ImGuiWaveformPanel waveform;

        ImPlotGLControl canvas;
        System.Windows.Forms.Timer renderTimer;
        BehaviorSubject<int> selection;
        EventLoopScheduler scheduler;

        string deviceName;
        bool probePaneCollapsed;
        ProbeScopeSource source;
        int boundSelection;

        /// <summary>
        /// The panel this visualizer draws into, so that a concrete scope can read the controls that
        /// belong to it.
        /// </summary>
        private protected ImGuiWaveformPanel Waveform => waveform;

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
        /// Builds the sequence displayed for the band at <paramref name="band"/> in the list returned by
        /// <see cref="CreateSource"/>, in the unit named by <see cref="RangeLabel"/>.
        /// </summary>
        /// <remarks>
        /// Called again whenever <see cref="ImGuiWaveformPanel.SelectionRevision"/> moves, so an
        /// implementation reads whatever panel controls it owns here rather than closing over them.
        /// </remarks>
        private protected abstract IObservable<Mat> ProcessBand(int band, IObservable<TFrame> frames);

        /// <summary>
        /// Unit the bands produce, shown beside the amplitude range control.
        /// </summary>
        private protected abstract string RangeLabel { get; }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            var context = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            var workflowBuilder = (WorkflowBuilder)provider.GetService(typeof(WorkflowBuilder));
            deviceName = FindUpstreamDeviceName(workflowBuilder?.Workflow, context.Source);

            var node = (ProbeScope<TFrame>)ExpressionBuilder.GetWorkflowElement(context.Source);
            waveform = new(node.HistoryBytes) { RangeLabel = this.RangeLabel };
            selector.ShowGrid = false;
            selector.ShowCoordinateReadout = false;
            selector.DefaultZoomWindowFraction = 1f;

            selection = new BehaviorSubject<int>(0);
            scheduler = new EventLoopScheduler();

            canvas = new ImPlotGLControl
            {
                Size = new System.Drawing.Size(1100, 620),
                Dock = DockStyle.Fill
            };
            canvas.HoverKeys.Add(Keys.Space);
            canvas.HoverKeys.Add(Keys.W);
            canvas.HoverKeys.Add(Keys.A);
            canvas.HoverKeys.Add(Keys.S);
            canvas.HoverKeys.Add(Keys.D);
            canvas.Render += RenderFrame;

            renderTimer = new System.Windows.Forms.Timer { Interval = 16 };
            renderTimer.Tick += (_, _) => canvas.Invalidate();
            renderTimer.Start();

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            visualizerService?.AddControl(canvas);
        }

        string FindUpstreamDeviceName(ExpressionBuilderGraph workflow, ExpressionBuilder self)
        {
            if (workflow is null || !TryFindNode(workflow, self, out var owner, out var node))
                throw new InvalidOperationException("ProbeScope could not locate itself in the workflow.");

            var upstream = owner.Predecessors(node).ToList();
            if (upstream.Count != 1)
                throw new InvalidOperationException("ProbeScope must have exactly one input.");

            var element = ExpressionBuilder.GetWorkflowElement(upstream[0].Value);
            var name = DeviceNameOf(element);
            if (name is null)
            {
                throw new InvalidOperationException(
                    $"ProbeScope must be attached directly to a data operator it can display, not to {element.GetType().Name}.");
            }

            if (string.IsNullOrEmpty(name))
                throw new InvalidOperationException($"The {element.GetType().Name} upstream of ProbeScope has no DeviceName set.");

            return name;
        }

        static bool TryFindNode(
            ExpressionBuilderGraph workflow,
            ExpressionBuilder target,
            out ExpressionBuilderGraph owner,
            out Node<ExpressionBuilder, ExpressionBuilderArgument> node)
        {
            foreach (var candidate in workflow)
            {
                if (ReferenceEquals(candidate.Value, target))
                {
                    owner = workflow;
                    node = candidate;
                    return true;
                }

                if (ExpressionBuilder.Unwrap(candidate.Value) is WorkflowExpressionBuilder nested &&
                    nested.Workflow is not null &&
                    TryFindNode(nested.Workflow, target, out owner, out node))
                {
                    return true;
                }
            }

            owner = null;
            node = null;
            return false;
        }

        /// <inheritdoc/>
        public override IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            return base.Visualize(source.Select(BindBands), provider);
        }

        IObservable<object> BindBands(IObservable<object> frames)
        {
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
                    .SelectMany(probe => Observable.Return<object>(probe)
                        .Concat(selection.Select(_ => BandOutput(probe, shared)).Switch())));
        }

        ProbeScopeSource ResolveSource()
        {
            DeviceInfo deviceInfo = null;
            using (DeviceManager.GetDevice(deviceName).Subscribe(info => deviceInfo = info)) { }
            return CreateSource(deviceInfo);
        }

        IObservable<object> BandOutput(ProbeScopeSource probe, IObservable<TFrame> frames)
        {
            var band = waveform.SelectedBand;
            var sampleRate = probe.Bands[band].SampleRate;
            return ProcessBand(band, frames)
                .Select(data => (object)new ProbeScopeSample(data, sampleRate));
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
                waveform.Bands = probe.Bands.Select(b => (b.Name, b.Description)).ToList();
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
            ImGui.Begin("##probescope",
                ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            var availX = ImGui.GetContentRegionAvail().X;
            var availY = ImGui.GetContentRegionAvail().Y;
            var probeWidth = probePaneCollapsed ? CollapsedPaneWidth : ProbePaneWidth;
            var waveWidth = Math.Max(200f, availX - probeWidth - ImGui.GetStyle().ItemSpacing.X);
            var probeOrigin = ImGui.GetCursorScreenPos();

            ImGui.BeginChild("##probePane", new Vector2(probeWidth, -1), ImGuiChildFlags.Borders,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
            if (ImGui.Button(probePaneCollapsed ? "»" : "«"))
                probePaneCollapsed = !probePaneCollapsed;
            if (!probePaneCollapsed)
            {
                var probeHeight = ImGui.GetContentRegionAvail().Y;
                selector.UpdateLayout(probeHeight, ImGui.GetContentRegionAvail().X);
                selector.DrawZoomedView(probeHeight, selectionEnabled: false);
            }
            ImGui.EndChild();

            ImGui.SameLine();

            ImGui.BeginChild("##wavePane", new Vector2(waveWidth, -1));
            waveform.Draw();
            ImGui.EndChild();

            // Scroll and zoom are handled from the root window, bounded to the probe pane so the
            // wheel still scrolls the channel list on the waveform side.
            if (!probePaneCollapsed)
                selector.HandleScrollInput(probeOrigin.Y + availY, probeOrigin.X + probeWidth);

            ImGui.End();

            if (source is not null && waveform.SelectionRevision != boundSelection)
            {
                boundSelection = waveform.SelectionRevision;
                selection.OnNext(boundSelection);
            }
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            renderTimer?.Stop();
            renderTimer?.Dispose();
            selection?.Dispose();
            scheduler?.Dispose();
            waveform?.Dispose();
            canvas?.Dispose();

            renderTimer = null;
            selection = null;
            scheduler = null;
            waveform = null;
            canvas = null;
            source = null;
            boundSelection = 0;
        }
    }
}
