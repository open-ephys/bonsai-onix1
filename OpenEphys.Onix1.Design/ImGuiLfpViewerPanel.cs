using System;
using System.Collections.Generic;
using System.Numerics;
using Hexa.NET.ImGui;
using Hexa.NET.ImPlot;
using Hexa.NET.Utilities.Text;
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
        }

        Decimator waveformMinDecimator;
        Decimator waveformMaxDecimator;
        Mat timeRange;
        int sampleRate = 30000;

        Mat rowOffsets;
        Mat scaledWaveformMin;
        Mat scaledWaveformMax;

        WaveformHistory history;

        bool[] channelHidden = Array.Empty<bool>();
        bool[] dragOriginal = Array.Empty<bool>();

        int expandedChannel = -1;

        int channelHeight = 20;
        double timebase = 2.0;
        double rangeAmplitude = 500;

        /// <summary>
        /// Seconds of data spanned by the display.
        /// </summary>
        public double Timebase
        {
            get => timebase;
            set => SetTimebase(value, PositionAtFraction(0.5f), 0.5f);
        }

        /// <summary>
        /// Height in pixels of one channel's row.
        /// </summary>
        public int ChannelHeight
        {
            get => channelHeight;
            set => channelHeight = Math.Max(MinChannelHeight, value);
        }

        /// <summary>
        /// Amplitude spanned by one channel's row, in the units of the incoming data.
        /// </summary>
        public double RangeAmplitude
        {
            get => rangeAmplitude;
            set => rangeAmplitude = Math.Max(1, value);
        }

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
            waveformMinDecimator.Process(data);
            waveformMaxDecimator.Process(data);

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
                timeRange is null ||
                waveformMinDecimator.Buffer.Rows != rows ||             // # channels
                waveformMinDecimator.Buffer.Cols != columns ||          // # downsamples
                waveformMinDecimator.DownsampleFactor != samplesPerBin; // factor to map totalSamples -> # downsamples

            if (buffersStale)
            {
                timeRange?.Dispose();
                waveformMinDecimator?.Dispose();
                waveformMaxDecimator?.Dispose();
                rowOffsets?.Dispose();
                scaledWaveformMin?.Dispose();
                scaledWaveformMax?.Dispose();
                pannedWaveformMinDecimator?.Dispose();
                pannedWaveformMaxDecimator?.Dispose();
                waveformMinDecimator = new Decimator(rows, columns, samplesPerBin, ReduceOperation.Min);
                waveformMaxDecimator = new Decimator(rows, columns, samplesPerBin, ReduceOperation.Max);

                // NB: the paused view reduces out of the history into its own pair, at the same width as
                // the live one, so that changing the timebase while paused rebuilds both together.
                pannedWaveformMinDecimator = new Decimator(rows, columns, samplesPerBin, ReduceOperation.Min);
                pannedWaveformMaxDecimator = new Decimator(rows, columns, samplesPerBin, ReduceOperation.Max);

                // NB: the plot's own axis is the column index, so that a column lands where the
                // divisions and the cursor put it. Any other unit needs the plot's first and last column
                // to bound it, which spreads the columns over one fewer step than there are columns and
                // leaves the two placements a column apart at the narrowest timebases.
                timeRange = new Mat(1, columns, Depth.F32, 1);
                CV.Range(timeRange, 0, columns);

                rowOffsets = new Mat(rows, columns, Depth.F32, 1);
                for (int i = 0; i < rows; i++)
                {
                    using var row = rowOffsets.GetRow(i);
                    row.Set(Scalar.All(-i));
                }

                scaledWaveformMin = new Mat(rows, columns, Depth.F32, 1);
                scaledWaveformMax = new Mat(rows, columns, Depth.F32, 1);
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
            // NB: ahead of the envelope, which resuming disposes. Handling it with the other
            // gestures would free the matrices the plot is about to read.
            if (HotkeyPressed(ImGuiKey.Space))
                Paused = !Paused;

            if (HotkeyPressed(ImGuiKey.C, false))
                ShowCursors = !ShowCursors;

            if (ShowCursors && HotkeyPressed(ImGuiKey.H, false))
                HomeCursors();

            // NB: also here and not only in Update, so a timebase change takes effect while no data is
            // arriving, as when paused after acquisition has stopped.
            if (waveformMinDecimator is not null)
                RebuildBuffers(waveformMinDecimator.Buffer.Rows);

            if (timeRange is not null)
            {
                // NB: no vertical padding, so the plot frame lands on the region's own edges and can
                // be lined up with whatever the owner puts beside it.
                var padding = ImGui.GetStyle().WindowPadding;
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(padding.X, 0));
                ImGui.BeginChild("##data");
                var (waveformMin, waveformMax) = DisplayEnvelope();
                WaveformPlot(waveformMin, waveformMax);
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
            timeRange?.Dispose();
            waveformMinDecimator?.Dispose();
            waveformMaxDecimator?.Dispose();
            rowOffsets?.Dispose();
            scaledWaveformMin?.Dispose();
            scaledWaveformMax?.Dispose();
            history?.Dispose();
            DisposeHistoryView();
            timeRange = null;
            waveformMinDecimator = null;
            waveformMaxDecimator = null;
            rowOffsets = null;
            scaledWaveformMin = null;
            scaledWaveformMax = null;
            history = null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ResetBuffers();
        }
    }
}
