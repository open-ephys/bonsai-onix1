using Bonsai;
using Bonsai.Dag;
using Bonsai.Design;
using Bonsai.Expressions;
using Hexa.NET.ImGui;
using OpenCV.Net;
using OpenEphys.ProbeInterface.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Subjects;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// One selectable band of a probe: a display name, the band sample rate, and how to turn the raw frame
    /// sequence into a sequence of microvolt matrices.
    /// </summary>
    internal sealed record ProbeScopeBand<TFrame>(
        string Name,
        int SampleRate,
        Func<IObservable<TFrame>, IObservable<Mat>> Scale);

    /// <summary>
    /// Everything a <see cref="ProbeScopeVisualizer{TFrame}"/> needs from a resolved device: the probe
    /// geometry and the bands available for display.
    /// </summary>
    internal sealed record ProbeScopeSource<TFrame>(
        SingleProbeGroup ProbeGroup,
        IReadOnlyList<ProbeScopeBand<TFrame>> Bands);

    /// <summary>
    /// Provides a type visualizer that shows a probe schematic beside a live multi-channel waveform viewer
    /// for a streaming probe. Subclasses supply how to read the device name off the upstream data operator
    /// and how to turn its <see cref="DeviceInfo"/> into geometry and bands.
    /// </summary>
    /// <remarks>
    /// The visualizer opens on a <see cref="ProbeScopeBuilder{TFrame}"/> node that must sit directly
    /// downstream of the data operator. The device is resolved once the first frame arrives, since data
    /// flowing proves the configuration operator has registered it.
    /// </remarks>
    /// <typeparam name="TFrame">The data frame type the scope displays.</typeparam>
    public abstract class ProbeScopeVisualizer<TFrame> : BufferedVisualizer
    {
        const float ProbePaneWidth = 260f;
        const float CollapsedPaneWidth = 28f;

        readonly ImGuiWaveformPanel waveform = new();
        readonly ImGuiProbeSelector selector = new();

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

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            var context = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            var workflowBuilder = (WorkflowBuilder)provider.GetService(typeof(WorkflowBuilder));
            deviceName = FindUpstreamDeviceName(workflowBuilder?.Workflow, context.Source);

            waveform.RangeLabel = "µV";
            selector.ShowGrid = false;
            selector.ShowCoordinateReadout = false;
            selector.DefaultZoomWindowMicrons = null;

            frames = new Subject<TFrame>();

            canvas = new ImPlotGLControl
            {
                Size = new System.Drawing.Size(1100, 620),
                Dock = DockStyle.Fill
            };
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
            waveform.Bands = source.Bands.Select(b => b.Name).ToList();
            BindScale();
        }

        void BindScale()
        {
            scaleSubscription?.Dispose();
            waveform.ResetBuffers();
            boundBand = waveform.SelectedBand;

            var band = source.Bands[boundBand];
            scaleSubscription = band.Scale(frames).Subscribe(data => waveform.Update(data, band.SampleRate));
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
                BindScale();
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            renderTimer?.Stop();
            renderTimer?.Dispose();
            scaleSubscription?.Dispose();
            frames?.Dispose();
            waveform.Dispose();
            canvas?.Dispose();

            renderTimer = null;
            scaleSubscription = null;
            frames = null;
            canvas = null;
            source = null;
            boundBand = -1;
        }
    }
}
