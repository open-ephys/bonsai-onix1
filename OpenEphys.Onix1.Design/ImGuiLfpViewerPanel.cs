using System;
using System.Numerics;
using Hexa.NET.ImGui;
using OpenCV.Net;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Draws a multi-channel signal as a stack of sweeping traces, each column showing the min and max of
    /// the samples it covers, into whatever region the caller has opened.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The owner calls <see cref="Update"/> with each block and <see cref="Draw"/> once a frame. Everything
    /// the display can be told is a property, so a control strip is one way to drive it, not part of it.
    /// </para>
    /// <para>
    /// While <see cref="Paused"/>, the display is frozen and can be panned back through a history of recent
    /// samples and redrawn at any timebase.
    /// </para>
    /// <para>
    /// Ported from <c>Bonsai.Ephys.Design.WaveformVisualizer</c>.
    /// </para>
    /// </remarks>
    internal sealed partial class ImGuiLfpViewerPanel : IDisposable
    {
        readonly long historyBytes;
        readonly int maxColumns;

        /// <param name="historyBytes">
        /// Memory for the history of recent samples. The owner sizes it, since only it knows its channel
        /// count and fastest band; the panel works out the seconds it buys once data arrives.
        /// </param>
        /// <param name="maxColumns">
        /// Most columns the decimation buffers hold. Drawing combines columns down to the plot's pixel width,
        /// so more than the widest screen buys nothing.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="historyBytes"/> is not positive, or <paramref name="maxColumns"/> is less than one.
        /// </exception>
        public ImGuiLfpViewerPanel(long historyBytes, int maxColumns)
        {
            if (historyBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(historyBytes));

            if (maxColumns < 1)
                throw new ArgumentOutOfRangeException(nameof(maxColumns));

            this.historyBytes = historyBytes;
            this.maxColumns = maxColumns;
            frameKeys = FrameKeys();
            tableKeys = TableKeys();
        }

        readonly WaveformCursors cursors = new();

        /// <summary>
        /// Whether the cursors are shown. While they are, a cursor's line can be dragged, and they read the selected
        /// channel.
        /// </summary>
        public bool ShowCursors
        {
            get => cursors.Show;
            set
            {
                selectOnShow |= value && !cursors.Show;
                cursors.Show = value;
            }
        }

        // NB: once, as the cursors are shown, rather than whenever there is no selection, so that clearing the
        // selection with the cursors shown leaves it clear.
        bool selectOnShow;

        /// <summary>
        /// Moves the cursors to the middle of the view, keeping their spacing, or spreads them evenly across it
        /// if they span more than it does.
        /// </summary>
        public void HomeCursors() => cursors.Home(Axis);

        /// <summary>
        /// Whether channels are drawn as rows of color, a compressed view of every channel at once, rather than
        /// as traces. Rows are laid out alike in both views, so a channel stays in place when the view changes.
        /// </summary>
        public bool ShowHeatmap { get; set; }

        /// <summary>
        /// Magnitude, in the units of the incoming data, at or below which the heatmap shows black. It can
        /// reach the range, where the colors are brightest.
        /// </summary>
        public double ColorThreshold
        {
            get => Math.Min(colorThreshold, rangeAmplitude);
            set => colorThreshold = Math.Max(0, Math.Min(value, rangeAmplitude));
        }

        /// <summary>
        /// Whether the channel height follows the plot's visible height so that every channel fits it, as far as
        /// the smallest height allows. Setting the height any other way stops it.
        /// </summary>
        public bool FitChannels { get; set; }

        /// <summary>
        /// The channel selected by clicking its trace or label or stepping with Q and E, or -1. The cursors read
        /// it.
        /// </summary>
        int selectedChannel = -1;

        Decimator decimator;
        int sampleRate = 30000;

        WaveformHistory history;

        bool[] channelHidden = Array.Empty<bool>();
        bool[] dragOriginal = Array.Empty<bool>();

        int expandedChannel = -1;

        float channelHeight = 20;
        double timebase = 2.5;
        double rangeAmplitude = 500;
        double colorThreshold;

        /// <summary>
        /// Seconds of data spanned by the display.
        /// </summary>
        public double Timebase
        {
            get => timebase;
            set => SetTimebaseAt(value, plotLeft + plotSpan / 2);
        }

        /// <summary>
        /// Height in pixels of one channel's row, which may be a fraction of a pixel, exactly the fit height while
        /// fitting and never less than it. Setting it stops fitting.
        /// </summary>
        public float ChannelHeight
        {
            get => FitChannels ? FitHeight : Math.Max(channelHeight, FitHeight);
            set
            {
                channelHeight = Math.Max(FitHeight, value);
                FitChannels = false;
            }
        }

        /// <summary>
        /// The channel height at which every channel just fills the plot's visible height, as far as the smallest
        /// height allows. Rows any shorter would leave part of the plot unused.
        /// </summary>
        float FitHeight => channelHidden.Length > 0 && visibleHeight > 0
            ? Math.Max(MinChannelHeight, visibleHeight / channelHidden.Length)
            : MinChannelHeight;

        /// <summary>
        /// The next multiple of the channel height step above <paramref name="height"/>, or below it when
        /// <paramref name="direction"/> is negative.
        /// </summary>
        /// <remarks>
        /// To a multiple rather than by a step, so that a height off the multiples, as a fitted one is, comes back
        /// onto them.
        /// </remarks>
        public static float StepChannelHeight(float height, int direction) => direction > 0
            ? (MathF.Floor(height / ChannelHeightStep) + 1) * ChannelHeightStep
            : (MathF.Ceiling(height / ChannelHeightStep) - 1) * ChannelHeightStep;

        /// <summary>
        /// Amplitude spanned by one channel's row, in the units of the incoming data.
        /// </summary>
        public double RangeAmplitude
        {
            get => rangeAmplitude;
            set => rangeAmplitude = Math.Max(1, value);
        }

        /// <summary>
        /// Refresh rate, in Hz, of the display the panel is drawn on, which decides how fast a sweep can be shown
        /// as it is written. The measured frame rate will not do, since it drops whenever a frame is slow.
        /// </summary>
        public double RefreshRate { get; set; } = 60;

        /// <summary>
        /// Unit of the incoming data, shown wherever an amplitude is.
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Whether the display is frozen. While it is, the view can be panned through the history and redrawn
        /// at any timebase.
        /// </summary>
        public bool Paused
        {
            get => pauseSample >= 0;
            set
            {
                if (value == Paused) return;
                if (value) Pause();
                else Resume();
            }
        }

        /// <summary>
        /// Supplies one block of samples. While paused, a block whose rate or channel count would clear the
        /// history is dropped.
        /// </summary>
        /// <param name="data">Channel-by-sample matrix of 32-bit floats, one row per channel.</param>
        /// <param name="dataSampleRate">Sample rate of the incoming data, in Hz.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="dataSampleRate"/> is less than one.
        /// </exception>
        public void Update(Mat data, int dataSampleRate)
        {
            if (dataSampleRate < 1)
                throw new ArgumentOutOfRangeException(nameof(dataSampleRate));

            var budgetSamples = historyBytes / (data.Rows * sizeof(float));
            var capacity = (int)Math.Max(1, Math.Min(int.MaxValue, budgetSamples));
            var historyReplaced = history is null || history.Rows != data.Rows || history.Capacity != capacity;

            // NB: the history converts sample offsets to time with a single factor, so it cannot hold two
            // rates. A change of band that keeps the rate is left alone: what the history holds is what was
            // displayed, seams between band switches are included.
            var historyInvalid = historyReplaced || dataSampleRate != sampleRate;

            // NB: a paused view is read from the history, so a block that would clear it is dropped. Once
            // the view resumes, the next block still differs and is applied as usual.
            if (Paused && historyInvalid)
                return;

            sampleRate = dataSampleRate;
            if (historyReplaced)
            {
                history?.Dispose();
                history = new WaveformHistory(data.Rows, capacity);
            }

            if (historyInvalid)
            {
                history.Clear();
                RebuildTimeBases(capacity / (double)sampleRate);
            }

            RebuildBuffers(data.Rows);
            decimator.Process(data);

            if (!Paused)
                history.Write(data);
        }

        /// <summary>
        /// Rebuilds the decimation buffers if the channel count or the timebase no longer matches them, and
        /// moves the window onto the new column spacing.
        /// </summary>
        void RebuildBuffers(int rows)
        {
            var totalSamples = Math.Max(1, (int)(timebase * sampleRate));
            var samplesPerBin = (totalSamples + maxColumns - 1) / maxColumns;
            var columns = totalSamples / samplesPerBin;
            var buffersStale =
                decimator is null ||
                decimator.Sweep.Rows != rows ||             // # channels
                decimator.Sweep.Cols != columns ||          // # downsamples
                decimator.DownsampleFactor != samplesPerBin; // factor to map totalSamples -> # downsamples

            if (buffersStale)
            {
                decimator?.Dispose();
                pannedDecimator?.Dispose();
                decimator = new Decimator(rows, columns, samplesPerBin);

                // NB: the paused view reduces out of the history into its own decimator, at the same width as
                // the live one, so that changing the timebase while paused rebuilds both together.
                pannedDecimator = new Decimator(rows, columns, samplesPerBin);

                if (channelHidden.Length != rows)
                {
                    channelHidden = new bool[rows];
                    dragOriginal = new bool[rows];
                }
            }

            // NB: also when only the anchor is waiting, so that a timebase change which happens to land on
            // the same column spacing still moves the window to where the gesture asked.
            if (buffersStale || anchorPending)
                RebuildWindow(samplesPerBin, columns);
        }

        /// <summary>
        /// Draws the waveform stack into the region the caller has opened. Called once per frame.
        /// </summary>
        public void Draw()
        {
            // NB: ahead of the envelope, which resuming with Space disposes. Answering it with the other
            // gestures would free the matrices the plot is about to read.
            AnswerFrameKeys();

            // NB: also here and not only in Update, so a timebase change takes effect while no data is
            // arriving, as when paused after acquisition has stopped.
            if (decimator is not null)
                RebuildBuffers(decimator.Sweep.Rows);

            if (decimator is not null)
            {
                // NB: no vertical padding, so the plot frame lands on the region's own edges and can
                // be lined up with whatever the owner puts beside it.
                var padding = ImGui.GetStyle().WindowPadding;
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(padding.X, 0));
                ImGui.BeginChild("##data", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollWithMouse);
                var (envelope, shown) = DisplayEnvelope();
                WaveformPlot(envelope, shown);
                ImGui.EndChild();
                ImGui.PopStyleVar();
            }
        }

        /// <summary>
        /// Discards the decimation buffers so the next <see cref="Update"/> rebuilds them.
        /// </summary>
        /// <remarks>
        /// The decimators carry reduction state across calls, so anything that changes the meaning
        /// of the incoming samples has to discard them rather than let a partially filled output bin
        /// be completed from a different signal.
        /// </remarks>
        public void ResetBuffers()
        {
            decimator?.Dispose();
            history?.Dispose();
            DisposeHistoryView();
            decimator = null;
            history = null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ResetBuffers();
        }
    }
}
