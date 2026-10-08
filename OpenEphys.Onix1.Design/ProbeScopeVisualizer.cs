using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
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

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            var context = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            var workflowBuilder = (WorkflowBuilder)provider.GetService(typeof(WorkflowBuilder));
            var node = (ProbeScope<TFrame>)ExpressionBuilder.GetWorkflowElement(context.Source);
            fault = null;
            try { deviceName = FindDeviceName(workflowBuilder?.Workflow, context.Source, node.DeviceName); }
            catch (InvalidOperationException ex) { fault = ex.Message; }

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
            canvas.HoverKeys.Add(Keys.Space);
            canvas.HoverKeys.Add(Keys.W);
            canvas.HoverKeys.Add(Keys.A);
            canvas.HoverKeys.Add(Keys.S);
            canvas.HoverKeys.Add(Keys.D);
            canvas.HoverKeys.Add(Keys.R);
            canvas.HoverKeys.Add(Keys.C);
            canvas.HoverKeys.Add(Keys.H);
            canvas.HoverKeys.Add(Keys.F);
            canvas.HoverKeys.Add(Keys.Delete);
            canvas.HoverKeys.Add(Keys.Q);
            canvas.HoverKeys.Add(Keys.E);
            canvas.Render += RenderFrame;

            // NB: paced from the handle's lifetime rather than Load's, since a handle can be recreated.
            canvas.HandleCreated += (_, _) => pacer = new DisplayPacer(canvas.Handle);
            canvas.HandleDestroyed += (_, _) => StopPacer();
            canvas.Paint += (_, _) => pacer?.Painted();

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            visualizerService?.AddControl(canvas);
        }

        // NB: the name is only ever read here, never written back to the node, so what the workflow file
        // holds is what was typed and is what runs. An empty name stands for the upstream device.
        string FindDeviceName(ExpressionBuilderGraph workflow, ExpressionBuilder self, string declared)
        {
            var (graph, node) = workflow is null ? default : Nodes(workflow).FirstOrDefault(x => x.Node.Value == self);
            if (node is null)
                throw new InvalidOperationException("ProbeScope could not locate itself in the workflow.");

            // NB: only through nodes with exactly one input, such as a Condition or a Gate, which cannot
            // bring in another device's data, and from a SubscribeSubject to whatever feeds its subject. It
            // stops at a branch such as Zip, where nothing upstream says which device the data is from.
            string attached = null;
            while (attached is null)
            {
                var upstream = graph.Predecessors(node).ToList();
                if (upstream.Count == 1)
                {
                    node = upstream[0];
                    attached = DeviceNameOf(ExpressionBuilder.GetWorkflowElement(node.Value));
                }
                else if (upstream.Count != 0 ||
                    ExpressionBuilder.Unwrap(node.Value) is not SubscribeSubject subscribe ||
                    !TryFindSubjectSource(workflow, graph, subscribe.Name, out graph, out node))
                {
                    break;
                }
            }

            if (string.IsNullOrEmpty(declared))
            {
                return string.IsNullOrEmpty(attached)
                    ? throw new InvalidOperationException(
                        "ProbeScope could not find a data operator upstream that it can display. Set its DeviceName.")
                    : attached;
            }

            if (!string.IsNullOrEmpty(attached) && attached != declared)
            {
                throw new InvalidOperationException(
                    $"ProbeScope's DeviceName is {declared}, but its data comes from {attached}.");
            }

            return declared;
        }

        /// <summary>
        /// Every node in <paramref name="graph"/> and the workflows nested in it, with the graph each belongs to.
        /// </summary>
        static IEnumerable<(ExpressionBuilderGraph Graph, Node<ExpressionBuilder, ExpressionBuilderArgument> Node)>
            Nodes(ExpressionBuilderGraph graph)
        {
            foreach (var node in graph)
            {
                yield return (graph, node);
                if (ExpressionBuilder.Unwrap(node.Value) is WorkflowExpressionBuilder { Workflow: { } nested })
                {
                    foreach (var inner in Nodes(nested))
                        yield return inner;
                }
            }
        }

        /// <summary>
        /// The node that feeds the subject <paramref name="name"/> as seen from <paramref name="scope"/>: the
        /// subject itself when it has an input, or else the one MulticastSubject that writes to it.
        /// </summary>
        /// <remarks>
        /// Looked up as Bonsai resolves a subject, in the innermost workflow that declares the name and then
        /// in each enclosing one.
        /// </remarks>
        static bool TryFindSubjectSource(
            ExpressionBuilderGraph workflow,
            ExpressionBuilderGraph scope,
            string name,
            out ExpressionBuilderGraph graph,
            out Node<ExpressionBuilder, ExpressionBuilderArgument> node)
        {
            for (; scope is not null; scope = Nodes(workflow).FirstOrDefault(x =>
                ExpressionBuilder.Unwrap(x.Node.Value) is WorkflowExpressionBuilder w && w.Workflow == scope).Graph)
            {
                var subject = scope.FirstOrDefault(n =>
                    ExpressionBuilder.Unwrap(n.Value) is SubjectExpressionBuilder s && s.Name == name);
                if (subject is null)
                    continue;

                if (scope.Predecessors(subject).Any())
                {
                    (graph, node) = (scope, subject);
                    return true;
                }

                var writers = Nodes(scope)
                    .Where(x => ExpressionBuilder.Unwrap(x.Node.Value) is MulticastSubject m && m.Name == name)
                    .ToList();
                (graph, node) = writers.Count == 1 ? writers[0] : default;
                return writers.Count == 1;
            }

            graph = null;
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
            return ProcessBand(selection, frames).Select(data => (object)new ProbeScopeSample(data, sampleRate));
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
