using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
    /// dropdown, the band sample rate, how to turn the raw frame sequence into a sequence of microvolt
    /// matrices, and optionally a filter that defines the band.
    /// </summary>
    internal sealed record ProbeScopeBand<TFrame>(
        string Name,
        string Description,
        int SampleRate,
        Func<IObservable<TFrame>, IObservable<Mat>> Scale, // NB: Convert to uV
        Func<IObservable<Mat>, IObservable<Mat>> Filter = null // NB: optional filter that can be used to define the band
        );

    /// <summary>
    /// Everything a <see cref="ProbeScopeVisualizer{TFrame}"/> needs from a resolved device: the probe
    /// geometry, the ADC channel groups common median referencing operates within, and the bands
    /// available for display.
    /// </summary>
    internal sealed record ProbeScopeSource<TFrame>(
        SingleProbeGroup ProbeGroup,
        int[][] AdcChannelGroups,
        IReadOnlyList<ProbeScopeBand<TFrame>> Bands);

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
        Subject<TFrame> frames;
        IDisposable scaleSubscription;

        string deviceName;
        bool probePaneCollapsed;
        ProbeScopeSource<TFrame> source;
        int boundBand = -1;

        /// <summary>
        /// Returns the device name declared on <paramref name="upstreamOperator"/>, or null if
        /// it is not a data operator this scope can display.
        /// </summary>
        private protected abstract string DeviceNameOf(object upstreamOperator);

        /// <summary>
        /// Builds the geometry and bands for a resolved device. Throw to report a device this
        /// scope cannot display.
        /// </summary>
        private protected abstract ProbeScopeSource<TFrame> CreateSource(DeviceInfo info);

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
            selector.DefaultZoomWindowMicrons = null;

            frames = new Subject<TFrame>();

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
        public override void Show(object value)
        {
            if (source is null)
                Initialize();

            frames.OnNext((TFrame)value);
        }

        void Initialize()
        {
            DeviceInfo deviceInfo = null;
            using (DeviceManager.GetDevice(deviceName).Subscribe(info => deviceInfo = info)) { }
            source = CreateSource(deviceInfo);

            selector.Refresh(source.ProbeGroup);
            waveform.Bands = source.Bands.Select(b => (b.Name, b.Description)).ToList();
            BindSelectedBand();
        }

        void BindSelectedBand()
        {
            scaleSubscription?.Dispose();
            waveform.ResetBuffers();
            boundBand = waveform.SelectedBand;

            var band = source.Bands[boundBand];
            var groups = source.AdcChannelGroups;

            var scaled = band.Scale(frames)
                .Select(data => waveform.UseCommonMedianReference ? Neuropixels.ApplyCmrF32(data, groups) : data);

            if (band.Filter is not null)
                scaled = band.Filter(scaled);

            scaleSubscription = scaled.Subscribe(data => waveform.Update(data, band.SampleRate));
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

            if (source is not null && waveform.SelectedBand != boundBand)
                BindSelectedBand();
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            renderTimer?.Stop();
            renderTimer?.Dispose();
            scaleSubscription?.Dispose();
            frames?.Dispose();
            waveform?.Dispose();
            canvas?.Dispose();

            renderTimer = null;
            scaleSubscription = null;
            frames = null;
            waveform = null;
            canvas = null;
            source = null;
            boundBand = -1;
        }
    }
}
